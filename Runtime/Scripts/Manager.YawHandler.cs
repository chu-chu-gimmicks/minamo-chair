using UnityEngine;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private const float YAW_ADJUSTMENT_SPEED = 60f;
        private const float PAUSE_DURATION = 0.5f;

        private float _yawAdjustmentTimer = 0f;
        private AxisState _prevYawAdjustmentInput = AxisState.Neutral;




        private void InitializeYawHandler()
        {
        }


        private void BeginYawHandler()
        {
            _enterPoint.localRotation = WithYaw(_enterPoint.localRotation, 0f);
            _model.localRotation = WithYaw(_model.localRotation, 0f);

            _yawAdjustmentTimer = 0f;
            _prevYawAdjustmentInput = AxisState.Neutral;
        }


        private void ResetYawHandler()
        {
            _exitPoint.localRotation = WithYaw(_exitPoint.localRotation, _enterPoint.localEulerAngles.y);

            _enterPoint.localRotation = WithYaw(_enterPoint.localRotation, 0f);
            _model.localRotation = WithYaw(_model.localRotation, 0f);

            _yawAdjustmentTimer = 0f;
            _prevYawAdjustmentInput = AxisState.Neutral;
        }


        private void UpdateYaw(bool isUserInVR, float deltaTime)
        {
            if (!NeedsManualYawAdjustment(isUserInVR, deltaTime, out int dir)) {  return; }

            float yaw = Mathf.DeltaAngle(0f, _enterPoint.localRotation.eulerAngles.y);
            float yawDelta = dir * YAW_ADJUSTMENT_SPEED * deltaTime;
            float newYaw = yaw + yawDelta;

            // 右回転では浮動小数点数の誤差により稀に誤判定する。
            if ((yaw < 0f && newYaw >= 0f) || (yaw > 0f && newYaw <= 0f))
            {
                _enterPoint.localRotation = WithYaw(_enterPoint.localRotation, 0f);

                if (_modelAdjustmentMode == ModelAdjustmentMode.HeightAndYaw || _modelAdjustmentMode == ModelAdjustmentMode.YawOnly)
                {
                    _model.localRotation = WithYaw(_model.localRotation, 0f);
                }

                _yawAdjustmentTimer = PAUSE_DURATION;
            }
            else
            {
                _enterPoint.localRotation = WithYaw(_enterPoint.localRotation, newYaw);

                if (_modelAdjustmentMode == ModelAdjustmentMode.HeightAndYaw || _modelAdjustmentMode == ModelAdjustmentMode.YawOnly)
                {
                    _model.localRotation = WithYaw(_model.localRotation, newYaw);
                }
            }
        }


        private bool NeedsManualYawAdjustment(bool isUserInVR, float deltaTime, out int dir)
        {

            _yawAdjustmentTimer = Mathf.Max(_yawAdjustmentTimer - deltaTime, 0f);

            AxisState prevInput = _prevYawAdjustmentInput;
            AxisState currInput;
            if (isUserInVR)
            {
                currInput = InputStateRX;
            }
            else
            {
                currInput = InputStateJump == ButtonState.Pressed
                    ? InputStateLX
                    : AxisState.Neutral;
            }
            _prevYawAdjustmentInput = currInput;
            dir = (int)currInput;

            if (_yawAdjustmentTimer > 0f)
            {
                if (currInput == AxisState.Negative)
                {
                    if (prevInput == AxisState.Positive)
                    {
                        _yawAdjustmentTimer = 0f;
                        return true;
                    }
                }
                else if (currInput == AxisState.Positive)
                {
                    if (prevInput == AxisState.Negative)
                    {
                        _yawAdjustmentTimer = 0f;
                        return true;
                    }
                }
                else
                {
                    _yawAdjustmentTimer = 0f;
                    return false;
                }
                return false;
            }
            else
            {
                return currInput != AxisState.Neutral;
            }
        }


        private static Quaternion WithYaw(Quaternion rotation, float yaw)
        {
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.y = yaw;
            return Quaternion.Euler(eulerAngles);
        }
    }
}
