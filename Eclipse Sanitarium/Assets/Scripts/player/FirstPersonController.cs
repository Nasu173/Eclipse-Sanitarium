using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("移动速度参数")]
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 6.0f;
    public float crouchSpeed = 1.5f;

    [Header("跳跃参数")]
    public bool canJump = true;
    public float jumpHeight = 1.2f;

    [Header("跳跃手感优化 (高级)")]
    [Tooltip("按键缓冲：落地前多久按下空格能被系统记住（提升连跳顺滑度）")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Tooltip("土狼时间：离开地面后或走下台阶时，有多久的宽限期可以起跳（防失灵）")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("下蹲参数")]
    public float standingHeight = 2.0f;
    public float crouchingHeight = 1.0f;
    public Transform cameraTransform;
    private float defaultCameraY;

    [Header("物理与滞空时间")]
    public float gravity = -15.0f;
    private Vector3 velocity;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
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
        UpdateJumpTimers(); // 【新增】更新手感优化计时器

        HandleMovement();
        HandleCrouch();
        HandleJump();
        ApplyGravity();
    }

    // --- 核心手感优化：计时器系统 ---
    private void UpdateJumpTimers()
    {
        // 1. 土狼时间计时器 (判断是否刚离开地面)
        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime; // 只要踩在地上，就充满宽限时间
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; // 离开地面，开始倒计时
        }

        // 2. 跳跃缓冲计时器 (判断最近是否按过空格)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime; // 只要按了空格，就记住这个输入0.2秒
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime; // 没按空格，开始倒计时遗忘
        }
    }

    // --- 逻辑分块 1：前后左右移动 ---
    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }

        Vector3 moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    // --- 逻辑分块 2：处理下蹲 ---
    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            controller.height = crouchingHeight;
            cameraTransform.localPosition = new Vector3(0, defaultCameraY * 0.5f, 0);
        }
        else
        {
            controller.height = standingHeight;
            cameraTransform.localPosition = new Vector3(0, defaultCameraY, 0);
        }

        controller.center = new Vector3(0, controller.height / 2, 0);
    }

    // --- 逻辑分块 3：处理跳跃 ---
    private void HandleJump()
    {
        // 判定条件改变：
        // 只要 1.允许跳跃 2.不在下蹲 3.按键缓冲内(刚按了空格) 4.土狼时间内(刚离开地面)
        if (canJump && !Input.GetKey(KeyCode.LeftControl) && jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            // 执行起跳
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // 【极其关键】起跳后立刻清空两个计时器，防止在空中无限连跳！
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    // --- 逻辑分块 4：处理重力 ---
    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}