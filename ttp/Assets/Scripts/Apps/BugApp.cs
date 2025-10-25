using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Linq;

/// <summary>
/// BugApp - Bug视频通话应用
/// 游戏核心恐怖元素,完成所有任务后自动触发
/// 结合了VideoCallApp和BugAngelApp的功能
/// </summary>
public class BugApp : MonoBehaviour
{
    [Header("=== UI面板 ===")]
    public GameObject incomingCallPanel;        // 来电界面面板 (完整界面1)
    public GameObject videoCallPanel;           // 通话中面板 (完整界面2)

    [Header("=== 来电界面UI (完整界面1) ===")]
    public Image callerAvatarImage;             // 来电者头像 (圆形灰色)
    public TextMeshProUGUI incomingCallText;    // "向你发起了视频通话" 文字
    public Button acceptButton;                 // "接听" 按钮
    public Button closeButton;                  // 左上角关闭按钮

    [Header("=== 通话界面UI (完整界面2) ===")]
    public Image videoBackgroundImage;          // 视频背景区域 (黑色背景,可切换不同图片)
    public Image dialogueBubbleImage;        // 对话泡泡图片 (顶部显示)
    public Image keyframeImage;             // 玩家关键帧/静帧承载图层
    public Button playerContinueButton;     // 玩家点击继续按钮

    [Header("底部控制按钮")]
    public Button micButton;                    // 麦克风按钮
    public Button hangupButton;                 // 挂断按钮 (红色圆形)
    public Button speakerButton;                // 听筒按钮

    [Header("=== Bug特效资源 ===")]
    public Sprite[] bugVideoFrames;             // Bug视频帧序列 (用于播放恐怖画面)
    public AudioClip ringingSound;              // 来电铃声
    public AudioClip connectionSound;           // 接通音效
    public AudioClip[] glitchSounds;            // 故障音效
    [Header("=== 视频播放组件 ===")]
    public VideoPlayer videoPlayer;             // MP4视频播放器
    public RawImage videoOutput;                // 用于显示视频的RawImage
    public Sprite fallbackPausedSprite;         // 视频暂停时的默认静态画面

    [Header("=== 剧情对话数据 ===")]
    [SerializeField] private List<BugDialogueNode> dialogueNodes = new List<BugDialogueNode>();
    [SerializeField] private string startingNodeId = "player_hello";
    [Header("=== 第二章 剧情对话数据 ===")]
    [SerializeField] private List<BugDialogueNode> chapter2DialogueNodes = new List<BugDialogueNode>();
    [SerializeField] private string chapter2StartingNodeId = "chapter2_player_intro";

    public enum BugCallStage
    {
        Chapter1 = 1,
        Chapter2 = 2
    }

    void LoadBugCallState()
    {
        if (dataManager == null)
        {
            chapter1CallTriggeredFlag = false;
            chapter2CallTriggeredFlag = false;
            return;
        }

        chapter1CallTriggeredFlag = dataManager.GetBool(Chapter1CallFlagKey, false);
        chapter2CallTriggeredFlag = dataManager.GetBool(Chapter2CallFlagKey, false);

        if (!chapter1CallTriggeredFlag && dataManager.IsVideoCallTriggered())
        {
            chapter1CallTriggeredFlag = true;
            dataManager.SetBool(Chapter1CallFlagKey, true);
        }
    }

    bool AllBugCallStagesCompleted()
    {
        return chapter1CallTriggeredFlag && chapter2CallTriggeredFlag;
    }

    void EnsureBugTriggerMonitoring()
    {
        if (!IsInvoking(nameof(CheckBugTriggerConditions)))
        {
            InvokeRepeating(nameof(CheckBugTriggerConditions), 1f, 1f);
        }
    }

    bool EvaluateChapter1Requirements(GameSaveData saveData)
    {
        if (dataManager == null)
        {
            return false;
        }

        bool chatUnlocked = dataManager.IsPasswordSolved("chat") || dataManager.GetBool("ChatApp_Unlocked", false);
        bool questionnaireCompleted = dataManager.IsQuestionnaireCompleted("guanqin");

        bool photoUnlocked = false;
        if (saveData.photosUnlocked != null && saveData.photosUnlocked.Contains("life_mosaic"))
        {
            photoUnlocked = true;
        }
        else if (dataManager.IsPasswordSolved("photoalbum_life") || dataManager.IsPasswordSolved("gallery_life_photo"))
        {
            photoUnlocked = true;
        }

        bool browserVisited = saveData.browserHistory != null &&
                               saveData.browserHistory.Any(url => ContainsAnyKeyword(url, Chapter1BrowserKeywords));

        return chatUnlocked && questionnaireCompleted && photoUnlocked && browserVisited;
    }

    bool EvaluateChapter2Requirements(GameSaveData saveData)
    {
        if (dataManager == null)
        {
            return false;
        }

        if (saveData.currentChapter < 2)
        {
            return false;
        }

        foreach (var group in Chapter2PasswordGroups)
        {
            bool groupSolved = group.Any(id => dataManager.IsPasswordSolved(id));
            if (!groupSolved)
            {
                return false;
            }
        }

        return true;
    }

    bool ContainsAnyKeyword(string value, IEnumerable<string> keywords)
    {
        if (string.IsNullOrEmpty(value) || keywords == null)
        {
            return false;
        }

        string lower = value.ToLower();
        return keywords.Any(keyword => !string.IsNullOrEmpty(keyword) && lower.Contains(keyword.ToLower()));
    }

    void MarkBugCallStageTriggered(BugCallStage stage)
    {
        if (stage == BugCallStage.Chapter1)
        {
            if (!chapter1CallTriggeredFlag && dataManager != null)
            {
                dataManager.SetBool(Chapter1CallFlagKey, true);
            }

            chapter1CallTriggeredFlag = true;
        }
        else if (stage == BugCallStage.Chapter2)
        {
            if (!chapter2CallTriggeredFlag && dataManager != null)
            {
                dataManager.SetBool(Chapter2CallFlagKey, true);
            }

            chapter2CallTriggeredFlag = true;
        }

        if (AllBugCallStagesCompleted())
        {
            CancelInvoke(nameof(CheckBugTriggerConditions));
        }
    }

    private static readonly string[] Chapter1BrowserKeywords =
    {
        "syxtyulegongsi",
        "闪耀星途娱乐公司"
    };

    private static readonly string[][] Chapter2PasswordGroups =
    {
        new [] { "blog_seat_puzzle", "seat_puzzle" },
        new [] { "weixin_repair_password" },
        new [] { "browser_chat_restore_completed" },
        new [] { "photoalbum_private", "gallery_private_album" },
        new [] { "chat_hidden_guanqin" }
    };

    private const string Chapter1CallFlagKey = "BugApp_Chapter1_CallTriggered";
    private const string Chapter2CallFlagKey = "BugApp_Chapter2_CallTriggered";
    
    // === 事件通知 ===
    public event Action<BugDialogueNode> OnNodeStarted;
    public event Action<BugDialogueNode> OnNodeEnded;

    // === 状态管理 ===
    private bool isIncomingCall = false;
    private bool isCallActive = false;
    private BugDialogueNode currentNode = null;
    private readonly Dictionary<string, BugDialogueNode> dialogueLookup = new Dictionary<string, BugDialogueNode>();
    private Coroutine videoPlaybackCoroutine = null;
    private Coroutine autoAdvanceCoroutine = null;
    private bool isVideoPreparing = false;

    // === Bug触发条件检测 ===
    private bool chapter1TasksCompleted = false;
    private bool chapter2TasksCompleted = false;
    private bool chapter1CallTriggeredFlag = false;
    private bool chapter2CallTriggeredFlag = false;
    private bool bugCallScheduled = false;
    private bool bugCallTriggered = false;
    private BugCallStage? pendingBugCallStage = null;
    private BugCallStage? activeBugCallStage = null;
    private Coroutine bugCallCoroutine = null;
    [SerializeField] private bool debugShortcutEnabled = true;
    private Coroutine waitForSaveDataCoroutine = null;
    private bool waitingForPlayerContinue = false;
    private string pendingNextNodeId = null;

    // === 管理器缓存 ===
    private WindowManager windowManager;
    private AppManager appManager;
    private DataManager dataManager;
    private UIManager uiManager;
    private StoryManager storyManager;

    // === 音效播放器 ===
    private AudioSource audioSource;

    void Awake()
    {
        // 添加AudioSource组件
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        BrowserApp.OnBrowserPageChanged += HandleBrowserPageChanged;
    }

    void OnDisable()
    {
        BrowserApp.OnBrowserPageChanged -= HandleBrowserPageChanged;
    }

    void Start()
    {
        InitializeBugApp();
    }

    void Update()
    {
        HandleDebugShortcuts();
    }

    void InitializeBugApp()
    {
        CacheManagers();
        LoadBugCallState();
        SetupUIEvents();
        HideAllPanels();
        InitializeDialogues();
        SetupBugTriggerSystem();
    }

    /// <summary>
    /// 调试快捷键 (F) 强制触发来电
    /// </summary>
    void HandleDebugShortcuts()
    {
        if (!debugShortcutEnabled)
        {
            return;
        }

        bool shortcutPressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            shortcutPressed = true;
        }
#endif
        if (Input.GetKeyDown(KeyCode.F))
        {
            shortcutPressed = true;
        }

        if (!shortcutPressed)
        {
            return;
        }

        Debug.Log("[BugApp] Debug: F key pressed, forcing bug call.");

        CancelInvoke(nameof(CheckBugTriggerConditions));
        if (bugCallCoroutine != null)
        {
            StopCoroutine(bugCallCoroutine);
            bugCallCoroutine = null;
        }

        bugCallTriggered = false;
        bugCallScheduled = false;
        pendingBugCallStage = chapter1CallTriggeredFlag ? BugCallStage.Chapter2 : BugCallStage.Chapter1;
        isIncomingCall = false;
        isCallActive = false;

        TriggerBugCall();
    }

    /// <summary>
    /// 缓存所有管理器引用
    /// </summary>
    void CacheManagers()
    {
        if (GameManager.Instance != null)
        {
            dataManager = GameManager.Instance.dataManager;
            appManager = GameManager.Instance.appManager;
            uiManager = GameManager.Instance.uiManager;
            storyManager = GameManager.Instance.storyManager;
        }

        if (windowManager == null)
        {
            windowManager = FindObjectOfType<WindowManager>();
        }
    }

    /// <summary>
    /// 设置UI事件监听
    /// </summary>
    void SetupUIEvents()
    {
        // 来电界面
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(AcceptCall);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseAttempt);
        }

        // 通话界面 - 控制按钮
        if (hangupButton != null)
        {
            hangupButton.onClick.AddListener(HangupCall);
        }

        if (micButton != null)
        {
            micButton.onClick.AddListener(OnMicButtonClicked);
        }

        if (speakerButton != null)
        {
            speakerButton.onClick.AddListener(OnSpeakerButtonClicked);
        }

        if (playerContinueButton != null)
        {
            playerContinueButton.onClick.AddListener(OnPlayerContinueClicked);
            playerContinueButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    void HideAllPanels()
    {
        if (incomingCallPanel != null) incomingCallPanel.SetActive(false);
        if (videoCallPanel != null) videoCallPanel.SetActive(false);
        if (dialogueBubbleImage != null) dialogueBubbleImage.gameObject.SetActive(false);
        if (keyframeImage != null) keyframeImage.gameObject.SetActive(false);
        if (playerContinueButton != null) playerContinueButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 初始化剧情对话数据
    /// </summary>
    void InitializeDialogues()
    {
        if (dialogueNodes == null)
        {
            dialogueNodes = new List<BugDialogueNode>();
        }

        if (dialogueNodes.Count == 0)
        {
            dialogueNodes = CreateDefaultChapter1DialogueNodes();
        }

        if (chapter2DialogueNodes == null)
        {
            chapter2DialogueNodes = new List<BugDialogueNode>();
        }

        if (chapter2DialogueNodes.Count == 0)
        {
            chapter2DialogueNodes = CreateDefaultChapter2DialogueNodes();
        }

        var stageToInitialize = activeBugCallStage ?? BugCallStage.Chapter1;
        BuildDialogueLookupForStage(stageToInitialize);
    }

    void BuildDialogueLookupForStage(BugCallStage stage)
    {
        dialogueLookup.Clear();

        var nodes = GetDialogueNodesForStage(stage);
        if (nodes == null)
        {
            Debug.LogWarning($"[BugApp] 阶段 {stage} 未配置对话节点");
            return;
        }

        foreach (var node in nodes)
        {
            if (node == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(node.nodeId))
            {
                node.nodeId = Guid.NewGuid().ToString();
            }

            if (dialogueLookup.ContainsKey(node.nodeId))
            {
                Debug.LogWarning($"[BugApp] 阶段 {stage} 检测到重复的对话节点ID: {node.nodeId}");
                continue;
            }

            dialogueLookup[node.nodeId] = node;
        }

        Debug.Log($"[BugApp] 初始化了 {dialogueLookup.Count} 个对话节点 (阶段 {stage})");
    }

    List<BugDialogueNode> GetDialogueNodesForStage(BugCallStage stage)
    {
        switch (stage)
        {
            case BugCallStage.Chapter2:
                return chapter2DialogueNodes;
            case BugCallStage.Chapter1:
            default:
                return dialogueNodes;
        }
    }

    string GetStartingNodeIdForStage(BugCallStage stage)
    {
        if (stage == BugCallStage.Chapter2)
        {
            if (!string.IsNullOrWhiteSpace(chapter2StartingNodeId) && dialogueLookup.ContainsKey(chapter2StartingNodeId))
            {
                return chapter2StartingNodeId;
            }

            var nodes = GetDialogueNodesForStage(BugCallStage.Chapter2);
            var firstNode = nodes?.FirstOrDefault(n => n != null);
            return firstNode?.nodeId;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(startingNodeId) && dialogueLookup.ContainsKey(startingNodeId))
            {
                return startingNodeId;
            }

            var nodes = GetDialogueNodesForStage(BugCallStage.Chapter1);
            var firstNode = nodes?.FirstOrDefault(n => n != null);
            return firstNode?.nodeId;
        }
    }

    List<BugDialogueNode> CreateDefaultChapter1DialogueNodes()
    {
        return new List<BugDialogueNode>
        {
            new BugDialogueNode
            {
                nodeId = "player_hello",
                nodeType = BugDialogueNodeType.PlayerDialogue,
                speakerName = "淮特",                autoAdvanceDelay = 1.5f,
                nextNodeId = "bug_proud"
            },
            new BugDialogueNode
            {
                nodeId = "bug_proud",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 0,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "player_concern"
            },
            new BugDialogueNode
            {
                nodeId = "player_concern",
                nodeType = BugDialogueNodeType.PlayerDialogue,
                speakerName = "淮特",                autoAdvanceDelay = 1.3f,
                nextNodeId = "player_pressure"
            },
            new BugDialogueNode
            {
                nodeId = "player_pressure",
                nodeType = BugDialogueNodeType.PlayerDialogue,
                speakerName = "淮特",                autoAdvanceDelay = 1.3f,
                nextNodeId = "bug_pressure"
            },
            new BugDialogueNode
            {
                nodeId = "bug_pressure",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 1,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "player_comfort"
            },
            new BugDialogueNode
            {
                nodeId = "player_comfort",
                nodeType = BugDialogueNodeType.PlayerDialogue,
                speakerName = "淮特",                autoAdvanceDelay = 1.5f,
                nextNodeId = "bug_no"
            },
            new BugDialogueNode
            {
                nodeId = "bug_no",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 2,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_unknown"
            },
            new BugDialogueNode
            {
                nodeId = "bug_unknown",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 2,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_nothing"
            },
            new BugDialogueNode
            {
                nodeId = "bug_nothing",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 2,
                triggerShake = true,
                shakeIntensity = 6f,
                shakeDuration = 0.4f,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_unlock"
            },
            new BugDialogueNode
            {
                nodeId = "bug_unlock",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 3,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_enter"
            },
            new BugDialogueNode
            {
                nodeId = "bug_enter",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 3,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_decide"
            },
            new BugDialogueNode
            {
                nodeId = "bug_decide",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 3,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_learn"
            },
            new BugDialogueNode
            {
                nodeId = "bug_learn",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 4,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_deadline"
            },
            new BugDialogueNode
            {
                nodeId = "bug_deadline",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 4,
                triggerGlitch = true,
                triggerShake = true,
                shakeIntensity = 8f,
                shakeDuration = 0.6f,
                autoAdvanceDelay = 0.8f,
                nextNodeId = "bug_outro"
            },
            new BugDialogueNode
            {
                nodeId = "bug_outro",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",                fallbackFrameIndex = 4,
                autoAdvanceDelay = 1.2f,
                nextNodeId = null
            }
        };
    }

    List<BugDialogueNode> CreateDefaultChapter2DialogueNodes()
    {
        return new List<BugDialogueNode>
        {
            new BugDialogueNode
            {
                nodeId = "chapter2_player_intro",
                nodeType = BugDialogueNodeType.PlayerDialogue,
                speakerName = "淮特",
                autoAdvanceDelay = 1.5f,
                nextNodeId = "chapter2_bug_signal"
            },
            new BugDialogueNode
            {
                nodeId = "chapter2_bug_signal",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",
                fallbackFrameIndex = 0,
                autoAdvanceDelay = 1.0f,
                nextNodeId = "chapter2_player_react"
            },
            new BugDialogueNode
            {
                nodeId = "chapter2_player_react",
                nodeType = BugDialogueNodeType.PlayerDialogue,
                speakerName = "淮特",
                autoAdvanceDelay = 1.3f,
                nextNodeId = "chapter2_bug_command"
            },
            new BugDialogueNode
            {
                nodeId = "chapter2_bug_command",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",
                fallbackFrameIndex = 2,
                triggerShake = true,
                shakeIntensity = 7f,
                shakeDuration = 0.5f,
                autoAdvanceDelay = 0.9f,
                nextNodeId = "chapter2_bug_outro"
            },
            new BugDialogueNode
            {
                nodeId = "chapter2_bug_outro",
                nodeType = BugDialogueNodeType.BugVideo,
                speakerName = "Bug天使",
                fallbackFrameIndex = 4,
                autoAdvanceDelay = 1.2f,
                nextNodeId = null
            }
        };
    }

    // ==================== Bug触发系统 ====================

    /// <summary>
    /// 设置Bug触发系统
    /// </summary>
    void SetupBugTriggerSystem()
    {
        CacheManagers();

        if (dataManager == null)
        {
            Debug.LogWarning("[BugApp] DataManager 未就绪，延迟初始化Bug触发系统");
            AttemptWaitForSaveData();
            return;
        }

        if (dataManager.GetSaveData() == null)
        {
            Debug.Log("[BugApp] 存档尚未加载，等待后再初始化Bug触发系统");
            AttemptWaitForSaveData();
            return;
        }

        SetupBugTriggerSystemInternal();
    }

    void SetupBugTriggerSystemInternal()
    {
        LoadBugCallState();

        if (AllBugCallStagesCompleted())
        {
            Debug.Log("[BugApp] 所有Bug视频阶段已完成");
            return;
        }

        EnsureBugTriggerMonitoring();
    }

    void AttemptWaitForSaveData()
    {
        if (waitForSaveDataCoroutine != null)
        {
            return;
        }

        waitForSaveDataCoroutine = StartCoroutine(WaitForSaveDataRoutine());
    }

    IEnumerator WaitForSaveDataRoutine()
    {
        while (true)
        {
            CacheManagers();

            if (dataManager != null && dataManager.GetSaveData() != null)
            {
                break;
            }

            yield return null;
        }

        waitForSaveDataCoroutine = null;
        LoadBugCallState();
        SetupBugTriggerSystemInternal();
    }

    /// <summary>
    /// 检查Bug触发条件
    /// </summary>
    void CheckBugTriggerConditions()
    {
        if (bugCallTriggered)
        {
            return;
        }

        CacheManagers();

        var saveData = dataManager?.GetSaveData();
        if (saveData == null)
        {
            return;
        }

        bool chapter1CompleteNow = EvaluateChapter1Requirements(saveData);
        bool chapter2CompleteNow = EvaluateChapter2Requirements(saveData);

        if (chapter1CompleteNow != chapter1TasksCompleted)
        {
            chapter1TasksCompleted = chapter1CompleteNow;
            Debug.Log($"[BugApp] 第一章任务完成状态更新: {chapter1TasksCompleted}");
        }

        if (chapter2CompleteNow != chapter2TasksCompleted)
        {
            chapter2TasksCompleted = chapter2CompleteNow;
            Debug.Log($"[BugApp] 第二章任务完成状态更新: {chapter2TasksCompleted}");
        }

        if (!chapter1CallTriggeredFlag && chapter1TasksCompleted)
        {
            Debug.Log("[BugApp] ✅ 第一章所有任务完成，准备触发Bug视频通话");
            ScheduleBugCall(BugCallStage.Chapter1);
            return;
        }

        if (!chapter2CallTriggeredFlag && chapter2TasksCompleted)
        {
            Debug.Log("[BugApp] ✅ 第二章所有任务完成，准备触发Bug视频通话");
            ScheduleBugCall(BugCallStage.Chapter2);
        }
    }

    void ScheduleBugCall(BugCallStage stage)
    {
        if (bugCallTriggered)
        {
            Debug.Log($"[BugApp] Bug视频通话正在进行或尚未结束，忽略阶段 {stage} 的调度请求");
            return;
        }

        if (bugCallScheduled && pendingBugCallStage == stage)
        {
            return;
        }

        Debug.Log($"[BugApp] 所有触发条件达成，准备启动Bug视频通话（阶段 {stage}）");

        bugCallScheduled = true;
        pendingBugCallStage = stage;

        if (bugCallCoroutine != null)
        {
            StopCoroutine(bugCallCoroutine);
        }

        bugCallCoroutine = StartCoroutine(WaitForDesktopAndTrigger());
    }

    public void RequestBugCallForStage(BugCallStage stage)
    {
        CacheManagers();

        if (stage == BugCallStage.Chapter1 && chapter1CallTriggeredFlag)
        {
            return;
        }

        if (stage == BugCallStage.Chapter2 && chapter2CallTriggeredFlag)
        {
            return;
        }

        ScheduleBugCall(stage);
    }

    void HandleBrowserPageChanged(BrowserApp.BrowserPageType pageType)
    {
        if (bugCallTriggered || pageType != BrowserApp.BrowserPageType.Home)
        {
            return;
        }

        CacheManagers();

        var saveData = dataManager?.GetSaveData();
        if (saveData == null)
        {
            return;
        }

        if (!chapter1CallTriggeredFlag && EvaluateChapter1Requirements(saveData))
        {
            ScheduleBugCall(BugCallStage.Chapter1);
        }
        else if (!chapter2CallTriggeredFlag && EvaluateChapter2Requirements(saveData))
        {
            ScheduleBugCall(BugCallStage.Chapter2);
        }
    }

    /// <summary>
    /// 等待玩家返回桌面后触发Bug通话
    /// </summary>
    IEnumerator WaitForDesktopAndTrigger()
    {
        if (!pendingBugCallStage.HasValue)
        {
            yield break;
        }

        // 等待玩家关闭所有窗口
        while (!IsDesktopIdle())
        {
            yield return null;
        }

        // 等待0.5秒确保UI稳定
        yield return new WaitForSeconds(0.5f);

        // 触发Bug通话
        TriggerBugCall();
    }

    /// <summary>
    /// 检查桌面是否空闲 (无活动窗口)
    /// </summary>
    bool IsDesktopIdle()
    {
        if (windowManager == null)
        {
            windowManager = FindObjectOfType<WindowManager>();
        }

        bool windowIdle = windowManager == null || !windowManager.HasActiveWindow();
        bool appIdle = appManager == null || !appManager.HasActiveWindow();
        return windowIdle && appIdle;
    }

    /// <summary>
    /// 触发Bug通话
    /// </summary>
    void TriggerBugCall()
    {
        if (!pendingBugCallStage.HasValue && !activeBugCallStage.HasValue)
        {
            Debug.LogWarning("[BugApp] 没有待触发的Bug视频阶段");
            return;
        }

        BugCallStage stage = pendingBugCallStage ?? activeBugCallStage ?? BugCallStage.Chapter1;

        Debug.Log($"[BugApp] 触发Bug视频通话（阶段 {stage}）!");

        activeBugCallStage = stage;
        pendingBugCallStage = null;

        bugCallTriggered = true;
        bugCallScheduled = false;
        bugCallCoroutine = null;

        CancelInvoke(nameof(CheckBugTriggerConditions));

        CacheManagers();
        MarkBugCallStageTriggered(stage);

        if (dataManager != null)
        {
            dataManager.MarkVideoCallTriggered();
        }

        UnlockBugApp();
        OpenBugAppWindow();
        ShowIncomingCall();
    }

    /// <summary>
    /// 解锁bugapp图标
    /// </summary>
    void UnlockBugApp()
    {
        const string bugAppId = "bugapp";

        if (windowManager != null)
        {
            windowManager.UnlockApp(bugAppId);
        }
        else if (appManager != null)
        {
            appManager.UnlockApp(bugAppId);
        }

        Debug.Log("[BugApp] bugapp图标已解锁");
    }

    /// <summary>
    /// 打开bugapp窗口
    /// </summary>
    void OpenBugAppWindow()
    {
        gameObject.SetActive(true);
        Debug.Log("[BugApp] bugapp窗口已打开");
    }

    // ==================== 来电处理 ====================

    /// <summary>
    /// 显示来电界面
    /// </summary>
    void ShowIncomingCall()
    {
        if (isIncomingCall || isCallActive)
        {
            Debug.LogWarning("[BugApp] 已有通话进行中");
            return;
        }

        isIncomingCall = true;

        // 显示来电界面
        if (incomingCallPanel != null)
        {
            incomingCallPanel.SetActive(true);
        }

        // 设置来电文字
        if (incomingCallText != null)
        {
            incomingCallText.text = "向你发起了视频通话";
        }

        // 播放来电铃声
        PlaySound(ringingSound, true);

        // 开始闪烁效果
        StartCoroutine(IncomingCallFlashEffect());

        Debug.Log("[BugApp] 显示来电界面");
    }

    /// <summary>
    /// 来电闪烁效果
    /// </summary>
    IEnumerator IncomingCallFlashEffect()
    {
        CanvasGroup canvasGroup = incomingCallPanel?.GetComponent<CanvasGroup>();
        if (canvasGroup == null && incomingCallPanel != null)
        {
            canvasGroup = incomingCallPanel.AddComponent<CanvasGroup>();
        }

        while (isIncomingCall && !isCallActive)
        {
            if (canvasGroup != null)
            {
                float alpha = Mathf.PingPong(Time.time * 2f, 0.3f) + 0.7f;
                canvasGroup.alpha = alpha;
            }
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// 接听通话
    /// </summary>
    void AcceptCall()
    {
        if (!isIncomingCall)
        {
            Debug.LogWarning("[BugApp] 没有来电");
            return;
        }

        Debug.Log("[BugApp] 接听通话");

        isIncomingCall = false;
        isCallActive = true;

        // 停止来电铃声
        StopSound();

        // 播放接通音效
        PlaySound(connectionSound, false);

        // 隐藏来电界面
        if (incomingCallPanel != null)
        {
            incomingCallPanel.SetActive(false);
        }

        // 显示通话界面
        if (videoCallPanel != null)
        {
            videoCallPanel.SetActive(true);
        }

        // 开始对话
        BeginConversation();
    }

    /// <summary>
    /// 尝试关闭应用
    /// </summary>
    void OnCloseAttempt()
    {
        if (isIncomingCall && !isCallActive)
        {
            // 来电中无法关闭
            if (uiManager != null)
            {
                uiManager.ShowError("无法挂断...通话无法中断...");
                uiManager.ShakeScreen(3f, 0.3f);
            }
            Debug.Log("[BugApp] 来电中无法关闭");
        }
        else
        {
            CloseBugApp();
        }
    }

    /// <summary>
    /// 关闭BugApp
    /// </summary>
    void CloseBugApp()
    {
        HideAllPanels();
        gameObject.SetActive(false);

        if (appManager != null)
        {
            appManager.currentActiveWindow = null;
        }

        Debug.Log("[BugApp] 应用已关闭");
    }

    // ==================== 对话系统 ====================

    void BeginConversation()
    {
        CancelAutoAdvance();
        StopVideoPlayback(false);

        var stage = activeBugCallStage ?? BugCallStage.Chapter1;
        BuildDialogueLookupForStage(stage);

        if (dialogueLookup.Count == 0)
        {
            Debug.LogWarning("[BugApp] 没有配置Bug对话节点");
            return;
        }

        string entryNodeId = GetStartingNodeIdForStage(stage);
        GotoNode(entryNodeId);
    }

    void GotoNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            Debug.LogWarning("[BugApp] 目标节点ID为空");
            EndCall();
            return;
        }

        if (!dialogueLookup.TryGetValue(nodeId, out var node))
        {
            Debug.LogWarning($"[BugApp] 找不到对话节点: {nodeId}");
            EndCall();
            return;
        }

        CancelAutoAdvance();
        CancelPlayerContinueAwait();

        if (currentNode != null)
        {
            OnNodeEnded?.Invoke(currentNode);
        }

        currentNode = node;
        OnNodeStarted?.Invoke(currentNode);
        PresentNode(node);
    }

    void PresentNode(BugDialogueNode node)
    {
        UpdateDialogueBubble(node);
        ApplyNodeEffects(node);

        switch (node.nodeType)
        {
            case BugDialogueNodeType.BugVideo:
                ShowBugSegment(node);
                break;
            case BugDialogueNodeType.PlayerDialogue:
                ShowPlayerSegment(node);
                break;
        }
    }

    void SetKeyframeSprite(Sprite sprite)
    {
        bool hasSprite = sprite != null;

        if (keyframeImage != null)
        {
            keyframeImage.sprite = sprite;
            keyframeImage.gameObject.SetActive(hasSprite);
        }
        else if (videoBackgroundImage != null)
        {
            videoBackgroundImage.sprite = sprite;
            videoBackgroundImage.enabled = hasSprite;
        }

        if (hasSprite && videoOutput != null)
        {
            videoOutput.gameObject.SetActive(false);
            videoOutput.texture = null;
        }
    }

    void AwaitPlayerContinue(string nextNodeId)
    {
        CancelAutoAdvance();

        if (playerContinueButton == null)
        {
            Debug.LogWarning("[BugApp] playerContinueButton 未配置，玩家节点将自动跳转");
            if (!string.IsNullOrEmpty(nextNodeId))
            {
                GotoNode(nextNodeId);
            }
            else
            {
                EndCall();
            }
            return;
        }

        waitingForPlayerContinue = true;
        pendingNextNodeId = nextNodeId;

        playerContinueButton.gameObject.SetActive(true);
        playerContinueButton.interactable = true;

        if (videoOutput != null)
        {
            videoOutput.gameObject.SetActive(false);
        }
    }

    void CancelPlayerContinueAwait()
    {
        waitingForPlayerContinue = false;
        pendingNextNodeId = null;

        if (playerContinueButton != null)
        {
            playerContinueButton.interactable = false;
            playerContinueButton.gameObject.SetActive(false);
        }
    }

    void OnPlayerContinueClicked()
    {
        if (!waitingForPlayerContinue)
        {
            return;
        }

        string targetNodeId = pendingNextNodeId;
        CancelPlayerContinueAwait();

        if (!string.IsNullOrEmpty(targetNodeId))
        {
            GotoNode(targetNodeId);
        }
        else
        {
            EndCall();
        }
    }

    void UpdateDialogueBubble(BugDialogueNode node)
    {
        if (dialogueBubbleImage == null)
        {
            return;
        }

        Sprite sprite = node != null ? node.dialogueBubbleSprite : null;
        dialogueBubbleImage.sprite = sprite;
        dialogueBubbleImage.gameObject.SetActive(sprite != null);
    }

    void ShowBugSegment(BugDialogueNode node)
    {
        if (node.nodeType != BugDialogueNodeType.BugVideo)
        {
            Debug.LogWarning($"[BugApp] 节点 {node?.nodeId} 标记为 {node?.nodeType}，但正尝试以 Bug 视频播放，已改用玩家关键帧逻辑。");
            ShowPlayerSegment(node);
            return;
        }

        StopVideoPlayback(false);

        if (videoOutput != null)
        {
            videoOutput.gameObject.SetActive(false);
        }

        bool hasVideo = videoPlayer != null && node.videoClip != null;

        if (hasVideo)
        {
            ApplyFallbackFrame(node);
            StartBugVideo(node);
        }
        else
        {
            ApplyFallbackFrame(node);
            float delay = Mathf.Max(node.autoAdvanceDelay, 0f);
            ScheduleAutoAdvance(delay, node.nextNodeId);
        }
    }

    void StartBugVideo(BugDialogueNode node)
    {
        if (videoPlayer == null || node.videoClip == null)
        {
            ApplyFallbackFrame(node);
            float delay = Mathf.Max(node.autoAdvanceDelay, 0f);
            ScheduleAutoAdvance(delay, node.nextNodeId);
            return;
        }

        if (videoPlaybackCoroutine != null)
        {
            StopCoroutine(videoPlaybackCoroutine);
            videoPlaybackCoroutine = null;
        }

        CancelPlayerContinueAwait();

        videoPlaybackCoroutine = StartCoroutine(PlayVideoSequence(node));
    }

    IEnumerator PlayVideoSequence(BugDialogueNode node)
    {
        isVideoPreparing = true;

        videoPlayer.loopPointReached -= OnVideoCompleted;
        videoPlayer.Stop();
        videoPlayer.clip = node.videoClip;
        videoPlayer.isLooping = false;

        if (audioSource != null && videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            if (currentNode != node)
            {
                isVideoPreparing = false;
                videoPlaybackCoroutine = null;
                yield break;
            }
            yield return null;
        }

        if (currentNode != node)
        {
            isVideoPreparing = false;
            videoPlaybackCoroutine = null;
            yield break;
        }

        if (videoOutput != null)
        {
            videoOutput.texture = videoPlayer.texture;
            videoOutput.gameObject.SetActive(true);
        }

        // 视频开始播放后隐藏静帧
        SetKeyframeSprite(null);

        if (keyframeImage == null && videoBackgroundImage != null)
        {
            videoBackgroundImage.enabled = false;
        }

        videoPlayer.loopPointReached += OnVideoCompleted;
        videoPlayer.Play();
        isVideoPreparing = false;
        videoPlaybackCoroutine = null;
    }

    void OnVideoCompleted(VideoPlayer player)
    {
        player.loopPointReached -= OnVideoCompleted;
        player.Pause();

        if (currentNode == null || currentNode.nodeType != BugDialogueNodeType.BugVideo)
        {
            return;
        }

        float delay = Mathf.Max(currentNode.autoAdvanceDelay, 0f);
        ScheduleAutoAdvance(delay, currentNode.nextNodeId);
    }

    void ShowPlayerSegment(BugDialogueNode node)
    {
        if (node.nodeType != BugDialogueNodeType.PlayerDialogue)
        {
            Debug.LogWarning($"[BugApp] 节点 {node?.nodeId} 标记为 {node?.nodeType}，但正尝试以玩家关键帧播放，操作已忽略。");
            return;
        }

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }

        ApplyFallbackFrame(node);

        if (!string.IsNullOrEmpty(node.nextNodeId))
        {
            AwaitPlayerContinue(node.nextNodeId);
        }
        else
        {
            AwaitPlayerContinue(null);
        }
    }

    IEnumerator AutoAdvanceAfterDelay(float delay, string nextNodeId)
    {
        yield return new WaitForSeconds(delay);
        GotoNode(nextNodeId);
    }

    void ScheduleAutoAdvance(float delay, string nextNodeId)
    {
        CancelAutoAdvance();

        if (string.IsNullOrEmpty(nextNodeId))
        {
            autoAdvanceCoroutine = StartCoroutine(EndCallAfterDelay(delay));
        }
        else
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(delay, nextNodeId));
        }
    }

    IEnumerator EndCallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndCall();
    }

    void CancelAutoAdvance()
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
    }

    void StopVideoPlayback(bool clearDisplay)
    {
        if (videoPlaybackCoroutine != null)
        {
            StopCoroutine(videoPlaybackCoroutine);
            videoPlaybackCoroutine = null;
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoCompleted;

            if (videoPlayer.isPlaying || isVideoPreparing)
            {
                videoPlayer.Stop();
            }

            if (clearDisplay)
            {
                videoPlayer.clip = null;
            }
        }

        isVideoPreparing = false;

        if (clearDisplay && videoOutput != null)
        {
            videoOutput.texture = null;
        }

        if (clearDisplay)
        {
            ApplyFallbackFrame(null);
            UpdateDialogueBubble(null);
        }
    }

    void ApplyFallbackFrame(BugDialogueNode node)
    {
        Sprite sprite = fallbackPausedSprite;

        if (node != null)
        {
            if (node.nodeType == BugDialogueNodeType.PlayerDialogue)
            {
                if (node.playerKeyframeSprite != null)
                {
                    sprite = node.playerKeyframeSprite;
                }
                else if (node.fallbackFrame != null)
                {
                    sprite = node.fallbackFrame;
                }
                else
                {
                    sprite = GetBugFrameByIndex(node.fallbackFrameIndex) ?? sprite;
                }
            }
            else
            {
                if (node.fallbackFrame != null)
                {
                    sprite = node.fallbackFrame;
                }
                else
                {
                    sprite = GetBugFrameByIndex(node.fallbackFrameIndex) ?? sprite;
                }
            }
        }

        SetKeyframeSprite(sprite);

    }

    Sprite GetBugFrameByIndex(int index)
    {
        if (bugVideoFrames == null || index < 0 || index >= bugVideoFrames.Length)
        {
            return null;
        }

        return bugVideoFrames[index];
    }

    void ApplyNodeEffects(BugDialogueNode node)
    {
        if (node.triggerGlitch)
        {
            PlayGlitchEffect();
        }

        if (node.triggerShake && uiManager != null)
        {
            uiManager.ShakeScreen(node.shakeIntensity, node.shakeDuration);
        }

    }

    // ==================== 通话控制 ====================

    /// <summary>
    /// 挂断通话
    /// </summary>
    void HangupCall()
    {
        if (!isCallActive)
        {
            Debug.LogWarning("[BugApp] 当前没有通话");
            return;
        }

        Debug.Log("[BugApp] 挂断通话");
        EndCall();
    }

    /// <summary>
    /// 结束通话
    /// </summary>
    void EndCall()
    {
        bugCallTriggered = false;
        bugCallScheduled = false;
        pendingBugCallStage = null;
        activeBugCallStage = null;

        isCallActive = false;
        isIncomingCall = false;

        if (currentNode != null)
        {
            OnNodeEnded?.Invoke(currentNode);
            currentNode = null;
        }

        CancelAutoAdvance();
        StopVideoPlayback(true);

        // 停止所有协程
        StopAllCoroutines();

        // 停止音效
        StopSound();

        // 隐藏所有面板
        HideAllPanels();

        if (!AllBugCallStagesCompleted())
        {
            EnsureBugTriggerMonitoring();
        }

        // 延迟触发章节过渡
        Invoke(nameof(TriggerChapterTransition), 1f);

        Debug.Log("[BugApp] 通话结束，准备章节过渡");
    }

    /// <summary>
    /// 麦克风按钮点击 (装饰用)
    /// </summary>
    void OnMicButtonClicked()
    {
        Debug.Log("[BugApp] 麦克风按钮点击 (无实际功能)");
    }

    /// <summary>
    /// 听筒按钮点击 (装饰用)
    /// </summary>
    void OnSpeakerButtonClicked()
    {
        Debug.Log("[BugApp] 听筒按钮点击 (无实际功能)");
    }

    // ==================== 特效系统 ====================

    /// <summary>
    /// 播放Glitch故障效果
    /// </summary>
    void PlayGlitchEffect()
    {
        // 播放随机Glitch音效
        if (glitchSounds != null && glitchSounds.Length > 0)
        {
            AudioClip randomGlitch = glitchSounds[UnityEngine.Random.Range(0, glitchSounds.Length)];
            PlaySound(randomGlitch, false);
        }

        // TODO: 可以添加视觉Glitch效果
        Debug.Log("[BugApp] 播放Glitch效果");
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    void PlaySound(AudioClip clip, bool loop)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // ==================== 进度保存 ====================

    /// <summary>
    /// 保存章节进度
    /// </summary>
    void SaveChapterProgress()
    {
        bool progressHandled = false;

        if (storyManager == null)
        {
            CacheManagers();
        }

        if (storyManager != null)
        {
            storyManager.RequestChapterProgress(2, true, "BugApp_SaveChapter");
            progressHandled = true;
        }
        else
        {
            var saveData = dataManager?.GetSaveData();
            if (saveData != null)
            {
                saveData.currentChapter = 2;
                saveData.currentDay++;
                dataManager.SaveGameData();
                progressHandled = true;
            }
        }

        if (progressHandled)
        {
            Debug.Log("[BugApp] 第二章已开启");
        }
    }

    /// <summary>
    /// 触发章节过渡
    /// </summary>
    void TriggerChapterTransition()
    {
        // 查找或创建ChapterManager
        ChapterManager chapterManager = FindObjectOfType<ChapterManager>();
        if (chapterManager == null)
        {
            GameObject chapterManagerObj = new GameObject("ChapterManager");
            chapterManager = chapterManagerObj.AddComponent<ChapterManager>();
        }

        // 触发章节过渡
        chapterManager.TransitionToChapter2();

        // 关闭BugApp
        CloseBugApp();
    }

    // ==================== 测试方法 ====================

    /// <summary>
    /// 手动触发Bug通话 (测试用)
    /// </summary>
    public void ManualTriggerBugCall()
    {
        if (!isIncomingCall && !isCallActive)
        {
            TriggerBugCall();
        }
    }
}

// ==================== 对话节点数据结构 ====================

public enum BugDialogueNodeType
{
    BugVideo,
    PlayerDialogue
}

[System.Serializable]
public class BugDialogueNode
{
    [Header("基础信息")]
    public string nodeId = Guid.NewGuid().ToString();
    [Tooltip("BugVideo: 播放 MP4 视频；PlayerDialogue: 使用关键帧静帧。")]
    public BugDialogueNodeType nodeType = BugDialogueNodeType.BugVideo;
    public string speakerName = "？？？";

    [Header("对白显示")]
    public Sprite dialogueBubbleSprite;

    [Header("视频/画面")]
    public VideoClip videoClip;
    [Tooltip("玩家回合: 若未设置玩家关键帧则使用此图; Bug回合: 作为视频缺失的静帧替代。")]
    public Sprite fallbackFrame;
    [Header("玩家关键帧")]
    [Tooltip("玩家对白使用的关键帧图片，如果为空则退回到备用静帧/序列帧。")]
    public Sprite playerKeyframeSprite;
    public int fallbackFrameIndex = -1;
    public string nextNodeId = "";
    public float autoAdvanceDelay = 0.5f;

    [Header("特效")]
    public bool triggerGlitch = false;
    public bool triggerShake = false;
    public float shakeIntensity = 5f;
    public float shakeDuration = 0.5f;
}
