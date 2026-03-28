using System.Collections;
using UnityEngine;

// 强制要求挂载此脚本的物体必须有 AudioSource 音频组件
[RequireComponent(typeof(AudioSource))]
public class ProximityTrigger : MonoBehaviour
{
    [Header("触发设置")]
    public bool triggerOnce = true;           // 是否只触发一次
    private bool _hasTriggered = false;

    [Header("镜头对焦设置")]
    public Transform focusTarget;             // 聚焦的具体位置（没填则默认看向自己）
    public float targetFOV = 40f;             // 放大时的视野（数值越小放得越大）
    public float transitionTime = 1f;         // 镜头移动和缩放的平滑过渡时间（秒）

    [Header("演出时长与音乐控制")]
    public bool useCustomDuration = true;     // 是否使用自定义演出时长？(勾选则不看音频原长)
    public float customDuration = 2f;         // 强制观看几秒？(对应表格里的"持续2秒")
    public bool stopAudioAfterFocus = true;   // 演出结束后，是否强制掐断还没播完的音效？

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (focusTarget == null) focusTarget = this.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        // 确保只有玩家碰到才会触发
        if (!_hasTriggered && other.CompareTag("Player"))
        {
            if (triggerOnce) _hasTriggered = true;

            // 开启演出协程
            StartCoroutine(FocusAndPlayRoutine(other.gameObject));
        }
    }

    private IEnumerator FocusAndPlayRoutine(GameObject player)
    {
        // 1. 获取玩家的相机和控制脚本
        Camera playerCam = player.GetComponentInChildren<Camera>();
        // 注意：这里的脚本名要改成你自己实际的脚本名，比如我们之前写的 FirstPersonController
        MonoBehaviour movementObj = player.GetComponent("FirstPersonController") as MonoBehaviour;
        // 你的鼠标视角控制脚本挂在相机上
        MonoBehaviour lookObj = playerCam.GetComponent("FirstPersonLook") as MonoBehaviour;

        // 2. 冻结玩家的移动和转头
        if (movementObj != null) movementObj.enabled = false;
        if (lookObj != null) lookObj.enabled = false;

        // 3. 记录初始状态，用于稍后恢复
        float originalFOV = playerCam.fieldOfView;
        Quaternion originalCamRot = playerCam.transform.rotation;

        // 计算目标旋转角度：让相机精准看向目标物体
        Vector3 directionToTarget = (focusTarget.position - playerCam.transform.position).normalized;
        Quaternion targetCamRot = Quaternion.LookRotation(directionToTarget);

        // ==========================================
        // 演出阶段 1：平滑推镜头，强行扭脖子
        // ==========================================
        float elapsedTime = 0f;
        while (elapsedTime < transitionTime)
        {
            playerCam.fieldOfView = Mathf.Lerp(originalFOV, targetFOV, elapsedTime / transitionTime);
            playerCam.transform.rotation = Quaternion.Slerp(originalCamRot, targetCamRot, elapsedTime / transitionTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerCam.fieldOfView = targetFOV;
        playerCam.transform.rotation = targetCamRot;

        // ==========================================
        // 演出阶段 2：播放音效并强制等待 N 秒
        // ==========================================
        if (_audioSource.clip != null)
        {
            _audioSource.Play();
        }

        // 计算到底要等多久：如果勾选了自定义，就用 customDuration；否则用音频本身的长度
        float waitTime = useCustomDuration ? customDuration : (_audioSource.clip != null ? _audioSource.clip.length : 2f);

        // 强制罚站等待
        yield return new WaitForSeconds(waitTime);

        // 如果勾选了强制停止音频，且音频还在播，就把它掐断
        if (stopAudioAfterFocus && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        // ==========================================
        // 演出阶段 3：平滑恢复原始视野
        // ==========================================
        elapsedTime = 0f;
        while (elapsedTime < transitionTime)
        {
            playerCam.fieldOfView = Mathf.Lerp(targetFOV, originalFOV, elapsedTime / transitionTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        playerCam.fieldOfView = originalFOV;

        // ==========================================
        // 演出阶段 4：归还玩家控制权
        // ==========================================
        if (movementObj != null) movementObj.enabled = true;
        if (lookObj != null)
        {
            // 利用反射调用 SyncRotation，防止恢复控制瞬间镜头抽搐
            lookObj.Invoke("SyncRotation", 0f);
            lookObj.enabled = true;
        }
    }
}