using UnityEngine;

/// <summary>
/// 语言切换测试脚本 (供快速测试双语UI使用)
/// 挂载到场景中任意物体(如GlobalObject或Manager)上即可生效。
/// </summary>
public class TestLanguageHotkey : MonoBehaviour
{
    [Header("测试快捷键设置")]
    [Tooltip("切换到中文的快捷键")]
    public KeyCode chineseKey = KeyCode.F9;

    [Tooltip("切换到英文的快捷键")]
    public KeyCode englishKey = KeyCode.F10;

    private void Update()
    {
        // 尝试自动寻找实例
        if (GlobalLanguage.Instance == null)
        {
            GlobalLanguage.Instance = FindObjectOfType<GlobalLanguage>();
        }

        // 测试切换中文
        if (Input.GetKeyDown(chineseKey))
        {
            if (GlobalLanguage.Instance != null)
            {
                GlobalLanguage.Instance.SetLanguageToCh();
                Debug.Log($"【测试脚本】语言已切换为: 中文 (快捷键 {chineseKey})");
            }
            else
            {
                Debug.LogWarning("未找到 GlobalLanguage 实例！请确保场景中有一个挂载了 GlobalLanguage 脚本的物体。");
            }
        }

        // 测试切换英文
        if (Input.GetKeyDown(englishKey))
        {
            if (GlobalLanguage.Instance != null)
            {
                GlobalLanguage.Instance.SetLanguageToEn();
                Debug.Log($"【测试脚本】语言已切换为: 英文 (快捷键 {englishKey})");
            }
            else
            {
                Debug.LogWarning("未找到 GlobalLanguage 实例！请确保场景中有一个挂载了 GlobalLanguage 脚本的物体。");
            }
        }
    }
}
