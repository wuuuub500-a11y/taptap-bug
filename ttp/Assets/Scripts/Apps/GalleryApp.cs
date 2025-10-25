using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 相册应用 - 支持相册分类和照片查看大图功能
/// 基于实际UI结构编写的完整实现
/// </summary>
public class GalleryApp : MonoBehaviour
{
    [Header("=== 主要UI组件 ===")]
    public Button closeButton;                  // 关闭按钮

    [Header("=== 相册分类按钮 ===")]
    public Button workAlbumButton;              // 工作相册按钮
    public Button lifeAlbumButton;              // 生活相册按钮
    public Button loveAlbumButton;              // 恋爱相册按钮
    public Button selfieAlbumButton;            // 自拍相册按钮
    public Button privateAlbumButton;           // 隐私相册按钮

    [Header("=== 相册面板 ===")]
    public GameObject workAlbumPanel;           // 工作相册面板
    public GameObject lifeAlbumPanel;           // 生活相册面板
    public GameObject loveAlbumPanel;           // 恋爱相册面板
    public GameObject selfieAlbumPanel;         // 自拍相册面板
    public GameObject privateAlbumPanel;        // 隐私相册面板

    [Header("=== 隐私相册分页 ===")]
    public GameObject privateAlbumPage1;        // 隐私相册第一页
    public GameObject privateAlbumPage2;        // 隐私相册第二页
    public Button privateAlbumPagePrevButton;   // 隐私相册上一页按钮
    public Button privateAlbumPageNextButton;   // 隐私相册下一页按钮
    public TextMeshProUGUI privateAlbumPageIndicator; // 隐私相册页码文本

    [Header("=== 照片按钮映射 ===")]
    // 工作相册 - 4张照片
    public Button workPhoto1;                   // 工作相册照片1 (位置 1.1)
    public Button workPhoto2;                   // 工作相册照片2 (位置 2.1)
    public Button workPhoto3;                   // 工作相册照片3 (位置 3.1)
    public Button workPhoto4;                   // 工作相册照片4 (位置 4.1)

    // 生活相册 - 2张照片（1张上锁）
    public Button lifePhoto1;                   // 生活相册照片1 (位置 1.1, 上锁)
    public Button lifePhoto2;                   // 生活相册照片2 (位置 2.1)

    // 恋爱相册 - 2张照片
    public Button lovePhoto1;                   // 恋爱相册照片1 (位置 1.1)
    public Button lovePhoto2;                   // 恋爱相册照片2 (位置 2.1)

    // 自拍相册 - 1张照片
    public Button selfiePhoto1;                 // 自拍相册照片1 (位置 1.1)

    // 隐私相册 - 6张照片（两页）
    public Button privatePhoto1;                // 隐私相册照片1 (第一页)
    public Button privatePhoto2;                // 隐私相册照片2 (第一页)
    public Button privatePhoto3;                // 隐私相册照片3 (第一页)
    public Button privatePhoto4;                // 隐私相册照片4 (第一页)
    public Button privatePhoto5;                // 隐私相册照片5 (第二页)
    public Button privatePhoto6;                // 隐私相册照片6 (第二页)

    [Header("=== 大图查看面板 ===")]
    public GameObject photoViewerPanel;         // 大图查看面板
    public Image photoViewerImage;              // 大图显示Image
    public Button closeViewerButton;            // 关闭大图按钮

    [Header("=== 隐私相册密码面板 ===")]
    public GameObject privateAlbumPasswordPanel; // 隐私相册密码面板
    public TMP_InputField privatePasswordInputField; // 密码输入框
    public Button privateConfirmButton;         // 确认按钮
    public Button privateCloseButton;           // 关闭按钮

    [Header("=== 生活相册密码面板 ===")]
    public GameObject lifeAlbumPasswordPanel;   // 生活相册密码面板
    public TMP_InputField lifePasswordInputField; // 密码输入框
    public Button lifeConfirmButton;            // 确认按钮
    public Button lifeCloseButton;              // 关闭按钮

    [Header("=== 无权限弹窗 ===")]
    public GameObject noAccessPanel;            // 无权限弹窗
    public Button noAccessConfirmButton;        // 无权限弹窗确认按钮
    public Button noAccessCloseButton;          // 无权限弹窗关闭按钮

    [Header("=== 密码错误弹窗 ===")]
    public GameObject passwordErrorPanel;       // 密码错误弹窗
    public Button errorConfirmButton;           // 错误弹窗确认按钮
    public Button errorCloseButton;             // 错误弹窗关闭按钮

    [Header("=== 上锁照片覆盖层 ===")]
    public GameObject lifePhoto1LockOverlay;    // 生活相册照片1的锁覆盖层

    [Header("=== 相册照片资源 ===")]
    public List<Sprite> workPhotos = new List<Sprite>();      // 工作相册照片列表
    public List<Sprite> lifePhotos = new List<Sprite>();      // 生活相册照片列表
    public List<Sprite> lovePhotos = new List<Sprite>();      // 恋爱相册照片列表
    public List<Sprite> selfiePhotos = new List<Sprite>();    // 自拍相册照片列表
    public List<Sprite> privatePhotos = new List<Sprite>();   // 隐私相册照片列表

    // === 密码设置 ===
    private string lifeAlbumPassword = "2557176";     // 生活相册上锁照片密码
    private string privateAlbumPassword = "2411";      // 隐私相册密码（第二章）

    // === 解锁状态管理 ===
    private bool isLifePhotoUnlocked = false;
    private bool isPrivateAlbumUnlocked = false;
    private bool isChapter2Unlocked = false;

    private int privateAlbumCurrentPage = 0;

    // === 当前状态 ===
    private AlbumType currentAlbumType = AlbumType.Work;

    private Button[] privatePhotoButtons;

    // === 相册类型枚举 ===
    public enum AlbumType
    {
        Work,       // 工作
        Life,       // 生活
        Love,       // 恋爱
        Selfie,     // 自拍
        Private     // 隐私
    }

    void Start()
    {
        InitializeGallery();
    }

    void OnEnable()
    {
        ChapterManager.OnChapter2Started += HandleChapter2Started;
    }

    void OnDisable()
    {
        ChapterManager.OnChapter2Started -= HandleChapter2Started;
    }

    void HandleChapter2Started()
    {
        isChapter2Unlocked = true;
        Debug.Log("[GalleryApp] 检测到第二章开启，开放隐私相册权限");

        if (isPrivateAlbumUnlocked)
        {
            return;
        }

        ShowPrivateAlbumPage(0);
    }

    void InitializeGallery()
    {
        SetupUIEvents();
        LoadPhotoResources();
        LoadUnlockState();

        // 初始隐藏所有面板
        HideAllPanels();

        // 默认显示工作相册
        ShowAlbum(AlbumType.Work);
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    void HideAllPanels()
    {
        // 隐藏相册面板
        if (workAlbumPanel != null) workAlbumPanel.SetActive(false);
        if (lifeAlbumPanel != null) lifeAlbumPanel.SetActive(false);
        if (loveAlbumPanel != null) loveAlbumPanel.SetActive(false);
        if (selfieAlbumPanel != null) selfieAlbumPanel.SetActive(false);
        if (privateAlbumPanel != null) privateAlbumPanel.SetActive(false);

        // 隐藏功能面板
        if (photoViewerPanel != null) photoViewerPanel.SetActive(false);
        if (privateAlbumPasswordPanel != null) privateAlbumPasswordPanel.SetActive(false);
        if (lifeAlbumPasswordPanel != null) lifeAlbumPasswordPanel.SetActive(false);
        if (noAccessPanel != null) noAccessPanel.SetActive(false);
        if (passwordErrorPanel != null) passwordErrorPanel.SetActive(false);

        // 隐藏锁覆盖层
        if (lifePhoto1LockOverlay != null) lifePhoto1LockOverlay.SetActive(false);
    }

    /// <summary>
    /// 绑定所有按钮事件
    /// </summary>
    void SetupUIEvents()
    {
        // 关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseGallery);
        }

        // 相册分类按钮
        if (workAlbumButton != null)
        {
            workAlbumButton.onClick.AddListener(() => ShowAlbum(AlbumType.Work));
        }

        if (lifeAlbumButton != null)
        {
            lifeAlbumButton.onClick.AddListener(() => ShowAlbum(AlbumType.Life));
        }

        if (loveAlbumButton != null)
        {
            loveAlbumButton.onClick.AddListener(() => ShowAlbum(AlbumType.Love));
        }

        if (selfieAlbumButton != null)
        {
            selfieAlbumButton.onClick.AddListener(() => ShowAlbum(AlbumType.Selfie));
        }

        if (privateAlbumButton != null)
        {
            privateAlbumButton.onClick.AddListener(OnPrivateAlbumButtonClicked);
        }

        // 照片按钮事件
        SetupPhotoButtonEvents();

        // 大图查看器关闭按钮
        if (closeViewerButton != null)
        {
            closeViewerButton.onClick.AddListener(ClosePhotoViewer);
        }

        // 密码面板事件
        SetupPasswordPanelEvents();

        // 无权限弹窗事件
        SetupNoAccessPanelEvents();

        // 密码错误弹窗事件
        SetupPasswordErrorPanelEvents();

        // 隐私相册分页事件
        SetupPrivateAlbumPagingEvents();

        CachePrivatePhotoButtons();
    }

    /// <summary>
    /// 设置照片按钮事件
    /// </summary>
    void SetupPhotoButtonEvents()
    {
        // 工作相册照片按钮 - 4张
        if (workPhoto1 != null)
        {
            workPhoto1.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Work, 0));
        }

        if (workPhoto2 != null)
        {
            workPhoto2.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Work, 1));
        }

        if (workPhoto3 != null)
        {
            workPhoto3.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Work, 2));
        }

        if (workPhoto4 != null)
        {
            workPhoto4.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Work, 3));
        }

        // 生活相册照片按钮 - 2张（第1张上锁）
        if (lifePhoto1 != null)
        {
            lifePhoto1.onClick.AddListener(OnLifePhoto1Clicked);
        }

        if (lifePhoto2 != null)
        {
            lifePhoto2.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Life, 1));
        }

        // 恋爱相册照片按钮 - 2张
        if (lovePhoto1 != null)
        {
            lovePhoto1.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Love, 0));
        }

        if (lovePhoto2 != null)
        {
            lovePhoto2.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Love, 1));
        }

        // 自拍相册照片按钮 - 1张
        if (selfiePhoto1 != null)
        {
            selfiePhoto1.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Selfie, 0));
        }

        SetupPrivatePhotoButtonEvents();
    }

    /// <summary>
    /// 设置密码面板事件
    /// </summary>
    void SetupPasswordPanelEvents()
    {
        // 隐私相册密码面板
        if (privateConfirmButton != null)
        {
            privateConfirmButton.onClick.AddListener(OnPrivatePasswordConfirm);
        }

        if (privatePasswordInputField != null)
        {
            privatePasswordInputField.onSubmit.AddListener((password) => OnPrivatePasswordConfirm());
        }

        if (privateCloseButton != null)
        {
            privateCloseButton.onClick.AddListener(ClosePrivatePasswordPanel);
        }

        // 生活相册密码面板
        if (lifeConfirmButton != null)
        {
            lifeConfirmButton.onClick.AddListener(OnLifePasswordConfirm);
        }

        if (lifePasswordInputField != null)
        {
            lifePasswordInputField.onSubmit.AddListener((password) => OnLifePasswordConfirm());
        }

        if (lifeCloseButton != null)
        {
            lifeCloseButton.onClick.AddListener(CloseLifePasswordPanel);
        }
    }

    /// <summary>
    /// 设置无权限弹窗事件
    /// </summary>
    void SetupNoAccessPanelEvents()
    {
        if (noAccessConfirmButton != null)
        {
            noAccessConfirmButton.onClick.AddListener(CloseNoAccessPanel);
        }

        if (noAccessCloseButton != null)
        {
            noAccessCloseButton.onClick.AddListener(CloseNoAccessPanel);
        }
    }

    void CachePrivatePhotoButtons()
    {
        privatePhotoButtons = new Button[]
        {
            privatePhoto1,
            privatePhoto2,
            privatePhoto3,
            privatePhoto4,
            privatePhoto5,
            privatePhoto6
        };
    }

    /// <summary>
    /// 设置密码错误弹窗事件
    /// </summary>
    void SetupPasswordErrorPanelEvents()
    {
        if (errorConfirmButton != null)
        {
            errorConfirmButton.onClick.AddListener(ClosePasswordErrorPanel);
        }

        if (errorCloseButton != null)
        {
            errorCloseButton.onClick.AddListener(ClosePasswordErrorPanel);
        }
    }

    void SetupPrivateAlbumPagingEvents()
    {
        if (privateAlbumPagePrevButton != null)
        {
            privateAlbumPagePrevButton.onClick.AddListener(() => ShowPrivateAlbumPage(privateAlbumCurrentPage - 1));
        }

        if (privateAlbumPageNextButton != null)
        {
            privateAlbumPageNextButton.onClick.AddListener(() => ShowPrivateAlbumPage(privateAlbumCurrentPage + 1));
        }
    }

    void SetupPrivatePhotoButtonEvents()
    {
        if (privatePhotoButtons == null || privatePhotoButtons.Length == 0)
        {
            CachePrivatePhotoButtons();
        }

        if (privatePhotoButtons == null)
        {
            return;
        }

        for (int i = 0; i < privatePhotoButtons.Length; i++)
        {
            Button button = privatePhotoButtons[i];
            int photoIndex = i;
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowPhotoViewer(AlbumType.Private, photoIndex));
        }
    }

    /// <summary>
    /// 加载解锁状态
    /// </summary>
    void LoadUnlockState()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            var dataManager = GameManager.Instance.dataManager;
            var saveData = dataManager.GetSaveData();

            isChapter2Unlocked = saveData != null && saveData.currentChapter >= 2;
            isLifePhotoUnlocked = dataManager.IsPasswordSolved("gallery_life_photo");
            isPrivateAlbumUnlocked = dataManager.IsPasswordSolved("gallery_private_album");

            Debug.Log($"相册解锁状态: 生活相册上锁照片={isLifePhotoUnlocked}, 隐私相册={isPrivateAlbumUnlocked}, 第二章={isChapter2Unlocked}");
        }
        else
        {
            isLifePhotoUnlocked = false;
            isPrivateAlbumUnlocked = false;
            isChapter2Unlocked = false;
        }

        ShowPrivateAlbumPage(0);
    }

    /// <summary>
    /// 保存生活相册解锁状态
    /// </summary>
    void SaveLifePhotoUnlockState()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved("gallery_life_photo");
            GameManager.Instance.dataManager.MarkPasswordSolved("photoalbum_life");
            Debug.Log("生活相册上锁照片已解锁并保存");
        }
    }

    /// <summary>
    /// 保存隐私相册解锁状态
    /// </summary>
    void SavePrivateAlbumUnlockState()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved("gallery_private_album");
            Debug.Log("隐私相册已解锁并保存");
        }
    }

    /// <summary>
    /// 加载照片资源
    /// </summary>
    void LoadPhotoResources()
    {
        // 从Resources加载照片
        LoadPhotoFromResources("相册/相册最佳员工合照", workPhotos);
        LoadPhotoFromResources("相册/相册老板合照", workPhotos);
        LoadPhotoFromResources("相册/相册两人走在前面", lifePhotos);
        LoadPhotoFromResources("相册/相册三人搞怪合照", lifePhotos);
        LoadPhotoFromResources("相册/相册男友合照", lovePhotos);
        LoadPhotoFromResources("相册/我的少女时代", lovePhotos);

        Debug.Log($"照片资源加载完成: 工作({workPhotos.Count}), 生活({lifePhotos.Count}), 恋爱({lovePhotos.Count}), 自拍({selfiePhotos.Count}), 隐私({privatePhotos.Count})");
    }

    /// <summary>
    /// 从Resources加载照片
    /// </summary>
    void LoadPhotoFromResources(string path, List<Sprite> photoList)
    {
        Sprite photo = Resources.Load<Sprite>(path);
        if (photo != null && !photoList.Contains(photo))
        {
            photoList.Add(photo);
        }
    }

    /// <summary>
    /// 显示指定相册
    /// </summary>
    void ShowAlbum(AlbumType albumType)
    {
        currentAlbumType = albumType;
        Debug.Log($"切换到 {albumType} 相册");

        // 隐藏所有相册面板
        if (workAlbumPanel != null) workAlbumPanel.SetActive(false);
        if (lifeAlbumPanel != null) lifeAlbumPanel.SetActive(false);
        if (loveAlbumPanel != null) loveAlbumPanel.SetActive(false);
        if (selfieAlbumPanel != null) selfieAlbumPanel.SetActive(false);
        if (privateAlbumPanel != null) privateAlbumPanel.SetActive(false);

        // 显示对应相册面板
        switch (albumType)
        {
            case AlbumType.Work:
                if (workAlbumPanel != null) workAlbumPanel.SetActive(true);
                break;
            case AlbumType.Life:
                if (lifeAlbumPanel != null) lifeAlbumPanel.SetActive(true);
                UpdateLifePhotoLockStatus();
                break;
            case AlbumType.Love:
                if (loveAlbumPanel != null) loveAlbumPanel.SetActive(true);
                break;
            case AlbumType.Selfie:
                if (selfieAlbumPanel != null) selfieAlbumPanel.SetActive(true);
                break;
            case AlbumType.Private:
                if (isPrivateAlbumUnlocked && isChapter2Unlocked && privateAlbumPanel != null)
                {
                    privateAlbumPanel.SetActive(true);
                    ShowPrivateAlbumPage(privateAlbumCurrentPage);
                }
                break;
        }

        // 更新照片按钮显示
        UpdatePhotoButtons(albumType);
    }

    /// <summary>
    /// 更新生活相册锁状态
    /// </summary>
    void UpdateLifePhotoLockStatus()
    {
        if (lifePhoto1LockOverlay != null)
        {
            lifePhoto1LockOverlay.SetActive(!isLifePhotoUnlocked);
        }
    }

    /// <summary>
    /// 更新照片按钮显示
    /// </summary>
    void UpdatePhotoButtons(AlbumType albumType)
    {
        List<Sprite> photos = GetPhotosByAlbumType(albumType);

        switch (albumType)
        {
            case AlbumType.Work:
                // 工作相册 - 4张照片
                UpdatePhotoButton(workPhoto1, photos.Count > 0 ? photos[0] : null);
                UpdatePhotoButton(workPhoto2, photos.Count > 1 ? photos[1] : null);
                UpdatePhotoButton(workPhoto3, photos.Count > 2 ? photos[2] : null);
                UpdatePhotoButton(workPhoto4, photos.Count > 3 ? photos[3] : null);
                break;
            case AlbumType.Life:
                // 生活相册 - 2张照片（第1张上锁）
                UpdatePhotoButton(lifePhoto1, photos.Count > 0 ? photos[0] : null);
                UpdatePhotoButton(lifePhoto2, photos.Count > 1 ? photos[1] : null);
                break;
            case AlbumType.Love:
                // 恋爱相册 - 2张照片
                UpdatePhotoButton(lovePhoto1, photos.Count > 0 ? photos[0] : null);
                UpdatePhotoButton(lovePhoto2, photos.Count > 1 ? photos[1] : null);
                break;
            case AlbumType.Selfie:
                // 自拍相册 - 1张照片
                UpdatePhotoButton(selfiePhoto1, photos.Count > 0 ? photos[0] : null);
                break;
            case AlbumType.Private:
                UpdatePrivatePhotoSprites();
                break;
        }
    }

    /// <summary>
    /// 更新单个照片按钮
    /// </summary>
    void UpdatePhotoButton(Button button, Sprite sprite)
    {
        if (button == null) return;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = sprite;
            buttonImage.enabled = (sprite != null);
        }

        button.interactable = (sprite != null);
    }

    void UpdatePrivatePhotoSprites()
    {
        if (privatePhotoButtons == null || privatePhotoButtons.Length == 0)
        {
            return;
        }

        List<Sprite> photos = GetPhotosByAlbumType(AlbumType.Private);

        for (int i = 0; i < privatePhotoButtons.Length; i++)
        {
            Button button = privatePhotoButtons[i];
            if (button == null)
            {
                continue;
            }

            Sprite sprite = (photos != null && i < photos.Count) ? photos[i] : null;
            UpdatePhotoButton(button, sprite);

            if (sprite == null)
            {
                button.interactable = false;
            }
        }
    }

    /// <summary>
    /// 根据相册类型获取照片列表
    /// </summary>
    List<Sprite> GetPhotosByAlbumType(AlbumType albumType)
    {
        switch (albumType)
        {
            case AlbumType.Work:
                return workPhotos;
            case AlbumType.Life:
                return lifePhotos;
            case AlbumType.Love:
                return lovePhotos;
            case AlbumType.Selfie:
                return selfiePhotos;
            case AlbumType.Private:
                return privatePhotos;
            default:
                return new List<Sprite>();
        }
    }

    /// <summary>
    /// 点击生活相册照片1（可能上锁）
    /// </summary>
    void OnLifePhoto1Clicked()
    {
        if (!isLifePhotoUnlocked)
        {
            Debug.Log("点击了上锁的生活相册照片，弹出密码输入框");
            ShowLifePasswordPanel();
        }
        else
        {
            ShowPhotoViewer(AlbumType.Life, 0);
        }
    }

    /// <summary>
    /// 显示照片大图查看器
    /// </summary>
    void ShowPhotoViewer(AlbumType albumType, int photoIndex)
    {
        List<Sprite> photos = GetPhotosByAlbumType(albumType);

        if (photoIndex < 0 || photoIndex >= photos.Count || photos[photoIndex] == null)
        {
            Debug.LogWarning($"照片索引 {photoIndex} 在 {albumType} 相册中无效!");
            return;
        }

        if (photoViewerPanel == null || photoViewerImage == null)
        {
            Debug.LogWarning("PhotoViewerPanel或PhotoViewerImage未设置!");
            return;
        }

        Debug.Log($"显示大图: {albumType} 相册照片 {photoIndex + 1}");

        // 设置大图
        photoViewerImage.sprite = photos[photoIndex];

        // 显示大图面板
        photoViewerPanel.SetActive(true);
    }

    /// <summary>
    /// 关闭照片大图查看器
    /// </summary>
    void ClosePhotoViewer()
    {
        if (photoViewerPanel != null)
        {
            photoViewerPanel.SetActive(false);
            Debug.Log("关闭大图查看器");
        }
    }

    /// <summary>
    /// 点击隐私相册按钮
    /// </summary>
    void OnPrivateAlbumButtonClicked()
    {
        Debug.Log("点击隐私相册");

        if (!isChapter2Unlocked)
        {
            Debug.Log("尚未进入第二章，隐私相册暂无权限");
            ShowNoAccessPanel();
            return;
        }

        if (!isPrivateAlbumUnlocked)
        {
            Debug.Log("隐私相册未解锁，显示密码面板");
            ShowPrivatePasswordPanel();
        }
        else
        {
            ShowAlbum(AlbumType.Private);
        }
    }

    void ShowPrivateAlbumPage(int pageIndex)
    {
        List<GameObject> pages = new List<GameObject>();
        if (privateAlbumPage1 != null) pages.Add(privateAlbumPage1);
        if (privateAlbumPage2 != null) pages.Add(privateAlbumPage2);

        if (pages.Count == 0)
        {
            privateAlbumCurrentPage = 0;
            return;
        }

        if (privateAlbumPage1 != null) privateAlbumPage1.SetActive(false);
        if (privateAlbumPage2 != null) privateAlbumPage2.SetActive(false);

        int clampedIndex = Mathf.Clamp(pageIndex, 0, pages.Count - 1);
        privateAlbumCurrentPage = clampedIndex;

        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == clampedIndex);
            }
        }

        if (privateAlbumPageIndicator != null)
        {
            privateAlbumPageIndicator.text = $"{clampedIndex + 1} / {pages.Count}";
        }

        if (privateAlbumPagePrevButton != null)
        {
            privateAlbumPagePrevButton.interactable = clampedIndex > 0;
        }

        if (privateAlbumPageNextButton != null)
        {
            privateAlbumPageNextButton.interactable = clampedIndex < pages.Count - 1;
        }

        UpdatePrivatePhotoSprites();
    }

    public void OpenPrivateAlbumFirstPage()
    {
        ShowPrivateAlbumPage(0);
    }

    public void OpenPrivateAlbumSecondPage()
    {
        ShowPrivateAlbumPage(1);
    }

    /// <summary>
    /// 显示生活相册密码面板
    /// </summary>
    void ShowLifePasswordPanel()
    {
        if (lifeAlbumPasswordPanel != null)
        {
            lifeAlbumPasswordPanel.SetActive(true);

            // 清空输入框
            if (lifePasswordInputField != null)
            {
                lifePasswordInputField.text = "";
                lifePasswordInputField.ActivateInputField();
            }
        }
    }

    /// <summary>
    /// 显示隐私相册密码面板
    /// </summary>
    void ShowPrivatePasswordPanel()
    {
        if (privateAlbumPasswordPanel != null)
        {
            privateAlbumPasswordPanel.SetActive(true);

            // 清空输入框
            if (privatePasswordInputField != null)
            {
                privatePasswordInputField.text = "";
                privatePasswordInputField.ActivateInputField();
            }
        }
    }

    /// <summary>
    /// 确认生活相册密码
    /// </summary>
    void OnLifePasswordConfirm()
    {
        if (lifePasswordInputField == null) return;

        string inputPassword = lifePasswordInputField.text.Trim();
        Debug.Log($"输入生活相册密码: {inputPassword}");

        if (inputPassword == lifeAlbumPassword)
        {
            // 密码正确，解锁照片
            Debug.Log("生活相册密码正确，照片已解锁!");
            isLifePhotoUnlocked = true;
            SaveLifePhotoUnlockState();
            CloseLifePasswordPanel();
            UpdateLifePhotoLockStatus();

            // 显示照片大图
            ShowPhotoViewer(AlbumType.Life, 0);
        }
        else
        {
            // 密码错误
            Debug.LogWarning("生活相册密码错误!");
            lifePasswordInputField.text = "";
            ShowPasswordErrorPanel();
        }
    }

    /// <summary>
    /// 确认隐私相册密码
    /// </summary>
    void OnPrivatePasswordConfirm()
    {
        if (privatePasswordInputField == null) return;

        string inputPassword = privatePasswordInputField.text.Trim();
        Debug.Log($"输入隐私相册密码: {inputPassword}");

        if (inputPassword == privateAlbumPassword)
        {
            // 密码正确，解锁隐私相册
            Debug.Log("隐私相册密码正确，相册已解锁!");
            isPrivateAlbumUnlocked = true;
            SavePrivateAlbumUnlockState();
            ClosePrivatePasswordPanel();
            privateAlbumCurrentPage = 0;
            ShowPrivateAlbumPage(privateAlbumCurrentPage);
            ShowAlbum(AlbumType.Private);
        }
        else
        {
            // 密码错误
            Debug.LogWarning("隐私相册密码错误!");
            privatePasswordInputField.text = "";
            ShowPasswordErrorPanel();
        }
    }

    /// <summary>
    /// 关闭生活相册密码面板
    /// </summary>
    void CloseLifePasswordPanel()
    {
        if (lifeAlbumPasswordPanel != null)
        {
            lifeAlbumPasswordPanel.SetActive(false);
            Debug.Log("关闭生活相册密码面板");
        }
    }

    /// <summary>
    /// 关闭隐私相册密码面板
    /// </summary>
    void ClosePrivatePasswordPanel()
    {
        if (privateAlbumPasswordPanel != null)
        {
            privateAlbumPasswordPanel.SetActive(false);
            Debug.Log("关闭隐私相册密码面板");
        }
    }

    /// <summary>
    /// 显示无权限弹窗
    /// </summary>
    void ShowNoAccessPanel()
    {
        if (noAccessPanel != null)
        {
            noAccessPanel.SetActive(true);
            Debug.Log("显示无权限弹窗");
        }
    }

    /// <summary>
    /// 关闭无权限弹窗
    /// </summary>
    void CloseNoAccessPanel()
    {
        if (noAccessPanel != null)
        {
            noAccessPanel.SetActive(false);
            Debug.Log("关闭无权限弹窗");
        }
    }

    /// <summary>
    /// 显示密码错误弹窗
    /// </summary>
    void ShowPasswordErrorPanel()
    {
        if (passwordErrorPanel != null)
        {
            passwordErrorPanel.SetActive(true);
            Debug.Log("显示密码错误弹窗");
        }
    }

    /// <summary>
    /// 关闭密码错误弹窗
    /// </summary>
    void ClosePasswordErrorPanel()
    {
        if (passwordErrorPanel != null)
        {
            passwordErrorPanel.SetActive(false);
            Debug.Log("关闭密码错误弹窗");
        }
    }

    /// <summary>
    /// 关闭相册窗口
    /// </summary>
    void CloseGallery()
    {
        Debug.Log("关闭相册窗口");
        gameObject.SetActive(false);

        // 通知AppManager
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    /// <summary>
    /// 检查生活相册上锁照片是否已解锁
    /// </summary>
    public bool IsLifePhotoUnlocked()
    {
        return isLifePhotoUnlocked;
    }

    /// <summary>
    /// 检查隐私相册是否已解锁
    /// </summary>
    public bool IsPrivateAlbumUnlocked()
    {
        return isPrivateAlbumUnlocked;
    }

    /// <summary>
    /// 设置生活相册上锁照片解锁状态（可通过剧情或其他方式调用）
    /// </summary>
    public void SetLifePhotoUnlocked(bool unlocked)
    {
        isLifePhotoUnlocked = unlocked;
        if (unlocked)
        {
            SaveLifePhotoUnlockState();
            UpdateLifePhotoLockStatus();
        }
        Debug.Log($"生活相册上锁照片解锁状态设置为: {unlocked}");
    }

    /// <summary>
    /// 设置隐私相册解锁状态（可通过剧情或其他方式调用）
    /// </summary>
    public void SetPrivateAlbumUnlocked(bool unlocked)
    {
        isPrivateAlbumUnlocked = unlocked;
        if (unlocked)
        {
            SavePrivateAlbumUnlockState();
        }
        Debug.Log($"隐私相册解锁状态设置为: {unlocked}");
    }

    /// <summary>
    /// 设置相册密码（可通过剧情或其他方式调用）
    /// </summary>
    public void SetLifeAlbumPassword(string newPassword)
    {
        lifeAlbumPassword = newPassword;
        Debug.Log($"生活相册密码已更改为: {newPassword}");
    }

    /// <summary>
    /// 设置隐私相册密码（可通过剧情或其他方式调用）
    /// </summary>
    public void SetPrivateAlbumPassword(string newPassword)
    {
        privateAlbumPassword = newPassword;
        Debug.Log($"隐私相册密码已更改为: {newPassword}");
    }
}
