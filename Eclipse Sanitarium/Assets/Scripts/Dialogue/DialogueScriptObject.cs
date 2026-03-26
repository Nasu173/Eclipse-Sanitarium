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
}
