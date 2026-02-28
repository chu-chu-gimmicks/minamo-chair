using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.MinamoChair
{
    public class UpdateHandler : UdonSharpBehaviour
    {
        [SerializeField] private Transform enterPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private Transform eyeRangeBox;
        [SerializeField] private Transform maxHeight;
        [SerializeField] private Transform minHeight;
        [SerializeField] private Transform station;
        [SerializeField] private Transform surface;

        [SerializeField] private Transform guideTransform;
        [SerializeField] private MeshRenderer guideMeshRenderer;
        [SerializeField] private Material[] guideMats;
        [SerializeField] private Transform loading;

        public float InitialEnterHeight { get; private set; } = 0.0f;
        public float InitialStationHeight { get; private set; } = 0.0f;

        private bool _isAutoAdjusting = false;
        public bool IsAutoAdjusting
        {
            get => _isAutoAdjusting;
            set
            {
                if (_isAutoAdjusting == value) { return; }
                _isAutoAdjusting = value;
                if (_isAutoAdjusting)
                {
                    exitPoint.localRotation = Quaternion.identity;
                    ShowGuide();
                }
                else
                {
                    HideGuide();
                }
            }
        }

        private const float ThighRadius = 0.04f;

        private const float HeightSpeedInVR      = 0.5f;
        private const float HeightSpeedInDesktop = 0.02f;

        private const float RotateSpeed = 60.0f;
        private const float PauseDuration = 0.4f;
        public float RotateTimer { get; set; }

        public bool HorizontalRequest { get; private set; }
        public int HorizontalDir { get; private set; }
        public void SetRequestAndDirHorizontal(bool request, int dir)
        {
            HorizontalRequest = request;
            HorizontalDir = dir;

            if (!HorizontalRequest)
            {
                RotateTimer = 0.0f;
            }
        }

        public bool VerticalRequest { get; private set; }
        public int VerticalDir { get; private set; }
        public void SetRequestAndDirVertical(bool request, int dir)
        {
            VerticalRequest = request;
            VerticalDir = dir;
        }

        private const float loadingUISpeed = 150.0f;




        private void OnDisable()
        {
            IsAutoAdjusting = false;
            SetRequestAndDirHorizontal(false, 0);
            SetRequestAndDirVertical(false, 0);
        }


        public void SetInitialHeights()
        {
            InitialEnterHeight = enterPoint.localPosition.y;
            InitialStationHeight = station.localPosition.y;
        }


        public void ResetChairTransform()
        {
            enterPoint.localPosition = new Vector3(0.0f, InitialEnterHeight, 0.0f);
            station.localPosition    = new Vector3(0.0f, InitialStationHeight, 0.0f);

            enterPoint.localRotation = Quaternion.identity;
            station.localRotation    = Quaternion.identity;
        }


        private void LateUpdate()
        {
            if (IsAutoAdjusting)
            {
                AutoAdjust();
                AdjustStationHeight();

                UpdateGuide();
            }
            else
            {
                if (!Networking.LocalPlayer.IsUserInVR())
                {
                    float wheelAxis = Input.GetAxis("Mouse ScrollWheel");
                    if (wheelAxis == 0.0f)
                    {
                        VerticalDir = 0;
                    }
                    else
                    {
                        VerticalDir = (wheelAxis > 0.0f) ? 1 : -1;
                    }
                }

                if (VerticalRequest)
                {
                    AdjustHeight(VerticalDir);
                }
            }

            if (HorizontalRequest)
            {
                RotateChair(HorizontalDir);
            }
        }




        private void AutoAdjust()
        {
            float viewHeight = VRCCameraSettings.ScreenCamera.Position.y;
            float lowerLimit = eyeRangeBox.position.y - (eyeRangeBox.localScale.y / 2);
            float upperLimit = eyeRangeBox.position.y + (eyeRangeBox.localScale.y / 2);

            if (viewHeight < lowerLimit)
            {
                float diff = lowerLimit - viewHeight;
                enterPoint.localPosition += new Vector3(0, diff, 0);
                station.localPosition += new Vector3(0, diff, 0);
            }
            else if (viewHeight > upperLimit)
            {
                float diff = viewHeight - upperLimit;
                enterPoint.localPosition -= new Vector3(0, diff, 0);
                station.localPosition -= new Vector3(0, diff, 0);
            }
        }


        private void AdjustStationHeight()
        {
            float diffToModel = enterPoint.position.y - station.position.y;
            float kneeHeight = Mathf.Min(Networking.LocalPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg).y, Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg).y);
            float lowerLegLength = kneeHeight - enterPoint.position.y;
            float offset = ThighRadius + surface.localPosition.y;
            station.localPosition += new Vector3(0, diffToModel + lowerLegLength - offset, 0);
        }




        private void AdjustHeight(int dir)
        {
            float heightDelta = 0.0f;
            float newHeadHeight = 0.0f;
            float lowerLimit = Mathf.Min(minHeight.position.y, eyeRangeBox.position.y - (eyeRangeBox.localScale.y / 2));
            float upperLimit = Mathf.Max(maxHeight.position.y, eyeRangeBox.position.y + (eyeRangeBox.localScale.y / 2));

            if (Networking.LocalPlayer.IsUserInVR())
            {
                heightDelta = dir * HeightSpeedInVR * Time.deltaTime;
            }
            else
            {
                heightDelta = dir * HeightSpeedInDesktop;
            }

            newHeadHeight = VRCCameraSettings.ScreenCamera.Position.y + heightDelta;
            if (dir < 0 && newHeadHeight < lowerLimit)
            {
                return;
            }
            else if (dir > 0 && newHeadHeight > upperLimit)
            {
                return;
            }

            Vector3 posDelta = new Vector3(0, heightDelta, 0);
            enterPoint.localPosition += posDelta;
            station.localPosition += posDelta;
        }


        private void RotateChair(int dir)
        {
            if (RotateTimer == 0.0f)
            {
                float yawNorm = Mathf.DeltaAngle(0.0f, enterPoint.localRotation.eulerAngles.y);
                float newYawNorm = yawNorm + dir * RotateSpeed * Time.deltaTime;

                // 右回転では浮動小数点数の誤差により稀に誤判定する。
                if ((yawNorm < 0.0f && newYawNorm >= 0.0f) || (yawNorm > 0.0f && newYawNorm <= 0.0f))
                {
                    enterPoint.localRotation = Quaternion.identity;
                    exitPoint.localRotation = Quaternion.identity;
                    station.localRotation = Quaternion.identity;
                    RotateTimer = PauseDuration;
                }
                else
                {
                    Quaternion newRot = Quaternion.Euler(0, newYawNorm, 0);
                    enterPoint.localRotation = newRot;
                    exitPoint.localRotation = newRot;
                    station.localRotation = newRot;
                }
            }
            else
            {
                RotateTimer = Mathf.Max(RotateTimer - Time.deltaTime, 0.0f);
            }
        }


        private void ShowGuide()
        {
            guideTransform.gameObject.SetActive(true);

            if (Networking.LocalPlayer.IsUserInVR())
            {
                guideMeshRenderer.material = guideMats[0];
            }
            else
            {
                guideMeshRenderer.material = guideMats[1];
            }
        }


        private void UpdateGuide()
        {
            Vector3 headPos = VRCCameraSettings.ScreenCamera.Position;
            guideTransform.position = headPos;

            loading.localEulerAngles += new Vector3(0, 0, -loadingUISpeed * Time.deltaTime);
        }


        private void HideGuide()
        {
            guideTransform.gameObject.SetActive(false);
        }


        public float GetEnterHeight()
        {
            return enterPoint.localPosition.y;
        }


        public float GetStationHeight()
        {
            return station.localPosition.y;
        }


        public float GetYaw()
        {
            return enterPoint.localRotation.eulerAngles.y;
        }


        public void ReflectSyncedData(float enterHeight, float stationHeight, float yaw)
        {
            enterPoint.localPosition = new Vector3(0.0f, enterHeight, 0.0f);
            station.localPosition = new Vector3(0.0f, stationHeight, 0.0f);

            enterPoint.localRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
            station.localRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        }
    }
}
