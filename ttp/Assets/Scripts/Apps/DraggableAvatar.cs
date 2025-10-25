using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 可拖动头像组件
/// </summary>
public class DraggableAvatar : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int avatarIndex;                     // 头像索引
    public SeatPuzzleGame puzzleGame;           // 游戏引用
    public Image avatarImage;                   // 头像图片
    public bool isKuro;                         // 是否是Kuro（固定不能拖动）
    [TextArea]
    public string hoverDialogue;                // 鼠标悬停时显示的对话
    public Vector2 tooltipOffset = new Vector2(0f, 80f); // Tooltip 相对偏移

    [HideInInspector] public int currentSeatIndex = -1;     // 当前所在座位索引 (-1表示在原始位置)
    [HideInInspector] public Vector3 originalPosition;      // 原始位置
    [HideInInspector] public Transform originalParent;      // 原始父对象

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isDragging = false;
    private Color originalAvatarColor = Color.white;

    void Awake()
    {
        Debug.Log($"[DraggableAvatar {avatarIndex}] Awake() 被调用");

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        Debug.Log($"[DraggableAvatar {avatarIndex}] RectTransform={rectTransform != null}, Canvas={canvas != null}");

        if (avatarImage == null)
        {
            avatarImage = GetComponent<Image>();
            if (avatarImage == null)
            {
                Debug.LogError($"[DraggableAvatar {avatarIndex}] 缺少Image组件，无法接收拖拽事件");
            }
        }

        if (avatarImage != null && !avatarImage.raycastTarget)
        {
            avatarImage.raycastTarget = true; // 确保UI Graphic能响应指针事件
            Debug.Log($"[DraggableAvatar {avatarIndex}] 已启用Image.RaycastTarget");
        }

        if (avatarImage != null)
        {
            originalAvatarColor = avatarImage.color;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log($"[DraggableAvatar {avatarIndex}] 添加了CanvasGroup组件");
        }
        canvasGroup.blocksRaycasts = true;

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        Debug.Log($"[DraggableAvatar {avatarIndex}] 原始位置: {originalPosition}, 父对象: {originalParent?.name}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[DraggableAvatar {avatarIndex}] OnBeginDrag被调用，isKuro={isKuro}");

        if (isKuro)
        {
            // Kuro不能拖动
            Debug.Log($"[DraggableAvatar {avatarIndex}] Kuro不能拖动");
            return;
        }

        puzzleGame?.HideAvatarTooltip();
        isDragging = true;

        // 设置为半透明
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // 如果之前在座位上，移除
        if (currentSeatIndex >= 0 && puzzleGame != null)
        {
            puzzleGame.RemoveAvatarFromSeat(currentSeatIndex);
            currentSeatIndex = -1;
        }

        Debug.Log($"[DraggableAvatar {avatarIndex}] 开始拖动，isDragging={isDragging}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isKuro || !isDragging) return;

        // 跟随鼠标移动
        if (canvas != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out pos);

            rectTransform.anchoredPosition = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isKuro || !isDragging) return;

        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        puzzleGame?.HideAvatarTooltip();

        // 如果没有成功放置到座位上，回到原位
        if (currentSeatIndex < 0)
        {
            ResetPosition();
            Debug.Log($"[DraggableAvatar {avatarIndex}] 未放置到座位，回到原位");
        }
    }

    /// <summary>
    /// 放置到座位上
    /// </summary>
    public void PlaceOnSeat(int seatIndex)
    {
        if (puzzleGame == null || puzzleGame.seatSlots == null || seatIndex < 0 || seatIndex >= puzzleGame.seatSlots.Length)
        {
            ResetPosition();
            return;
        }

        SeatSlot seat = puzzleGame.seatSlots[seatIndex];
        if (seat == null)
        {
            ResetPosition();
            return;
        }

        // 移动到座位位置
        rectTransform.SetParent(seat.transform, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;

        ResetAvatarColor();

        // 通知游戏
        puzzleGame.PlaceAvatar(this, seatIndex);

        Debug.Log($"[DraggableAvatar {avatarIndex}] 放置到座位 {seatIndex}");
    }

    /// <summary>
    /// 重置到原始位置
    /// </summary>
    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        currentSeatIndex = -1;
        ResetAvatarColor();
    }

    /// <summary>
    /// 设置头像颜色（用于成功时变红）
    /// </summary>
    public void SetAvatarColor(Color color)
    {
        if (avatarImage != null)
        {
            avatarImage.color = color;
        }
    }

    public void ResetAvatarColor()
    {
        if (avatarImage != null)
        {
            avatarImage.color = originalAvatarColor;
        }
    }

    /// <summary>
    /// 点击检测
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[DraggableAvatar {avatarIndex}] 🔥🔥🔥 头像被点击了！！！");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (puzzleGame != null)
        {
            puzzleGame.ShowAvatarTooltip(this, eventData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        puzzleGame?.HideAvatarTooltip();
    }
}
