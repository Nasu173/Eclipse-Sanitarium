using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂载到拥有 Collider (需勾选 Is Trigger) 的空物体上。
/// 玩家触碰到后，会自动判定多个任务完成或增加进度。
/// </summary>
public class TaskAreaCompleter : MonoBehaviour
{
    [Header("要完成的任务列表")]
    public List<TaskData> targetTasks = new List<TaskData>();

    [Header("增加的进度量通常是1")]
    public int progressAmount = 1;

    [Header("是否只触发一次？")]
    public bool triggerOnce = true;

    [Header("判定触碰对象的标签配置")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 1. 如果已经触发过了，且要求只触发一次，则无视
        if (triggerOnce && hasTriggered) return;

        // 2. 只有带此Tag的物体（默认是Player玩家）碰到才会起效
        if (other.CompareTag(playerTag))
        {
            if (TaskManager.Instance != null)
            {
                // 3. 遍历列表里所有的任务都进行完成判定
                foreach (var task in targetTasks)
                {
                    if (task != null)
                    {
                        TaskManager.Instance.UpdateProgress(task, progressAmount);
                    }
                }
            }
            
            hasTriggered = true;
        }
    }
}
