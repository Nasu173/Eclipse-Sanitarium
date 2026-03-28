using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("视角设置")]
    public float mouseSensitivity = 300f; // 鼠标灵敏度
    public Transform playerBody;          // 指向玩家根节点，用于左右旋转

    // 记录当前上下旋转的累计角度
    private float xRotation = 0f;

    void Start()
    {
        // 游戏开始时，把鼠标指针隐藏，并锁定在屏幕正中央
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += DisableLook;
            DialogueManager.Instance.OnDialogueEnded += EnableLook;
        }
    }

    private void DisableLook() { this.enabled = false; }
    private void EnableLook() 
    { 
        if (!NPCCameraLock.IsCameraLockActive) 
        {
            SyncRotation();
            this.enabled = true; 
        }
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= DisableLook;
            DialogueManager.Instance.OnDialogueEnded -= EnableLook;
        }
    }

    void Update()
    {
        // 1. 获取鼠标输入数据
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 2. 处理上下看
        xRotation -= mouseY;

        // 限制角度
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 应用相机的局部旋转
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. 处理左右看
        // 让玩家的身体根节点绕着世界坐标的 Y 轴旋转
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // --------------------------------------------------------
    // 提供给外部调用的视角同步方法（防止强制转头后视角抽搐）
    // --------------------------------------------------------
    public void SyncRotation()
    {
        // 读取当前相机真实的 X 轴局部旋转角度，并覆盖掉内部记录的值
        Vector3 currentRotation = transform.localEulerAngles;
        // 处理 Unity 角度超过 180 度的换算问题
        xRotation = currentRotation.x > 180f ? currentRotation.x - 360f : currentRotation.x;
    }
}