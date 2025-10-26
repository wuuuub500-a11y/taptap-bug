using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem : MonoBehaviour
{
    public Transform targetTransform;// ָ������λ�ã��� Inspector ���ã�
    public float radius = 0.5f;

    Vector3 initialPosition;
    Collider2D myCollider;
    bool isPlaced = false;
    static DraggableItem currentDragging = null;
    Vector3 dragOffset; // ���ڱ�����������������ƫ��

    void Start()
    {
        initialPosition = transform.position;
        myCollider = GetComponent<Collider2D>();

        if (targetTransform == null)
        {
            Debug.LogWarning($"targetTransform δ����");
        }
    }
    void Update()
    {
        if (isPlaced) return; // �Ѿ��������

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z; // ���� z ����
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

    // ����
    public void ResetToInitial()
    {
        isPlaced = false;
        transform.position = initialPosition;
        if (myCollider != null) myCollider.enabled = true;
    }
}
