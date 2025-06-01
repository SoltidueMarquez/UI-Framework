using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plugins.UI_Framework.Scripts.Tools
{
    public class UIButtonEx : Button
    {
        public enum ButtonExState
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled,
        }
        
        public UnityEvent onPointerEnter = new UnityEvent();
        public UnityEvent onPointerExit = new UnityEvent();
        public UnityEvent onRightClick = new UnityEvent();
        public UnityEvent onDeselect = new UnityEvent();
        public UnityEvent onSelect = new UnityEvent();

        [Header("鼠标移开时自动弹起(OnDeselect)")] public bool autoUp;
        public string clickSound;
        public string highLightSound;
        public bool cantClick;

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable() || cantClick)
                return;
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                UISystemProfilerApi.AddMarker("ButtonEx.onPointerClick", this);
                onRightClick.Invoke();
                return;
            }
            else
            {
                base.OnPointerClick(eventData);
            }

            if (!string.IsNullOrEmpty(clickSound))
                // AudioMgr.I.PlaySe2D(CueSheetType.ui, clickSound);
                eventData.Use();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("ButtonEx.onPointerEnter", this);
            onPointerEnter.Invoke();
            // if (!string.IsNullOrEmpty(highLightSound))
            // AudioMgr.I.PlaySe2D(CueSheetType.ui, highLightSound);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("ButtonEx.onPointerExit", this);
            onPointerExit.Invoke();

            if ((colors.disabledColor.a == 0f || autoUp))
            {
                ManualStateTransition(ButtonExState.Normal);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            onSelect.Invoke();
            // if (!string.IsNullOrEmpty(highLightSound))
            //     AudioMgr.I.PlaySe2D(CueSheetType.ui, highLightSound);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            onDeselect.Invoke();
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (cantClick) return;
            base.OnSubmit(eventData);
            // if (!string.IsNullOrEmpty(clickSound))
            //     AudioMgr.I.PlaySe2D(CueSheetType.ui, clickSound);
        }

        public void ManualStateTransition(ButtonExState state)
        {
            if (state != ButtonExState.Selected && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            DoStateTransition((SelectionState)state, false);
        }
    }
}