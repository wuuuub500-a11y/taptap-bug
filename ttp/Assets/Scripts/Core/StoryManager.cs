using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 剧情管理器 - 管理游戏的章节、剧情事件和触发条件
/// </summary>
public class StoryManager : MonoBehaviour
{
    [System.Serializable]
    public class StoryEvent
    {
        public string eventId;
        public string eventName;
        public int triggerDay;  // 触发日期
        public List<string> requiredClues = new List<string>();  // 需要的线索
        public List<string> requiredApps = new List<string>();   // 需要解锁的应用
        public bool isTriggered = false;
        public System.Action onTrigger;  // 触发时执行的行为
    }

    [Header("章节设置")]
    public int currentChapter = 1;
    public int currentDay = 1;
    public int totalDays = 7;

    [Header("剧情事件")]
    public List<StoryEvent> storyEvents = new List<StoryEvent>();

    private Dictionary<string, StoryEvent> eventDictionary = new Dictionary<string, StoryEvent>();
    private List<string> completedEvents = new List<string>();

    void Start()
    {
        InitializeStoryEvents();
        LoadStoryProgress();
    }

    void InitializeStoryEvents()
    {
        // 初始化事件字典
        foreach (var storyEvent in storyEvents)
        {
            eventDictionary[storyEvent.eventId] = storyEvent;
        }

        // 定义第一天的事件
        AddStoryEvent(new StoryEvent
        {
            eventId = "day1_intro",
            eventName = "序章：发现电脑",
            triggerDay = 1,
            requiredClues = new List<string>(),
            requiredApps = new List<string>()
        });

        // 定义第一天晚上的事件
        AddStoryEvent(new StoryEvent
        {
            eventId = "day1_bug_angel",
            eventName = "遇到Bug天使",
            triggerDay = 1,
            requiredClues = new List<string> { "blog_password", "chat_opened" },
            requiredApps = new List<string> { "blog", "chat" }
        });

        // Day 2-3 事件
        AddStoryEvent(new StoryEvent
        {
            eventId = "day2_hidden_chat",
            eventName = "解锁隐藏聊天",
            triggerDay = 2,
            requiredClues = new List<string> { "questionnaire_completed" },
            requiredApps = new List<string> { "chat", "questionnaire" }
        });

        // Day 4-6 事件
        AddStoryEvent(new StoryEvent
        {
            eventId = "day4_deep_investigation",
            eventName = "深入调查",
            triggerDay = 4,
            requiredClues = new List<string> { "relationship_info", "work_info" },
            requiredApps = new List<string> { "album", "browser" }
        });

        // Day 7 结局事件
        AddStoryEvent(new StoryEvent
        {
            eventId = "day7_ending_good",
            eventName = "好结局",
            triggerDay = 7,
            requiredClues = new List<string> { "all_clues_found" },
            requiredApps = new List<string>()
        });

        AddStoryEvent(new StoryEvent
        {
            eventId = "day7_ending_bad",
            eventName = "坏结局",
            triggerDay = 7,
            requiredClues = new List<string>(),
            requiredApps = new List<string>()
        });

        AddStoryEvent(new StoryEvent
        {
            eventId = "day7_ending_hidden",
            eventName = "隐藏结局（真相）",
            triggerDay = 7,
            requiredClues = new List<string> { "all_clues_found", "bug_angel_secret" },
            requiredApps = new List<string> { "bugapp" }
        });

        Debug.Log($"初始化了 {eventDictionary.Count} 个剧情事件");
    }

    void AddStoryEvent(StoryEvent storyEvent)
    {
        if (!eventDictionary.ContainsKey(storyEvent.eventId))
        {
            storyEvents.Add(storyEvent);
            eventDictionary[storyEvent.eventId] = storyEvent;
        }
    }

    void LoadStoryProgress()
    {
        // 从数据管理器加载剧情进度
        var saveData = GameManager.Instance.dataManager?.GetSaveData();
        if (saveData != null)
        {
            currentChapter = saveData.currentChapter;
            currentDay = saveData.currentDay;
        }

        UpdateChapterBasedOnDay();
        SyncTimeManager();
    }

    void SyncTimeManager()
    {
        if (GameManager.Instance != null && GameManager.Instance.timeManager != null)
        {
            GameManager.Instance.timeManager.SyncWithStoryDay(currentDay);
        }
    }

    bool SetDayInternal(int targetDay, bool autoUpdateChapter)
    {
        int clampedDay = Mathf.Clamp(targetDay, 1, totalDays);
        if (currentDay == clampedDay)
        {
            return false;
        }

        currentDay = clampedDay;

        if (autoUpdateChapter)
        {
            UpdateChapterBasedOnDay();
        }

        SyncTimeManager();
        return true;
    }

    bool SetChapterInternal(int targetChapter)
    {
        int clampedChapter = Mathf.Max(1, targetChapter);
        if (currentChapter == clampedChapter)
        {
            return false;
        }

        currentChapter = clampedChapter;
        return true;
    }

    public void RequestAdvanceDay(string reason = "ExternalAdvance", bool autoSave = true)
    {
        bool changed = SetDayInternal(currentDay + 1, true);
        if (changed && autoSave)
        {
            SaveStoryProgress(reason);
        }
    }

    public void RequestSetDay(int targetDay, string reason = "ExternalSetDay", bool autoSave = true, bool autoUpdateChapter = true)
    {
        bool changed = SetDayInternal(targetDay, autoUpdateChapter);
        if (changed && autoSave)
        {
            SaveStoryProgress(reason);
        }
    }

    public void RequestSetChapter(int targetChapter, string reason = "ExternalSetChapter", bool autoSave = true)
    {
        bool changed = SetChapterInternal(targetChapter);
        if (changed && autoSave)
        {
            SaveStoryProgress(reason);
        }
    }

    public void RequestChapterProgress(int targetChapter, bool advanceDay, string reason = "ExternalChapterProgress", bool autoSave = true)
    {
        bool changed = false;

        if (advanceDay)
        {
            changed |= SetDayInternal(currentDay + 1, false);
        }

        changed |= SetChapterInternal(targetChapter);

        if (changed && autoSave)
        {
            SaveStoryProgress(reason);
        }
    }

    /// <summary>
    /// 推进到下一天
    /// </summary>
    public void AdvanceToNextDay(string reason = "StoryAdvance")
    {
        if (currentDay >= totalDays)
        {
            Debug.Log("游戏结束，触发结局");
            TriggerEnding();
            return;
        }

        bool dayChanged = SetDayInternal(currentDay + 1, true);
        if (dayChanged)
        {
            SaveStoryProgress(reason);
        }

        // 触发当天的事件
        CheckAndTriggerDayEvents();

        Debug.Log($"进入第 {currentDay} 天，第 {currentChapter} 章");
    }

    /// <summary>
    /// 根据天数更新章节
    /// </summary>
    void UpdateChapterBasedOnDay()
    {
        if (currentDay == 1)
        {
            currentChapter = 1;  // 第一章
        }
        else if (currentDay >= 2 && currentDay <= 3)
        {
            currentChapter = 2;  // 第二章
        }
        else if (currentDay >= 4 && currentDay <= 6)
        {
            currentChapter = 3;  // 第三章
        }
        else if (currentDay == 7)
        {
            currentChapter = 4;  // 结局章节
        }
    }

    /// <summary>
    /// 检查并触发当天的事件
    /// </summary>
    void CheckAndTriggerDayEvents()
    {
        foreach (var storyEvent in storyEvents)
        {
            if (storyEvent.triggerDay == currentDay && !storyEvent.isTriggered)
            {
                // 检查是否满足触发条件
                if (CheckEventConditions(storyEvent))
                {
                    TriggerEvent(storyEvent.eventId);
                }
            }
        }
    }

    /// <summary>
    /// 检查事件触发条件
    /// </summary>
    bool CheckEventConditions(StoryEvent storyEvent)
    {
        // 检查所需线索
        if (storyEvent.requiredClues != null && storyEvent.requiredClues.Count > 0)
        {
            foreach (var clue in storyEvent.requiredClues)
            {
                if (!GameManager.Instance.dataManager.IsPasswordSolved(clue))
                {
                    return false;
                }
            }
        }

        // 检查所需应用
        if (storyEvent.requiredApps != null && storyEvent.requiredApps.Count > 0)
        {
            var saveData = GameManager.Instance.dataManager.GetSaveData();
            if (saveData != null && saveData.unlockedApps != null)
            {
                foreach (var app in storyEvent.requiredApps)
                {
                    if (!saveData.unlockedApps.Contains(app))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    public void TriggerEvent(string eventId)
    {
        if (!eventDictionary.ContainsKey(eventId))
        {
            Debug.LogWarning($"事件 {eventId} 不存在！");
            return;
        }

        StoryEvent storyEvent = eventDictionary[eventId];

        if (storyEvent.isTriggered)
        {
            Debug.LogWarning($"事件 {eventId} 已经触发过了！");
            return;
        }

        storyEvent.isTriggered = true;
        completedEvents.Add(eventId);

        Debug.Log($"触发事件: {storyEvent.eventName}");

        // 执行事件回调
        storyEvent.onTrigger?.Invoke();

        // 显示事件提示
        GameManager.Instance.uiManager?.ShowMessage($"剧情事件：{storyEvent.eventName}");

        // 保存事件状态
        SaveStoryProgress($"TriggerEvent:{eventId}");
    }

    /// <summary>
    /// 触发结局
    /// </summary>
    void TriggerEnding()
    {
        // 根据收集的线索和完成的事件判断结局
        var clueManager = GameManager.Instance.GetComponent<ClueManager>();

        if (clueManager != null)
        {
            int totalClues = clueManager.GetTotalCluesCount();
            int foundClues = clueManager.GetFoundCluesCount();

            // 隐藏结局：所有线索 + Bug天使秘密
            if (foundClues == totalClues && completedEvents.Contains("bug_angel_secret"))
            {
                TriggerEvent("day7_ending_hidden");
            }
            // 好结局：大部分线索
            else if (foundClues >= totalClues * 0.7f)
            {
                TriggerEvent("day7_ending_good");
            }
            // 坏结局：线索不足
            else
            {
                TriggerEvent("day7_ending_bad");
            }
        }
        else
        {
            // 默认坏结局
            TriggerEvent("day7_ending_bad");
        }
    }

    /// <summary>
    /// 保存剧情进度
    /// </summary>
    void SaveStoryProgress(string reason = null)
    {
        var saveData = GameManager.Instance.dataManager?.GetSaveData();
        if (saveData != null)
        {
            saveData.currentChapter = currentChapter;
            saveData.currentDay = currentDay;
            GameManager.Instance.dataManager.SaveGameData();

            if (!string.IsNullOrEmpty(reason))
            {
                Debug.Log($"保存剧情进度（原因：{reason}） -> 第 {currentChapter} 章，第 {currentDay} 天");
            }
        }
    }

    /// <summary>
    /// 获取当前章节名称
    /// </summary>
    public string GetChapterName()
    {
        switch (currentChapter)
        {
            case 1: return "第一章：初识";
            case 2: return "第二章：探索";
            case 3: return "第三章：深入";
            case 4: return "结局";
            default: return "未知章节";
        }
    }

    /// <summary>
    /// 获取当前天数
    /// </summary>
    public int GetCurrentDay()
    {
        return currentDay;
    }

    /// <summary>
    /// 获取剩余天数
    /// </summary>
    public int GetRemainingDays()
    {
        return totalDays - currentDay + 1;
    }

    /// <summary>
    /// 检查事件是否已完成
    /// </summary>
    public bool IsEventCompleted(string eventId)
    {
        return completedEvents.Contains(eventId);
    }
}
