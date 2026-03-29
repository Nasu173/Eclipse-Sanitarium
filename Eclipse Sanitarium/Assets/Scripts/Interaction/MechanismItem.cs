using UnityEngine;
using UnityEngine.Events;

public class MechanismItem : MonoBehaviour, IInteractable
{
    [Header("交互提示 (中英双语)")]
    public string interactPrompt_Ch = "操作 控制台";
    public string interactPrompt_En = "Operate Console"; // 【新增】

    [Header("机关设置")]
    public bool isOneTimeUse = false;
    private bool _hasBeenUsed = false;

    [Header("触发事件")]
    public UnityEvent onInteractEvent;

    private Outline _outline;

    void Start()
    {
        _outline = GetComponent<Outline>();
        if (_outline == null) _outline = gameObject.AddComponent<Outline>();

        _outline.OutlineMode = Outline.Mode.OutlineAll;
        _outline.OutlineColor = new Color(1f, 1f, 1f, 0.5f);
        _outline.OutlineWidth = 3f;
        _outline.enabled = false;
    }

    public string GetInteractPrompt()
    {
        if (isOneTimeUse && _hasBeenUsed) return "";

        // 【核心修改】动态返回对应语言的按键提示
        bool isEn = GlobalLanguage.Instance != null && GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En;
        return isEn ? interactPrompt_En : interactPrompt_Ch;
    }

    public void OnInteract()
    {
        if (isOneTimeUse && _hasBeenUsed) return;

        if (isOneTimeUse)
        {
            _hasBeenUsed = true;
            if (_outline != null) _outline.enabled = false;
        }

        onInteractEvent?.Invoke();
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        if (isOneTimeUse && _hasBeenUsed)
        {
            if (_outline != null) _outline.enabled = false;
            return;
        }
        if (_outline != null) _outline.enabled = isHighlighted;
    }
}