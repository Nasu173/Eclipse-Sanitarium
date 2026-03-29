using UnityEngine;

public class ReadableItem : MonoBehaviour, IInteractable
{
    [Header("中文文档配置")]
    public string documentTitle_Ch = "未知文件";
    [TextArea(5, 10)]
    public string documentContent_Ch = "这里是文档的正文内容...";

    [Header("英文文档配置")] // 【新增】英文配置选项
    public string documentTitle_En = "Unknown File";
    [TextArea(5, 10)]
    public string documentContent_En = "Document content here...";

    [Header("系统设置")]
    public bool isRecordable = true;

    private Outline _outline;

    void Start()
    {
        _outline = GetComponent<Outline>();
        if (_outline == null) _outline = gameObject.AddComponent<Outline>();

        _outline.OutlineMode = Outline.Mode.OutlineAll;
        _outline.OutlineColor = new Color(1f, 1f, 1f, 0.5f);
        _outline.OutlineWidth = 3f;
        _outline.enabled = false;
    }

    public string GetInteractPrompt()
    {
        // 【核心修改】判断当前是否为英文
        bool isEn = GlobalLanguage.Instance != null && GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En;

        string title = isEn ? documentTitle_En : documentTitle_Ch;
        string verb = isEn ? "Read " : "阅读 ";
        return verb + title;
    }

    public void OnInteract()
    {
        // 【核心修改】判断并传递正确的语言文本给 UI
        bool isEn = GlobalLanguage.Instance != null && GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En;

        string title = isEn ? documentTitle_En : documentTitle_Ch;
        string content = isEn ? documentContent_En : documentContent_Ch;

        DocumentUIManager.Instance.ShowDocument(title, content);
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        if (_outline != null) _outline.enabled = isHighlighted;
    }
}