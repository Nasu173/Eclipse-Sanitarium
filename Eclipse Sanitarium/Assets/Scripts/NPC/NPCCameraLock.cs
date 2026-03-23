using System.Collections;
using UnityEngine;

public class NPCCameraLock : MonoBehaviour
{
    [Header("镜头对焦设置")]
    public Transform focusTarget;             // 聚焦的目标（没填则默认看向自己）
    public Vector3 cameraOffset = new Vector3(0, 1.5f, -2f);  // 镜头偏移量
    public float targetFOV = 45f;             // 对话时的视野
    public float transitionTime = 0.5f;       // 镜头过渡时间

    private Camera _playerCamera;
    private MonoBehaviour _playerMovement;    // 玩家移动控制脚本
    private MonoBehaviour _playerLook;        // 玩家视角控制脚本
    private DialogueManager _dialogueManager; // 对话管理器引用

    // 保存原始状态
    private float _originalFOV;
    private Quaternion _originalCameraRot;
    private Vector3 _originalCameraPos;
    private bool _isLocking = false;

    void Start()
    {
        _playerCamera = Camera.main;
        _dialogueManager = DialogueManager.Instance;

        if (focusTarget == null) focusTarget = transform;
    }

    /// <summary>
    /// 开始锁定（对话开始时调用）
    /// </summary>
    public void StartLock()
    {
        if (_isLocking || _playerCamera == null) return;

        // 获取玩家控制组件
        GameObject player = _playerCamera.transform.root.gameObject;
        _playerMovement = player.GetComponent("FirstPersonController") as MonoBehaviour;
        _playerLook = _playerCamera.GetComponent("FirstPersonLook") as MonoBehaviour;

        StartCoroutine(LockCoroutine());
    }

    private IEnumerator LockCoroutine()
    {
        _isLocking = true;

        // 1. 冻结玩家的移动和转头
        if (_playerMovement != null) _playerMovement.enabled = false;
        if (_playerLook != null) _playerLook.enabled = false;

        // 2. 记录初始状态
        _originalFOV = _playerCamera.fieldOfView;
        _originalCameraRot = _playerCamera.transform.rotation;
        _originalCameraPos = _playerCamera.transform.position;

        // 3. 计算目标位置（焦点目标前方）
        Vector3 targetPosition = focusTarget.position + focusTarget.TransformDirection(cameraOffset);

        // 4. 计算目标旋转（看向焦点目标）
        Vector3 directionToTarget = (focusTarget.position - targetPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // 5. 平滑推镜头
        float elapsedTime = 0f;
        Vector3 startPosition = _playerCamera.transform.position;
        Quaternion startRotation = _playerCamera.transform.rotation;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;

            // 平滑移动位置
            _playerCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            // 平滑旋转
            _playerCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            // 平滑改变视野
            _playerCamera.fieldOfView = Mathf.Lerp(_originalFOV, targetFOV, t);

            yield return null;
        }

        // 确保最终状态准确
        _playerCamera.transform.position = targetPosition;
        _playerCamera.transform.rotation = targetRotation;
        _playerCamera.fieldOfView = targetFOV;

        // 6. 等待对话结束
        yield return StartCoroutine(WaitForDialogueEnd());

        // 7. 对话结束后恢复
        StartCoroutine(UnlockCoroutine());
    }

    /// <summary>
    /// 等待对话结束
    /// </summary>
    private IEnumerator WaitForDialogueEnd()
    {
        // 等待直到对话结束
        while (_dialogueManager != null && _dialogueManager._isDialogueActive)
        {
            yield return null;
        }
    }

    private IEnumerator UnlockCoroutine()
    {
        // 1. 平滑恢复原始视野和位置
        float elapsedTime = 0f;
        Vector3 startPosition = _playerCamera.transform.position;
        Quaternion startRotation = _playerCamera.transform.rotation;
        float startFOV = _playerCamera.fieldOfView;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;

            _playerCamera.transform.position = Vector3.Lerp(startPosition, _originalCameraPos, t);
            _playerCamera.transform.rotation = Quaternion.Slerp(startRotation, _originalCameraRot, t);
            _playerCamera.fieldOfView = Mathf.Lerp(startFOV, _originalFOV, t);

            yield return null;
        }

        // 确保完全恢复
        _playerCamera.transform.position = _originalCameraPos;
        _playerCamera.transform.rotation = _originalCameraRot;
        _playerCamera.fieldOfView = _originalFOV;

        // 2. 归还玩家控制权
        if (_playerMovement != null) _playerMovement.enabled = true;
        if (_playerLook != null)
        {
            // 调用SyncRotation，防止恢复控制瞬间镜头抽搐
            _playerLook.Invoke("SyncRotation", 0f);
            _playerLook.enabled = true;
        }

        _isLocking = false;
    }
}