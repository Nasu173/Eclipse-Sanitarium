using UnityEngine;

/// <summary>
/// 可以挂载在任何可互动物品（比如 TestItem、机制物品）的同一个 GameObject 上。
/// 这样当玩家对它按 E 产生互动时，这个组件会被同步触发，用于汇报并尝试完成特定任务。
/// </summary>
public class TaskInteractCompleter : MonoBehaviour, IInteractable
{
    [Header("互动的目标任务")]
    [Tooltip("指定当你与这个物品互动时，试图完成哪个任务")]
    public TaskData targetTask;

    [Header("互动提示词（可选）")]
    [Tooltip("通常留空即可，会自动采用别的互动组件的文字（取决于哪一个排在前面）。非空时可强行显示提示词。")]
    public string prompt = "";
    public string prompt_En = "";

    public string GetInteractPrompt()
    {
        if (string.IsNullOrEmpty(prompt)) return "";
        
        if (GlobalLanguage.Instance != null && GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En)
        {
            return prompt_En;
        }
        return prompt;
    }

    public void OnInteract()
    {
        if (TaskManager.Instance != null && targetTask != null)
        {
            // 发起定向完成请求
            TaskManager.Instance.RequestTaskCompletion(targetTask);
        }
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        // 留空，高亮逻辑通常由主互动组件（如 TestItem 等）去表现
    }
}
