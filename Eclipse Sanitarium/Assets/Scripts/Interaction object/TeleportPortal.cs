using UnityEngine;
using System.Collections;
using ProjectUI; // 确保引用了 ScreenFader 所在的命名空间

public class TeleportPortal : MonoBehaviour, IInteractable
{
    [Header("传送设置")]
    public Transform destination;        // 传送目的地
    public string promptText = "进入";    // 交互提示语
    public float fadeDuration = 0.8f;    // 渐变耗时
    public float stayBlackTime = 0.5f;   // 黑屏停留时间（用于缓冲）

    private bool _isTeleporting = false;

    // --- IInteractable 接口实现 ---

    public string GetInteractPrompt()
    {
        return _isTeleporting ? "" : promptText;
    }

    public void OnInteract()
    {
        if (_isTeleporting || destination == null) return;

        StartCoroutine(TeleportRoutine());
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        // 如果物体有高亮逻辑（如描边组件），可以在这里开启/关闭
        // 这里的实现视项目具体高亮方案而定
    }

    // --- 核心传送流程 ---

    private IEnumerator TeleportRoutine()
    {
        _isTeleporting = true;

        // 1. 获取玩家组件引用
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // 如果没加 Tag，尝试通过组件寻找
            var controllerFind = FindObjectOfType<FirstPersonController>();
            if (controllerFind != null) player = controllerFind.gameObject;
        }

        if (player == null)
        {
            Debug.LogError("TeleportPortal: 未找到玩家对象！请确保玩家带有 'Player' 标签或挂载了 FirstPersonController。");
            _isTeleporting = false;
            yield break;
        }

        var moveCtrl = player.GetComponent<FirstPersonController>();
        var lookCtrl = player.GetComponentInChildren<FirstPersonLook>();
        var interactor = player.GetComponentInChildren<PlayerInteractor>();
        var charController = player.GetComponent<CharacterController>();

        // 2. 锁定操作
        if (moveCtrl != null) moveCtrl.enabled = false;
        if (lookCtrl != null) lookCtrl.enabled = false;
        if (interactor != null) interactor.SetInteractorActive(false);

        // 3. 开始淡出（变黑）
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(fadeDuration);
        }
        else
        {
            Debug.LogWarning("TeleportPortal: 场景中未发现 ScreenFader 实例。");
            yield return new WaitForSeconds(fadeDuration);
        }

        // 4. 执行传送
        // 提示：CharacterController 开启的情况下直接改 Position 有时会失效，建议暂时禁用
        if (charController != null) charController.enabled = false;
        
        player.transform.position = destination.position;
        player.transform.rotation = destination.rotation;
        
        if (charController != null) charController.enabled = true;

        // 5. 稍微停留一下，让场景加载或相机平滑
        yield return new WaitForSeconds(stayBlackTime);

        // 6. 开始淡入（变亮）
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeIn(fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // 7. 恢复操作
        if (moveCtrl != null) moveCtrl.enabled = true;
        if (lookCtrl != null) lookCtrl.enabled = true;
        if (interactor != null) interactor.SetInteractorActive(true);

        _isTeleporting = false;
    }
}
