using UnityEngine;
using UnityEngine.UI;

public class LanguageSwitchButtons : MonoBehaviour
{
    [Header("按钮设置")]
    [Tooltip("中文按钮")]
    public Button chineseButton;

    [Tooltip("英文按钮")]
    public Button englishButton;

    [Header("可选：切换后要更新的UI组件")]
    [Tooltip("需要刷新文本的TextMeshPro组件（如标题、提示等）")]
    public GameObject[] uiElementsToRefresh;

    private void Start()
    {
        // 绑定按钮事件
        if (chineseButton != null)
        {
            chineseButton.onClick.AddListener(OnChineseButtonClick);
        }
        else
        {
            Debug.LogWarning("中文按钮未绑定！");
        }

        if (englishButton != null)
        {
            englishButton.onClick.AddListener(OnEnglishButtonClick);
        }
        else
        {
            Debug.LogWarning("英文按钮未绑定！");
        }

        // 可选：根据当前语言更新按钮状态
        UpdateButtonStates();
    }

    /// <summary>
    /// 中文按钮点击事件
    /// </summary>
    public void OnChineseButtonClick()
    {
        if (GlobalLanguage.Instance == null)
        {
            Debug.LogError("GlobalLanguage.Instance 不存在！");
            return;
        }

        // 调用GlobalLanguage的方法切换到中文
        GlobalLanguage.Instance.SetLanguageToCh();

        Debug.Log("语言已切换到：中文");

        // 更新按钮状态
        UpdateButtonStates();

        // 刷新UI文本
        RefreshUIElements();
    }

    /// <summary>
    /// 英文按钮点击事件
    /// </summary>
    public void OnEnglishButtonClick()
    {
        if (GlobalLanguage.Instance == null)
        {
            Debug.LogError("GlobalLanguage.Instance 不存在！");
            return;
        }

        // 调用GlobalLanguage的方法切换到英文
        GlobalLanguage.Instance.SetLanguageToEn();

        Debug.Log("语言已切换到：English");

        // 更新按钮状态
        UpdateButtonStates();

        // 刷新UI文本
        RefreshUIElements();
    }

    /// <summary>
    /// 更新按钮状态（如高亮当前语言按钮）
    /// </summary>
    private void UpdateButtonStates()
    {
        if (GlobalLanguage.Instance == null) return;

        // 这里可以根据需要设置按钮的高亮状态
        // 例如：改变按钮颜色、显示选中标记等

        switch (GlobalLanguage.Instance.currentLanguageType)
        {
            case GlobalLanguage.LanguageType.Ch:
                // 高亮中文按钮，淡化英文按钮
                SetButtonHighlight(chineseButton, true);
                SetButtonHighlight(englishButton, false);
                break;

            case GlobalLanguage.LanguageType.En:
                // 高亮英文按钮，淡化中文按钮
                SetButtonHighlight(chineseButton, false);
                SetButtonHighlight(englishButton, true);
                break;
        }
    }

    /// <summary>
    /// 设置按钮高亮状态
    /// </summary>
    private void SetButtonHighlight(Button button, bool highlighted)
    {
        if (button == null) return;

        // 可以自定义高亮效果，比如改变颜色
        ColorBlock colors = button.colors;

        if (highlighted)
        {
            // 高亮状态：更亮的颜色
            colors.normalColor = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        }
        else
        {
            // 非高亮状态：半透明
            colors.normalColor = new Color(1f, 1f, 1f, 0.5f);
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
        }

        button.colors = colors;
    }

    /// <summary>
    /// 刷新UI元素（如标题、提示文本等）
    /// </summary>
    private void RefreshUIElements()
    {
        if (uiElementsToRefresh == null) return;

        foreach (GameObject uiElement in uiElementsToRefresh)
        {
            if (uiElement == null) continue;

            // 触发UI组件的刷新
            // 这里可以根据需要调用各组件的刷新方法
            IRefreshable refreshable = uiElement.GetComponent<IRefreshable>();
            if (refreshable != null)
            {
                refreshable.OnLanguageChanged();
            }

            // 或者通过SendMessage方式
            uiElement.SendMessage("OnLanguageChanged", SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// 直接切换语言（不通过按钮，可用于其他脚本调用）
    /// </summary>
    public void SwitchToChinese()
    {
        OnChineseButtonClick();
    }

    /// <summary>
    /// 直接切换语言（不通过按钮，可用于其他脚本调用）
    /// </summary>
    public void SwitchToEnglish()
    {
        OnEnglishButtonClick();
    }
}

// 可选：刷新接口，需要刷新的UI组件可以实现这个接口
public interface IRefreshable
{
    void OnLanguageChanged();
}