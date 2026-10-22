using ChuChuGimmicks.MinamoChair.Runtime;
using UnityEditor;
using UnityEngine;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    [CustomEditor(typeof(Setup))]
    [CanEditMultipleObjects]
    public class SetupEditor : Editor
    {
        private const float MIN_LIMIT = -1f;
        private const float MAX_LIMIT = 3f;

        private SerializedProperty _drawGizmosProp;

        private SerializedProperty _autoAdjustmentMinHeightProp;
        private SerializedProperty _autoAdjustmentMaxHeightProp;

        private SerializedProperty _manualAdjustmentMinHeightProp;
        private SerializedProperty _manualAdjustmentMaxHeightProp;

        private SerializedProperty _modelAdjustmentModeProp;
        private SerializedProperty _modelSurfaceHeightProp;

        private SerializedProperty _managerProp;

        private bool _isOpen = false;




        private void OnEnable()
        {
            _drawGizmosProp = serializedObject.FindProperty(Setup.FieldNames.DrawGizmos);

            _autoAdjustmentMinHeightProp = serializedObject.FindProperty(Setup.FieldNames.AutoAdjustmentMinHeight);
            _autoAdjustmentMaxHeightProp = serializedObject.FindProperty(Setup.FieldNames.AutoAdjustmentMaxHeight);

            _manualAdjustmentMinHeightProp = serializedObject.FindProperty(Setup.FieldNames.ManualAdjustmentMinHeight);
            _manualAdjustmentMaxHeightProp = serializedObject.FindProperty(Setup.FieldNames.ManualAdjustmentMaxHeight);

            _modelAdjustmentModeProp = serializedObject.FindProperty(Setup.FieldNames.ModelAdjustmentMode);
            _modelSurfaceHeightProp = serializedObject.FindProperty(Setup.FieldNames.ModelSurfaceHeight);

            _managerProp = serializedObject.FindProperty(Setup.FieldNames.Manager);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox(
                "Gizmos の表示切り替え\n" +
                "エディタの Gizmos が有効な場合、セットアップ用ガイドが表示されます。\n" +
                "\n" +
                "Toggle Gizmo Visibility\n" +
                "Setup guides are displayed when Gizmos are enabled in the editor.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(_drawGizmosProp);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.HelpBox(
                "自動調整時の高さ範囲\n" +
                "プレイヤーの視点の高さは緑の Cube の範囲内に収まります。\n" +
                "\n" +
                "Automatic Height Adjustment Range\n" +
                "The player's eye height stays within the range indicated by the green cube.",
                MessageType.Info
            );
            DrawFloatRange(_autoAdjustmentMinHeightProp, _autoAdjustmentMaxHeightProp, MIN_LIMIT, MAX_LIMIT);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);


            EditorGUILayout.HelpBox(
                "手動調整時の高さ範囲\n" +
                "プレイヤーの視点の高さは赤い枠線の Cube の範囲内に収まります。\n" +
                "\n" +
                "Manual Height Adjustment Range\n" +
                "The player's eye height stays within the range indicated by the red wireframe cube.",
                MessageType.Info
            );
            DrawFloatRange(_manualAdjustmentMinHeightProp, _manualAdjustmentMaxHeightProp, MIN_LIMIT, MAX_LIMIT);
            _manualAdjustmentMinHeightProp.floatValue = Mathf.Min(_autoAdjustmentMinHeightProp.floatValue, _manualAdjustmentMinHeightProp.floatValue);
            _manualAdjustmentMaxHeightProp.floatValue = Mathf.Max(_autoAdjustmentMaxHeightProp.floatValue, _manualAdjustmentMaxHeightProp.floatValue);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.HelpBox(
                "モデルの動作設定\n" +
                "自動/手動調整に合わせて、モデルがどのように動くか決定します。\n" +
                "\n" +
                "Model Behavior Settings\n" +
                "Choose how the model responds to automatic and manual height adjustments.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(_modelAdjustmentModeProp);
            if (_modelAdjustmentModeProp.intValue == (int)ModelAdjustmentMode.HeightAndYaw)
            {
                EditorGUILayout.HelpBox(
                    " - 自動高さ調整時: 高さはアバターに合わせて自動調整、向きは連動\n" +
                    " - 手動高さ調整時: 高さ/向きともに連動\n" +
                    "青い Plane をモデルの座面の高さに合わせてください。\n" +
                    "\n" +
                    " - Automatic Height Adjustment: Height adjusts automatically to fit the avatar; facing direction follows\n" +
                    " - Manual Height Adjustment: Both height and facing direction follow\n" +
                    "Set the blue plane to the height of the model's seat surface.",
                    MessageType.Info
                );
                EditorGUILayout.Slider(_modelSurfaceHeightProp, MIN_LIMIT, MAX_LIMIT);
            }
            else if (_modelAdjustmentModeProp.intValue == (int)ModelAdjustmentMode.YawOnly)
            {
                EditorGUILayout.HelpBox(
                    " - 自動高さ調整時: 向きのみ連動\n" +
                    " - 手動高さ調整時: 向きのみ連動\n" +
                    "\n" + 
                    " - Automatic Height Adjustment: Only facing direction follows\n" +
                    " - Manual Height Adjustment: Only facing direction follows",

                    MessageType.Info
                );
            }
            else if (_modelAdjustmentModeProp.intValue == (int)ModelAdjustmentMode.None)
            {
                EditorGUILayout.HelpBox(
                    " - 自動高さ調整時: 高さ/向きともに変化しない\n" +
                    " - 手動高さ調整時: 高さ/向きともに変化しない\n" +
                    "\n" +
                    " - Automatic Height Adjustment: Neither height nor facing direction changes\n" +
                    " - Manual Height Adjustment: Neither height nor facing direction changes",
                    MessageType.Info
                );
            }
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            if (EditorGUI.EndChangeCheck())
            {
                ApplyChange();
            }

            EditorGUI.BeginDisabledGroup(true);
            _isOpen = EditorGUILayout.Foldout(_isOpen, "References", true);
            if (_isOpen)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_managerProp);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawFloatRange(SerializedProperty minProp, SerializedProperty maxProp, float minLimit, float maxLimit)
        {
            float lowerValue = minProp.floatValue;
            float upperValue = maxProp.floatValue;
            EditorGUILayout.MinMaxSlider("Range", ref lowerValue, ref upperValue, minLimit, maxLimit);
            minProp.floatValue = lowerValue;
            maxProp.floatValue = upperValue;

            // UI 上のレイアウト都合で Max → Min の順に表示する
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(maxProp);
            if (EditorGUI.EndChangeCheck())
            {
                float currentValue = maxProp.floatValue;
                float lowerBound = Mathf.Max(minProp.floatValue, minLimit);
                float upperBound = maxLimit;
                maxProp.floatValue = Mathf.Clamp(currentValue, lowerBound, upperBound);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(minProp);
            if (EditorGUI.EndChangeCheck())
            {
                float currentValue = minProp.floatValue;
                float lowerBound = minLimit;
                float upperBound = Mathf.Min(maxProp.floatValue, maxLimit);
                minProp.floatValue = Mathf.Clamp(currentValue, lowerBound, upperBound);
            }
        }


        private void ApplyChange()
        {
            SerializedObject so = new SerializedObject(_managerProp.objectReferenceValue);
            SerializedProperty autoAdjustmentMinHeightProp = so.FindProperty(Manager.FieldNames.AutoAdjustmentMinHeight);
            SerializedProperty autoAdjustmentMaxHeightProp = so.FindProperty(Manager.FieldNames.AutoAdjustmentMaxHeight);
            SerializedProperty manualAdjustmentMinHeightProp = so.FindProperty(Manager.FieldNames.ManualAdjustmentMinHeight);
            SerializedProperty manualAdjustmentMaxHeightProp = so.FindProperty(Manager.FieldNames.ManualAdjustmentMaxHeight);
            SerializedProperty modelAdjustmentModeProp = so.FindProperty(Manager.FieldNames.ModelAdjustmentMode);
            SerializedProperty modelSurfaceHeightProp = so.FindProperty(Manager.FieldNames.ModelSurfaceHeight);

            so.Update();
            autoAdjustmentMinHeightProp.floatValue = _autoAdjustmentMinHeightProp.floatValue;
            autoAdjustmentMaxHeightProp.floatValue = _autoAdjustmentMaxHeightProp.floatValue;
            manualAdjustmentMinHeightProp.floatValue = _manualAdjustmentMinHeightProp.floatValue;
            manualAdjustmentMaxHeightProp.floatValue= _manualAdjustmentMaxHeightProp.floatValue;
            modelAdjustmentModeProp.intValue = _modelAdjustmentModeProp.intValue;
            modelSurfaceHeightProp.floatValue= _modelSurfaceHeightProp.floatValue;
            so.ApplyModifiedProperties();
        }
    }
}
