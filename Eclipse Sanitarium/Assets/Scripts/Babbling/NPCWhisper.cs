using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCWhisper : MonoBehaviour
{
    [Header("字幕框设置")]
    [Tooltip("字幕框UI预制体（或场景中的UI物体）")]
    public GameObject whisperUI;

    [Tooltip("字幕文本组件")]
    public TextMeshProUGUI whisperText;

    [Tooltip("字幕框偏移量（相对NPC位置）")]
    public Vector3 uiOffset = new Vector3(0, 2.2f, 0);

    [Header("呓语内容")]
    [Tooltip("不同阶段的呓语列表")]
    public List<PhaseWhisper> phaseWhispers;

    [Header("显示设置")]
    [Tooltip("打字机速度（字符/秒）")]
    public float typingSpeed = 20f;

    [Tooltip("每条呓语显示完后等待时间（秒）")]
    public float waitBetweenWhispers = 2f;

    [Tooltip("触发范围")]
    public float triggerRange = 3f;

    [Tooltip("是否循环播放呓语")]
    public bool loopWhispers = true;

    [Header("音效设置")]
    [Tooltip("呓语音效")]
    public AudioClip whisperSound;

    [Tooltip("音效播放间隔（秒）")]
    public float soundInterval = 0.5f;

    // 私有变量
    private Camera _mainCamera;
    private bool _isPlayerInRange = false;
    private bool _isWhispering = false;
    private int _currentWhisperIndex = 0;
    private List<string> _currentWhisperList;
    private Coroutine _whisperCoroutine;
    private AudioSource _audioSource;

    void Start()
    {
        _mainCamera = Camera.main;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        // 初始化字幕框
        if (whisperUI != null)
        {
            whisperUI.SetActive(false);
        }

        // 根据当前阶段获取呓语列表
        UpdateWhisperList();
    }

    void Update()
    {
        // 始终让字幕框面向摄像机
        if (whisperUI != null && whisperUI.activeSelf)
        {
            whisperUI.transform.LookAt(_mainCamera.transform);
            whisperUI.transform.Rotate(0, 180, 0); // 翻转，让文字正向
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_isPlayerInRange)
        {
            _isPlayerInRange = true;
            StartWhisper();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && _isPlayerInRange)
        {
            _isPlayerInRange = false;
            StopWhisper();
        }
    }

    /// <summary>
    /// 开始播放呓语
    /// </summary>
    private void StartWhisper()
    {
        if (_isWhispering) return;

        // 更新呓语列表（可能阶段已变化）
        UpdateWhisperList();

        if (_currentWhisperList == null || _currentWhisperList.Count == 0) return;

        _currentWhisperIndex = 0;
        _isWhispering = true;

        if (_whisperCoroutine != null)
        {
            StopCoroutine(_whisperCoroutine);
        }
        _whisperCoroutine = StartCoroutine(WhisperCoroutine());
    }

    /// <summary>
    /// 停止播放呓语
    /// </summary>
    private void StopWhisper()
    {
        if (!_isWhispering) return;

        _isWhispering = false;

        if (_whisperCoroutine != null)
        {
            StopCoroutine(_whisperCoroutine);
            _whisperCoroutine = null;
        }

        // 隐藏字幕框
        if (whisperUI != null)
        {
            whisperUI.SetActive(false);
        }

        // 清空文本
        if (whisperText != null)
        {
            whisperText.text = "";
        }

        // 停止音效
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    /// <summary>
    /// 呓语协程
    /// </summary>
    private IEnumerator WhisperCoroutine()
    {
        // 显示字幕框
        if (whisperUI != null)
        {
            whisperUI.SetActive(true);
        }

        while (_isWhispering && loopWhispers)
        {
            // 获取当前呓语内容
            string currentWhisper = _currentWhisperList[_currentWhisperIndex];

            // 播放打字机效果
            yield return StartCoroutine(TypeText(currentWhisper));

            // 等待
            yield return new WaitForSeconds(waitBetweenWhispers);

            // 移动到下一条呓语
            _currentWhisperIndex++;
            if (_currentWhisperIndex >= _currentWhisperList.Count)
            {
                if (loopWhispers)
                {
                    _currentWhisperIndex = 0;
                }
                else
                {
                    break;
                }
            }
        }

        // 循环结束，隐藏字幕框
        if (!_isPlayerInRange)
        {
            if (whisperUI != null)
            {
                whisperUI.SetActive(false);
            }
        }

        _isWhispering = false;
        _whisperCoroutine = null;
    }

    /// <summary>
    /// 打字机效果
    /// </summary>
    private IEnumerator TypeText(string text)
    {
        if (whisperText == null) yield break;

        whisperText.text = "";

        float soundTimer = 0f;

        foreach (char c in text.ToCharArray())
        {
            whisperText.text += c;

            // 播放音效（不是空格才播放）
            if (!char.IsWhiteSpace(c) && whisperSound != null)
            {
                if (soundTimer <= 0)
                {
                    _audioSource.PlayOneShot(whisperSound);
                    soundTimer = soundInterval;
                }
                else
                {
                    soundTimer -= Time.deltaTime;
                }
            }

            yield return new WaitForSeconds(1f / typingSpeed);
        }
    }

    /// <summary>
    /// 根据当前阶段更新呓语列表
    /// </summary>
    private void UpdateWhisperList()
    {
        // 获取NPC身上的NPCComponent来获取当前阶段
        NPCComponent npcComp = GetComponent<NPCComponent>();
        if (npcComp != null && phaseWhispers != null)
        {
            foreach (var phase in phaseWhispers)
            {
                if (phase.phaseIndex == npcComp.currentPhase)
                {
                    _currentWhisperList = phase.whisperLines;
                    return;
                }
            }
        }

        // 默认使用第一个阶段的呓语
        if (phaseWhispers != null && phaseWhispers.Count > 0)
        {
            _currentWhisperList = phaseWhispers[0].whisperLines;
        }
    }

    /// <summary>
    /// 手动设置呓语列表（供外部调用）
    /// </summary>
    public void SetWhisperList(List<string> whispers)
    {
        _currentWhisperList = whispers;

        // 如果正在播放，重新开始
        if (_isPlayerInRange && !_isWhispering)
        {
            StartWhisper();
        }
    }

    /// <summary>
    /// 刷新呓语（当NPC阶段变化时调用）
    /// </summary>
    public void RefreshWhisper()
    {
        UpdateWhisperList();

        // 如果玩家在范围内，重新开始播放
        if (_isPlayerInRange)
        {
            StopWhisper();
            StartWhisper();
        }
    }

    /// <summary>
    /// 绘制触发范围（用于调试）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}

/// <summary>
/// 阶段呓语配置
/// </summary>
[System.Serializable]
public class PhaseWhisper
{
    [Tooltip("阶段索引（0=人类, 1=初期异化, 2=中期转化, 3=完全植物化）")]
    public int phaseIndex;

    [Tooltip("该阶段的呓语列表（循环播放）")]
    [TextArea(2, 4)]
    public List<string> whisperLines;
}