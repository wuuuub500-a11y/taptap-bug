using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 日历应用 - 显示2013/2014/2015三年的日历
/// 功能: 关闭窗口、上一年、下一年
/// </summary>
public class CalendarApp : MonoBehaviour
{
    [Header("主要UI组件")]
    public Button closeButton;          // 关闭按钮
    public Button previousYearButton;   // 上一年按钮
    public Button nextYearButton;       // 下一年按钮

    [Header("年份日历图片")]
    public GameObject calendar2013;     // 2013年日历图片
    public GameObject calendar2014;     // 2014年日历图片
    public GameObject calendar2015;     // 2015年日历图片

    // 当前显示的年份
    private int currentYear = 2015;     // 默认显示2015年

    void Start()
    {
        SetupButtons();
        ShowYear(currentYear);
    }

    /// <summary>
    /// 绑定所有按钮事件
    /// </summary>
    void SetupButtons()
    {
        // 关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCalendar);
        }

        // 上一年按钮
        if (previousYearButton != null)
        {
            previousYearButton.onClick.AddListener(ShowPreviousYear);
        }

        // 下一年按钮
        if (nextYearButton != null)
        {
            nextYearButton.onClick.AddListener(ShowNextYear);
        }
    }

    /// <summary>
    /// 关闭日历窗口
    /// </summary>
    void CloseCalendar()
    {
        Debug.Log("关闭日历窗口");
        gameObject.SetActive(false);

        // 通知AppManager
        if (GameManager.Instance != null && GameManager.Instance.appManager != null)
        {
            GameManager.Instance.appManager.currentActiveWindow = null;
        }
    }

    /// <summary>
    /// 显示上一年
    /// </summary>
    void ShowPreviousYear()
    {
        // 2013 -> 2015 -> 2014 -> 2013 (循环)
        currentYear--;
        if (currentYear < 2013)
        {
            currentYear = 2015;
        }

        ShowYear(currentYear);
        Debug.Log($"切换到 {currentYear} 年");
    }

    /// <summary>
    /// 显示下一年
    /// </summary>
    void ShowNextYear()
    {
        // 2013 -> 2014 -> 2015 -> 2013 (循环)
        currentYear++;
        if (currentYear > 2015)
        {
            currentYear = 2013;
        }

        ShowYear(currentYear);
        Debug.Log($"切换到 {currentYear} 年");
    }

    /// <summary>
    /// 显示指定年份的日历
    /// </summary>
    void ShowYear(int year)
    {
        // 隐藏所有年份
        if (calendar2013 != null) calendar2013.SetActive(false);
        if (calendar2014 != null) calendar2014.SetActive(false);
        if (calendar2015 != null) calendar2015.SetActive(false);

        // 显示指定年份
        switch (year)
        {
            case 2013:
                if (calendar2013 != null) calendar2013.SetActive(true);
                break;
            case 2014:
                if (calendar2014 != null) calendar2014.SetActive(true);
                break;
            case 2015:
                if (calendar2015 != null) calendar2015.SetActive(true);
                break;
            default:
                Debug.LogWarning($"不支持的年份: {year}");
                break;
        }
    }
}
