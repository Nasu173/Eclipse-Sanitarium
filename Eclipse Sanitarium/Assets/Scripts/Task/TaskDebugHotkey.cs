using UnityEngine;

public class TaskDebugHotkey : MonoBehaviour
{
    private void Update()
    {
        // 测试后门：按 P 键强制完成当前进行中的任务
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (TaskManager.Instance == null) return;

            TaskData activeTask = TaskManager.Instance.GetActiveTask();
            
            if (activeTask != null && activeTask.status == TaskStatus.InProgress)
            {
                Debug.Log($"<color=yellow>[调试工具] 通过快捷键 (P) 强制完成任务: {activeTask.taskName}</color>");
                TaskManager.Instance.CompleteTask(activeTask);
            }
            else
            {
                Debug.Log("[调试工具] 当前没有正在进行中的任务可以完成。");
            }
        }
    }
}
