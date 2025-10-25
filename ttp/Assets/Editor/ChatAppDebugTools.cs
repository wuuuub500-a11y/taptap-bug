using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 应用调试工具 - 用于重置各个应用的解锁状态
/// </summary>
public class ChatAppDebugTools : EditorWindow
{
    [MenuItem("Tools/应用调试工具/清除聊天应用解锁状态")]
    public static void ClearChatAppUnlockState()
    {
        // 清除PlayerPrefs中的数据
        PlayerPrefs.DeleteKey("ChatApp_Unlocked");
        PlayerPrefs.DeleteKey("ChatApp_Guanqin_Hidden_Unlocked");
        PlayerPrefs.Save();
        
        Debug.Log("✅ 已清除PlayerPrefs中的聊天应用解锁状态");

        // 清除saveData.json中的数据
        string saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                if (saveData != null)
                {
                    // 清除dialogueFlags中的相关标记
                    if (saveData.dialogueFlags != null)
                    {
                        saveData.dialogueFlags.Remove("ChatApp_Unlocked");
                        saveData.dialogueFlags.Remove("ChatApp_Guanqin_Hidden_Unlocked");
                    }
                    
                    // 清除passwordSolved中的相关标记
                    if (saveData.passwordSolved != null)
                    {
                        saveData.passwordSolved.Remove("chat");
                        saveData.passwordSolved.Remove("chat_hidden_guanqin");
                    }
                    
                    // 保存修改后的数据
                    string updatedJsonData = JsonUtility.ToJson(saveData, true);
                    File.WriteAllText(saveFilePath, updatedJsonData);
                    
                    Debug.Log($"✅ 已清除saveData.json中的聊天应用解锁状态\n文件路径: {saveFilePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 清除saveData.json时出错: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ 存档文件不存在: {saveFilePath}");
        }
        
        EditorUtility.DisplayDialog("完成", "聊天应用解锁状态已清除！\n请重新运行游戏测试。", "确定");
    }

    [MenuItem("Tools/应用调试工具/清除相册应用解锁状态")]
    public static void ClearPhotoAlbumUnlockState()
    {
        // 清除saveData.json中的数据
        string saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

                if (saveData != null)
                {
                    // 清除passwordSolved中的相关标记
                    if (saveData.passwordSolved != null)
                    {
                        saveData.passwordSolved.Remove("photoalbum_life");          // PhotoAlbumApp生活相册密码
                        saveData.passwordSolved.Remove("life_mosaic");              // PhotoAlbumApp特殊照片
                        saveData.passwordSolved.Remove("gallery_life_photo");       // GalleryApp生活相册照片
                        saveData.passwordSolved.Remove("gallery_private_album");    // GalleryApp隐私相册
                    }

                    // 清除photosUnlocked列表中的相关照片
                    if (saveData.photosUnlocked != null)
                    {
                        saveData.photosUnlocked.Remove("life_mosaic");              // 移除特殊照片解锁状态
                        saveData.photosUnlocked.Remove("life_001");
                        saveData.photosUnlocked.Remove("life_002");
                        saveData.photosUnlocked.Remove("life_003");
                    }

                    // 保存修改后的数据
                    string updatedJsonData = JsonUtility.ToJson(saveData, true);
                    File.WriteAllText(saveFilePath, updatedJsonData);

                    Debug.Log($"✅ 已清除saveData.json中的相册应用解锁状态\n文件路径: {saveFilePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 清除saveData.json时出错: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ 存档文件不存在: {saveFilePath}");
        }

        EditorUtility.DisplayDialog("完成", "相册应用解锁状态已清除！\n请重新运行游戏测试。", "确定");
    }

    [MenuItem("Tools/应用调试工具/清除所有应用解锁状态")]
    public static void ClearAllAppsUnlockState()
    {
        if (!EditorUtility.DisplayDialog("确认", "确定要清除所有应用的解锁状态吗？\n这包括：\n• 聊天应用\n• 相册应用\n• 隐藏内容", "确定", "取消"))
        {
            return;
        }

        // 清除PlayerPrefs中的数据
        PlayerPrefs.DeleteKey("ChatApp_Unlocked");
        PlayerPrefs.DeleteKey("ChatApp_Guanqin_Hidden_Unlocked");
        PlayerPrefs.Save();

        Debug.Log("✅ 已清除PlayerPrefs中的应用解锁状态");

        // 清除saveData.json中的数据
        string saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

                if (saveData != null)
                {
                    // 清除dialogueFlags中的相关标记
                    if (saveData.dialogueFlags != null)
                    {
                        saveData.dialogueFlags.Remove("ChatApp_Unlocked");
                        saveData.dialogueFlags.Remove("ChatApp_Guanqin_Hidden_Unlocked");
                    }

                    // 清除passwordSolved中的所有应用相关标记
                    if (saveData.passwordSolved != null)
                    {
                        // 聊天应用
                        saveData.passwordSolved.Remove("chat");
                        saveData.passwordSolved.Remove("chat_hidden_guanqin");

                        // 相册应用
                        saveData.passwordSolved.Remove("photoalbum_life");
                        saveData.passwordSolved.Remove("life_mosaic");
                        saveData.passwordSolved.Remove("gallery_life_photo");
                        saveData.passwordSolved.Remove("gallery_private_album");
                    }

                    // 清除photosUnlocked列表
                    if (saveData.photosUnlocked != null)
                    {
                        saveData.photosUnlocked.Clear();
                    }

                    // 保存修改后的数据
                    string updatedJsonData = JsonUtility.ToJson(saveData, true);
                    File.WriteAllText(saveFilePath, updatedJsonData);

                    Debug.Log($"✅ 已清除saveData.json中的所有应用解锁状态\n文件路径: {saveFilePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 清除saveData.json时出错: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ 存档文件不存在: {saveFilePath}");
        }

        EditorUtility.DisplayDialog("完成", "所有应用解锁状态已清除！\n请重新运行游戏测试。", "确定");
    }

    [MenuItem("Tools/应用调试工具/查看BugApp触发条件状态")]
    public static void CheckBugAppTriggerStatus()
    {
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("提示", "请先运行游戏！", "确定");
            return;
        }

        var dataManager = GameManager.Instance?.dataManager;
        if (dataManager == null)
        {
            EditorUtility.DisplayDialog("错误", "DataManager不存在！", "确定");
            return;
        }

        var saveData = dataManager.GetSaveData();
        if (saveData == null)
        {
            EditorUtility.DisplayDialog("错误", "存档数据不存在！", "确定");
            return;
        }

        // 检查4个任务状态
        bool chatUnlocked = dataManager.IsPasswordSolved("chat") || dataManager.GetBool("ChatApp_Unlocked", false);
        bool questionnaireCompleted = dataManager.IsQuestionnaireCompleted("guanqin");
        bool photoUnlocked = saveData.photosUnlocked != null && saveData.photosUnlocked.Contains("life_mosaic");
        bool browserUsed = false;

        if (saveData.browserHistory != null)
        {
            browserUsed = saveData.browserHistory.Exists(url =>
                !string.IsNullOrEmpty(url) &&
                (url.Contains("syxtyulegongsi") || url.Contains("闪耀星途娱乐公司")));
        }

        bool videoCallTriggered = dataManager.IsVideoCallTriggered();

        string status = "=== BugApp触发条件检查 ===\n\n";
        status += $"1. 聊天应用解锁: {(chatUnlocked ? "✅" : "❌")}\n";
        status += $"2. 关钦问卷完成: {(questionnaireCompleted ? "✅" : "❌")}\n";
        status += $"3. 生活相册照片解锁: {(photoUnlocked ? "✅" : "❌")}\n";
        status += $"4. 浏览器访问公司: {(browserUsed ? "✅" : "❌")}\n\n";
        status += $"视频通话已触发: {(videoCallTriggered ? "是" : "否")}\n\n";

        bool allCompleted = chatUnlocked && questionnaireCompleted && photoUnlocked && browserUsed;
        if (allCompleted && !videoCallTriggered)
        {
            status += "状态: 所有任务已完成！请返回桌面触发视频通话。";
        }
        else if (allCompleted && videoCallTriggered)
        {
            status += "状态: 视频通话已经触发过了。";
        }
        else
        {
            status += "状态: 还有任务未完成，请继续游戏。";
        }

        Debug.Log(status);
        EditorUtility.DisplayDialog("BugApp触发条件", status, "确定");
    }

    [MenuItem("Tools/应用调试工具/清除BugApp触发状态")]
    public static void ClearBugAppTriggerState()
    {
        // 清除saveData.json中的数据
        string saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

                if (saveData != null)
                {
                    // 重置视频通话触发状态
                    saveData.videoCallTriggered = false;

                    // 保存修改后的数据
                    string updatedJsonData = JsonUtility.ToJson(saveData, true);
                    File.WriteAllText(saveFilePath, updatedJsonData);

                    Debug.Log($"✅ 已清除BugApp视频通话触发状态\n文件路径: {saveFilePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 清除saveData.json时出错: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ 存档文件不存在: {saveFilePath}");
        }

        EditorUtility.DisplayDialog("完成", "BugApp触发状态已清除！\n可以重新触发视频通话了。", "确定");
    }

    [MenuItem("Tools/应用调试工具/查看存档文件路径")]
    public static void ShowSaveFilePath()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log($"存档文件路径: {saveFilePath}");
        EditorUtility.DisplayDialog("存档文件路径", saveFilePath, "确定");

        // 在Finder/资源管理器中打开文件夹
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem("Tools/应用调试工具/删除整个存档文件")]
    public static void DeleteSaveFile()
    {
        if (!EditorUtility.DisplayDialog("警告", "确定要删除整个存档文件吗？这将清除所有游戏进度！", "确定", "取消"))
        {
            return;
        }

        string saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log($"✅ 已删除存档文件: {saveFilePath}");
            EditorUtility.DisplayDialog("完成", "存档文件已删除！", "确定");
        }
        else
        {
            Debug.LogWarning($"⚠️ 存档文件不存在: {saveFilePath}");
            EditorUtility.DisplayDialog("提示", "存档文件不存在", "确定");
        }
    }
}
