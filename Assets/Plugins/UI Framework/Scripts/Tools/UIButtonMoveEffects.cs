using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Battle.UI
{
    public class UIButtonMoveEffects : MonoBehaviour
    {
        private Tween _pointerEnter;
        private Tween _pointerExit;
        private Tween _pointerClick;
        private Vector3 _originalScale;

        public UnityEvent afterClickAnim;

        private bool _pendingExit; // 新增标志位

        [LabelText("需要替换的按钮图片")] public Image btnImage;
        [LabelText("需要更换颜色的的Text")] public Text btnText;

        [Header("Pointer Enter")]
        public bool enterNeedScale;
        public Color enterTextColor;
        public Sprite enterSprite;
        [Header("Pointer Exit")]
        public bool exitNeedScale;
        public Color exitTextColor;
        public Sprite exitSprite;

        private void Start()
        {
            _originalScale = transform.localScale;

            _pointerEnter = transform.DOScale(_originalScale * 1.2f, 0.3f)
                .SetAutoKill(false)
                .Pause();

            _pointerExit = transform.DOScale(_originalScale, 0.3f)
                .SetAutoKill(false)
                .Pause();

            _pointerClick = transform.DOScale(_originalScale * 0.8f, .15f)
                .OnComplete(() => { transform.DOScale(_originalScale, .1f).OnComplete(OnClickComplete); })
                .SetAutoKill(false) // 绑定完成回调
                .Pause();

            if (btnImage != null && exitSprite != null) btnImage.sprite = exitSprite;
            if (btnText != null) btnText.color = exitTextColor;
        }

        private void OnDestroy()
        {
            _pointerEnter?.Kill();
            _pointerExit?.Kill();
            _pointerClick?.Kill();
        }

        private void OnClickComplete()
        {
            // 点击动画完成后检查是否需要执行退出动画
            if (_pendingExit)
            {
                _pendingExit = false;
                _pointerExit.Restart();
            }
            
            afterClickAnim?.Invoke();
        }

        public void PointerEnter()
        {
            // 改按钮贴图
            if (btnImage != null && enterSprite != null)
                btnImage.sprite = enterSprite;

            // 改文字颜色
            if (btnText != null)
                btnText.color = enterTextColor;

            if (enterNeedScale)
            {
                _pendingExit = false; // 取消待处理的退出请求
                _pointerExit.Pause();
                _pointerClick.Pause();
                _pointerEnter.Restart();
            }
        }

        public void PointerExit()
        {
            // 改按钮贴图
            if (btnImage != null && exitSprite != null)
                btnImage.sprite = exitSprite;

            // 改文字颜色
            if (btnText != null)
                btnText.color = exitTextColor;

            if (exitNeedScale)
            {
                // 如果点击动画正在播放，标记待处理退出
                if (_pointerClick != null && _pointerClick.IsPlaying())
                {
                    _pendingExit = true;
                    _pointerExit.Pause();
                }
                else
                {
                    // 直接执行退出动画
                    _pointerEnter.Pause();
                    _pointerClick.Pause();
                    _pointerExit.Restart();
                }
            }
            
        }

        public void PointerClick()
        {
            _pointerEnter.Pause();
            _pointerExit.Pause();
            _pointerClick.Restart();
        }
    }
}