using System;
using System.Collections.Generic;
using UnityEngine;

namespace EclipseSanitarium.TaskSystem
{
    // --- 核心枚举与基础类 ---

    public enum TaskStatus { NotStarted, InProgress, Completed, Failed }

    // --- 条件检查系统 (Conditions) ---
    // 使用 SerializeReference 可以在 Inspector 里通过 Add 按钮选择不同的子类
    [Serializable]
    public abstract class TaskCondition
    {
        public string description;
        public abstract bool IsMet();
    }

    [Serializable]
    public class ItemCondition : TaskCondition
    {
        public string itemID;
        public int amount = 1;

        public override bool IsMet()
        {
            // TODO: 对接背包系统 InventoryManager.Instance.HasItem(itemID, amount)
            Debug.Log($"检查物品: {itemID} x {amount}");
            return true; // 占位
        }
    }

    [Serializable]
    public class StatCondition : TaskCondition
    {
        public string statName = "Sanity";
        public float minValue = 0;
        public float maxValue = 100;

        public override bool IsMet()
        {
            // TODO: 对接属性系统 PlayerStats.Instance.GetStat(statName)
            Debug.Log($"检查属性: {statName} 是否在 {minValue}~{maxValue}");
            return true; // 占位
        }
    }

    // --- 行为触发系统 (Actions) ---
    [Serializable]
    public abstract class TaskAction
    {
        public abstract void Execute();
    }

    [Serializable]
    public class StatAction : TaskAction
    {
        public string statName = "Sanity";
        public float changeAmount;

        public override void Execute()
        {
            Debug.Log($"<color=orange>属性变更: {statName} += {changeAmount}</color>");
            // TODO: 对接属性系统 PlayerStats.Instance.ModifyStat(statName, changeAmount)
        }
    }

    [Serializable]
    public class WorldEventAction : TaskAction
    {
        public string eventName;
        public override void Execute()
        {
            Debug.Log($"<color=yellow>世界事件触发: {eventName}</color>");
            // 可以在这里广播给音效、灯光或剧情控制器
        }
    }

    // --- 主任务数据 SO ---
    [CreateAssetMenu(fileName = "NewTask", menuName = "Eclipse/Task System/Advanced Task")]
    public class TaskData : ScriptableObject
    {
        [Header("基础说明")]
        public string taskId;
        public string taskName;
        [TextArea(3, 5)] public string description_Zh;

        [Header("完成条件 (所有条件满足才算完成)")]
        [SerializeReference] public List<TaskCondition> completionConditions = new List<TaskCondition>();

        [Header("开始时的行为")]
        [SerializeReference] public List<TaskAction> onStartActions = new List<TaskAction>();

        [Header("完成时的行为")]
        [SerializeReference] public List<TaskAction> onCompleteActions = new List<TaskAction>();

        [Header("状态 (运行时)")]
        public TaskStatus status = TaskStatus.NotStarted;
        public int currentProgress;
        public int totalProgress = 1;

        public bool IsCompleted => currentProgress >= totalProgress;

        [Header("后续跳转")]
        public List<TaskBranch> branches;
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
        [SerializeReference] public List<TaskAction> branchActions;
    }
}
