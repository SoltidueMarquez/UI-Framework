using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Tools
{
    /// <summary>
    /// 基本思路是在物体空间下构建一个圆柱空间，然后将模型绕着贴到圆柱上。
    /// 在圆柱空间下实现卷起效果是非常简单，然后再将卷起后的顶点转回到物体空间即可。
    /// </summary>
    public class PageFlip : BaseMeshEffect
    {
        [Tooltip("细分次数，值越高效果越平滑，但性能开销越大"), SerializeField] private int tess = 5;
        [Tooltip("圆柱轴心点的本地坐标原点")] public Vector2 origin = new Vector3(0.5f, -0.5f);
        [Tooltip("翻页方向，决定圆柱的轴向")] public Vector2 direction = new Vector3(1f, -1f);
        [Tooltip("圆柱半径，控制翻页弯曲程度")] public float radius = 50;
        [Tooltip("弯曲起始点的深度")] public float depth;
        [Tooltip("是否持续卷起"), SerializeField] public bool rollup;
        [Tooltip("启用三角形排序以解决Overlay模式渲染顺序问题。")] public bool sort;

        [Header("报错可以试着点击一下isMeshDirty")]
        [SerializeField] bool isMeshDirty = true;
        
        // 顶点和索引缓存列表
        private List<UIVertex> m_TempVertices = new List<UIVertex>(); // 临时顶点缓存
        private List<UIVertex> m_Vertices = new List<UIVertex>();     // 最终顶点列表
        private List<int> m_Indices = new List<int>();                // 三角形索引列表
        private List<Triangle> m_TriangleInfos = new List<Triangle>();// 三角形信息列表（用于排序）

        /// <summary>
        /// 初始化缓存列表
        /// </summary>
        protected override void Start()
        {
            base.Start();
            m_TempVertices = new List<UIVertex>();
            m_Vertices = new List<UIVertex>();
            m_Indices = new List<int>();
            m_TriangleInfos = new List<Triangle>();
            isMeshDirty = true;
        }

        /// <summary>
        /// 计算圆柱轴心在世界空间的位置
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCylinderPositionWs()
        {
            RectTransform transform = graphic.rectTransform;
            // 将origin从本地坐标转换为世界坐标，并沿Z轴偏移（radius - depth）
            return transform.TransformPoint((Vector3)(origin * transform.sizeDelta) + Vector3.back * (radius - depth));
        }

        /// <summary>
        /// 设置圆柱轴心的世界坐标（反向转换到本地坐标）
        /// </summary>
        /// <param name="pos"></param>
        public void SetCylinderPositionWs(Vector3 pos)
        {
            RectTransform transform = graphic.rectTransform;
            Vector2 positionLS = transform.InverseTransformPoint(pos);
            // 将本地坐标转换为0-1范围的origin值
            origin.x = positionLS.x / transform.sizeDelta.x;
            origin.y = positionLS.y / transform.sizeDelta.y;
        }

        /// <summary>
        /// 顶点插值：在两个顶点之间生成中点
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static UIVertex Lerp(UIVertex a, UIVertex b)
        {
            UIVertex c;
            c.position = Vector3.Lerp(a.position, b.position, 0.5f);  // 位置插值
            c.normal = Vector3.Lerp(a.normal, b.normal, 0.5f);        // 法线插值
            c.tangent = Vector4.Lerp(a.tangent, b.tangent, 0.5f);    // 切线插值
            c.uv0 = Vector4.Lerp(a.uv0, b.uv0, 0.5f);                // 纹理坐标插值
            c.uv1 = Vector4.Lerp(a.uv1, b.uv1, 0.5f);
            c.uv2 = Vector4.Lerp(a.uv2, b.uv2, 0.5f);
            c.uv3 = Vector4.Lerp(a.uv3, b.uv3, 0.5f);
            c.color = Color.Lerp(a.color, b.color, 0.5f);            // 颜色插值
            return c;
        }

        /// <summary>
        /// 将一个三角形拆成四个更小的三角形
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="output"></param>
        void Tessellate(UIVertex a, UIVertex b, UIVertex c, List<UIVertex> output)
        {
            // 生成三个边的中点
            UIVertex d = Lerp(a, b);
            UIVertex e = Lerp(b, c);
            UIVertex f = Lerp(c, a);

            // 添加四个新的三角形（共12个顶点）
            output.Add(a); output.Add(d); output.Add(f);  // 三角形ADF
            output.Add(d); output.Add(b); output.Add(e); // 三角形DBE
            output.Add(f); output.Add(e); output.Add(c); // 三角形FEC
            output.Add(f); output.Add(d); output.Add(e); // 三角形FDE
        }

        /// <summary>
        /// 合并重复顶点并重建索引
        /// </summary>
        /// <param name="vertexList"></param>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        private static void Merge(List<UIVertex> vertexList, ref List<UIVertex> vertices, ref List<int> indices)
        {
            Dictionary<UIVertex, int> vertexToIndex = new Dictionary<UIVertex, int>();
            // 局部函数：添加顶点或返回已有索引
            int AddOrGetVertex(UIVertex vertex)
            {
                if (vertexToIndex.TryGetValue(vertex, out int index))
                    return index;
                vertexToIndex.Add(vertex, vertexToIndex.Count);
                return vertexToIndex.Count - 1;
            }

            indices.Clear();
            // 遍历所有三角形，重新映射索引
            for (int i = 0; i < vertexList.Count; i += 3)
            {
                int i0 = AddOrGetVertex(vertexList[i]);
                int i1 = AddOrGetVertex(vertexList[i + 1]);
                int i2 = AddOrGetVertex(vertexList[i + 2]);
                indices.Add(i0); indices.Add(i1); indices.Add(i2);
            }

            // 更新顶点列表为去重后的数据
            vertices.Clear();
            vertices.AddRange(vertexToIndex.Keys);
        }

        /// <summary>
        /// 核心方法：修改网格数据以实现翻页效果
        /// </summary>
        /// <param name="vh"></param>
        public override void ModifyMesh(VertexHelper vh)
        {
            RectTransform graphicRectTransform = graphic.rectTransform;
            
            // 仅在参数变化时重新生成细分网格
            if (isMeshDirty)
            {
                vh.GetUIVertexStream(m_TempVertices);  // 从VertexHelper获取原始顶点数据
                
                #region 执行Tess次细分网格，使有足够的顶点可以细分
                for (int i = 0; i < tess; i++)
                {
                    m_Vertices.Clear();
                    for (int vi = 0; vi < m_TempVertices.Count; vi += 3)
                    {
                        Tessellate(
                            m_TempVertices[vi],
                            m_TempVertices[vi + 1],
                            m_TempVertices[vi + 2],
                            m_Vertices
                        );
                    }
                    // 更新临时顶点列表（最后一次迭代不更新）
                    if (i != tess - 1)
                    {
                        m_TempVertices.Clear();
                        m_TempVertices.AddRange(m_Vertices);
                    }
                }
                #endregion

                // 合并重复顶点并重建索引
                Merge(m_Vertices, ref m_TempVertices, ref m_Indices);
                m_Vertices.Clear();
                m_Vertices.AddRange(m_TempVertices);
                
                // 标记网格已更新
                isMeshDirty = false;
            }

            #region 通过柱面映射偏移顶点来实现翻页效果
            // 性能分析标记：顶点偏移计算
            Profiler.BeginSample("顶点偏移");
            // 构建圆柱坐标系变换矩阵,计算圆柱空间信息（该圆柱是基于物体空间的，以便和网格顶点兼容）
            Vector3 position = graphicRectTransform.InverseTransformPoint(GetCylinderPositionWs());
            Vector3 forward = Vector3.forward;
            Vector3 right = direction.normalized;           // 圆柱右方向（翻页方向）
            Vector3 up = Vector3.Cross(forward, right);      // 圆柱上方向（正交向量）
            Matrix4x4 cylinder = new Matrix4x4(right, up, forward, new Vector4(position.x, position.y, position.z, 1));
            
            // 遍历所有顶点进行圆柱映射
            for (int i = 0; i < m_Vertices.Count; i++)
            {
                UIVertex MapVertex(UIVertex vertex)
                {
                    Vector3 cylinderToVertex = vertex.position - position;
                    float length = Vector3.Dot(cylinderToVertex, right); // 通过投影获得顶点离柱面起点的长度，该长度之后将被绕在圆柱上，故其表示弧长
                    if (length < 0) // 左侧的书面不需要翻动
                        return vertex;

                    float rad = length / radius; // 将弧长转为夹角弧度
                    float height = Vector3.Dot(cylinderToVertex, up); //通过投影获得顶点在圆柱上的高

                    // 计算顶点被环绕贴到柱面后的新位置（圆柱空间位置）
                    Vector3 positionCs;
                    if (rollup || rad < Mathf.PI)
                    {
                        positionCs = new Vector3(
                            Mathf.Sin(rad) * radius,
                            height,
                            Mathf.Cos(rad) * radius
                        );
                    }
                    else // 如果不需要持续将顶点绕在圆柱上，可以在绕到180度时，使后续顶点直接向后伸展而不是继续环绕
                    {
                        positionCs = new Vector3(
                            Mathf.Sin(Mathf.PI) * radius - (length - Mathf.PI * radius),
                            height,
                            Mathf.Cos(Mathf.PI) * radius
                        );
                    }

                    // 将圆柱坐标系下的顶点转到物体坐标系
                    vertex.position = cylinder.MultiplyPoint(positionCs);

                    return vertex;
                }

                m_Vertices[i] = MapVertex(m_TempVertices[i]); // 计算结果不能累加，故始终用最原始的顶点计算
            }
            Profiler.EndSample();
            #endregion

            #region 基元（三角面）顺序是会影响渲染顺序的，所以可以根据顶点被卷起后的深度值排序基元，来实现不依靠深度测试的遮挡效果。
            // 这对Overly模式下的画布非常有用，因为该模式下无法使用深度测试
            if (sort)
            {
                Profiler.BeginSample("三角面排序");
                // 统计三角形信息
                m_TriangleInfos.Clear();
                for (int i = 0; i < m_Indices.Count; i += 3)
                    m_TriangleInfos.Add(
                        new Triangle(m_Vertices, new Vector3Int(m_Indices[i], m_Indices[i + 1], m_Indices[i + 2])));
                // 排序
                m_TriangleInfos.Sort();
                // 写回新的三角形顺序
                m_Indices.Clear();
                foreach (Triangle triangle in m_TriangleInfos)
                {
                    m_Indices.Add(triangle.indices.x);
                    m_Indices.Add(triangle.indices.y);
                    m_Indices.Add(triangle.indices.z);
                }

                Profiler.EndSample();
            }
            #endregion
            
            // 处理因深度偏移导致书面凹陷的情况，这些顶点应该是未被卷起状态
            Profiler.BeginSample("消除顶点凹陷");
            for (int i = 0; i < m_Vertices.Count; i++)
            {
                UIVertex vertex = m_Vertices[i];
                vertex.position.z = Mathf.Min(vertex.position.z, 0);// 限制Z轴不超过0
                m_Vertices[i] = vertex;
            }

            Profiler.EndSample();

            // 将修改后的顶点数据写回VertexHelper
            vh.Clear();
            vh.AddUIVertexStream(m_Vertices, m_Indices);
        }
        
        /// <summary>
        /// 在场景视图中绘制Gizmos（圆柱位置和方向）
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Vector3 positionWS = GetCylinderPositionWs();
            Vector3 boundaryWS = transform.TransformVector(direction.normalized * radius);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(positionWS, boundaryWS);        // 绘制圆柱方向
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(positionWS, boundaryWS.magnitude);    // 绘制圆柱半径
        }

        /// <summary>
        /// 设置图形顶点脏数据
        /// </summary>
        public void SetVerticesDirty()
        {
            graphic.SetVerticesDirty();
        }
    }
    
    
    /// <summary>
    /// 存储三角形深度信息用于排序
    /// </summary>
    public struct Triangle : IComparable<Triangle>
    {
        public Triangle(List<UIVertex> vertices, Vector3Int indices)
        {
            // 计算三角形中心深度（三个顶点Z坐标平均值）
            depth = vertices[indices.x].position.z +
                    vertices[indices.y].position.z +
                    vertices[indices.z].position.z;
            this.indices = indices; // 存储三角形顶点的索引
        }

        private float depth;
        public Vector3Int indices;

        // 按深度降序排序（确保远处三角形先绘制）
        public int CompareTo(Triangle other)
        {
            return -depth.CompareTo(other.depth);
        }
    }
}