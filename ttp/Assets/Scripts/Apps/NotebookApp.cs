using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NotebookApp : MonoBehaviour
{
    [Header("UI组件")]
    public Transform notesContainer;
    public GameObject notePrefab;
    public TMP_InputField newNoteInput;
    public Button addNoteButton;
    public Button clearAllButton;

    [Header("笔记设置")]
    public int maxNotes = 50;
    public int noteCharacterLimit = 200;

    private List<string> playerNotes = new List<string>();

    void Start()
    {
        InitializeNotebook();
    }

    void InitializeNotebook()
    {
        // 加载已有笔记
        LoadNotes();

        // 设置按钮事件
        if (addNoteButton != null)
        {
            addNoteButton.onClick.AddListener(AddNewNote);
        }

        if (clearAllButton != null)
        {
            clearAllButton.onClick.AddListener(ClearAllNotes);
        }

        // 设置输入框限制
        if (newNoteInput != null)
        {
            newNoteInput.characterLimit = noteCharacterLimit;
        }

        // 显示笔记列表
        RefreshNoteList();
    }

    void LoadNotes()
    {
        var saveData = GameManager.Instance.dataManager.GetSaveData();
        if (saveData != null && saveData.playerNotes != null)
        {
            playerNotes = new List<string>(saveData.playerNotes);
        }
    }

    void SaveNotes()
    {
        GameManager.Instance.dataManager.GetSaveData().playerNotes = new List<string>(playerNotes);
        GameManager.Instance.dataManager.SaveGameData();
    }

    void AddNewNote()
    {
        if (newNoteInput == null) return;

        string noteText = newNoteInput.text.Trim();

        if (string.IsNullOrEmpty(noteText))
        {
            GameManager.Instance.uiManager?.ShowMessage("笔记内容不能为空！");
            return;
        }

        if (playerNotes.Count >= maxNotes)
        {
            GameManager.Instance.uiManager?.ShowMessage($"笔记数量已达上限（{maxNotes}条）！");
            return;
        }

        // 添加时间戳
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string noteWithTimestamp = $"[{timestamp}] {noteText}";

        playerNotes.Add(noteWithTimestamp);
        newNoteInput.text = "";

        SaveNotes();
        RefreshNoteList();

        GameManager.Instance.uiManager?.ShowSuccess("笔记添加成功！");
    }

    void DeleteNote(int index)
    {
        if (index >= 0 && index < playerNotes.Count)
        {
            playerNotes.RemoveAt(index);
            SaveNotes();
            RefreshNoteList();
        }
    }

    void ClearAllNotes()
    {
        if (playerNotes.Count == 0)
        {
            GameManager.Instance.uiManager?.ShowMessage("没有笔记需要清除");
            return;
        }

        // 这里可以添加确认对话框
        playerNotes.Clear();
        SaveNotes();
        RefreshNoteList();

        GameManager.Instance.uiManager?.ShowSuccess("所有笔记已清除");
    }

    void RefreshNoteList()
    {
        // 清除现有的笔记显示
        foreach (Transform child in notesContainer)
        {
            Destroy(child.gameObject);
        }

        // 显示所有笔记
        for (int i = playerNotes.Count - 1; i >= 0; i--) // 最新的在上面
        {
            CreateNoteItem(playerNotes[i], i);
        }
    }

    void CreateNoteItem(string noteText, int index)
    {
        GameObject noteItem = Instantiate(notePrefab, notesContainer);

        // 查找文本组件
        TextMeshProUGUI noteTextComponent = noteItem.transform.Find("NoteText")?.GetComponent<TextMeshProUGUI>();
        Button deleteButton = noteItem.transform.Find("DeleteButton")?.GetComponent<Button>();

        if (noteTextComponent != null)
        {
            noteTextComponent.text = noteText;
        }

        if (deleteButton != null)
        {
            int currentIndex = index; // 避免闭包问题
            deleteButton.onClick.AddListener(() => DeleteNote(currentIndex));
        }
    }

    /// <summary>
    /// 通过代码添加笔记（用于自动记录线索）
    /// </summary>
    public void AddNoteAutomatically(string clueText)
    {
        if (playerNotes.Count >= maxNotes)
        {
            Debug.LogWarning("笔记数量已达上限，无法自动添加");
            return;
        }

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string autoNote = $"[{timestamp}] [自动记录] {clueText}";

        playerNotes.Add(autoNote);
        SaveNotes();
        RefreshNoteList();

        Debug.Log($"自动添加笔记: {clueText}");
    }

    /// <summary>
    /// 获取所有笔记
    /// </summary>
    public List<string> GetAllNotes()
    {
        return new List<string>(playerNotes);
    }

    /// <summary>
    /// 搜索笔记
    /// </summary>
    public List<string> SearchNotes(string keyword)
    {
        List<string> foundNotes = new List<string>();

        if (string.IsNullOrEmpty(keyword))
        {
            return foundNotes;
        }

        foreach (var note in playerNotes)
        {
            if (note.ToLower().Contains(keyword.ToLower()))
            {
                foundNotes.Add(note);
            }
        }

        return foundNotes;
    }
}