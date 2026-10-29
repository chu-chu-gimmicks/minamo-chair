using UnityEngine;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    public enum GizmoShape
    {
        Cube,
        WireCube,
        Sphere,
        WireSphere,
    }


    public class GizmoDrawer : MonoBehaviour
    {
        public class FieldNames
        {
            public const string Description = nameof(_description);
            public const string DrawGizmo = nameof(_drawGizmo);

            public const string Shape = nameof(_shape);
            public const string ReferenceTransform = nameof(_referenceTransform);
            public const string Color = nameof(_color);

            public const string Center = nameof(_center);
            public const string Size = nameof(_size);
            public const string Radius = nameof(_radius);
        }




        [SerializeField, TextArea] private string _description;
        [SerializeField] private bool _drawGizmo;

        [SerializeField] private GizmoShape _shape;
        [SerializeField] private Transform _referenceTransform;
        [SerializeField] private Color32 _color;

        [SerializeField] private Vector3 _center;
        [SerializeField] private Vector3 _size;
        [SerializeField] private float _radius;




        private void OnDrawGizmos()
        {
            if (_drawGizmo)
            {
                Matrix4x4 matrix = _referenceTransform == null
                    ? this.transform.localToWorldMatrix
                    : _referenceTransform.localToWorldMatrix;

                switch (_shape)
                {
                    case GizmoShape.Cube:
                        DrawCube(matrix, _color, _center, _size);
                        break;
                    case GizmoShape.WireCube:
                        DrawWireCube(matrix, _color, _center, _size);
                        break;
                    case GizmoShape.Sphere:
                        DrawSphere(matrix, _color, _center, _radius);
                        break;
                    case GizmoShape.WireSphere:
                        DrawWireSphere(matrix, _color, _center, _radius);
                        break;
                }
            }
        }


        public static void DrawCube(Matrix4x4 matrix, Color32 color, Vector3 center, Vector3 size)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Gizmos.matrix = matrix;
            Gizmos.color = color;
            Gizmos.DrawCube(center, size);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }


        public static void DrawWireCube(Matrix4x4 matrix, Color32 color, Vector3 center, Vector3 size)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Gizmos.matrix = matrix;
            Gizmos.color = color;
            Gizmos.DrawWireCube(center, size);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }


        public static void DrawSphere(Matrix4x4 matrix, Color32 color, Vector3 center, float radius)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Gizmos.matrix = matrix;
            Gizmos.color = color;
            Gizmos.DrawSphere(center, radius);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }


        public static void DrawWireSphere(Matrix4x4 matrix, Color32 color, Vector3 center, float radius)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Gizmos.matrix = matrix;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(center, radius);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
