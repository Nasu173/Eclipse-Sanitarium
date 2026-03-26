using UnityEngine;

// 继承 MonoBehaviour，并实现 IInteractable 接口
public class ReadableItem : MonoBehaviour, IInteractable
{
    [Header("文档内容配置")]
    public string documentTitle = "未知文件";

    // [TextArea] 是个非常实用的标签！
    // 它会在 Unity 面板里生成一个多行大文本框，方便策划填入长篇的“观测笔记”或“日记”
    [TextArea(5, 10)]
    public string documentContent = "这里是文档的正文内容...";

    [Header("系统设置")]
    // 对应表格里的“阅读完成后自动记录到文档集UI”
    public bool isRecordable = true;

    // --- 以下是必须实现的接口方法 ---

    public string GetInteractPrompt()
    {
        // 玩家准心对准时，屏幕会显示 "[E] 阅读 混乱的观测笔记"
        return "阅读 " + documentTitle;
    }

    public void OnInteract()
    {
        // 当玩家按下 E 键时触发
        // 【第一步】暂停玩家的移动和视角转动（通常恐怖游戏阅读时不能乱跑）

        // 【第二步】呼出阅读界面的 UI 面板，并把文字传进去
        // 极简！直接把当前物品配好的标题和正文扔给单例管理器
        DocumentUIManager.Instance.ShowDocument(documentTitle, documentContent);

        // 【第三步】如果勾选了记录，就把它扔进玩家的档案集里
        if (isRecordable)
        {
            Debug.Log($"已将《{documentTitle}》永久记录到玩家的档案库中！");
        }
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        // 这里的逻辑和你之前写的 TestItem 一样
        // 如果你们项目后续用了描边插件（Outline），就获取 Outline 组件并开关它
        // 目前为了测试，我们可以先留空，或者简单变色
        if (TryGetComponent<Renderer>(out Renderer r))
        {
            r.material.color = isHighlighted ? Color.yellow : Color.white;
        }
    }
}