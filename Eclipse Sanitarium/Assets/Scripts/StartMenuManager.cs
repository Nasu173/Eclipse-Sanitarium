using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class StartMenuManager : MonoBehaviour
{
    // 单例模式：确保全局唯一，跨场景传递数据
    public static StartMenuManager Instance;

    public string gameSceneName = "movetask";

    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button optionQuitButton;

    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameObject optionUI;

    [Header("音效配置")]
    public AudioSource audioSource; // 播放按钮点击音效
    public AudioClip clickSound;

    private void Awake()
    {
        // 单例初始化：防止重复创建，保留跨场景
        if (Instance == null)
        {
            Instance = this;

        }
        else
        {
            Destroy(gameObject);
            return;
        }

        startMenuUI.SetActive(true);

        BindAllEvents();
    }

    private void Start()
    {
        
    }

    /// <summary>
    /// 绑定所有交互事件（按钮+Slider）
    /// </summary>
    private void BindAllEvents()
    {
        //
        playButton.onClick.AddListener(() =>
        {
            // 销毁主菜单的 UI 管理器和设置管理器
            Destroy(Instance.gameObject); // 销毁自己

            // 如果主菜单的 SettingsUIManager 是单独的物体，也要销毁
            SettingsUIManager menuSettings = FindObjectOfType<SettingsUIManager>();
            if (menuSettings != null && menuSettings.gameObject.scene.name == "StartScene")
            {
                Destroy(menuSettings.gameObject);
            }

            SceneManager.LoadScene("GameScene");
        });
        //
        optionButton.onClick.AddListener(() =>
        {
            optionUI.SetActive(true);
            startMenuUI.SetActive(false);
        });
        //
        optionQuitButton.onClick.AddListener(() =>
        {
            optionUI.SetActive(false);
            startMenuUI.SetActive(true);
        });
        //
        quitButton.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        });

    }

    /// <summary>
    /// 播放按钮点击音效
    /// </summary>
    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
