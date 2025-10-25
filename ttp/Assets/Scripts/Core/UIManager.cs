using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI元素")]
    public GameObject passwordPanel;
    public TMPro.TMP_InputField passwordInputField;
    public Text passwordTitleText;
    public Button passwordConfirmButton;
    public Button passwordCancelButton;

    [Header("消息提示")]
    public GameObject messagePanel;
    public Text messageText;
    public Button messageCloseButton;
    public float messageDisplayTime = 3f;

    private System.Action<string> onPasswordConfirmed;
    private System.Action onPasswordCancelled;

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        // 初始化密码面板
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(false);

            if (passwordConfirmButton != null)
            {
                passwordConfirmButton.onClick.AddListener(OnPasswordConfirm);
            }

            if (passwordCancelButton != null)
            {
                passwordCancelButton.onClick.AddListener(OnPasswordCancel);
            }
        }

        // 初始化消息面板
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);

            if (messageCloseButton != null)
            {
                messageCloseButton.onClick.AddListener(CloseMessage);
            }
        }
    }

    /// <summary>
    /// 显示密码输入面板
    /// </summary>
    public void ShowPasswordPanel(string title, System.Action<string> onConfirm, System.Action onCancel = null)
    {
        if (passwordPanel == null)
        {
            Debug.LogError("密码面板未分配！");
            return;
        }

        if (passwordTitleText != null)
        {
            passwordTitleText.text = title;
        }

        if (passwordInputField != null)
        {
            passwordInputField.text = "";
            passwordInputField.Select();
        }

        onPasswordConfirmed = onConfirm;
        onPasswordCancelled = onCancel;

        passwordPanel.SetActive(true);
    }

    /// <summary>
    /// 隐藏密码输入面板
    /// </summary>
    public void HidePasswordPanel()
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(false);
        }

        onPasswordConfirmed = null;
        onPasswordCancelled = null;
    }

    /// <summary>
    /// 显示消息
    /// </summary>
    public void ShowMessage(string message, float displayTime = -1f)
    {
        if (messagePanel == null || messageText == null)
        {
            Debug.LogError("消息面板或消息文本未分配！");
            return;
        }

        messageText.text = message;
        messagePanel.SetActive(true);

        if (displayTime < 0)
        {
            displayTime = messageDisplayTime;
        }

        if (displayTime > 0)
        {
            CancelInvoke(nameof(AutoCloseMessage));
            Invoke(nameof(AutoCloseMessage), displayTime);
        }
    }

    /// <summary>
    /// 隐藏消息面板
    /// </summary>
    public void CloseMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        CancelInvoke(nameof(AutoCloseMessage));
    }

    /// <summary>
    /// 显示错误消息
    /// </summary>
    public void ShowError(string errorMessage)
    {
        ShowMessage($"错误: {errorMessage}");
    }

    /// <summary>
    /// 显示成功消息
    /// </summary>
    public void ShowSuccess(string successMessage)
    {
        ShowMessage(successMessage);
    }

    /// <summary>
    /// 密码确认按钮点击事件
    /// </summary>
    private void OnPasswordConfirm()
    {
        string password = passwordInputField != null ? passwordInputField.text : "";
        onPasswordConfirmed?.Invoke(password);
        HidePasswordPanel();
    }

    /// <summary>
    /// 密码取消按钮点击事件
    /// </summary>
    private void OnPasswordCancel()
    {
        onPasswordCancelled?.Invoke();
        HidePasswordPanel();
    }

    /// <summary>
    /// 自动关闭消息
    /// </summary>
    private void AutoCloseMessage()
    {
        CloseMessage();
    }

    /// <summary>
    /// 添加屏幕震动效果
    /// </summary>
    public void ShakeScreen(float intensity = 10f, float duration = 0.5f)
    {
        if (Camera.main != null)
        {
            StartCoroutine(ShakeCameraCoroutine(Camera.main, intensity, duration));
        }
        else
        {
            // 如果没有主相机，尝试震动整个屏幕
            Debug.LogWarning("未找到主相机，使用备用震动方法");
            StartCoroutine(ShakeScreenCoroutine(intensity, duration));
        }
    }

    /// <summary>
    /// 相机震动协程
    /// </summary>
    private System.Collections.IEnumerator ShakeCameraCoroutine(Camera camera, float intensity, float duration)
    {
        Vector3 originalPosition = camera.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 计算震动偏移
            float x = Random.Range(-intensity, intensity) * 0.1f;
            float y = Random.Range(-intensity, intensity) * 0.1f;

            // 应用震动偏移
            camera.transform.localPosition = originalPosition + new Vector3(x, y, 0);

            yield return null;
        }

        // 恢复原始位置
        camera.transform.localPosition = originalPosition;
    }

    /// <summary>
    /// 屏幕震动协程 (备用方法)
    /// </summary>
    private System.Collections.IEnumerator ShakeScreenCoroutine(float intensity, float duration)
    {
        Vector3 originalPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 计算震动偏移
            float x = Random.Range(-intensity, intensity) * 0.05f;
            float y = Random.Range(-intensity, intensity) * 0.05f;

            // 应用震动偏移
            transform.position = originalPosition + new Vector3(x, y, 0);

            yield return null;
        }

        // 恢复原始位置
        transform.position = originalPosition;
    }
}