// PasswordInputer.cs
using UnityEngine;

public class NewPasswordInputer : MonoBehaviour
{
    [Tooltip("拖入负责显示和校验的 PasswordChecker 实例")]
    public PasswordChecker checker;
    void Awake()
    {
        if (checker == null)
        {
            Debug.LogWarning("[PasswordInputer] 未设置 checker 引用，无法发送输入。");
            return;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0)||Input.GetKeyDown(KeyCode.Alpha0))
        {
            checker.HandleInput("0");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            checker.HandleInput("1");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            checker.HandleInput("2");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            checker.HandleInput("3");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
        {
           checker.HandleInput("4");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
        {
            checker.HandleInput("5");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
        {
            checker.HandleInput("6");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
        {
            checker.HandleInput("7");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            checker.HandleInput("8");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
        {
            checker.HandleInput("9");
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            checker.HandleInput("*");
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            checker.HandleInput("#");
        }
    }
}
