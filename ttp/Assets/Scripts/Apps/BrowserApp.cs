using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 浏览器应用 - 支持URL输入,显示预设页面图片或404
/// UI设计: 首页(搜索框) + 页面图片 + 404图片
/// 输入框同步: URL输入框和搜索框双向同步
/// </summary>
public class BrowserApp : MonoBehaviour
{
    public enum BrowserPageType
    {
        Home,
        Company,
        NotFound,
        FujianPage,
        FumeiPage,
        GuanqinPage,
        WeixinRepairPage
    }

    public static event Action<BrowserPageType> OnBrowserPageChanged;

    [Header("主要UI组件")]
    public Button closeButton;              // 关闭按钮
    public TMP_InputField urlInputField;    // 顶部URL输入框
    public Button goButton;                 // URL确认按钮(右侧小箭头)

    [Header("首页 - 搜索界面")]
    public GameObject homePage;             // 首页面板(完整界面1.PNG的内容)
    public TMP_InputField searchInputField; // 首页搜索框
    public Button searchButton;             // 首页搜索按钮

    [Header("页面显示")]
    public GameObject companyPage;          // 公司页面图片(公司页面.PNG)
    public Button companyBackButton;        // 公司页面返回按钮

    [Header("404页面")]
    public GameObject notFoundPage;         // 404页面(404界面.PNG)
    public Button notFoundBackButton;       // 404返回按钮

    [Header("付简页面")]
    public GameObject fujianPage;           // 付简页面
    public Button fujianBackButton;         // 付简页面返回按钮

    [Header("付觅页面")]
    public GameObject fumeiPage;            // 付觅页面
    public Button fumeiBackButton;          // 付觅页面返回按钮

    [Header("关钦页面")]
    public GameObject guanqinPage;          // 关钦页面
    public Button guanqinBackButton;        // 关钦页面返回按钮
    public Image guanqinPhoto1;             // 关钦照片1
    public Image guanqinPhoto2;             // 关钦照片2
    public Image guanqinPhoto3;             // 关钦照片3
    public Image guanqinPhoto4;             // 关钦照片4

    [Header("SQL Client 页面组件")]
    public GameObject sqlClientDownloadPage;         // 下载页面/卡片
    public Button sqlClientDownloadButton;           // 下载按钮
    public GameObject sqlClientDownloadOverlayPanel; // 按钮激活的面板

    [Header("第二章额外展示")]
    public GameObject chapter2PhotoWidget;          // 第二章首页小照片

    [Header("微信维修页面 - 统一流程")]
    public GameObject weixinRepairPage;     // 微信维修页面
    public Button weixinRepairBackButton;   // 微信维修返回按钮
    public GameObject blackScreenPanel;     // 黑屏提示面板
    public Image weixinRepairTipImage;      // 提示图片（显示2秒）

    // 密码输入面板（黑屏后显示）
    public GameObject passwordPanel;        // 密码输入面板
    public TMP_InputField passwordInput;   // 密码输入框
    public Button passwordConfirmButton;   // 密码确认按钮
    public Button passwordCancelButton;    // 密码取消按钮
    public TextMeshProUGUI passwordPromptText; // 密码提示文字
    public TextMeshProUGUI passwordErrorText;  // 密码错误文字

    // 密码错误提示面板
    public GameObject passwordErrorPanel;   // 密码错误提示面板
    public Button passwordErrorConfirmButton; // 密码错误确认按钮

    // 聊天恢复确认面板（密码正确后显示）
    public GameObject chatRestorePanel;     // 聊天恢复确认面板
    public Button chatRestoreConfirmButton; // 聊天恢复确认按钮
    public Button chatRestoreCloseButton;   // 聊天恢复关闭按钮

    private const string WEIXIN_REPAIR_PASSWORD = "REPAIR"; // 微信维修密码

    // 当前URL
    private string currentUrl = "";

    // 章节状态
    private bool isChapter2 = false;

    private bool isSqlClientDownloadStarted = false;
    private const string SQL_CLIENT_DOWNLOAD_FLAG = "browser_sql_client_downloading";
    private const string SQL_CLIENT_DOWNLOAD_URL = "sql-client.666.com";
    private const string SQL_CLIENT_DOWNLOAD_URL_SHORT = "sql-client.666";
    private const string CHAT_RESTORE_URL = "chat.kuro.secret.com";
    private const string CHAT_RESTORE_URL_SHORT = "chat.kuro.secret";

    // 预设URL列表
    private List<string> validUrls = new List<string>();
    private BrowserPageType currentPage = BrowserPageType.Home;

    void Start()
    {
        InitializeBrowser();

        // 监听章节切换事件
        ChapterManager.OnChapter2Started += OnChapter2Started;
    }

    void OnDestroy()
    {
        // 清理事件监听
        ChapterManager.OnChapter2Started -= OnChapter2Started;
    }

    void InitializeBrowser()
    {
        // 检查当前章节状态（与ChatManager保持同步）
        var chapterManager = FindObjectOfType<ChapterManager>();
        if (chapterManager != null && chapterManager.IsChapter2())
        {
            isChapter2 = true;
            Debug.Log("[BrowserApp] 检测到第二章状态，启用第二章功能");
        }

        SetupUIEvents();
        LoadValidUrls();
        LoadSqlClientDownloadState();
        UpdateChapter2UI();
        ShowHomePage(); // 默认显示首页
    }

    /// <summary>
    /// 绑定所有按钮事件
    /// </summary>
    void SetupUIEvents()
    {
        // 关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBrowser);
        }

        // 顶部URL输入框 - 确认按钮
        if (goButton != null)
        {
            goButton.onClick.AddListener(OnUrlSubmit);
        }

        if (sqlClientDownloadButton != null)
        {
            sqlClientDownloadButton.onClick.AddListener(OnSqlClientDownloadButtonClicked);
        }

        // 顶部URL输入框 - 回车提交
        if (urlInputField != null)
        {
            urlInputField.onSubmit.AddListener((url) => OnUrlSubmit());
            // 监听输入变化,同步到搜索框
            urlInputField.onValueChanged.AddListener(OnUrlInputChanged);
        }

        // 首页搜索按钮
        if (searchButton != null)
        {
            searchButton.onClick.AddListener(OnSearchSubmit);
        }

        // 首页搜索框 - 回车提交
        if (searchInputField != null)
        {
            searchInputField.onSubmit.AddListener((keyword) => OnSearchSubmit());
            // 监听输入变化,同步到URL输入框
            searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
        }

        // 公司页面返回按钮
        if (companyBackButton != null)
        {
            companyBackButton.onClick.AddListener(ShowHomePage);
        }

        // 404返回按钮
        if (notFoundBackButton != null)
        {
            notFoundBackButton.onClick.AddListener(ShowHomePage);
        }

        // 聊天恢复面板按钮
        if (chatRestoreConfirmButton != null)
        {
            chatRestoreConfirmButton.onClick.AddListener(OnChatRestoreConfirmed);
        }

        if (chatRestoreCloseButton != null)
        {
            chatRestoreCloseButton.onClick.AddListener(OnChatRestoreCancelled);
        }

        // 密码验证面板按钮
        if (passwordConfirmButton != null)
        {
            passwordConfirmButton.onClick.AddListener(OnPasswordSubmit);
        }

        if (passwordCancelButton != null)
        {
            passwordCancelButton.onClick.AddListener(OnPasswordCancel);
        }

        if (passwordInput != null)
        {
            passwordInput.onSubmit.AddListener((password) => OnPasswordSubmit());
        }

        // 密码错误面板按钮
        if (passwordErrorConfirmButton != null)
        {
            passwordErrorConfirmButton.onClick.AddListener(OnPasswordErrorConfirm);
        }

        // 新页面返回按钮
        if (fujianBackButton != null)
        {
            fujianBackButton.onClick.AddListener(ShowHomePage);
        }

        if (fumeiBackButton != null)
        {
            fumeiBackButton.onClick.AddListener(ShowHomePage);
        }

        if (guanqinBackButton != null)
        {
            guanqinBackButton.onClick.AddListener(ShowHomePage);
        }

        if (weixinRepairBackButton != null)
        {
            weixinRepairBackButton.onClick.AddListener(ShowHomePage);
        }

        // 关钦页面不需要照片点击事件，改为闪烁动画

        // 统一的微信维修流程按钮
        if (passwordConfirmButton != null)
        {
            passwordConfirmButton.onClick.AddListener(OnPasswordSubmit);
        }

        if (passwordCancelButton != null)
        {
            passwordCancelButton.onClick.AddListener(OnPasswordCancel);
        }

        if (passwordInput != null)
        {
            passwordInput.onSubmit.AddListener((password) => OnPasswordSubmit());
        }

        // 聊天恢复确认按钮
        if (chatRestoreConfirmButton != null)
        {
            chatRestoreConfirmButton.onClick.AddListener(OnChatRestoreConfirmed);
        }

        if (chatRestoreCloseButton != null)
        {
            chatRestoreCloseButton.onClick.AddListener(OnChatRestoreCancelled);
        }
    }

    /// <summary>
    /// 加载有效URL列表
    /// </summary>
    void LoadValidUrls()
    {
        validUrls.Clear();

        // 闪耀星途娱乐公司的各种可能输入
        AddValidUrlVariant("syxtyulegongsi");
        AddValidUrlVariant("闪耀星途娱乐公司");

        // 第二章特殊关键词
        AddValidUrlVariant(CHAT_RESTORE_URL);
        AddValidUrlVariant(CHAT_RESTORE_URL_SHORT);

        // 四个新页面的关键词
        AddValidUrlVariant("付简");
        AddValidUrlVariant("付觅");
        AddValidUrlVariant("关钦");
        AddValidUrlVariant("微信维修");
        AddValidUrlVariant("weixinweixiu");

        // SQL Client 下载链接
        AddValidUrlVariant(SQL_CLIENT_DOWNLOAD_URL);
        AddValidUrlVariant(SQL_CLIENT_DOWNLOAD_URL_SHORT);
    }

    bool AddValidUrlVariant(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        string lower = url.ToLower();
        if (!validUrls.Contains(lower))
        {
            validUrls.Add(lower);
            return true;
        }

        return false;
    }

    /// <summary>
    /// URL输入框值改变时同步到搜索框
    /// </summary>
    void OnUrlInputChanged(string value)
    {
        if (searchInputField != null && homePage != null && homePage.activeSelf)
        {
            // 避免循环触发
            searchInputField.onValueChanged.RemoveListener(OnSearchInputChanged);
            searchInputField.text = value;
            searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
        }
    }

    /// <summary>
    /// 搜索框值改变时同步到URL输入框
    /// </summary>
    void OnSearchInputChanged(string value)
    {
        if (urlInputField != null)
        {
            // 避免循环触发
            urlInputField.onValueChanged.RemoveListener(OnUrlInputChanged);
            urlInputField.text = value;
            urlInputField.onValueChanged.AddListener(OnUrlInputChanged);
        }
    }

    /// <summary>
    /// 关闭浏览器窗口
    /// </summary>
    void CloseBrowser()
    {
        Debug.Log("关闭浏览器窗口");
        gameObject.SetActive(false);

        // 通知AppManager
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    /// <summary>
    /// URL输入框提交
    /// </summary>
    void OnUrlSubmit()
    {
        if (urlInputField != null)
        {
            string url = urlInputField.text.Trim();
            NavigateToUrl(url);
        }
    }

    /// <summary>
    /// 首页搜索框提交
    /// </summary>
    void OnSearchSubmit()
    {
        if (searchInputField != null)
        {
            string keyword = searchInputField.text.Trim();
            NavigateToUrl(keyword);
        }
    }

    /// <summary>
    /// 导航到指定URL (public方法,可被其他脚本调用)
    /// </summary>
    public void NavigateToUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("URL为空!");
            return;
        }

        string trimmedOriginalUrl = url.Trim();
        string lowerOriginalUrl = trimmedOriginalUrl.ToLower();

        // 第二章特殊URL：SQL Client 下载
        if (isChapter2 && IsSqlClientDownloadUrl(lowerOriginalUrl))
        {
            SyncInputFields(trimmedOriginalUrl);
            TriggerSqlClientDownload();
            return;
        }

        // 清理URL (去除http://、https://、www.等前缀)
        string cleanedUrl = CleanUrl(trimmedOriginalUrl);

        Debug.Log($"尝试访问: {cleanedUrl} (原始输入: {url})");

        // 同步两个输入框
        SyncInputFields(trimmedOriginalUrl);

        // 检查URL是否在有效列表中
        if (IsValidUrl(cleanedUrl))
        {
            // 检查是否是新页面关键词（使用原始URL进行匹配）
            if (url == "付简" || cleanedUrl == "付简")
            {
                ShowFujianPage();
                SaveBrowserHistory(cleanedUrl);
            }
            else if (url == "付觅" || cleanedUrl == "付觅")
            {
                ShowFumeiPage();
                SaveBrowserHistory(cleanedUrl);
            }
            else if (url == "关钦" || cleanedUrl == "关钦")
            {
                ShowGuanqinPage();
                SaveBrowserHistory(cleanedUrl);
            }
            else if (url == "微信维修" || url == "weixinweixiu" || cleanedUrl == "微信维修" || cleanedUrl == "weixinweixiu")
            {
                ShowWeixinRepairPage();
                SaveBrowserHistory(cleanedUrl);
            }
            else if (isChapter2 && (cleanedUrl == CHAT_RESTORE_URL_SHORT || lowerOriginalUrl.Contains(CHAT_RESTORE_URL)))
            {
                // 第二章专用关键词：触发微信维修流程
                ShowWeixinRepairPage();
                SaveBrowserHistory(cleanedUrl);
            }
            else
            {
                // 默认显示公司页面
                ShowCompanyPage();
                SaveBrowserHistory(cleanedUrl);
            }
        }
        else
        {
            // 未找到页面,显示404
            Show404Page();
            // 404页面不保存到历史记录
        }

        currentUrl = cleanedUrl;
    }

    /// <summary>
    /// 同步两个输入框的值
    /// </summary>
    void SyncInputFields(string value)
    {
        // 临时移除监听器,避免循环触发
        if (urlInputField != null)
        {
            urlInputField.onValueChanged.RemoveListener(OnUrlInputChanged);
            urlInputField.text = value;
            urlInputField.onValueChanged.AddListener(OnUrlInputChanged);
        }

        if (searchInputField != null)
        {
            searchInputField.onValueChanged.RemoveListener(OnSearchInputChanged);
            searchInputField.text = value;
            searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
        }
    }

    /// <summary>
    /// 清理URL,提取关键部分
    /// </summary>
    string CleanUrl(string url)
    {
        url = url.ToLower().Trim();

        // 移除常见前缀
        string[] prefixes = { "https://", "http://", "www.", "https://www.", "http://www." };
        foreach (var prefix in prefixes)
        {
            if (url.StartsWith(prefix))
            {
                url = url.Substring(prefix.Length);
            }
        }

        // 移除尾部的斜杠和.com/.cn等后缀
        url = url.TrimEnd('/');
        string[] suffixes = { ".com", ".cn", ".net", ".org", ".com.cn" };
        foreach (var suffix in suffixes)
        {
            if (url.EndsWith(suffix))
            {
                url = url.Substring(0, url.Length - suffix.Length);
            }
        }

        return url;
    }

    /// <summary>
    /// 检查URL是否有效
    /// 第一章：只能搜索公司相关关键词
    /// 第二章：可以搜索公司关键词 + 第二章特殊关键词
    /// </summary>
    bool IsValidUrl(string url)
    {
        // 检查是否完全包含拼音（所有章节通用）
        if (url.Contains("syxtyulegongsi"))
        {
            return true;
        }

        // 检查是否完全包含中文全名（所有章节通用）
        if (url.Contains("闪耀星途娱乐公司"))
        {
            return true;
        }

        // 第二章专属关键词检查
        if (isChapter2)
        {
            // 检查第二章页面关键词
            if (url.Contains("付简") || url.Contains("付觅") || url.Contains("关钦") ||
                url.Contains("微信维修") || url.Contains("weixinweixiu"))
            {
                return true;
            }

            // 检查第二章特殊URL（chat.kuro.secret相关）
            if (url.Contains("chat.kuro.secret"))
            {
                return true;
            }

            // 检查SQL Client下载链接
            if (url.Contains("sql-client.666"))
            {
                return true;
            }

            // 检查validUrls列表中的其他关键词（作为备用检查）
            string lowerUrl = url.ToLower();
            if (validUrls.Contains(lowerUrl))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 显示首页
    /// </summary>
    void ShowHomePage()
    {
        Debug.Log("显示浏览器首页");

        // 显示首页
        if (homePage != null) homePage.SetActive(true);

        // 隐藏所有其他页面
        HideAllPages();

        // 重新显示首页（因为HideAllPages会隐藏它）
        if (homePage != null) homePage.SetActive(true);

        // 清空输入框
        SyncInputFields("");

        UpdateChapter2UI();

        currentUrl = "";
        NotifyPageChange(BrowserPageType.Home);
    }

    /// <summary>
    /// 显示公司页面
    /// </summary>
    void ShowCompanyPage()
    {
        Debug.Log("显示公司页面");

        // 隐藏其他页面
        if (homePage != null) homePage.SetActive(false);
        if (notFoundPage != null) notFoundPage.SetActive(false);

        // 显示公司页面
        if (companyPage != null) companyPage.SetActive(true);
        NotifyPageChange(BrowserPageType.Company);
    }

    /// <summary>
    /// 显示404页面
    /// </summary>
    void Show404Page()
    {
        Debug.Log($"404 - 页面不存在: {currentUrl}");

        // 隐藏其他页面
        if (homePage != null) homePage.SetActive(false);
        if (companyPage != null) companyPage.SetActive(false);

        // 显示404页面
        if (notFoundPage != null) notFoundPage.SetActive(true);
        NotifyPageChange(BrowserPageType.NotFound);
    }

    void NotifyPageChange(BrowserPageType pageType)
    {
        currentPage = pageType;
        OnBrowserPageChanged?.Invoke(pageType);
    }

    /// <summary>
    /// 保存浏览历史到DataManager
    /// </summary>
    void SaveBrowserHistory(string url)
    {
        if (GameManager.Instance == null || GameManager.Instance.dataManager == null)
        {
            return;
        }

        var saveData = GameManager.Instance.dataManager.GetSaveData();
        if (saveData != null)
        {
            if (saveData.browserHistory == null)
            {
                saveData.browserHistory = new List<string>();
            }

            if (!saveData.browserHistory.Contains(url))
            {
                saveData.browserHistory.Add(url);
                GameManager.Instance.dataManager.SaveGameData();
                Debug.Log($"保存浏览历史: {url}");
            }
        }
    }

    /// <summary>
    /// 添加有效URL (可用于扩展)
    /// </summary>
    public void AddValidUrl(string url)
    {
        if (AddValidUrlVariant(url))
        {
            Debug.Log($"添加有效URL: {url}");
        }
    }

    /// <summary>
    /// 检查URL是否存在
    /// </summary>
    public bool UrlExists(string url)
    {
        string cleaned = CleanUrl(url);
        return IsValidUrl(cleaned);
    }

    // ==================== 第二章功能 ====================

    /// <summary>
    /// 章节切换事件处理
    /// </summary>
    void OnChapter2Started()
    {
        isChapter2 = true;
        Debug.Log("[BrowserApp] 检测到第二章，启用 chat.kuro.secret.com 关键词功能");
        UpdateChapter2UI();
    }

    
    /// <summary>
    /// 确认恢复聊天记录（第四步 - 切换微信）
    /// </summary>
    void OnChatRestoreConfirmed()
    {
        Debug.Log("[BrowserApp] 玩家确认恢复聊天记录，切换到微信2");

        // 隐藏恢复面板
        if (chatRestorePanel != null)
        {
            chatRestorePanel.SetActive(false);
        }

        // 隐藏黑屏面板
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(false);
        }

        // 保存聊天记录恢复状态（存档点）
        MarkChatRestoreCompleted();

        // 切换到微信2应用
        TriggerWeixin2App();
    }

    /// <summary>
    /// 标记聊天记录恢复完成（存档点）
    /// </summary>
    void MarkChatRestoreCompleted()
    {
        if (GameManager.Instance?.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved("browser_chat_restore_completed");
            Debug.Log("[BrowserApp] 聊天记录恢复状态已保存");
        }
    }

    /// <summary>
    /// 取消恢复聊天记录
    /// </summary>
    void OnChatRestoreCancelled()
    {
        Debug.Log("[BrowserApp] 玩家取消恢复聊天记录");

        // 隐藏恢复面板
        if (chatRestorePanel != null)
        {
            chatRestorePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 密码提交
    /// </summary>
    void OnPasswordSubmit()
    {
        if (passwordInput == null)
        {
            Debug.LogWarning("[BrowserApp] 密码输入框未配置");
            return;
        }

        string inputPassword = passwordInput.text.Trim();
        Debug.Log($"[BrowserApp] 尝试密码: {inputPassword}");

        if (inputPassword == WEIXIN_REPAIR_PASSWORD)
        {
            Debug.Log("[BrowserApp] 密码正确！进入第三步 - 聊天恢复确认");

            // 标记密码已验证
            MarkPasswordVerified();

            // 隐藏密码面板
            if (passwordPanel != null)
            {
                passwordPanel.SetActive(false);
            }

            // 显示聊天恢复确认面板
            StartCoroutine(ShowChatRestoreConfirmation());
        }
        else
        {
            Debug.LogWarning("[BrowserApp] 密码错误");

            // 显示密码错误面板
            if (passwordErrorPanel != null)
            {
                passwordErrorPanel.SetActive(true);
            }

            // 隐藏密码输入面板
            if (passwordPanel != null)
            {
                passwordPanel.SetActive(false);
            }

            // 清空输入框
            passwordInput.text = "";
        }
    }

    /// <summary>
    /// 密码取消
    /// </summary>
    void OnPasswordCancel()
    {
        Debug.Log("[BrowserApp] 取消密码输入");

        // 隐藏密码面板
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 密码错误确认
    /// </summary>
    void OnPasswordErrorConfirm()
    {
        Debug.Log("[BrowserApp] 确认密码错误");

        // 隐藏密码错误面板
        if (passwordErrorPanel != null)
        {
            passwordErrorPanel.SetActive(false);
        }

        // 重新显示密码输入面板
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(true);
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
    }


    // ==================== 新页面功能 ====================

    /// <summary>
    /// 显示付简页面
    /// </summary>
    void ShowFujianPage()
    {
        Debug.Log("显示付简页面");

        // 隐藏其他页面
        HideAllPages();

        // 显示付简页面
        if (fujianPage != null) fujianPage.SetActive(true);
        NotifyPageChange(BrowserPageType.FujianPage);
    }

    /// <summary>
    /// 显示付觅页面
    /// </summary>
    void ShowFumeiPage()
    {
        Debug.Log("显示付觅页面");

        // 隐藏其他页面
        HideAllPages();

        // 显示付觅页面
        if (fumeiPage != null) fumeiPage.SetActive(true);
        NotifyPageChange(BrowserPageType.FumeiPage);
    }

    /// <summary>
    /// 显示关钦页面（带照片切换逻辑）
    /// </summary>
    void ShowGuanqinPage()
    {
        Debug.Log($"显示关钦页面（当前章节：{(isChapter2 ? "第二章" : "第一章")}）");

        // 隐藏其他页面
        HideAllPages();

        // 显示关钦页面
        if (guanqinPage != null) guanqinPage.SetActive(true);

        if (isChapter2)
        {
            // 第二章：启动照片闪烁逻辑
            StartCoroutine(GuanqinPhotoSequence());
        }
        else
        {
            // 第一章：只显示静态页面，显示第一张照片
            Debug.Log("[BrowserApp] 第一章：显示关钦静态页面");
            if (guanqinPhoto1 != null)
            {
                guanqinPhoto1.gameObject.SetActive(true);
            }
        }

        NotifyPageChange(BrowserPageType.GuanqinPage);
    }

    /// <summary>
    /// 关钦照片序列逻辑 - 1234来回闪三轮，每轮0.5秒
    /// </summary>
    IEnumerator GuanqinPhotoSequence()
    {
        Image[] photos = { guanqinPhoto1, guanqinPhoto2, guanqinPhoto3, guanqinPhoto4 };

        // 初始状态：隐藏所有照片
        foreach (var photo in photos)
        {
            if (photo != null)
            {
                photo.gameObject.SetActive(false);
            }
        }

        // 进行三轮闪烁
        for (int round = 0; round < 3; round++)
        {
            Debug.Log($"[BrowserApp] 关钦页面：第{round + 1}轮闪烁开始");

            // 1→2→3→4
            for (int i = 0; i < photos.Length; i++)
            {
                if (photos[i] != null)
                {
                    photos[i].gameObject.SetActive(true);
                }

                yield return new WaitForSeconds(0.5f);

                if (photos[i] != null)
                {
                    photos[i].gameObject.SetActive(false);
                }
            }

            // 4→3→2→1
            for (int i = photos.Length - 1; i >= 0; i--)
            {
                if (photos[i] != null)
                {
                    photos[i].gameObject.SetActive(true);
                }

                yield return new WaitForSeconds(0.5f);

                if (photos[i] != null)
                {
                    photos[i].gameObject.SetActive(false);
                }
            }
        }

        Debug.Log("[BrowserApp] 关钦页面：三轮闪烁完成，显示关钦页面");

        // 闪烁完成后，显示关钦页面内容
        ShowGuanqinPageContent();
    }

    /// <summary>
    /// 显示关钦页面内容（闪烁完成后）
    /// </summary>
    void ShowGuanqinPageContent()
    {
        // 这里可以显示关钦页面的主要内容
        // 比如文字、背景图片等
        Debug.Log("[BrowserApp] 显示关钦页面主内容");
    }

    
    /// <summary>
    /// 显示微信维修页面（统一流程：黑屏2秒→密码输入→恢复确认→切换微信）
    /// </summary>
    void ShowWeixinRepairPage()
    {
        Debug.Log($"显示微信维修页面（当前章节：{(isChapter2 ? "第二章" : "第一章")}）");

        // 隐藏其他页面
        HideAllPages();

        // 调试信息
        if (weixinRepairPage == null)
        {
            Debug.LogError("[BrowserApp] WeixinRepairPage未配置！");
            return;
        }

        Debug.Log($"[BrowserApp] WeixinRepairPage状态：{weixinRepairPage.activeSelf}");
        Debug.Log($"[BrowserApp] WeixinRepairPage位置：{weixinRepairPage.transform.position}");
        Debug.Log($"[BrowserApp] WeixinRepairPage大小：{weixinRepairPage.GetComponent<RectTransform>().rect.size}");

        // 显示微信维修页面
        weixinRepairPage.SetActive(true);

        if (isChapter2)
        {
            // 第二章：启动统一流程（黑屏→密码→恢复→切换）
            StartCoroutine(WeixinRepairUnifiedSequence());
        }
        else
        {
            // 第一章：只显示微信维修页面，不启动特殊流程
            Debug.Log("[BrowserApp] 第一章：显示微信维修基础页面");
        }

        NotifyPageChange(BrowserPageType.WeixinRepairPage);
    }

    /// <summary>
    /// 微信维修统一流程：黑屏2秒→密码输入→恢复确认→切换微信
    /// </summary>
    IEnumerator WeixinRepairUnifiedSequence()
    {
        // 第一步：黑屏提示2秒
        Debug.Log("[BrowserApp] 微信维修流程：第一步 - 黑屏提示");

        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
        }

        // 显示提示图片2秒
        if (weixinRepairTipImage != null)
        {
            weixinRepairTipImage.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        // 第二步：显示密码输入
        Debug.Log("[BrowserApp] 微信维修流程：第二步 - 密码输入");

        if (weixinRepairTipImage != null)
        {
            weixinRepairTipImage.gameObject.SetActive(false);
        }

        if (passwordPanel != null)
        {
            passwordPanel.SetActive(true);

            // 设置密码输入框
            if (passwordInput != null)
            {
                passwordInput.text = "";
                passwordInput.Select();
                passwordInput.ActivateInputField();
            }

            // 设置提示文字
            if (passwordPromptText != null)
            {
                passwordPromptText.text = "请输入维修密码";
            }

            // 初始化密码错误面板（默认隐藏）
            if (passwordErrorPanel != null)
            {
                passwordErrorPanel.SetActive(false);
            }
        }

        // 等待密码验证完成（在密码提交方法中继续流程）
    }

    /// <summary>
    /// 微信维修密码提交
    /// </summary>
    void OnWeixinPasswordSubmit()
    {
        if (passwordInput == null)
        {
            Debug.LogWarning("[BrowserApp] 密码输入框未配置");
            return;
        }

        string inputPassword = passwordInput.text.Trim();
        Debug.Log($"[BrowserApp] 尝试密码: {inputPassword}");

        if (inputPassword == WEIXIN_REPAIR_PASSWORD)
        {
            Debug.Log("[BrowserApp] 密码正确！进入第三步 - 聊天恢复确认");

            // 标记密码已验证
            MarkPasswordVerified();

            // 隐藏密码面板
            if (passwordPanel != null)
            {
                passwordPanel.SetActive(false);
            }

            // 显示聊天恢复确认面板
            StartCoroutine(ShowChatRestoreConfirmation());
        }
        else
        {
            Debug.LogWarning("[BrowserApp] 密码错误");

            // 显示密码错误面板
            if (passwordErrorPanel != null)
            {
                passwordErrorPanel.SetActive(true);
            }

            // 隐藏密码输入面板
            if (passwordPanel != null)
            {
                passwordPanel.SetActive(false);
            }

            // 清空输入框
            passwordInput.text = "";
        }
    }

    /// <summary>
    /// 微信维修密码取消（统一流程）
    /// </summary>
    void OnWeixinPasswordCancel()
    {
        Debug.Log("[BrowserApp] 取消密码输入");

        // 隐藏所有面板
        if (passwordPanel != null) passwordPanel.SetActive(false);
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);

        // 返回首页
        ShowHomePage();
    }

    /// <summary>
    /// 显示聊天恢复确认（第三步）
    /// </summary>
    IEnumerator ShowChatRestoreConfirmation()
    {
        Debug.Log("[BrowserApp] 微信维修流程：第三步 - 聊天恢复确认");

        // 显示聊天恢复确认面板
        if (chatRestorePanel != null)
        {
            chatRestorePanel.SetActive(true);
        }

        yield return null; // 立即显示
    }

    /// <summary>
    /// 标记密码已验证（存档点）
    /// </summary>
    void MarkPasswordVerified()
    {
        if (GameManager.Instance?.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved("weixin_repair_password");
            Debug.Log("[BrowserApp] 密码验证状态已保存");
        }
    }

    /// <summary>
    /// 触发微信2应用
    /// </summary>
    void TriggerWeixin2App()
    {
        Debug.Log("[BrowserApp] 聊天恢复确认：切换到第二章聊天应用");

        // 调用AppManager切换到第二章聊天应用
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.SwitchToChapter2Chat();
        }
        else
        {
            Debug.LogError("[BrowserApp] AppManager未找到，无法切换到第二章聊天应用");
        }

        // 关闭浏览器
        CloseBrowser();
    }

    /// <summary>
    /// 隐藏所有页面
    /// </summary>
    void HideAllPages()
    {
        if (homePage != null) homePage.SetActive(false);
        if (companyPage != null) companyPage.SetActive(false);
        if (notFoundPage != null) notFoundPage.SetActive(false);
        if (fujianPage != null) fujianPage.SetActive(false);
        if (fumeiPage != null) fumeiPage.SetActive(false);
        if (guanqinPage != null) guanqinPage.SetActive(false);
        if (weixinRepairPage != null) weixinRepairPage.SetActive(false);
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);

        // 隐藏关钦的所有照片
        if (guanqinPhoto1 != null) guanqinPhoto1.gameObject.SetActive(false);
        if (guanqinPhoto2 != null) guanqinPhoto2.gameObject.SetActive(false);
        if (guanqinPhoto3 != null) guanqinPhoto3.gameObject.SetActive(false);
        if (guanqinPhoto4 != null) guanqinPhoto4.gameObject.SetActive(false);

        // 隐藏密码面板相关的所有UI
        if (passwordPanel != null) passwordPanel.SetActive(false);
        if (passwordErrorPanel != null) passwordErrorPanel.SetActive(false);
        if (chatRestorePanel != null) chatRestorePanel.SetActive(false);
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
    }

    bool IsSqlClientDownloadUrl(string rawUrl)
    {
        if (string.IsNullOrEmpty(rawUrl))
        {
            return false;
        }

        string lower = rawUrl.ToLower();
        return lower.Contains(SQL_CLIENT_DOWNLOAD_URL) || lower.Contains(SQL_CLIENT_DOWNLOAD_URL_SHORT);
    }

    void TriggerSqlClientDownload()
    {
        if (isSqlClientDownloadStarted)
        {
            ShowHomePage();
            return;
        }

        isSqlClientDownloadStarted = true;

        if (GameManager.Instance?.dataManager != null)
        {
            GameManager.Instance.dataManager.SetBool(SQL_CLIENT_DOWNLOAD_FLAG, true);
        }

        ShowHomePage();
        Debug.Log("[BrowserApp] 已开始下载 SQL Client");
    }

    void LoadSqlClientDownloadState()
    {
        if (GameManager.Instance?.dataManager != null)
        {
            isSqlClientDownloadStarted = GameManager.Instance.dataManager.GetBool(SQL_CLIENT_DOWNLOAD_FLAG, false);
        }
        else
        {
            isSqlClientDownloadStarted = false;
        }

        UpdateChapter2UI();
    }

    void UpdateSqlClientDownloadUI()
    {
        if (sqlClientDownloadPage == null)
        {
            return;
        }

        bool shouldShow = isChapter2 && isSqlClientDownloadStarted;
        sqlClientDownloadPage.SetActive(shouldShow);

        if (sqlClientDownloadOverlayPanel != null)
        {
            sqlClientDownloadOverlayPanel.SetActive(false);
        }
    }

    void OnSqlClientDownloadButtonClicked()
    {
        if (sqlClientDownloadOverlayPanel != null)
        {
            sqlClientDownloadOverlayPanel.SetActive(true);
        }
    }

    void UpdateChapter2UI()
    {
        UpdateSqlClientDownloadUI();

        if (chapter2PhotoWidget != null)
        {
            chapter2PhotoWidget.SetActive(isChapter2);
        }
    }
}
