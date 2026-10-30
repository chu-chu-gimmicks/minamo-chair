using UnityEngine;
using VRC.SDKBase;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private const float HEIGHT_ADJUSTMENT_SPEED = 0.5f;
        private const float THIGH_RADIUS = 0.03f;

        private float _initialEnterPointHeight = 0f;
        private float _initialModelHeight = 0f;

        private float _modelSurfaceOffset = 0f;




        private void InitializeHeightHandler()
        {
            _initialEnterPointHeight = _enterPoint.localPosition.y;
            _initialModelHeight = _model.localPosition.y;

            _modelSurfaceOffset = _modelSurfaceHeight - _model.localPosition.y;
        }


        private void BeginHeightHandler()
        {
            _enterPoint.localPosition = WithHeight(_enterPoint.localPosition, _initialEnterPointHeight);
            _model.localPosition = WithHeight(_model.localPosition, _initialModelHeight);
        }


        private void ResetHeightHandler(Transform referenceTransform)
        {
            _exitPoint.localPosition = _enterPoint.localPosition;

            Vector3 origin = referenceTransform.position;
            Vector3 direction = -referenceTransform.up;
            float maxDistance = 4.0f;
            int layerMask = ~0;
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, layerMask, queryTriggerInteraction))
            {
                Vector3 localHitPoint = referenceTransform.InverseTransformPoint(hit.point);
                float localTargetHeight = Mathf.Max(localHitPoint.y, _exitPoint.localPosition.y);
                _exitPoint.localPosition = WithHeight(_exitPoint.localPosition, localTargetHeight);
            }

            _enterPoint.localPosition = WithHeight(_enterPoint.localPosition, _initialEnterPointHeight);
            _model.localPosition = WithHeight(_model.localPosition, _initialModelHeight);
        }


        private void AdjustHeightAutomatically(Transform referenceTransform)
        {
            float localViewpointHeight = GetLocalViewpointHeight(referenceTransform);
            float localMinHeight = _autoAdjustmentMinHeight;
            float localMaxHeight = _autoAdjustmentMaxHeight;
            if (localViewpointHeight < localMinHeight)
            {
                float heightDelta = localMinHeight - localViewpointHeight;
                Vector3 posDelta = new Vector3(0f, heightDelta, 0f);
                _enterPoint.localPosition += posDelta;
            }
            else if (localViewpointHeight > localMaxHeight)
            {
                float heightDelta = localMaxHeight - localViewpointHeight;
                Vector3 posDelta = new Vector3(0f, heightDelta, 0f);
                _enterPoint.localPosition += posDelta;
            }
        }


        private void AdjustModelHeightAutomatically(Transform referenceTransform)
        {
            if (_modelAdjustmentMode != ModelAdjustmentMode.HeightAndYaw) { return; }

            float localKneeHeight = GetLocalKneeHeight(referenceTransform);
            float localTargetHeight = localKneeHeight - THIGH_RADIUS - _modelSurfaceOffset;

            _model.localPosition = WithHeight(_model.localPosition, localTargetHeight);
        }


        private void UpdateHeight(bool isUserInVR, Transform referenceTransform, float deltaTime)
        {
            if (!NeedsManualHeightAdjustment(isUserInVR, out int dir)) { return; }

            float localViewpointHeight = GetLocalViewpointHeight(referenceTransform);
            float speed = HEIGHT_ADJUSTMENT_SPEED * deltaTime;
            float heightDelta = dir * speed;
            float newLocalViewpointHeight = localViewpointHeight + heightDelta;

            float localMinHeight = _manualAdjustmentMinHeight;
            float localMaxHeight = _manualAdjustmentMaxHeight;
            if (dir < 0 && newLocalViewpointHeight < localMinHeight) { return; }
            if (dir > 0 && newLocalViewpointHeight > localMaxHeight) { return; }

            Vector3 posDelta = new Vector3(0, heightDelta, 0);
            _enterPoint.localPosition += posDelta;
            if (_modelAdjustmentMode == ModelAdjustmentMode.HeightAndYaw)
            {
                _model.localPosition += posDelta;
            }
        }


        private bool NeedsManualHeightAdjustment(bool isUserInVR, out int dir)
        {
            if (isUserInVR)
            {
                dir = (int)InputStateRY;
                return InputStateRY != AxisState.Neutral;
            }
            else
            {
                dir = (int)InputStateLY;
                return InputStateJump == ButtonState.Pressed && InputStateLY != AxisState.Neutral;
            }
        }


        private float GetLocalViewpointHeight(Transform referenceTransform)
        {
            // Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head) だと非ヒューマノイドボーンでは機能しない
            // VRCCameraSettings.ScreenCamera.Position.y だと三人称視点のとき三人称カメラ座標になる
            Vector3 viewpointPos = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Vector3 localViewpointPos = referenceTransform.InverseTransformPoint(viewpointPos);
            return localViewpointPos.y;
        }


        private float GetLocalKneeHeight(Transform referenceTransform)
        {
            Vector3 leftLowerLegPos = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg);
            Vector3 rightLowerLegPos = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg);
            Vector3 localLeftLowerLegPos = referenceTransform.InverseTransformPoint(leftLowerLegPos);
            Vector3 localRightLowerLegPos = referenceTransform.InverseTransformPoint(rightLowerLegPos);
            return Mathf.Min(localLeftLowerLegPos.y, localRightLowerLegPos.y);
        }


        private static Vector3 WithHeight(Vector3 position, float height)
        {
            position.y = height;
            return position;
        }
    }
}
