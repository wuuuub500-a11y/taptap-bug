// PasswordInputer.cs
using UnityEngine;

public class PasswordInputer : MonoBehaviour
{
    [Tooltip("拖入负责显示和校验的 PasswordChecker 实例")]
    public PasswordChecker checker;

    private BoxCollider2D boxCollider;

    // 网格映射：从上到下四行，每行三个列
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
            Debug.LogWarning("[PasswordInputer] 需要在同一 GameObject 上添加 BoxCollider2D 用作可点击区域！");
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

            // 列：0..2 从左到右
            int col = Mathf.Clamp((int)(rel.x * 3f), 0, 2);
            int row = Mathf.Clamp((int)((1f - rel.y) * 4f), 0, 3);

            string value = grid[row, col];

            if (checker == null)
            {
                Debug.LogWarning("[PasswordInputer] 未设置 checker 引用，无法发送输入。");
                return;
            }
            if (IsDigit(value) && checker.CurrentInputLength >= 5)
            {
                // 忽略
                return;
            }

            // 传递给 checker
            checker.HandleInput(value);
        }
    }

    private bool IsDigit(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        return char.IsDigit(s[0]);
    }
}
