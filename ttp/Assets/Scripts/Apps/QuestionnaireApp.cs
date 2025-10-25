using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 问卷应用 - 关钦的好对象问卷
/// </summary>
public class QuestionnaireApp : MonoBehaviour
{
    [Header("UI组件")]
    public Button closeButton;
    public Button submitButton;
    public TextMeshProUGUI titleText;

    [Header("问题输入框")]
    public TMP_InputField question1Input; // 另一半的专业
    public TMP_InputField question2Input; // 另一半最喜欢的配饰
    public TMP_InputField question3Input; // 另一半最喜欢的电影
    public TMP_InputField question4Input; // 你们相爱的日子

    [Header("问题文本")]
    public TextMeshProUGUI question1Text;
    public TextMeshProUGUI question2Text;
    public TextMeshProUGUI question3Text;
    public TextMeshProUGUI question4Text;

    [Header("问卷面板")]
    public GameObject questionnairePanel;

    private string currentContactId = "";

    // 正确答案
    private Dictionary<string, string[]> correctAnswers = new Dictionary<string, string[]>
    {
        {
            "guanqin", new string[]
            {
                "计算机",      // 问题1: 另一半的专业
                "手表",        // 问题2: 另一半最喜欢的配饰
                "我的少女时代", // 问题3: 另一半最喜欢的电影
                "2015214"     // 问题4: 你们相爱的日子
            }
        }
    };

    void Start()
    {
        InitializeQuestionnaire();
    }

    void InitializeQuestionnaire()
    {
        SetupUIEvents();
        SetupQuestions();

        if (questionnairePanel != null)
        {
            questionnairePanel.SetActive(false);
        }
    }

    void SetupUIEvents()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseQuestionnaire);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitQuestionnaire);
        }
    }

    void SetupQuestions()
    {
        if (question1Text != null)
        {
            question1Text.text = "1. 另一半的专业是:";
        }

        if (question2Text != null)
        {
            question2Text.text = "2. 另一半最喜欢的配饰是:";
        }

        if (question3Text != null)
        {
            question3Text.text = "3. 另一半最喜欢的电影:";
        }

        if (question4Text != null)
        {
            question4Text.text = "4. 你们相爱的日子:";
        }

        if (titleText != null)
        {
            titleText.text = "求婚表情包";
        }
    }

    /// <summary>
    /// 打开问卷
    /// </summary>
    public void OpenQuestionnaire(string contactId)
    {
        currentContactId = contactId;

        if (questionnairePanel != null)
        {
            questionnairePanel.SetActive(true);
        }

        gameObject.SetActive(true);

        // 清空之前的答案
        ClearAnswers();

        Debug.Log($"打开问卷: {contactId}");
    }

    void ClearAnswers()
    {
        if (question1Input != null) question1Input.text = "";
        if (question2Input != null) question2Input.text = "";
        if (question3Input != null) question3Input.text = "";
        if (question4Input != null) question4Input.text = "";
    }

    void CloseQuestionnaire()
    {
        if (questionnairePanel != null)
        {
            questionnairePanel.SetActive(false);
        }

        gameObject.SetActive(false);

        // 返回聊天应用
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.OpenApp("chat");
        }
    }

    void SubmitQuestionnaire()
    {
        if (!correctAnswers.ContainsKey(currentContactId))
        {
            Debug.LogWarning($"没有为 {currentContactId} 设置问卷答案");
            return;
        }

        string[] answers = correctAnswers[currentContactId];

        // 获取用户输入
        string answer1 = question1Input != null ? question1Input.text.Trim() : "";
        string answer2 = question2Input != null ? question2Input.text.Trim() : "";
        string answer3 = question3Input != null ? question3Input.text.Trim() : "";
        string answer4 = question4Input != null ? question4Input.text.Trim() : "";

        // 验证答案
        bool isCorrect = answer1 == answers[0] &&
                         answer2 == answers[1] &&
                         answer3 == answers[2] &&
                         answer4 == answers[3];

        if (isCorrect)
        {
            Debug.Log("问卷答案正确!");
            OnQuestionnaireCorrect();
        }
        else
        {
            Debug.Log("问卷答案错误!");
            OnQuestionnaireWrong();
        }
    }

    /// <summary>
    /// 答案正确时的处理
    /// </summary>
    void OnQuestionnaireCorrect()
    {
        // 显示成功消息
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowSuccess("答案正确!解锁了隐藏的聊天记录!");
        }

        // 保存问卷完成状态
        if (GameManager.Instance != null && GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.MarkQuestionnaireCompleted(currentContactId);
        }

        // 解锁隐藏聊天记录
        var chatApp = FindObjectOfType<ChatApp>();
        if (chatApp != null)
        {
            chatApp.UnlockHiddenChat(currentContactId);
        }

        // 关闭问卷
        CloseQuestionnaire();

        // 返回聊天应用查看隐藏消息
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.OpenApp("chat");
        }
    }

    /// <summary>
    /// 答案错误时的处理
    /// </summary>
    void OnQuestionnaireWrong()
    {
        // 播放错误提示音
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowError("答案错误!请重新检查");
            GameManager.Instance.uiManager.ShakeScreen(2f, 0.2f);
        }

        // 清空答案,让玩家重新填写
        // ClearAnswers(); // 可选:是否清空答案
    }

    /// <summary>
    /// 检查问卷是否已完成
    /// </summary>
    public bool IsQuestionnaireCompleted(string contactId)
    {
        // 可以通过检查是否解锁了隐藏聊天来判断
        var chatApp = FindObjectOfType<ChatApp>();
        if (chatApp != null)
        {
            // 这里需要ChatApp提供一个公开的方法来检查
            return true; // 临时返回
        }
        return false;
    }
}
