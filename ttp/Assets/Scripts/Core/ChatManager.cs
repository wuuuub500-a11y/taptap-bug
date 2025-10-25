using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 聊天应用管理器 - 管理第一章和第二章的聊天应用切换
/// </summary>
public class ChatManager : MonoBehaviour
{
    [Header("=== 聊天应用配置 ===")]
    public GameObject chatApp1;     // 第一章的ChatApp (ChatWindow)
    public GameObject chatApp2;     // 第二章的ChatApp (ChatWindow下的chatapp-c2)
    
    [Header("=== 聊天恢复功能 ===")]
    public GameObject chatRestorePanel;      // 聊天恢复确认面板
    public Button restoreConfirmButton;      // 确认恢复按钮
    public Button restoreCancelButton;       // 取消按钮
    
    // 状态管理
    private bool isChapter2 = false;
    private bool chatRestoreShown = false;
    private bool chatRestoreCompleted = false;
    private DataManager dataManager;
    
    void Start()
    {
        InitializeChatManager();
        
        // 监听章节切换事件
        ChapterManager.OnChapter2Started += OnChapter2Started;
    }
    
    void OnDestroy()
    {
        // 清理事件监听
        ChapterManager.OnChapter2Started -= OnChapter2Started;
    }
    
    /// <summary>
    /// 初始化聊天管理器
    /// </summary>
    void InitializeChatManager()
    {
        // 检查当前章节
        var chapterManager = FindObjectOfType<ChapterManager>();
        if (chapterManager != null && chapterManager.IsChapter2())
        {
            isChapter2 = true;
        }

        if (GameManager.Instance != null)
        {
            dataManager = GameManager.Instance.dataManager;
        }

        chatRestoreCompleted = dataManager != null && dataManager.IsPasswordSolved("browser_chat_restore_completed");
        chatRestoreShown = chatRestoreCompleted;

        // 初始化UI状态
        InitializeChatApps();
        InitializeRestorePanel();

        Debug.Log("[ChatManager] 初始化完成，当前章节：" + (isChapter2 ? "第二章" : "第一章"));
    }
    
    /// <summary>
    /// 初始化聊天应用状态
    /// </summary>
    void InitializeChatApps()
    {
        if (isChapter2 && chatRestoreCompleted)
        {
            // 第二章：启用第二章聊天应用
            if (chatApp1 != null) chatApp1.SetActive(false);
            if (chatApp2 != null) chatApp2.SetActive(true);
        }
        else
        {
            // 第一章：启用第一章聊天应用
            if (chatApp1 != null) chatApp1.SetActive(true);
            if (chatApp2 != null) chatApp2.SetActive(false);
        }
    }
    
    /// <summary>
    /// 初始化聊天恢复面板
    /// </summary>
    void InitializeRestorePanel()
    {
        if (chatRestorePanel != null)
        {
            chatRestorePanel.SetActive(false);
        }
        
        // 绑定按钮事件
        if (restoreConfirmButton != null)
        {
            restoreConfirmButton.onClick.RemoveAllListeners();
            restoreConfirmButton.onClick.AddListener(OnRestoreConfirmed);
        }
        
        if (restoreCancelButton != null)
        {
            restoreCancelButton.onClick.RemoveAllListeners();
            restoreCancelButton.onClick.AddListener(OnRestoreCancelled);
        }
    }
    
    /// <summary>
    /// 启用第二章功能
    /// </summary>
    public void EnableChapter2Features()
    {
        isChapter2 = true;
        Debug.Log("[ChatManager] 启用第二章功能");
    }
    
    /// <summary>
    /// 章节切换事件处理
    /// </summary>
    void OnChapter2Started()
    {
        Debug.Log("[ChatManager] 检测到章节切换到第二章");
        EnableChapter2Features();
        chatRestoreCompleted = dataManager != null && dataManager.IsPasswordSolved("browser_chat_restore_completed");
        InitializeChatApps();
    }
    
    /// <summary>
    /// 显示聊天恢复确认面板
    /// </summary>
    public void ShowChatRestorePanel()
    {
        if (!isChapter2 || chatRestoreShown || chatRestoreCompleted)
        {
            Debug.LogWarning("[ChatManager] 不在第二章或已经显示过恢复面板");
            return;
        }
        
        if (chatRestorePanel != null)
        {
            chatRestorePanel.SetActive(true);
            Debug.Log("[ChatManager] 显示聊天恢复面板");
        }
        else
        {
            Debug.LogWarning("[ChatManager] 聊天恢复面板未配置，直接切换到第二章聊天应用");
            SwitchToChapter2Chat();
        }
    }
    
    /// <summary>
    /// 确认恢复聊天记录
    /// </summary>
    void OnRestoreConfirmed()
    {
        Debug.Log("[ChatManager] 玩家确认恢复聊天记录");
        
        // 隐藏恢复面板
        if (chatRestorePanel != null)
        {
            chatRestorePanel.SetActive(false);
        }
        
        // 切换到第二章聊天应用
        SwitchToChapter2Chat();

        chatRestoreShown = true;
        chatRestoreCompleted = true;
        if (dataManager != null)
        {
            dataManager.MarkPasswordSolved("browser_chat_restore_completed");
        }
    }
    
    /// <summary>
    /// 取消恢复聊天记录
    /// </summary>
    void OnRestoreCancelled()
    {
        Debug.Log("[ChatManager] 玩家取消恢复聊天记录");
        
        // 隐藏恢复面板
        if (chatRestorePanel != null)
        {
            chatRestorePanel.SetActive(false);
        }
        
        chatRestoreShown = true;
    }
    
    /// <summary>
    /// 切换到第二章聊天应用
    /// </summary>
    public void SwitchToChapter2Chat()
    {
        if (!isChapter2)
        {
            Debug.LogWarning("[ChatManager] 不在第二章，无法切换");
            return;
        }

        if (dataManager != null && !dataManager.IsPasswordSolved("browser_chat_restore_completed"))
        {
            dataManager.MarkPasswordSolved("browser_chat_restore_completed");
        }

        chatRestoreCompleted = true;

        // 关闭第一章聊天应用
        if (chatApp1 != null)
        {
            chatApp1.SetActive(false);
        }

        // 开启第二章聊天应用
        if (chatApp2 != null)
        {
            chatApp2.SetActive(true);
            Debug.Log("[ChatManager] 已切换到第二章聊天应用");
        }
        else
        {
            Debug.LogError("[ChatManager] 第二章聊天应用(chatApp2)未配置！");
        }
    }
    
    /// <summary>
    /// 获取当前活跃的聊天应用
    /// </summary>
    public GameObject GetCurrentChatApp()
    {
        if (isChapter2)
        {
            return chatApp2;
        }
        else
        {
            return chatApp1;
        }
    }
    
    /// <summary>
    /// 检查是否在第二章
    /// </summary>
    public bool IsChapter2()
    {
        return isChapter2;
    }
    
    /// <summary>
    /// 强制切换到第二章聊天应用（测试用）
    /// </summary>
    [ContextMenu("强制切换到第二章聊天")]
    public void ForceSwitchToChapter2Chat()
    {
        isChapter2 = true;
        SwitchToChapter2Chat();
    }
    
    /// <summary>
    /// 强制显示聊天恢复面板（测试用）
    /// </summary>
    [ContextMenu("强制显示聊天恢复面板")]
    public void ForceShowRestorePanel()
    {
        isChapter2 = true;
        ShowChatRestorePanel();
    }
}
