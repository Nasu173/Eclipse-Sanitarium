using UnityEngine;

// 注意这里：继承 MonoBehaviour 的同时，实现 IInteractable 接口
public class TestItem : MonoBehaviour, IInteractable
{
    public string itemName = "生锈的钥匙";

    // 用于演示高亮的材质替换（正式项目通常用 Outline Shader，这里用变色代替）
    private Renderer _renderer;
    private Color _originalColor;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
    }

    // 实现接口：返回 UI 提示
    public string GetInteractPrompt()
    {
        return "拾取 " + itemName;
    }

    // 实现接口：按下 E 键的逻辑
    public void OnInteract()
    {
        Debug.Log("你捡起了：" + itemName);
        // 实际开发中这里会调用 Backpack.AddItem(this)
        // 然后销毁场景中的这个物体
        Destroy(gameObject);
    }

    // 实现接口：高亮逻辑
    public void ToggleHighlight(bool isHighlighted)
    {
        if (isHighlighted)
        {
            // 视线扫过时变成黄色
            _renderer.material.color = Color.yellow;
        }
        else
        {
            // 视线移开时恢复原色
            _renderer.material.color = _originalColor;
        }
    }
}