using System;
using System.Collections.Generic;
using UnityEngine;

namespace EclipseSanitarium.TaskSystem
{
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        [Header("任务配置")]
        [SerializeField] private List<TaskData> taskDatabase = new List<TaskData>();
        
        [Header("运行状态")]
        [SerializeField] private TaskData activeTask;
        
        public event Action<TaskData> OnTaskStarted;
        public event Action<TaskData> OnTaskUpdated;
        public event Action<TaskData> OnTaskCompleted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            foreach (var task in taskDatabase) task.ResetTask();
        }

        private void Start()
        {
            if (taskDatabase.Count > 0) StartTask(taskDatabase[0]);
        }

        public void StartTask(TaskData task)
        {
            if (task == null) return;
            
            task.status = TaskStatus.InProgress;
            activeTask = task;

            // 执行开始行为
            foreach (var action in task.onStartActions) action.Execute();

            Debug.Log($"<color=green>[任务系统] 启动: {task.taskName}</color>");
            OnTaskStarted?.Invoke(task);
        }

        public void UpdateProgress(TaskData task, int amount)
        {
            if (task != null && task.status == TaskStatus.InProgress)
            {
                task.currentProgress += amount;
                Debug.Log($"[任务系统] 进度更新: {task.taskName} ({task.currentProgress}/{task.totalProgress})");
                OnTaskUpdated?.Invoke(task);

                if (task.IsCompleted && task.CheckConditions())
                {
                    CompleteTask(task);
                }
            }
        }

        // 外部逻辑（如触发器、对话结束）调用此方法请求尝试完成任务

        public void RequestTaskCompletion()
        {
            if (activeTask == null || activeTask.status != TaskStatus.InProgress) return;

            if (activeTask.CheckConditions())
            {
                CompleteTask(activeTask);
            }
            else
            {
                Debug.Log("[任务系统] 条件未满足，无法完成。");
                OnTaskUpdated?.Invoke(activeTask);
            }
        }

        public void CompleteTask(TaskData task, int branchIndex = -1)
        {
            task.status = TaskStatus.Completed;
            
            // 执行完成行为
            foreach (var action in task.onCompleteActions) action.Execute();

            Debug.Log($"<color=cyan>[任务系统] 完成: {task.taskName}</color>");
            OnTaskCompleted?.Invoke(task);

            TaskData nextTask = null;
            if (branchIndex >= 0 && branchIndex < task.branches.Count)
            {
                foreach (var action in task.branches[branchIndex].branchActions) action.Execute();
                nextTask = task.branches[branchIndex].nextTask;
            }
            else
            {
                nextTask = task.defaultNextTask;
            }

            if (nextTask != null) StartTask(nextTask);
        }

        public TaskData GetActiveTask() => activeTask;
    }
}
