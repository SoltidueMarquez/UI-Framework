using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Tools
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class PolygonUI : Image
    {
        [Tooltip("边数"), Range(3, 128)] public int sides = 4; // 默认边数为4，表示四边形

        private Vector2 center;
        private float radius;
        List<UIVertex> m_vVertices = new List<UIVertex>();
        List<int> m_vIndices = new List<int>();

        // 核心函数
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            m_vVertices.Clear();
            m_vIndices.Clear();

            var rect = GetPixelAdjustedRect(); //获取当前ui的矩形
            //计算多边形的中心
            center = Vector2.zero; // 根据需要调整中心位置
            radius = rect.width / 2; // 根据需要调整半径
            //计算每条边之间的弧度差
            float angleStep = 360f / sides;

            // 存储顶点

            // 添加顶点

            // 中心圆点
            m_vVertices.Add(new UIVertex
            {
                position = new Vector3(0, 0),
                color = color,
                uv0 = new Vector2(0.5f, 0.5f) // 根据纹理调整uv
            });

            for (int i = 1; i <= sides; ++i) // 计算每个顶点的坐标和uv坐标，颜色
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius + center.x;
                float y = Mathf.Sin(angle) * radius + center.y;
                // print($"index:{i},x:{x),y:{y}")
                m_vVertices.Add(new UIVertex
                {
                    position = new Vector3(x, y),
                    color = color,
                    uv0 = new Vector2(x / radius + 0.5f, y / radius + 0.5f) // 根据纹理调整uv
                });
            }
            
            // 添加三角形
            for (int i = 1; i < sides; ++i)
            {
                m_vIndices.Add(0); // 逆时针添加顶点顺序
                m_vIndices.Add(i + 1);
                m_vIndices.Add(i);
            }
            // 封闭多边形
            m_vIndices.Add(0);
            m_vIndices.Add(1);
            m_vIndices.Add(sides);
            
            // 将顶点添加到VertexHelper
            vh.AddUIVertexStream(m_vVertices, m_vIndices);
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            return base.Raycast(sp, eventCamera);
        }

        private void OnMouseDown()
        {
            print("OnMouseDown");
        }
    }
}