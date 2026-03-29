using UnityEngine;
using TMPro; // 必须引入 TextMeshPro

// 强制要求挂载这个脚本的物体，身上必须要有 TextMeshProUGUI 组件
[RequireComponent(typeof(TextMeshProUGUI))]
public class UILocalizeText : MonoBehaviour
{
    [Header("多语言文本配置")]
    public string text_Ch = "中文文本";
    public string text_En = "English Text";

    private TextMeshProUGUI _tmpText;

    void Start()
    {
        // 获取自己身上的 TMP 组件
        _tmpText = GetComponent<TextMeshProUGUI>();

        // 【核心】向全局语言管家订阅“语言改变”事件
        if (GlobalLanguage.Instance != null)
        {
            GlobalLanguage.Instance.OnLanguageChanged += RefreshText;

            // 游戏刚开始时，立刻手动刷新一次，确保初始显示的语言和后台一致
            RefreshText();
        }
    }

    void OnDestroy()
    {
        // 物体销毁时取消订阅，防止内存泄漏
        if (GlobalLanguage.Instance != null)
        {
            GlobalLanguage.Instance.OnLanguageChanged -= RefreshText;
        }
    }

    // 每次听到语言切换的广播，就会自动执行这个方法
    private void RefreshText()
    {
        if (_tmpText == null || GlobalLanguage.Instance == null) return;

        // 判断当前语言并替换文字
        bool isEn = GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En;
        _tmpText.text = isEn ? text_En : text_Ch;
    }
}