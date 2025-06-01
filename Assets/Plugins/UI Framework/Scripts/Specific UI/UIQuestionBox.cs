using System;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Specific_UI
{
    /// <summary>
    /// 总是创建并销毁
    /// </summary>
    public class UIQuestionBox: UIFormBase
    {
        public Button yesBtn;
        public Button noBtn;
        public Text contentText;

        private Action<bool> _callback;
        private Action _yesCallback;

        protected override void OnInit()
        {
            base.OnInit();
            yesBtn.onClick.AddListener(YesClick);
            noBtn.onClick.AddListener(NoClick);
            Open();

            OnClose += DestroySelf;
        }

        public UIQuestionBox SetTips(string contentKey)
        {
            contentText.text = contentKey;
            return this;
        }

        /// <summary>
        /// 点击yes或no按钮后的callback
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public UIQuestionBox Callback(Action<bool> action)
        {
            _callback = action;
            return this;
        }

        /// <summary>
        /// 点击yes按钮后的callback
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public UIQuestionBox YesCallback(Action action)
        {
            _yesCallback = action;
            return this;
        }

        private void YesClick()
        {
            _callback?.Invoke(true);
            _yesCallback?.Invoke();
            Close();
        }

        private void NoClick()
        {
            _callback?.Invoke(false);
            Close();
        }
    }
}