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
                GizmoDrawer.DrawCube(_manager.ReferenceTransform, _autoAdjustmentMaxHeight, _autoAdjustmentMinHeight);
                GizmoDrawer.DrawWireCube(_manager.ReferenceTransform, _manualAdjustmentMaxHeight, _manualAdjustmentMinHeight);
                if (_modelAdjustmentMode == ModelAdjustmentMode.HeightAndYaw)
                {
                    GizmoDrawer.DrawRect(_manager.ReferenceTransform, _modelSurfaceHeight);
                }
            }
        }
    }
}
