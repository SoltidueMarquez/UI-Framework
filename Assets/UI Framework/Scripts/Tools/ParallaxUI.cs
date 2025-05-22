using UnityEngine;

namespace UI_Framework.Scripts.Tools
{
    /// <summary>
    /// UI视差效果控制器（支持所有Canvas渲染模式）
    /// </summary>
    /// <remarks>
    /// 功能特性：
    /// 1. 自动适配ScreenSpace-Overlay/Camera/WorldSpace三种模式
    /// 2. 基于物理的平滑插值移动
    /// 3. 动态坐标转换系统
    /// 4. 自动组件依赖解析
    /// </remarks>
    public class ParallaxUI : MonoBehaviour
    {
        [Tooltip("视差偏移强度系数（建议值0.1-2）")] public float offsetMultiplier = 1f;

        [Tooltip("移动平滑时间（秒）")] public float smoothTime = 0.3f;

        // 运行时状态
        [Tooltip("初始基准位置")] private Vector3 m_StartPosition;
        [Tooltip("平滑速度缓存")] private Vector3 m_Velocity;

        // 自动获取的组件引用 
        private Canvas m_TargetCanvas; // 所属Canvas组件
        private Camera m_TargetCamera; // 主摄像机引用

        // WorldSpace模式专用参数
        private RectTransform m_CanvasRectTransform; // Canvas的RectTransform
        private RectTransform m_SelfRectTransform; // 自身的RectTransform
        private Vector2 m_CanvasCenterOffset; // Canvas中心点偏移量

        private void Start()
        {
            // 组件自动获取
            m_TargetCanvas = GetComponentInParent<Canvas>();
            m_SelfRectTransform = GetComponent<RectTransform>();

            if (m_TargetCanvas == null) return;

            // 初始化
            switch (m_TargetCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    // Overlay模式：直接记录初始锚点位置
                    m_StartPosition = m_SelfRectTransform.anchoredPosition;
                    break;

                case RenderMode.ScreenSpaceCamera:
                    // ScreenCamera模式：
                    // 1. 优先使用Canvas指定的摄像机
                    // 2. 使用世界坐标系基准位置
                    m_TargetCamera = m_TargetCanvas.worldCamera ? m_TargetCanvas.worldCamera : Camera.main;
                    m_StartPosition = transform.position;
                    break;

                case RenderMode.WorldSpace:
                    // WorldSpace模式特殊初始化：
                    // 1. 获取Canvas的RectTransform用于坐标转换
                    // 2. 计算中心点偏移量（考虑Pivot的影响）
                    // 3. 使用本地坐标系基准位置
                    m_CanvasRectTransform = m_TargetCanvas.GetComponent<RectTransform>();
                    m_TargetCamera = m_TargetCanvas.worldCamera ? m_TargetCanvas.worldCamera : Camera.main;
                    m_StartPosition = m_SelfRectTransform.localPosition;

                    // 计算Canvas中心点补偿值：
                    // 公式 = Canvas尺寸 * Pivot(中心点)位置
                    // 示例：当Pivot为(0.5,0.5)时，中心点偏移为尺寸的一半
                    Vector2 canvasSize = m_CanvasRectTransform.rect.size;
                    m_CanvasCenterOffset = new Vector2(
                        canvasSize.x * m_CanvasRectTransform.pivot.x,
                        canvasSize.y * m_CanvasRectTransform.pivot.y
                    );
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (m_TargetCanvas == null) return;

            // 视差偏移计算
            Vector2 offset = Vector2.zero;
            Vector2 mousePosition = Input.mousePosition;

            switch (m_TargetCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    // Overlay模式偏移计算：
                    // 1. 将鼠标坐标标准化到[0,1]范围
                    // 2. 转换到[-1,1]区间
                    // 3. 示例：屏幕右下角(1920,1080) -> (1,1)
                    Vector2 screenSize = m_TargetCanvas.pixelRect.size;
                    Vector2 normalizedPosition = mousePosition / screenSize;
                    offset = (normalizedPosition - new Vector2(0.5f, 0.5f)) * 2f;
                    break;

                case RenderMode.ScreenSpaceCamera:
                    // ScreenCamera模式偏移计算：
                    // 使用摄像机的视口坐标（ViewportPoint）
                    // 将屏幕坐标转换到摄像机视角的[0,1]范围
                    if (m_TargetCamera == null) return;
                    Vector3 viewportPos = m_TargetCamera.ScreenToViewportPoint(mousePosition);
                    offset = (viewportPos - new Vector3(0.5f, 0.5f)) * 2f;
                    break;

                case RenderMode.WorldSpace:
                    // WorldSpace模式核心算法：
                    // 1. 将屏幕坐标转换为Canvas本地坐标
                    // 2. 调整中心点偏移
                    // 3. 标准化到[-1,1]范围
                    if (m_TargetCamera == null || m_CanvasRectTransform == null) return;

                    // 屏幕坐标->Canvas本地坐标转换
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            m_CanvasRectTransform,
                            mousePosition,
                            m_TargetCamera,
                            out Vector2 localPoint))
                    {
                        // 中心点补偿计算：
                        // 转换后的本地坐标减去预计算的偏移量
                        // 示例：当Canvas宽1000，Pivot在中心时：
                        // localPoint(500,0) -> centeredPoint(0, -500)
                        Vector2 centeredPoint = localPoint - m_CanvasCenterOffset;

                        // 标准化处理：
                        // 将坐标值按Canvas尺寸等比例缩放
                        // 公式：坐标值 / (尺寸 * 0.5f)
                        // 结果范围：[-1, 1]
                        Vector2 canvasSize = m_CanvasRectTransform.rect.size;
                        offset = new Vector2(
                            centeredPoint.x / (canvasSize.x * 0.5f),
                            centeredPoint.y / (canvasSize.y * 0.5f)
                        );
                    }

                    break;
            }

            // 平滑移动应用
            Vector3 targetPosition = m_StartPosition + (Vector3)(offset * offsetMultiplier);

            switch (m_TargetCanvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    // Overlay模式使用锚点位置移动：
                    // 保持UI元素在Canvas内的相对定位
                    m_SelfRectTransform.anchoredPosition = Vector3.SmoothDamp(
                        m_SelfRectTransform.anchoredPosition,
                        targetPosition,
                        ref m_Velocity,
                        smoothTime);
                    break;

                case RenderMode.WorldSpace:
                    // WorldSpace模式使用本地坐标移动：
                    // 保持与父物体的相对位置关系
                    m_SelfRectTransform.localPosition = Vector3.SmoothDamp(
                        m_SelfRectTransform.localPosition,
                        targetPosition,
                        ref m_Velocity,
                        smoothTime);
                    break;

                default:
                    // 其他模式使用世界坐标移动：
                    // 适用于ScreenCamera等需要绝对定位的情况
                    transform.position = Vector3.SmoothDamp(
                        transform.position,
                        targetPosition,
                        ref m_Velocity,
                        smoothTime);
                    break;
            }
        }
    }
}