using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Crime : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [HideInInspector] public Container CurrentContainer; // 当前所在容器（null 表示未在任何容器中）

    public int CrimeId;
    private RectTransform rt;
    private Canvas canvas; // 所属 Canvas（用于坐标转换）
    private Vector2 pointerOffset; // 鼠标相对物体中心的偏移（避免跳跃）
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("DraggableItem 必须在 Canvas 的子物体中。");

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
        Vector2 localPointer;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, eventData.position, eventData.pressEventCamera, out localPointer);
        pointerOffset = rt.pivot * rt.rect.size - localPointer;
        if (CurrentContainer != null)
        {
            CurrentContainer.RemoveItem(this);
        }
        if (canvas != null)
        {
            rt.SetParent(canvas.transform, true);
            rt.SetAsLastSibling();
        }

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 localPoint;
        RectTransform canvasRT = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localPoint);
        rt.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        Rect itemScreenRect = GetScreenRect(rt);
        Container target = Container.FindBestOverlapContainer(itemScreenRect);

        if (target == null || !target.HasSpace())
        {
            target = Container.defaultContainer;
        }

        if (target == null)
        {
            Debug.LogWarning("没有找到默认容器（Container.defaultContainer 未设置）。物体保持当前位置。");
            canvasGroup.blocksRaycasts = true;
            return;
        }
        int slot = target.AddItemToLeftmost(this);
        if (slot == -1)
        {
            if (target != Container.defaultContainer && Container.defaultContainer != null)
            {
                Container.defaultContainer.AddItemToLeftmost(this);
            }
            else
            {
                Debug.LogWarning("所有容器均已满，物体未能放入。");
            }
        }

        canvasGroup.blocksRaycasts = true;
    }
    private Rect GetScreenRect(RectTransform r)
    {
        Vector3[] corners = new Vector3[4];
        r.GetWorldCorners(corners);
        Vector3 bl = corners[0];
        Vector3 tr = corners[2];
        return new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
    }
}
