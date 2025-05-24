using UI_Framework.Scripts.Tools;
using UnityEditor;
using UnityEditor.UI;

namespace UI_Framework.Editor
{
    [CustomEditor(typeof(PolygonUI))]
    public class PolygonEditor : ImageEditor
    {
        private PolygonUI m_PolygonUI;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_PolygonUI = (PolygonUI)target;

            // 找到变量X
            SerializedProperty sides = serializedObject.FindProperty("sides");

            // 更新Inspector面板
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), sides);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}