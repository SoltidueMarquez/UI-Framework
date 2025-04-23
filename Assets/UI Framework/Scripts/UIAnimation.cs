using System;
using DG.Tweening;
using UnityEngine;

namespace UI_Framework.Scripts
{
    public static class UIAnimation
    {
        #region 渐入渐出

        public static void FadeIn(GameObject gameObject, float duration = 0.5f, Action onFinish = default)
        {
            FormActiveByType(gameObject);
            
            gameObject.TryGetComponent<CanvasGroup>(out var canvas);
            if (canvas == null) canvas = gameObject.AddComponent<CanvasGroup>();
            canvas.alpha = 0;
            canvas.DOFade(1, duration).OnComplete(() =>
            {
                onFinish?.Invoke();
            });
        }

        public static void FadeOut(GameObject gameObject, float duration = 0.5f, Action onFinish = default)
        {
            gameObject.GetComponent<CanvasGroup>().DOFade(0, duration).OnComplete(() =>
            {
                gameObject.SetActive(false);
                onFinish?.Invoke();
            });
        }

        #endregion

        #region 缩放

        public static void ZoomIn(GameObject gameObject, float duration = 0.5f, Action onFinish = default)
        {
            FormActiveByType(gameObject);
            
            gameObject.transform.localScale = Vector3.zero;
            gameObject.transform.DOScale(1, duration).OnComplete(() =>
            {
                onFinish?.Invoke();
            });
        }
        
        public static void ZoomOut(GameObject gameObject, float duration = 0.5f, Action onFinish = default)
        {
            gameObject.transform.DOScale(0, duration).OnComplete(() =>
            {
                gameObject.SetActive(false);
                onFinish?.Invoke();
            });
        }

        #endregion

        public static void FormActiveByType(GameObject gameObject)
        {
            gameObject.SetActive(true);
            
            var formBase = gameObject.GetComponent<UIFormBase>();
            if (formBase == null) return;
            switch (formBase.formType)
            {
                case FormType.Top:
                    gameObject.transform.SetAsLastSibling();
                    break;
            }
        }
    }
}