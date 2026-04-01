using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWhispers", menuName = "DialogueSystem/Whispers")]
public class WhispersScriptObject : ScriptableObject
{
    [Tooltip("呓语ID，用于识别")]
    public string whispersId;

    [Tooltip("NPC名称")]
    public string npcName;

    [Tooltip("中文呓语列表（按阶段分组）")]
    public List<PhaseWhispersData> phaseWhispers_Ch;

    [Tooltip("英文呓语列表（按阶段分组）")]
    public List<PhaseWhispersData> phaseWhispers_En;
}

/// <summary>
/// 阶段呓语数据
/// </summary>
[System.Serializable]
public class PhaseWhispersData
{
    [Tooltip("阶段索引（0=人类, 1=初期异化, 2=中期转化, 3=完全植物化）")]
    public int phaseIndex;

    [Tooltip("该阶段的呓语列表（循环播放）")]
    [TextArea(2, 4)]
    public List<string> whisperLines;
}