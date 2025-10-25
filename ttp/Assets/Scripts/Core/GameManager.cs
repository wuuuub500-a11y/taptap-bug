using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("核心管理器")]
    public SceneManager sceneManager;
    public DataManager dataManager;
    public UIManager uiManager;
    public AppManager appManager;
    public TimeManager timeManager;
    public StoryManager storyManager;
    public ClueManager clueManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeManagers()
    {
        // 确保所有管理器都已分配
        if (sceneManager == null) Debug.LogError("SceneManager 未分配！");
        if (dataManager == null) Debug.LogError("DataManager 未分配！");
        if (uiManager == null) Debug.LogError("UIManager 未分配！");
        if (appManager == null) Debug.LogError("AppManager 未分配！");
        if (timeManager == null) Debug.LogError("TimeManager 未分配！");
        if (storyManager == null) Debug.LogError("StoryManager 未分配！");
        if (clueManager == null) Debug.LogError("ClueManager 未分配！");

        if (GetComponent<BugVideoTrigger>() == null)
        {
            gameObject.AddComponent<BugVideoTrigger>();
        }

        Debug.Log("游戏管理器初始化完成");
    }

    void Start()
    {
        // 游戏启动逻辑
        dataManager.LoadGameData();
        InitializeGame();
    }

    void InitializeGame()
    {
        // 直接进入电脑视角(不经过房间)
        if (sceneManager != null)
        {
            sceneManager.SwitchToComputerView();
        }

        // 初始化窗口管理器
        if (appManager != null)
        {
            var windowManager = FindObjectOfType<WindowManager>();
            if (windowManager != null)
            {
                // 确保所有窗口初始状态正确
                windowManager.CloseAllWindows();
            }
        }

        Debug.Log("游戏初始化完成 - 电脑视角");
    }

    // 游戏退出时保存
    void OnApplicationQuit()
    {
        dataManager.SaveGameData();
    }
}
