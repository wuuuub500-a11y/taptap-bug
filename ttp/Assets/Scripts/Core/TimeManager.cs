using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("时间显示")]
    public TMPro.TextMeshProUGUI dateText;
    public TMPro.TextMeshProUGUI timeText;

    [Header("时间设置")]
    [Range(1, 31)]
    public int currentDay = 1;
    [Range(1, 12)]
    public int currentMonth = 10;
    public int currentYear = 2024;

    private void Start()
    {
        UpdateTimeDisplay();
    }

    /// <summary>
    /// 更新时间显示
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (dateText != null)
        {
            dateText.text = $"{currentYear}年{currentMonth:00}月{currentDay:00}日";
        }

        if (timeText != null)
        {
            timeText.text = "00:00"; // 剧情时间不需要具体时间
        }
    }

    /// <summary>
    /// 前进一天
    /// </summary>
    public void AdvanceDay()
    {
        SyncWithStoryDay(currentDay + 1);
    }

    public void SyncWithStoryDay(int storyDay)
    {
        int daysInMonth = GetDaysInMonth(currentMonth, currentYear);
        currentDay = Mathf.Clamp(storyDay, 1, daysInMonth);

        UpdateTimeDisplay();
    }

    /// <summary>
    /// 获取指定月份的天数
    /// </summary>
    private int GetDaysInMonth(int month, int year)
    {
        switch (month)
        {
            case 4: case 6: case 9: case 11:
                return 30;
            case 2:
                return IsLeapYear(year) ? 29 : 28;
            default:
                return 31;
        }
    }

    /// <summary>
    /// 判断是否为闰年
    /// </summary>
    private bool IsLeapYear(int year)
    {
        return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
    }

    /// <summary>
    /// 设置当前日期
    /// </summary>
    public void SetDate(int year, int month, int day)
    {
        currentYear = year;
        currentMonth = Mathf.Clamp(month, 1, 12);
        currentDay = Mathf.Clamp(day, 1, GetDaysInMonth(currentMonth, currentYear));
        UpdateTimeDisplay();
    }

    /// <summary>
    /// 获取当前日期字符串
    /// </summary>
    public string GetCurrentDateString()
    {
        return $"{currentYear}年{currentMonth:00}月{currentDay:00}日";
    }

    /// <summary>
    /// 获取剩余天数
    /// </summary>
    public int GetRemainingDays(int totalDays = 7)
    {
        return totalDays - currentDay + 1;
    }
}