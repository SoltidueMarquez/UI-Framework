using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI_Framework.Scripts.Tools
{
    // TODO：场景切换工具，待实现更多效果
    public class SceneChangeHelper : PersistentSingleton<SceneChangeHelper>
    {
        [SerializeField] private float durationTime = 1f;
        [SerializeField] private Image mask;
        
        [Header("动画序列")]
        
        // 淡入淡出遮罩
        private Sequence m_SceneFadeOutSequence;
        private Sequence m_SceneFadeInSequence;

        public event Action OnSceneFadeOutComplete;
        public event Action OnSceneFadeInComplete;

        protected override void Awake()
        {
            base.Awake();
            InitializeSequences();
        }
        
        private void InitializeSequences()
        {
            // 初始化淡入序列（遮罩不透明）
            m_SceneFadeInSequence = DOTween.Sequence().Append(mask.DOFade(1, durationTime)).OnComplete(() => OnSceneFadeInComplete?.Invoke()).SetAutoKill(false).Pause();

            // 初始化淡出序列（遮罩透明）
            m_SceneFadeOutSequence = DOTween.Sequence().Append(mask.DOFade(0, durationTime)).OnComplete(() => OnSceneFadeOutComplete?.Invoke()).SetAutoKill(false).Pause();

            // 初始状态设置为透明
            mask.color = new Color(0, 0, 0, 0);
        }
        
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("场景名不存在，检查builder");
                return;
            }

            if (mask == null)
            {
                Debug.LogError("遮罩图片找不到了");
                SceneManager.LoadScene(sceneName);
                return;
            }

            // 先播放淡入动画，加载场景后播放淡出动画
            m_SceneFadeInSequence.Restart();
            m_SceneFadeInSequence.OnComplete(() =>
            {
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
                asyncOp.completed += OnSceneLoaded;
            });
        }

        private void OnSceneLoaded(AsyncOperation asyncOp)
        {
            asyncOp.completed -= OnSceneLoaded;
            m_SceneFadeInSequence.Pause();
            m_SceneFadeOutSequence.Restart();
        }
        
        private void OnDestroy()
        {
            // 清理动画序列
            m_SceneFadeInSequence?.Kill();
            m_SceneFadeOutSequence?.Kill();
        }
    }
}
