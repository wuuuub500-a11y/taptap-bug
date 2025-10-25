using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DesktopIcon : MonoBehaviour
{
    [Header("图标设置")]
    public string appId;
    public string appName;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button iconButton;
    public GameObject lockedOverlay;

    [Header("视觉效果")]
    public Color normalColor = Color.white;
    public Color lockedColor = Color.gray;
    public float hoverScale = 1.1f;
    public float animationDuration = 0.2f;

    private bool isUnlocked = false;

    void Start()
    {
        InitializeIcon();
    }

    void InitializeIcon()
    {
        // 设置应用名称
        if (nameText != null)
        {
            nameText.text = appName;
        }

        // 获取Button组件（如果没有则添加）
        if (iconButton == null)
        {
            iconButton = GetComponent<Button>();
            if (iconButton == null)
            {
                iconButton = gameObject.AddComponent<Button>();
            }
        }

        // 初始化状态
        UpdateIconState();
    }

    /// <summary>
    /// 设置解锁状态
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        UpdateIconState();
    }

    /// <summary>
    /// 更新图标显示状态
    /// </summary>
    private void UpdateIconState()
    {
        if (iconImage != null)
        {
            iconImage.color = isUnlocked ? normalColor : lockedColor;
        }

        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }

        if (iconButton != null)
        {
            iconButton.interactable = isUnlocked;
        }
    }

    /// <summary>
    /// 鼠标悬停效果
    /// </summary>
    public void OnHoverEnter()
    {
        if (isUnlocked)
        {
            StartCoroutine(ScaleAnimation(hoverScale));
        }
    }

    /// <summary>
    /// 鼠标离开效果
    /// </summary>
    public void OnHoverExit()
    {
        StartCoroutine(ScaleAnimation(1f));
    }

    private System.Collections.IEnumerator ScaleAnimation(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, targetScale);
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
    }

    /// <summary>
    /// 播放点击音效
    /// </summary>
    public void PlayClickSound()
    {
        // 这里可以添加音效播放逻辑
        Debug.Log($"点击了应用: {appName}");
    }

    /// <summary>
    /// 播放锁定提示音效
    /// </summary>
    public void PlayLockedSound()
    {
        // 这里可以添加锁定状态的音效
        Debug.Log($"应用 {appName} 已锁定");
    }

    /// <summary>
    /// 获取应用ID
    /// </summary>
    public string GetAppId()
    {
        return appId;
    }

    /// <summary>
    /// 获取应用名称
    /// </summary>
    public string GetAppName()
    {
        return appName;
    }

    /// <summary>
    /// 检查是否已解锁
    /// </summary>
    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}