using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 桌面点击检测器，处理不规则图标的点击检测
/// </summary>
public class DesktopClickDetector : MonoBehaviour, IPointerClickHandler
{
    [System.Serializable]
    public class IconHotspot
    {
        public string appId;
        public RectTransform iconRect;
        public Image iconImage;
        public float alphaThreshold = 0.1f;
        public Rect clickArea; // 手动设置的点击区域
        public bool useCustomArea = false;
    }

    [Header("图标热点配置")]
    public List<IconHotspot> iconHotspots = new List<IconHotspot>();

    [Header("调试设置")]
    public bool showDebugInfo = false;
    public bool drawClickAreas = false;

    void Start()
    {
        // 如果没有配置图标，尝试自动查找
        if (iconHotspots.Count == 0)
        {
            AutoDetectIcons();
        }
    }

    void AutoDetectIcons()
    {
        // 查找所有带有IrregularIconClickHandler的图标
        IrregularIconClickHandler[] iconHandlers = FindObjectsOfType<IrregularIconClickHandler>();

        foreach (var handler in iconHandlers)
        {
            if (handler.iconImage != null)
            {
                IconHotspot hotspot = new IconHotspot
                {
                    appId = handler.appId,
                    iconRect = handler.iconImage.rectTransform,
                    iconImage = handler.iconImage,
                    alphaThreshold = 0.1f,
                    useCustomArea = false
                };

                // 计算图标的大致点击区域
                hotspot.clickArea = new Rect(
                    -hotspot.iconRect.rect.width / 2,
                    -hotspot.iconRect.rect.height / 2,
                    hotspot.iconRect.rect.width,
                    hotspot.iconRect.rect.height
                );

                iconHotspots.Add(hotspot);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"自动检测到 {iconHotspots.Count} 个图标热点");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        if (showDebugInfo)
        {
            Debug.Log($"桌面点击位置: {localPoint}");
        }

        // 检查每个图标热点
        foreach (var hotspot in iconHotspots)
        {
            if (IsIconClicked(hotspot, localPoint, eventData))
            {
                // 通知AppManager
                if (GameManager.Instance.appManager != null)
                {
                    GameManager.Instance.appManager.OnAppIconClicked(hotspot.appId);
                }
                return; // 找到匹配的图标，停止检测
            }
        }
    }

    bool IsIconClicked(IconHotspot hotspot, Vector2 clickPoint, PointerEventData eventData)
    {
        // 转换点击点到图标的本地坐标系
        Vector2 iconLocalPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            hotspot.iconRect,
            eventData.position,
            eventData.pressEventCamera,
            out iconLocalPoint
        );

        // 检查是否在点击区域内
        if (hotspot.useCustomArea)
        {
            // 使用自定义矩形区域
            if (!hotspot.clickArea.Contains(iconLocalPoint))
            {
                return false;
            }
        }
        else
        {
            // 使用图标Rect范围
            Rect iconBounds = new Rect(
                -hotspot.iconRect.rect.width / 2,
                -hotspot.iconRect.rect.height / 2,
                hotspot.iconRect.rect.width,
                hotspot.iconRect.rect.height
            );

            if (!iconBounds.Contains(iconLocalPoint))
            {
                return false;
            }
        }

        // 如果有sprite，进行像素级检测
        if (hotspot.iconImage != null && hotspot.iconImage.sprite != null)
        {
            return IsPixelOpaque(hotspot, iconLocalPoint);
        }

        return true; // 没有sprite，使用区域检测
    }

    bool IsPixelOpaque(IconHotspot hotspot, Vector2 localPoint)
    {
        if (hotspot.iconImage.sprite == null) return true;

        // 转换为UV坐标
        Rect spriteRect = hotspot.iconImage.sprite.rect;
        Vector2 uv = new Vector2(
            (localPoint.x + hotspot.iconRect.rect.width / 2) / hotspot.iconRect.rect.width,
            (localPoint.y + hotspot.iconRect.rect.height / 2) / hotspot.iconRect.rect.height
        );

        // 检查边界
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
        {
            return false;
        }

        // 获取像素
        Texture2D texture = hotspot.iconImage.sprite.texture;
        int pixelX = Mathf.FloorToInt(uv.x * spriteRect.width);
        int pixelY = Mathf.FloorToInt(uv.y * spriteRect.height);

        pixelX = Mathf.Clamp(pixelX, 0, texture.width - 1);
        pixelY = Mathf.Clamp(pixelY, 0, texture.height - 1);

        Color pixelColor = texture.GetPixel(pixelX, pixelY);
        return pixelColor.a > hotspot.alphaThreshold;
    }

    /// <summary>
    /// 手动添加图标热点
    /// </summary>
    public void AddIconHotspot(string appId, RectTransform iconRect, Image iconImage)
    {
        IconHotspot hotspot = new IconHotspot
        {
            appId = appId,
            iconRect = iconRect,
            iconImage = iconImage,
            alphaThreshold = 0.1f,
            useCustomArea = false
        };

        hotspot.clickArea = new Rect(
            -iconRect.rect.width / 2,
            -iconRect.rect.height / 2,
            iconRect.rect.width,
            iconRect.rect.height
        );

        iconHotspots.Add(hotspot);

        if (showDebugInfo)
        {
            Debug.Log($"添加图标热点: {appId}");
        }
    }

    /// <summary>
    /// 设置自定义点击区域
    /// </summary>
    public void SetCustomClickArea(string appId, Rect area)
    {
        foreach (var hotspot in iconHotspots)
        {
            if (hotspot.appId == appId)
            {
                hotspot.clickArea = area;
                hotspot.useCustomArea = true;
                return;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawClickAreas || !Application.isPlaying) return;

        foreach (var hotspot in iconHotspots)
        {
            if (hotspot.iconRect != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(hotspot.iconRect.position, hotspot.iconRect.sizeDelta);
            }
        }
    }
}