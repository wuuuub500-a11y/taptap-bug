using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 线索管理器 - 管理游戏中的所有线索收集和推理
/// </summary>
public class ClueManager : MonoBehaviour
{
    [System.Serializable]
    public class Clue
    {
        public string clueId;
        public string clueName;
        public string clueDescription;
        public string clueType;  // password, date, relationship, work等
        public string clueValue;  // 线索的具体值
        public string source;  // 来源（blog, chat, album等）
        public bool isFound = false;
        public string foundDate;  // 发现日期
    }

    [Header("线索列表")]
    public List<Clue> allClues = new List<Clue>();

    private Dictionary<string, Clue> clueDictionary = new Dictionary<string, Clue>();
    private List<Clue> foundClues = new List<Clue>();

    void Start()
    {
        InitializeClues();
        LoadClueProgress();
    }

    void InitializeClues()
    {
        // 定义所有线索
        AddClue(new Clue
        {
            clueId = "blog_password",
            clueName = "博客密码线索",
            clueDescription = "在博客《新的开始》中发现：2557176",
            clueType = "password",
            clueValue = "2557176",
            source = "blog"
        });

        AddClue(new Clue
        {
            clueId = "relationship_anniversary",
            clueName = "恋爱纪念日",
            clueDescription = "在博客《约会纪念》中发现：2015214",
            clueType = "date",
            clueValue = "2015214",
            source = "blog"
        });

        AddClue(new Clue
        {
            clueId = "favorite_gift",
            clueName = "最喜欢的礼物",
            clueDescription = "在博客中发现：手表",
            clueType = "relationship",
            clueValue = "watch",
            source = "blog"
        });

        AddClue(new Clue
        {
            clueId = "chat_password",
            clueName = "聊天软件密码",
            clueDescription = "需要解密：132199",
            clueType = "password",
            clueValue = "132199",
            source = "unknown"
        });

        AddClue(new Clue
        {
            clueId = "questionnaire_link",
            clueName = "问卷链接",
            clueDescription = "关钦发送的问卷链接",
            clueType = "relationship",
            clueValue = "questionnaire_guanqin",
            source = "chat"
        });

        AddClue(new Clue
        {
            clueId = "company_website",
            clueName = "公司网站",
            clueDescription = "老板发送的公司网站：https://.syxtyulegongsi.com",
            clueType = "work",
            clueValue = "syxtyulegongsi.com",
            source = "chat"
        });

        AddClue(new Clue
        {
            clueId = "hidden_chat_proposal",
            clueName = "隐藏的求婚聊天",
            clueDescription = "关钦提议结婚，主角犹豫",
            clueType = "relationship",
            clueValue = "marriage_proposal",
            source = "chat_hidden"
        });

        AddClue(new Clue
        {
            clueId = "friend_xiaofu",
            clueName = "朋友小付",
            clueDescription = "一起拍照的朋友",
            clueType = "relationship",
            clueValue = "xiaofu",
            source = "blog"
        });

        AddClue(new Clue
        {
            clueId = "work_start_date",
            clueName = "工作开始日期",
            clueDescription = "2014-03-15开始实习",
            clueType = "work",
            clueValue = "2014-03-15",
            source = "blog"
        });

        AddClue(new Clue
        {
            clueId = "internet_celebrity",
            clueName = "网红身份",
            clueDescription = "主角是网红",
            clueType = "identity",
            clueValue = "internet_celebrity",
            source = "blog"
        });

        Debug.Log($"初始化了 {allClues.Count} 个线索");
    }

    void AddClue(Clue clue)
    {
        if (!clueDictionary.ContainsKey(clue.clueId))
        {
            allClues.Add(clue);
            clueDictionary[clue.clueId] = clue;
        }
    }

    void LoadClueProgress()
    {
        // 从数据管理器加载线索进度
        // TODO: 实现从存档加载
    }

    /// <summary>
    /// 发现线索
    /// </summary>
    public void DiscoverClue(string clueId)
    {
        if (!clueDictionary.ContainsKey(clueId))
        {
            Debug.LogWarning($"线索 {clueId} 不存在！");
            return;
        }

        Clue clue = clueDictionary[clueId];

        if (clue.isFound)
        {
            Debug.LogWarning($"线索 {clue.clueName} 已经发现过了！");
            return;
        }

        clue.isFound = true;
        clue.foundDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        foundClues.Add(clue);

        Debug.Log($"发现线索: {clue.clueName} - {clue.clueDescription}");

        // 显示线索提示
        GameManager.Instance.uiManager?.ShowMessage($"发现线索：{clue.clueName}");

        // 自动记录到记事本
        if (GameManager.Instance.dataManager != null)
        {
            GameManager.Instance.dataManager.AddPlayerNote($"[线索] {clue.clueName}: {clue.clueDescription}");
        }

        // 保存线索状态
        SaveClueProgress();

        // 检查是否触发新事件
        CheckClueEvents();
    }

    /// <summary>
    /// 检查线索相关事件
    /// </summary>
    void CheckClueEvents()
    {
        // 检查是否收集到足够的线索触发新事件
        int foundCount = foundClues.Count;
        int totalCount = allClues.Count;

        // 收集50%线索时解锁提示
        if (foundCount >= totalCount * 0.5f && foundCount < totalCount * 0.51f)
        {
            GameManager.Instance.uiManager?.ShowMessage("已收集一半的线索，继续探索！");
        }

        // 收集所有线索时
        if (foundCount == totalCount)
        {
            GameManager.Instance.uiManager?.ShowMessage("所有线索已收集完毕！");
            var storyManager = GameManager.Instance.GetComponent<StoryManager>();
            if (storyManager != null)
            {
                GameManager.Instance.dataManager?.MarkPasswordSolved("all_clues_found");
            }
        }
    }

    /// <summary>
    /// 根据来源发现线索
    /// </summary>
    public void DiscoverClueBySource(string source, string clueValue)
    {
        foreach (var clue in allClues)
        {
            if (clue.source == source && clue.clueValue == clueValue && !clue.isFound)
            {
                DiscoverClue(clue.clueId);
                return;
            }
        }
    }

    /// <summary>
    /// 获取已发现的线索列表
    /// </summary>
    public List<Clue> GetFoundClues()
    {
        return new List<Clue>(foundClues);
    }

    /// <summary>
    /// 获取所有线索列表
    /// </summary>
    public List<Clue> GetAllClues()
    {
        return new List<Clue>(allClues);
    }

    /// <summary>
    /// 检查线索是否已发现
    /// </summary>
    public bool IsClueFound(string clueId)
    {
        if (clueDictionary.ContainsKey(clueId))
        {
            return clueDictionary[clueId].isFound;
        }
        return false;
    }

    /// <summary>
    /// 获取已发现线索数量
    /// </summary>
    public int GetFoundCluesCount()
    {
        return foundClues.Count;
    }

    /// <summary>
    /// 获取总线索数量
    /// </summary>
    public int GetTotalCluesCount()
    {
        return allClues.Count;
    }

    /// <summary>
    /// 获取线索进度百分比
    /// </summary>
    public float GetClueProgress()
    {
        if (allClues.Count == 0) return 0f;
        return (float)foundClues.Count / allClues.Count * 100f;
    }

    /// <summary>
    /// 根据类型获取线索
    /// </summary>
    public List<Clue> GetCluesByType(string clueType)
    {
        List<Clue> cluesByType = new List<Clue>();
        foreach (var clue in foundClues)
        {
            if (clue.clueType == clueType)
            {
                cluesByType.Add(clue);
            }
        }
        return cluesByType;
    }

    /// <summary>
    /// 根据来源获取线索
    /// </summary>
    public List<Clue> GetCluesBySource(string source)
    {
        List<Clue> cluesBySource = new List<Clue>();
        foreach (var clue in foundClues)
        {
            if (clue.source == source)
            {
                cluesBySource.Add(clue);
            }
        }
        return cluesBySource;
    }

    /// <summary>
    /// 保存线索进度
    /// </summary>
    void SaveClueProgress()
    {
        // TODO: 实现线索进度保存
        GameManager.Instance.dataManager?.SaveGameData();
    }

    /// <summary>
    /// 获取线索详情
    /// </summary>
    public Clue GetClue(string clueId)
    {
        if (clueDictionary.ContainsKey(clueId))
        {
            return clueDictionary[clueId];
        }
        return null;
    }

    /// <summary>
    /// 生成线索摘要（用于UI显示）
    /// </summary>
    public string GetClueSummary()
    {
        string summary = $"已收集线索: {foundClues.Count}/{allClues.Count}\n\n";

        // 按类型分组显示
        var passwordClues = GetCluesByType("password");
        var dateClues = GetCluesByType("date");
        var relationshipClues = GetCluesByType("relationship");
        var workClues = GetCluesByType("work");

        if (passwordClues.Count > 0)
        {
            summary += "【密码线索】\n";
            foreach (var clue in passwordClues)
            {
                summary += $"- {clue.clueName}: {clue.clueValue}\n";
            }
            summary += "\n";
        }

        if (dateClues.Count > 0)
        {
            summary += "【日期线索】\n";
            foreach (var clue in dateClues)
            {
                summary += $"- {clue.clueName}: {clue.clueValue}\n";
            }
            summary += "\n";
        }

        if (relationshipClues.Count > 0)
        {
            summary += "【关系线索】\n";
            foreach (var clue in relationshipClues)
            {
                summary += $"- {clue.clueName}\n";
            }
            summary += "\n";
        }

        if (workClues.Count > 0)
        {
            summary += "【工作线索】\n";
            foreach (var clue in workClues)
            {
                summary += $"- {clue.clueName}\n";
            }
        }

        return summary;
    }
}
