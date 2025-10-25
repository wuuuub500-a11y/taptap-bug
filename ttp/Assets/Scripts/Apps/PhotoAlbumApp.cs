using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 相册应用 - 管理生活、爱情、工作和隐藏相册
/// </summary>
public class PhotoAlbumApp : MonoBehaviour
{
    [Header("UI组件")]
    public Button closeButton;
    public Button backButton;

    [Header("相册分组")]
    public Transform albumContainer;
    public GameObject albumItemPrefab;

    [Header("相册内容")]
    public GameObject albumContentPanel;
    public TextMeshProUGUI albumTitleText;
    public Transform photosContainer;
    public GameObject photoItemPrefab;
    public GameObject photoDetailPanel;

    [Header("照片详情")]
    public Image detailPhoto;
    public TextMeshProUGUI detailCaptionText;
    public Button detailCloseButton;

    [Header("密码输入")]
    public GameObject passwordPanel;
    public TMP_InputField passwordInput;
    public Button passwordSubmitButton;
    public Button passwordCancelButton;
    public TextMeshProUGUI passwordPromptText;

    [Header("第二章 - 隐私相册")]
    public GameObject privateAlbumPanel;
    public TMP_InputField privatePasswordInput;
    public Button privatePasswordSubmitButton;
    public Button privatePasswordCancelButton;
    public TextMeshProUGUI privatePasswordPromptText;
    public TextMeshProUGUI privateAlbumTitleText;
    public Transform privatePhotosContainer;
    public GameObject privatePhotoItemPrefab;

    [Header("翻页功能")]
    public Button prevPageButton;
    public Button nextPageButton;
    public TextMeshProUGUI pageInfoText;
    public int photosPerPage = 6;

    // 相册数据
    private Dictionary<string, AlbumData> albums = new Dictionary<string, AlbumData>();
    private Dictionary<string, bool> albumUnlocked = new Dictionary<string, bool>();
    private string currentAlbumId = "";

    // 密码与状态
    private const string LIFE_ALBUM_PASSWORD_ID = "photoalbum_life";
    private const string LIFE_SPECIAL_PHOTO_ID = "life_mosaic";
    private const string SHARED_PASSWORD = "2557176"; // 博客密码(晴天歌词+拍照日期)
    private const string PRIVATE_ALBUM_PASSWORD = "2411"; // 隐私相册密码
    private const string PRIVATE_ALBUM_ID = "private";
    private const string PRIVATE_ALBUM_PASSWORD_ID = "photoalbum_private";
    private bool isLifeAlbumUnlocked = false;
    private bool isPrivateAlbumUnlocked = false;
    private bool isChapter2 = false;

    // 翻页状态
    private int currentPage = 1;
    private int totalPages = 1;
    private List<PhotoData> currentPhotos = new List<PhotoData>();
    private bool isUnlockingAlbum = false;
    private PhotoData pendingPhotoToUnlock = null;

    private bool hasInitialized = false;
    private bool HasPhotoUnlocked(string photoId)
    {
        var dataMgr = GameManager.Instance != null ? GameManager.Instance.dataManager : null;
        var saveData = dataMgr != null ? dataMgr.GetSaveData() : null;
        return saveData != null && saveData.photosUnlocked != null && saveData.photosUnlocked.Contains(photoId);
    }

    void Start()
    {
        InitializeAlbum();

        // 监听章节切换事件
        ChapterManager.OnChapter2Started += OnChapter2Started;
    }

    void OnDestroy()
    {
        // 清理事件监听
        ChapterManager.OnChapter2Started -= OnChapter2Started;
    }

    void OnEnable()
    {
        if (!hasInitialized)
        {
            return;
        }

        LoadUnlockState();

        if (isLifeAlbumUnlocked)
        {
            ShowAlbumList();
        }
        else
        {
            PrepareLockedAlbumState();
            ShowAlbumPasswordGate();
        }
    }

    void InitializeAlbum()
    {
        SetupUIEvents();
        LoadAlbumData();
        LoadUnlockState();

        if (isLifeAlbumUnlocked)
        {
            ShowAlbumList();
        }
        else
        {
            PrepareLockedAlbumState();
            ShowAlbumPasswordGate();
        }

        hasInitialized = true;
    }

    void SetupUIEvents()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseAlbum);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToAlbumList);
        }

        if (detailCloseButton != null)
        {
            detailCloseButton.onClick.AddListener(ClosePhotoDetail);
        }

        if (passwordSubmitButton != null)
        {
            passwordSubmitButton.onClick.AddListener(OnPasswordSubmit);
        }

        if (passwordCancelButton != null)
        {
            passwordCancelButton.onClick.AddListener(OnPasswordCancel);
        }

        if (passwordInput != null)
        {
            passwordInput.onSubmit.AddListener((password) => OnPasswordSubmit());
        }

        // 隐私相册事件
        if (privatePasswordSubmitButton != null)
        {
            privatePasswordSubmitButton.onClick.AddListener(OnPrivatePasswordSubmit);
        }

        if (privatePasswordCancelButton != null)
        {
            privatePasswordCancelButton.onClick.AddListener(OnPrivatePasswordCancel);
        }

        if (privatePasswordInput != null)
        {
            privatePasswordInput.onSubmit.AddListener((password) => OnPrivatePasswordSubmit());
        }

        // 翻页按钮事件
        if (prevPageButton != null)
        {
            prevPageButton.onClick.AddListener(OnPrevPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(OnNextPage);
        }
    }

    void LoadUnlockState()
    {
        var dataManager = GameManager.Instance != null ? GameManager.Instance.dataManager : null;
        if (dataManager != null)
        {
            var saveData = dataManager.GetSaveData();
            isChapter2 = saveData != null && saveData.currentChapter >= 2;

            bool unlocked = dataManager.IsPasswordSolved(LIFE_ALBUM_PASSWORD_ID);
            if (!unlocked && HasPhotoUnlocked(LIFE_SPECIAL_PHOTO_ID))
            {
                unlocked = true;
                dataManager.MarkPasswordSolved(LIFE_ALBUM_PASSWORD_ID);
            }
            isLifeAlbumUnlocked = unlocked;

            // 加载隐私相册解锁状态
            isPrivateAlbumUnlocked = dataManager.IsPasswordSolved(PRIVATE_ALBUM_PASSWORD_ID);
            if (albumUnlocked.ContainsKey(PRIVATE_ALBUM_ID))
            {
                albumUnlocked[PRIVATE_ALBUM_ID] = isPrivateAlbumUnlocked;
            }
        }
        else
        {
            isLifeAlbumUnlocked = false;
            isPrivateAlbumUnlocked = false;
            isChapter2 = false;
        }
    }

    void SaveUnlockState()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved(LIFE_ALBUM_PASSWORD_ID);
        }
    }

    void LoadAlbumData()
    {
        // 生活相册 (与闺蜜小付有关)
        albums["life"] = new AlbumData
        {
            id = "life",
            name = "生活",
            description = "日常生活的美好瞬间",
            isLocked = false,
            photos = new List<PhotoData>
            {
                new PhotoData { id = "life_001", caption = "和小付的第一次合影", description = "这是我们第一次一起拍照，那时候我们都还很青涩", isLocked = false },
                new PhotoData { id = "life_002", caption = "咖啡店的下午", description = "在小付最喜欢的咖啡店度过了一个悠闲的下午", isLocked = false },
                new PhotoData { id = "life_003", caption = "雨中漫步", description = "下雨天一起在公园散步，感觉很浪漫", isLocked = false },
                new PhotoData { id = LIFE_SPECIAL_PHOTO_ID, caption = "神秘的风景照", description = "这张照片有些特殊，需要密码才能看清", isLocked = true, password = SHARED_PASSWORD }
            }
        };

        var lockedLifePhoto = albums["life"].photos.Find(p => p.id == LIFE_SPECIAL_PHOTO_ID);
        if (lockedLifePhoto != null && IsPhotoUnlocked(LIFE_SPECIAL_PHOTO_ID))
        {
            lockedLifePhoto.isLocked = false;
        }

        // 爱情相册 (与关钦有关)
        albums["love"] = new AlbumData
        {
            id = "love",
            name = "爱情",
            description = "与关钦的甜蜜时光",
            isLocked = false,
            photos = new List<PhotoData>
            {
                new PhotoData { id = "love_001", caption = "100天纪念", description = "我们在一起100天的纪念日，他送了我一块手表", isLocked = false },
                new PhotoData { id = "love_002", caption = "生日惊喜", description = "我生日时他准备的惊喜，真的太感动了", isLocked = false },
                new PhotoData { id = "love_003", caption = "看电影", description = "一起去看电影，我们最喜欢的《我的少女时代》", isLocked = false },
                new PhotoData { id = "love_004", caption = "海边漫步", description = "在海边散步的浪漫时光", isLocked = false }
            }
        };

        // 工作相册
        albums["work"] = new AlbumData
        {
            id = "work",
            name = "工作",
            description = "工作相关照片",
            isLocked = false,
            photos = new List<PhotoData>
            {
                new PhotoData { id = "work_001", caption = "公司门口", description = "新公司的门口，开始了新的工作生涯", isLocked = false },
                new PhotoData { id = "work_002", caption = "团队聚餐", description = "和同事们一起聚餐，氛围很好", isLocked = false }
            }
        };

        // 隐藏相册 (第二章解锁)
        albums["hidden"] = new AlbumData
        {
            id = "hidden",
            name = "隐藏",
            description = "秘密照片",
            isLocked = true, // 第二章才解锁
            photos = new List<PhotoData>()
        };

        // 隐私相册（第二章专用，需要密码）
        albums[PRIVATE_ALBUM_ID] = new AlbumData
        {
            id = PRIVATE_ALBUM_ID,
            name = "隐私",
            description = "需要密码才能查看的照片",
            isLocked = true,
            photos = new List<PhotoData>
            {
                new PhotoData { id = "private_photo_01", caption = "自拍 01", description = "第一张隐私自拍", isLocked = false },
                new PhotoData { id = "private_photo_02", caption = "自拍 02", description = "第二张隐私自拍", isLocked = false },
                new PhotoData { id = "private_photo_03", caption = "自拍 03", description = "第三张隐私自拍", isLocked = false },
                new PhotoData { id = "private_photo_04", caption = "自拍 04", description = "第四张隐私自拍", isLocked = false },
                new PhotoData { id = "private_photo_05", caption = "自拍 05", description = "第五张隐私自拍", isLocked = false },
                new PhotoData { id = "private_photo_06", caption = "自拍 06", description = "第六张隐私自拍", isLocked = false }
            }
        };

        // 初始化解锁状态
        albumUnlocked["life"] = true;
        albumUnlocked["love"] = true;
        albumUnlocked["work"] = true;
        albumUnlocked["hidden"] = false; // 第二章解锁
        albumUnlocked[PRIVATE_ALBUM_ID] = isPrivateAlbumUnlocked;
    }

    void CloseAlbum()
    {
        if (privateAlbumPanel != null)
        {
            privateAlbumPanel.SetActive(false);
        }

        if (passwordPanel != null)
        {
            passwordPanel.SetActive(false);
        }

        gameObject.SetActive(false);
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    void ShowAlbumList()
    {
        if (albumContainer == null) return;

        if (privateAlbumPanel != null)
        {
            privateAlbumPanel.SetActive(false);
        }

        if (albums.ContainsKey(PRIVATE_ALBUM_ID))
        {
            albums[PRIVATE_ALBUM_ID].isLocked = !isChapter2 || !isPrivateAlbumUnlocked;
        }

        // 清除现有相册列表
        ClearAlbumList();

        // 显示相册列表
        foreach (var kvp in albums)
        {
            AlbumData album = kvp.Value;
            CreateAlbumItem(album);
        }

        // 隐藏相册内容面板
        if (albumContentPanel != null)
        {
            albumContentPanel.SetActive(false);
        }
    }

    void ClearAlbumList()
    {
        if (albumContainer == null) return;

        foreach (Transform child in albumContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void CreateAlbumItem(AlbumData album)
    {
        if (albumItemPrefab == null || albumContainer == null) return;

        GameObject albumObj = Instantiate(albumItemPrefab, albumContainer);

        TextMeshProUGUI titleText = albumObj.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = albumObj.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
        Button albumButton = albumObj.GetComponent<Button>();
        Image backgroundImage = albumObj.GetComponent<Image>();

        if (titleText != null)
        {
            titleText.text = album.name;
        }

        if (descText != null)
        {
            descText.text = album.description;
        }

        // 设置相册状态
        bool isPrivateAlbum = album.id == PRIVATE_ALBUM_ID;
        bool isLocked = isPrivateAlbum ? (!isChapter2 || !isPrivateAlbumUnlocked) : album.isLocked;
        if (backgroundImage != null)
        {
            backgroundImage.color = isLocked ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
        }

        if (titleText != null)
        {
            titleText.color = isLocked ? Color.gray : Color.black;
        }

        if (albumButton != null)
        {
            albumButton.onClick.RemoveAllListeners();
            if (isPrivateAlbum)
            {
                albumButton.interactable = true; // 始终允许点击以提示或进入
            }
            else
            {
                albumButton.interactable = !isLocked;
            }

            if (isPrivateAlbum)
            {
                albumButton.onClick.AddListener(OnPrivateAlbumClicked);

                if (isLocked && !isChapter2)
                {
                    albumButton.onClick.AddListener(() => ShowPrivateAlbumLockedMessage());
                }
            }
            else
            {
                albumButton.onClick.AddListener(() => OpenAlbum(album.id));
            }

            if (isLocked && !isPrivateAlbum)
            {
                albumButton.onClick.AddListener(() => ShowAlbumLockedMessage(album.name));
            }
        }
    }

    void ShowAlbumLockedMessage(string albumName)
    {
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowMessage($"相册【{albumName}】尚未解锁，请先完成前面的任务！");
        }
    }

    void ShowPrivateAlbumLockedMessage()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowMessage("隐私相册暂无权限，待进入第二章后才能查看");
        }
    }

    void OnPrivateAlbumClicked()
    {
        if (!isChapter2)
        {
            ShowPrivateAlbumLockedMessage();
            return;
        }

        if (!isPrivateAlbumUnlocked)
        {
            ShowPrivateAlbumPasswordDialog();
        }
        else
        {
            ShowPrivateAlbum();
        }
    }

    void OpenAlbum(string albumId)
    {
        if (!albums.ContainsKey(albumId))
        {
            Debug.LogWarning($"相册 {albumId} 不存在");
            return;
        }

        AlbumData album = albums[albumId];
        currentAlbumId = albumId;

        // 显示相册内容面板
        if (albumContentPanel != null)
        {
            albumContentPanel.SetActive(true);
        }

        // 设置标题
        if (albumTitleText != null)
        {
            albumTitleText.text = album.name + "相册";
        }

        // 显示照片
        DisplayPhotos(album.photos);
    }

    void DisplayPhotos(List<PhotoData> photos)
    {
        if (photosContainer == null) return;

        // 清除现有照片
        foreach (Transform child in photosContainer)
        {
            Destroy(child.gameObject);
        }

        // 创建照片项
        foreach (var photo in photos)
        {
            CreatePhotoItem(photo);
        }
    }

    void CreatePhotoItem(PhotoData photo)
    {
        if (photoItemPrefab == null || photosContainer == null) return;

        GameObject photoObj = Instantiate(photoItemPrefab, photosContainer);

        TextMeshProUGUI captionText = photoObj.transform.Find("CaptionText")?.GetComponent<TextMeshProUGUI>();
        Button photoButton = photoObj.GetComponent<Button>();
        Image photoImage = photoObj.transform.Find("PhotoImage")?.GetComponent<Image>();

        if (captionText != null)
        {
            captionText.text = photo.caption;
        }

        bool isLocked = photo.isLocked && !IsPhotoUnlocked(photo.id);

        if (isLocked)
        {
            if (photoImage != null)
            {
                photoImage.color = new Color(0.3f, 0.3f, 0.3f);
            }

            if (captionText != null)
            {
                captionText.text = "【密码保护】" + photo.caption;
            }
        }
        else if (photoImage != null)
        {
            photoImage.color = Color.white;
        }

        if (photoButton != null)
        {
            photoButton.onClick.AddListener(() => OnPhotoClicked(photo));
        }
    }

    void OnPhotoClicked(PhotoData photo)
    {
        bool isLocked = photo.isLocked && !IsPhotoUnlocked(photo.id);
        if (isLocked)
        {
            ShowPasswordPanel(photo);
        }
        else
        {
            ShowPhotoDetail(photo);
        }
    }

    void ShowPasswordPanel(PhotoData photo)
    {
        isUnlockingAlbum = false;
        pendingPhotoToUnlock = photo;

        if (passwordPanel != null)
        {
            passwordPanel.SetActive(true);
        }

        if (passwordPromptText != null)
        {
            passwordPromptText.text = $"照片【{photo.caption}】需要密码才能查看";
        }

        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.Select();
        }

        // 保存当前照片信息
        // 这里可以用一个临时变量存储当前照片
    }

    void HidePasswordPanel()
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(false);
        }

        pendingPhotoToUnlock = null;
        isUnlockingAlbum = false;
    }

    void OnPasswordCancel()
    {
        bool wasUnlockingAlbum = isUnlockingAlbum && !isLifeAlbumUnlocked;
        HidePasswordPanel();

        if (wasUnlockingAlbum)
        {
            CloseAlbum();
        }
    }

    void OnPasswordSubmit()
    {
        if (passwordInput == null) return;

        string password = passwordInput.text.Trim();

        if (isUnlockingAlbum)
        {
            HandleAlbumPasswordSubmit(password);
            return;
        }

        if (pendingPhotoToUnlock != null)
        {
            HandlePhotoPasswordSubmit(password);
            return;
        }
    }

    void HandleAlbumPasswordSubmit(string password)
    {
        if (password == SHARED_PASSWORD)
        {
            isLifeAlbumUnlocked = true;
            SaveUnlockState();
            HidePasswordPanel();
            ShowSuccessMessage("相册已解锁！");
            ShowAlbumList();
        }
        else
        {
            ShowPasswordError();
        }
    }

    void HandlePhotoPasswordSubmit(string password)
    {
        if (pendingPhotoToUnlock == null)
        {
            return;
        }

        string expectedPassword = pendingPhotoToUnlock.password;

        if (!string.IsNullOrEmpty(expectedPassword) && password == expectedPassword)
        {
            UnlockPhoto(pendingPhotoToUnlock.id);
            pendingPhotoToUnlock = null;
            HidePasswordPanel();
            ShowSuccessMessage("照片已解锁！");

            if (!string.IsNullOrEmpty(currentAlbumId))
            {
                OpenAlbum(currentAlbumId);
            }
        }
        else
        {
            ShowPasswordError();
        }
    }

    void ShowPasswordError()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowError("密码错误！");
            GameManager.Instance.uiManager.ShakeScreen(2f, 0.2f);
        }

        if (passwordPromptText != null)
        {
            passwordPromptText.text = "密码错误，请重新输入";
        }

        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.Select();
        }
    }

    void UnlockPhoto(string photoId)
    {
        foreach (var album in albums.Values)
        {
            foreach (var photo in album.photos)
            {
                if (photo.id == photoId)
                {
                    photo.isLocked = false;
                    Debug.Log($"解锁照片: {photo.caption}");

                    // 记录解锁状态
                    SavePhotoUnlock(photoId);

                    if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
                    {
                        GameManager.Instance.dataManager.MarkPasswordSolved(photoId);
                    }

                    // 显示特殊照片的描述
                    if (photoId == LIFE_SPECIAL_PHOTO_ID)
                    {
                        photo.description = "这是一张三人照：前面两个人手挽着手，后面还有一个人。隐晦地暗示了三个人之间的关系...";
                        isLifeAlbumUnlocked = true;
                        SaveUnlockState();
                        
                        // 添加线索到笔记本
                        if (GameManager.Instance != null)
                        {
                            var notebookApp = FindObjectOfType<NotebookApp>();
                            if (notebookApp != null)
                            {
                                notebookApp.AddNoteAutomatically("解锁了一张特殊的照片，似乎暗示着某种三角关系...");
                            }
                        }

                        // 震动效果
                        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                        {
                            GameManager.Instance.uiManager.ShakeScreen(4f, 0.4f);
                        }
                    }
                    break;
                }
            }
        }
    }

    void SavePhotoUnlock(string photoId)
    {
        var saveData = GameManager.Instance.dataManager.GetSaveData();
        if (saveData != null)
        {
            if (saveData.photosUnlocked == null)
            {
                saveData.photosUnlocked = new List<string>();
            }

            if (!saveData.photosUnlocked.Contains(photoId))
            {
                saveData.photosUnlocked.Add(photoId);
                GameManager.Instance.dataManager.SaveGameData();
            }
        }
    }

    void ShowPhotoDetail(PhotoData photo)
    {
        if (photoDetailPanel != null)
        {
            photoDetailPanel.SetActive(true);
        }

        if (detailPhoto != null)
        {
            // 这里应该设置照片的sprite
            // detailPhoto.sprite = photo.sprite;
            detailPhoto.color = photo.isLocked ? Color.gray : Color.white;
        }

        if (detailCaptionText != null)
        {
            detailCaptionText.text = $"{photo.caption}\n\n{photo.description}";
        }
    }

    void ClosePhotoDetail()
    {
        if (photoDetailPanel != null)
        {
            photoDetailPanel.SetActive(false);
        }
    }

    void BackToAlbumList()
    {
        currentAlbumId = "";
        if (privateAlbumPanel != null)
        {
            privateAlbumPanel.SetActive(false);
        }
        if (privatePasswordInput != null)
        {
            privatePasswordInput.text = "";
        }
        ShowAlbumList();
    }

    void ShowSuccessMessage(string message)
    {
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowSuccess(message);
        }
    }

    void PrepareLockedAlbumState()
    {
        ClearAlbumList();

        if (albumContentPanel != null)
        {
            albumContentPanel.SetActive(false);
        }

        if (photoDetailPanel != null)
        {
            photoDetailPanel.SetActive(false);
        }
    }

    void ShowAlbumPasswordGate()
    {
        isUnlockingAlbum = true;
        pendingPhotoToUnlock = null;

        if (passwordPanel != null)
        {
            passwordPanel.SetActive(true);
        }

        if (passwordPromptText != null)
        {
            passwordPromptText.text = "相册被加密，请输入密码";
        }

        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.Select();
        }
    }

    /// <summary>
    /// 解锁隐藏相册 (第二章使用)
    /// </summary>
    public void UnlockHiddenAlbum()
    {
        if (albumUnlocked.ContainsKey("hidden"))
        {
            albumUnlocked["hidden"] = true;
            if (albums.ContainsKey("hidden"))
            {
                albums["hidden"].isLocked = false;
            }
            
            ShowSuccessMessage("解锁了隐藏相册！");
            ShowAlbumList();
        }
    }

    bool IsPhotoUnlocked(string photoId)
    {
        return HasPhotoUnlocked(photoId) || (GameManager.Instance?.dataManager?.IsPasswordSolved(photoId) ?? false);
    }

    // ==================== 第二章功能 ====================

    /// <summary>
    /// 章节切换事件处理
    /// </summary>
    void OnChapter2Started()
    {
        Debug.Log("[PhotoAlbumApp] 检测到第二章，启用隐私相册功能");

        isChapter2 = true;

        if (!isPrivateAlbumUnlocked && albumUnlocked.ContainsKey(PRIVATE_ALBUM_ID))
        {
            albumUnlocked[PRIVATE_ALBUM_ID] = false;
        }

        // 在相册列表中添加隐私相册入口
        AddPrivateAlbumEntry();

        if (hasInitialized)
        {
            ShowAlbumList();
        }
    }

    /// <summary>
    /// 添加隐私相册入口
    /// </summary>
    void AddPrivateAlbumEntry()
    {
        // 这里可以在相册列表中添加隐私相册按钮
        // 具体实现取决于你的UI结构
        Debug.Log("[PhotoAlbumApp] 隐私相册入口已添加");
    }

    /// <summary>
    /// 显示隐私相册密码输入
    /// </summary>
    public void ShowPrivateAlbumPasswordDialog()
    {
        if (privateAlbumPanel != null)
        {
            privateAlbumPanel.SetActive(true);

            // 清空密码输入框
            if (privatePasswordInput != null)
            {
                privatePasswordInput.text = "";
                privatePasswordInput.Select();
                privatePasswordInput.ActivateInputField();
            }

            // 设置提示文字
            if (privatePasswordPromptText != null)
            {
                privatePasswordPromptText.text = "请输入密码查看隐私相册";
            }

            Debug.Log("[PhotoAlbumApp] 显示隐私相册密码输入");
        }
        else
        {
            Debug.LogWarning("[PhotoAlbumApp] 隐私相册面板未配置");
        }
    }

    /// <summary>
    /// 隐私相册密码提交
    /// </summary>
    void OnPrivatePasswordSubmit()
    {
        if (privatePasswordInput == null)
        {
            Debug.LogWarning("[PhotoAlbumApp] 隐私相册密码输入框未配置");
            return;
        }

        string inputPassword = privatePasswordInput.text.Trim();
        Debug.Log($"[PhotoAlbumApp] 尝试隐私相册密码: {inputPassword}");

        if (inputPassword == PRIVATE_ALBUM_PASSWORD)
        {
            Debug.Log("[PhotoAlbumApp] 隐私相册密码正确！");

            // 标记为已解锁
            isPrivateAlbumUnlocked = true;
            if (GameManager.Instance?.dataManager != null)
            {
                GameManager.Instance.dataManager.MarkPasswordSolved(PRIVATE_ALBUM_PASSWORD_ID);
            }

            albumUnlocked[PRIVATE_ALBUM_ID] = true;
            if (albums.ContainsKey(PRIVATE_ALBUM_ID))
            {
                albums[PRIVATE_ALBUM_ID].isLocked = false;
            }

            ShowSuccessMessage("隐私相册已解锁");

            // 隐藏密码面板
            if (privateAlbumPanel != null)
            {
                privateAlbumPanel.SetActive(false);
            }

            // 刷新相册列表状态
            ShowAlbumList();

            // 显示隐私相册
            ShowPrivateAlbum();
        }
        else
        {
            Debug.LogWarning("[PhotoAlbumApp] 隐私相册密码错误");

            // 显示错误提示
            if (privatePasswordPromptText != null)
            {
                privatePasswordPromptText.text = "密码错误，请重试";
            }

            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.ShowError("密码错误，无法进入隐私相册");
            }

            if (privatePasswordInput != null)
            {
                privatePasswordInput.text = "";
                privatePasswordInput.Select();
            }
        }
    }

    /// <summary>
    /// 隐私相册密码取消
    /// </summary>
    void OnPrivatePasswordCancel()
    {
        if (privateAlbumPanel != null)
        {
            privateAlbumPanel.SetActive(false);
        }

        if (privatePasswordInput != null)
        {
            privatePasswordInput.text = "";
        }
        Debug.Log("[PhotoAlbumApp] 取消隐私相册密码输入");
    }

    /// <summary>
    /// 显示隐私相册
    /// </summary>
    void ShowPrivateAlbum()
    {
        if (!isPrivateAlbumUnlocked)
        {
            ShowPrivateAlbumPasswordDialog();
            return;
        }

        // 设置标题
        if (privateAlbumTitleText != null)
        {
            privateAlbumTitleText.text = "隐私相册";
        }

        currentAlbumId = PRIVATE_ALBUM_ID;

        if (privatePasswordInput != null)
        {
            privatePasswordInput.text = "";
        }

        if (privatePasswordPromptText != null)
        {
            privatePasswordPromptText.text = "";
        }

        if (albumContentPanel != null)
        {
            albumContentPanel.SetActive(false);
        }

        if (photoDetailPanel != null)
        {
            photoDetailPanel.SetActive(false);
        }

        // 加载隐私相册数据
        LoadPrivateAlbumData();

        // 显示隐私相册面板
        if (privateAlbumPanel != null)
        {
            privateAlbumPanel.SetActive(true);
        }

        Debug.Log("[PhotoAlbumApp] 显示隐私相册");
    }

    /// <summary>
    /// 加载隐私相册数据
    /// </summary>
    void LoadPrivateAlbumData()
    {
        currentPhotos.Clear();

        if (albums.ContainsKey(PRIVATE_ALBUM_ID) && albums[PRIVATE_ALBUM_ID].photos != null)
        {
            currentPhotos.AddRange(albums[PRIVATE_ALBUM_ID].photos);
        }

        if (currentPhotos.Count == 0)
        {
            Debug.LogWarning("[PhotoAlbumApp] 隐私相册没有配置照片数据");
        }

        // 初始化翻页
        currentPage = 1;
        totalPages = Mathf.CeilToInt((float)currentPhotos.Count / photosPerPage);
        if (totalPages <= 0)
        {
            totalPages = 1;
        }

        // 显示第一页
        DisplayCurrentPage();
    }

    /// <summary>
    /// 显示当前页的照片
    /// </summary>
    void DisplayCurrentPage()
    {
        if (privatePhotosContainer == null || privatePhotoItemPrefab == null)
        {
            Debug.LogWarning("[PhotoAlbumApp] 隐私相册UI组件未配置");
            return;
        }

        // 清空现有照片
        foreach (Transform child in privatePhotosContainer)
        {
            Destroy(child.gameObject);
        }

        // 计算当前页的照片范围
        int startIndex = (currentPage - 1) * photosPerPage;
        int endIndex = Mathf.Min(startIndex + photosPerPage, currentPhotos.Count);

        // 生成当前页的照片
        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject photoItem = Instantiate(privatePhotoItemPrefab, privatePhotosContainer);
            // 设置照片数据和事件
            SetupPhotoItem(photoItem, currentPhotos[i]);
        }

        // 更新页码信息
        UpdatePageInfo();

        // 更新翻页按钮状态
        UpdatePageButtons();
    }

    /// <summary>
    /// 设置照片项
    /// </summary>
    void SetupPhotoItem(GameObject photoItem, PhotoData photoData)
    {
        // 这里设置照片的UI，比如图片、标题等
        // 并添加点击事件来查看大图
        Button photoButton = photoItem.GetComponent<Button>();
        if (photoButton != null)
        {
            photoButton.onClick.AddListener(() => OnPrivatePhotoClicked(photoData));
        }

        // 可以在这里设置照片的其他UI元素
        TextMeshProUGUI captionText = photoItem.GetComponentInChildren<TextMeshProUGUI>();
        if (captionText != null)
        {
            captionText.text = photoData.caption;
        }
    }

    /// <summary>
    /// 隐私照片点击事件
    /// </summary>
    void OnPrivatePhotoClicked(PhotoData photoData)
    {
        Debug.Log($"[PhotoAlbumApp] 点击隐私照片: {photoData.caption}");
        ShowPhotoDetail(photoData);
    }

    /// <summary>
    /// 上一页
    /// </summary>
    void OnPrevPage()
    {
        if (currentPhotos.Count == 0)
        {
            return;
        }

        if (currentPage > 1)
        {
            currentPage--;
            DisplayCurrentPage();
        }
    }

    /// <summary>
    /// 下一页
    /// </summary>
    void OnNextPage()
    {
        if (currentPhotos.Count == 0)
        {
            return;
        }

        if (currentPage < totalPages)
        {
            currentPage++;
            DisplayCurrentPage();
        }
    }

    /// <summary>
    /// 更新页码信息
    /// </summary>
    void UpdatePageInfo()
    {
        if (pageInfoText != null)
        {
            int safeTotal = Mathf.Max(1, totalPages);
            int safeCurrent = Mathf.Clamp(currentPage, 1, safeTotal);
            pageInfoText.text = $"{safeCurrent} / {safeTotal}";
        }
    }

    /// <summary>
    /// 更新翻页按钮状态
    /// </summary>
    void UpdatePageButtons()
    {
        if (prevPageButton != null)
        {
            prevPageButton.interactable = currentPage > 1;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPage < totalPages;
        }
    }
}

[System.Serializable]
public class AlbumData
{
    public string id;
    public string name;
    public string description;
    public bool isLocked;
    public List<PhotoData> photos;
}

[System.Serializable]
public class PhotoData
{
    public string id;
    public string caption;
    public string description;
    public bool isLocked;
    public string password;
    // public Sprite sprite; // 照片图片
}
