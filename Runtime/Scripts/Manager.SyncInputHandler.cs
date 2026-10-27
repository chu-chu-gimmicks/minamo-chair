namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private bool _prevSyncInput = false;




        private void InitializeSyncHandler()
        {
        }


        private void BeginSyncHandler()
        {
            _prevSyncInput = false;
        }


        private void ResetSyncHandler()
        {
            _prevSyncInput = false;
        }


        private void UpdateSync(bool isUserInVR)
        {
            bool prevInput = _prevSyncInput;
            bool currInput = isUserInVR
                ? InputStateRX == AxisState.Neutral && InputStateRY == AxisState.Neutral
                : InputStateJump == ButtonState.Released;
            _prevSyncInput = currInput;

            if (!prevInput && currInput)
            {
                UpdateSyncedTransform();
                ScheduleSync();
            }
        }
    }
}
