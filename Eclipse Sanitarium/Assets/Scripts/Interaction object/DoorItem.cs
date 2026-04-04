using UnityEngine;
using System.Collections;

public class DoorItem : MonoBehaviour, IInteractable
{
    [Header("门旋转设置 / Door Settings")]
    [Tooltip("门旋转的轴心物体。你可以在门（Cube）下面创建一个空物体，把它移动到门轴的边缘，然后拖入此框中。如果不填，默认以门自身的中心点旋转。")]
    public Transform hingeTransform;
    
    [Tooltip("每次打开旋转的角度 (绕自身轴)")]
    public float openAngle = 90f;
    
    [Tooltip("开门/关门的速度")]
    public float rotationSpeed = 5f;

    [Header("交互提示 / Interact Prompts")]
    public string openPrompt_Ch = "打开门";
    public string closePrompt_Ch = "关闭门";
    public string openPrompt_En = "Open Door";
    public string closePrompt_En = "Close Door";

    [Header("系统设置")]
    public bool enableOutline = true; // 是否启用描边高亮

    private Outline _outline;
    private bool isOpen = false;
    private bool isAnimating = false;

    // 记录精准的世界坐标和旋转，防止多次开关产生浮点误差导致的偏移
    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private Vector3 openPosition;
    private Quaternion openRotation;

    private float currentAngle = 0f;

    void Start()
    {
        // 初始化描边逻辑
        if (enableOutline)
        {
            _outline = GetComponent<Outline>();
            if (_outline == null) _outline = gameObject.AddComponent<Outline>();

            _outline.OutlineMode = Outline.Mode.OutlineAll;
            _outline.OutlineColor = new Color(1f, 1f, 1f, 0.5f);
            _outline.OutlineWidth = 3f;
            _outline.enabled = false;
        }

        // 如果没有指定转轴，则以门自身作为转轴
        if (hingeTransform == null)
        {
            hingeTransform = transform; 
        }

        // ===== 计算并记录关门和开门的绝对位置和旋转 =====
        closedPosition = transform.position;
        closedRotation = transform.rotation;

        // 临时将门旋转到打开的状态
        transform.RotateAround(hingeTransform.position, Vector3.up, openAngle);
        
        // 记录打开状态下的绝对位置和旋转
        openPosition = transform.position;
        openRotation = transform.rotation;

        // 将门恢复到关闭时的位置
        transform.position = closedPosition;
        transform.rotation = closedRotation;
    }

    public string GetInteractPrompt()
    {
        bool isEn = GlobalLanguage.Instance != null && GlobalLanguage.Instance.currentLanguageType == GlobalLanguage.LanguageType.En;
        
        if (isOpen)
        {
            return isEn ? closePrompt_En : closePrompt_Ch;
        }
        else
        {
            return isEn ? openPrompt_En : openPrompt_Ch;
        }
    }

    public void OnInteract()
    {
        if (isAnimating) return; 

        isOpen = !isOpen;
        StartCoroutine(AnimateDoor());
    }

    public void ToggleHighlight(bool isHighlighted)
    {
        if (_outline != null) _outline.enabled = isHighlighted;
    }

    private IEnumerator AnimateDoor()
    {
        isAnimating = true;

        float targetAngle = isOpen ? openAngle : 0f;
        
        // 使用插值平滑改变角度
        while (Mathf.Abs(targetAngle - currentAngle) > 0.01f)
        {
            float prevAngle = currentAngle;
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
            float angleDelta = currentAngle - prevAngle;

            // 围绕轴心物体的世界垂直轴（Up）旋转相应的差值角度，确保不受空物体歪斜干扰
            transform.RotateAround(hingeTransform.position, Vector3.up, angleDelta);
            
            yield return null;
        }

        // 动画结束时直接对齐到预先计算好的精准坐标，彻底消除因为多次旋转造成的浮点漂移误差
        transform.position = isOpen ? openPosition : closedPosition;
        transform.rotation = isOpen ? openRotation : closedRotation;
        currentAngle = targetAngle; // 确保当前的累计角度一致

        isAnimating = false;
    }
}
