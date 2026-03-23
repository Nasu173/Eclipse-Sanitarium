using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NPCComponent : MonoBehaviour, IInteractable
{
    [Header("基础信息")]
    [Tooltip("NPC唯一ID（如 'nurse_li', 'patient_201'）")]
    public string npcId;

    [Tooltip("NPC显示名称")]
    public string npcDisplayName;

    [Tooltip("NPC类型")]
    public NPCType npcType = NPCType.Patient;

    [Header("对话触发器")]
    [Tooltip("该NPC对应的对话触发器")]
    public DialogueTrriger dialogueTrigger;
    public NPCCameraLock cameraLock;

    [Header("视觉表现")]
    [Tooltip("NPC的不同阶段模型")]
    public List<PhaseModel> phaseModels;

    [Tooltip("当前阶段索引（对应病人转化阶段）")]
    public int currentPhase = 0;


    [Header("交互设置")]
    [Tooltip("交互提示文本")]
    public string interactionPrompt = "对话";

    [Tooltip("高亮组件（可选）")]
    public Outline outlineComponent;

    [Tooltip("是否启用高亮效果")]
    public bool enableHighlight = true;


    [Header("事件系统")]
    public UnityEvent onDialogueTriggered;
    public UnityEvent onPhaseChange;

    private void Start()
    {
        // 初始化模型阶段
        UpdatePhaseModel();
    }

    /// <summary>
    /// 外部调用：触发NPC对话（由PlayerInteractor调用）
    /// </summary>
    public virtual void TriggerDialogue()
    {
        if (dialogueTrigger != null)
        {
            // 直接调用触发器的TriggerDialogue方法
            dialogueTrigger.TriggerDialogue();
            onDialogueTriggered?.Invoke();

            Debug.Log($"NPC {npcDisplayName} 触发对话");
        }
        else
        {
            Debug.LogWarning($"NPC {npcId} 没有设置对话触发器");
        }
    }

    /// <summary>
    /// 更新模型阶段（用于病人转化）
    /// </summary>
    private void UpdatePhaseModel()
    {
        if (phaseModels == null || phaseModels.Count == 0) return;

        // 禁用所有阶段模型
        foreach (var phase in phaseModels)
        {
            if (phase.model != null)
            {
                phase.model.SetActive(false);
            }
        }

        // 启用当前阶段模型
        PhaseModel currentPhaseModel = phaseModels.Find(p => p.phaseIndex == currentPhase);
        if (currentPhaseModel != null && currentPhaseModel.model != null)
        {
            currentPhaseModel.model.SetActive(true);
        }
    }

    /// <summary>
    /// 手动设置NPC阶段
    /// </summary>
    public void SetPhase(int newPhase)
    {
        if (newPhase < 0 || newPhase >= phaseModels.Count) return;

        currentPhase = newPhase;
        UpdatePhaseModel();
        onPhaseChange?.Invoke();

        Debug.Log($"NPC {npcDisplayName} 进入阶段 {newPhase}");
    }

    /// <summary>
    /// 获取当前NPC的名称
    /// </summary>
    public string GetNPCName()
    {
        return npcDisplayName;
    }

    public string GetInteractPrompt()
    {
        return $"{interactionPrompt} {npcDisplayName}";
    }

    public void OnInteract()
    {
        // 开始相机锁定
        if (cameraLock != null) cameraLock.StartLock();

        // 触发对话
        dialogueTrigger.TriggerDialogue();
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        if (!enableHighlight || outlineComponent == null) return;
        outlineComponent.enabled = isHighlighted;
    }
}

// NPC类型枚举
public enum NPCType
{
    Nurse,      // 护士长/护士
    Patient,    // 病人
    Other
}

// 阶段模型类（用于病人转化）
[System.Serializable]
public class PhaseModel
{
    [Tooltip("阶段索引（0=人类, 1=初期异化, 2=中期转化, 3=完全植物化）")]
    public int phaseIndex;

    [Tooltip("该阶段的模型GameObject")]
    public GameObject model;
}