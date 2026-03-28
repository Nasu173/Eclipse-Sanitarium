using System.Collections.Generic;
using UnityEngine;
using TMPro; // 【新增】引入 TextMeshPro 命名空间

public class PlayerInteractor : MonoBehaviour
{
    [Header("视觉与高亮 (远距离)")]
    public float visionRange = 8f;            // 视线能及的最远距离（高亮范围）
    [Range(30f, 180f)]
    public float fieldOfView = 90f;           // 玩家的视野角度（通常设为90度左右）
    public LayerMask interactableLayer;       // 互动物品的图层

    [Header("操作与 UI (近距离)")]
    public float interactRange = 2.5f;        // 必须靠近到多近才能按 E
    public TextMeshProUGUI promptText;        // 【修改】使用 TextMeshProUGUI 组件

    private Camera _mainCam;
    private IInteractable _interactionTarget; // 当前准心对准、可以按 E 的目标

    // 内部列表：用来记录当前画面里有哪些物体正在发光
    private List<IInteractable> _highlightedObjects = new List<IInteractable>();
    private List<IInteractable> _visibleThisFrame = new List<IInteractable>();

    void Start()
    {
        _mainCam = GetComponent<Camera>();
        promptText.gameObject.SetActive(false);

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += DisableInteractor;
            DialogueManager.Instance.OnDialogueEnded += EnableInteractor;
        }
    }

    private void DisableInteractor() => SetInteractorActive(false);
    private void EnableInteractor() => SetInteractorActive(true);

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= DisableInteractor;
            DialogueManager.Instance.OnDialogueEnded -= EnableInteractor;
        }
    }

    void Update()
    {
        HandleHighlightsInFOV();
        HandleInteractionRaycast();
    }

    // ==========================================
    // 系统 1：广角雷达（负责视野内的高亮）
    // ==========================================
    private void HandleHighlightsInFOV()
    {
        _visibleThisFrame.Clear();

        // 1. 获取玩家周围一定半径内的所有互动物体
        Collider[] hitColliders = Physics.OverlapSphere(_mainCam.transform.position, visionRange, interactableLayer);

        foreach (var hitCol in hitColliders)
        {
            // 计算相机到物体的方向
            Vector3 dirToTarget = (hitCol.transform.position - _mainCam.transform.position).normalized;

            // 2. 检查物体是否在视野夹角内
            if (Vector3.Angle(_mainCam.transform.forward, dirToTarget) < fieldOfView / 2f)
            {
                // 3. 视线遮挡检测：发射一条射线看看中间有没有墙壁挡着
                // （如果不加这个，隔着墙壁物体也会发光）
                if (Physics.Raycast(_mainCam.transform.position, dirToTarget, out RaycastHit sightHit, visionRange))
                {
                    // 如果射线第一个打到的就是这个互动物体本身（没有被墙挡住）
                    if (sightHit.collider == hitCol)
                    {
                        IInteractable interactable = hitCol.GetComponent<IInteractable>();
                        if (interactable != null)
                        {
                            _visibleThisFrame.Add(interactable);
                        }
                    }
                }
            }
        }

        // 【终极防报错清理】先把列表中已经被 Destroy 的物体剔除掉
        _highlightedObjects.RemoveAll(item => (item as UnityEngine.Object) == null);

        // 4. 对比上一帧，如果物体离开了视野，关闭高亮
        foreach (var oldObj in _highlightedObjects)
        {
            if (!_visibleThisFrame.Contains(oldObj))
            {
                oldObj.ToggleHighlight(false);
            }
        }

        // 5. 如果是新进入视野的物体，开启高亮
        foreach (var newObj in _visibleThisFrame)
        {
            if (!_highlightedObjects.Contains(newObj))
            {
                newObj.ToggleHighlight(true);
            }
        }

        // 6. 更新记录，供下一帧对比
        _highlightedObjects.Clear();
        _highlightedObjects.AddRange(_visibleThisFrame);
    }

    // ==========================================
    // 系统 2：准心射线（负责近距离弹出 UI 和按 E）
    // ==========================================
    private void HandleInteractionRaycast()
    {
        // 从屏幕正中心发射一条【短】射线
        Ray ray = new Ray(_mainCam.transform.position, _mainCam.transform.forward);

        // 注意这里的距离换成了 interactRange (比如 2.5米)
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // 记录为当前可交互目标，并显示 TMPro UI
                _interactionTarget = interactable;
                promptText.text = "[E] " + _interactionTarget.GetInteractPrompt();
                promptText.gameObject.SetActive(true);

                // 按下 E 键执行操作
                if (Input.GetKeyDown(KeyCode.E))
                {
                    _interactionTarget.OnInteract();

                    // 保护机制：如果物品被拾取后销毁了，立刻清空 UI
                    if ((_interactionTarget as UnityEngine.Object) == null)
                    {
                        ClearInteractionTarget();
                    }
                }
                return; // 成功找到交互目标，直接结束函数
            }
        }

        // 如果射线没有打到互动物品，或者距离太远
        ClearInteractionTarget();
    }

    private void ClearInteractionTarget()
    {
        if (_interactionTarget != null)
        {
            _interactionTarget = null;
            promptText.gameObject.SetActive(false);
        }
    }

    // --------------------------------------------------------
    // 提供给外部（比如 UI 管理器）调用的休眠开关
    // --------------------------------------------------------
    public void SetInteractorActive(bool isActive)
    {
        // 开启或关闭这个脚本的 Update 运行
        this.enabled = isActive;

        // 如果是关闭状态，主动打扫卫生
        if (!isActive)
        {
            // 1. 强制清空近距离的准心目标和 TMPro 提示
            ClearInteractionTarget();

            // 2. 把远处还在高亮的物体也全部熄灭
            foreach (var obj in _highlightedObjects)
            {
                SafeToggleHighlight(obj, false);
            }
            _highlightedObjects.Clear();
            _visibleThisFrame.Clear();
        }
    }
    // --------------------------------------------------------
    // 【终极防报错神器】：专门处理接口物体的高亮开关 
    // --------------------------------------------------------
    private void SafeToggleHighlight(IInteractable target, bool state)
    {
        if (target == null) return;

        // 重点：将接口强制转换为 UnityEngine.Object
        // 只有这样，Unity 底层重载的 "==" 运算符才会生效，真正去检查 C++ 对象是否已经被 Destroy
        if (target as UnityEngine.Object != null)
        {
            target.ToggleHighlight(state);
        }
    }
}