using VRC.SDKBase;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private bool _prevExitInput = false;



        
        private void InitializeExitInputHandler()
        {
        }


        private void ResetExitInputHandler()
        {
            _prevExitInput = false;
        }


        private void UpdateExit(bool isUserInVR)
        {
            bool prevInput = _prevExitInput;
            bool currInput = isUserInVR
                ? InputStateLX != AxisState.Neutral || InputStateLY != AxisState.Neutral
                : (InputStateLX != AxisState.Neutral || InputStateLY != AxisState.Neutral) && InputStateJump == ButtonState.Released;
            _prevExitInput = currInput;

            if (!prevInput  && currInput)
            {
                _station.ExitStation(Networking.LocalPlayer);
            }
        }
    }
}
