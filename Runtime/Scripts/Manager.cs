using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class Manager : UdonSharpBehaviour
    {
        public Transform ReferenceTransform => this.transform;
        
        private const float AUTO_ADJUSTMENT_DURATION = 2.5f;

        [SerializeField] private VRCStation _station;
        [SerializeField] private FrameEventReceiver _frameEventReceiver;

        [SerializeField] private Transform _enterPoint;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private Transform _model;
        [SerializeField] private Transform _guide;
        [SerializeField] private UnityEngine.UI.Image _guideImage;
        [SerializeField] private Sprite[] _guideSprites;
        [SerializeField] private GameObject _disabledOverlay;

        [SerializeField] private float _autoAdjustmentMinHeight;
        [SerializeField] private float _autoAdjustmentMaxHeight;

        [SerializeField] private float _manualAdjustmentMinHeight;
        [SerializeField] private float _manualAdjustmentMaxHeight;

        [SerializeField] private ModelAdjustmentMode _modelAdjustmentMode;
        [SerializeField] private float _modelSurfaceHeight;

        private State _state = State.Idle;
        private bool _isUserInVR = false;

        private int _pendingEndAutoAdjustmentCount = 0;




        private void InitializeChair()
        {
            _isUserInVR = Networking.LocalPlayer.IsUserInVR();

            InitializeSyncedData();
            InitializeInputReceiver();
            InitializeHeightHandler();
            InitializeYawHandler();
            InitializeGuideHandler(_isUserInVR);
            InitializeSyncHandler();
            InitializeExitInputHandler();
        }


        private void BeginSeating()
        {
            _state = State.AutomaticallyAdjusting;

            BeginSyncedData();
            BeginInputReceiver();
            BeginHeightHandler();
            BeginYawHandler();
            BeginGuideHandler();
            BeginSyncHandler();
            BeginExitInputHandler();

            SendCustomEventDelayedSeconds(nameof(_EndAutoAdjustment), AUTO_ADJUSTMENT_DURATION);
            _pendingEndAutoAdjustmentCount++;
        }


        private void ResetSeating()
        {
            _state = State.Idle;

            ResetSyncedData();
            ResetInputReceiver();
            ResetHeightHandler(ReferenceTransform);
            ResetYawHandler();
            ResetGuideHandler();
            ResetSyncHandler();
            ResetExitInputHandler();
        }


        private void OnEnable()
        {
            InitializeChair();
        }


        private void OnDisable()
        {
            ResetSeating();
        }


        public override void Interact()
        {
            _station.UseStation(Networking.LocalPlayer);
        }


        public override void OnStationEntered(VRCPlayerApi player)
        {
            Log.Info(nameof(Manager), $"{nameof(OnStationEntered)}: playerId={player.playerId}, displayName={player.displayName}");

            if (player.isLocal)
            {
                if (!Networking.IsOwner(this.gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                }

                BeginSeating();

                _sittingPlayerId = player.playerId;
                UpdateSyncedTransform();
                ScheduleSync();
                SchedulePeriodicSync();

                _frameEventReceiver.enabled = true;
            }
            else
            {
                // Join 時に既に他のプレイヤーが座っていた場合、OnStationEntered() -> OnStationExited() -> OnStationEntered() の順に発火する（アバター読み込みが発生するため）。
                // Join 時の OnDeserialization() -> ReflectSyncedData() は OnStationExited() でリセットしてしまうため、2回目の OnStationEntered() でも ReflectSyncedData() を実行する。
                ReflectSyncedData();
            }
        }


        public void _EndAutoAdjustment()
        {
            Log.Info(nameof(Manager), $"{nameof(_EndAutoAdjustment)}: {nameof(_pendingEndAutoAdjustmentCount)}={_pendingEndAutoAdjustmentCount}->{_pendingEndAutoAdjustmentCount - 1}");

            _pendingEndAutoAdjustmentCount--;

            if (_pendingEndAutoAdjustmentCount > 0) { return; }
            if (_state != State.AutomaticallyAdjusting) { return; }

            Log.Info(nameof(Manager), $"{nameof(_EndAutoAdjustment)}: End");

            _state = State.ManuallyAdjusting;

            OnAutoAdjustmentEndedGuideHandler();

            UpdateSyncedTransform();
            ScheduleSync();
        }


        public override void OnStationExited(VRCPlayerApi player)
        {
            Log.Info(nameof(Manager), $"{nameof(OnStationExited)}: playerId={player.playerId}, displayName={player.displayName}");

            _frameEventReceiver.enabled = false;

            ResetSeating();

            if (Networking.IsOwner(this.gameObject))
            {
                _sittingPlayerId = 0;
                UpdateSyncedTransform();
                ScheduleSync();
            }
        }


        public void _LateUpdate()
        {
            if (_state == State.AutomaticallyAdjusting)
            {
                float deltaTime = Time.deltaTime;
                AdjustHeightAutomatically(ReferenceTransform);
                AdjustModelHeightAutomatically(ReferenceTransform);
                UpdateYaw(_isUserInVR, deltaTime);
                AdjustGuideHeightAutomatically(ReferenceTransform);
                UpdateExit(_isUserInVR);
            }
            else if (_state == State.ManuallyAdjusting)
            {
                float deltaTime = Time.deltaTime;
                UpdateHeight(_isUserInVR, ReferenceTransform, deltaTime);
                UpdateYaw(_isUserInVR, deltaTime);
                UpdateGuide(_isUserInVR, ReferenceTransform);
                UpdateSync(_isUserInVR);
                UpdateExit(_isUserInVR);
            }
        }


        // 座っているプレイヤーが退室しても OnStationExited() は発火しない
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // OnStationExit() が発火していない -> _sittingPlayerId != 0
            if (_sittingPlayerId == 0) { return; }

            Log.Info(nameof(Manager), $"{nameof(OnOwnershipTransferred)}: playerId={player.playerId}, displayName={player.displayName}");
            
            if (player.isLocal)
            {
                ResetSeating();

                _sittingPlayerId = 0;
                UpdateSyncedTransform();
                ScheduleSync();
            }
            else
            {
                ResetSeating();
            }
        }
    }
}
