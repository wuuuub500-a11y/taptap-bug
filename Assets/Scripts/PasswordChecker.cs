// PasswordChecker.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordChecker : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("��ʾ��ǰ����� Text��UnityEngine.UI.Text��")]
    public TextMeshProUGUI PasswordText;
    public TextMeshProUGUI StatusText;
    public string correctMessage;
    public string incorrectMessage;
    [Header("Password")]
    [Tooltip("��ȷ���룬����Ϊ 5 λ����")]
    public string correctPassword;

    private string currentInput = "";

    // �� PasswordInputer ��ѯ��ǰ���볤�ȣ���ĿҪ��
    public int CurrentInputLength => currentInput.Length;
    public bool isUnlocked = false;
    // PasswordInputer �����������������ݵ�Ԫ���ֵ��"0".."9" / "*" / "#"��
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
            // ȷ�ϣ�ֻ�е�ǡ�� 5 λʱ���бȽ�
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
