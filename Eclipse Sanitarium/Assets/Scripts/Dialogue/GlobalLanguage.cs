using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLanguage : MonoBehaviour
{
    public static GlobalLanguage Instance;

    public Image fadePanel; // 淡入淡出全屏面板（黑色Image）
    public Image languagePanel; // 语言选择面板
    public float fadeDuration = 0.2f; // 淡入/淡出过渡时长

    public bool isFirstTime = true; // 是否首次进入游戏

    public enum LanguageType
    {
        Ch,
        En,
    }

    public LanguageType currentLanguageType;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (isFirstTime)
        {
            languagePanel.gameObject.SetActive(true);
            isFirstTime = false;
        }
    }

    public void SetLanguageToCh()
    {
        currentLanguageType = LanguageType.Ch;

        StartCoroutine(FadeInOut());
    }

    public void SetLanguageToEn()
    {
        currentLanguageType = LanguageType.En;

        StartCoroutine(FadeInOut());
    }

    /// <summary>
    /// 淡入效果：从透明→黑色（用于场景切换前）
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 1);
    }

    /// <summary>
    /// 淡出效果：从黑色→透明（用于场景加载后）
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 0);
    }

    private IEnumerator FadeInOut()
    {
        yield return StartCoroutine(FadeIn());

        languagePanel.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(FadeOut());
    }
}
