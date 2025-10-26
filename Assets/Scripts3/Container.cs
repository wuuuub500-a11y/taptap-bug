using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Container : MonoBehaviour
{
    public int capacity = 4; // 容器格数（从左到右均分）
    public int[] correctCrimes;
    public bool isDefault = false; // 是否为 container0（默认容器）
    public RectTransform itemsParent; // 可选：用于放 item 的父物体（留空则使用 this.transform）

    // 内部槽位数组，长度 = capacity；空位为 null
    private Crime[] slots;

    // 静态管理所有容器
    public static List<Container> allContainers = new List<Container>();
    public static Container defaultContainer;

    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        if (itemsParent == null)
            itemsParent = rt;

        slots = new Crime[capacity];

        if (isDefault)
            defaultContainer = this;
    }

    void OnEnable()
    {
        if (!allContainers.Contains(this))
            allContainers.Add(this);
    }

    void OnDisable()
    {
        allContainers.Remove(this);
        if (defaultContainer == this)
            defaultContainer = null;
    }

    // 尝试在最左边空位放入 item（返回放入的 slotIndex 或 -1 表示失败）
    public int AddItemToLeftmost(Crime item)
    {
        for (int i = 0; i < capacity; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                PlaceItemAtSlot(item, i);
                item.CurrentContainer = this;
                return i;
            }
        }
        return -1; // 满
    }

    // 从容器移除 item（并把右侧所有物体左移一位）
    public void RemoveItem(Crime item)
    {
        int idx = IndexOf(item);
        if (idx == -1) return;

        slots[idx] = null;

        // 右侧所有左移一格
        for (int j = idx + 1; j < capacity; j++)
        {
            if (slots[j] != null)
            {
                slots[j - 1] = slots[j];
                // 更新被移动物体的位置（实际位置设置也在 PlaceItemAtSlot）
                PlaceItemAtSlot(slots[j - 1], j - 1);
                slots[j] = null;
            }
            else
            {
                // 发现空位，右侧后面也都为空，直接可以结束
                break;
            }
        }

        item.CurrentContainer = null;
    }

    // 返回 item 在本容器中的索引，否则 -1
    public int IndexOf(Crime item)
    {
        for (int i = 0; i < capacity; i++)
            if (slots[i] == item) return i;
        return -1;
    }

    public bool HasSpace()
    {
        for (int i = 0; i < capacity; i++)
            if (slots[i] == null) return true;
        return false;
    }
    public Vector2 GetSlotAnchoredPosition(int slotIndex)
    {
        float w = rt.rect.width;
        float slotW = w / capacity;
        // 左端中心 x = -w/2 + slotW/2
        float leftCenterX = -w * 0.5f + slotW * 0.5f;
        float x = leftCenterX + slotIndex * slotW;
        float y = 0f; // 垂直居中
        return new Vector2(x, y);
    }

    private void PlaceItemAtSlot(Crime item, int slotIndex)
    {
        var itemRT = item.GetComponent<RectTransform>();
        itemRT.SetParent(itemsParent, false);
        itemRT.localScale = Vector3.one;
        itemRT.anchoredPosition = GetSlotAnchoredPosition(slotIndex);
        itemRT.SetAsLastSibling();
    }

    // 将 RectTransform 转换为屏幕坐标 Rect
    public Rect GetScreenRect()
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector3 bl = corners[0];
        Vector3 tr = corners[2];
        return new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
    }
    public float OverlapAreaWith(Rect itemScreenRect)
    {
        Rect c = GetScreenRect();
        if (!c.Overlaps(itemScreenRect)) return 0f;
        float xMin = Mathf.Max(c.xMin, itemScreenRect.xMin);
        float xMax = Mathf.Min(c.xMax, itemScreenRect.xMax);
        float yMin = Mathf.Max(c.yMin, itemScreenRect.yMin);
        float yMax = Mathf.Min(c.yMax, itemScreenRect.yMax);
        return Mathf.Max(0f, xMax - xMin) * Mathf.Max(0f, yMax - yMin);
    }

    // 查找与给定 itemScreenRect 重叠面积最大的容器（返回 null 表示没有重叠）
    public static Container FindBestOverlapContainer(Rect itemScreenRect)
    {
        Container best = null;
        float bestArea = 0f;
        foreach (var c in allContainers)
        {
            float area = c.OverlapAreaWith(itemScreenRect);
            if (area > bestArea)
            {
                bestArea = area;
                best = c;
            }
        }
        return bestArea > 0f ? best : null;
    }
    public bool IsCorrectlyPlaced()
    {
        bool isCorrect= true;
        for (int i = 0; i < correctCrimes.Length; i++) {
            bool isExist = false;
            for (int j = 0; j < slots.Length; j++)
            {
                if(slots[j] != null && slots[j].CrimeId == correctCrimes[i])
                {
                    isExist = true;
                    break;
                }
            }
            if (!isExist) {
                isCorrect = false;
            }
        }
        return isCorrect;
    }
}
