
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace ChuChuGimmicks.MinamoChair
{
    public class Manager : UdonSharpBehaviour
    {
        #region Debug
        //[SerializeField] private TMPro.TextMeshProUGUI debugTMP;
        //private int debugCount = 0;
        //public void MyDebug(string text)
        //{
        //    if (debugTMP != null)
        //    {
        //        debugCount++;
        //        debugTMP.text = $"{debugCount}: {text}";
        //    }
        //}


        //public override void OnPostSerialization(SerializationResult result)
        //{
        //    MyDebug($"{result.byteCount}B Synced");
        //}
        #endregion


        [SerializeField] private UpdateHandler updateHandler;

        [SerializeField] private VRCStation station;

        // 最初に非アクティブにする
        [SerializeField] private GameObject eyeRangeBox;
        [SerializeField] private GameObject surface;
        [SerializeField] private GameObject limits;

        [UdonSynced] private int syncSittingPlayerID;
        [UdonSynced] private float syncEnterHeight = 0.0f;
        [UdonSynced] private float syncStationHeight = 0.0f;
        [UdonSynced] private float syncYaw = 0.0f;

        private bool isLocalPlayerSitting = false;

        private const float AutoAdjustTime = 2.5f;
        private int autoAdjustCount = 0;

        private const float InputThreshold = 0.5f;

        private bool lastHeightChange = false;
        private bool lastRotationChange = false;

        // 同期のインターバル
        private const float SyncInterval = 1.0f;
        private const float RetryInterval = 1.0f; // ネットワークが詰まっていた際の再同期用
        // SendCustomEventDelayedSeconds の予約状況
        private bool isPendingSync = false;
        // 最後に同期した時間
        private double lastSyncTime = double.MinValue;

        // 定期同期のインターバル
        private const float PeriodicSyncInterval = 10.0f;
        // 定期同期用 SendCustomEventDelayedSeconds の予約状況
        private bool isPendingPeriodicSync = false;








        private void OnEnable()
        {
            if (eyeRangeBox.activeInHierarchy)
            {
                eyeRangeBox.SetActive(false);
            }

            if (surface.activeInHierarchy)
            {
                surface.SetActive(false);
            }

            if (limits.activeInHierarchy)
            {
                limits.SetActive(false);
            }

            updateHandler.SetInitialHeights();
        }


        public override void Interact()
        {
            station.UseStation(Networking.LocalPlayer);
        }


        // Join時に既に他のプレイヤーが座っていた場合、OnStationEntered > OnStationExited > OnStationEntered の順で呼ばれる。
        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                ReflectSyncedData();
                return;
            }

            isLocalPlayerSitting = true;

            if (!Networking.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
            syncSittingPlayerID = Networking.LocalPlayer.playerId;

            StartAutoAdjust();

            RequestSync();
            RequestPeriodicSync();
        }


        public override void OnStationExited(VRCPlayerApi player)
        {
            ResetChair();

            if (Networking.IsOwner(this.gameObject))
            {
                syncEnterHeight = updateHandler.InitialEnterHeight;
                syncStationHeight = updateHandler.InitialStationHeight;
                syncYaw = 0.0f;
                RequestSync();
            }
        }


        // プレイヤーが座ったまま退出したときのため（OnStationExitedは発火しない）
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player.playerId != syncSittingPlayerID) { return; }

            ResetChair();
        }


        // プレイヤーが座ったまま退出した後、確実にオーナーが移動してから同期（OnStationEntered()でも発火する可能性があるが、悪影響はない）
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!player.isLocal) { return; }

            syncEnterHeight = updateHandler.InitialEnterHeight;
            syncStationHeight = updateHandler.InitialStationHeight;
            syncYaw = 0.0f;
            RequestSync();
        }


        private void ResetChair()
        {
            isLocalPlayerSitting = false;

            syncSittingPlayerID = -1;

            updateHandler.ResetChairTransform();

            if (updateHandler.enabled)
            {
                updateHandler.enabled = false;
            }

            lastHeightChange = false;
            lastRotationChange = false;
        }








        // -------- UpdateHandlerの有効・無効化の関数 --------


        private void EnableUpdate()
        {
            if (!isLocalPlayerSitting) { return; }
            if (updateHandler.enabled) { return; }

            if (updateHandler.IsAutoAdjusting)
            {
                updateHandler.enabled = true;
            }
            else
            {
                if (updateHandler.HorizontalRequest || updateHandler.VerticalRequest)
                {
                    updateHandler.enabled = true;
                }
            }
        }


        private void DisableUpdate()
        {
            if (!isLocalPlayerSitting) { return; }
            if (!updateHandler.enabled) { return; }

            if (updateHandler.IsAutoAdjusting)
            {
                return;
            }
            else
            {
                if (!updateHandler.HorizontalRequest && !updateHandler.VerticalRequest)
                {
                    updateHandler.enabled = false;
                }
            }
        }








        // -------- Sit時高さ調整の関数 --------


        private void StartAutoAdjust()
        {
            if (!isLocalPlayerSitting) { return; }

            updateHandler.IsAutoAdjusting = true;
            EnableUpdate();

            autoAdjustCount++;

            SendCustomEventDelayedSeconds(nameof(StopAutoAdjust), AutoAdjustTime);
        }


        public void StopAutoAdjust()
        {
            autoAdjustCount = Mathf.Max(autoAdjustCount - 1, 0);
            if (autoAdjustCount > 0) { return; }
            if (!isLocalPlayerSitting) { return; }

            updateHandler.IsAutoAdjusting = false;
            DisableUpdate();
            RequestSync();
        }








        // -------- 操作入力処理の関数 --------


        // 左スティックの水平方向の入力を受け取る
        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            if (!isLocalPlayerSitting) { return; }
            if (Utilities.IsValid(VRCCameraSettings.PhotoCamera) && VRCCameraSettings.PhotoCamera.Active) { return; }

            if (Networking.LocalPlayer.IsUserInVR())
            {
                if (Mathf.Abs(value) >= InputThreshold)
                {
                    station.ExitStation(Networking.LocalPlayer);
                }
            }
            else
            {
                if (Mathf.Abs(value) >= InputThreshold)
                {
                    int dir = (value >= 0) ? 1 : -1;
                    updateHandler.SetRequestAndDirHorizontal(true, dir);
                    EnableUpdate();

                    lastRotationChange = true;
                }
                else
                {
                    if (updateHandler.RotateTimer != 0.0f)
                    {
                        updateHandler.RotateTimer = 0.0f;
                    }

                    updateHandler.SetRequestAndDirHorizontal(false, 0);
                    DisableUpdate();

                    if (!lastHeightChange && lastRotationChange)
                    {
                        RequestSync();
                    }

                    lastRotationChange = false;
                }
            }
        }


        // 左スティックの垂直方向の入力を受け取る
        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            if (!isLocalPlayerSitting) { return; }
            if (Utilities.IsValid(VRCCameraSettings.PhotoCamera) && VRCCameraSettings.PhotoCamera.Active) { return; }

            if (Mathf.Abs(value) >= InputThreshold)
            {
                station.ExitStation(Networking.LocalPlayer);
            }
        }


        // 右スティックの水平方向の入力を受け取る
        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            if (!Networking.LocalPlayer.IsUserInVR()) { return; }
            if (!isLocalPlayerSitting) { return; }
            if (Utilities.IsValid(VRCCameraSettings.PhotoCamera) && VRCCameraSettings.PhotoCamera.Active) { return; }

            if (Mathf.Abs(value) >= InputThreshold)
            {
                int dir = (value >= 0.0f) ? 1 : -1;
                updateHandler.SetRequestAndDirHorizontal(true, dir);
                EnableUpdate();

                lastRotationChange = true;
            }
            else
            {
                if (updateHandler.RotateTimer != 0.0f)
                {
                    updateHandler.RotateTimer = 0.0f;
                }

                updateHandler.SetRequestAndDirHorizontal(false, 0);
                DisableUpdate();

                if (!lastHeightChange && lastRotationChange)
                {
                    RequestSync();
                }

                lastRotationChange = false;
            }
        }


        // 右スティックの垂直方向の入力を受け取る
        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (!Networking.LocalPlayer.IsUserInVR()) { return; }
            if (!isLocalPlayerSitting) { return; }
            if (updateHandler.IsAutoAdjusting) { return; }
            if (Utilities.IsValid(VRCCameraSettings.PhotoCamera) && VRCCameraSettings.PhotoCamera.Active) { return; }

            if (Mathf.Abs(value) >= InputThreshold)
            {
                int dir = (value > 0.0f) ? 1 : -1;
                updateHandler.SetRequestAndDirVertical(true, dir);
                EnableUpdate();

                lastHeightChange = true;
            }
            else
            {
                updateHandler.SetRequestAndDirVertical(false, 0);
                DisableUpdate();

                if (lastHeightChange && !lastRotationChange)
                {
                    RequestSync();
                }

                lastHeightChange = false;
            }
        }


        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (Networking.LocalPlayer.IsUserInVR()) { return; }
            if (!isLocalPlayerSitting) { return; }
            if (updateHandler.IsAutoAdjusting) { return; }
            if (Utilities.IsValid(VRCCameraSettings.PhotoCamera) && VRCCameraSettings.PhotoCamera.Active) { return; }

            if (value)
            {
                updateHandler.SetRequestAndDirVertical(true, 0);
                EnableUpdate();

                lastHeightChange = true;
            }
            else
            {
                updateHandler.SetRequestAndDirVertical(false, 0);
                DisableUpdate();

                if (lastHeightChange && !lastRotationChange)
                {
                    RequestSync();
                }

                lastHeightChange = false;
            }
        }








        // -------- 同期用の関数 --------


        // 入力が途切れた時に呼ばれる
        // 連続して呼ばれても同期間のインターバルを保つ
        private void RequestSync()
        {
            // 重複予約回避
            if (isPendingSync) { return; }
            if (!Networking.IsOwner(this.gameObject)) { return; }

            double now = Time.timeAsDouble;

            if (now >= lastSyncTime + SyncInterval)
            {
                // 前回の同期から SyncInterval 秒経っていれば即同期実行
                ExecuteSync();
            }
            else
            {
                // 次に実行可能なタイミングまでの差分だけ待つ
                float delay = (float)(lastSyncTime + SyncInterval - now) + 0.01f;

                if (delay > 0.0f)
                {
                    SendCustomEventDelayedSeconds(nameof(ExecuteSync), delay);
                    isPendingSync = true;
                }
                else
                {
                    // 万が一待機時間がなかったら即同期実行
                    ExecuteSync();
                }
            }
        }


        public void ExecuteSync()
        {
            isPendingSync = false;

            if (!Networking.IsOwner(this.gameObject)) { return; }

            if (Networking.IsClogged)
            {
                // ネットワークが詰まっていたら RetryInterval 秒後に同期予約
                SendCustomEventDelayedSeconds(nameof(ExecuteSync), RetryInterval);
                isPendingSync = true;
            }
            else
            {
                // 正常なら同期実行
                UpdateSyncedVariables();
                RequestSerialization();
                lastSyncTime = Time.timeAsDouble;
            }
        }


        // 定期的に同期させる
        // 連続して呼ばれても同期間のインターバルを保つ
        private void RequestPeriodicSync()
        {
            // 重複予約回避
            if (isPendingPeriodicSync) { return; }
            if (!Networking.IsOwner(this.gameObject)) { return; }

            SendCustomEventDelayedSeconds(nameof(ExecutePeriodicSync), PeriodicSyncInterval);
            isPendingPeriodicSync = true;
        }


        public void ExecutePeriodicSync()
        {
            isPendingPeriodicSync = false;

            if (!Networking.IsOwner(this.gameObject)) { return; }

            RequestSync();

            if (isLocalPlayerSitting)
            {
                RequestPeriodicSync();
            }
        }


        private void UpdateSyncedVariables()
        {
            syncEnterHeight = updateHandler.GetEnterHeight();
            syncStationHeight = updateHandler.GetStationHeight();
            syncYaw = updateHandler.GetYaw();
        }


        public override void OnDeserialization()
        {
            ReflectSyncedData();
        }


        private void ReflectSyncedData()
        {
            updateHandler.ReflectSyncedData(syncEnterHeight, syncStationHeight, syncYaw);
        }
    }
}
