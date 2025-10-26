// PasswordInputer.cs
using UnityEngine;

public class PasswordInputer : MonoBehaviour
{
    [Tooltip("���븺����ʾ��У��� PasswordChecker ʵ��")]
    public PasswordChecker checker;

    private BoxCollider2D boxCollider;

    // ����ӳ�䣺���ϵ������У�ÿ��������
    private readonly string[,] grid = new string[4, 3]
    {
        { "7", "8", "9" },   
        { "4", "5", "6" },   
        { "1", "2", "3" },   
        { "*", "0", "#" }   
    };

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning("[PasswordInputer] ��Ҫ��ͬһ GameObject ����� BoxCollider2D �����ɵ������");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point2D = new Vector2(wp.x, wp.y);

            if (boxCollider == null) return;

            if (!boxCollider.OverlapPoint(point2D)) return;

            Bounds b = boxCollider.bounds;
            Vector2 rel = new Vector2(
                (point2D.x - b.min.x) / b.size.x,
                (point2D.y - b.min.y) / b.size.y
            );

            // �У�0..2 ������
            int col = Mathf.Clamp((int)(rel.x * 3f), 0, 2);
            int row = Mathf.Clamp((int)((1f - rel.y) * 4f), 0, 3);

            string value = grid[row, col];

            if (checker == null)
            {
                Debug.LogWarning("[PasswordInputer] δ���� checker ���ã��޷��������롣");
                return;
            }
            if (IsDigit(value) && checker.CurrentInputLength >= 5)
            {
                // ����
                return;
            }

            // ���ݸ� checker
            checker.HandleInput(value);
        }
    }

    private bool IsDigit(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        return char.IsDigit(s[0]);
    }
}
