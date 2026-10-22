using ChuChuGimmicks.MinamoChair.Runtime;
using UdonSharpEditor;
using UnityEditor;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    [CustomEditor(typeof(Manager))]
    public class ManagerEditor : Editor
    {
        SerializedProperty _stationProp;
        SerializedProperty _frameEventReceiver;
        
        SerializedProperty _enterPointProp;
        SerializedProperty _exitPointProp;
        SerializedProperty _modelProp;
        SerializedProperty _guideProp;
        SerializedProperty _guideImageProp;
        SerializedProperty _guideSpritesProp;
        SerializedProperty _disabledOverlayProp;

        SerializedProperty _autoAdjustmentMinHeightProp;
        SerializedProperty _autoAdjustmentMaxHeightProp;

        SerializedProperty _manualAdjustmentMinHeightProp;
        SerializedProperty _manualAdjustmentMaxHeightProp;

        SerializedProperty _modelAdjustmentModeProp;
        SerializedProperty _modelSurfaceHeightProp;

        private bool _isOpen = false;




        private void OnEnable()
        {
            _stationProp = serializedObject.FindProperty(Manager.FieldNames.Station);
            _frameEventReceiver = serializedObject.FindProperty(Manager.FieldNames.FrameEventReceiver);

            _enterPointProp = serializedObject.FindProperty(Manager.FieldNames.EnterPoint);
            _exitPointProp = serializedObject.FindProperty(Manager.FieldNames.ExitPoint);
            _modelProp = serializedObject.FindProperty(Manager.FieldNames.Model);
            _guideProp = serializedObject.FindProperty(Manager.FieldNames.Guide);
            _guideImageProp = serializedObject.FindProperty(Manager.FieldNames.GuideImage);
            _guideSpritesProp = serializedObject.FindProperty(Manager.FieldNames.GuideSprites);
            _disabledOverlayProp = serializedObject.FindProperty(Manager.FieldNames.DisabledOverlay);

            _autoAdjustmentMinHeightProp = serializedObject.FindProperty(Manager.FieldNames.AutoAdjustmentMinHeight);
            _autoAdjustmentMaxHeightProp = serializedObject.FindProperty(Manager.FieldNames.AutoAdjustmentMaxHeight);

            _manualAdjustmentMinHeightProp = serializedObject.FindProperty(Manager.FieldNames.ManualAdjustmentMinHeight);
            _manualAdjustmentMaxHeightProp = serializedObject.FindProperty(Manager.FieldNames.ManualAdjustmentMaxHeight);

            _modelAdjustmentModeProp = serializedObject.FindProperty(Manager.FieldNames.ModelAdjustmentMode);
            _modelSurfaceHeightProp = serializedObject.FindProperty(Manager.FieldNames.ModelSurfaceHeight);
        }


        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) { return; }

            EditorGUI.BeginDisabledGroup(true);
            _isOpen = EditorGUILayout.Foldout(_isOpen, "References", true);
            if (_isOpen)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_stationProp);
                EditorGUILayout.PropertyField(_frameEventReceiver);

                EditorGUILayout.PropertyField(_enterPointProp);
                EditorGUILayout.PropertyField(_exitPointProp);
                EditorGUILayout.PropertyField(_modelProp);
                EditorGUILayout.PropertyField(_guideProp);
                EditorGUILayout.PropertyField(_guideImageProp);
                EditorGUILayout.PropertyField(_guideSpritesProp);
                EditorGUILayout.PropertyField(_disabledOverlayProp);

                EditorGUILayout.PropertyField(_autoAdjustmentMinHeightProp);
                EditorGUILayout.PropertyField(_autoAdjustmentMaxHeightProp);

                EditorGUILayout.PropertyField(_manualAdjustmentMinHeightProp);
                EditorGUILayout.PropertyField(_manualAdjustmentMaxHeightProp);

                EditorGUILayout.PropertyField(_modelAdjustmentModeProp);
                EditorGUILayout.PropertyField(_modelSurfaceHeightProp);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
