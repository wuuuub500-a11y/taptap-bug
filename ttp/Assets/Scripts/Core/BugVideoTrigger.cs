using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 全局Bug视频触发控制器
/// 玩家完成关键线索后返回桌面，将自动触发Bug视频通话
/// </summary>
public class BugVideoTrigger : MonoBehaviour
{
    [SerializeField] private string bugAppId = "bugapp";
    [SerializeField] private float checkInterval = 1f;
    [SerializeField] private float idleDelay = 0.5f;

    private enum TriggerStage
    {
        Chapter1 = 1,
        Chapter2 = 2
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

    private const string Chapter1FlagKey = "BugApp_Chapter1_CallTriggered";
    private const string Chapter2FlagKey = "BugApp_Chapter2_CallTriggered";

    private DataManager dataManager;
    private AppManager appManager;
    private WindowManager windowManager;
    private BugApp bugApp;

    private bool chapter1Triggered;
    private bool chapter2Triggered;

    private bool waitingForIdle = false;
    private TriggerStage? pendingStage = null;

    void Start()
    {
        CacheManagers();
        bugApp = FindBugAppInstance();

        LoadStageFlags();

        if (ChapterCallsCompleted())
        {
            StartCoroutine(EnsureBugAppUnlockedDelayed());
            return;
        }

        InvokeRepeating(nameof(EvaluateConditions), checkInterval, checkInterval);
    }

    void CacheManagers()
    {
        if (GameManager.Instance != null)
        {
            if (dataManager == null)
            {
                dataManager = GameManager.Instance.dataManager;
            }

            if (appManager == null)
            {
                appManager = GameManager.Instance.appManager;
            }
        }

        if (windowManager == null)
        {
            windowManager = FindObjectOfType<WindowManager>();
        }
    }

    void EvaluateConditions()
    {
        CacheManagers();

        if (bugApp == null)
        {
            bugApp = FindBugAppInstance();
        }

        var saveData = dataManager?.GetSaveData();
        if (saveData == null)
        {
            return;
        }

        if (!chapter1Triggered && EvaluateChapter1Requirements(saveData))
        {
            RequestStage(TriggerStage.Chapter1);
            return;
        }

        if (!chapter2Triggered && EvaluateChapter2Requirements(saveData))
        {
            RequestStage(TriggerStage.Chapter2);
        }
    }

    void RequestStage(TriggerStage stage)
    {
        if (waitingForIdle)
        {
            return;
        }

        pendingStage = stage;
        StartCoroutine(WaitForDesktopThenTrigger());
        waitingForIdle = true;
    }

    IEnumerator WaitForDesktopThenTrigger()
    {
        while (!IsDesktopIdle())
        {
            yield return null;
        }

        yield return new WaitForSeconds(idleDelay);

        waitingForIdle = false;

        if (!pendingStage.HasValue)
        {
            yield break;
        }

        TriggerStage stage = pendingStage.Value;
        pendingStage = null;

        TriggerBugCall(stage);
    }

    bool IsDesktopIdle()
    {
        CacheManagers();

        bool windowIdle = windowManager == null || !windowManager.HasActiveWindow();
        bool appIdle = appManager == null || !appManager.HasActiveWindow();

        return windowIdle && appIdle;
    }

    void TriggerBugCall(TriggerStage stage)
    {
        CacheManagers();

        if (stage == TriggerStage.Chapter1)
        {
            chapter1Triggered = true;
            dataManager?.SetBool(Chapter1FlagKey, true);
        }
        else if (stage == TriggerStage.Chapter2)
        {
            chapter2Triggered = true;
            dataManager?.SetBool(Chapter2FlagKey, true);
        }

        if (bugApp == null)
        {
            bugApp = FindBugAppInstance();
        }

        BugApp.BugCallStage bugStage = stage == TriggerStage.Chapter1
            ? BugApp.BugCallStage.Chapter1
            : BugApp.BugCallStage.Chapter2;

        if (bugApp != null)
        {
            bugApp.RequestBugCallForStage(bugStage);
        }
        else
        {
            UnlockBugApp();
            OpenBugApp();
            StartCoroutine(RequestBugCallNextFrame(bugStage));
        }

        if (ChapterCallsCompleted())
        {
            CancelInvoke(nameof(EvaluateConditions));
        }

        Debug.Log($"Bug视频通话触发！（通过BugVideoTrigger，阶段 {stage}）");
    }

    void UnlockBugApp()
    {
        CacheManagers();

        if (windowManager != null)
        {
            windowManager.UnlockApp(bugAppId);
        }
        else if (appManager != null)
        {
            appManager.UnlockApp(bugAppId);
        }

        if (appManager != null)
        {
            var appData = appManager.GetAppData(bugAppId);
            if (appData != null)
            {
                appData.isUnlocked = true;
            }
        }
    }

    void OpenBugApp()
    {
        CacheManagers();

        if (windowManager != null)
        {
            windowManager.OpenApp(bugAppId);
        }
        else if (appManager != null)
        {
            appManager.OpenApp(bugAppId);
        }
    }

    IEnumerator RequestBugCallNextFrame(BugApp.BugCallStage stage)
    {
        yield return null;

        bugApp = FindBugAppInstance();
        if (bugApp != null)
        {
            bugApp.RequestBugCallForStage(stage);
        }
        else
        {
            Debug.LogWarning("未找到BugApp，无法触发Bug通话");
        }
    }

    IEnumerator EnsureBugAppUnlockedDelayed()
    {
        yield return null;
        UnlockBugApp();
    }

    BugApp FindBugAppInstance()
    {
        return FindObjectOfType<BugApp>(true);
    }

    void LoadStageFlags()
    {
        if (dataManager == null)
        {
            chapter1Triggered = false;
            chapter2Triggered = false;
            return;
        }

        chapter1Triggered = dataManager.GetBool(Chapter1FlagKey, false);
        chapter2Triggered = dataManager.GetBool(Chapter2FlagKey, false);

        if (!chapter1Triggered && dataManager.IsVideoCallTriggered())
        {
            chapter1Triggered = true;
            dataManager.SetBool(Chapter1FlagKey, true);
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
        if (dataManager == null || saveData.currentChapter < 2)
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

    bool ChapterCallsCompleted()
    {
        return chapter1Triggered && chapter2Triggered;
    }
}
