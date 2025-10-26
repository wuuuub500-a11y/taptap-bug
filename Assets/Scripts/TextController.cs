using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class TextController : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textMeshPro;
    public TextAsset csvFile;
    [Header("Typing settings")]
    [Tooltip("每秒字符显示数")]
    public float speed = 20f;

    [Tooltip("一行最多字符数（超出按字符换行）")]
    public int maxCharacter = 20;

    [Tooltip("是否跳过空白行（true：跳过，false：按空行处理）")]
    public bool skipEmptyLines = true;

    [Tooltip("是否在 Start 时自动开始显示第一行")]
    public bool playOnStart = true;

    // 内部状态
    private string[] lines;            // CSV 的行
    private int currentLineIndex = 0;  // 当前正在显示的行索引
    private Coroutine typingCoroutine = null;
    private string processedLine = ""; // 已处理（包含按 maxCharacter 换行）的当前行文本
    private bool isTyping = false;     // 正在逐字符显示中
    private bool finishedAll = false;  // 是否已经显示完所有行
    private Coroutine multiShowCoroutine = null;
    void Start()
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextController: 请在 Inspector 中分配 textMeshPro.");
            enabled = false;
            return;
        }

        if (csvFile == null)
        {
            Debug.LogError("TextController: 请在 Inspector 中分配 csvFile (TextAsset).");
            enabled = false;
            return;
        }

        // 分割文件为行（保留空行以便根据 skipEmptyLines 决定）
        lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // 如果要求跳过空行，提前调整索引到第一个非空行
        if (skipEmptyLines)
        {
            AdvanceToNextNonEmptyLine(ref currentLineIndex);
        }

        textMeshPro.text = "";

        if (playOnStart)
        {
            BeginShowCurrentLine();
        }
    }

    void Update()
    {
        if (finishedAll) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                //直接显示完整字符串
                ShowFullCurrentLineImmediately();
            }
        }
    }

    private void BeginShowCurrentLine()
    {
        if (currentLineIndex < 0 || currentLineIndex >= lines.Length)
        {
            // 超出范围（防护）
            finishedAll = true;
            Debug.Log(1);
            return;
        }

        string rawLine = lines[currentLineIndex] ?? "";
        // 处理换行规则（按字符数插入换行符）
        processedLine = InsertLineBreaks(rawLine, Mathf.Max(1, maxCharacter));
        // 启动打字效果协程
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTextCoroutine(processedLine, Mathf.Max(0.0001f, speed)));
    }

    private IEnumerator TypeTextCoroutine(string fullText, float charactersPerSecond)
    {
        isTyping = true;
        textMeshPro.text = "";
        int len = fullText.Length;
        if (charactersPerSecond <= 0) charactersPerSecond = 1f;
        float delay = 1f / charactersPerSecond;

        for (int i = 0; i < len; i++)
        {
            textMeshPro.text += fullText[i];
            // 等待下一字符或被打断（点击会立即显示全部）
            float t = 0f;
            while (t < delay)
            {
                // 如果协程没有被停止，循环等待
                t += Time.deltaTime;
                yield return null;
            }
        }

        // 完成逐字符显示
        isTyping = false;
        typingCoroutine = null;
    }

    private void ShowFullCurrentLineImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        textMeshPro.text = processedLine;
        isTyping = false;
    }

    /// <summary>
    /// 按 maxChar 每隔插入换行符（不会拆除原有换行，先把原换行统一为单个 '\n' 然后再按每行长度处理）
    /// </summary>
    private string InsertLineBreaks(string raw, int maxChar)
    {
        if (string.IsNullOrEmpty(raw)) return "";

        // 先标准化原有换行为 '\n'，再按每个段落分别处理
        string[] paragraphs = raw.Replace("\r\n", "\n").Split(new[] { '\n' }, StringSplitOptions.None);
        StringBuilder sb = new StringBuilder();

        for (int p = 0; p < paragraphs.Length; p++)
        {
            string para = paragraphs[p];
            int count = 0;
            for (int i = 0; i < para.Length; i++)
            {
                sb.Append(para[i]);
                count++;
                if (count >= maxChar && i != para.Length - 1)
                {
                    sb.Append('\n');
                    count = 0;
                }
            }
            // 段落结束，如果不是最后一个段落，就添加原来的换行
            if (p != paragraphs.Length - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 将索引前移到数组中下一个非空行（当 skipEmptyLines=true 时使用）
    /// </summary>
    private void AdvanceToNextNonEmptyLine(ref int index)
    {
        while (index < lines.Length)
        {
            if (!string.IsNullOrEmpty(lines[index])) break;
            index++;
        }
    }

    // 由外部调用显示
    public IEnumerator StartShowing(float waitTime, int lineCount)
    {
        if (finishedAll) yield return null;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }
        if (multiShowCoroutine != null)
        {
            StopCoroutine(multiShowCoroutine);
            multiShowCoroutine = null;
        }
        if (lineCount <= 0) yield return null;
        yield return multiShowCoroutine = StartCoroutine(ShowLinesCoroutine(waitTime, lineCount));
    }

    private IEnumerator ShowLinesCoroutine(float waitTime, int lineCount)
    {
        int startIndex = currentLineIndex;
        int endIndex = startIndex + Mathf.Max(0, lineCount) - 1;

        for (int idx = startIndex; idx <= endIndex; idx++)
        {
            // 到达文件结尾
            if (idx >= lines.Length)
            {
                finishedAll = true;
                multiShowCoroutine = null;
                Debug.Log(1);
                yield break;
            }

            currentLineIndex = idx;
            string rawLine = lines[currentLineIndex] ?? "";
            processedLine = InsertLineBreaks(rawLine, Mathf.Max(1, maxCharacter));

            // 清空并启动逐字符显示
            textMeshPro.text = "";
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                isTyping = false;
            }
            typingCoroutine = StartCoroutine(TypeTextCoroutine(processedLine, Mathf.Max(0.0001f, speed)));
            while (isTyping)
                yield return null;

            typingCoroutine = null;

            // 显示完当前行后，等待 finishTime 秒再继续
            if (waitTime > 0f)
            {
                float t = 0f;
                while (t < waitTime)
                {
                    if (t > Time.deltaTime && Input.GetMouseButtonDown(0)) break;
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            // 准备下一行索引（保留原有 skipEmptyLines 行为）
            currentLineIndex++;

            if (currentLineIndex >= lines.Length)
            {
                finishedAll = true;
                multiShowCoroutine = null;
                Debug.Log(1);
                yield break;
            }
        }

        multiShowCoroutine = null;
    }
    
    // 重置
    public void ResetAndRestart()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = null;
        currentLineIndex = 0;
        finishedAll = false;
        textMeshPro.text = "";
        if (skipEmptyLines) AdvanceToNextNonEmptyLine(ref currentLineIndex);
        BeginShowCurrentLine();
    }
}
