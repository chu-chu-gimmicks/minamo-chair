using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private const float INPUT_THRESHOLD = 0.5f;

        private bool _isPhotoCameraValid = false;
        private VRCCameraSettings _photoCamera;

        private AxisState InputStateLX { get; set; } = AxisState.Neutral;
        private AxisState InputStateLY { get; set; } = AxisState.Neutral;
        private AxisState InputStateRX { get; set; } = AxisState.Neutral;
        private AxisState InputStateRY { get; set; } = AxisState.Neutral;
        private ButtonState InputStateJump { get; set; } = ButtonState.Released;




        private void InitializeInputReceiver()
        {
            VRCCameraSettings photoCamera = VRCCameraSettings.PhotoCamera;
            _isPhotoCameraValid = Utilities.IsValid(photoCamera);
            _photoCamera = photoCamera;
        }


        private void ResetInputReceiver()
        {
            InputStateLX = AxisState.Neutral;
            InputStateLY = AxisState.Neutral;
            InputStateRX = AxisState.Neutral;
            InputStateRY = AxisState.Neutral;
            InputStateJump = ButtonState.Released;
        }


        public void _InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            if (!CanReceiveInput())
            {
                InputStateLX = AxisState.Neutral;
                return;
            }

            InputStateLX = GetAxisState(value);
        }


        public void _InputMoveVertical(float value, UdonInputEventArgs args)
        {
            if (!CanReceiveInput())
            {
                InputStateLY = AxisState.Neutral;
                return;
            }

            InputStateLY = GetAxisState(value);
        }


        public void _InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            if (!CanReceiveInput())
            {
                InputStateRX = AxisState.Neutral;
                return;
            }

            InputStateRX = GetAxisState(value);
        }


        public void _InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (!CanReceiveInput())
            {
                InputStateRY = AxisState.Neutral;
                return;
            }

            InputStateRY = GetAxisState(value);
        }


        public void _InputJump(bool value, UdonInputEventArgs args)
        {
            if (!CanReceiveInput())
            {
                InputStateJump = ButtonState.Released;
                return;
            }

            InputStateJump = GetButtonState(value);
        }


        private bool CanReceiveInput()
        {
            return !(_isPhotoCameraValid && _photoCamera.Active);
        }


        private AxisState GetAxisState(float value)
        {
            if (value <= -INPUT_THRESHOLD) { return AxisState.Negative; }
            if (value >= INPUT_THRESHOLD) { return AxisState.Positive; }
            return AxisState.Neutral;
        }


        private ButtonState GetButtonState(bool value)
        {
            return value ? ButtonState.Pressed : ButtonState.Released;
        }
    }
}
