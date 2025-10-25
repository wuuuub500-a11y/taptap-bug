using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 简化版博客应用 - 只实现基础功能
/// </summary>
public class SimpleBlogApp : MonoBehaviour
{
    [Header("主要按钮")]
    public Button closeButton;          // 关闭博客窗口
    public Button diaryButton;          // 日记按钮（弹出无访问权限）
    public Button messageButton;        // 消息按钮（只有点击交互）
    public Button collectionButton;     // 收藏按钮（只有点击交互）

    [Header("无访问权限弹窗")]
    public GameObject noAccessPanel;    // 弹窗面板
    public Button confirmButton;        // 确认按钮
    public Button closePopupButton;     // 弹窗关闭按钮

    [Header("第二章 - 座位拖拽游戏")]
    public GameObject seatPuzzlePanel;     // 座位游戏面板
    public Button startPuzzleButton;       // 开始游戏按钮
    public Button closePuzzleButton;       // 关闭游戏按钮
    public Transform seatContainer;        // 座位容器
    public Transform characterContainer;   // 角色容器
    public Button puzzleSubmitButton;      // 提交答案按钮
    public Button puzzleResetButton;       // 重置按钮
    public TextMeshProUGUI puzzleResultText;      // 结果提示文字

    [Header("座位配置")]
    public GameObject[] seatSlots;         // 座位槽位数组
    public GameObject[] characterPieces;   // 角色拖拽块数组
    public Vector2[] correctPositions;     // 正确位置数组

    [Header("滚动区域")]
    public ScrollRect diaryScrollRect;  // 博客内容滚动区

    [Header("第二章 - 解锁后的博客日志")]
    public GameObject unlockedDiaryPanel;    // 解锁后的博客日志面板
    public Button diaryBackButton;           // 返回按钮
    public ScrollRect unlockedDiaryScrollRect; // 解锁内容滚动区域

    // 游戏状态
    private bool isChapter2 = false;
    private bool isPuzzleCompleted = false;
    private Dictionary<string, Vector2> initialPositions = new Dictionary<string, Vector2>();

    void Start()
    {
        Debug.Log("[SimpleBlogApp] 开始初始化博客应用");

        // 检查当前章节状态
        var chapterManager = FindObjectOfType<ChapterManager>();
        if (chapterManager != null && chapterManager.IsChapter2())
        {
            isChapter2 = true;
            Debug.Log("[SimpleBlogApp] 检测到第二章状态");
        }

        // 检查游戏完成状态
        if (GameManager.Instance?.dataManager != null)
        {
            isPuzzleCompleted = GameManager.Instance.dataManager.IsPasswordSolved("seat_puzzle");
            Debug.Log($"[SimpleBlogApp] 座位游戏完成状态: {isPuzzleCompleted}");
        }

        SetupButtons();
        HideNoAccessPanel();

        // 监听章节切换事件
        ChapterManager.OnChapter2Started += OnChapter2Started;

        Debug.Log("[SimpleBlogApp] 博客应用初始化完成");
    }

    void OnDestroy()
    {
        // 清理事件监听
        ChapterManager.OnChapter2Started -= OnChapter2Started;
    }

    /// <summary>
    /// 设置所有按钮的点击事件
    /// </summary>
    void SetupButtons()
    {
        // 主窗口关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBlogWindow);
        }

        // 日记按钮 - 显示无访问权限弹窗
        if (diaryButton != null)
        {
            diaryButton.onClick.AddListener(ShowNoAccessPanel);
        }

        // 消息按钮 - 只有点击交互
        if (messageButton != null)
        {
            messageButton.onClick.AddListener(OnMessageButtonClick);
        }

        // 收藏按钮 - 只有点击交互
        if (collectionButton != null)
        {
            collectionButton.onClick.AddListener(OnCollectionButtonClick);
        }

        // 无访问权限弹窗 - 确认按钮
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HideNoAccessPanel);
        }

        // 无访问权限弹窗 - 关闭按钮
        if (closePopupButton != null)
        {
            closePopupButton.onClick.AddListener(HideNoAccessPanel);
        }

        // 座位游戏按钮
        if (startPuzzleButton != null)
        {
            startPuzzleButton.onClick.AddListener(ShowSeatPuzzle);
        }

        if (closePuzzleButton != null)
        {
            closePuzzleButton.onClick.AddListener(HideSeatPuzzle);
        }

        if (puzzleSubmitButton != null)
        {
            puzzleSubmitButton.onClick.AddListener(CheckPuzzleSolution);
        }

        if (puzzleResetButton != null)
        {
            puzzleResetButton.onClick.AddListener(ResetPuzzle);
        }

        // 解锁后博客日志按钮
        if (diaryBackButton != null)
        {
            diaryBackButton.onClick.AddListener(HideUnlockedDiary);
        }
    }

    /// <summary>
    /// 关闭博客窗口
    /// </summary>
    void CloseBlogWindow()
    {
        Debug.Log("关闭博客窗口");
        gameObject.SetActive(false);

        // 通知AppManager
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    /// <summary>
    /// 显示无访问权限弹窗
    /// </summary>
    void ShowNoAccessPanel()
    {
        Debug.Log("显示无访问权限弹窗");

        // 第二章：显示座位游戏
        if (isChapter2 && !isPuzzleCompleted)
        {
            ShowSeatPuzzle();
        }
        else if (isChapter2 && isPuzzleCompleted)
        {
            // 第二章且已完成游戏：解锁日记内容
            UnlockDiaryContent();
        }
        else
        {
            // 第一章：显示普通无权限弹窗
            if (noAccessPanel != null)
            {
                noAccessPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 隐藏无访问权限弹窗
    /// </summary>
    void HideNoAccessPanel()
    {
        Debug.Log("隐藏无访问权限弹窗");

        if (noAccessPanel != null)
        {
            noAccessPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 消息按钮点击（暂时只打印日志）
    /// </summary>
    void OnMessageButtonClick()
    {
        Debug.Log("点击了消息按钮");
    }

    /// <summary>
    /// 收藏按钮点击（暂时只打印日志）
    /// </summary>
    void OnCollectionButtonClick()
    {
        Debug.Log("点击了收藏按钮");
    }

    // ==================== 第二章功能 ====================

    /// <summary>
    /// 章节切换事件处理
    /// </summary>
    void OnChapter2Started()
    {
        isChapter2 = true;
        Debug.Log("[SimpleBlogApp] 检测到第二章，启用座位拖拽游戏");

        // 检查游戏是否已完成
        if (GameManager.Instance?.dataManager != null)
        {
            isPuzzleCompleted = GameManager.Instance.dataManager.IsPasswordSolved("seat_puzzle");
        }
    }

    /// <summary>
    /// 显示座位拖拽游戏
    /// </summary>
    void ShowSeatPuzzle()
    {
        Debug.Log("[SimpleBlogApp] 尝试显示座位拖拽游戏");

        // 诊断日志：检查所有必需组件
        Debug.Log($"[SimpleBlogApp] seatPuzzlePanel: {(seatPuzzlePanel != null ? "✓" : "✗ 未配置")}");
        Debug.Log($"[SimpleBlogApp] seatContainer: {(seatContainer != null ? "✓" : "✗ 未配置")}");
        Debug.Log($"[SimpleBlogApp] characterContainer: {(characterContainer != null ? "✓" : "✗ 未配置")}");
        Debug.Log($"[SimpleBlogApp] seatSlots: {(seatSlots != null ? $"{seatSlots.Length}个" : "✗ 未配置")}");
        Debug.Log($"[SimpleBlogApp] characterPieces: {(characterPieces != null ? $"{characterPieces.Length}个" : "✗ 未配置")}");
        Debug.Log($"[SimpleBlogApp] correctPositions: {(correctPositions != null ? $"{correctPositions.Length}个" : "✗ 未配置")}");

        if (seatPuzzlePanel != null)
        {
            seatPuzzlePanel.SetActive(true);
            InitializePuzzle();
            Debug.Log("[SimpleBlogApp] ✓ 座位拖拽游戏已显示");
        }
        else
        {
            Debug.LogError("[SimpleBlogApp] ✗ 座位游戏面板未配置 - 请在Unity Inspector中拖拽seatPuzzlePanel对象");
        }
    }

    /// <summary>
    /// 隐藏座位拖拽游戏
    /// </summary>
    void HideSeatPuzzle()
    {
        if (seatPuzzlePanel != null)
        {
            seatPuzzlePanel.SetActive(false);
        }
        Debug.Log("[SimpleBlogApp] 隐藏座位拖拽游戏");
    }

    /// <summary>
    /// 初始化座位游戏
    /// </summary>
    void InitializePuzzle()
    {
        Debug.Log("[SimpleBlogApp] 开始初始化座位游戏");

        // 保存角色初始位置
        initialPositions.Clear();
        if (characterPieces != null)
        {
            Debug.Log($"[SimpleBlogApp] 保存 {characterPieces.Length} 个角色的初始位置");
            foreach (var character in characterPieces)
            {
                if (character != null)
                {
                    initialPositions[character.name] = character.transform.position;
                    Debug.Log($"[SimpleBlogApp] 角色 {character.name} 初始位置: {character.transform.position}");
                }
                else
                {
                    Debug.LogWarning("[SimpleBlogApp] 发现空的characterPieces元素");
                }
            }
        }
        else
        {
            Debug.LogError("[SimpleBlogApp] characterPieces数组未配置！");
        }

        // 重置角色位置
        ResetPuzzle();

        // 清空结果提示
        if (puzzleResultText != null)
        {
            puzzleResultText.text = "";
        }
        else
        {
            Debug.LogWarning("[SimpleBlogApp] puzzleResultText未配置");
        }

        Debug.Log("[SimpleBlogApp] ✓ 座位游戏初始化完成");
    }

    /// <summary>
    /// 重置座位游戏
    /// </summary>
    void ResetPuzzle()
    {
        if (characterPieces == null || initialPositions.Count == 0)
        {
            return;
        }

        // 将所有角色恢复到初始位置
        foreach (var character in characterPieces)
        {
            if (character != null && initialPositions.ContainsKey(character.name))
            {
                character.transform.position = initialPositions[character.name];
            }
        }

        Debug.Log("[SimpleBlogApp] 座位游戏已重置");
    }

    /// <summary>
    /// 检查座位游戏答案
    /// </summary>
    void CheckPuzzleSolution()
    {
        if (characterPieces == null || correctPositions == null)
        {
            Debug.LogWarning("[SimpleBlogApp] 座位游戏配置不完整");
            return;
        }

        bool allCorrect = true;
        int correctCount = 0;

        // 检查每个角色是否在正确位置
        for (int i = 0; i < characterPieces.Length && i < correctPositions.Length; i++)
        {
            if (characterPieces[i] != null)
            {
                Vector2 currentPos = characterPieces[i].transform.position;
                Vector2 correctPos = correctPositions[i];
                float distance = Vector2.Distance(currentPos, correctPos);

                // 允许一定的误差范围
                if (distance < 0.5f)
                {
                    correctCount++;
                }
                else
                {
                    allCorrect = false;
                }
            }
        }

        // 检查是否所有角色都在正确位置
        if (allCorrect && correctCount == characterPieces.Length)
        {
            OnPuzzleCompleted();
        }
        else
        {
            OnPuzzleFailed(correctCount, characterPieces.Length);
        }
    }

    /// <summary>
    /// 座位游戏完成
    /// </summary>
    void OnPuzzleCompleted()
    {
        Debug.Log("[SimpleBlogApp] 座位游戏完成！");

        isPuzzleCompleted = true;

        // 标记游戏完成
        if (GameManager.Instance?.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkPasswordSolved("seat_puzzle");
        }

        // 显示成功提示
        if (puzzleResultText != null)
        {
            puzzleResultText.text = "恭喜！座位安排正确！\n日志已解锁";
        }

        // 延迟隐藏游戏面板并解锁内容
        Invoke(nameof(HideSeatPuzzle), 2f);
        Invoke(nameof(UnlockDiaryContent), 2.5f);
    }

    /// <summary>
    /// 座位游戏失败
    /// </summary>
    void OnPuzzleFailed(int correctCount, int totalCount)
    {
        Debug.Log($"[SimpleBlogApp] 座位游戏失败，{correctCount}/{totalCount} 正确");

        // 显示失败提示
        if (puzzleResultText != null)
        {
            puzzleResultText.text = $"座位安排不正确\n({correctCount}/{totalCount} 正确)\n请继续尝试";
        }
    }

    /// <summary>
    /// 解锁日记内容 - 显示解锁后的博客日志页面
    /// </summary>
    void UnlockDiaryContent()
    {
        Debug.Log("[SimpleBlogApp] 座位游戏完成，解锁博客日志内容");

        // 隐藏座位游戏面板
        if (seatPuzzlePanel != null)
        {
            seatPuzzlePanel.SetActive(false);
        }

        // 显示解锁后的博客日志页面
        if (unlockedDiaryPanel != null)
        {
            unlockedDiaryPanel.SetActive(true);
            Debug.Log("[SimpleBlogApp] ✓ 显示解锁后的博客日志页面");

            // 初始化解锁内容（如果需要的话）
            InitializeUnlockedDiaryContent();
        }
        else
        {
            Debug.LogWarning("[SimpleBlogApp] unlockedDiaryPanel未配置，无法显示解锁内容");

            // 如果没有配置解锁面板，就继续使用原来的日志功能
            if (diaryScrollRect != null)
            {
                Debug.Log("[SimpleBlogApp] 使用原有日记滚动区域显示解锁内容");
                // 这里可以添加解锁内容的显示逻辑
            }
        }
    }

    /// <summary>
    /// 初始化解锁后的博客日志内容
    /// </summary>
    void InitializeUnlockedDiaryContent()
    {
        Debug.Log("[SimpleBlogApp] 初始化解锁后的博客日志内容");

        // 这里可以设置解锁后的具体内容
        // 比如显示隐藏的博客文章、秘密日志等

        if (unlockedDiaryScrollRect != null)
        {
            // 可以在这里动态创建或显示解锁内容
            Debug.Log("[SimpleBlogApp] 解锁内容滚动区域已准备");
        }
        else
        {
            Debug.LogWarning("[SimpleBlogApp] unlockedDiaryScrollRect未配置");
        }
    }

    /// <summary>
    /// 隐藏解锁后的博客日志页面
    /// </summary>
    void HideUnlockedDiary()
    {
        Debug.Log("[SimpleBlogApp] 隐藏解锁后的博客日志页面");

        if (unlockedDiaryPanel != null)
        {
            unlockedDiaryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 获取游戏完成状态（外部调用）
    /// </summary>
    public bool IsPuzzleCompleted()
    {
        return isPuzzleCompleted;
    }
}
