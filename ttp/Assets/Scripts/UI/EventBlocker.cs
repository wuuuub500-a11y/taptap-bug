using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 用于控制UI事件拦截的组件
/// </summary>
public class EventBlocker : MonoBehaviour, IPointerClickHandler, ICanvasRaycastFilter
{
    [Header("事件拦截设置")]
    public bool blockEvents = false;
    public LayerMask blockLayer;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果设置为拦截事件，则不处理点击
        if (blockEvents)
        {
            eventData.Use();
        }
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // 如果设置为拦截事件，则不允许射线穿透
        return !blockEvents;
    }

    /// <summary>
    /// 设置事件拦截状态
    /// </summary>
    public void SetBlockEvents(bool block)
    {
        blockEvents = block;
    }
}