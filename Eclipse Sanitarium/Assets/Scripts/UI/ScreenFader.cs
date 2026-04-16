using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ProjectUI
{
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private CanvasGroup faderCanvasGroup;
        [SerializeField] private float defaultFadeDuration = 1.0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            // 初始状态：确保黑幕存在但透明（或者根据需求初始黑屏）
            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0;
                faderCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// 屏幕变黑 (Fade to Black)
        /// </summary>
        public Coroutine FadeOut(float duration = -1)
        {
            float d = duration < 0 ? defaultFadeDuration : duration;
            return StartCoroutine(FadeRoutine(1, d));
        }

        /// <summary>
        /// 屏幕变亮 (Fade from Black)
        /// </summary>
        public Coroutine FadeIn(float duration = -1)
        {
            float d = duration < 0 ? defaultFadeDuration : duration;
            return StartCoroutine(FadeRoutine(0, d));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            if (faderCanvasGroup == null) yield break;

            float startAlpha = faderCanvasGroup.alpha;
            float elapsed = 0;

            // 如果要变黑，拦截所有输入射线，防止点击到后面的 UI 或物体
            if (targetAlpha > 0.5f) faderCanvasGroup.blocksRaycasts = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            faderCanvasGroup.alpha = targetAlpha;

            // 如果完全变亮，取消射线拦截
            if (targetAlpha < 0.1f) faderCanvasGroup.blocksRaycasts = false;
        }
    }
}
