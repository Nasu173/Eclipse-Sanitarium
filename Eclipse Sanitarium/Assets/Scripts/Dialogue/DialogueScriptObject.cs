using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "DialogueSystem/Dialogue")]
public class DialogueScriptObject : ScriptableObject
{
    [Tooltip("对话ID，用于识别不同对话")]
    public int dialogueId;

    [Tooltip("中文对话内容列表")]
    public List<DialogueLine> dialogueLines_Ch = new List<DialogueLine>();

    [Tooltip("英文对话内容列表")]
    public List<DialogueLine> dialogueLines_En = new List<DialogueLine>();

    [Tooltip("对话结束后是否自动关闭UI")]
    public bool closeOnComplete = true;

    [Tooltip("对话结束后触发任务完成")]
    public bool CurrentTaskWillComplete;

    [Header("限定完成的指定任务")]
    [Tooltip("【防止提前/乱完成任务】指定只有满足这个任务正进行中才会完成该任务（例如你只想这句对话结束特定的找钥匙任务）。留空的话它就会像以前一样无条件地尝试完成当前的任意任务。")]
    public TaskData targetTaskToComplete;
}
