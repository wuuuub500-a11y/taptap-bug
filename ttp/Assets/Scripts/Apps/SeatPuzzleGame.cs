using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// 座位排列小游戏 - 第二章博客日志解密游戏
/// 需要将5个人的头像拖动到正确的座位上
/// </summary>
public class SeatPuzzleGame : MonoBehaviour
{
    [Header("=== UI组件 ===")]
    public Button closeButton;              // 右上角关闭按钮
    public Button confirmButton;            // 确认按钮
    public TextMeshProUGUI titleText;       // 标题文字 "请为他们排出满意的座位"

    [Header("=== 座位网格 (3x3) ===")]
    public SeatSlot[] seatSlots;            // 9个座位槽

    [Header("=== 可拖动的头像 ===")]
    public DraggableAvatar[] draggableAvatars;  // 5个可拖动的头像

    [Header("=== 正确答案配置 ===")]
    [Tooltip("每个头像对应的正确座位索引 (0-8)")]
    public int[] correctSeatIndices;        // 每个Avatar的正确座位索引

    [Header("=== 日志确认弹窗 ===")]
    public GameObject[] diaryEntryPopups;          // 进入日志前的确认弹窗（按顺序）
    public Button[] diaryConfirmButtons;           // 弹窗中的确认按钮
    public Button[] diaryThinkAgainButtons;        // 弹窗中的“再想想”按钮
    public Button[] diaryCloseButtons;             // 弹窗中的关闭(X)按钮

    [Header("=== 颜色配置 ===")]
    public Color incorrectAvatarColor = new Color(1f, 0.35f, 0.35f); // 回答错误时的颜色

    [Header("=== 悬停提示 ===")]
    public GameObject avatarTooltipPanel;   // Tooltip外层面板
    public TextMeshProUGUI avatarTooltipText; // Tooltip文字
    public RectTransform avatarTooltipRect; // Tooltip RectTransform（可选）
    public Vector2 tooltipDefaultOffset = new Vector2(0f, 80f); // 默认Y向偏移

    // 状态管理
    private Dictionary<int, DraggableAvatar> currentPlacements = new Dictionary<int, DraggableAvatar>();
    private bool isInitialized = false;
    private int currentDiaryPopupIndex = -1;

    void Awake()
    {
        HideAvatarTooltip();
        HideAllDiaryPopups();
        ResetAllAvatarColors();
    }

    void Start()
    {
        Debug.Log("[SeatPuzzle] SeatPuzzleGame Start() 被调用");
        SetupUIEvents();
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void InitializeGame()
    {
        if (isInitialized)
        {
            ResetGame();
            HideAvatarTooltip();
            return;
        }

        // 初始化座位槽
        for (int i = 0; i < seatSlots.Length; i++)
        {
            if (seatSlots[i] != null)
            {
                seatSlots[i].slotIndex = i;
                seatSlots[i].puzzleGame = this;
            }
        }

        // 初始化可拖动头像
        Debug.Log($"[SeatPuzzle] 开始初始化 {draggableAvatars.Length} 个可拖动头像");
        for (int i = 0; i < draggableAvatars.Length; i++)
        {
            if (draggableAvatars[i] != null)
            {
                Debug.Log($"[SeatPuzzle] 检查头像 {i}: {draggableAvatars[i].name}");

                // 检查必需的组件
                var dragAvatar = draggableAvatars[i];
                var rectTransform = dragAvatar.GetComponent<RectTransform>();
                var image = dragAvatar.GetComponent<Image>();
                var button = dragAvatar.GetComponent<UnityEngine.UI.Button>();
                var collider = dragAvatar.GetComponent<UnityEngine.BoxCollider2D>();
                var hasCollider = (button != null) || (collider != null);

                Debug.Log($"[SeatPuzzle] 头像 {i} 组件检查:");
                Debug.Log($"  - DraggableAvatar脚本: {dragAvatar != null}");
                Debug.Log($"  - RectTransform: {rectTransform != null}");
                Debug.Log($"  - Image: {image != null}");
                Debug.Log($"  - Button组件: {button != null}");
                Debug.Log($"  - BoxCollider2D组件: {collider != null}");
                Debug.Log($"  - 有交互组件: {hasCollider}");

                if (rectTransform == null)
                {
                    Debug.LogError($"[SeatPuzzle] ✗ 头像 {i} 缺少RectTransform组件！");
                }
                if (image == null)
                {
                    Debug.LogError($"[SeatPuzzle] ✗ 头像 {i} 缺少Image组件！");
                }
                if (!hasCollider)
                {
                    Debug.LogError($"[SeatPuzzle] ✗ 头像 {i} 缺少Button或BoxCollider2D组件！");
                }

                draggableAvatars[i].avatarIndex = i;
                draggableAvatars[i].puzzleGame = this;
                draggableAvatars[i].ResetPosition();
                Debug.Log($"[SeatPuzzle] ✅ 头像 {i} 初始化完成，名称: {draggableAvatars[i].name}");
            }
            else
            {
                Debug.LogError($"[SeatPuzzle] ✗ 头像 {i} 为空！");
            }
        }

        currentPlacements.Clear();
        isInitialized = true;

        Debug.Log("[SeatPuzzle] 游戏初始化完成");
    }

    public void ShowAvatarTooltip(DraggableAvatar avatar, PointerEventData eventData)
    {
        if (avatarTooltipPanel == null || avatarTooltipText == null)
        {
            return;
        }

        avatarTooltipPanel.SetActive(true);
        avatarTooltipText.text = string.IsNullOrEmpty(avatar?.hoverDialogue) ? string.Empty : avatar.hoverDialogue;

        if (avatarTooltipRect != null)
        {
            Vector2 offset = tooltipDefaultOffset;
            if (avatar != null)
            {
                offset += avatar.tooltipOffset;
            }

            RectTransform parentRect = avatarTooltipRect.parent as RectTransform;
            Camera eventCamera = eventData?.enterEventCamera ?? eventData?.pressEventCamera;

            if (parentRect != null && eventData != null)
            {
                Vector2 localPos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventCamera, out localPos))
                {
                    avatarTooltipRect.anchoredPosition = localPos + offset;
                    return;
                }
            }

            if (avatar != null)
            {
                RectTransform avatarRect = avatar.GetComponent<RectTransform>();
                if (avatarRect != null)
                {
                    Vector2 avatarCenter = avatarRect.anchoredPosition;
                    avatarTooltipRect.anchoredPosition = avatarCenter + offset;
                    return;
                }
            }

            avatarTooltipRect.anchoredPosition = offset;
        }
    }

    public void HideAvatarTooltip()
    {
        if (avatarTooltipPanel != null)
        {
            avatarTooltipPanel.SetActive(false);
        }

        if (avatarTooltipText != null)
        {
            avatarTooltipText.text = string.Empty;
        }
    }

    void SetupDiaryPopupEvents()
    {
        if (diaryEntryPopups == null || diaryEntryPopups.Length == 0)
        {
            return;
        }

        for (int i = 0; i < diaryEntryPopups.Length; i++)
        {
            if (diaryEntryPopups[i] != null)
            {
                diaryEntryPopups[i].SetActive(false);
            }

            if (diaryConfirmButtons != null && i < diaryConfirmButtons.Length && diaryConfirmButtons[i] != null)
            {
                int index = i;
                diaryConfirmButtons[i].onClick.RemoveAllListeners();
                diaryConfirmButtons[i].onClick.AddListener(() => OnDiaryConfirmClicked(index));
            }
            else
            {
                Debug.LogWarning($"[SeatPuzzle] 弹窗 {i} 缺少确认按钮绑定");
            }

            if (diaryThinkAgainButtons != null && i < diaryThinkAgainButtons.Length && diaryThinkAgainButtons[i] != null)
            {
                int index = i;
                diaryThinkAgainButtons[i].onClick.RemoveAllListeners();
                diaryThinkAgainButtons[i].onClick.AddListener(() => OnDiaryCancelClicked(index));
            }
            else
            {
                Debug.LogWarning($"[SeatPuzzle] 弹窗 {i} 缺少再想想按钮绑定");
            }

            if (diaryCloseButtons != null && i < diaryCloseButtons.Length && diaryCloseButtons[i] != null)
            {
                int index = i;
                diaryCloseButtons[i].onClick.RemoveAllListeners();
                diaryCloseButtons[i].onClick.AddListener(() => OnDiaryCancelClicked(index));
            }
            else
            {
                Debug.LogWarning($"[SeatPuzzle] 弹窗 {i} 缺少关闭按钮绑定");
            }
        }

        currentDiaryPopupIndex = -1;
    }

    void ShowDiaryEntryPopup(int index)
    {
        if (!HasDiaryPopupsConfigured())
        {
            return;
        }

        if (index < 0 || index >= diaryEntryPopups.Length)
        {
            Debug.LogWarning($"[SeatPuzzle] 弹窗索引 {index} 超出范围");
            return;
        }

        HideAllDiaryPopups();

        if (diaryEntryPopups[index] != null)
        {
            diaryEntryPopups[index].SetActive(true);
            currentDiaryPopupIndex = index;
            Debug.Log($"[SeatPuzzle] 显示第 {index + 1} 个日志确认弹窗");
        }
    }

    void HideDiaryEntryPopup(int index)
    {
        if (diaryEntryPopups == null || index < 0 || index >= diaryEntryPopups.Length)
        {
            return;
        }

        if (diaryEntryPopups[index] != null)
        {
            diaryEntryPopups[index].SetActive(false);
        }
    }

    void HideAllDiaryPopups()
    {
        if (diaryEntryPopups == null || diaryEntryPopups.Length == 0)
        {
            return;
        }

        for (int i = 0; i < diaryEntryPopups.Length; i++)
        {
            if (diaryEntryPopups[i] != null)
            {
                diaryEntryPopups[i].SetActive(false);
            }
        }

        currentDiaryPopupIndex = -1;
    }

    /// <summary>
    /// 设置UI事件
    /// </summary>
    void SetupUIEvents()
    {
        Debug.Log("[SeatPuzzle] 开始设置UI事件");

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            Debug.Log("[SeatPuzzle] ✅ 关闭按钮事件已绑定");
        }
        else
        {
            Debug.LogError("[SeatPuzzle] ✗ 关闭按钮未配置！");
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            Debug.Log("[SeatPuzzle] ✅ 确认按钮事件已绑定");
        }
        else
        {
            Debug.LogError("[SeatPuzzle] ✗ 确认按钮未配置！");
        }

        SetupDiaryPopupEvents();

        // 检查EventSystem
        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            Debug.Log("[SeatPuzzle] ✅ EventSystem已找到");
        }
        else
        {
            Debug.LogError("[SeatPuzzle] ✗ 场景中没有EventSystem！");
        }

        // 检查GraphicRaycaster
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null)
            {
                Debug.Log("[SeatPuzzle] ✅ GraphicRaycaster已找到");
            }
            else
            {
                Debug.LogError("[SeatPuzzle] ✗ Canvas上没有GraphicRaycaster组件！");
            }

            Debug.Log($"[SeatPuzzle] Canvas设置: renderMode={canvas.renderMode}, sortingOrder={canvas.sortingOrder}");

            // 检查Canvas是否覆盖头像
            var canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                Debug.Log($"[SeatPuzzle] Canvas Rect: width={canvasRect.rect.width}, height={canvasRect.rect.height}");
            }
        }
    }

    /// <summary>
    /// 头像放置到座位
    /// </summary>
    public void PlaceAvatar(DraggableAvatar avatar, int seatIndex)
    {
        // 移除该座位上之前的头像
        if (currentPlacements.ContainsKey(seatIndex))
        {
            DraggableAvatar previousAvatar = currentPlacements[seatIndex];
            if (previousAvatar != null && previousAvatar != avatar)
            {
                previousAvatar.ResetPosition();
            }
        }

        // 如果avatar之前在其他座位上，移除
        foreach (var kvp in currentPlacements)
        {
            if (kvp.Value == avatar && kvp.Key != seatIndex)
            {
                currentPlacements.Remove(kvp.Key);
                break;
            }
        }

        // 放置头像到新座位
        currentPlacements[seatIndex] = avatar;
        avatar.currentSeatIndex = seatIndex;

        Debug.Log($"[SeatPuzzle] 头像 {avatar.avatarIndex} 放置到座位 {seatIndex}");
    }

    /// <summary>
    /// 移除座位上的头像
    /// </summary>
    public void RemoveAvatarFromSeat(int seatIndex)
    {
        if (currentPlacements.ContainsKey(seatIndex))
        {
            currentPlacements.Remove(seatIndex);
        }
    }

    /// <summary>
    /// 获取座位上的头像
    /// </summary>
    public DraggableAvatar GetAvatarAtSeat(int seatIndex)
    {
        if (currentPlacements.TryGetValue(seatIndex, out DraggableAvatar avatar))
        {
            return avatar;
        }
        return null;
    }

    /// <summary>
    /// 检查座位是否被占用
    /// </summary>
    public bool IsSeatOccupied(int seatIndex)
    {
        return currentPlacements.ContainsKey(seatIndex);
    }

    /// <summary>
    /// 确认按钮点击
    /// </summary>
    void OnConfirmButtonClicked()
    {
        ResetAllAvatarColors();
        bool allCorrect = CheckSolution();

        if (allCorrect)
        {
            OnPuzzleSolved();
        }
        else
        {
            HighlightIncorrectAvatars();
            OnPuzzleFailed();
        }
    }

    void OnDiaryConfirmClicked(int index)
    {
        if (!HasDiaryPopupsConfigured())
        {
            CompletePuzzleAndOpenDiary();
            return;
        }

        HideDiaryEntryPopup(index);

        if (index < diaryEntryPopups.Length - 1)
        {
            ShowDiaryEntryPopup(index + 1);
        }
        else
        {
            CompletePuzzleAndOpenDiary();
        }
    }

    void OnDiaryCancelClicked(int index)
    {
        if (!HasDiaryPopupsConfigured())
        {
            return;
        }

        HideDiaryEntryPopup(index);
        currentDiaryPopupIndex = -1;
        Debug.Log($"[SeatPuzzle] 弹窗 {index + 1} 被取消，保持在小游戏界面");
    }

    /// <summary>
    /// 检查解答是否正确
    /// </summary>
    bool CheckSolution()
    {
        // 检查所有头像是否都已放置
        if (currentPlacements.Count != draggableAvatars.Length)
        {
            Debug.Log($"[SeatPuzzle] 未放置所有头像: {currentPlacements.Count}/{draggableAvatars.Length}");
            return false;
        }

        // 检查每个头像是否在正确的位置
        for (int i = 0; i < draggableAvatars.Length; i++)
        {
            DraggableAvatar avatar = draggableAvatars[i];
            if (avatar == null) continue;

            int correctSeatIndex = correctSeatIndices[i];
            
            if (avatar.currentSeatIndex != correctSeatIndex)
            {
                Debug.Log($"[SeatPuzzle] 头像 {i} 位置错误: 当前={avatar.currentSeatIndex}, 正确={correctSeatIndex}");
                return false;
            }
        }

        Debug.Log("[SeatPuzzle] 所有位置正确！");
        return true;
    }

    bool HasDiaryPopupsConfigured()
    {
        return diaryEntryPopups != null && diaryEntryPopups.Length > 0;
    }

    /// <summary>
    /// 拼图完成
    /// </summary>
    void OnPuzzleSolved()
    {
        Debug.Log("[SeatPuzzle] 拼图完成！解锁博客日志");

        // 播放成功效果
        PlaySuccessEffect();
        ResetAllAvatarColors();

        if (HasDiaryPopupsConfigured())
        {
            ShowDiaryEntryPopup(0);
        }
        else
        {
            CompletePuzzleAndOpenDiary();
        }
    }

    /// <summary>
    /// 拼图失败
    /// </summary>
    void OnPuzzleFailed()
    {
        Debug.Log("[SeatPuzzle] 拼图错误，请重新排列");

        // 显示错误提示
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowError("排列有误，请重新尝试");
            GameManager.Instance.uiManager.ShakeScreen(3f, 0.3f);
        }

        // 震动效果
        PlayFailEffect();
    }

    /// <summary>
    /// 播放成功效果
    /// </summary>
    void PlaySuccessEffect()
    {
        // TODO: 实现Kuro和玩家头像变红色
        // TODO: 密码界面变红色
        
        // 显示成功消息
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowSuccess("密码正确！日志已解锁");
            GameManager.Instance.uiManager.ShakeScreen(5f, 0.4f);
        }

        Debug.Log("[SeatPuzzle] 播放成功效果");
    }

    /// <summary>
    /// 播放失败效果
    /// </summary>
    void PlayFailEffect()
    {
        // 轻微震动
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShakeScreen(2f, 0.2f);
        }
    }

    void ResetAllAvatarColors()
    {
        if (draggableAvatars == null)
        {
            return;
        }

        foreach (var avatar in draggableAvatars)
        {
            avatar?.ResetAvatarColor();
        }
    }

    void HighlightIncorrectAvatars()
    {
        if (draggableAvatars == null || correctSeatIndices == null)
        {
            return;
        }

        for (int i = 0; i < draggableAvatars.Length; i++)
        {
            var avatar = draggableAvatars[i];
            if (avatar == null)
            {
                continue;
            }

            int expectedIndex = (i < correctSeatIndices.Length) ? correctSeatIndices[i] : -1;
            bool isCorrect = avatar.currentSeatIndex >= 0 && avatar.currentSeatIndex == expectedIndex;

            if (!isCorrect)
            {
                avatar.SetAvatarColor(incorrectAvatarColor);
            }
        }
    }

    /// <summary>
    /// 关闭拼图并显示日志
    /// </summary>
    void ClosePuzzleAndShowDiary()
    {
        gameObject.SetActive(false);

        // 通知BlogApp显示日志
        var blogApp = GetComponentInParent<BlogApp>();
        if (blogApp != null)
        {
            // 这里可以调用显示日志的方法
            Debug.Log("[SeatPuzzle] 通知BlogApp显示日志");
        }
    }

    void CompletePuzzleAndOpenDiary()
    {
        HideAllDiaryPopups();
        HideAvatarTooltip();

        var blogApp = GetComponentInParent<BlogApp>();
        if (blogApp != null)
        {
            blogApp.MarkSeatPuzzleSolved();
            blogApp.OpenDiaryAfterPuzzle();
            Debug.Log("[SeatPuzzle] 玩家确认进入日志，已解锁并打开日志面板");
        }
        else
        {
            Debug.LogWarning("[SeatPuzzle] 未找到BlogApp，无法打开日志面板");
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    void OnCloseButtonClicked()
    {
        Debug.Log("[SeatPuzzle] 🔥🔥🔥 关闭按钮被点击了！！！");

        // 直接隐藏游戏面板
        HideAllDiaryPopups();
        HideAvatarTooltip();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 检查拼图是否已解决
    /// </summary>
    bool IsPuzzleSolved()
    {
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            return GameManager.Instance.dataManager.IsPasswordSolved("blog_seat_puzzle");
        }
        return false;
    }

    /// <summary>
    /// 重置游戏
    /// </summary>
    public void ResetGame()
    {
        // 重置所有头像
        foreach (var avatar in draggableAvatars)
        {
            if (avatar != null)
            {
                avatar.ResetPosition();
            }
        }

        currentPlacements.Clear();
        HideAvatarTooltip();
        HideAllDiaryPopups();
        ResetAllAvatarColors();
        Debug.Log("[SeatPuzzle] 游戏已重置");
    }
}
