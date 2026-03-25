using UnityEngine;

// 任何想要和玩家互动的物体（门、纸条、钥匙），都必须继承这个接口
public interface IInteractable
{
    // 1. 获取提示文本（比如返回："拾取 钥匙" 或 "阅读 日记"）
    string GetInteractPrompt();

    // 2. 玩家按下 E 键时执行的逻辑
    void OnInteract();

    // 3. 玩家视线扫过时，开启或关闭高亮表现
    void ToggleHighlight(bool isHighlighted);
}