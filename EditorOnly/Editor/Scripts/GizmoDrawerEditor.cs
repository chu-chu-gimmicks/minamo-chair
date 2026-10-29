using UnityEditor;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GizmoDrawer))]
    public class GizmoDrawerEditor : Editor
    {
        private SerializedProperty _descriptionProp;
        private SerializedProperty _drawGizmoProp;

        private SerializedProperty _shapeProp;
        private SerializedProperty _referenceTransformProp;
        private SerializedProperty _colorProp;

        private SerializedProperty _centerProp;
        private SerializedProperty _sizeProp;
        private SerializedProperty _radiusProp;

        private bool _isOpen = false;




        private void OnEnable()
        {
            _descriptionProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.Description);
            _drawGizmoProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.DrawGizmo);

            _shapeProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.Shape);
            _referenceTransformProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.ReferenceTransform);
            _colorProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.Color);

            _centerProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.Center);
            _sizeProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.Size);
            _radiusProp = serializedObject.FindProperty(GizmoDrawer.FieldNames.Radius);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(_descriptionProp.stringValue, MessageType.Info);
            EditorGUILayout.PropertyField(_drawGizmoProp);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginDisabledGroup(true);
            _isOpen = EditorGUILayout.Foldout(_isOpen, "References", true);
            if (_isOpen)
            {
                EditorGUILayout.PropertyField(_shapeProp);
                EditorGUILayout.PropertyField(_referenceTransformProp);
                EditorGUILayout.PropertyField(_colorProp);

                switch (_shapeProp.enumValueIndex)
                {
                    case (int)GizmoShape.Cube:
                    case (int)GizmoShape.WireCube:
                        EditorGUILayout.PropertyField(_centerProp);
                        EditorGUILayout.PropertyField(_sizeProp);
                        break;
                    case (int)GizmoShape.Sphere:
                    case (int)GizmoShape.WireSphere:
                        EditorGUILayout.PropertyField(_centerProp);
                        EditorGUILayout.PropertyField(_radiusProp);
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
