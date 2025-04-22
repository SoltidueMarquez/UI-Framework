using UnityEngine;

namespace UI_Framework.Scripts
{
    /// <summary>
    /// UI基础类
    /// </summary>
    public class UIFormBase : MonoBehaviour, IUIForm
    {
        private UIMgr m_UIMgr;

        [Tooltip("打开状态")] public bool isOpen;

        /// <summary>
        /// 预制体的加载路径，一定要重写
        /// </summary>
        public virtual string PrefabPath { get; }

        public int id { get; private set; }

        [Tooltip("是否唯一")] public bool ifUnique;

        #region 创建时与销毁时
        private void Awake()
        {
            id = IdManager.GetUniqueID();
            
            // 先初始化id，再进行注册
            IUIForm uiForm = this;
            uiForm.RegisterForm();

            m_UIMgr = UIMgr.Instance;
            Init();
        }

        private void OnDestroy()
        {
            IUIForm uiForm = this;
            uiForm.UnRegisterForm();
            
            IdManager.ReCycleId(id); // 回收id
        }
        #endregion

        #region 创建时初始化
        /// <summary>
        /// 只会在创建时调用一次
        /// </summary>
        protected void Init()
        {
            gameObject.SetActive(false);
            isOpen = false;

            OnInit();
        }

        /// <summary>
        /// 只会在创建时调用一次
        /// </summary>
        protected virtual void OnInit() { }
        #endregion

        #region 开启与关闭
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        
        public void Open()
        {
            gameObject.SetActive(true);
            isOpen = true;
            OnOpen();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            isOpen = false;
            OnClose();
        }
        #endregion

        public UIFormBase GetUIFormBase()
        {
            return this;
        }
    }
}