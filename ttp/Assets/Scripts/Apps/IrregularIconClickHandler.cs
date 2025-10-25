using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

#pragma warning disable CS0414

public class IrregularIconClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("点击检测设置")]
    public Image iconImage;
    public float alphaThreshold = 0.1f; // 透明度阈值，小于此值视为透明
    public bool usePixelPerfectDetection = true;

    [Header("视觉反馈")]
    public float hoverScale = 1.1f;
    public float animationDuration = 0.2f;
    public Color hoverColor = Color.white;
    public Color normalColor = Color.gray;

    [Header("图标信息")]
    public string appId;
    public string appName;

    private bool isHovering = false;
    private bool isUnlocked = false;
    private Vector3 originalScale;
    private Color originalColor;

    void Start()
    {
        InitializeClickHandler();
    }

    void InitializeClickHandler()
    {
        // 如果没有指定Image组件，尝试获取
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }

        // 设置初始状态
        originalScale = transform.localScale;

        if (iconImage != null)
        {
            originalColor = iconImage.color;
        }
        else
        {
            originalColor = normalColor;
        }

        // 设置点击检测
        SetupClickDetection();
    }

    void SetupClickDetection()
    {
        // 确保有Image组件
        if (iconImage == null)
        {
            iconImage = gameObject.AddComponent<Image>();
        }

        // 设置Image为可点击
        iconImage.raycastTarget = true;

        // 如果使用像素级检测，设置透明度阈值
        if (usePixelPerfectDetection && iconImage.sprite != null)
        {
            // 创建带有alpha通道的材质
            Material alphaMaterial = new Material(Shader.Find("UI/Default"));
            alphaMaterial.SetFloat("_AlphaThreshold", alphaThreshold);
            iconImage.material = alphaMaterial;
        }
    }

    /// <summary>
    /// 检查点击位置是否在非透明区域
    /// </summary>
    private bool IsClickValid(PointerEventData eventData)
    {
        if (iconImage == null || iconImage.sprite == null)
        {
            return true; // 如果没有sprite，默认接受所有点击
        }

        // 获取点击位置在Image中的本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            iconImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // 转换为Sprite的UV坐标
        Rect spriteRect = iconImage.sprite.rect;
        Vector2 uv = new Vector2(
            (localPoint.x - iconImage.rectTransform.rect.x) / iconImage.rectTransform.rect.width,
            (localPoint.y - iconImage.rectTransform.rect.y) / iconImage.rectTransform.rect.height
        );

        // 检查是否在Sprite范围内
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
        {
            return false;
        }

        // 获取像素颜色
        Texture2D texture = iconImage.sprite.texture;
        int pixelX = Mathf.FloorToInt(uv.x * spriteRect.width);
        int pixelY = Mathf.FloorToInt(uv.y * spriteRect.height);

        // 确保坐标在纹理范围内
        pixelX = Mathf.Clamp(pixelX, 0, texture.width - 1);
        pixelY = Mathf.Clamp(pixelY, 0, texture.height - 1);

        // 检查像素透明度
        Color pixelColor = texture.GetPixel(pixelX, pixelY);
        return pixelColor.a > alphaThreshold;
    }

    /// <summary>
    /// 点击事件处理
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUnlocked)
        {
            Debug.Log($"应用 {appName} 已锁定");
            return;
        }

        // 如果使用像素级检测，验证点击位置
        if (usePixelPerfectDetection && !IsClickValid(eventData))
        {
            return; // 点击了透明区域，忽略
        }

        Debug.Log($"点击了应用: {appName}");
        OnIconClicked();
    }

    /// <summary>
    /// 鼠标进入事件
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isUnlocked)
        {
            return;
        }

        // 如果使用像素级检测，验证鼠标位置
        if (usePixelPerfectDetection && !IsClickValid(eventData))
        {
            return;
        }

        isHovering = true;
        StartCoroutine(HoverAnimation(true));
    }

    /// <summary>
    /// 鼠标离开事件
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        StartCoroutine(HoverAnimation(false));
    }

    /// <summary>
    /// 悬停动画
    /// </summary>
    private IEnumerator HoverAnimation(bool hoverIn)
    {
        Vector3 targetScale = hoverIn ? originalScale * hoverScale : originalScale;
        Color targetColor = hoverIn ? (isUnlocked ? hoverColor : normalColor) : (isUnlocked ? originalColor : normalColor);

        Vector3 startScale = transform.localScale;
        Color startColor = iconImage != null ? iconImage.color : Color.white;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (iconImage != null)
            {
                iconImage.color = Color.Lerp(startColor, targetColor, t);
            }

            yield return null;
        }

        transform.localScale = targetScale;

        if (iconImage != null)
        {
            iconImage.color = targetColor;
        }
    }

    /// <summary>
    /// 图标点击事件
    /// </summary>
    private void OnIconClicked()
    {
        // 通知AppManager
        if (GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.OnAppIconClicked(appId);
        }
    }

    /// <summary>
    /// 设置解锁状态
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        UpdateVisualState();
    }

    /// <summary>
    /// 更新视觉状态
    /// </summary>
    private void UpdateVisualState()
    {
        if (iconImage != null)
        {
            iconImage.color = isUnlocked ? originalColor : normalColor;
        }
    }

    /// <summary>
    ///设置图标信息
    /// </summary>
    public void SetIconInfo(string id, string name)
    {
        appId = id;
        appName = name;
    }

    /// <summary>
    ///强制设置颜色
    /// </summary>
    public void SetColor(Color color)
    {
        if (iconImage != null)
        {
            iconImage.color = color;
        }
        originalColor = color;
    }

    /// <summary>
    /// 播放点击音效
    /// </summary>
    public void PlayClickSound()
    {
        // 这里可以添加音效播放逻辑
        Debug.Log($"播放 {appName} 点击音效");
    }

    void OnDestroy()
    {
        // 清理协程
        StopAllCoroutines();
    }
}