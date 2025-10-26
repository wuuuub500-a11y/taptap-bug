// PasswordInputer.cs
using UnityEngine;

public class ConsoleInputer : MonoBehaviour
{
    public ConsoleChecker checker;
    void Awake()
    {
        if (checker == null)
        {
            Debug.LogWarning("未设置 checker 引用，无法发送输入。");
            return;
        }
    }
    void Update()
    {

        foreach (char c in Input.inputString)
        {
            if (c == '\b') // 处理退格键
            {
                checker.HandleInput("Backspace");
            }
            else if (c == '\n' || c == '\r') // 处理回车键
            {
                checker.HandleInput("Return");
            }
            else
            {
               checker.HandleInput(c.ToString());
            }
        }
    }
}
