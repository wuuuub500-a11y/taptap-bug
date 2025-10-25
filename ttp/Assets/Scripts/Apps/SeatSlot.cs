using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 座位槽组件
/// </summary>
public class SeatSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public SeatPuzzleGame puzzleGame;
    public Image backImage;         // 后脑勺图案（白色，提示正确位置）
    public bool isKuroSeat;         // 是否是Kuro的固定座位

    void Awake()
    {
        var slotImage = GetComponent<Image>();
        if (slotImage == null)
        {
            slotImage = gameObject.AddComponent<Image>();
            slotImage.color = Color.clear; // 透明但可接收射线
            Debug.LogWarning($"[SeatSlot {slotIndex}] 未配置Image，已自动添加透明Image用于拖拽检测");
        }

        if (!slotImage.raycastTarget)
        {
            slotImage.raycastTarget = true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[SeatSlot {slotIndex}] OnDrop被调用，isKuroSeat={isKuroSeat}");

        if (isKuroSeat)
        {
            // Kuro的座位不允许放置其他人
            Debug.Log($"[SeatSlot {slotIndex}] Kuro的座位，不能放置");
            return;
        }

        DraggableAvatar avatar = eventData.pointerDrag?.GetComponent<DraggableAvatar>();
        if (avatar != null && puzzleGame != null)
        {
            Debug.Log($"[SeatSlot {slotIndex}] 检测到拖放头像 {avatar.avatarIndex}");
            avatar.PlaceOnSeat(slotIndex);
        }
        else
        {
            Debug.Log($"[SeatSlot {slotIndex}] 拖放失败，avatar={avatar}, puzzleGame={puzzleGame}");
        }
    }
}
