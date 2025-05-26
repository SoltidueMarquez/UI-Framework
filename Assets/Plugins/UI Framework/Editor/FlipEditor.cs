using UI_Framework.Scripts.Tools;
using UnityEditor;
using UnityEngine;

namespace UI_Framework.Editor
{
#if UNITY_EDITOR    // 自定义编辑器界面
    [CustomEditor(typeof(PageFlip))]
    public class FlipEditor : UnityEditor.Editor
    {
        // 是否启用场景编辑工具
        bool enableEditorUI;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();  // 绘制默认Inspector
            EditorGUILayout.Separator();
            // 切换场景工具开关
            enableEditorUI = GUILayout.Toggle(enableEditorUI, "是否启用场景编辑工具FlipEditor");
        }

        // 场景视图中的操作
        void OnSceneGUI()
        {
            if (enableEditorUI == false)
                return;

            PageFlip pageFlip = (PageFlip)target;
            Transform transform = pageFlip.transform;
            
            // 获取圆柱轴心和方向的世界坐标
            Vector3 positionWS = pageFlip.GetCylinderPositionWs();
            Vector3 boundaryWS = transform.TransformVector(pageFlip.direction.normalized * pageFlip.radius);

            // 创建可拖拽的手柄
            Vector3 newPositionWS = Handles.PositionHandle(positionWS, transform.rotation);
            Vector3 newBoundaryWS = Handles.PositionHandle(newPositionWS + boundaryWS, transform.rotation) - newPositionWS;

            // 检测手柄变化并更新参数
            if (positionWS != newPositionWS || boundaryWS != newBoundaryWS)
            {
                Undo.RecordObject(pageFlip, "Flip");
                pageFlip.SetCylinderPositionWs(newPositionWS);
                Vector3 newBoundaryCS = transform.InverseTransformVector(newBoundaryWS);
                pageFlip.direction = newBoundaryCS;
                pageFlip.radius = newBoundaryCS.magnitude;
                pageFlip.SetVerticesDirty();
            }
        }
    }
#endif
}