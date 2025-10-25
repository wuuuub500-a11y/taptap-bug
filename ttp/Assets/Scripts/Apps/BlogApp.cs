using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
#pragma warning disable CS0414

public class BlogApp : MonoBehaviour
{
    [Header("主要UI组件")]
    public Button closeButton;
    public Button diaryButton;
    public Button messageButton;
    public Button collectionButton;  // 草稿按钮
    public ScrollRect diaryScrollRect;
    public ScrollRect contentScrollRect;
    public Scrollbar scrollbar;
    public Button diaryScrollRectButton;  // DiaryScrollRect的点击按钮

    [Header("博客长图滚动")]
    public Image blogScrollImage;  // 博客长图Image组件
    public Button[] clickableAreas;  // 可点击区域按钮数组
    public Sprite[] imageDetails;  // 对应的大图Sprite数组

    [Header("图片查看器")]
    public GameObject imageViewerPanel;  // 大图查看器面板
    public Image imageViewerImage;  // 显示大图的Image
    public Button imageViewerCloseButton;  // 关闭大图的按钮

    [Header("弹窗面板")]
    public GameObject noAccessPanel;  // 无访问权限弹窗
    public GameObject noNewPanel;  // 暂无新内容弹窗 (用于消息和草稿)
    public GameObject noContentPanel;  // 暂无内容弹窗
    public Button noAccessConfirmButton;  // 无访问权限确认按钮
    public Button noAccessCloseButton;  // 无访问权限关闭按钮
    public Button noNewConfirmButton;  // 暂无新内容确认按钮
    public Button noNewCloseButton;  // 暂无新内容关闭按钮
    public TextMeshProUGUI noNewText;  // 暂无新内容文本
    public Button noContentConfirmButton;  // 暂无内容确认按钮
    public Button noContentCloseButton;  // 暂无内容关闭按钮

    [Header("第二章 - 座位排列小游戏")]
    public GameObject seatPuzzlePanel;  // 座位排列游戏面板
    public SeatPuzzleGame seatPuzzleGame;  // 座位游戏组件引用

    [Header("第二章 - 解锁后的日志博客面板")]
    public GameObject logPanel;  // 日志博客面板

    [Header("日记列表UI")]
    public Transform diaryEntriesContainer;
    public GameObject diaryEntryPrefab;

    [Header("文章内容UI")]
    public GameObject contentPanel;
    public TextMeshProUGUI articleTitleText;
    public TextMeshProUGUI articleContentText;
    public TextMeshProUGUI articleDateText;
    public Button articleBackButton;

    [Header("博客数据")]
    public BlogData blogData;

    // 状态管理
    private List<BlogPost> currentPosts = new List<BlogPost>();
    private List<BlogPost> collectedPosts = new List<BlogPost>();
    private BlogPost currentPost;
    private bool isDiaryMode = false;
    private bool isContentMode = false;

    void Start()
    {
        InitializeBlog();
    }

    void InitializeBlog()
    {
        Debug.Log("[BlogApp] 开始初始化博客应用");

        LoadBlogData();
        SetupUIEvents();
        InitializePanels();  // 初始化面板状态

        // 添加诊断日志
        int currentChapter = GetCurrentChapter();
        bool puzzleSolved = IsSeatPuzzleSolved();
        Debug.Log($"[BlogApp] 当前章节: {currentChapter}, 座位游戏完成状态: {puzzleSolved}");

        Debug.Log("[BlogApp] 博客应用初始化完成");
    }

    /// <summary>
    /// 初始化所有面板的初始状态
    /// </summary>
    void InitializePanels()
    {
        // 隐藏所有弹窗
        if (noAccessPanel != null) noAccessPanel.SetActive(false);
        if (noNewPanel != null) noNewPanel.SetActive(false);
        if (noContentPanel != null) noContentPanel.SetActive(false);
        if (imageViewerPanel != null) imageViewerPanel.SetActive(false);
        if (contentPanel != null) contentPanel.SetActive(false);
        if (seatPuzzlePanel != null) seatPuzzlePanel.SetActive(false);  // 隐藏座位游戏面板
        if (logPanel != null) logPanel.SetActive(false);  // 隐藏日志面板

        // 显示博客长图滚动视图
        if (diaryScrollRect != null)
        {
            diaryScrollRect.gameObject.SetActive(true);

            // 强制刷新ContentSizeFitter
            StartCoroutine(RefreshScrollRect());
        }

        Debug.Log("博客面板初始化完成");
    }

    /// <summary>
    /// 刷新ScrollRect的Content大小
    /// </summary>
    System.Collections.IEnumerator RefreshScrollRect()
    {
        yield return null;  // 等待一帧

        if (diaryScrollRect != null && diaryScrollRect.content != null)
        {
            // 强制重建布局
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(diaryScrollRect.content);

            float contentHeight = diaryScrollRect.content.rect.height;
            float viewportHeight = diaryScrollRect.viewport.rect.height;

            Debug.Log($"===== ScrollRect 调试信息 =====");
            Debug.Log($"Content高度: {contentHeight}");
            Debug.Log($"Viewport高度: {viewportHeight}");
            Debug.Log($"是否可以滚动: {contentHeight > viewportHeight}");
            Debug.Log($"垂直滚动条: {(diaryScrollRect.verticalScrollbar != null ? "已连接" : "未连接")}");

            // 如果Content高度还是0或小于Viewport,强制设置Content高度
            if (contentHeight <= viewportHeight)
            {
                Debug.LogWarning($"Content高度异常({contentHeight}),尝试手动设置...");

                // 获取BlogImageContainer的高度
                if (blogScrollImage != null)
                {
                    RectTransform imageRect = blogScrollImage.rectTransform;
                    float imageHeight = imageRect.rect.height;
                    Debug.Log($"BlogImageContainer高度: {imageHeight}");

                    // 手动设置Content的sizeDelta
                    RectTransform contentRect = diaryScrollRect.content;
                    contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, imageHeight);
                    Debug.Log($"已手动设置Content高度为: {imageHeight}");
                }
            }
        }
    }

    void LoadBlogData()
    {
        // 从数据管理器加载博客数据 (添加空引用检查)
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            var appData = GameManager.Instance.dataManager.GetAppData();
            if (appData != null)
            {
                // 这里应该从JSON加载，暂时使用默认数据
                LoadDefaultBlogData();
            }
            else
            {
                LoadDefaultBlogData();
            }
        }
        else
        {
            LoadDefaultBlogData();
        }

        currentPosts = blogData.posts;
    }

    void LoadDefaultBlogData()
    {
        blogData = new BlogData();
        blogData.posts = new List<BlogPost>
        {
            new BlogPost
            {
                id = "post_001",
                title = "新的开始",
                date = "2013-08-12",
                content = "今天和小付一起去拍照了，天气真好！我们就像周杰伦的《晴天》，喜欢第五句的音调许愿我们能永远这么好。风景真的很美，感觉我们的友谊也会像这风景一样永远美好。拍了很多照片，小付说我拍照技术越来越好了。其实我只是想把美好的瞬间都记录下来，毕竟时间过得太快了。",
                tags = new List<string> { "友情", "拍照", "回忆" },
                containsClue = true,
                clueType = "password",
                clueValue = "2557176",
                isCollected = false
            },
            new BlogPost
            {
                id = "post_002",
                title = "工作第一天",
                date = "2014-03-15",
                content = "今天开始在新公司实习了，老板看起来很严厉，但同事们都很好。希望能在这里学到很多！公司氛围还不错，中午大家一起吃饭，感觉会是一个不错的开始。虽然工作压力不小，但我相信只要努力就一定会有收获。",
                tags = new List<string> { "工作", "实习", "新开始" },
                containsClue = false,
                isCollected = false
            },
            new BlogPost
            {
                id = "post_003",
                title = "约会纪念",
                date = "2015-02-14",
                content = "和关钦在一起的第100天！他送了我一块很漂亮的手表，说这是我最喜欢的配饰。真的好开心~ 我们去吃了很棒的晚餐，还看了电影。他说最想送给我的礼物还是那块手表，因为那代表了我们的开始。2015214，我们开始相爱的日子。",
                tags = new List<string> { "爱情", "纪念日", "礼物" },
                containsClue = true,
                clueType = "relationship",
                clueValue = "watch",
                isCollected = false
            },
            new BlogPost
            {
                id = "post_004",
                title = "生日快乐",
                date = "2015-06-18",
                content = "今天我生日，关钦准备了惊喜！他说最想送给我的礼物还是那块手表，因为那代表了我们的开始。2015214，我们开始相爱的日子。朋友们都来庆祝，真的很感动。虽然最近工作很忙，但有大家的陪伴，我觉得很幸福。",
                tags = new List<string> { "生日", "爱情", "朋友" },
                containsClue = true,
                clueType = "anniversary",
                clueValue = "2015214",
                isCollected = false
            },
            new BlogPost
            {
                id = "post_005",
                title = "网红生活",
                date = "2016-01-20",
                content = "最近粉丝数一直在涨，好开心！虽然工作很忙，但是能和大家分享生活真的很幸福。每天都有很多人留言支持我，这让我觉得很有动力。不过有时候也会感到压力，担心自己不够好。但还是要保持积极的心态！",
                tags = new List<string> { "网红", "工作", "生活" },
                containsClue = false,
                isCollected = false
            }
        };
    }

    void SetupUIEvents()
    {
        // 关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBlog);
        }

        // 日记按钮 - 根据章节显示不同内容
        if (diaryButton != null)
        {
            diaryButton.onClick.AddListener(OnDiaryButtonClicked);
        }

        // DiaryScrollRect点击按钮 - 第二章跳转到座位游戏或日志博客
        if (diaryScrollRectButton != null)
        {
            diaryScrollRectButton.onClick.AddListener(OnDiaryScrollRectClicked);
        }

        // 消息按钮 - 显示NoNewPanel "暂无消息"
        if (messageButton != null)
        {
            messageButton.onClick.AddListener(() => ShowNoNewPopup("暂无消息"));
        }

        // 草稿按钮 (collectionButton) - 显示NoContentPanel
        if (collectionButton != null)
        {
            collectionButton.onClick.AddListener(ShowNoContentPanel);
        }

        // 图片查看器关闭按钮
        if (imageViewerCloseButton != null)
        {
            imageViewerCloseButton.onClick.AddListener(CloseImageViewer);
        }

        // 无访问权限弹窗按钮
        if (noAccessConfirmButton != null)
        {
            noAccessConfirmButton.onClick.AddListener(HideNoAccessPanel);
        }

        if (noAccessCloseButton != null)
        {
            noAccessCloseButton.onClick.AddListener(HideNoAccessPanel);
        }

        // 暂无新内容弹窗按钮
        if (noNewConfirmButton != null)
        {
            noNewConfirmButton.onClick.AddListener(HideNoNewPanel);
        }

        if (noNewCloseButton != null)
        {
            noNewCloseButton.onClick.AddListener(HideNoNewPanel);
        }

        // 暂无内容弹窗按钮
        if (noContentConfirmButton != null)
        {
            noContentConfirmButton.onClick.AddListener(HideNoContentPanel);
        }

        if (noContentCloseButton != null)
        {
            noContentCloseButton.onClick.AddListener(HideNoContentPanel);
        }

        // 设置可点击区域
        SetupClickableAreas();

        // 文章返回按钮
        if (articleBackButton != null)
        {
            articleBackButton.onClick.AddListener(BackToDiaryList);
        }

        // 解锁后博客日志返回按钮已移除，直接使用原有博客功能
    }

    void CloseBlog()
    {
        // 关闭博客应用窗口 (添加空引用检查)
        gameObject.SetActive(false);

        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    void ShowDiaryMode()
    {
        // 显示博客长图
        if (diaryScrollRect != null)
        {
            diaryScrollRect.gameObject.SetActive(true);
        }

        // 隐藏其他面板
        HideAllPanels();
    }

    void RefreshDiaryList()
    {
        // 清除现有列表
        if (diaryEntriesContainer != null)
        {
            foreach (Transform child in diaryEntriesContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建日记条目
            foreach (var post in currentPosts)
            {
                CreateDiaryEntry(post);
            }
        }
    }

    void CreateDiaryEntry(BlogPost post)
    {
        // 添加Prefab空引用检查
        if (diaryEntryPrefab == null)
        {
            Debug.LogError("DiaryEntryPrefab未设置!");
            return;
        }

        if (diaryEntriesContainer == null)
        {
            Debug.LogError("DiaryEntriesContainer未设置!");
            return;
        }

        GameObject entryObj = Instantiate(diaryEntryPrefab, diaryEntriesContainer);

        // 设置日记条目内容
        TextMeshProUGUI titleText = entryObj.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI dateText = entryObj.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI previewText = entryObj.transform.Find("PreviewText")?.GetComponent<TextMeshProUGUI>();
        Button entryButton = entryObj.GetComponent<Button>();
        Button collectButton = entryObj.transform.Find("CollectButton")?.GetComponent<Button>();

        if (titleText != null)
        {
            titleText.text = post.title;
        }

        if (dateText != null)
        {
            dateText.text = post.date;
        }

        if (previewText != null)
        {
            // 显示内容预览（前50个字符）
            string preview = post.content.Length > 50 ? post.content.Substring(0, 50) + "..." : post.content;
            previewText.text = preview;
        }

        if (entryButton != null)
        {
            entryButton.onClick.AddListener(() => OpenPost(post));
        }

        if (collectButton != null)
        {
            collectButton.onClick.AddListener(() => ToggleCollect(post, collectButton));

            // 更新收藏按钮状态
            UpdateCollectButtonState(collectButton, post.isCollected);
        }
    }

    void OpenPost(BlogPost post)
    {
        currentPost = post;
        isContentMode = true;

        // 隐藏日记列表
        if (diaryScrollRect != null)
        {
            diaryScrollRect.gameObject.SetActive(false);
        }

        // 显示内容面板
        if (contentPanel != null)
        {
            contentPanel.SetActive(true);
        }

        // 设置文章内容
        if (articleTitleText != null)
        {
            articleTitleText.text = post.title;
        }

        if (articleContentText != null)
        {
            articleContentText.text = post.content;
        }

        if (articleDateText != null)
        {
            articleDateText.text = post.date;
        }

        // 如果包含线索，显示提示
        if (post.containsClue)
        {
            Debug.Log($"发现线索: {post.clueType} - {post.clueValue}");

            // 自动记录到记事本
            if (GameManager.Instance.appManager != null)
            {
                var notebookApp = FindObjectOfType<NotebookApp>();
                if (notebookApp != null)
                {
                    notebookApp.AddNoteAutomatically($"在博客《{post.title}��中发现线索：{post.clueValue}");
                }
            }

            // 显示线索发现提示
            GameManager.Instance.uiManager?.ShowMessage("发现了一条重要线索...");

            // 短暂震动效果
            if (GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.ShakeScreen(3f, 0.3f);
            }
        }

        // 标记文章为已读
        MarkPostAsRead(post.id);
    }

    void BackToDiaryList()
    {
        ShowDiaryMode();
    }

    void ToggleCollect(BlogPost post, Button collectButton)
    {
        post.isCollected = !post.isCollected;

        if (post.isCollected)
        {
            if (!collectedPosts.Contains(post))
            {
                collectedPosts.Add(post);
            }
        }
        else
        {
            collectedPosts.Remove(post);
        }

        UpdateCollectButtonState(collectButton, post.isCollected);

        // 保存收藏状态
        SaveCollectionState();
    }

    void UpdateCollectButtonState(Button collectButton, bool isCollected)
    {
        if (collectButton == null) return;

        // 这里可以根据收藏状态改变按钮的外观
        Image buttonImage = collectButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            // 如果有收藏图标，可以切换颜色或图像
            buttonImage.color = isCollected ? Color.yellow : Color.white;
        }
    }

    void SaveCollectionState()
    {
        // 保存收藏状态到数据管理器 (添加空引用检查)
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            var saveData = GameManager.Instance.dataManager.GetSaveData();
            if (saveData != null)
            {
                // 这里可以扩展保存结构来包含收藏信息
                GameManager.Instance.dataManager.SaveGameData();
            }
        }
    }

    void MarkPostAsRead(string postId)
    {
        // 添加空引用检查
        if (GameManager.Instance == null || GameManager.Instance.dataManager == null)
        {
            return;
        }

        var saveData = GameManager.Instance.dataManager.GetSaveData();
        if (saveData != null && saveData.blogPostsRead == null)
        {
            saveData.blogPostsRead = new List<string>();
        }

        if (saveData != null && !saveData.blogPostsRead.Contains(postId))
        {
            saveData.blogPostsRead.Add(postId);
            GameManager.Instance.dataManager.SaveGameData();
        }
    }

    /// <summary>
    /// 搜索博客文章
    /// </summary>
    public List<BlogPost> SearchPosts(string keyword)
    {
        List<BlogPost> foundPosts = new List<BlogPost>();

        if (string.IsNullOrEmpty(keyword))
        {
            return foundPosts;
        }

        foreach (var post in blogData.posts)
        {
            if (post.title.ToLower().Contains(keyword.ToLower()) ||
                post.content.ToLower().Contains(keyword.ToLower()) ||
                (post.tags != null && post.tags.Exists(tag => tag.ToLower().Contains(keyword.ToLower()))))
            {
                foundPosts.Add(post);
            }
        }

        return foundPosts;
    }

    /// <summary>
    /// 按标签筛选文章
    /// </summary>
    public List<BlogPost> FilterPostsByTag(string tag)
    {
        List<BlogPost> filteredPosts = new List<BlogPost>();

        if (string.IsNullOrEmpty(tag))
        {
            filteredPosts = new List<BlogPost>(blogData.posts);
        }
        else
        {
            foreach (var post in blogData.posts)
            {
                if (post.tags != null && post.tags.Contains(tag))
                {
                    filteredPosts.Add(post);
                }
            }
        }

        currentPosts = filteredPosts;
        RefreshDiaryList();

        return filteredPosts;
    }

    /// <summary>
    /// 日记按钮点击 - 根据章节和小游戏状态决定行为
    /// </summary>
    void OnDiaryButtonClicked()
    {
        int currentChapter = GetCurrentChapter();

        if (currentChapter == 1)
        {
            // 第一章：显示无权限弹窗
            ShowNoAccessPanel();
        }
        else if (currentChapter >= 2)
        {
            // 第二章：检查小游戏是否通过
            if (IsSeatPuzzleSolved())
            {
                // 小游戏已通过：显示隐藏日志
                ShowHiddenDiary();
            }
            else
            {
                // 小游戏未通过：打开座位排列游戏
                ShowSeatPuzzleGame();
            }
        }
    }

    /// <summary>
    /// DiaryScrollRect点击事件 - 第二章跳转到座位游戏或日志博客
    /// </summary>
    void OnDiaryScrollRectClicked()
    {
        int currentChapter = GetCurrentChapter();

        if (currentChapter == 1)
        {
            // 第一章：保持原有逻辑（暂无权限）
            ShowNoAccessPanel();
        }
        else if (currentChapter >= 2)
        {
            // 第二章：检查小游戏是否通过
            if (IsSeatPuzzleSolved())
            {
                // 小游戏已通过：直接显示隐藏日志
                ShowHiddenDiary();
            }
            else
            {
                // 小游戏未通过：打开座位排列游戏
                ShowSeatPuzzleGame();
            }
        }
    }

    /// <summary>
    /// 获取当前章节
    /// </summary>
    int GetCurrentChapter()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            var saveData = GameManager.Instance.dataManager.GetSaveData();
            if (saveData != null)
            {
                return saveData.currentChapter;
            }
        }
        return 1; // 默认第一章
    }

    /// <summary>
    /// 检查座位排列游戏是否已完成
    /// </summary>
    bool IsSeatPuzzleSolved()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            return GameManager.Instance.dataManager.IsPasswordSolved("blog_seat_puzzle");
        }
        return false;
    }

    /// <summary>
    /// 保存座位排列游戏完成状态
    /// </summary>
    public void MarkSeatPuzzleSolved()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved("blog_seat_puzzle");
            Debug.Log("座位排列游戏已完成并保存");
        }
    }

    /// <summary>
    /// 显示座位排列小游戏
    /// </summary>
    void ShowSeatPuzzleGame()
    {
        Debug.Log("[BlogApp] 尝试显示座位排列游戏");

        // 诊断日志：检查所有必需组件
        Debug.Log($"[BlogApp] seatPuzzlePanel: {(seatPuzzlePanel != null ? "✓" : "✗ 未配置")}");
        Debug.Log($"[BlogApp] seatPuzzleGame: {(seatPuzzleGame != null ? "✓" : "✗ 未配置")}");

        if (seatPuzzlePanel != null)
        {
            seatPuzzlePanel.SetActive(true);

            // 初始化游戏
            if (seatPuzzleGame != null)
            {
                seatPuzzleGame.InitializeGame();
                Debug.Log("[BlogApp] ✓ 座位排列游戏已显示并初始化");
            }
            else
            {
                Debug.LogError("[BlogApp] ✗ seatPuzzleGame组件未配置 - 请在Unity Inspector中拖拽SeatPuzzleGame组件");
            }
        }
        else
        {
            Debug.LogError("[BlogApp] ✗ 座位排列游戏面板未配置 - 请在Unity Inspector中拖拽seatPuzzlePanel对象");
        }
    }

    /// <summary>
    /// 显示隐藏日志 - 显示日志博客面板
    /// </summary>
    void ShowHiddenDiary()
    {
        Debug.Log("[BlogApp] 座位游戏完成，显示解锁后的日志博客面板");

        // 隐藏所有其他面板，确保博客界面清晰
        HideAllPanels();

        // 显示日志博客面板
        if (logPanel != null)
        {
            logPanel.SetActive(true);
            Debug.Log("[BlogApp] ✓ 显示日志博客面板");

            // 如果日志面板里有日记列表容器，显示日记列表
            if (diaryEntriesContainer != null && diaryEntryPrefab != null)
            {
                ShowDiaryList();
            }
        }
        else
        {
            Debug.LogError("[BlogApp] logPanel未配置，无法显示日志博客面板");

            // 如果没有配置日志面板，就显示原来的博客滚动区域
            if (diaryScrollRect != null)
            {
                diaryScrollRect.gameObject.SetActive(true);
                Debug.Log("[BlogApp] 使用原有博客滚动区域作为备选");
            }
        }

        // 进入日记模式
        isDiaryMode = true;
        Debug.Log("[BlogApp] 已进入日记模式，可以使用所有博客功能");
    }

    public void OpenDiaryAfterPuzzle()
    {
        ShowHiddenDiary();
    }

    /// <summary>
    /// 显示日记列表
    /// </summary>
    void ShowDiaryList()
    {
        Debug.Log("[BlogApp] 显示日记列表");

        // 清空现有日记
        foreach (Transform child in diaryEntriesContainer)
        {
            Destroy(child.gameObject);
        }

        // 这里可以添加具体的日记条目
        // 显示所有博客文章作为日记
        if (blogData != null && blogData.posts != null && blogData.posts.Count > 0)
        {
            // 显示所有博客文章作为日记
            foreach (var post in blogData.posts)
            {
                CreateDiaryEntry(post);
            }
        }
        else
        {
            // 如果没有博客数据，创建一个默认的日记条目
            Debug.Log("[BlogApp] 没有博客数据，无法创建日记条目");
        }
    }

    
    /// <summary>
    /// 隐藏所有面板（除了主博客滚动区域）
    /// </summary>
    void HideAllPanels()
    {
        // 隐藏所有弹窗和面板
        if (noAccessPanel != null) noAccessPanel.SetActive(false);
        if (noNewPanel != null) noNewPanel.SetActive(false);
        if (noContentPanel != null) noContentPanel.SetActive(false);
        if (imageViewerPanel != null) imageViewerPanel.SetActive(false);
        if (contentPanel != null) contentPanel.SetActive(false);
        if (seatPuzzlePanel != null) seatPuzzlePanel.SetActive(false);
        if (logPanel != null) logPanel.SetActive(false);

        Debug.Log("[BlogApp] 已隐藏所有其他面板");
    }

    // 已移除解锁后博客日志相关方法，直接使用原有博客功能

    /// <summary>
    /// 显示无访问权限弹窗 (阶段1新增)
    /// </summary>
    void ShowNoAccessPanel()
    {
        if (noAccessPanel != null)
        {
            noAccessPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("NoAccessPanel未设置!");
        }
    }

    /// <summary>
    /// 隐藏无访问权限弹窗 (阶段1新增)
    /// </summary>
    void HideNoAccessPanel()
    {
        if (noAccessPanel != null)
        {
            noAccessPanel.SetActive(false);
        }
    }

    // ==================== 新增功能方法 ====================

    /// <summary>
    /// 显示博客滚动视图(长图)
    /// </summary>
    void ShowBlogScrollView()
    {
        // 显示滚动区域
        if (diaryScrollRect != null)
        {
            diaryScrollRect.gameObject.SetActive(true);
        }

        // 隐藏其他面板
        HideAllPanels();

        Debug.Log("显示博客长图滚动视图");
    }

    /// <summary>
    /// 设置可点击区域
    /// </summary>
    void SetupClickableAreas()
    {
        if (clickableAreas == null || clickableAreas.Length == 0)
        {
            return;
        }

        for (int i = 0; i < clickableAreas.Length; i++)
        {
            if (clickableAreas[i] != null)
            {
                int index = i;  // 捕获循环变量
                clickableAreas[i].onClick.RemoveAllListeners();
                clickableAreas[i].onClick.AddListener(() => ShowImageDetail(index));
            }
        }

        Debug.Log($"设置了 {clickableAreas.Length} 个可点击区域");
    }

    /// <summary>
    /// 显示图片详情(大图)
    /// </summary>
    void ShowImageDetail(int index)
    {
        if (imageViewerPanel == null || imageViewerImage == null)
        {
            Debug.LogWarning("图片查看器未配置!");
            return;
        }

        if (imageDetails == null || index < 0 || index >= imageDetails.Length)
        {
            Debug.LogWarning($"图片索引 {index} 超出范围!");
            return;
        }

        // 设置大图
        imageViewerImage.sprite = imageDetails[index];

        // 显示图片查看器
        imageViewerPanel.SetActive(true);

        Debug.Log($"显示大图: {index}");
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

    /// <summary>
    /// 显示暂无新内容弹窗
    /// </summary>
    void ShowNoNewPopup(string message)
    {
        if (noNewPanel == null)
        {
            Debug.LogWarning("NoNewPanel未配置!");
            return;
        }

        // 设置提示文本
        if (noNewText != null)
        {
            noNewText.text = message;
        }

        // 显示弹窗
        noNewPanel.SetActive(true);

        Debug.Log($"显示提示: {message}");
    }

    /// <summary>
    /// 隐藏暂无新内容弹窗
    /// </summary>
    void HideNoNewPanel()
    {
        if (noNewPanel != null)
        {
            noNewPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 显示暂无内容弹窗
    /// </summary>
    void ShowNoContentPanel()
    {
        if (noContentPanel != null)
        {
            noContentPanel.SetActive(true);
            Debug.Log("显示NoContentPanel弹窗");
        }
        else
        {
            Debug.LogWarning("NoContentPanel未配置!");
        }
    }

    /// <summary>
    /// 隐藏暂无内容弹窗
    /// </summary>
    void HideNoContentPanel()
    {
        if (noContentPanel != null)
        {
            noContentPanel.SetActive(false);
        }
    }
}

// 博客数据结构
[System.Serializable]
public class BlogData
{
    public List<BlogPost> posts;
}

[System.Serializable]
public class BlogPost
{
    public string id;
    public string title;
    public string date;
    public string content;
    public List<string> tags;
    public bool containsClue;
    public string clueType;
    public string clueValue;
    public bool isCollected;
}
