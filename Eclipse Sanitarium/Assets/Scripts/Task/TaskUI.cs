using UnityEngine;
using TMPro;

public class TaskUI : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private TextMeshProUGUI taskNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        if (taskPanel != null)
        {
            panelCanvasGroup = taskPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = taskPanel.AddComponent<CanvasGroup>();
            
            // 防止拦截射线点击
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskStarted -= HandleTaskStarted;
            TaskManager.Instance.OnTaskUpdated -= HandleTaskUpdated;
            TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;

            TaskManager.Instance.OnTaskStarted += HandleTaskStarted;
            TaskManager.Instance.OnTaskUpdated += HandleTaskUpdated;
            TaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;

            // 初始化显示当前任务
            RefreshUI(TaskManager.Instance.GetActiveTask());
        }
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskStarted -= HandleTaskStarted;
            TaskManager.Instance.OnTaskUpdated -= HandleTaskUpdated;
            TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
        }
    }

    private void HandleTaskStarted(TaskData task)
    {
        RefreshUI(task);
    }

    private void HandleTaskUpdated(TaskData task)
    {
        RefreshUI(task);
    }

    private void HandleTaskCompleted(TaskData task)
    {
        if (taskNameText != null)
        {
            string nameToDisplay = GlobalLanguage.Instance != null && GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En && !string.IsNullOrEmpty(task.taskName_En) 
                ? task.taskName_En 
                : task.taskName;
            taskNameText.text = $"<s>{nameToDisplay}</s> <color=green>✓</color>";
        }
    }

    private void RefreshUI(TaskData task)
    {
        if (task == null)
        {
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0f;
            return;
        }

        if (panelCanvasGroup != null) panelCanvasGroup.alpha = 1f;

        bool isEnglish = GlobalLanguage.Instance != null && GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En;

        if (taskNameText != null) 
        {
            taskNameText.text = isEnglish && !string.IsNullOrEmpty(task.taskName_En) ? task.taskName_En : task.taskName;
        }
        
        if (descriptionText != null) 
        {
            descriptionText.text = isEnglish && !string.IsNullOrEmpty(task.description_En) ? task.description_En : task.description_Zh;
        }
    }
}
