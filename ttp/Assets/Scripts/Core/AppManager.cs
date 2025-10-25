using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 应用管理器 - 管理桌面应用的打开、关闭和状态
/// </summary>
public class AppManager : MonoBehaviour
{
    [System.Serializable]
    public class AppData
    {
        public string appId;            // 应用唯一ID
        public string appName;          // 应用显示名称
        public GameObject appIcon;      // 桌面图标GameObject(Button组件)
        public GameObject appWindow;    // 应用窗口GameObject
        public GameObject chapter2AppWindow; // 第二章应用窗口GameObject（可选）
        public bool isUnlocked = false; // 是否已解锁
        public bool isBroken = false;   // 是否损坏(点击显示损坏弹窗)
    }

    [Header("应用程序配置")]
    public List<AppData> apps = new List<AppData>();
    public Transform appWindowContainer;

    [Header("窗口管理")]
    public GameObject currentActiveWindow = null;
    public float windowOpenDuration = 0.3f;

    [Header("损坏按键弹窗")]
    public GameObject brokenButtonPanel;        // 损坏弹窗面板
    public Button brokenConfirmButton;          // 损坏弹窗确认按钮
    public Button brokenCloseButton;            // 损坏弹窗关闭按钮(叉子)

    private Dictionary<string, AppData> appDictionary = new Dictionary<string, AppData>();
    private WindowManager windowManager;
    private bool isChapter2 = false;
    private DataManager dataManager;

    void Start()
    {
        windowManager = FindObjectOfType<WindowManager>();

        if (GameManager.Instance != null)
        {
            dataManager = GameManager.Instance.dataManager;
        }

        // 检查当前章节状态
        var chapterManager = FindObjectOfType<ChapterManager>();
        if (chapterManager != null && chapterManager.IsChapter2())
        {
            isChapter2 = true;
        }
        else if (dataManager?.GetSaveData()?.currentChapter >= 2)
        {
            isChapter2 = true;
        }

        Debug.Log($"[AppManager] 检测章节状态: {(isChapter2 ? "第二章" : "第一章")}");

        InitializeApps();
        InitializeBrokenButtonPanel();

        // 监听章节切换事件
        ChapterManager.OnChapter2Started += OnChapter2Started;
    }

    void OnDestroy()
    {
        // 清理事件监听
        ChapterManager.OnChapter2Started -= OnChapter2Started;
    }

    /// <summary>
    /// 初始化所有应用
    /// </summary>
    void InitializeApps()
    {
        bool bugVideoTriggered = GameManager.Instance != null &&
                                  GameManager.Instance.dataManager != null &&
                                  GameManager.Instance.dataManager.IsVideoCallTriggered();

        foreach (var app in apps)
        {
            if (app.appId == "bugapp" && !bugVideoTriggered)
            {
                app.isUnlocked = false;
            }

            // 添加到字典
            appDictionary[app.appId] = app;

            // 设置图标点击事件
            if (app.appIcon != null)
            {
                // 检查是否有SimpleDesktopIcon组件
                SimpleDesktopIcon simpleIcon = app.appIcon.GetComponent<SimpleDesktopIcon>();
                if (simpleIcon != null)
                {
                    // 使用SimpleDesktopIcon,设置解锁状态
                    simpleIcon.SetUnlocked(app.isUnlocked);
                    simpleIcon.SetIconInfo(app.appId, app.appName);
                    Debug.Log($"初始化SimpleDesktopIcon: {app.appName}, isUnlocked={app.isUnlocked}");
                }
                else
                {
                    // 使用普通Button
                    Button iconButton = app.appIcon.GetComponent<Button>();
                    if (iconButton != null)
                    {
                        // 确保按钮使用Color Tint过渡,但不透明
                        if (iconButton.transition == Selectable.Transition.ColorTint)
                        {
                            ColorBlock colors = iconButton.colors;
                            // 修复所有可能导致透明的颜色
                            colors.pressedColor = new Color(
                                colors.pressedColor.r,
                                colors.pressedColor.g,
                                colors.pressedColor.b,
                                1f  // 按下时不透明
                            );
                            colors.selectedColor = new Color(
                                colors.selectedColor.r,
                                colors.selectedColor.g,
                                colors.selectedColor.b,
                                1f  // 选中时不透明(这是关键!)
                            );
                            colors.highlightedColor = new Color(
                                colors.highlightedColor.r,
                                colors.highlightedColor.g,
                                colors.highlightedColor.b,
                                1f  // 悬停时不透明
                            );
                            iconButton.colors = colors;
                        }

                        // 移除所有旧的点击事件
                        iconButton.onClick.RemoveAllListeners();

                        // 添加新的点击事件
                        string appIdCopy = app.appId; // 捕获局部变量避免闭包问题
                        iconButton.onClick.AddListener(() => OnAppIconClicked(appIdCopy));
                    }
                    else
                    {
                        Debug.LogWarning($"应用 {app.appName} 的appIcon没有Button或SimpleDesktopIcon组件!");
                    }
                }
            }

            // 初始化窗口状态(隐藏所有窗口)
            if (app.appWindow != null)
            {
                app.appWindow.SetActive(false);
            }

            // 初始化第二章窗口状态
            if (app.chapter2AppWindow != null)
            {
                app.chapter2AppWindow.SetActive(false);
            }
        }

        Debug.Log($"初始化了 {apps.Count} 个应用程序");
    }

    /// <summary>
    /// 初始化损坏弹窗
    /// </summary>
    void InitializeBrokenButtonPanel()
    {
        // 初始隐藏弹窗
        if (brokenButtonPanel != null)
        {
            brokenButtonPanel.SetActive(false);
        }

        // 绑定确认按钮
        if (brokenConfirmButton != null)
        {
            // 确保按下时不透明
            if (brokenConfirmButton.transition == Selectable.Transition.ColorTint)
            {
                ColorBlock colors = brokenConfirmButton.colors;
                colors.pressedColor = new Color(colors.pressedColor.r, colors.pressedColor.g, colors.pressedColor.b, 1f);
                colors.selectedColor = new Color(colors.selectedColor.r, colors.selectedColor.g, colors.selectedColor.b, 1f);
                colors.highlightedColor = new Color(colors.highlightedColor.r, colors.highlightedColor.g, colors.highlightedColor.b, 1f);
                brokenConfirmButton.colors = colors;
            }

            brokenConfirmButton.onClick.RemoveAllListeners();
            brokenConfirmButton.onClick.AddListener(CloseBrokenButtonPanel);
        }

        // 绑定关闭按钮(叉子)
        if (brokenCloseButton != null)
        {
            // 确保叉子按钮有点击反馈
            if (brokenCloseButton.transition == Selectable.Transition.ColorTint)
            {
                ColorBlock colors = brokenCloseButton.colors;
                colors.normalColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 1f);
                colors.pressedColor = new Color(colors.pressedColor.r, colors.pressedColor.g, colors.pressedColor.b, 1f);
                colors.selectedColor = new Color(colors.selectedColor.r, colors.selectedColor.g, colors.selectedColor.b, 1f);
                colors.highlightedColor = new Color(colors.highlightedColor.r, colors.highlightedColor.g, colors.highlightedColor.b, 1f);
                brokenCloseButton.colors = colors;
            }

            brokenCloseButton.onClick.RemoveAllListeners();
            brokenCloseButton.onClick.AddListener(CloseBrokenButtonPanel);
        }
    }

    /// <summary>
    /// 应用图标点击事件 - 核心逻辑
    /// </summary>
    public void OnAppIconClicked(string appId)
    {
        if (!appDictionary.ContainsKey(appId))
        {
            Debug.LogWarning($"应用 {appId} 不存在!");
            return;
        }

        AppData app = appDictionary[appId];

        // 1. 优先检查:按键是否损坏
        if (app.isBroken)
        {
            Debug.Log($"按键已损坏: {app.appName}");
            ShowBrokenButtonPanel();
            return;
        }

        // 2. 检查:应用是否解锁
        if (!app.isUnlocked)
        {
            Debug.Log($"应用尚未解锁: {app.appName}");

            // 只在UIManager配置完整时显示消息
            if (GameManager.Instance != null &&
                GameManager.Instance.uiManager != null &&
                GameManager.Instance.uiManager.messagePanel != null)
            {
                GameManager.Instance.uiManager.ShowMessage($"应用 {app.appName} 尚未解锁");
            }
            return;
        }

        // 3. 正常打开应用
        OpenApp(appId);
    }

    /// <summary>
    /// 打开应用
    /// </summary>
    public void OpenApp(string appId)
    {
        // 如果有WindowManager,委托给它处理
        if (windowManager != null)
        {
            windowManager.OpenApp(appId);
            return;
        }

        if (!appDictionary.ContainsKey(appId))
        {
            Debug.LogWarning($"应用 {appId} 不存在!");
            return;
        }

        AppData app = appDictionary[appId];

        // 根据章节选择合适的窗口
        GameObject targetWindow = GetAppWindowForChapter(app);
        if (targetWindow == null)
        {
            Debug.LogWarning($"应用 {app.appName} 没有设置窗口!");
            return;
        }

        // 关闭当前活动窗口
        if (currentActiveWindow != null && currentActiveWindow != targetWindow)
        {
            string currentAppId = GetAppIdByWindow(currentActiveWindow);
            if (!string.IsNullOrEmpty(currentAppId))
            {
                CloseApp(currentAppId);
            }
        }

        // 打开新窗口
        targetWindow.SetActive(true);
        currentActiveWindow = targetWindow;
        Debug.Log($"打开应用: {app.appName} (章节: {(isChapter2 ? "第二章" : "第一章")})");

        // 设置窗口层级
        if (appWindowContainer != null)
        {
            targetWindow.transform.SetParent(appWindowContainer, false);  // false = 保持本地坐标
            targetWindow.transform.SetAsLastSibling();

            // 确保AppWindowContainer是激活状态
            if (!appWindowContainer.gameObject.activeSelf)
            {
                appWindowContainer.gameObject.SetActive(true);
                Debug.Log("激活AppWindowContainer");
            }
        }

        // 播放打开动画
        PlayWindowOpenAnimation(targetWindow);
    }

    /// <summary>
    /// 关闭应用
    /// </summary>
    public void CloseApp(string appId)
    {
        if (!appDictionary.ContainsKey(appId))
        {
            Debug.LogWarning($"应用 {appId} 不存在!");
            return;
        }

        AppData app = appDictionary[appId];

        if (app.appWindow != null)
        {
            PlayWindowCloseAnimation(app.appWindow, () =>
            {
                app.appWindow.SetActive(false);
                if (currentActiveWindow == app.appWindow)
                {
                    currentActiveWindow = null;
                }
                Debug.Log($"关闭应用: {app.appName}");
            });
        }
    }

    /// <summary>
    /// 显示损坏弹窗
    /// </summary>
    public void ShowBrokenButtonPanel()
    {
        if (brokenButtonPanel != null)
        {
            brokenButtonPanel.SetActive(true);
            Debug.Log("显示按键损坏弹窗");
        }
        else
        {
            Debug.LogWarning("BrokenButtonPanel未设置!");
        }
    }

    /// <summary>
    /// 关闭损坏弹窗
    /// </summary>
    public void CloseBrokenButtonPanel()
    {
        if (brokenButtonPanel != null)
        {
            brokenButtonPanel.SetActive(false);
            Debug.Log("关闭按键损坏弹窗");
        }
    }

    /// <summary>
    /// 解锁应用
    /// </summary>
    public void UnlockApp(string appId)
    {
        if (!appDictionary.ContainsKey(appId))
        {
            Debug.LogWarning($"应用 {appId} 不存在!");
            return;
        }

        AppData app = appDictionary[appId];
        if (!app.isUnlocked)
        {
            app.isUnlocked = true;
            Debug.Log($"解锁应用: {app.appName}");

            // 保存解锁状态
            if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
            {
                GameManager.Instance.dataManager.UnlockApp(appId);
            }
        }
    }

    /// <summary>
    /// 电脑视角激活时调用 - 刷新应用解锁状态
    /// </summary>
    public void OnComputerViewActivated()
    {
        Debug.Log("电脑视角已激活");
        RefreshAppStates();
    }

    /// <summary>
    /// 从存档刷新应用状态
    /// </summary>
    private void RefreshAppStates()
    {
        if (GameManager.Instance == null || GameManager.Instance.dataManager == null)
            return;

        var saveData = GameManager.Instance.dataManager.GetSaveData();
        if (saveData != null && saveData.unlockedApps != null)
        {
            foreach (var appId in saveData.unlockedApps)
            {
                UnlockApp(appId);
            }
        }
    }

    /// <summary>
    /// 关闭所有应用窗口
    /// </summary>
    public void CloseAllApps()
    {
        if (currentActiveWindow != null)
        {
            string currentAppId = GetAppIdByWindow(currentActiveWindow);
            if (!string.IsNullOrEmpty(currentAppId))
            {
                CloseApp(currentAppId);
            }
        }
    }

    /// <summary>
    /// 通过窗口获取应用ID
    /// </summary>
    private string GetAppIdByWindow(GameObject window)
    {
        foreach (var kvp in appDictionary)
        {
            if (kvp.Value.appWindow == window)
            {
                return kvp.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// 播放窗口打开动画
    /// </summary>
    private void PlayWindowOpenAnimation(GameObject window)
    {
        if (window != null)
        {
            window.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleAnimation(window, Vector3.zero, Vector3.one, windowOpenDuration));
        }
    }

    /// <summary>
    /// 播放窗口关闭动画
    /// </summary>
    private void PlayWindowCloseAnimation(GameObject window, System.Action onComplete)
    {
        if (window != null)
        {
            StartCoroutine(ScaleAnimation(window, Vector3.one, Vector3.zero, windowOpenDuration, onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 缩放动画协程
    /// </summary>
    private System.Collections.IEnumerator ScaleAnimation(GameObject target, Vector3 fromScale, Vector3 toScale, float duration, System.Action onComplete = null)
    {
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float timer = 0f;
        target.transform.localScale = fromScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            target.transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }

        target.transform.localScale = toScale;
        onComplete?.Invoke();
    }

    // ==================== 公共查询方法 ====================

    public AppData GetAppData(string appId)
    {
        return appDictionary.ContainsKey(appId) ? appDictionary[appId] : null;
    }

    public List<AppData> GetUnlockedApps()
    {
        return apps.FindAll(app => app.isUnlocked);
    }

    public bool HasActiveWindow()
    {
        return currentActiveWindow != null;
    }

    public string GetCurrentAppId()
    {
        return GetAppIdByWindow(currentActiveWindow);
    }

    // ==================== 章节切换功能 ====================

    /// <summary>
    /// 章节切换事件处理
    /// </summary>
    void OnChapter2Started()
    {
        isChapter2 = true;
        Debug.Log("[AppManager] 切换到第二章 - 聊天恢复功能已启用");
    }

    /// <summary>
    /// 根据章节获取应用窗口
    /// </summary>
    GameObject GetAppWindowForChapter(AppData app)
    {
        if (app.appId == "chatapp" && isChapter2 && app.chapter2AppWindow != null)
        {
            if (IsChatRestoreCompleted())
            {
                return app.chapter2AppWindow;
            }
        }
        return app.appWindow;
    }

    /// <summary>
    /// 聊天恢复 - 切换到第二章聊天应用
    /// </summary>
    public void SwitchToChapter2Chat()
    {
        if (!isChapter2)
        {
            Debug.LogWarning("[AppManager] 尚未进入第二章，无法切换聊天应用");
            return;
        }

        Debug.Log("[AppManager] 聊天恢复：切换到第二章聊天应用");

        // 如果当前打开的是聊天应用，切换到第二章版本
        if (currentActiveWindow != null)
        {
            string currentAppId = GetAppIdByWindow(currentActiveWindow);
            if (currentAppId == "chatapp")
            {
                Debug.Log("[AppManager] 当前打开的是聊天应用，切换到第二章版本");
                CloseApp("chatapp");
                OpenChatAppChapter2();
                return;
            }
        }

        // 如果没有打开聊天应用，直接打开第二章聊天应用
        OpenChatAppChapter2();
    }

    /// <summary>
    /// 打开第二章聊天应用
    /// </summary>
    void OpenChatAppChapter2()
    {
        AppData chatApp = GetAppData("chatapp");
        if (chatApp == null)
        {
            Debug.LogError("[AppManager] 未找到聊天应用配置");
            return;
        }

        if (chatApp.chapter2AppWindow == null)
        {
            Debug.LogError("[AppManager] 聊天应用未配置第二章窗口");
            return;
        }

        MarkChatRestoreCompleted();

        // 关闭当前活动窗口
        if (currentActiveWindow != null && currentActiveWindow != chatApp.chapter2AppWindow)
        {
            string currentAppId = GetAppIdByWindow(currentActiveWindow);
            if (!string.IsNullOrEmpty(currentAppId))
            {
                CloseApp(currentAppId);
            }
        }

        // 打开第二章聊天窗口
        chatApp.chapter2AppWindow.SetActive(true);
        currentActiveWindow = chatApp.chapter2AppWindow;
        Debug.Log("[AppManager] 已打开第二章聊天应用");

        // 设置窗口层级
        if (appWindowContainer != null)
        {
            chatApp.chapter2AppWindow.transform.SetParent(appWindowContainer, false);
            chatApp.chapter2AppWindow.transform.SetAsLastSibling();

            if (!appWindowContainer.gameObject.activeSelf)
            {
                appWindowContainer.gameObject.SetActive(true);
            }
        }

        // 播放打开动画
        PlayWindowOpenAnimation(chatApp.chapter2AppWindow);
    }

    /// <summary>
    /// 强制切换到第二章（测试用）
    /// </summary>
    [ContextMenu("强制切换到第二章")]
    public void ForceChapter2()
    {
        OnChapter2Started();
    }

    bool IsChatRestoreCompleted()
    {
        if (GameManager.Instance?.dataManager == null)
        {
            return false;
        }

        return GameManager.Instance.dataManager.IsPasswordSolved("browser_chat_restore_completed");
    }

    void MarkChatRestoreCompleted()
    {
        if (GameManager.Instance?.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved("browser_chat_restore_completed");
        }
    }
}
