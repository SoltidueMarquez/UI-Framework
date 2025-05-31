using System;
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

        [Tooltip("UI显示类型")] public FormType formType = FormType.None;

        [Tooltip("动画类型")] public FormAnimType formAnimType;
        
        [Tooltip("是否唯一")] public bool ifUnique;
        
        public int id { get; private set; }

        /// <summary>
        /// 预制体的加载路径，一定要重写
        /// </summary>

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
        private void Init()
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
            isOpen = true;
            OpenAnim();
            OnOpen();
        }

        public void Close()
        {
            isOpen = false;
            CloseAnim();
            OnClose();
        }
        #endregion

        #region 动画
        private void OpenAnim()
        {
            switch (formAnimType)
            {
                case FormAnimType.None:
                    gameObject.SetActive(true);
                    break;
                case FormAnimType.Fade:
                    UIAnimation.FadeIn(gameObject);
                    break;
                case FormAnimType.Zoom:
                    UIAnimation.ZoomIn(gameObject);
                    break;
            }
        }
        
        private void CloseAnim()
        {
            switch (formAnimType)
            {
                case FormAnimType.None:
                    gameObject.SetActive(false);
                    break;
                case FormAnimType.Fade:
                    UIAnimation.FadeOut(gameObject);
                    break;
                case FormAnimType.Zoom:
                    UIAnimation.ZoomOut(gameObject);
                    break;
            }
        }
        #endregion

        public UIFormBase GetUIFormBase()
        {
            return this;
        }
    }
}