using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 章节管理器 - 管理第一章到第二章的过渡
/// 负责播放过场动画和切换章节状态
/// </summary>
public class ChapterManager : MonoBehaviour
{
    [Header("=== 过场视频配置 ===")]
    public VideoPlayer transitionVideoPlayer;
    public RawImage videoDisplayImage;
    public GameObject transitionPanel;

    [Header("=== 第一章开场视频 ===")]
    public bool playChapter1IntroOnStart = true;
    public VideoClip chapter1IntroClip;
    [SerializeField] private string chapter1IntroFlagKey = "chapter1_intro_played";
    
    [Header("=== UI配置 ===")]
    public Canvas mainCanvas;
    public Canvas transitionCanvas;
    
    [Header("=== 场景管理 ===")]
    public bool useSceneTransition = false;
    public string chapter2SceneName = "Chapter2";
    
    // 状态管理
    private bool isTransitioning = false;
    private bool isChapter2 = false;
    private bool isChapter1IntroPlaying = false;
    
    // 事件通知
    public static event System.Action OnChapter2Started;
    
    void Start()
    {
        InitializeChapterManager();
    }
    
    /// <summary>
    /// 初始化章节管理器
    /// </summary>
    void InitializeChapterManager()
    {
        // 检查当前章节状态
        var saveData = GameManager.Instance?.dataManager?.GetSaveData();
        if (saveData != null && saveData.currentChapter >= 2)
        {
            isChapter2 = true;
            EnableChapter2Features();
            Debug.Log("[ChapterManager] 已在第二章");
        }
        
        // 初始化过场UI
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);

            if (videoDisplayImage == null)
            {
                // 在过场面板中自动查找RawImage以避免漏配引用
                videoDisplayImage = transitionPanel.GetComponentInChildren<RawImage>(true);
                if (videoDisplayImage == null)
                {
                    Debug.LogWarning("[ChapterManager] TransitionPanel下未找到RawImage，过场视频将无法显示");
                }
            }
        }
        
        // 初始化VideoPlayer
        if (transitionVideoPlayer != null)
        {
            transitionVideoPlayer.loopPointReached += OnTransitionVideoEnded;
            transitionVideoPlayer.renderMode = VideoRenderMode.APIOnly; // 使用API模式，便于赋值给RawImage
            transitionVideoPlayer.targetTexture = null;
            transitionVideoPlayer.playOnAwake = false;
        }

        Debug.Log("[ChapterManager] 初始化完成");

        if (ShouldPlayChapter1Intro())
        {
            StartCoroutine(PlayChapter1IntroRoutine());
        }
    }
    
    /// <summary>
    /// 触发章节过渡
    /// </summary>
    public void TransitionToChapter2()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[ChapterManager] 章节过渡正在进行中");
            return;
        }
        
        if (isChapter2)
        {
            Debug.LogWarning("[ChapterManager] 已在第二章");
            return;
        }
        
        Debug.Log("[ChapterManager] 开始章���过渡：第一章 → 第二章");
        StartCoroutine(ChapterTransitionRoutine());
    }
    
    /// <summary>
    /// 章节过渡协程
    /// </summary>
    IEnumerator ChapterTransitionRoutine()
    {
        isTransitioning = true;
        
        // 1. 显示过场面板
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
        }
        else
        {
            // 如果没有配置过场面板，创建一个临时的黑色覆盖
            transitionPanel = CreateTransitionPanel();
        }
        
        // 2. 播放过场动画
        if (transitionVideoPlayer != null)
        {
            yield return StartCoroutine(PlayTransitionVideo());
        }
        else
        {
            // 如果没有视频，等待2秒
            yield return new WaitForSeconds(2f);
        }
        
        // 3. 切换到第二章状态
        SwitchToChapter2();
        
        // 4. 隐藏过场面板
        yield return StartCoroutine(HideTransitionPanel());
        
        isTransitioning = false;
        Debug.Log("[ChapterManager] 章节过渡完成");
    }
    
    /// <summary>
    /// 播放过场视频
    /// </summary>
    IEnumerator PlayTransitionVideo()
    {
        if (transitionVideoPlayer == null)
        {
            yield break;
        }

        VideoClip currentClip = transitionVideoPlayer.clip;
        yield return StartCoroutine(PlayVideoClipInternal(currentClip, false));

        Debug.Log("[ChapterManager] 过场视频播放完成");
    }
    
    /// <summary>
    /// 过场视频结束回调
    /// </summary>
    void OnTransitionVideoEnded(VideoPlayer player)
    {
        Debug.Log("[ChapterManager] 过场视频自然结束");
    }

    IEnumerator PlayChapter1IntroRoutine()
    {
        if (transitionVideoPlayer == null || chapter1IntroClip == null)
        {
            yield break;
        }

        isTransitioning = true;
        isChapter1IntroPlaying = true;

        if (transitionPanel == null)
        {
            transitionPanel = CreateTransitionPanel();
        }

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
        }

        yield return StartCoroutine(PlayVideoClipInternal(chapter1IntroClip, true));

        if (GameManager.Instance?.dataManager != null)
        {
            GameManager.Instance.dataManager.SetBool(chapter1IntroFlagKey, true);
        }

        yield return StartCoroutine(HideTransitionPanel());

        isTransitioning = false;
        isChapter1IntroPlaying = false;

        Debug.Log("[ChapterManager] 第一章开场视频播放完成");
    }
    
    /// <summary>
    /// 切换到第二章状态
    /// </summary>
    void SwitchToChapter2()
    {
        isChapter2 = true;
        
        // 更新存档数据
        var saveData = GameManager.Instance?.dataManager?.GetSaveData();
        if (saveData != null)
        {
            saveData.currentChapter = 2;
            saveData.currentDay = 2; // 进入第二天
            GameManager.Instance?.dataManager?.SaveGameData();
        }
        
        // 启用第二章功能
        EnableChapter2Features();
        
        // 通知其他脚本章节已切换
        OnChapter2Started?.Invoke();
        
        Debug.Log("[ChapterManager] 已切换到第二章");
    }
    
    /// <summary>
    /// 启用第二章功能
    /// </summary>
    void EnableChapter2Features()
    {
        // 这里可以通知各个应用启用第二章功能
        // 例如：博客的拖拽游戏、相册的隐私相册、浏览器的关键词等
        
        // 查找并启用ChatManager
        var chatManager = FindObjectOfType<ChatManager>();
        if (chatManager != null)
        {
            chatManager.EnableChapter2Features();
        }
        
        Debug.Log("[ChapterManager] 第二章功能已启用");
    }

    bool ShouldPlayChapter1Intro()
    {
        if (!playChapter1IntroOnStart || chapter1IntroClip == null)
        {
            return false;
        }

        if (isChapter2)
        {
            return false;
        }

        var dataManager = GameManager.Instance?.dataManager;
        if (dataManager == null)
        {
            return true;
        }

        return !dataManager.GetBool(chapter1IntroFlagKey, false);
    }
    
    /// <summary>
    /// 隐藏过场面版
    /// </summary>
    IEnumerator HideTransitionPanel()
    {
        if (transitionPanel != null)
        {
            // 添加淡出效果
            CanvasGroup canvasGroup = transitionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = transitionPanel.AddComponent<CanvasGroup>();
            }
            
            float duration = 1f;
            float timer = 0f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = 1f - (timer / duration);
                yield return null;
            }
            
            transitionPanel.SetActive(false);
            canvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// 创建临时过场面版
    /// </summary>
    GameObject CreateTransitionPanel()
    {
        GameObject panel = new GameObject("TransitionPanel");
        panel.transform.SetParent(mainCanvas.transform, false);
        
        // 设置为全屏覆盖
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // 添加黑色背景
        Image image = panel.AddComponent<Image>();
        image.color = Color.black;
        
        // 添加CanvasGroup用于淡入淡出
        panel.AddComponent<CanvasGroup>();
        
        // 设置层级
        panel.transform.SetAsLastSibling();
        
        return panel;
    }

    IEnumerator PlayVideoClipInternal(VideoClip clip, bool restorePreviousClip)
    {
        if (transitionVideoPlayer == null || clip == null)
        {
            yield break;
        }

        VideoClip previousClip = transitionVideoPlayer.clip;

        if (previousClip != clip)
        {
            transitionVideoPlayer.clip = clip;
        }

        transitionVideoPlayer.Stop();
        transitionVideoPlayer.Prepare();
        while (!transitionVideoPlayer.isPrepared)
        {
            yield return null;
        }

        if (videoDisplayImage != null)
        {
            videoDisplayImage.texture = transitionVideoPlayer.texture;
            videoDisplayImage.gameObject.SetActive(true);
        }

        transitionVideoPlayer.Play();

        while (transitionVideoPlayer.isPlaying)
        {
            yield return null;
        }

        if (videoDisplayImage != null)
        {
            videoDisplayImage.gameObject.SetActive(false);
        }

        if (restorePreviousClip && previousClip != clip)
        {
            transitionVideoPlayer.clip = previousClip;
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
    /// 强制切换到第二章（测试用）
    /// </summary>
    [ContextMenu("强制切换到第二章")]
    public void ForceSwitchToChapter2()
    {
        if (!isChapter2)
        {
            TransitionToChapter2();
        }
    }
    
    void OnDestroy()
    {
        // 清理VideoPlayer事件
        if (transitionVideoPlayer != null)
        {
            transitionVideoPlayer.loopPointReached -= OnTransitionVideoEnded;
        }
    }
}
