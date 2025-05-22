using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Tools
{
    // TODO：场景切换工具，待实现
    public class SceneChangeHelper : MonoBehaviour
    {
        public float durationTime;
        
        public Image mask;
        
        [Header("淡出效果")]
        private Sequence m_SceneChange_CrossOut;
        public Sprite m_MaskSprite;
        
        

        public event Action AfterSceneOut;

        private void Awake()
        {
            m_SceneChange_CrossOut.Append(mask.DOFade(0, durationTime).OnComplete(() => { AfterSceneOut?.Invoke(); }));
        }
    }
}
