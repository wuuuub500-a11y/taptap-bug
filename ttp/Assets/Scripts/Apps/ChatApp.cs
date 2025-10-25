using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 聊天应用 - 基于长图滚动显示聊天记录
/// 参考BlogApp的设计模式
/// </summary>
public class ChatApp : MonoBehaviour
{
    [Header("主要UI组件")]
    public Button closeButton;

        [Header("联系人按钮（固定，不滚动）")]
    public Button[] contactButtons;           // 联系人点击按钮 (关钦、付简、付觅、漫英)
           // 联系人点击区域 (关钦、付简、付觅、漫英)

        [Header("每个联系人独立的聊天记录面板")]
    public GameObject guanqinChatPanel;       // 关钦聊天记录面板 (包含ScrollRect和可点击区域)
    public GameObject fujianChatPanel;        // 付简聊天记录面板
    public GameObject fumeiChatPanel;         // 付觅聊天记录面板
    public GameObject manyingChatPanel;       // 漫英聊天记录面板

    [Header("聊天记录返回按钮")]
    public Button guanqinBackButton;          // 关钦聊天记录返回按钮
    public Button fujianBackButton;           // 付简聊天记录返回按钮
    public Button fumeiBackButton;            // 付觅聊天记录返回按钮
    public Button manyingBackButton;          // 漫英聊天记录返回按钮

    [Header("关钦聊天记录可点击区域")]
    public Button guanqinClickArea_Questionnaire;  // 问卷链接

    [Header("付简聊天记录可点击区域")]
    public Button fujianClickArea_Speech;          // 演讲图片

    [Header("付觅聊天记录可点击区域")]
    public Button fumeiClickArea_Contract;         // 签约合同链接

    [Header("漫英聊天记录可点击区域")]
    public Button manyingClickArea_Photo;          // 漫英聊天图片

    [Header("聊天记录内可点击图片资源")]
    public Sprite fujianPhotoSprite;         // 付简聊天图片
    public Sprite fumeiPhotoSprite;          // 付觅聊天图片
    public Sprite manyingPhotoSprite;        // 漫英聊天图片

    [Header("图片查看器")]
    public GameObject imageViewerPanel;       // 大图查看器面板
    public Image imageViewerImage;            // 显示大图的Image
    public Button imageViewerCloseButton;     // 关闭大图按钮

    [Header("问卷面板")]
    public GameObject questionnairePanel;           // 问卷面板
    public Image questionnaireImage;                // 情感测试问卷.PNG
    public TMP_InputField question1InputField;      // 问题1输入框
    public TMP_InputField question2InputField;      // 问题2输入框
    public TMP_InputField question3InputField;      // 问题3输入框
    public TMP_InputField question4InputField;      // 问题4输入框
    public Button submitButton;                     // 提交按钮
    public Button closeQuestionnaireButton;         // 问卷关闭按钮

    [Header("隐藏聊天面板")]
    public GameObject hiddenChatPanel;              // 隐藏聊天记录面板
    public Image hiddenChatImage;                   // 隐藏聊天记录.PNG
    public Button closeHiddenChatButton;            // 隐藏聊天关闭按钮

    
    [Header("密码面板")]
    public GameObject passwordPanel;                // 密码输入面板
    public TMP_InputField passwordInputField;       // 密码输入框
    public Button passwordConfirmButton;            // 密码确认按钮
    public Button passwordCancelButton;             // 密码取消按钮

    [Header("密码错误提示")]
    public GameObject passwordErrorPanel;           // 密码错误提示面板
    public Image passwordErrorImage;                // 密码错误.jpg
    public Button passwordErrorCloseButton;         // 密码错误关闭按钮
    public Button passwordErrorConfirmButton;       // 密码错误确认按钮

    // 状态管理
    private string currentContactId = "";
    private bool isGuanqinHiddenUnlocked = false;
    private bool isChatAppUnlocked = false;  // 聊天应用是否已解锁
    private const string CHAT_PASSWORD = "132199";  // 聊天应用密码
    private bool uiEventsInitialized = false;
    private bool hasInitialized = false;
    private bool isQuestionnaireErrorActive = false;

    // 调试用 - 监测输入框内容
    private string lastPasswordInput = "";

void Start()
    {
        LoadUnlockState();
        EnsureUIEventsBound();

        if (!isChatAppUnlocked)
        {
            ShowPasswordPanel();
        }
        else
        {
            InitializeChat();
        }

        hasInitialized = true;
    }

    void Update()
    {
        // 调试用：实时显示输入框内容
        if (passwordPanel != null && passwordPanel.activeSelf && passwordInputField != null)
        {
            string currentInput = passwordInputField.text;
            if (currentInput != lastPasswordInput)
            {
                lastPasswordInput = currentInput;
                Debug.Log($"[实时监测] 密码输入框内容变化: [{currentInput}]");
            }
        }
    }

    void OnPasswordSubmit()
    {
        ValidatePassword();
    }

    void OnEnable()
    {
        EnsureUIEventsBound();

        if (!hasInitialized)
        {
            return;
        }

        if (isChatAppUnlocked)
        {
            InitializeChat();
        }
        else
        {
            ShowPasswordPanel();
        }
    }

    /// <summary>
    /// 加载解锁状态
    /// </summary>
    void LoadUnlockState()
    {
        var dataManager = GameManager.Instance != null ? GameManager.Instance.dataManager : null;

        if (dataManager != null)
        {
            isChatAppUnlocked = dataManager.GetBool("ChatApp_Unlocked", false);
            isGuanqinHiddenUnlocked = dataManager.GetBool("ChatApp_Guanqin_Hidden_Unlocked", false);
            Debug.Log($"聊天应用解锁状态: {isChatAppUnlocked}, 关钦隐藏聊天解锁状态: {isGuanqinHiddenUnlocked}");
        }
        else
        {
            isChatAppUnlocked = false;
            isGuanqinHiddenUnlocked = false;
        }
    }

    /// <summary>
    /// 保存解锁状态
    /// </summary>
    void SaveUnlockState()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.SetBool("ChatApp_Unlocked", true);
            GameManager.Instance.dataManager.MarkPasswordSolved("chat");
            Debug.Log("聊天应用已解锁并保存");
        }
    }

    /// <summary>
    /// 重置解锁状态（用于测试和调试）
    /// 可以在Unity编辑器的Inspector面板中右键点击脚本，选择"Reset Unlock State"来调用
    /// </summary>
    [ContextMenu("重置聊天应用解锁状态")]
    public void ResetUnlockState()
    {
        isChatAppUnlocked = false;

        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.SetBool("ChatApp_Unlocked", false);
            Debug.Log("✅ 聊天应用解锁状态已重置为未解锁");
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager或DataManager不存在，无法重置保存的状态");
        }
    }

    /// <summary>
    /// 保存隐藏聊天解锁状态
    /// </summary>
    void SaveHiddenChatUnlockState()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.SetBool("ChatApp_Guanqin_Hidden_Unlocked", true);
            GameManager.Instance.dataManager.MarkPasswordSolved("chat_hidden_guanqin");
            Debug.Log("关钦隐藏聊天记录已解锁并保存");
        }
    }

    /// <summary>
    /// 显示密码面板
    /// </summary>
    void ShowPasswordPanel()
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(true);

            // 清空密码输入框
            if (passwordInputField != null)
            {
                passwordInputField.text = "";
                FocusPasswordInputField();
            }
            else
            {
                Debug.LogError("❌ 密码输入框(passwordInputField)未在Inspector中配置!");
            }

            // 隐藏其他所有面板
            HideAllPanels();

            // 确保错误提示被隐藏
            HidePasswordErrorImmediate();

            Debug.Log("显示密码输入面板");
        }
        else
        {
            Debug.LogWarning("密码面板未配置,直接进入聊天应用");
            InitializeChat();
        }
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    void ValidatePassword()
    {
        if (passwordInputField == null)
        {
            Debug.LogWarning("密码输入框未配置!");
            return;
        }

        // 详细调试信息
        Debug.Log($"=== 密码验证调试信息 ===");
        Debug.Log($"InputField对象: {passwordInputField.name}");
        Debug.Log($"InputField.text原始值: [{passwordInputField.text}]");
        Debug.Log($"InputField.text长度: {passwordInputField.text.Length}");
        Debug.Log($"正确密码: [{CHAT_PASSWORD}]");

        string inputPassword = passwordInputField.text.Trim();
        Debug.Log($"输入的密码(Trim后): [{inputPassword}]");
        Debug.Log($"输入密码长度: {inputPassword.Length}");

        if (inputPassword == CHAT_PASSWORD)
        {
            Debug.Log("✅ 密码正确!解锁聊天应用");

            // 标记为已解锁
            isChatAppUnlocked = true;
            SaveUnlockState();

            // 记录密码已破解
            GameManager.Instance?.dataManager?.MarkPasswordSolved("chat");

            // 隐藏密码面板
            if (passwordPanel != null)
            {
                passwordPanel.SetActive(false);
            }

            // 初始化聊天应用
            InitializeChat();
        }
        else
        {
            Debug.LogWarning($"❌ 密码错误! 输入:[{inputPassword}] 期望:[{CHAT_PASSWORD}]");

            // 显示密码错误提示
            if (passwordErrorPanel != null)
            {
                passwordErrorPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    void HideAllPanels()
    {
        if (questionnairePanel != null) questionnairePanel.SetActive(false);
        if (hiddenChatPanel != null) hiddenChatPanel.SetActive(false);
        if (passwordErrorPanel != null) passwordErrorPanel.SetActive(false);
        if (imageViewerPanel != null) imageViewerPanel.SetActive(false);
        
        if (contactButtons != null)
        {
            foreach (var btn in contactButtons)
            {
                if (btn != null) btn.gameObject.SetActive(false);
            }
        }
        
        HideAllChatPanels();
    }


    void InitializeChat()
    {
        SetupUIEvents();
        InitializePanels();
        ShowContactList();
    }

    /// <summary>
    /// 初始化所有面板的初始状态
    /// </summary>
    void InitializePanels()
    {
        // 隐藏所有弹窗
        if (questionnairePanel != null) questionnairePanel.SetActive(false);
        if (hiddenChatPanel != null) hiddenChatPanel.SetActive(false);
        if (passwordErrorPanel != null) passwordErrorPanel.SetActive(false);
        if (imageViewerPanel != null) imageViewerPanel.SetActive(false);

        // 显示联系人按钮
        if (contactButtons != null)
        {
            foreach (var btn in contactButtons)
            {
                if (btn != null) btn.gameObject.SetActive(true);
            }
        }

        // 隐藏所有聊天记录面板
        if (guanqinChatPanel != null) guanqinChatPanel.SetActive(false);
        if (fujianChatPanel != null) fujianChatPanel.SetActive(false);
        if (fumeiChatPanel != null) fumeiChatPanel.SetActive(false);
        if (manyingChatPanel != null) manyingChatPanel.SetActive(false);

        Debug.Log("聊天应用面板初始化完成");
    }

void SetupUIEvents()
    {
        if (uiEventsInitialized)
        {
            return;
        }

        uiEventsInitialized = true;

        // 关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseChat);
        }

        // 密码输入区域
        if (passwordConfirmButton != null)
        {
            passwordConfirmButton.onClick.AddListener(OnPasswordSubmit);
        }

        if (passwordCancelButton != null)
        {
            passwordCancelButton.onClick.AddListener(OnPasswordCancel);
        }

        if (passwordInputField != null)
        {
            passwordInputField.onSubmit.AddListener(_ => OnPasswordSubmit());
            passwordInputField.onValueChanged.AddListener(_ => HidePasswordErrorImmediate());
        }

        // 联系人按钮
        SetupContactButtons();

        // 聊天记录返回按钮
        if (guanqinBackButton != null)
        {
            guanqinBackButton.onClick.AddListener(BackToContactList);
        }
        if (fujianBackButton != null)
        {
            fujianBackButton.onClick.AddListener(BackToContactList);
        }
        if (fumeiBackButton != null)
        {
            fumeiBackButton.onClick.AddListener(BackToContactList);
        }
        if (manyingBackButton != null)
        {
            manyingBackButton.onClick.AddListener(BackToContactList);
        }

        // 关钦可点击区域
        if (guanqinClickArea_Questionnaire != null)
        {
            guanqinClickArea_Questionnaire.onClick.AddListener(OpenQuestionnaire);
        }

        // 付简可点击区域
        if (fujianClickArea_Speech != null)
        {
            fujianClickArea_Speech.onClick.AddListener(() => ShowChatImage(fujianPhotoSprite));
        }

        // 付觅可点击区域
        if (fumeiClickArea_Contract != null)
        {
            fumeiClickArea_Contract.onClick.AddListener(() => ShowChatImage(fumeiPhotoSprite));
        }

        // 漫英可点击区域
        if (manyingClickArea_Photo != null)
        {
            manyingClickArea_Photo.onClick.AddListener(() => ShowChatImage(manyingPhotoSprite));
        }

        // 问卷按钮
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitQuestionnaire);
        }

        if (closeQuestionnaireButton != null)
        {
            closeQuestionnaireButton.onClick.AddListener(CloseQuestionnairePanel);
        }

        // 隐藏聊天按钮
        if (closeHiddenChatButton != null)
        {
            closeHiddenChatButton.onClick.AddListener(CloseHiddenChatPanel);
        }

        // 密码错误按钮
        if (passwordErrorCloseButton != null)
        {
            passwordErrorCloseButton.onClick.AddListener(ClosePasswordErrorPanel);
        }

        if (passwordErrorConfirmButton != null)
        {
            passwordErrorConfirmButton.onClick.AddListener(OnPasswordErrorConfirm);
        }

        // 图片查看器关闭按钮
        if (imageViewerCloseButton != null)
        {
            imageViewerCloseButton.onClick.AddListener(CloseImageViewer);
        }
    }

    void EnsureUIEventsBound()
    {
        if (!uiEventsInitialized)
        {
            SetupUIEvents();
        }
    }

    void CloseChat()
    {
        gameObject.SetActive(false);
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    /// <summary>
    /// 显示联系人列表（固定按钮，不需要滚动）
    /// </summary>
    void ShowContactList()
    {
        // 显示联系人按钮区域
        if (contactButtons != null)
        {
            foreach (var btn in contactButtons)
            {
                if (btn != null) btn.gameObject.SetActive(true);
            }
        }

        // 隐藏所有聊天记录面板
        HideAllChatPanels();

        Debug.Log("显示联系人列表");
    }

    /// <summary>
    /// 设置联系人按钮
    /// </summary>
    void SetupContactButtons()
    {
        if (contactButtons == null || contactButtons.Length == 0)
        {
            return;
        }

        // 假设按钮顺序: 0=关钦, 1=付简, 2=付觅, 3=漫英
        if (contactButtons.Length > 0 && contactButtons[0] != null)
        {
            contactButtons[0].onClick.RemoveAllListeners();
            contactButtons[0].onClick.AddListener(() => OpenChat("guanqin"));
        }

        if (contactButtons.Length > 1 && contactButtons[1] != null)
        {
            contactButtons[1].onClick.RemoveAllListeners();
            contactButtons[1].onClick.AddListener(() => OpenChat("fujian"));
        }

        if (contactButtons.Length > 2 && contactButtons[2] != null)
        {
            contactButtons[2].onClick.RemoveAllListeners();
            contactButtons[2].onClick.AddListener(() => OpenChat("fumei"));
        }

        if (contactButtons.Length > 3 && contactButtons[3] != null)
        {
            contactButtons[3].onClick.RemoveAllListeners();
            contactButtons[3].onClick.AddListener(() => OpenChat("manying"));
        }

        Debug.Log($"设置了 {contactButtons.Length} 个联系人按钮");
    }

    /// <summary>
    /// 打开聊天记录 - 显示对应联系人的聊天面板
    /// </summary>
    void OpenChat(string contactId)
    {
        currentContactId = contactId;

        // 先隐藏所有聊天面板
        HideAllChatPanels();

        // 根据联系人ID显示对应的聊天面板
        GameObject panelToShow = null;
        switch (contactId)
        {
            case "guanqin":
                panelToShow = guanqinChatPanel;
                break;
            case "fujian":
                panelToShow = fujianChatPanel;
                break;
            case "fumei":
                panelToShow = fumeiChatPanel;
                break;
            case "manying":
                panelToShow = manyingChatPanel;
                break;
        }

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);

            // 滚动到顶部并刷新滚动区域
            ScrollRect scrollRect = panelToShow.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                RefreshScrollRect(scrollRect);
            }
        }

        Debug.Log($"打开聊天: {contactId}");
    }

    void RefreshScrollRect(ScrollRect scrollRect)
    {
        if (scrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;

        if (scrollRect.content != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        }

        StartCoroutine(RefreshScrollRectNextFrame(scrollRect));
    }

    IEnumerator RefreshScrollRectNextFrame(ScrollRect scrollRect)
    {
        yield return null;

        if (scrollRect == null)
        {
            yield break;
        }

        if (scrollRect.content != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        }

        if (scrollRect.viewport != null && scrollRect.content != null)
        {
            float contentHeight = scrollRect.content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            bool canScroll = contentHeight > viewportHeight;

            Debug.Log($"[ChatApp] ScrollRect刷新 -> Content: {contentHeight}, Viewport: {viewportHeight}, 可滚动: {canScroll}");

            scrollRect.vertical = canScroll;
            scrollRect.verticalScrollbar?.gameObject.SetActive(canScroll && scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent);
        }

        scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 返回联系人列表 - 隐藏所有聊天记录面板
    /// </summary>
    public void BackToContactList()
    {
        // 隐藏所有聊天记录面板
        HideAllChatPanels();

        // 清空当前联系人
        currentContactId = "";

        Debug.Log("返回联系人列表");
    }

    /// <summary>
    /// 隐藏所有聊天面板
    /// </summary>
    void HideAllChatPanels()
    {
        if (guanqinChatPanel != null) guanqinChatPanel.SetActive(false);
        if (fujianChatPanel != null) fujianChatPanel.SetActive(false);
        if (fumeiChatPanel != null) fumeiChatPanel.SetActive(false);
        if (manyingChatPanel != null) manyingChatPanel.SetActive(false);
    }

    /// <summary>
    /// 显示聊天图片大图
    /// </summary>
    void ShowChatImage(Sprite imageSprite)
    {
        if (imageViewerPanel == null || imageViewerImage == null)
        {
            Debug.LogWarning("图片查看器未配置!");
            return;
        }

        if (imageSprite == null)
        {
            Debug.LogWarning("图片Sprite未配置!");
            return;
        }

        // 设置大图
        imageViewerImage.sprite = imageSprite;

        // 显示图片查看器
        imageViewerPanel.SetActive(true);

        Debug.Log($"显示聊天图片大图: {imageSprite.name}");
    }


    /// <summary>
    /// 打开问卷
    /// </summary>
    void OpenQuestionnaire()
    {
        Debug.Log("打开问卷面板");

        // 显示问卷面板
        if (questionnairePanel != null)
        {
            questionnairePanel.SetActive(true);

            // 清空输入框
            if (question1InputField != null) question1InputField.text = "";
            if (question2InputField != null) question2InputField.text = "";
            if (question3InputField != null) question3InputField.text = "";
            if (question4InputField != null) question4InputField.text = "";
        }
        else
        {
            Debug.LogWarning("问卷面板未配置!");
        }
    }

    /// <summary>
    /// 提交问卷
    /// </summary>
    void SubmitQuestionnaire()
    {
        if (question1InputField == null || question2InputField == null ||
            question3InputField == null || question4InputField == null)
        {
            Debug.LogWarning("问卷输入框未配置!");
            return;
        }

        string answer1 = question1InputField.text.Trim();
        string answer2 = question2InputField.text.Trim();
        string answer3 = question3InputField.text.Trim();
        string answer4 = question4InputField.text.Trim();

        Debug.Log($"答案: 1={answer1}, 2={answer2}, 3={answer3}, 4={answer4}");

        // 正确答案 (从博客线索获取)
        // 答案1: 计算机 - 必须完全包含"计算机"这三个字
        // 答案2: 手表 - 必须完全包含"手表"这两个字(post_003)
        // 答案3: 我的少女时代 - 必须完全包含"我的少女时代"这七个字(post_004)
        // 答案4: 2015214 (post_004)
        bool isCorrect =
            answer1.Contains("计算机") &&
            answer2.Contains("手表") &&
            answer3.Contains("我的少女时代") &&
            answer4 == "2015214";

        if (isCorrect)
        {
            Debug.Log("问卷答对!解锁隐藏聊天记录");

            // 标记问卷完成
            GameManager.Instance?.dataManager?.MarkQuestionnaireCompleted("guanqin");

            // 关闭问卷
            CloseQuestionnairePanel();

            // 显示隐藏聊天记录面板
            if (hiddenChatPanel != null)
            {
                hiddenChatPanel.SetActive(true);
            }

            // 标记为已解锁并保存
            isGuanqinHiddenUnlocked = true;
            SaveHiddenChatUnlockState();
        }
        else
        {
            Debug.Log("问卷答错!显示错误提示");
            ShowQuestionnaireError();
        }
    }

    /// <summary>
    /// 关闭问卷面板
    /// </summary>
    void CloseQuestionnairePanel()
    {
        if (questionnairePanel != null)
        {
            questionnairePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 关闭隐藏聊天面板
    /// </summary>
    void CloseHiddenChatPanel()
    {
        if (hiddenChatPanel != null)
        {
            hiddenChatPanel.SetActive(false);
        }

        Debug.Log("隐藏聊天记录已查看");
    }

    /// <summary>
    /// 显示密码错误提示
    /// </summary>
    void ShowPasswordError()
    {
        isQuestionnaireErrorActive = false;

        if (passwordErrorPanel != null)
        {
            passwordErrorPanel.SetActive(true);
            Debug.Log("显示密码错误提示");
        }
        else
        {
            Debug.LogWarning("密码错误面板未配置!");
        }
    }

    void ShowQuestionnaireError()
    {
        isQuestionnaireErrorActive = true;

        if (questionnairePanel != null && !questionnairePanel.activeSelf)
        {
            questionnairePanel.SetActive(true);
        }

        if (passwordErrorPanel != null)
        {
            passwordErrorPanel.SetActive(true);
            Debug.Log("显示问卷错误提示");
        }
        else
        {
            Debug.LogWarning("问卷错误面板未配置!");
        }
    }

    /// <summary>
    /// 关闭密码错误提示
    /// </summary>
    void ClosePasswordErrorPanel()
    {
        if (passwordErrorPanel != null)
        {
            passwordErrorPanel.SetActive(false);
        }

        if (isQuestionnaireErrorActive)
        {
            isQuestionnaireErrorActive = false;

            if (questionnairePanel != null)
            {
                questionnairePanel.SetActive(false);
            }

            // 回到当前聊天记录
            if (!string.IsNullOrEmpty(currentContactId))
            {
                OpenChat(currentContactId);
            }

            return;
        }

        if (passwordPanel != null && !passwordPanel.activeSelf)
        {
            passwordPanel.SetActive(true);
        }

        if (passwordInputField != null)
        {
            passwordInputField.text = "";
            FocusPasswordInputField();
        }
    }

    void OnPasswordErrorConfirm()
    {
        ClosePasswordErrorPanel();
    }

    void OnPasswordCancel()
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(false);
        }

        CloseChat();
    }

    void FocusPasswordInputField()
    {
        if (passwordInputField == null)
        {
            return;
        }

        passwordInputField.Select();
        passwordInputField.ActivateInputField();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(passwordInputField.gameObject);
        }
    }

    void HidePasswordErrorImmediate()
    {
        if (passwordErrorPanel != null && passwordErrorPanel.activeSelf)
        {
            passwordErrorPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 解锁隐藏聊天记录（由QuestionnaireApp调用）
    /// </summary>
    public void UnlockHiddenChat(string contactId)
    {
        if (contactId == "guanqin")
        {
            isGuanqinHiddenUnlocked = true;
            SaveHiddenChatUnlockState();

            // 显示隐藏聊天记录面板
            if (hiddenChatPanel != null)
            {
                hiddenChatPanel.SetActive(true);
            }

            Debug.Log("解锁关钦的隐藏聊天记录");
        }
        else
        {
            Debug.LogWarning($"联系人 {contactId} 没有隐藏聊天记录");
        }
    }

    /// <summary>
    /// 检查隐藏聊天是否已解锁
    /// </summary>
    public bool IsHiddenChatUnlocked(string contactId)
    {
        if (contactId == "guanqin")
        {
            return isGuanqinHiddenUnlocked;
        }
        return false;
    }



    /// <summary>
    /// 关闭图片查看器
    /// </summary>
    void CloseImageViewer()
    {
        if (imageViewerPanel != null)
        {
            imageViewerPanel.SetActive(false);
        }
    }
}
