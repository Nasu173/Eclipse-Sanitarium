using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Tooltip("对话UI面板")]
    public GameObject dialogueUI;

    [Tooltip("说话人名字文本")]
    public TextMeshProUGUI speakerNameText;

    [Tooltip("对话内容文本")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("继续提示文本（Space）")]
    public TextMeshProUGUI continuePrompt;

    [Tooltip("打字机效果速度（字符/秒）")]
    public float typingSpeed = 25f; // 恢复默认值

    [Header("对话音效设置")]
    [Tooltip("对话音效音频源")]
    public AudioSource dialogueSoundSource;

    [Tooltip("说话音效（每个字符播放一次）")]
    public AudioClip[] voiceClips;

    [Tooltip("音效播放概率（0-1），避免每个字符都有声音显得嘈杂")]
    [Range(0f, 1f)] public float soundPlayChance = 0.7f;

    [Header("对话设置")]
    [Tooltip("自动继续对话的延迟时间")]
    public float defaultAutoContinueDelay = 1f;

    //当前对话行索引
    private int _currentLineIndex;
    //当前对话对象
    private DialogueScriptObject _currentDialogue;
    //正在打印
    private bool _isTyping;
    //正在对话
    public bool _isDialogueActive;
    private Coroutine _typingCoroutine;

    //对话事件
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始隐藏对话面板
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }

        // 自动创建音频源（如果未指定）
        if (dialogueSoundSource == null)
        {
            dialogueSoundSource = gameObject.AddComponent<AudioSource>();
            dialogueSoundSource.playOnAwake = false;
            dialogueSoundSource.spatialBlend = 0f;
        }
    }

    private void Update()
    {
        if (_isDialogueActive)
        {
            // 正在打字时按下空格：立即显示完整文本
            if (_isTyping && Input.GetKeyDown(KeyCode.Space))
            {
                SkipTyping();
            }
            // 不是打字状态且按下空格：继续下一句
            else if (!_isTyping && Input.GetKeyDown(KeyCode.Space))
            {
                ContinueDialogue();
            }
        }
    }

    /// <summary>
    /// 跳过当前打字，直接显示完整文本
    /// </summary>
    private void SkipTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        DialogueLine currentLine = GetCurrentLanguageLine();
        if (currentLine != null)
        {
            dialogueText.text = currentLine.dialogueText;
        }
        _isTyping = false;

        if (currentLine != null && currentLine.waitForInput)
        {
            continuePrompt.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 根据当前语言获取对应的对话行
    /// </summary>
    private DialogueLine GetCurrentLanguageLine()
    {
        if (_currentDialogue == null) return null;

        // 根据当前语言选择对应的列表
        if (GlobalLanguage.Instance != null)
        {
            switch (GlobalLanguage.Instance.currentLanguageType)
            {
                case GlobalLanguage.LanguageType.Ch:
                    if (_currentLineIndex < _currentDialogue.dialogueLines_Ch.Count)
                        return _currentDialogue.dialogueLines_Ch[_currentLineIndex];
                    break;
                case GlobalLanguage.LanguageType.En:
                    if (_currentLineIndex < _currentDialogue.dialogueLines_En.Count)
                        return _currentDialogue.dialogueLines_En[_currentLineIndex];
                    break;
            }
        }

        // 默认返回中文（如果语言系统不存在）
        if (_currentLineIndex < _currentDialogue.dialogueLines_Ch.Count)
            return _currentDialogue.dialogueLines_Ch[_currentLineIndex];

        return null;
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    public void StartDialogue(DialogueScriptObject dialogue)
    {
        // 检查对话数据
        if (dialogue == null)
        {
            Debug.LogError("对话数据为空！");
            return;
        }

        // 检查当前语言是否有内容
        bool hasContent = false;
        if (GlobalLanguage.Instance != null)
        {
            switch (GlobalLanguage.Instance.currentLanguageType)
            {
                case GlobalLanguage.LanguageType.Ch:
                    hasContent = dialogue.dialogueLines_Ch != null && dialogue.dialogueLines_Ch.Count > 0;
                    break;
                case GlobalLanguage.LanguageType.En:
                    hasContent = dialogue.dialogueLines_En != null && dialogue.dialogueLines_En.Count > 0;
                    break;
            }
        }
        else
        {
            hasContent = dialogue.dialogueLines_Ch != null && dialogue.dialogueLines_Ch.Count > 0;
        }

        if (!hasContent)
        {
            Debug.LogError($"对话 {dialogue.name} 在当前语言下没有内容！");
            return;
        }

        Debug.Log("Start Dialogue: " + dialogue.name);

        _currentDialogue = dialogue;
        _currentLineIndex = 0;
        _isDialogueActive = true;

        // 显示对话面板
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
        }

        // 禁用玩家输入
        OnDialogueStarted?.Invoke();

        // 显示第一行对话
        DisplayCurrentLine();
    }

    /// <summary>
    /// 继续显示下一行对话
    /// </summary>
    public void ContinueDialogue()
    {
        _currentLineIndex++;

        // 检查是否还有下一行（根据当前语言）
        bool hasNextLine = false;
        if (GlobalLanguage.Instance != null)
        {
            switch (GlobalLanguage.Instance.currentLanguageType)
            {
                case GlobalLanguage.LanguageType.Ch:
                    hasNextLine = _currentLineIndex < _currentDialogue.dialogueLines_Ch.Count;
                    break;
                case GlobalLanguage.LanguageType.En:
                    hasNextLine = _currentLineIndex < _currentDialogue.dialogueLines_En.Count;
                    break;
            }
        }
        else
        {
            hasNextLine = _currentLineIndex < _currentDialogue.dialogueLines_Ch.Count;
        }

        if (hasNextLine)
        {
            //展示下一行
            DisplayCurrentLine();
        }
        else
        {
            //结束对话
            EndDialogue();
        }
    }

    /// <summary>
    /// 显示当前行对话（带打字机效果）
    /// </summary>
    public void DisplayCurrentLine()
    {
        DialogueLine currentLine = GetCurrentLanguageLine();
        if (currentLine == null)
        {
            Debug.LogError("无法获取当前语言的对话行");
            EndDialogue();
            return;
        }

        // 更新说话人名字
        speakerNameText.text = currentLine.speakerName;

        // 清空对话文本
        dialogueText.text = "";

        // 控制是否显示继续提示（初始隐藏）
        continuePrompt.gameObject.SetActive(false);

        // 停止之前的打字机协程（如果有）
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        // 开始新的打字机效果
        _typingCoroutine = StartCoroutine(TypeText(currentLine));
    }

    /// <summary>
    /// 打字机效果协程
    /// </summary>
    private IEnumerator TypeText(DialogueLine line)
    {
        _isTyping = true;
        dialogueText.text = "";

        // 逐字显示文本
        foreach (char c in line.dialogueText.ToCharArray())
        {
            // 只对可见字符播放音效
            if (!char.IsWhiteSpace(c) && voiceClips != null && voiceClips.Length > 0)
            {
                if (UnityEngine.Random.value <= soundPlayChance)
                {
                    int randomIndex = UnityEngine.Random.Range(0, voiceClips.Length);
                    dialogueSoundSource.PlayOneShot(voiceClips[randomIndex]);
                }
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(1f / typingSpeed);
        }

        _isTyping = false;
        _typingCoroutine = null;

        // 如果需要等待玩家输入，显示提示
        if (line.waitForInput)
        {
            continuePrompt.gameObject.SetActive(true);
        }
        // 否则自动继续
        else
        {
            yield return new WaitForSeconds(line.autoContinueDelay);
            ContinueDialogue();
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    private void EndDialogue()
    {
        _isDialogueActive = false;

        // 隐藏对话面板
        if (_currentDialogue != null && _currentDialogue.closeOnComplete && dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }

        // 处理任务完成
        if (_currentDialogue != null && _currentDialogue.CurrentTaskWillComplete)
        {
            Debug.Log("对话结束，触发任务完成");
            // 这里可以调用任务系统
            // TaskSystem.Instance?.CompleteCurrentTask();
        }

        // 恢复玩家输入
        OnDialogueEnded?.Invoke();

        _currentDialogue = null;
    }
}