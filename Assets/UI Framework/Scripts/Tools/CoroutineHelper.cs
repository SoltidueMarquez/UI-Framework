using System;
using System.Collections;
using UnityEngine;

namespace UI_Framework.Scripts.Tools
{
    public class CoroutineHelper : Singleton<CoroutineHelper>
    {
        public event Action OnGUIInvoke;
        private int _guiFrame;
        
        public void Reload()
        {
            StopAllCoroutines();
            OnGUIInvoke = null;
        }
        
        public static Coroutine DelayGuiFrames(int frameCount, Action action)
        {
            return Instance.StartCoroutine(Instance.DoActionGuiFrame(frameCount, action));
        }

        private IEnumerator DoActionGuiFrame(int frameCount, Action action)
        {
            int end = _guiFrame + frameCount;
            yield return new WaitUntil(() => _guiFrame >= end);

            action.Invoke();
        }
        
        private void OnGUI()
        {
            OnGUIInvoke?.Invoke();
            _guiFrame++;
        }
    }
}