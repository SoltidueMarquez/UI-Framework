using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Tools
{
    /// <summary>
    /// 滑动页面效果，和UIList搭配使用更佳
    /// 根据content下子物体的位置和宽度将整个scrollview内容划分成多个区域，
    /// 鼠标没拖动到一定程度坐标就锁死在某个区域内，
    /// 拖动超过阈值isDrug=true直接插值渐变坐标切换到相邻的区域
    /// 注意是isDrug=false时执行切换
    /// </summary>
    public class HorizontalPageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private float speed = 3; // 滑动速度 
        [SerializeField] private bool speedUp; // 是否加速滑动模式
        [SerializeField] private ScrollRect rect; // ScrollRect组件
        
        private float m_TargetHorizontal;
        private readonly List<float> m_PosList = new(); //存图片的位置(0, 0.333, 0.666, 1) 
        private bool m_IsDrag = true; // 是否拖动
        private float m_StartTime;
        private int m_CurIndex;

        public event Action<int, int> OnEndSwitchIndex; // 拖动结束时的切换事件
        public event Action<int> OnStartSwitchIndex;    // 拖动开始时的事件

        // 只是为了演示，需要外部调用的话就把这个Start删掉在外部调用Init即可，注意需要等子物体都生成完再调用
        private void Start()
        {
            Init();
        }

        public void Init()
        {
            InitScrollView();// 计算并设置ScrollView子物体的间距，保证可以合理使用
            InitIndexPos();
        }

        private int GetActiveChildrenNum(Transform targetTransform)
        {
            int activeChildCount = 0;
            for (int i = 0; i < targetTransform.childCount; i++)
            {
                Transform child = targetTransform.GetChild(i);
                if (child.gameObject.activeSelf) activeChildCount++;
            }
            return activeChildCount;
        }
        
        private void InitScrollView()
        {
            // 确保有子物体
            if (GetActiveChildrenNum(rect.content.transform) == 0)
            {
                Debug.LogError("Content没有激活的子物体");
                return;
            }

            // 获取Viewport的宽度
            RectTransform viewportRect = rect.viewport.GetComponent<RectTransform>();
            float viewportWidth = viewportRect.rect.width;

            // 获取第一个子物体的宽度
            RectTransform firstChild = rect.content.transform.GetChild(0).GetComponent<RectTransform>();
            float childWidth = firstChild.rect.width;

            // 计算间距和左边距，确保非负
            float spacing = Mathf.Max(0, viewportWidth / 2 - childWidth);
            float padding = Mathf.Max(0, viewportWidth / 2 - childWidth / 2);

            // 配置HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = rect.content.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = rect.content.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layoutGroup.spacing = spacing;
            layoutGroup.padding = new RectOffset(
                Mathf.RoundToInt(padding),
                Mathf.RoundToInt(padding),
                0, 
                0  
            );
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = false;

            // 强制立即刷新布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect.content.GetComponent<RectTransform>());
        }
        
        private void InitIndexPos()
        {
            var activeChildNum = GetActiveChildrenNum(rect.content.transform);
            for (var i = 0; i < activeChildNum; i++) //添加content下的物体的位置信息到posList里去
            {
                m_PosList.Add(i * (1f / Mathf.Max(activeChildNum - 1, 0.001f))); //存图片位置
            }

            // 直接跳转到上次的章节，可以用PlayerPrefs来记录，这边先默认归0
            m_CurIndex = 0;
            rect.horizontalNormalizedPosition = Mathf.Lerp(rect.horizontalNormalizedPosition, m_PosList[m_CurIndex], 10);
            OnEndSwitchIndex?.Invoke(m_CurIndex, -1);
        }

        private void Update()
        {
            if (!m_IsDrag)
            {
                m_StartTime += Time.deltaTime;
                var t = m_StartTime * speed;
                if (speedUp)
                {
                    // 加速滑动效果
                    rect.horizontalNormalizedPosition = Mathf.Lerp(rect.horizontalNormalizedPosition, m_TargetHorizontal, t);
                }
                else
                {
                    // 缓慢匀速滑动效果
                    rect.horizontalNormalizedPosition = Mathf.Lerp(rect.horizontalNormalizedPosition, m_TargetHorizontal, Time.deltaTime * speed);
                }
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData) //开始拖动
        {
            m_IsDrag = true;
            // 根据UI切换执行事件函数
            OnStartSwitchIndex?.Invoke(m_CurIndex);
        }

        public void OnEndDrag(PointerEventData eventData) //结束拖动
        {
            var posX = rect.horizontalNormalizedPosition;

            //计算_curIndex应该改变到哪一个页面的index
            var index = m_CurIndex;
            var offset = m_PosList[index] - posX;
            if (offset < 0 && m_CurIndex < m_PosList.Count - 1) index++;
            else if (offset > 0 && m_CurIndex > 0) index--;

            // 根据UI切换执行事件函数
            OnEndSwitchIndex?.Invoke(index, m_CurIndex);

            m_CurIndex = index;
            m_TargetHorizontal = m_PosList[m_CurIndex]; //设置当前坐标，更新函数进行插值  
            m_IsDrag = false;
            m_StartTime = 0;
        }
    }
}