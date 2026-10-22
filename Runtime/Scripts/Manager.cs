
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class Manager : UdonSharpBehaviour
    {
        public Transform ReferenceTransform => this.transform;
        
        private const float AUTO_ADJUSTMENT_DURATION = 2.5f;

        [SerializeField] private VRC.SDKBase.VRCStation _station;
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

        private VRCTweenHandle _autoAdjustmentHandle;




        public override void Interact()
        {
            _station.UseStation(Networking.LocalPlayer);
        }


        public override void OnStationEntered(VRCPlayerApi player)
        {
            Log(nameof(Manager), nameof(OnStationEntered), $"playerId={player.playerId}, displayName={player.displayName}");

            if (player.isLocal)
            {
                if (!Networking.IsOwner(this.gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                }

                Initialize(_isUserInVR);

                _sittingPlayerId = player.playerId;
                UpdateSyncedTransform();
                RequestSync();
                RequestPeriodicSync();

                _frameEventReceiver.enabled = true;
            }
            else
            {
                // Join 時に既に他のプレイヤーが座っていた場合、OnStationEntered() -> OnStationExited() -> OnStationEntered() の順に発火する（アバター読み込みが発生するため）。
                // Join 時の OnDeserialization() -> ReflectSyncedData() は OnStationExited() でリセットしてしまうため、2回目の OnStationEntered() でも ReflectSyncedData() を実行する。
                ReflectSyncedData();
            }
        }


        private void Initialize(bool isUserInVR)
        {
            _state = State.AutomaticallyAdjusting;
            _isUserInVR = Networking.LocalPlayer.IsUserInVR();

            InitializeSyncedData();
            InitializeInputReceiver();
            InitializeHeightHandler();
            InitializeYawHandler();
            InitializeGuideHandler(isUserInVR);
            InitializeSyncHandler();

            if (_autoAdjustmentHandle.IsValid && _autoAdjustmentHandle.IsActive)
            {
                _autoAdjustmentHandle.Restart();
            }
            else
            {
                _autoAdjustmentHandle = VRCTween.DelayedCall(this, nameof(EndAutoAdjustment), AUTO_ADJUSTMENT_DURATION);
            }
        }


        public void EndAutoAdjustment()
        {
            if (_state != State.AutomaticallyAdjusting) { return; }

            Log(nameof(Manager), nameof(EndAutoAdjustment));

            _state = State.ManuallyAdjusting;

            OnAutoAdjustmentEndedGuideHandler();

            UpdateSyncedTransform();
            RequestSync();
        }


        public override void OnStationExited(VRCPlayerApi player)
        {
            Log(nameof(Manager), nameof(OnStationExited), $"playerId={player.playerId}, displayName={player.displayName}");

            _frameEventReceiver.enabled = true;

            ResetChair();

            if (Networking.IsOwner(this.gameObject))
            {
                _sittingPlayerId = 0;
                UpdateSyncedTransform();
                RequestSync();
            }
        }


        public void _LateUpdate()
        {
            if (_state == State.AutomaticallyAdjusting)
            {
                float deltaTime = Time.deltaTime;
                AdjustHeightAutomatically(ReferenceTransform);
                AdjustYawManually(_isUserInVR, deltaTime);
                AdjustModelHeightAutomatically(ReferenceTransform);
                AdjustGuideHeightAutomatically(ReferenceTransform);
            }
            else if (_state == State.ManuallyAdjusting)
            {
                float deltaTime = Time.deltaTime;
                AdjustHeightManually(_isUserInVR, ReferenceTransform, deltaTime);
                AdjustYawManually(_isUserInVR, deltaTime);
                UpdateGuide(_isUserInVR, ReferenceTransform);
                UpdateSync(_isUserInVR);
            }
        }


        // 座っているプレイヤーが退室しても OnStationExited() は発火しない
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // OnStationExit() が発火していない -> _sittingPlayerId != 0
            if (_sittingPlayerId == 0) { return; }

            Log(nameof(Manager), nameof(OnOwnershipTransferred), $"playerId={player.playerId}, displayName={player.displayName}: Sitting player left");
            
            if (player.isLocal)
            {
                ResetChair();

                _sittingPlayerId = 0;
                UpdateSyncedTransform();
                RequestSync();
            }
            else
            {
                ResetChair();
            }
        }


        private void ResetChair()
        {
            _state = State.Idle;

            ResetSyncedData();
            ResetInputReceiver();
            ResetHeightHandler(ReferenceTransform);
            ResetYawHandler();
            ResetGuideHandler();
            ResetSyncHandler();

            if (_autoAdjustmentHandle.IsValid && _autoAdjustmentHandle.IsActive)
            {
                _autoAdjustmentHandle.Kill();
            }
        }
    }
}
