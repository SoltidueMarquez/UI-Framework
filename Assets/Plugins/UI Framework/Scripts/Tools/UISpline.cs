using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI
{
    /// <summary>
    /// 直接继承UGUI脚本计算网格实现UI效果，最好把它放在(0,0,0)的位置
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UISpline : Graphic
    {
        [Header("UI Spline")] public int resolution = 10;
        public float handleStrength = 1f;
        public Sprite splineSprite;

        [Range(0f, 1f)] public float handlePosition = 0.5f;
        public float width = 1f;
        public AnimationCurve widthMapping;
        public Vector2 textureScale = new Vector2(1f, 1f);

        [Header("Pointer")] public bool enablePointer;
        public Image pointerImg;
        private Vector3 m_Start;
        private Vector3 m_End;
        private Vector3 m_Handle;
        private bool m_ResetHandle = false;
        private float m_SingleSegmentTextureUV;

        public override Texture mainTexture => splineSprite.texture;

        // 生成网格
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            MakeSpline(vh);
            material.color = color;
        }

        /// <summary>
        /// 实际要调用的绘制指示器的函数,注意这边的指示器箭头只有在overlay下显示正常
        /// </summary>
        /// <param name="s">距离自身位置的偏移量，起点</param>
        /// <param name="e">距离自身位置的偏移量，终点</param>
        public void UpdateUISpline(Vector3 s, Vector3 e)
        {
            if (s == m_Start && e == m_End) return;
            m_Start = s;
            m_End = e;
            //通过起始点和结束点计算出中间的控制点
            Vector3 vec = m_End - m_Start;
            Vector3 mid = m_Start + vec * handlePosition;
            Vector3 normal = new Vector3(-vec.y, vec.x, 0).normalized;
            if (m_End.x < m_Start.x)
            {
                normal = -normal;
            }

            Vector3 temp = mid + normal * Mathf.Sqrt(vec.sqrMagnitude) * handleStrength;
            if (m_ResetHandle)
            {
                m_ResetHandle = false;
                m_Handle = mid;
            }

            SetVerticesDirty();
            
            m_Handle = Vector3.Lerp(m_Handle, temp, Time.deltaTime * 20f);
            if (enablePointer)
            {
                pointerImg.gameObject.SetActive(true);
                pointerImg.transform.position = m_End + transform.position;
                pointerImg.transform.up = (m_End - temp).normalized;
            }
        }

        private void MakeSpline(VertexHelper vh)
        {
            var points = CalculateBezierPoints(m_Start, m_End, m_Handle, resolution);
            m_SingleSegmentTextureUV = Vector3.Distance(m_Start, m_End) / Screen.height;
            for (int i = 0; i < points.Count - 1; i++)
            {
                bool isLast = (i == points.Count - 2);
                float newWidth = widthMapping.Evaluate(i / (float)(points.Count - 1)) * width;
                AddSegment(vh, i, points[i], points[i + 1], points[i + (isLast ? 1 : 2)], newWidth, isLast);
            }
        }

        private void AddVert(VertexHelper vh, Vector3 position, Vector4 uv)
        {
            UIVertex uiVertex = UIVertex.simpleVert;
            uiVertex.color = color;
            uiVertex.position = position;
            uiVertex.uv0 = uv;
            vh.AddVert(uiVertex);
        }

        // 添加一个线段
        private void AddSegment(VertexHelper vh, int i, Vector3 start, Vector3 end, Vector3 next, float width,
            bool isLast)
        {
            // 根据起始点和结束点计算出四个顶点
            // 为了衔接后续的线段，使用结束点与下一个点的方向来计算法线
            Vector3 directionl = (end - start).normalized;
            Vector3 direction2 = (next - end).normalized;
            Vector3 normal1 = new Vector3(-directionl.y, directionl.x, 0);
            Vector3 normal2 = new Vector3(-direction2.y, direction2.x, 0);
            if (isLast)
            {
                normal2 = normal1;
            }
            Vector3 p1 = start + normal1 * width / 2;
            Vector3 p2 = start - normal1 * width / 2;
            Vector3 p3 = end + normal2 * width / 2;
            Vector3 p4 = end - normal2 * width / 2;
            //添加顶点
            AddVert(vh, p1, new Vector4(i * m_SingleSegmentTextureUV * textureScale.x, 1f));
            AddVert(vh, p2, new Vector4(i * m_SingleSegmentTextureUV * textureScale.x, 0f));
            AddVert(vh, p3, new Vector4((i + 1) * m_SingleSegmentTextureUV * textureScale.x, 1f));
            AddVert(vh, p4, new Vector4((i + 1) * m_SingleSegmentTextureUV * textureScale.x, 0f));
            //添加三角形
            int startIndex = vh.currentVertCount - 4;
            vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
            vh.AddTriangle(startIndex, startIndex + 3, startIndex + 1);
        }

        // 计算二次贝塞尔曲线上的点
        private List<Vector3> CalculateBezierPoints(Vector3 start, Vector3 end, Vector3 handle, int resolution)
        {
            if (resolution < 1) resolution = 1;
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 point = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * handle + t * t * end;
                points.Add(point);
            }

            return points;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ResetHandle = true;
            // sprite = splineSprite;
        }
    }
}