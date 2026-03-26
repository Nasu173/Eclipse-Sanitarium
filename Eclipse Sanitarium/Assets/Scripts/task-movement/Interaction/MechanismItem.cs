using UnityEngine;
using UnityEngine.Events; // 【核心】必须引入这个命名空间才能用 UnityEvent

public class MechanismItem : MonoBehaviour, IInteractable
{
    [Header("交互提示")]
    public string interactPrompt = "操作 控制台";

    [Header("机关设置")]
    // 很多解密机关（比如开锁）是一次性的，勾选后用过一次就不能再按了
    public bool isOneTimeUse = false;
    private bool _hasBeenUsed = false;

    [Header("触发事件")]
    // 这里的 UnityEvent 会在编辑器面板里变成一个可以无限添加列表的 UI 槽位
    public UnityEvent onInteractEvent;

    public string GetInteractPrompt()
    {
        // 如果是一次性机关且已经用过，就不再显示提示
        if (isOneTimeUse && _hasBeenUsed) return "";

        return interactPrompt;
    }

    public void OnInteract()
    {
        // 防御逻辑：用过的一次性机关直接拦截
        if (isOneTimeUse && _hasBeenUsed) return;

        if (isOneTimeUse)
        {
            _hasBeenUsed = true;
        }

        // 【核心】呼叫所有在 Inspector 面板里连线的函数
        // "?" 是 C# 的安全调用，意思是如果里面没连线，就什么都不做，防止报错
        onInteractEvent?.Invoke();
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        // 如果用过了，就不再高亮
        if (isOneTimeUse && _hasBeenUsed)
        {
            if (TryGetComponent<Renderer>(out Renderer r)) r.material.color = Color.white;
            return;
        }

        // 基础变色测试
        if (TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material.color = isHighlighted ? Color.yellow : Color.white;
        }
    }
}