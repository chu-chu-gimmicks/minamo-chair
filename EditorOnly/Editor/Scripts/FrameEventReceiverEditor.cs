using ChuChuGimmicks.MinamoChair.Runtime;
using UdonSharpEditor;
using UnityEditor;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    [CustomEditor(typeof(FrameEventReceiver))]
    public class FrameEventReceiverEditor : Editor
    {
        private SerializedProperty _managerProp;

        private bool _isOpen = false;




        private void OnEnable()
        {
            _managerProp = serializedObject.FindProperty(FrameEventReceiver.FieldNames.Manager);
        }


        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) { return; }

            EditorGUI.BeginDisabledGroup(true);
            _isOpen = EditorGUILayout.Foldout(_isOpen, "References", true);
            if (_isOpen)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_managerProp);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
