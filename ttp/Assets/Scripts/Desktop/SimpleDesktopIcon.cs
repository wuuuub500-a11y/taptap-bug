using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
/// <summary>
/// 简化的桌面图标控制器
/// </summary>
public class SimpleDesktopIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("图标配置")]
    public string appId;
    public string appName;
    public bool isUnlocked = false;

    [Header("视觉设置")]
    public Color normalColor = Color.white;
    public Color lockedColor = Color.gray;
    public Color hoverColor = Color.yellow;
    public float hoverScale = 1.1f;
    public float animationSpeed = 5f;

    [Header("组件引用")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public GameObject lockedOverlay;

    private Vector3 originalScale;
    private Color targetColor;
    private Vector3 targetScale;
    private bool isHovering = false;

    void Awake()
    {
        // 自动获取组件
        if (iconImage == null) iconImage = GetComponent<Image>();
        if (iconImage == null) iconImage = GetComponentInChildren<Image>();

        originalScale = transform.localScale;
        targetScale = originalScale;

        // 不在这里设置颜色,等Start()中根据isUnlocked设置
    }

    void Start()
    {
        // 根据解锁状态设置初始颜色
        targetColor = isUnlocked ? normalColor : lockedColor;

        // 立即应用颜色(不用动画)
        if (iconImage != null)
        {
            iconImage.color = targetColor;
        }

        UpdateVisualState();
    }

    void Update()
    {
        // 平滑动画
        if (iconImage != null)
        {
            iconImage.color = Color.Lerp(iconImage.color, targetColor, Time.deltaTime * animationSpeed);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUnlocked)
        {
            Debug.Log($"应用 {appName} 已锁定");
            // 可以添加锁定提示音效
            return;
        }

        Debug.Log($"点击应用: {appName}");
        OpenApplication();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isUnlocked) return;

        isHovering = true;
        targetColor = hoverColor;
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetColor = isUnlocked ? normalColor : lockedColor;  // 根据解锁状态恢复颜色
        targetScale = originalScale;
    }

    /// <summary>
    /// 打开应用
    /// </summary>
    private void OpenApplication()
    {
        // 优先通过AppManager打开应用(新逻辑)
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.OnAppIconClicked(appId);
        }
        else
        {
            // 兼容旧逻辑:通过WindowManager打开应用
            WindowManager windowManager = FindObjectOfType<WindowManager>();
            if (windowManager != null)
            {
                windowManager.OpenApp(appId);
            }
            else
            {
                Debug.LogError("找不到AppManager或WindowManager！");
            }
        }

        // 播放点击音效
        PlayClickSound();
    }

    /// <summary>
    /// 设置解锁状态
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;

        // 立即更新颜色(不用等待动画)
        targetColor = isUnlocked ? normalColor : lockedColor;
        if (iconImage != null)
        {
            iconImage.color = targetColor;  // 立即应用颜色
        }

        UpdateVisualState();

        Debug.Log($"[SimpleDesktopIcon] {appName} SetUnlocked={unlocked}, 颜色={targetColor}");
    }

    /// <summary>
    /// 更新视觉状态
    /// </summary>
    private void UpdateVisualState()
    {
        // 更新目标颜色(如果不在悬停状态)
        if (!isHovering)
        {
            targetColor = isUnlocked ? normalColor : lockedColor;
        }

        // 更新锁定覆盖层
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }

        // 更新名称颜色
        if (nameText != null)
        {
            nameText.color = isUnlocked ? Color.white : Color.gray;
        }
    }

    /// <summary>
    /// 播放点击音效
    /// </summary>
    private void PlayClickSound()
    {
        // 这里可以添加音效播放逻辑
        Debug.Log($"播放 {appName} 点击音效");
    }

    /// <summary>
    /// 设置图标信息
    /// </summary>
    public void SetIconInfo(string id, string name)
    {
        appId = id;
        appName = name;

        // 更新名称文本
        if (nameText != null)
        {
            nameText.text = name;
        }
    }
}