using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

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


        private void BeginSyncedData()
        {
        }


        private void ResetSyncedData()
        {
        }


        private void ScheduleSync()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }
            if (_isPendingSync) { return; }

            double now = Networking.GetServerTimeInSeconds();
            double elapsed = Networking.CalculateServerDeltaTime(now, _lastSyncTime);
            double remaining = SYNC_INTERVAL - elapsed;
            if (remaining > 0.0f)
            {
                Log.Info($"SyncSender", $"{nameof(ScheduleSync)}: {nameof(now)}={now:F3}, {nameof(remaining)}={remaining:F3}");

                SendCustomEventDelayedSeconds(nameof(ProcessSync), (float)remaining);
                _isPendingSync = true;
            }
            else
            {
                ProcessSync();
            }
        }


        public void ProcessSync()
        {
            _isPendingSync = false;

            if (!Networking.IsOwner(this.gameObject)) { return; }

            if (Networking.IsClogged)
            {
                SendCustomEventDelayedSeconds(nameof(ProcessSync), RETRY_INTERVAL);
                _isPendingSync = true;
            }
            else
            {
                double now = Networking.GetServerTimeInSeconds();

                Log.Info($"SyncSender", $"{nameof(ProcessSync)}: {nameof(now)}={now:F3}, {nameof(_lastSyncTime)}={_lastSyncTime:F3}, diff={now-_lastSyncTime:F3}");

                RequestSerialization();
                _lastSyncTime = now;
            }
        }


        private void SchedulePeriodicSync()
        {
            if (!Networking.IsOwner(this.gameObject)) { return; }
            if (_isPendingPeriodicSync) { return; }

            Log.Info($"SyncReceiver", $"{nameof(SchedulePeriodicSync)}: Schedule next periodic sync");

            SendCustomEventDelayedSeconds(nameof(ProcessPeriodicSync), PERIODIC_SYNC_INTERVAL);
            _isPendingPeriodicSync = true;
        }


        public void ProcessPeriodicSync()
        {
            _isPendingPeriodicSync = false;

            if (!Networking.IsOwner(this.gameObject)) { return; }

            UpdateSyncedTransform();
            ScheduleSync();

            if (_state != State.Idle)
            {
                SchedulePeriodicSync();
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
            Log.Info($"SyncReceiver", nameof(OnDeserialization));

            ReflectSyncedData();
        }


        private void ReflectSyncedData()
        {
            Log.Info($"SyncReceiver", nameof(ReflectSyncedData));

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
