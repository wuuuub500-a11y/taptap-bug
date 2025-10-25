using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    [Header("数据文件路径")]
    public string saveFileName = "saveData.json";
    public string appDataFileName = "appData.json";

    private GameSaveData gameSaveData;
    private AppDataConfig appDataConfig;

    void Start()
    {
        LoadGameData();
    }

    /// <summary>
    /// 加载游戏数据
    /// </summary>
    public void LoadGameData()
    {
        LoadSaveData();
        LoadAppData();
    }

    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public void SaveGameData()
    {
        SaveSaveData();
    }

    /// <summary>
    /// 加载存档数据
    /// </summary>
    private void LoadSaveData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            gameSaveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            Debug.Log("存档数据加载成功");
        }
        else
        {
            CreateNewSaveData();
            Debug.Log("创建新存档数据");
        }
    }

    /// <summary>
    /// 保存存档数据
    /// </summary>
    private void SaveSaveData()
    {
        if (gameSaveData == null)
        {
            gameSaveData = new GameSaveData();
        }

        string jsonData = JsonUtility.ToJson(gameSaveData, true);
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("存档数据保存成功");
    }

    /// <summary>
    /// 加载应用配置数据
    /// </summary>
    private void LoadAppData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Data", appDataFileName);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            appDataConfig = JsonUtility.FromJson<AppDataConfig>(jsonData);
            Debug.Log("应用配置数据加载成功");
        }
        else
        {
            Debug.LogError($"找不到应用配置文件: {filePath}");
            CreateDefaultAppData();
        }
    }

    /// <summary>
    /// 创建新存档数据
    /// </summary>
    private void CreateNewSaveData()
    {
        gameSaveData = new GameSaveData
        {
            currentChapter = 1,
            currentDay = 1,
            unlockedApps = new List<string> { "blog", "chat" }, // 初始解锁的应用
            playerNotes = new List<string>(),
            dialogueFlags = new Dictionary<string, bool>(),
            passwordSolved = new List<string>(),
            chatHistory = new Dictionary<string, List<ChatMessage>>(),
            blogPostsRead = new List<string>(),
            photosUnlocked = new List<string>(),
            browserHistory = new List<string>(),
            questionnaireCompleted = new List<string>(),
            videoCallTriggered = false
        };

        SaveSaveData();
    }

    /// <summary>
    /// 创建默认应用数据
    /// </summary>
    private void CreateDefaultAppData()
    {
        appDataConfig = new AppDataConfig
        {
            apps = new List<AppConfig>
            {
                new AppConfig { id = "blog", name = "博客", requiresPassword = false, password = "" },
                new AppConfig { id = "chat", name = "聊天软件", requiresPassword = true, password = "132199" },
                new AppConfig { id = "notebook", name = "记事本", requiresPassword = false, password = "" },
                new AppConfig { id = "browser", name = "浏览器", requiresPassword = false, password = "" },
                new AppConfig { id = "album", name = "生活相册", requiresPassword = false, password = "" },
                new AppConfig { id = "questionnaire", name = "好对象问卷", requiresPassword = false, password = "" },
                new AppConfig { id = "videocall", name = "Bug视频通话", requiresPassword = false, password = "" },
                new AppConfig { id = "calendar", name = "日历", requiresPassword = false, password = "" },
                new AppConfig { id = "unknown", name = "未知软件", requiresPassword = false, password = "" }
            }
        };

        // 创建StreamingAssets文件夹和Data文件夹
        string dataPath = Path.Combine(Application.streamingAssetsPath, "Data");
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        string jsonData = JsonUtility.ToJson(appDataConfig, true);
        string filePath = Path.Combine(dataPath, appDataFileName);
        File.WriteAllText(filePath, jsonData);
        Debug.Log("创建默认应用配置数据");
    }

    /// <summary>
    /// 获取存档数据
    /// </summary>
    public GameSaveData GetSaveData()
    {
        return gameSaveData;
    }

    /// <summary>
    /// 获取应用配置数据
    /// </summary>
    public AppDataConfig GetAppData()
    {
        return appDataConfig;
    }

    /// <summary>
    /// 添加玩家笔记
    /// </summary>
    public void AddPlayerNote(string note)
    {
        if (gameSaveData.playerNotes == null)
        {
            gameSaveData.playerNotes = new List<string>();
        }
        gameSaveData.playerNotes.Add(note);
        SaveSaveData();
    }

    /// <summary>
    /// 解锁应用
    /// </summary>
    public void UnlockApp(string appId)
    {
        if (gameSaveData.unlockedApps == null)
        {
            gameSaveData.unlockedApps = new List<string>();
        }

        if (!gameSaveData.unlockedApps.Contains(appId))
        {
            gameSaveData.unlockedApps.Add(appId);
            SaveSaveData();
            Debug.Log($"应用 {appId} 已解锁");
        }
    }

    /// <summary>
    /// 标记密码已解决
    /// </summary>
    public void MarkPasswordSolved(string passwordId)
    {
        if (gameSaveData.passwordSolved == null)
        {
            gameSaveData.passwordSolved = new List<string>();
        }

        if (!gameSaveData.passwordSolved.Contains(passwordId))
        {
            gameSaveData.passwordSolved.Add(passwordId);
            SaveSaveData();
            Debug.Log($"密码 {passwordId} 已标记为解决");
        }
    }

    /// <summary>
    /// 检查密码是否已解决
    /// </summary>
    public bool IsPasswordSolved(string passwordId)
    {
        return gameSaveData.passwordSolved != null && gameSaveData.passwordSolved.Contains(passwordId);
    }

    /// <summary>
    /// 标记问卷已完成
    /// </summary>
    public void MarkQuestionnaireCompleted(string contactId)
    {
        if (gameSaveData.questionnaireCompleted == null)
        {
            gameSaveData.questionnaireCompleted = new List<string>();
        }

        if (!gameSaveData.questionnaireCompleted.Contains(contactId))
        {
            gameSaveData.questionnaireCompleted.Add(contactId);
            SaveSaveData();
            Debug.Log($"问卷 {contactId} 已标记为完成");
        }
    }

    /// <summary>
    /// 检查问卷是否已完成
    /// </summary>
    public bool IsQuestionnaireCompleted(string contactId)
    {
        return gameSaveData.questionnaireCompleted != null && gameSaveData.questionnaireCompleted.Contains(contactId);
    }

    /// <summary>
    /// 标记视频通话已触发
    /// </summary>
    public void MarkVideoCallTriggered()
    {
        gameSaveData.videoCallTriggered = true;
        SaveSaveData();
        Debug.Log("视频通话已标记为触发");
    }

    /// <summary>
    /// 检查视频通话是否已触发
    /// </summary>
    public bool IsVideoCallTriggered()
    {
        if (gameSaveData == null)
        {
            Debug.LogWarning("尝试查询视频通话状态时存档数据尚未加载，默认返回未触发。");
            return false;
        }

        return gameSaveData.videoCallTriggered;
    }

    /// <summary>
    /// 获取布尔状态值
    /// </summary>
    public bool GetBool(string key, bool defaultValue = false)
    {
        if (string.IsNullOrEmpty(key))
        {
            return defaultValue;
        }

        if (gameSaveData == null)
        {
            return defaultValue;
        }

        if (gameSaveData.dialogueFlags != null && gameSaveData.dialogueFlags.TryGetValue(key, out bool savedValue))
        {
            return savedValue;
        }

        int storedValue = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
        return storedValue == 1;
    }

    /// <summary>
    /// 设置布尔状态值
    /// </summary>
    public void SetBool(string key, bool value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (gameSaveData != null)
        {
            if (gameSaveData.dialogueFlags == null)
            {
                gameSaveData.dialogueFlags = new Dictionary<string, bool>();
            }

            gameSaveData.dialogueFlags[key] = value;
            SaveSaveData();
        }

        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}

// 数据结构定义
[System.Serializable]
public class GameSaveData
{
    public int currentChapter;
    public int currentDay;
    public List<string> unlockedApps;
    public List<string> playerNotes;
    public Dictionary<string, bool> dialogueFlags;
    public List<string> passwordSolved;
    public Dictionary<string, List<ChatMessage>> chatHistory;
    public List<string> blogPostsRead;
    public List<string> photosUnlocked;
    public List<string> browserHistory;
    public List<string> questionnaireCompleted;
    public bool videoCallTriggered;
}

[System.Serializable]
public class AppDataConfig
{
    public List<AppConfig> apps;
}

[System.Serializable]
public class AppConfig
{
    public string id;
    public string name;
    public bool requiresPassword;
    public string password;
}

[System.Serializable]
public class ChatMessage
{
    public string sender;
    public string content;
    public string timestamp;
    public bool isRead;
}
