// PasswordInputer.cs
using UnityEngine;

public class ConsoleInputer : MonoBehaviour
{
    public ConsoleChecker checker;
    void Awake()
    {
        if (checker == null)
        {
            Debug.LogWarning("δ���� checker ���ã��޷��������롣");
            return;
        }
    }
    void Update()
    {

        foreach (char c in Input.inputString)
        {
            if (c == '\b') // �����˸��
            {
                checker.HandleInput("Backspace");
            }
            else if (c == '\n' || c == '\r') // ����س���
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
