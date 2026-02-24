using System;
using System.Collections.Generic;
using UnityEngine;

namespace EclipseSanitarium.TaskSystem
{
    [Serializable]
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    [Serializable]
    public class TaskBranch
    {
        [Tooltip("分支描述，用于选择界面显示")]
        public string branchDescription;
        
        [Tooltip("该分支引导到的下一个任务ID")]
        public string nextTaskId;
        
        [Tooltip("选择此分支对属性的影响 (可选，Key为属性名，Value为增量)")]
        public List<StatImpact> statImpacts;
    }

    [Serializable]
    public class StatImpact
    {
        public string statName; // 如 "Sanity"
        public float amount;    // 如 -10
    }

    [Serializable]
    public class TaskData
    {
        [Header("基础信息")]
        public string taskId;
        public string taskName;
        [TextArea(3, 5)]
        public string description_Zh;
        [TextArea(3, 5)]
        public string description_En;

        [Header("进展控制")]
        public int currentProgress;
        public int totalProgress = 1;

        [Header("状态")]
        public TaskStatus status = TaskStatus.NotStarted;

        [Header("分支与后续")]
        [Tooltip("如果该列表为空，任务完成后可能直接结束或按 ID 顺序跳转")]
        public List<TaskBranch> branches;

        [Tooltip("默认的下一个任务ID (如果不涉及到复杂分支选择)")]
        public string defaultNextTaskId;

        public bool IsCompleted => currentProgress >= totalProgress;
    }
}
