using UnityEngine;
using VRC.SDKBase;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private bool _prevGuideShowInput = false;




        private void InitializeGuideHandler(bool isUserInVR)
        {
            _guide.gameObject.SetActive(true);

            Sprite guideTexture = _guideSprites[isUserInVR ? 0 : 1];
            _guideImage.sprite = guideTexture;

            _disabledOverlay.SetActive(true);
        }


        private void ResetGuideHandler()
        {
            _guide.gameObject.SetActive(false);
            _guide.localPosition = Vector3.zero;
            _guide.localRotation = Quaternion.identity;
            _disabledOverlay.SetActive(false);

            _prevGuideShowInput = false;
        }


        private void UpdateGuide(bool isUserInVR, Transform referenceTransform)
        {
            bool prevInput = _prevGuideShowInput;
            bool currInput = isUserInVR ? InputStateLX != AxisState.Neutral || InputStateLY != AxisState.Neutral || InputStateRX != AxisState.Neutral || InputStateRY != AxisState.Neutral : InputStateJump == ButtonState.Pressed;
            _prevGuideShowInput = currInput;

            if (currInput)
            {
                if (!prevInput)
                {
                    _guide.gameObject.SetActive(true);
                }

                AdjustGuideHeightAutomatically(referenceTransform);
            }
            else
            {
                if (prevInput)
                {
                    _guide.gameObject.SetActive(false);
                }
            }
        }


        private void AdjustGuideHeightAutomatically(Transform referenceTransform)
        {
            Vector3 viewpointPos = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Vector3 localViewPointPos = referenceTransform.InverseTransformPoint(viewpointPos);

            Vector3 pos = _guide.localPosition;
            pos.y = localViewPointPos.y;
            _guide.localPosition = pos;
        }


        private void OnAutoAdjustmentEndedGuideHandler()
        {
            _guide.gameObject.SetActive(false);
            _disabledOverlay.SetActive(false);
        }
    }
}
