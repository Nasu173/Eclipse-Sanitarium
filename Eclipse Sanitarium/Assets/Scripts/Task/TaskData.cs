using System;
using System.Collections.Generic;
using UnityEngine;

// --- 主任务数据 SO ---
[CreateAssetMenu(fileName = "NewTask", menuName = "Eclipse/Task System/Advanced Task")]
public class TaskData : ScriptableObject
{
    [Header("基础说明")]
    public string taskId;
    public string taskName;
    [TextArea(3, 5)] public string description_Zh;

    [Header("完成条件 (所有条件满足才算完成)")]
    [SerializeReference, SubclassSelector] public List<TaskCondition> completionConditions = new List<TaskCondition>();

    [Header("开始时的行为")]
    [SerializeReference, SubclassSelector] public List<TaskAction> onStartActions = new List<TaskAction>();

    [Header("完成时的行为")]
    [SerializeReference, SubclassSelector] public List<TaskAction> onCompleteActions = new List<TaskAction>();

    [Header("状态 (运行时)")]
    public TaskStatus status = TaskStatus.NotStarted;
    public int currentProgress;
    public int totalProgress = 1;

    public bool IsCompleted => currentProgress >= totalProgress;

    [Header("后续跳转")]
    public List<TaskBranch> branches = new List<TaskBranch>();
    public TaskData defaultNextTask;

    public void ResetTask()
    {
        status = TaskStatus.NotStarted;
        currentProgress = 0;
    }

    public bool CheckConditions()
    {
        if (completionConditions.Count == 0) return true;
        foreach (var cond in completionConditions)
        {
            if (!cond.IsMet()) return false;
        }
        return true;
    }
}

[Serializable]
public class TaskBranch
{
    public string branchName;
    public TaskData nextTask;
    [SerializeReference, SubclassSelector] public List<TaskAction> branchActions = new List<TaskAction>();
}
