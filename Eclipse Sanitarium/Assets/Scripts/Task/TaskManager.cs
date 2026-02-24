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
        
        // 事件定义
        public event Action<TaskData> OnTaskStarted;
        public event Action<TaskData> OnTaskUpdated;
        public event Action<TaskData> OnTaskCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject); // 根据具体项目需求决定是否跨场景
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 示例：启动第一个任务
            if (taskDatabase.Count > 0)
            {
                StartTask(taskDatabase[0].taskId);
            }
        }

        public void StartTask(string taskId)
        {
            TaskData task = GetTaskById(taskId);
            if (task != null)
            {
                task.status = TaskStatus.InProgress;
                activeTask = task;
                Debug.Log($"任务开始: {task.taskName}");
                OnTaskStarted?.Invoke(task);
            }
        }

        public void UpdateProgress(string taskId, int amount)
        {
            TaskData task = GetTaskById(taskId);
            if (task != null && task.status == TaskStatus.InProgress)
            {
                task.currentProgress += amount;
                OnTaskUpdated?.Invoke(task);

                if (task.IsCompleted)
                {
                    CompleteTask(taskId);
                }
            }
        }

        public void CompleteTask(string taskId, int branchIndex = -1)
        {
            TaskData task = GetTaskById(taskId);
            if (task == null || task.status != TaskStatus.InProgress) return;

            task.status = TaskStatus.Completed;
            Debug.Log($"任务完成: {task.taskName}");
            OnTaskCompleted?.Invoke(task);

            // 处理分支跳转
            string nextId = "";
            if (branchIndex >= 0 && branchIndex < task.branches.Count)
            {
                // 应用属性影响 (此处预留给 PlayerStats)
                ApplyBranchImpacts(task.branches[branchIndex]);
                nextId = task.branches[branchIndex].nextTaskId;
            }
            else
            {
                nextId = task.defaultNextTaskId;
            }

            if (!string.IsNullOrEmpty(nextId))
            {
                StartTask(nextId);
            }
        }

        private void ApplyBranchImpacts(TaskBranch branch)
        {
            if (branch.statImpacts == null) return;
            foreach (var impact in branch.statImpacts)
            {
                Debug.Log($"[占位] 对属性 {impact.statName} 应用增量: {impact.amount}");
                // TODO: 待后续成员提供 PlayerStats 系统后对接
                // PlayerStats.Instance.ModifyStat(impact.statName, impact.amount);
            }
        }

        public TaskData GetTaskById(string id) => taskDatabase.Find(t => t.taskId == id);
        public TaskData GetActiveTask() => activeTask;
    }
}
