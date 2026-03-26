using UnityEngine;
using TMPro;
using System.Collections; // 【新增】引入协程需要的命名空间

public class DocumentUIManager : MonoBehaviour
{
    public static DocumentUIManager Instance { get; private set; }

    [Header("UI 组件引用")]
    public GameObject documentPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    [Header("玩家控制引用")]
    public MonoBehaviour playerMovement;
    public MonoBehaviour playerLook;
    // 【新增】把我们的交互扫描仪也拖进来管辖
    public PlayerInteractor playerInteractor;

    private bool _isReading = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        documentPanel.SetActive(false);
    }

    void Update()
    {
        // 只有在阅读状态下，才检测关闭按键
        if (_isReading && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            CloseDocument();
        }
    }

    public void ShowDocument(string title, string content)
    {
        titleText.text = title;
        contentText.text = content;

        documentPanel.SetActive(true);
        _isReading = true;

        // 冻结移动、转头，以及【新增的交互射线】
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerLook != null) playerLook.enabled = false;

        // 强制扫描仪休眠，"[E] 提示" 会在这里自动消失
        if (playerInteractor != null) playerInteractor.SetInteractorActive(false);
    }

    private void CloseDocument()
    {
        documentPanel.SetActive(false);
        _isReading = false;

        // 恢复移动和转头
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerLook != null) playerLook.enabled = true;

        // 【核心修复】开启一个协程，等当前这一帧彻底跑完，再恢复交互扫描仪
        // 这样就完美避开了这一帧里按下的 E 键！
        StartCoroutine(EnableInteractorNextFrame());
    }

    // 协程：等待一帧
    private IEnumerator EnableInteractorNextFrame()
    {
        // yield return null 的意思是：在此暂停，直到下一帧再继续往下执行
        yield return null;

        if (playerInteractor != null)
        {
            playerInteractor.SetInteractorActive(true);
        }
    }
}