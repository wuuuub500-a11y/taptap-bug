using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 简化的窗口管理器
/// </summary>
public class WindowManager : MonoBehaviour
{
    [System.Serializable]
    public class AppWindow
    {
        public string appId;
        public string appName;
        public GameObject windowObject;
        public bool requiresPassword = false;
        public string password = "";
        public bool isUnlocked = false;
    }

    [Header("窗口配置")]
    public List<AppWindow> appWindows = new List<AppWindow>();
    public Transform windowContainer;

    [Header("动画设置")]
    public float windowOpenDuration = 0.3f;
    public float windowCloseDuration = 0.2f;

    private GameObject currentActiveWindow = null;
    private Dictionary<string, AppWindow> windowDictionary = new Dictionary<string, AppWindow>();

    void Start()
    {
        InitializeWindows();
        LoadUnlockStates();
    }

    void InitializeWindows()
    {
        bool bugVideoTriggered = GameManager.Instance != null &&
                                  GameManager.Instance.dataManager != null &&
                                  GameManager.Instance.dataManager.IsVideoCallTriggered();

        // 初始化窗口字典
        foreach (var app in appWindows)
        {
            if (app.appId == "bugapp" && !bugVideoTriggered)
            {
                app.isUnlocked = false;
            }

            windowDictionary[app.appId] = app;

            // 初始关闭所有窗口
            if (app.windowObject != null)
            {
                app.windowObject.SetActive(false);
            }
        }

        Debug.Log($"初始化了 {appWindows.Count} 个应用窗口");
    }

    void LoadUnlockStates()
    {
        // 从数据管理器加载解锁状态
        var saveData = GameManager.Instance.dataManager?.GetSaveData();
        if (saveData != null && saveData.unlockedApps != null)
        {
            foreach (var appId in saveData.unlockedApps)
            {
                UnlockApp(appId);
            }
        }

        // 默认解锁基础应用
        UnlockApp("blog");
        UnlockApp("notebook");
    }

    /// <summary>
    /// 打开应用
    /// </summary>
    public void OpenApp(string appId)
    {
        if (!windowDictionary.ContainsKey(appId))
        {
            Debug.LogWarning($"应用 {appId} 不存在！");
            return;
        }

        AppWindow app = windowDictionary[appId];

        // 检查解锁状态
        if (!app.isUnlocked)
        {
            Debug.Log($"应用 {app.appName} 尚未解锁");
            GameManager.Instance.uiManager?.ShowMessage($"应用 {app.appName} 尚未解锁");
            return;
        }

        // 检查密码
        if (app.requiresPassword && !IsPasswordUnlocked(appId))
        {
            RequestPassword(app);
            return;
        }

        // 关闭当前窗口
        if (currentActiveWindow != null)
        {
            CloseWindow(currentActiveWindow);
        }

        // 打开新窗口
        if (app.windowObject != null)
        {
            currentActiveWindow = app.windowObject;
            currentActiveWindow.SetActive(true);

            // 将窗口移到容器顶层
            if (windowContainer != null)
            {
                currentActiveWindow.transform.SetParent(windowContainer);
                currentActiveWindow.transform.SetAsLastSibling();
            }

            Debug.Log($"打开应用: {app.appName}");

            // 播放打开动画
            StartCoroutine(WindowAnimation(currentActiveWindow, Vector3.zero, Vector3.one, windowOpenDuration));

            // 通知应用已打开
            NotifyAppOpened(appId);
        }
        else
        {
            Debug.LogError($"应用 {app.appName} 的窗口对象未设置！");
        }
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void CloseWindow(GameObject window)
    {
        if (window == null) return;

        StartCoroutine(WindowAnimation(window, window.transform.localScale, Vector3.zero, windowCloseDuration, () => {
            window.SetActive(false);
            if (currentActiveWindow == window)
            {
                currentActiveWindow = null;
            }
        }));
    }

    /// <summary>
    /// 通过应用ID关闭窗口
    /// </summary>
    public void CloseApp(string appId)
    {
        if (windowDictionary.ContainsKey(appId))
        {
            CloseWindow(windowDictionary[appId].windowObject);
        }
    }

    /// <summary>
    /// 解锁应用
    /// </summary>
    public void UnlockApp(string appId)
    {
        if (!windowDictionary.ContainsKey(appId))
        {
            Debug.LogWarning($"应用 {appId} 不存在！");
            return;
        }

        AppWindow app = windowDictionary[appId];
        if (!app.isUnlocked)
        {
            app.isUnlocked = true;
            Debug.Log($"解锁应用: {app.appName}");

            // 更新桌面图标状态
            UpdateDesktopIcon(appId, true);

            // 保存解锁状态
            GameManager.Instance.dataManager?.UnlockApp(appId);
        }
    }

    /// <summary>
    /// 更新���面图标状态
    /// </summary>
    private void UpdateDesktopIcon(string appId, bool unlocked)
    {
        // 查找对应的桌面图标
        SimpleDesktopIcon[] icons = FindObjectsOfType<SimpleDesktopIcon>();
        foreach (var icon in icons)
        {
            if (icon.appId == appId)
            {
                icon.SetUnlocked(unlocked);
                break;
            }
        }
    }

    /// <summary>
    /// 请求密码
    /// </summary>
    private void RequestPassword(AppWindow app)
    {
        if (GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowPasswordPanel(
                $"请输入 {app.appName} 的密码",
                (password) => VerifyPassword(app, password),
                () => Debug.Log("密码输入取消")
            );
        }
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    private void VerifyPassword(AppWindow app, string password)
    {
        if (password == app.password)
        {
            Debug.Log($"密码验证成功，解锁应用 {app.appName}");
            UnlockPassword(app.appId);
            OpenApp(app.appId);
        }
        else
        {
            Debug.Log("密码错误！");
            GameManager.Instance.uiManager?.ShowError("密码错误！");

            // 震动效果
            if (GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.ShakeScreen(3f, 0.3f);
            }
        }
    }

    /// <summary>
    /// 解锁密码
    /// </summary>
    private void UnlockPassword(string appId)
    {
        GameManager.Instance.dataManager?.MarkPasswordSolved(appId);
    }

    /// <summary>
    /// 检查密码是否已解锁
    /// </summary>
    private bool IsPasswordUnlocked(string appId)
    {
        return GameManager.Instance.dataManager?.IsPasswordSolved(appId) ?? false;
    }

    /// <summary>
    /// 窗口动画
    /// </summary>
    private IEnumerator WindowAnimation(GameObject window, Vector3 fromScale, Vector3 toScale, float duration, System.Action onComplete = null)
    {
        if (window == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float timer = 0f;
        window.transform.localScale = fromScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            window.transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }

        window.transform.localScale = toScale;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 通知应用已打开
    /// </summary>
    private void NotifyAppOpened(string appId)
    {
        // 根据应用类型初始化
        switch (appId)
        {
            case "blog":
                var blogApp = currentActiveWindow?.GetComponent<BlogApp>();
                if (blogApp != null)
                {
                    // 可以调用应用的初始化方法
                    Debug.Log("博客应用已打开");
                }
                break;

            case "notebook":
                var notebookApp = currentActiveWindow?.GetComponent<NotebookApp>();
                if (notebookApp != null)
                {
                    Debug.Log("记事本应用已打开");
                }
                break;

            default:
                Debug.Log($"应用 {appId} 已打开");
                break;
        }
    }

    /// <summary>
    /// 获取当前活动窗口
    /// </summary>
    public GameObject GetCurrentActiveWindow()
    {
        return currentActiveWindow;
    }

    /// <summary>
    /// 检查是否有活动窗口
    /// </summary>
    public bool HasActiveWindow()
    {
        return currentActiveWindow != null;
    }

    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public void CloseAllWindows()
    {
        if (currentActiveWindow != null)
        {
            CloseWindow(currentActiveWindow);
        }
    }
}
