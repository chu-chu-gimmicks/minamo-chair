
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private bool _prevSyncInput = false;




        private void InitializeSyncHandler()
        {

        }


        private void ResetSyncHandler()
        {
            _prevSyncInput = false;
        }


        private void UpdateSync(bool isUserInVR)
        {
            bool prevInput = _prevSyncInput;
            bool currInput = isUserInVR ? InputStateLX != AxisState.Neutral || InputStateLY != AxisState.Neutral || InputStateRX != AxisState.Neutral || InputStateRY != AxisState.Neutral : InputStateJump == ButtonState.Pressed;
            _prevSyncInput = currInput;

            if (!prevInput && currInput)
            {
                UpdateSyncedTransform();
                RequestSync();
            }
        }
    }
}
