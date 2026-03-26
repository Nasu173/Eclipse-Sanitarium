using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Tooltip("说话人名字")]
    public string speakerName;

    [Tooltip("对话内容")]
    [TextArea(3, 5)] // 多行输入框，方便编辑长文本
    public string dialogueText;

    [Tooltip("该对话是否需要等待玩家点击后才继续")]
    public bool waitForInput = true;

    [Header("对话控制")]
    [Tooltip("自动继续的延迟时间（当waitForInput=false时生效）")]
    public float autoContinueDelay = 1f;
}