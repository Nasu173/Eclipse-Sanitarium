using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrriger : MonoBehaviour
{
    [Tooltip("要触发的对话")]
    public DialogueScriptObject dialogueToTrigger;

    [Tooltip("下一个要触发的对话")]
    public DialogueScriptObject nextDialogueToTrigger;

    [Tooltip("触发类型")]
    public TriggerType triggerType;

    [Tooltip("是否只触发一次")]
    public bool triggerOnce = true;

    [Tooltip("解锁的空气墙")]
    public GameObject airWallToDisable;

    public bool _hasTriggered = false;

    public enum TriggerType
    {
        Interaction, // 交互触发
        Collision    // 碰撞触发
    }

    // 碰撞触发对话（可通过交互调用）
    private void OnTriggerEnter(Collider other)
    {
        if (triggerType == TriggerType.Collision && other.CompareTag("Player") &&
            !_hasTriggered && dialogueToTrigger != null)
        {
            TriggerDialogue();
        }
    }

    // 交互触发对话（通过玩家输入调用）在交互脚本中调用


    //触发对话
    public void TriggerDialogue()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("没有找到DialogueManager实例！");
            return;
        }

        if (dialogueToTrigger != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueToTrigger);
            _hasTriggered = true;
        }

        if (airWallToDisable != null)
        {
            airWallToDisable.SetActive(false);
        }
    }
}
