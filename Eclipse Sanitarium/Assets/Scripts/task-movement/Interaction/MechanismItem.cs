using UnityEngine;
using UnityEngine.Events;

public class MechanismItem : MonoBehaviour, IInteractable
{
    [Header("交互提示")]
    public string interactPrompt = "操作 控制台";

    [Header("机关设置")]
    public bool isOneTimeUse = false;
    private bool _hasBeenUsed = false;

    [Header("触发事件")]
    public UnityEvent onInteractEvent;

    // 【新增】用于存放描边组件的引用
    private Outline _outline;

    void Start()
    {
        // 尝试获取或自动添加 Outline 组件
        _outline = GetComponent<Outline>();
        if (_outline == null)
        {
            _outline = gameObject.AddComponent<Outline>();
        }

        // 初始化描边的样式
        _outline.OutlineMode = Outline.Mode.OutlineAll;
        _outline.OutlineColor = new Color(1f, 1f, 1f, 0.5f);
        _outline.OutlineWidth = 3f;

        _outline.enabled = false;
    }

    public string GetInteractPrompt()
    {
        if (isOneTimeUse && _hasBeenUsed) return "";
        return interactPrompt;
    }

    public void OnInteract()
    {
        if (isOneTimeUse && _hasBeenUsed) return;

        if (isOneTimeUse)
        {
            _hasBeenUsed = true;

            // 【新增】如果是一次性机关，按完后立刻强制熄灭高光
            if (_outline != null)
            {
                _outline.enabled = false;
            }
        }

        onInteractEvent?.Invoke();
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        // 如果是一次性机关且用过了，绝不发光
        if (isOneTimeUse && _hasBeenUsed)
        {
            if (_outline != null) _outline.enabled = false;
            return;
        }

        // 【核心修改】用开关 Outline 组件代替修改材质颜色
        if (_outline != null)
        {
            _outline.enabled = isHighlighted;
        }
    }
}