using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Container : MonoBehaviour
{
    public int capacity = 4; // ���������������Ҿ��֣�
    public int[] correctCrimes;
    public bool isDefault = false; // �Ƿ�Ϊ container0��Ĭ��������
    public RectTransform itemsParent; // ��ѡ�����ڷ� item �ĸ����壨������ʹ�� this.transform��

    // �ڲ���λ���飬���� = capacity����λΪ null
    private Crime[] slots;

    // ��̬������������
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

    // ����������߿�λ���� item�����ط���� slotIndex �� -1 ��ʾʧ�ܣ�
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
        return -1; // ��
    }

    // �������Ƴ� item�������Ҳ�������������һλ��
    public void RemoveItem(Crime item)
    {
        int idx = IndexOf(item);
        if (idx == -1) return;

        slots[idx] = null;

        // �Ҳ���������һ��
        for (int j = idx + 1; j < capacity; j++)
        {
            if (slots[j] != null)
            {
                slots[j - 1] = slots[j];
                // ���±��ƶ������λ�ã�ʵ��λ������Ҳ�� PlaceItemAtSlot��
                PlaceItemAtSlot(slots[j - 1], j - 1);
                slots[j] = null;
            }
            else
            {
                // ���ֿ�λ���Ҳ����Ҳ��Ϊ�գ�ֱ�ӿ��Խ���
                break;
            }
        }

        item.CurrentContainer = null;
    }

    // ���� item �ڱ������е����������� -1
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
        // ������� x = -w/2 + slotW/2
        float leftCenterX = -w * 0.5f + slotW * 0.5f;
        float x = leftCenterX + slotIndex * slotW;
        float y = 0f; // ��ֱ����
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

    // �� RectTransform ת��Ϊ��Ļ���� Rect
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

    // ��������� itemScreenRect �ص������������������ null ��ʾû���ص���
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
