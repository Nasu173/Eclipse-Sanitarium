using UnityEngine;

public class TaskTrigger : MonoBehaviour
{
    public enum TriggerType { Collision, Interaction, Custom }

    [Header("触发配置")]
    public TriggerType type = TriggerType.Interaction;
    
    [Tooltip("直接拖入关联的 Task 资源")]
    public TaskData targetTask;
    
    public int progressAmount = 1;
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    public void Trigger()
    {
        if (triggerOnce && hasTriggered) return;
        if (targetTask == null)
        {
            // 如果没指定特定任务，默认尝试更新当前活动任务
            targetTask = TaskManager.Instance.GetActiveTask();
        }

        if (TaskManager.Instance != null && targetTask != null)
        {
            TaskManager.Instance.UpdateProgress(targetTask, progressAmount);
            hasTriggered = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (type == TriggerType.Collision && other.CompareTag("Player"))
        {
            Trigger();
        }
    }

    public void OnInteract()
    {
        if (type == TriggerType.Interaction)
        {
            Trigger();
        }
    }
}
