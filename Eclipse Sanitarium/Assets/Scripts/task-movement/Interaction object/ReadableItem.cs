using UnityEngine;

// 继承 MonoBehaviour，并实现 IInteractable 接口
public class ReadableItem : MonoBehaviour, IInteractable
{
    [Header("文档内容配置")]
    public string documentTitle = "未知文件";

    [TextArea(5, 10)]
    public string documentContent = "这里是文档的正文内容...";

    [Header("系统设置")]
    public bool isRecordable = true;

    // 【新增】用于存放描边组件的引用
    private Outline _outline;

    void Start()
    {
        // 游戏开始时，尝试获取身上的 Outline 组件
        _outline = GetComponent<Outline>();

        // 如果没有手动挂载 Outline 组件，代码自动帮它加上，防止报错
        if (_outline == null)
        {
            _outline = gameObject.AddComponent<Outline>();
        }

        // 初始化描边的样式（恐怖游戏建议用极细的白光或微弱的冷光）
        _outline.OutlineMode = Outline.Mode.OutlineAll;
        _outline.OutlineColor = new Color(1f, 1f, 1f, 0.5f); // 半透明的白色
        _outline.OutlineWidth = 3f;                          // 描边宽度

        // 初始状态下必须关闭发光
        _outline.enabled = false;
    }

    public string GetInteractPrompt()
    {
        return "阅读 " + documentTitle;
    }

    public void OnInteract()
    {
        // 呼出全局文档 UI
        DocumentUIManager.Instance.ShowDocument(documentTitle, documentContent);
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        // 【核心修改】用开关 Outline 组件代替修改材质颜色
        if (_outline != null)
        {
            _outline.enabled = isHighlighted;
        }
    }
}