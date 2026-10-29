using ChuChuGimmicks.MinamoChair.Runtime;
using UnityEngine;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    public class Setup : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public static class FieldNames
        {
            public const string DrawGizmo = nameof(_drawGizmo);

            public const string AutoAdjustmentMinHeight = nameof(_autoAdjustmentMinHeight);
            public const string AutoAdjustmentMaxHeight = nameof(_autoAdjustmentMaxHeight);

            public const string ManualAdjustmentMinHeight = nameof(_manualAdjustmentMinHeight);
            public const string ManualAdjustmentMaxHeight = nameof(_manualAdjustmentMaxHeight);

            public const string ModelAdjustmentMode = nameof(_modelAdjustmentMode);
            public const string ModelSurfaceHeight = nameof(_modelSurfaceHeight);

            public const string Manager = nameof(_manager);
        }




        private static readonly Color32 AUTO_ADJUSTMENT_CUBE_COLOR = new Color32(0, 255, 0, 127);
        private static readonly Vector3 AUTO_ADJUSTMENT_CUBE_SCALE = new Vector3(0.25f, 1f, 0.25f);

        private static readonly Vector3 MANUAL_ADJUSTMENT_CUBE_SCALE = new Vector3(0.25f, 1f, 0.25f);
        private static readonly Color32 MANUAL_ADJUSTMENT_CUBE_COLOR = new Color32(255, 0, 0, 127);

        private static readonly Vector3 MODEL_SURFACE_CUBE_SCALE = new Vector3(0.25f, 0.001f, 0.25f);
        private static readonly Color32 MODEL_SURFACE_CUBE_COLOR = new Color32(0, 0, 255, 127);

        [SerializeField] private bool _drawGizmo;

        [SerializeField] private float _autoAdjustmentMinHeight;
        [SerializeField] private float _autoAdjustmentMaxHeight;

        [SerializeField] private float _manualAdjustmentMinHeight;
        [SerializeField] private float _manualAdjustmentMaxHeight;

        [SerializeField] private ModelAdjustmentMode _modelAdjustmentMode;
        [SerializeField] private float _modelSurfaceHeight;

        [SerializeField] private Manager _manager;




        private void OnDrawGizmos()
        {
            if (_drawGizmo)
            {
                Transform referenceTransform = _manager.ReferenceTransform;
                DrawAutoAdjustmentCube(referenceTransform, _autoAdjustmentMaxHeight, _autoAdjustmentMinHeight);
                DrawManualAdjustmentCube(referenceTransform, _manualAdjustmentMaxHeight, _manualAdjustmentMinHeight);
                if (_modelAdjustmentMode == ModelAdjustmentMode.HeightAndYaw)
                {
                    DrawModelSurfaceCube(referenceTransform, _modelSurfaceHeight);
                }
            }
        }


        private void DrawAutoAdjustmentCube(Transform referenceTransform, float maxHeight, float minHeight)
        {
            if (maxHeight <= minHeight) { return; }

            Matrix4x4 matrix = referenceTransform.localToWorldMatrix;
            Color32 color = AUTO_ADJUSTMENT_CUBE_COLOR;
            Vector3 center = Vector3.zero;
            center.y = (maxHeight + minHeight) * 0.5f;
            Vector3 scale = AUTO_ADJUSTMENT_CUBE_SCALE;
            scale.y = maxHeight - minHeight;

            GizmoDrawer.DrawCube(matrix, color, center, scale);
        }


        private void DrawManualAdjustmentCube(Transform referenceTransform, float maxHeight, float minHeight)
        {
            if (maxHeight <= minHeight) { return; }

            Matrix4x4 matrix = referenceTransform.localToWorldMatrix;
            Color32 color = MANUAL_ADJUSTMENT_CUBE_COLOR;
            Vector3 center = Vector3.zero;
            center.y = (maxHeight + minHeight) * 0.5f;
            Vector3 scale = MANUAL_ADJUSTMENT_CUBE_SCALE;
            scale.y = maxHeight - minHeight;

            GizmoDrawer.DrawWireCube(matrix, color, center, scale);
        }


        private void DrawModelSurfaceCube(Transform referenceTransform, float height)
        {
            Matrix4x4 matrix = referenceTransform.localToWorldMatrix;
            Color32 color = MODEL_SURFACE_CUBE_COLOR;
            Vector3 center = Vector3.zero;
            center.y = height;
            Vector3 scale = MODEL_SURFACE_CUBE_SCALE;

            GizmoDrawer.DrawCube(matrix, color, center, scale);
        }
    }
}
