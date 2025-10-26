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
    [Tooltip("ÿ���ַ���ʾ��")]
    public float speed = 20f;

    [Tooltip("һ������ַ������������ַ����У�")]
    public int maxCharacter = 20;

    [Tooltip("�Ƿ������հ��У�true��������false�������д���")]
    public bool skipEmptyLines = true;

    [Tooltip("�Ƿ��� Start ʱ�Զ���ʼ��ʾ��һ��")]
    public bool playOnStart = true;

    // �ڲ�״̬
    private string[] lines;            // CSV ����
    private int currentLineIndex = 0;  // ��ǰ������ʾ��������
    private Coroutine typingCoroutine = null;
    private string processedLine = ""; // �Ѵ��������� maxCharacter ���У��ĵ�ǰ���ı�
    private bool isTyping = false;     // �������ַ���ʾ��
    private bool finishedAll = false;  // �Ƿ��Ѿ���ʾ��������
    private Coroutine multiShowCoroutine = null;
    void Start()
    {
        if (textMeshPro == null)
        {
            Debug.LogError("TextController: ���� Inspector �з��� textMeshPro.");
            enabled = false;
            return;
        }

        if (csvFile == null)
        {
            Debug.LogError("TextController: ���� Inspector �з��� csvFile (TextAsset).");
            enabled = false;
            return;
        }

        // �ָ��ļ�Ϊ�У����������Ա���� skipEmptyLines ������
        lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // ���Ҫ���������У���ǰ������������һ���ǿ���
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
                //ֱ����ʾ�����ַ���
                ShowFullCurrentLineImmediately();
            }
        }
    }

    private void BeginShowCurrentLine()
    {
        if (currentLineIndex < 0 || currentLineIndex >= lines.Length)
        {
            // ������Χ��������
            finishedAll = true;
            Debug.Log(1);
            return;
        }

        string rawLine = lines[currentLineIndex] ?? "";
        // �����й��򣨰��ַ������뻻�з���
        processedLine = InsertLineBreaks(rawLine, Mathf.Max(1, maxCharacter));
        // ��������Ч��Э��
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
            // �ȴ���һ�ַ��򱻴�ϣ������������ʾȫ����
            float t = 0f;
            while (t < delay)
            {
                // ���Э��û�б�ֹͣ��ѭ���ȴ�
                t += Time.deltaTime;
                yield return null;
            }
        }

        // ������ַ���ʾ
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
    /// �� maxChar ÿ�����뻻�з���������ԭ�л��У��Ȱ�ԭ����ͳһΪ���� '\n' Ȼ���ٰ�ÿ�г��ȴ���
    /// </summary>
    private string InsertLineBreaks(string raw, int maxChar)
    {
        if (string.IsNullOrEmpty(raw)) return "";

        // �ȱ�׼��ԭ�л���Ϊ '\n'���ٰ�ÿ������ֱ���
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
            // �������������������һ�����䣬�����ԭ���Ļ���
            if (p != paragraphs.Length - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// ������ǰ�Ƶ���������һ���ǿ��У��� skipEmptyLines=true ʱʹ�ã�
    /// </summary>
    private void AdvanceToNextNonEmptyLine(ref int index)
    {
        while (index < lines.Length)
        {
            if (!string.IsNullOrEmpty(lines[index])) break;
            index++;
        }
    }

    // ���ⲿ������ʾ
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
            // �����ļ���β
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

            // ��ղ��������ַ���ʾ
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

            // ��ʾ�굱ǰ�к󣬵ȴ� finishTime ���ټ���
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

            // ׼����һ������������ԭ�� skipEmptyLines ��Ϊ��
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
    
    // ����
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
