using System.Collections.Generic;
using UnityEngine;

namespace UI_Framework.Scripts
{
    // TODO：页面是否唯一的处理
    // TODO：加载预制体暂时为Resource.Load
    // TODO：LRU是不以应该包含没有被打开的面板？LRU的使用有点奇怪
    public class UIMgr : Singleton<UIMgr>
    {
        [Tooltip("UI字典，键为物体名称，值为类")] public Dictionary<int, UIFormBase> forms = new Dictionary<int, UIFormBase>();

        [Tooltip("双向链表，用于LRU，最前端是最近使用的，全都是打开的面板")]
        public LinkedList<UIFormBase> ShowFormsLRU = new LinkedList<UIFormBase>();

        [Tooltip("字典存储窗体名称对应的链表节点，用于快速LRU操作")] private Dictionary<int, LinkedListNode<UIFormBase>> lruNodeDict
            = new Dictionary<int, LinkedListNode<UIFormBase>>();

        [Tooltip("面板根节点")] public Transform uiRoot => this.transform;

        #region 注册注销

        // 面板注册方法
        public void RegisterForm(IUIForm uiForm)
        {
            var form = uiForm.GetUIFormBase();
            if (!forms.ContainsKey(form.id)) // 如果字典里没有就加进去
            {
                forms.Add(form.id, form);
            }
            else // 有了就重新赋值
            {
                forms[form.id] = form;
            }
        }

        // 面板注销方法
        public void UnRegisterForm(IUIForm uiForm)
        {
            var form = uiForm.GetUIFormBase();
            if (forms.ContainsKey(form.id))
            {
                forms.Remove(form.id);

                RemoveUnUseLru(form); // 更新LRU
            }
        }

        #endregion

        #region LRU维护

        private void UpdateUseLru(UIFormBase form)
        {
            // 如果存在节点，移动到链表头部表示最近使用
            if (lruNodeDict.TryGetValue(form.id, out var node))
            {
                ShowFormsLRU.Remove(node);
                ShowFormsLRU.AddFirst(node);
            }
            else// 如果不存在就更新
            {
                var newNode = ShowFormsLRU.AddFirst(form);
                lruNodeDict[form.id] = newNode;
            }
        }

        private void RemoveUnUseLru(UIFormBase form)
        {
            // 从LRU链表和字典中移除
            if (lruNodeDict.TryGetValue(form.id, out var node))
            {
                ShowFormsLRU.Remove(node);
                lruNodeDict.Remove(form.id);
            }
        }

        #endregion

        #region 动态生成与销毁

        public int CreateUI<T>() where T : UIFormBase
        {
            // 通过反射获取默认构造实例（不实际创建GameObject）
            var tempInstance = System.Activator.CreateInstance<T>();
            var path = tempInstance.PrefabPath;

            // 加载预制体
            var prefab = Resources.Load<GameObject>(path);
            var ui = Instantiate(prefab, uiRoot);

            return ui.GetComponent<T>().id;
        }

        public void DestroyUI(int id)
        {
            if (!forms.ContainsKey(id)) return;

            var form = forms[id];
            Destroy(form.gameObject);
        }

        #endregion

        #region UI操作

        // 显示UI
        public void ShowUIForm(int id)
        {
            if (!forms.ContainsKey(id)) return;

            var form = forms[id];
            form.Open();

            // LRU更新，将节点移到最前面
            UpdateUseLru(form);
        }

        // 隐藏UI面板
        public void HideUIForm(int id)
        {
            if (!forms.ContainsKey(id)) return;

            var form = forms[id];
            form.Close();

            // LRU更新，删除节点
            RemoveUnUseLru(form);
        }

        #endregion
    }

    public interface IUIForm
    {
        void RegisterForm() => UIMgr.Instance.RegisterForm(this);

        void UnRegisterForm() => UIMgr.Instance.UnRegisterForm(this);

        UIFormBase GetUIFormBase();
    }
}