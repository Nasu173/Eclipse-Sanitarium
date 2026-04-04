using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 挂载在场景中始终激活的物体上（例如一个专门管事件的空物体 GameManager 或 SceneEventManager）。
/// 用于监听某个特定任务完成，然后触发对应的场景表现。
/// </summary>
public class TaskEventListener : MonoBehaviour
{
    [Header("需要监听的目标任务")]
    public TaskData targetTask;

    [Header("该任务【开始】时触发的事件")]
    public UnityEvent onTaskStarted;

    [Header("该任务【完成】时触发的事件")]
    public UnityEvent onTaskCompleted;

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
        // 判断触发的任务是不是我们正在窃听的那个任务
        if (task == targetTask)
        {
            onTaskStarted?.Invoke();
        }
    }

    private void HandleTaskCompleted(TaskData task)
    {
        if (task == targetTask)
        {
            onTaskCompleted?.Invoke();
        }
    }
}
