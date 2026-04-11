using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TaskEventEntry
{
    [Header("需要监听的目标任务")]
    public TaskData targetTask;

    [Header("该任务【开始】时触发的事件")]
    public UnityEvent onTaskStarted;

    [Header("该任务【完成】时触发的事件")]
    public UnityEvent onTaskCompleted;
}

/// <summary>
/// 挂载在场景中始终激活的物体上（例如一个专门管事件的空物体 GameManager 或 SceneEventManager）。
/// 用于监听特定任务状态变化，支持同时配置多个任务。
/// </summary>
public class TaskEventListener : MonoBehaviour
{
    [Header("监听任务列表配置")]
    public List<TaskEventEntry> taskEvents = new List<TaskEventEntry>();

    private void Start()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskStarted += HandleTaskStarted;
            TaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;
        }
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskStarted -= HandleTaskStarted;
            TaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
        }
    }

    private void HandleTaskStarted(TaskData task)
    {
        foreach (var entry in taskEvents)
        {
            if (entry.targetTask == task)
            {
                entry.onTaskStarted?.Invoke();
            }
        }
    }

    private void HandleTaskCompleted(TaskData task)
    {
        foreach (var entry in taskEvents)
        {
            if (entry.targetTask == task)
            {
                entry.onTaskCompleted?.Invoke();
            }
        }
    }
}
