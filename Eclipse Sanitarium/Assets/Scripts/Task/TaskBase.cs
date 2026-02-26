using System;
using System.Collections.Generic;
using UnityEngine;

// --- 核心枚举与基础类 ---

public enum TaskStatus { NotStarted, InProgress, Completed, Failed }

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
        Debug.Log($"检查物品: {itemID} x {amount}");
        return true; 
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
        Debug.Log($"检查属性: {statName} 是否在 {minValue}~{maxValue}");
        return true; 
    }
}

[Serializable]
public class DialogueCondition : TaskCondition
{
    public string dialogueID;
    public bool mustBeFullyRead = true;

    public override bool IsMet()
    {
        Debug.Log($"检查对话完成情况: {dialogueID}");
        return true; 
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
    }
}

[Serializable]
public class WorldEventAction : TaskAction
{
    public string eventName;
    public override void Execute()
    {
        Debug.Log($"<color=yellow>世界事件触发: {eventName}</color>");
    }
}

[Serializable]
public class GiveItemAction : TaskAction
{
    public string itemID;
    public int amount = 1;

    public override void Execute()
    {
        Debug.Log($"<color=lime>获得物品: {itemID} x {amount}</color>");
    }
}

[Serializable]
public class DialogueAction : TaskAction
{
    public string dialogueID;

    public override void Execute()
    {
        Debug.Log($"<color=lightblue>执行对话: {dialogueID}</color>");
    }
}
