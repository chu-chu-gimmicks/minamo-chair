
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private const float SYNC_INTERVAL = 1f;
        private const float RETRY_INTERVAL = 1f;

        private const float PERIODIC_SYNC_INTERVAL = 10f;

        [UdonSynced] private int _sittingPlayerId = 0;
        [UdonSynced] private float _enterPointHeight = 0.0f;
        [UdonSynced] private float _modelHeight = 0.0f;
        [UdonSynced] private float _yaw = 0.0f;

        private bool _isPendingSync = false;
        private double _lastSyncTime = 0f;

        private bool _isPendingPeriodicSync = false;




        private void InitializeSyncedData()
        {
            double now = Networking.GetServerTimeInSeconds();
            _lastSyncTime = now;
        }


        private void ResetSyncedData()
        {

        }


        private void RequestSync()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }
            if (_isPendingSync) { return; }

            double now = Networking.GetServerTimeInSeconds();
            double elapsed = Networking.CalculateServerDeltaTime(now, _lastSyncTime);
            double remaining = SYNC_INTERVAL - elapsed;
            if (remaining > 0.0f)
            {
                Log($"SyncSender", $"{nameof(RequestSync)}", $"{nameof(now)}={now}, {nameof(remaining)}={remaining}");

                SendCustomEventDelayedSeconds(nameof(ExecuteSync), (float)remaining);
                _isPendingSync = true;
            }
            else
            {
                ExecuteSync();
            }
        }


        public void ExecuteSync()
        {
            _isPendingSync = false;

            if (!Networking.IsOwner(this.gameObject)) { return; }

            if (Networking.IsClogged)
            {
                SendCustomEventDelayedSeconds(nameof(ExecuteSync), RETRY_INTERVAL);
                _isPendingSync = true;
            }
            else
            {
                double now = Networking.GetServerTimeInSeconds();

                Log($"SyncSender", $"{nameof(ExecuteSync)}", $"{nameof(now)}={now}, {nameof(_lastSyncTime)}={_lastSyncTime}, diff={now-_lastSyncTime}");

                RequestSerialization();
                _lastSyncTime = now;
            }
        }


        private void RequestPeriodicSync()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }
            if (_isPendingPeriodicSync) { return; }

            SendCustomEventDelayedSeconds(nameof(ExecutePeriodicSync), PERIODIC_SYNC_INTERVAL);
            _isPendingPeriodicSync = true;
        }


        public void ExecutePeriodicSync()
        {
            _isPendingPeriodicSync = false;

            if (!Networking.IsOwner(this.gameObject)) { return; }

            UpdateSyncedTransform();
            RequestSync();

            if (_state != State.Idle)
            {
                Log($"SyncReceiver", nameof(ExecutePeriodicSync), $"Request next periodic sync");

                RequestPeriodicSync();
            }
        }


        private void UpdateSyncedTransform()
        {
            _enterPointHeight = _enterPoint.localPosition.y;
            _modelHeight = _model.localPosition.y;
            _yaw = _enterPoint.localEulerAngles.y;
        }


        public override void OnDeserialization()
        {
            Log($"SyncReceiver", nameof(OnDeserialization));

            ReflectSyncedData();
        }


        private void ReflectSyncedData()
        {
            Log($"SyncReceiver", nameof(ReflectSyncedData));

            Vector3 pos;

            pos = _enterPoint.localPosition;
            pos.y = _enterPointHeight;
            _enterPoint.localPosition = pos;

            pos = _model.localPosition;
            pos.y = _modelHeight;
            _model.localPosition = pos;

            Vector3 rot;

            rot = _enterPoint.localEulerAngles;
            rot.y = _yaw;
            _enterPoint.localEulerAngles = rot;

            rot = _model.localEulerAngles;
            rot.y = _yaw;
            _model.localEulerAngles = rot;
        }
    }
}
