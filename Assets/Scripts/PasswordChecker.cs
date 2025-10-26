// PasswordChecker.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordChecker : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("显示当前输入的 Text（UnityEngine.UI.Text）")]
    public TextMeshProUGUI PasswordText;
    public TextMeshProUGUI StatusText;
    public string correctMessage;
    public string incorrectMessage;
    [Header("Password")]
    [Tooltip("正确密码，必须为 5 位数字")]
    public string correctPassword;

    private string currentInput = "";

    // 供 PasswordInputer 查询当前输入长度（题目要求）
    public int CurrentInputLength => currentInput.Length;
    public bool isUnlocked = false;
    // PasswordInputer 会调用这个方法来传递单元格的值（"0".."9" / "*" / "#"）
    public void HandleInput(string input)
    {
        if (input == "*")
        {
            StatusText.text = "";
            currentInput = "";
            UpdateDisplay();
            return;
        }

        if (input == "#")
        {
            // 确认：只有当恰好 5 位时进行比较
            if (currentInput.Length == 5)
            {
                if (currentInput == correctPassword)
                {
                    isUnlocked = true;
                    StatusText.text = correctMessage;
                }
                else
                {
                    StatusText.text = incorrectMessage;
                }
            }
            else
            {
                StatusText.text = incorrectMessage;
            }
            UpdateDisplay();
            return;
        }
        if (input.Length == 1 && char.IsDigit(input[0]))
        {
            if (currentInput.Length < 5)
            {
                currentInput += input;
                UpdateDisplay();
            }
        }
    }

    private void UpdateDisplay()
    {
        if (PasswordText != null)
        {
            PasswordText.text = currentInput;
        }
    }
}
