using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Bug视频通话应用 - 死者打来的视频通话
/// </summary>
public class VideoCallApp : MonoBehaviour
{
    [Header("UI组件")]
    public Button closeButton;
    public Button acceptButton;
    public GameObject incomingCallPanel;
    public GameObject videoCallPanel;
    public GameObject videoCallBackground;

    [Header("来电显示")]
    public TextMeshProUGUI callerNameText;
    public TextMeshProUGUI callingStatusText;
    public Image callerImage;

    [Header("视频通话界面")]
    public TextMeshProUGUI callDurationText;
    public Button hangUpButton;
    public Image videoBackgroundImage;
    public TextMeshProUGUI dialogueText;
    public GameObject[] dialogueOptions; // 对话选项按钮

    [Header("Bug特效")]
    public Animator glitchEffect;
    public AudioClip ringingSound;
    public AudioClip connectionSound;
    public AudioClip[] glitchSounds;

    [Header("通话数据")]
    private bool isIncomingCall = false;
    private bool isCallActive = false;
    private float callStartTime;
    private int currentDialogueIndex = 0;
    private List<DialogueData> dialogues = new List<DialogueData>();

    // Bug触发条件检测
    private bool allTasksCompleted = false;
    private bool allAppsViewed = false;

    void Start()
    {
        InitializeVideoCall();
    }

    void InitializeVideoCall()
    {
        SetupUIEvents();
        SetupBugConditions();
        HideAllPanels();
    }

    void SetupUIEvents()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseVideoCall);
        }

        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(AcceptCall);
        }

        if (hangUpButton != null)
        {
            hangUpButton.onClick.AddListener(HangUpCall);
        }

        // 为对话选项按钮添加事件
        if (dialogueOptions != null)
        {
            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                int index = i;
                if (dialogueOptions[i] != null)
                {
                    Button optionButton = dialogueOptions[i].GetComponent<Button>();
                    if (optionButton != null)
                    {
                        optionButton.onClick.AddListener(() => OnDialogueOptionSelected(index));
                    }
                }
            }
        }
    }

    void SetupBugConditions()
    {
        // 在游戏初始化时检查是否满足所有触发条件
        CheckBugConditions();
    }

    void HideAllPanels()
    {
        if (incomingCallPanel != null)
        {
            incomingCallPanel.SetActive(false);
        }

        if (videoCallPanel != null)
        {
            videoCallPanel.SetActive(false);
        }
    }

    void CloseVideoCall()
    {
        // 如果有来电但不是接听状态，显示无权限
        if (isIncomingCall && !isCallActive)
        {
            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.ShowError("无权限接听此通话");
                GameManager.Instance.uiManager.ShakeScreen(2f, 0.2f);
            }
            return;
        }

        HideAllPanels();
        gameObject.SetActive(false);

        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    /// <summary>
    /// 检查Bug触发条件
    /// </summary>
    public void CheckBugConditions()
    {
        var saveData = GameManager.Instance.dataManager.GetSaveData();
        if (saveData == null) return;

        // 检查是否完成所有四个主要交互
        bool chatUnlocked = GameManager.Instance.dataManager.IsPasswordSolved("chat");
        bool questionnaireCompleted = GameManager.Instance.dataManager.IsQuestionnaireCompleted("guanqin");
        bool photoUnlocked = saveData.photosUnlocked != null && saveData.photosUnlocked.Contains("life_mosaic");
        bool browserUsed = saveData.browserHistory != null && saveData.browserHistory.Count > 0;

        allTasksCompleted = chatUnlocked && questionnaireCompleted && photoUnlocked && browserUsed;

        // 检查是否查看了所有应用
        allAppsViewed = CheckAllAppsViewed(saveData);

        Debug.Log($"Bug条件检查 - 任务完成: {allTasksCompleted}, 应用查看: {allAppsViewed}");

        // 如果满足条件，自动触发视频通话
        if (allTasksCompleted && allAppsViewed)
        {
            StartCoroutine(DelayedVideoCall());
        }
    }

    bool CheckAllAppsViewed(GameSaveData saveData)
    {
        // 检查博客阅读
        bool blogViewed = saveData.blogPostsRead != null && saveData.blogPostsRead.Count >= 5;

        // 检查聊天记录
        bool chatViewed = saveData.chatHistory != null && saveData.chatHistory.Count > 0;

        // 检查浏览器使用
        bool browserViewed = saveData.browserHistory != null && saveData.browserHistory.Count > 0;

        // 检查相册查看
        bool albumViewed = saveData.photosUnlocked != null && saveData.photosUnlocked.Count > 0;

        return blogViewed && chatViewed && browserViewed && albumViewed;
    }

    IEnumerator DelayedVideoCall()
    {
        // 等待一段时间后触发视频通话
        yield return new WaitForSeconds(2f);

        Debug.Log("Bug视频通话触发！");
        TriggerIncomingCall();
    }

    /// <summary>
    /// 触发来电
    /// </summary>
    public void TriggerIncomingCall()
    {
        if (isIncomingCall || isCallActive) return;

        isIncomingCall = true;

        // 显示来电界面
        if (incomingCallPanel != null)
        {
            incomingCallPanel.SetActive(true);
        }

        // 设置来电显示
        if (callerNameText != null)
        {
            callerNameText.text = "未知号码";
        }

        if (callingStatusText != null)
        {
            callingStatusText.text = "来电中...";
        }

        // 播放来电铃声
        if (ringingSound != null && GameManager.Instance != null)
        {
            AudioSource.PlayClipAtPoint(ringingSound, Camera.main.transform.position);
        }

        // 开始闪烁效果
        StartCoroutine(FlashingEffect());

        Debug.Log("收到Bug来电");
    }

    void AcceptCall()
    {
        if (!isIncomingCall) return;

        isIncomingCall = false;
        isCallActive = true;
        callStartTime = Time.time;

        // 隐藏来电界面
        if (incomingCallPanel != null)
        {
            incomingCallPanel.SetActive(false);
        }

        // 显示视频通话界面
        if (videoCallPanel != null)
        {
            videoCallPanel.SetActive(true);
        }

        // 设置视频通话界面
        if (callerNameText != null)
        {
            callerNameText.text = "???";
        }

        // 播放接听音效
        if (connectionSound != null)
        {
            AudioSource.PlayClipAtPoint(connectionSound, Camera.main.transform.position);
        }

        // 开始视频通话对话
        StartCoroutine(StartVideoCallDialogue());

        Debug.Log("接听了Bug视频通话");
    }

    void HangUpCall()
    {
        if (!isCallActive) return;

        isCallActive = false;

        // 关闭视频通话界面
        if (videoCallPanel != null)
        {
            videoCallPanel.SetActive(false);
        }

        // 停止所有效果
        StopAllCoroutines();

        // 保存游戏进度，进入第二章
        SaveChapterProgress();

        CloseVideoCall();

        Debug.Log("挂断了Bug视频通话");
    }

    IEnumerator StartVideoCallDialogue()
    {
        // 加载对话数据
        LoadDialogues();

        // 显示背景
        if (videoCallBackground != null)
        {
            videoCallBackground.SetActive(true);
        }

        // 开始对话序列
        for (int i = 0; i < dialogues.Count; i++)
        {
            currentDialogueIndex = i;
            var dialogue = dialogues[i];

            // 显示对话文本
            if (dialogueText != null)
            {
                dialogueText.text = dialogue.text;
            }

            // 强烈震动效果（突脸时）
            if (dialogue.triggerShake)
            {
                if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                {
                    GameManager.Instance.uiManager.ShakeScreen(8f, 1f);
                }
            }

            // 显示对话选项
            ShowDialogueOptions(dialogue.options);

            // 播放Glitch效果
            if (dialogue.triggerGlitch)
            {
                PlayGlitchEffect();
            }

            // 等待玩家选择
            yield return new WaitUntil(() => dialogue.hasBeenAnswered);
        }

        // 对话结束，自动挂断
        yield return new WaitForSeconds(2f);
        HangUpCall();
    }

    void LoadDialogues()
    {
        dialogues = new List<DialogueData>
        {
            new DialogueData
            {
                text = "你...还记得我吗？",
                triggerShake = true,
                triggerGlitch = true,
                options = new string[] { "你是谁？", "我认识你吗？" },
                hasBeenAnswered = false
            },
            new DialogueData
            {
                text = "我们以前...见过面的...在那个雨天...",
                triggerShake = false,
                triggerGlitch = true,
                options = new string[] { "我不记得了", "告诉我你是谁" },
                hasBeenAnswered = false
            },
            new DialogueData
            {
                text = "没关系...很快...你就会想起一切的...",
                triggerShake = true,
                triggerGlitch = true,
                options = new string[] { "你想做什么？", "离开这里！" },
                hasBeenAnswered = false
            }
        };
    }

    void ShowDialogueOptions(string[] options)
    {
        if (dialogueOptions == null) return;

        // 先隐藏所有选项
        foreach (var option in dialogueOptions)
        {
            if (option != null)
            {
                option.SetActive(false);
            }
        }

        // 显示当前对话的选项
        for (int i = 0; i < options.Length && i < dialogueOptions.Length; i++)
        {
            if (dialogueOptions[i] != null)
            {
                dialogueOptions[i].SetActive(true);
                TextMeshProUGUI optionText = dialogueOptions[i].GetComponentInChildren<TextMeshProUGUI>();
                if (optionText != null)
                {
                    optionText.text = options[i];
                }
            }
        }
    }

    void OnDialogueOptionSelected(int optionIndex)
    {
        if (currentDialogueIndex >= 0 && currentDialogueIndex < dialogues.Count)
        {
            dialogues[currentDialogueIndex].hasBeenAnswered = true;

            // 隐藏选项
            foreach (var option in dialogueOptions)
            {
                if (option != null)
                {
                    option.SetActive(false);
                }
            }

            // 可以根据选项触发不同的反应
            Debug.Log($"选择了选项 {optionIndex}");
        }
    }

    void PlayGlitchEffect()
    {
        // 播放Glitch特效
        if (glitchEffect != null)
        {
            glitchEffect.SetTrigger("Glitch");
        }

        // 播放Glitch音效
        if (glitchSounds != null && glitchSounds.Length > 0)
        {
            AudioClip randomGlitch = glitchSounds[Random.Range(0, glitchSounds.Length)];
            if (randomGlitch != null)
            {
                AudioSource.PlayClipAtPoint(randomGlitch, Camera.main.transform.position);
            }
        }
    }

    IEnumerator FlashingEffect()
    {
        while (isIncomingCall)
        {
            // 来电闪烁效果
            if (incomingCallPanel != null)
            {
                CanvasGroup canvasGroup = incomingCallPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = incomingCallPanel.AddComponent<CanvasGroup>();
                }

                // 闪烁动画
                float alpha = Mathf.PingPong(Time.time * 2f, 0.5f) + 0.5f;
                canvasGroup.alpha = alpha;
            }

            yield return null;
        }
    }

    void Update()
    {
        // 更新通话时长
        if (isCallActive)
        {
            float duration = Time.time - callStartTime;
            if (callDurationText != null)
            {
                int minutes = Mathf.FloorToInt(duration / 60f);
                int seconds = Mathf.FloorToInt(duration % 60f);
                callDurationText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    void SaveChapterProgress()
    {
        bool progressHandled = false;

        var storyMgr = GameManager.Instance != null ? GameManager.Instance.storyManager : null;
        if (storyMgr != null)
        {
            storyMgr.RequestChapterProgress(2, true, "VideoCallApp_SaveChapter");
            progressHandled = true;
        }
        else
        {
            var saveData = GameManager.Instance.dataManager.GetSaveData();
            if (saveData != null)
            {
                saveData.currentChapter = 2;
                saveData.currentDay++;
                GameManager.Instance.dataManager.SaveGameData();
                progressHandled = true;
            }
        }

        if (progressHandled && GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowSuccess("恭喜完成第一章！现在进入第二章...");
        }

        if (progressHandled)
        {
            Debug.Log("第二章已开启");
        }
    }

    /// <summary>
    /// 手动触发视频通话（用于测试）
    /// </summary>
    public void ManualTriggerVideoCall()
    {
        if (!isIncomingCall && !isCallActive)
        {
            TriggerIncomingCall();
        }
    }
}

[System.Serializable]
public class DialogueData
{
    public string text;
    public bool triggerShake;
    public bool triggerGlitch;
    public string[] options;
    public bool hasBeenAnswered = false;
}