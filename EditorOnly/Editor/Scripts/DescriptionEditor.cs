using UnityEditor;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    [CustomEditor(typeof(Description))]
    public class DescriptionEditor : Editor
    {
        private SerializedProperty _descriptionProp;




        private void OnEnable()
        {
            _descriptionProp = serializedObject.FindProperty(Description.FieldName.Description);
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(_descriptionProp.stringValue, MessageType.Info);
        }
    }
}
