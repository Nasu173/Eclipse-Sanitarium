using UnityEngine;
using System; // 引入 System 以使用 Action 事件

public class GlobalLanguage : MonoBehaviour
{
    // 单例模式，全游戏唯一
    public static GlobalLanguage Instance;

    public enum LanguageType
    {
        Ch, // 中文
        En  // 英文
    }

    [Header("当前语言状态")]
    public LanguageType currentLanguageType = LanguageType.Ch;

    // 【核心】定义一个语言改变事件。
    // 其他脚本（比如你的对话管理器或 UI 文本）可以监听这个事件，做到瞬间切换语言而不需要刷新场景。
    public event Action OnLanguageChanged;

    private void Awake()
    {
        // 经典的持久化单例写法
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // 【修复警告】确保自身是根节点
            // 保证切换场景时，这个语言管家不会被销毁
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 供 SettingsUIManager 等外部脚本调用的切换方法
    public void SetLanguageToCh()
    {
        if (currentLanguageType == LanguageType.Ch) return; // 如果已经是中文，就不用浪费性能切换

        currentLanguageType = LanguageType.Ch;
        Debug.Log("【系统】全局语言已切换为：中文");

        // 广播事件：告诉全游戏所有关心语言的人，语言变了！
        OnLanguageChanged?.Invoke();
    }

    public void SetLanguageToEn()
    {
        if (currentLanguageType == LanguageType.En) return;

        currentLanguageType = LanguageType.En;
        Debug.Log("【系统】全局语言已切换为：English");

        // 广播事件
        OnLanguageChanged?.Invoke();
    }
}