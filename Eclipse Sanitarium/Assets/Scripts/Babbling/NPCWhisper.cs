using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCWhisper : MonoBehaviour
{
    [Header("字幕框设置")]
    public GameObject whisperUI;
    public TextMeshProUGUI whisperText;
    public Vector3 uiOffset = new Vector3(0, 2.2f, 0);

    [Header("呓语配置")]
    public WhispersScriptObject whispersData;

    [Header("显示设置")]
    public float typingSpeed = 20f;
    public float waitBetweenWhispers = 2f;
    public float triggerRange = 3f;
    public bool loopWhispers = true;

    [Header("音效设置")]
    public AudioClip whisperSound;
    public float soundInterval = 0.5f;

    // 私有变量
    private Camera _mainCamera;
    private bool _isPlayerInRange = false;
    private bool _isWhispering = false;
    private int _currentWhisperIndex = 0;
    private List<string> _currentWhisperList;
    private Coroutine _whisperCoroutine;
    private AudioSource _audioSource;
    private NPCComponent _npcComponent;

    void Start()
    {
        _mainCamera = Camera.main;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        _npcComponent = GetComponent<NPCComponent>();

        if (whisperUI != null)
        {
            whisperUI.SetActive(false);
            whisperUI.transform.localPosition = uiOffset;
        }
    }

    void Update()
    {
        if (whisperUI != null && whisperUI.activeSelf && _mainCamera != null)
        {
            whisperUI.transform.LookAt(_mainCamera.transform);
            whisperUI.transform.Rotate(0, 180, 0);
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

    private void StartWhisper()
    {
        if (_isWhispering) return;

        _currentWhisperIndex = 0;
        _isWhispering = true;

        if (_whisperCoroutine != null)
        {
            StopCoroutine(_whisperCoroutine);
        }
        _whisperCoroutine = StartCoroutine(WhisperCoroutine());
    }

    private void StopWhisper()
    {
        if (!_isWhispering) return;

        _isWhispering = false;

        if (_whisperCoroutine != null)
        {
            StopCoroutine(_whisperCoroutine);
            _whisperCoroutine = null;
        }

        if (whisperUI != null)
        {
            whisperUI.SetActive(false);
        }

        if (whisperText != null)
        {
            whisperText.text = "";
        }

        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    private IEnumerator WhisperCoroutine()
    {
        if (whisperUI != null)
        {
            whisperUI.SetActive(true);
        }

        while (_isWhispering && loopWhispers)
        {
            // 【关键修改】每次播放前实时获取当前语言和阶段的呓语
            _currentWhisperList = GetCurrentWhisperList();

            if (_currentWhisperList == null || _currentWhisperList.Count == 0)
            {
                yield break;
            }

            string currentWhisper = _currentWhisperList[_currentWhisperIndex];

            yield return StartCoroutine(TypeText(currentWhisper));

            yield return new WaitForSeconds(waitBetweenWhispers);

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

        if (!_isPlayerInRange && whisperUI != null)
        {
            whisperUI.SetActive(false);
        }

        _isWhispering = false;
        _whisperCoroutine = null;
    }

    /// <summary>
    /// 实时获取当前语言和阶段的呓语列表（像对话系统一样）
    /// </summary>
    private List<string> GetCurrentWhisperList()
    {
        if (whispersData == null) return null;

        // 获取当前阶段
        int currentPhase = 0;
        if (_npcComponent != null)
        {
            currentPhase = _npcComponent.currentPhase;
        }

        // 获取当前语言（实时获取，就像对话系统那样）
        GlobalLanguage.LanguageType currentLang = GlobalLanguage.LanguageType.Ch;
        if (GlobalLanguage.Instance != null)
        {
            currentLang = GlobalLanguage.Instance.currentLanguageType;
        }

        // 根据语言选择对应的呓语列表
        List<PhaseWhispersData> phaseDataList = null;
        switch (currentLang)
        {
            case GlobalLanguage.LanguageType.Ch:
                phaseDataList = whispersData.phaseWhispers_Ch;
                break;
            case GlobalLanguage.LanguageType.En:
                phaseDataList = whispersData.phaseWhispers_En;
                break;
        }

        // 根据阶段获取呓语列表
        if (phaseDataList != null)
        {
            foreach (var phaseData in phaseDataList)
            {
                if (phaseData.phaseIndex == currentPhase)
                {
                    return phaseData.whisperLines;
                }
            }
        }

        // 如果没找到对应阶段，使用第一个阶段
        if (phaseDataList != null && phaseDataList.Count > 0)
        {
            return phaseDataList[0].whisperLines;
        }

        return null;
    }

    private IEnumerator TypeText(string text)
    {
        if (whisperText == null) yield break;

        whisperText.text = "";

        float soundTimer = 0f;

        foreach (char c in text.ToCharArray())
        {
            whisperText.text += c;

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
    /// 刷新呓语（当NPC阶段变化时调用）
    /// </summary>
    public void RefreshWhisper()
    {
        // 如果正在播放，重新开始
        if (_isPlayerInRange && _isWhispering)
        {
            StopWhisper();
            StartWhisper();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}