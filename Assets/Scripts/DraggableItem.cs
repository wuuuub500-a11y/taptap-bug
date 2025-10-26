using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem : MonoBehaviour
{
    public Transform targetTransform;// 指定放置位置（在 Inspector 设置）
    public float radius = 0.5f;

    Vector3 initialPosition;
    Collider2D myCollider;
    bool isPlaced = false;
    static DraggableItem currentDragging = null;
    Vector3 dragOffset; // 用于保持鼠标与物体间的相对偏移

    void Start()
    {
        initialPosition = transform.position;
        myCollider = GetComponent<Collider2D>();

        if (targetTransform == null)
        {
            Debug.LogWarning($"targetTransform 未设置");
        }
    }
    void Update()
    {
        if (isPlaced) return; // 已经放置完成

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z; // 保持 z 不变
        if (Input.GetMouseButtonDown(0))
        {
            if (currentDragging == null)
            {
                Vector2 p2 = new Vector2(mouseWorld.x, mouseWorld.y);
                if (myCollider.OverlapPoint(p2))
                {
                    currentDragging = this;
                    dragOffset = transform.position - mouseWorld;
                }
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (currentDragging == this)
            {
                transform.position = mouseWorld + dragOffset;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (currentDragging == this)
            {
                HandleRelease();
                currentDragging = null;
            }
        }
    }

    void HandleRelease()
    {
        if (targetTransform != null)
        {
            float dist = Vector2.Distance(transform.position, targetTransform.position);
            if (dist <= radius)
            {
                transform.position = targetTransform.position;
                isPlaced = true;
                if (myCollider != null) myCollider.enabled = false;
                PlacementManager.Instance.ItemPlaced();
                return;
            }
        }

        transform.position = initialPosition;
    }

    // 重置
    public void ResetToInitial()
    {
        isPlaced = false;
        transform.position = initialPosition;
        if (myCollider != null) myCollider.enabled = true;
    }
}
