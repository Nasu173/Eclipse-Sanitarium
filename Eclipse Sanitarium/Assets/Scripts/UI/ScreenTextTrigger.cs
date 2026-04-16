using System.Collections;
using UnityEngine;
using TMPro; // 使用 TextMeshPro

[RequireComponent(typeof(Collider))]
public class ScreenTextTrigger : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("想要显示在屏幕上的纯文本组件 (不需要背景Panel)")]
    public TextMeshProUGUI floatingTextLabel;

    [Header("文本内容")]
    [TextArea(3, 5)]
    public string contentText = "这是玩家碰到碰撞箱后会在屏幕上打字显示的句子。";

    [Header("显示设置")]
    [Tooltip("打字机速度（字符/秒）")]
    public float typingSpeed = 25f;
    [Tooltip("文本全部显示后，持续停留的时间（秒）")]
    public float displayDuration = 2f;

    [Header("触发设置")]
    [Tooltip("判定触碰对象的标签配置")]
    public string playerTag = "Player";
    [Tooltip("是否只触发一次？")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        // 游戏开始时先清空并隐藏该文本
        if (floatingTextLabel != null)
        {
            floatingTextLabel.text = "";
            floatingTextLabel.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 如果已经触发过了且设置了只触发一次，直接跳过
        if (triggerOnce && hasTriggered) return;

        // 仅当触碰对象是玩家时
        if (other.CompareTag(playerTag))
        {
            if (floatingTextLabel != null)
            {
                hasTriggered = true;

                // 停止可能正在执行的旧打字协程
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }

                typingCoroutine = StartCoroutine(ShowTextRoutine());
            }
            else
            {
                Debug.LogWarning("触发器没有配置浮动文本组件(Floating Text Label)！", this);
            }
        }
    }

    private IEnumerator ShowTextRoutine()
    {
        // 激活并清空文本
        floatingTextLabel.gameObject.SetActive(true);
        floatingTextLabel.text = "";

        // 1. 打字机效果
        foreach (char c in contentText.ToCharArray())
        {
            floatingTextLabel.text += c;
            // 控制每个字符的等待时间
            yield return new WaitForSeconds(1f / typingSpeed);
        }

        // 2. 文本完整显示后，保持2秒
        yield return new WaitForSeconds(displayDuration);

        // 3. 彻底消失
        floatingTextLabel.text = "";
        floatingTextLabel.gameObject.SetActive(false);
        typingCoroutine = null; // 标记协程已结束
    }
}
