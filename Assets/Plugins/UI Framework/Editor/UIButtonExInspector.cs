using Plugins.UI_Framework.Scripts.Tools;
using UnityEditor;
using UnityEditor.UI;

namespace Core.Editor
{
    [CustomEditor(typeof(UIButtonEx))]
    [CanEditMultipleObjects]
    public class UIButtonExInspector : SelectableEditor
    {
        private SerializedProperty _autoUpProperty;
        private SerializedProperty _clickSoundProperty;
        private SerializedProperty _highLightSoundProperty;
        private SerializedProperty _cantClickProperty;
        private SerializedProperty _onRightClickProperty;
        private SerializedProperty _onClickProperty;
        private SerializedProperty _onPointerEnterProperty;
        private SerializedProperty _onPointerExitProperty;
        private SerializedProperty _onDeselectProperty;
        private SerializedProperty _onSelectProperty;
        private SerializedProperty _onSubmitProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _autoUpProperty = serializedObject.FindProperty("autoUp");
            _clickSoundProperty = serializedObject.FindProperty("clickSound");
            _highLightSoundProperty = serializedObject.FindProperty("highLightSound");
            _cantClickProperty = serializedObject.FindProperty("cantClick");
            _onClickProperty = serializedObject.FindProperty("m_OnClick");
            _onPointerEnterProperty = serializedObject.FindProperty("onPointerEnter");
            _onPointerExitProperty = serializedObject.FindProperty("onPointerExit");
            _onRightClickProperty = serializedObject.FindProperty("onRightClick");
            _onDeselectProperty = serializedObject.FindProperty("onDeselect");
            _onSelectProperty = serializedObject.FindProperty("onSelect");
            _onSubmitProperty = serializedObject.FindProperty("onSubmit");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_autoUpProperty);
            EditorGUILayout.PropertyField(_clickSoundProperty);
            EditorGUILayout.PropertyField(_highLightSoundProperty);
            EditorGUILayout.PropertyField(_cantClickProperty);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_onClickProperty);
            EditorGUILayout.PropertyField(_onPointerEnterProperty);
            EditorGUILayout.PropertyField(_onPointerExitProperty);
            EditorGUILayout.PropertyField(_onRightClickProperty);
            EditorGUILayout.PropertyField(_onDeselectProperty);
            EditorGUILayout.PropertyField(_onSelectProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}