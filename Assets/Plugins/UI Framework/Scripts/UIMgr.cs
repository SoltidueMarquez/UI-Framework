using System.Collections.Generic;
using System.Linq;
using Audio_Manager;
using UnityEngine;

namespace UI_Framework.Scripts
{
    // TODO：加载预制体暂时为Resource.Load
    // TODO：LRU是不以应该包含没有被打开的面板？LRU的使用有点奇怪
    public class UIMgr : PersistentSingleton<UIMgr>
    {
        [Tooltip("UI字典，键为物体名称，值为类，全部的UI都在这里")] public Dictionary<int, UIFormBase> forms = new Dictionary<int, UIFormBase>();

        [Tooltip("当前显示的UI")] public List<int> activeUIList = new List<int>();

        [Tooltip("面板根节点")] private Transform uiRoot => this.transform;

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

        [Tooltip("双向链表，用于LRU，最前端是最近使用的，全都是打开的面板")]
        public LinkedList<UIFormBase> formsLRU = new LinkedList<UIFormBase>();

        [Tooltip("字典存储窗体名称对应的链表节点，用于快速LRU操作")] private Dictionary<int, LinkedListNode<UIFormBase>> lruNodeDict
            = new Dictionary<int, LinkedListNode<UIFormBase>>();

        private void UpdateUseLru(UIFormBase form)
        {
            // 如果存在节点，移动到链表头部表示最近使用
            if (lruNodeDict.TryGetValue(form.id, out var node))
            {
                formsLRU.Remove(node);
                formsLRU.AddFirst(node);
            }
            else // 如果不存在就更新
            {
                var newNode = formsLRU.AddFirst(form);
                lruNodeDict[form.id] = newNode;
            }
        }

        private void RemoveUnUseLru(UIFormBase form)
        {
            // 从LRU链表和字典中移除
            if (lruNodeDict.TryGetValue(form.id, out var node))
            {
                formsLRU.Remove(node);
                lruNodeDict.Remove(form.id);
            }
        }

        #endregion

        #region 动态生成销毁、与获取
        public T CreateUI<T>() where T : UIFormBase
        {
            // 先检查是否有唯一实例
            foreach (var kv in forms)
            {
                if (kv.Value is T { ifUnique: true } existingForm)
                {
                    return existingForm; // 已存在唯一实例，返回其id
                }
            }

            // 加载预制体（推荐使用静态注册表而不是反射）
            var path = UIFormRegistry.GetPath<T>(); // 替代 tempInstance.PrefabPath
            var prefab = Resources.Load<GameObject>(path);
            var ui = Instantiate(prefab, uiRoot);
            var form = ui.GetComponent<T>();

            forms[form.id] = form;

            form.OnOpen += () =>
            {
                activeUIList.Add(form.id);
                // LRU更新，将节点移到最前面
                UpdateUseLru(form);
            };
            form.OnClose += () =>
            {
                activeUIList.Remove(form.id);
                // LRU更新
                UpdateUseLru(form);
            };
            return form;
        }

        public void DestroyUI(int id)
        {
            if (!forms.ContainsKey(id)) return;

            var form = forms[id];
            form.DestroySelf();
        }
        
        public void DestroyUI(UIFormBase ui)
        {
            DestroyUI(ui.id);
        }
        
        /// <summary>
        /// 获取到此类UI的全部实例id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<int> GetCurrentUIIds<T>() where T : UIFormBase
        {
            List<int> ids = new();
            foreach (var kv in forms)
            {
                if (kv.Value is T existingForm)
                {
                    ids.Add(existingForm.id); // 已存在唯一实例，返回其id
                }
            }

            return ids;
        }
        
        public List<T> GetCurrentUIs<T>() where T : UIFormBase
        {
            List<T> uis = new();
            foreach (var kv in forms)
            {
                if (kv.Value is T existingForm)
                {
                    uis.Add(existingForm); // 已存在唯一实例，返回其id
                }
            }

            return uis;
        }
        
        public T GetFirstUI<T>() where T : UIFormBase
        {
            foreach (var kv in forms)
            {
                if (kv.Value is T existingForm)
                {
                    return existingForm;
                }
            }
            return null;
        }
        #endregion
        
        #region UI操作
        // 显示UI
        public void ShowUIForm(int id)
        {
            if (!forms.ContainsKey(id)) return;

            var form = forms[id];
            if (form.isOpen) return; //已经打开了就直接返回
            form.Open();
        }

        // 隐藏UI面板
        public void HideUIForm(int id)
        {
            if (!forms.ContainsKey(id)) return;

            var form = forms[id];
            if (!form.isOpen) return;
            form.Close();
        }

        // 隐藏所有UI
        public void HideAllUIForm()
        {
            foreach (var id in activeUIList.Where(id => forms.ContainsKey(id)))
            {
                HideUIForm(id);
            }

            activeUIList.Clear();
        }

        // 倒序关闭UI
        public void HideUIFormTurn()
        {
            for (var i = activeUIList.Count - 1; i >= 0; i--)
            {
                HideUIForm(activeUIList[i]);
            }

            activeUIList.Clear();
        }

        // 是否有显示的UI
        public bool HasActiveForm()
        {
            return activeUIList.Count > 0;
        }

        #endregion
    }

    public interface IUIForm
    {
        void RegisterForm() => UIMgr.Instance.RegisterForm(this);

        void UnRegisterForm() => UIMgr.Instance.UnRegisterForm(this);

        UIFormBase GetUIFormBase();
    }

    public enum FormType{
        None,
        Top,// 总是在上层
    }
}