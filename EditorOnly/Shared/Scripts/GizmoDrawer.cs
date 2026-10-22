using UnityEngine;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    public static class GizmoDrawer
    {
        private static readonly Vector3 CUBE_SCALE = new Vector3(0.25f, 1f, 0.25f);
        private static readonly Color32 CUBE_COLOR = new Color32(0, 255, 0, 127);

        private static readonly Vector3 WIRE_CUBE_SCALE = new Vector3(0.25f, 1f, 0.25f);
        private static readonly Color32 WIRE_CUBE_COLOR = new Color32(255, 0, 0, 127);

        private static readonly Vector3 RECT_SCALE = new Vector3(0.25f, 0.001f, 0.25f);
        private static readonly Color32 RECT_COLOR = new Color32(0, 0, 255, 127);




        public static void DrawCube(Transform referenceTransform, float maxHeight, float minHeight)
        {
            if (maxHeight <= minHeight) { return; }

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Vector3 center = Vector3.zero;
            center.y = (maxHeight + minHeight) * 0.5f;
            Vector3 scale = CUBE_SCALE;
            scale.y = maxHeight - minHeight;

            Gizmos.matrix = referenceTransform.localToWorldMatrix * Matrix4x4.TRS(center, Quaternion.identity, scale);
            Gizmos.color = CUBE_COLOR;

            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }


        public static void DrawWireCube(Transform referenceTransform, float maxHeight, float minHeight)
        {
            if (maxHeight <= minHeight) { return; }

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Vector3 center = Vector3.zero;
            center.y = (maxHeight + minHeight) * 0.5f;
            Vector3 scale = WIRE_CUBE_SCALE;
            scale.y = maxHeight - minHeight;

            Gizmos.matrix = referenceTransform.localToWorldMatrix * Matrix4x4.TRS(center, Quaternion.identity, scale);
            Gizmos.color = WIRE_CUBE_COLOR;

            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }


        public static void DrawRect(Transform referenceTransform, float height)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;

            Vector3 center = Vector3.zero;
            center.y = height;
            Vector3 scale = RECT_SCALE;

            Gizmos.matrix = referenceTransform.localToWorldMatrix * Matrix4x4.TRS(center, Quaternion.identity, scale);
            Gizmos.color = RECT_COLOR;

            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
