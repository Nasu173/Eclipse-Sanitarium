using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIManager : MonoBehaviour
{
    [Header("调试与控制")]
    [Tooltip("打开/关闭设置面板的快捷键")]
    public KeyCode toggleKey = KeyCode.G;

    [Header("默认初始设置 (仅玩家第一次打开游戏时生效)")]
    [Range(0f, 1f)] public float defaultVolume = 1f;
    [Range(50f, 1000f)] public float defaultSensitivity = 300f;
    [Tooltip("默认语言: 0=中文, 1=英文")]
    public int defaultLanguageIndex = 0;

    [Header("UI 面板引用")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public Slider sensitivitySlider;
    public TMP_Dropdown languageDropdown;

    [Header("玩家系统引用")]
    [Tooltip("拖入玩家相机上的 FirstPersonLook 脚本")]
    public MonoBehaviour playerLookScript;

    private bool _isOpen = false;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // 1. 初始化并加载本地保存的设置（或者使用我们在 Inspector 填的默认值）
        LoadAndApplySettings();

        // 2. 绑定 UI 组件的值改变事件 (当玩家拖动滑块时触发保存)
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (_isOpen) CloseSettings();
            else OpenSettings();
        }
    }

    /// <summary>
    /// 核心逻辑：读取本地存档，如果没有存档就用默认值，并应用到游戏和UI中
    /// </summary>
    private void LoadAndApplySettings()
    {
        // 读取音量 (键值对："SavedVolume"，如果找不到，就返回 defaultVolume)
        float savedVolume = PlayerPrefs.GetFloat("SavedVolume", defaultVolume);
        AudioListener.volume = savedVolume;
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume; // 同步给UI
        }

        // 读取灵敏度
        float savedSensitivity = PlayerPrefs.GetFloat("SavedSensitivity", defaultSensitivity);
        if (playerLookScript != null)
        {
            var field = playerLookScript.GetType().GetField("mouseSensitivity");
            if (field != null) field.SetValue(playerLookScript, savedSensitivity);
        }
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 50f;
            sensitivitySlider.maxValue = 1000f;
            sensitivitySlider.value = savedSensitivity; // 同步给UI
        }

        // 读取语言设置
        int savedLanguage = PlayerPrefs.GetInt("SavedLanguage", defaultLanguageIndex);
        if (languageDropdown != null)
        {
            languageDropdown.value = savedLanguage; // 同步给UI
        }

        // 游戏刚启动时，手动强制应用一次语言（因为 UI 的 onValueChanged 在初始化时可能不会自动触发）
        ApplyLanguage(savedLanguage);
    }

    // ==========================================
    // 界面开关与游戏状态拦截逻辑
    // ==========================================
    private void OpenSettings()
    {
        _isOpen = true;
        settingsPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerLookScript != null) playerLookScript.enabled = false;
    }

    private void CloseSettings()
    {
        _isOpen = false;
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerLookScript != null)
        {
            playerLookScript.Invoke("SyncRotation", 0f);
            playerLookScript.enabled = true;
        }

        // 关闭面板时强制将刚才改动的数据写入硬盘，防止游戏突然崩溃导致没存上
        PlayerPrefs.Save();
    }

    // ==========================================
    // 具体功能实现与数据保存逻辑
    // ==========================================
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("SavedVolume", value); // 记录到本地
    }

    private void OnSensitivityChanged(float value)
    {
        if (playerLookScript != null)
        {
            var field = playerLookScript.GetType().GetField("mouseSensitivity");
            if (field != null) field.SetValue(playerLookScript, value);
        }
        PlayerPrefs.SetFloat("SavedSensitivity", value); // 记录到本地
    }

    private void OnLanguageDropdownChanged(int index)
    {
        ApplyLanguage(index);
        PlayerPrefs.SetInt("SavedLanguage", index); // 记录到本地
    }

    private void ApplyLanguage(int index)
    {
        if (GlobalLanguage.Instance == null) return;

        if (index == 0)
        {
            GlobalLanguage.Instance.SetLanguageToCh();
        }
        else if (index == 1)
        {
            GlobalLanguage.Instance.SetLanguageToEn();
        }
    }
}