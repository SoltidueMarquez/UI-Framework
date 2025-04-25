using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Tools
{
    public class UIList : MonoBehaviour
    {
        public Transform cloneItem;
        public List<Component> items = new();
        [Tooltip("item个数")] public int Count => items.Count;

        public Component this[int key] => GetItem<Component>(key);

        protected void Awake()
        {
            // 如果cloneItem是最外层的prefab那么这个值为null，如果是prefab中的一个gameobject，那么当调用awake的时候，scene已经被设置了值
            if (cloneItem.gameObject.scene.name != null)
            {
                cloneItem.gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            // 动态更新需要克隆的UI的物体
            if (cloneItem == null && transform.childCount > 0)
            {
                cloneItem = transform.GetChild(0);
            }
        }
#endif

        #region 克隆方法

        /// <summary>
        /// 克隆UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T CloneItem<T>() where T : Component
        {
            Transform item = Instantiate(cloneItem, transform); // 生成物体
            item.gameObject.SetActive(true);
            T component = item.GetComponent<T>();
            if (component == null)
            {
                component = item.GetComponentInChildren<T>();
            }

            items.Add(component); // 添加进列表中
            item.gameObject.name += items.Count; // 更改名称
            return component;
        }

        /// <summary>
        /// 克隆UI到指定下标位置
        /// </summary>
        /// <param name="insertIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T CloneItem<T>(int insertIndex) where T : Component
        {
            if (insertIndex < 0 || insertIndex > items.Count)
            {
                Debug.LogError($"insertIndex【{insertIndex}】不在0到items.count范围中！");
                return default;
            }

            T component = CloneItem<T>();
            items.RemoveAt(items.Count - 1);
            items.Insert(insertIndex, component);
            // 调整游戏对象在其父级下的子对象列表中的顺序，将其移动到指定位置的后一位
            // SiblingIndex会计算inactive的物体，第一个是模板物体，所以items中是从1开始
            component.transform.SetSiblingIndex(insertIndex + 1);
            return component;
        }

        #endregion

        #region 寻找与获取

        /// <summary>
        /// 根据对应方法筛查对应的UI
        /// </summary>
        /// <param name="match">筛查方法，Predicate就是返回值为bool的Func</param>
        /// <returns>符合规则的UI的下标</returns>
        public int FindIndex(Predicate<Component> match)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (match(items[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public Component Find(Predicate<Component> match)
        {
            return items.FirstOrDefault(t => match(t));
        }

        public int GetIndex(Component item)
        {
            return items.FindIndex((x) => x == item);
        }

        #endregion

        #region 清除与移除

        public virtual void ClearItems(bool destroy = true, bool delay = false)
        {
            if (destroy)
            {
                foreach (var item in items.Where(item => item != null))
                {
                    if (delay)
                        Destroy(item.gameObject);
                    else
                        DestroyImmediate(item.gameObject);
                }
            }

            // 强制立即重建 UI 布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            items.Clear();
        }
        
        public virtual void RemoveItemAt(int index, bool destroy = true, bool delay = false)
        {
            if (index < 0 || index >= items.Count) return;
            if (cloneItem == null) return;
            if (delay)
            {
                CoroutineHelper.DelayGuiFrames(1, () =>
                {
                    if (destroy) DestroyImmediate(items[index].gameObject);
                    items.RemoveAt(index);
                }); //等一下UI的事件更新再销毁
            }
            else
            {
                if (destroy) DestroyImmediate(items[index].gameObject);
                items.RemoveAt(index);
            }
        }
        
        public void RemoveItem(Component item, bool destroy = true, bool delay = false)
        {
            RemoveItemAt(GetIndex(item), destroy, delay);
        }
        
        public void RemoveItemLast(bool destroy = true, bool delay = false)
        {
            RemoveItemAt(items.Count - 1, destroy, delay);
        }
        
        public virtual void RemoveItems(int start, int count, bool destroy = true, bool delay = false)
        {
            if (cloneItem == null) return;
            if (delay)
            {
                CoroutineHelper.DelayGuiFrames(1, () =>
                {
                    for (int i = start + count - 1; i >= start; i--)
                    {
                        if (i >= 0 && i < items.Count)
                        {
                            if (destroy) DestroyImmediate(items[i].gameObject);
                            items.RemoveAt(i);
                        }
                    }
                }); //等一下UI的事件更新再销毁
            }
            else
            {
                for (int i = start + count - 1; i >= start; i--)
                {
                    if (i >= 0 && i < items.Count)
                    {
                        if (destroy) DestroyImmediate(items[i].gameObject);
                        items.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region 显示与隐藏

        public void HideItem(int index)
        {
            items[index].gameObject.SetActive(false);
        }

        public void HideAllItem()
        {
            foreach (var item in items) item.gameObject.SetActive(false);
        }

        public void ShowAllItem()
        {
            foreach (var item in items) item.gameObject.SetActive(true);
        }

        #endregion
        
        #region 移动、交换

        protected virtual void SwapItem(int i, int j)
        {
            // SiblingIndex会计算inactive的物体，第一个是模板物体，所以items中是从1开始
            int si = i + 1;
            int sj = j + 1;
            items[i].transform.SetSiblingIndex(sj);
            items[j].transform.SetSiblingIndex(si);

            (items[i], items[j]) = (items[j], items[i]);
        }

        /// <summary>
        /// 按顺序swap一遍，消耗有点点高
        /// </summary>
        /// <param name="srcIndex"></param>
        /// <param name="destIndex"></param>
        public void MoveItem(int srcIndex, int destIndex)
        {
            if (srcIndex > destIndex)
            {
                for (int i = srcIndex; i > destIndex; i--)
                {
                    SwapItem(i, i - 1);
                }
            }
            else if (srcIndex < destIndex)
            {
                for (int i = srcIndex; i < destIndex; i++)
                {
                    SwapItem(i, i + 1);
                }
            }
        }
        
        #endregion

        #region 排序（快排）

        protected int PartitionWithData<T>(List<T> data, int low, int high, Comparison<T> comparison)
        {
            T temp;
            T pivot = data[high];

            // index of smaller element 
            int i = (low - 1);
            for (int j = low; j <= high - 1; j++)
            {
                // If current element is smaller 
                // than or equal to pivot 
                if (comparison(data[j], pivot) < 0) // arr[j] < pivot)
                {
                    i++;

                    if (i != j) // swap arr[i] and arr[j] 
                    {
                        temp = data[i];
                        data[i] = data[j];
                        data[j] = temp;
                        SwapItem(i, j);
                    }
                }
            }

            // swap arr[i+1] and arr[high] 
            // (or pivot) 
            if (i + 1 != high)
            {
                temp = data[i + 1];
                data[i + 1] = data[high];
                data[high] = temp;
                SwapItem(i + 1, high);
            }

            return i + 1;
        }

        protected void QuickSortWithData<T>(List<T> data, Comparison<T> comparison)
        {
            int l = 0;
            int h = data.Count - 1;
            // Create an auxiliary stack 
            int[] stack = new int[h - l + 1];
            if (stack.Length <= 1)
                return;
            // initialize top of stack 
            int top = -1;

            // push initial values of l and h to 
            // stack 
            stack[++top] = l;
            stack[++top] = h;

            // Keep popping from stack while 
            // is not empty 
            while (top >= 0)
            {
                // Pop h and l 
                h = stack[top--];
                l = stack[top--];

                // Set pivot element at its 
                // correct position in 
                // sorted array 
                int p = PartitionWithData(data, l, h, comparison);

                // If there are elements on 
                // left side of pivot, then 
                // push left side to stack 
                if (p - 1 > l)
                {
                    stack[++top] = l;
                    stack[++top] = p - 1;
                }

                // If there are elements on 
                // right side of pivot, then 
                // push right side to stack 
                if (p + 1 < h)
                {
                    stack[++top] = p + 1;
                    stack[++top] = h;
                }
            }
        }
        
        protected int Partition(int low, int high, Comparison<Component> comparison)
        {
            var pivot = items[high];

            int i = (low - 1);
            for (int j = low; j <= high - 1; j++)
            {
                if (comparison(items[j], pivot) < 0)
                {
                    i++;

                    if (i != j)
                    {
                        SwapItem(i, j);
                    }
                }
            }

            if (i + 1 != high)
            {
                SwapItem(i + 1, high);
            }

            return i + 1;
        }

        protected void QuickSort(Comparison<Component> comparison)
        {
            int l = 0;
            int h = items.Count - 1;
            int[] stack = new int[h - l + 1];
            if (stack.Length <= 1)
                return;

            int top = -1;

            stack[++top] = l;
            stack[++top] = h;

            while (top >= 0)
            {
                h = stack[top--];
                l = stack[top--];

                int p = Partition(l, h, comparison);

                if (p - 1 > l)
                {
                    stack[++top] = l;
                    stack[++top] = p - 1;
                }

                if (p + 1 < h)
                {
                    stack[++top] = p + 1;
                    stack[++top] = h;
                }
            }
        }

        public void Sort(Comparison<Component> comparison)
        {
            if (items.Count > 1)
            {
                QuickSort(comparison);
            }
        }

        public void SortWithData<T>(List<T> data, Comparison<T> comparison)
        {
            if (items.Count > 1 && data.Count == items.Count)
            {
                QuickSortWithData(data, comparison);
            }
        }
        
        #endregion

        /// <summary>
        /// 获取对应下标的物体
        /// </summary>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetItem<T>(int index) where T : Component
        {
            return items[index] as T;
        }
    }
}