
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private const float YAW_ADJUSTMENT_SPEED = 60.0f;
        private const float PAUSE_DURATION = 0.4f;

        private float _yawAdjustmentTimer = 0f;
        private AxisState _prevYawAdjustmentInput = AxisState.Neutral;




        private void InitializeYawHandler()
        {
        }


        private void ResetYawHandler()
        {
            _exitPoint.localRotation = _enterPoint.localRotation;

            _enterPoint.localRotation = Quaternion.identity;
            _model.localRotation = Quaternion.identity;

            _yawAdjustmentTimer = 0f;
            _prevYawAdjustmentInput = AxisState.Neutral;
        }


        private void AdjustYawManually(bool isUserInVR, float deltaTime)
        {
            if (!NeedsManualYawAdjustment(isUserInVR, deltaTime, out int dir)) {  return; }

            float yaw = Mathf.DeltaAngle(0.0f, _enterPoint.localRotation.eulerAngles.y);
            float newYaw = yaw + dir * YAW_ADJUSTMENT_SPEED * deltaTime;

            // 右回転では浮動小数点数の誤差により稀に誤判定する。
            if ((yaw < 0.0f && newYaw >= 0.0f) || (yaw > 0.0f && newYaw <= 0.0f))
            {
                _enterPoint.localRotation = Quaternion.identity;
                if (_modelAdjustmentMode == ModelAdjustmentMode.HeightAndYaw || _modelAdjustmentMode == ModelAdjustmentMode.YawOnly)
                {
                    _model.localRotation = Quaternion.identity;
                }
                _yawAdjustmentTimer = PAUSE_DURATION;
            }
            else
            {
                Quaternion newRot = Quaternion.Euler(0, newYaw, 0);
                _enterPoint.localRotation = newRot;
                if (_modelAdjustmentMode == ModelAdjustmentMode.HeightAndYaw || _modelAdjustmentMode == ModelAdjustmentMode.YawOnly)
                {
                    _model.localRotation = newRot;
                }
            }
        }


        private bool NeedsManualYawAdjustment(bool isUserInVR, float deltaTime, out int dir)
        {

            _yawAdjustmentTimer = Mathf.Max(_yawAdjustmentTimer - deltaTime, 0.0f);

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

            if (_yawAdjustmentTimer > 0.0f)
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
    }
}
