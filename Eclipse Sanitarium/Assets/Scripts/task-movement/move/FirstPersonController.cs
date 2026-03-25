using UnityEngine;

// 强制要求有 CharacterController 组件
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("移动速度参数")]
    public float walkSpeed = 3.0f;     // 行走速度
    public float sprintSpeed = 6.0f;   // 奔跑速度
    public float crouchSpeed = 1.5f;   // 下蹲时的移动速度

    [Header("下蹲参数")]
    public float standingHeight = 2.0f;     // 站立时胶囊体的高度
    public float crouchingHeight = 1.0f;    // 下蹲时胶囊体的高度
    public Transform cameraTransform;       // 玩家头部相机的 Transform 组件
    private float defaultCameraY;           // 记录相机默认的高度

    [Header("物理与重力")]
    public float gravity = -9.81f;          // 模拟真实世界的重力加速度
    private Vector3 velocity;               // 记录当前角色在 Y 轴（上下）的速度

    // 内部组件引用
    private CharacterController controller;

    void Start()
    {
        // 获取 CharacterController 组件
        controller = GetComponent<CharacterController>();
        // 记录相机初始的相对高度
        defaultCameraY = cameraTransform.localPosition.y;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += DisableMovement;
            DialogueManager.Instance.OnDialogueEnded += EnableMovement;
        }
    }

    private void DisableMovement() { this.enabled = false; }
    private void EnableMovement() { if (!NPCCameraLock.IsCameraLockActive) this.enabled = true; }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= DisableMovement;
            DialogueManager.Instance.OnDialogueEnded -= EnableMovement;
        }
    }

    void Update()
    {

        HandleMovement();
        HandleCrouch();
        ApplyGravity();
    }

    // --- 逻辑分块 1：前后左右移动与奔跑 ---
    private void HandleMovement()
    {
        // 1. 获取输入
        float x = Input.GetAxis("Horizontal"); //A/D
        float z = Input.GetAxis("Vertical"); //W/S

        // 2. 决定当前速度：按住了左Shift，是奔跑速度；否则是走路速度
        // （如果正在下蹲，强制用下蹲速度）
        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }

        // 3. 计算方向：局部右方向 * x + 局部前方向 * z
        Vector3 moveDirection = transform.right * x + transform.forward * z;

        // 4. 执行移动：方向 * 速度 * 帧时间
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    // --- 逻辑分块 2：处理下蹲 ---
    private void HandleCrouch()
    {

        // 如果按住了左 Ctrl 键
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // 把胶囊体的高度砍半
            controller.height = crouchingHeight;
            // 降低相机的高度
            cameraTransform.localPosition = new Vector3(0, defaultCameraY * 0.5f, 0);
        }
        else
        {
            // 松开 Ctrl 时，恢复站立高度和相机高度
            controller.height = standingHeight;
            cameraTransform.localPosition = new Vector3(0, defaultCameraY, 0);
        }

        // 改变了高度后，把胶囊体的中心点重新对齐到底部
        controller.center = new Vector3(0, controller.height / 2, 0);
    }

    // --- 逻辑分块 3：处理重力 ---
    private void ApplyGravity()
    {
        // 如果玩家已经踩在地上，并且 Y 轴速度还在往下掉
        if (controller.isGrounded && velocity.y < 0)
        {
            // 给一个微小的向下力，确保玩家紧贴地面
            velocity.y = -2f;
        }

        // v = g * t （重力加速度不断累加到 Y 轴速度上）
        velocity.y += gravity * Time.deltaTime;

        // 执行 Y 轴方向的移动
        controller.Move(velocity * Time.deltaTime);
    }
}