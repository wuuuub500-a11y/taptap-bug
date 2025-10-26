using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class KeyChecker : MonoBehaviour
{
    [Header("CSV (每行: 文本, 4位01串 -> Up,Down,Left,Right)")]
    public TextAsset keySequenceCsv;

    [Header("UI")]
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI statusText;
    public FileImageController imageController;

    [Header("Game settings")]
    public float maxTime = 30f;     // 初始倒计时时间
    public float penalty = 3f;      // 按错一次扣除的时间 (秒)
    public float wrongMsgDuration; // 错误提示显示时间（秒）
    public float correctMsgDuration; // 正确提示显示时间（可选）
    public bool isFinished;
    // 内部状态
    private bool isUnlocked;
    private float timeLeft;
    private bool isBegun = false;
    private int currentIndex = 0;
    private List<SequenceEntry> sequence = new List<SequenceEntry>();
    private GameState state = GameState.Idle;
    private Coroutine clearStatusCoroutine = null;
    private enum GameState { Idle, Running, Win, Lose }

    [Serializable]
    private class SequenceEntry
    {
        public string displayText;
        public string pattern; // "0101" length 4, chars '0'/'1'
    }

    void Start()
    {
        isUnlocked= false;
        isUnlocked = false;
        LoadCsv();
        ResetAll();
    }

    void Update()
    {
        if (isUnlocked) return;
        // Start game on left mouse click when idle
        if (!isBegun && Input.GetMouseButtonDown(0))
        {
            Begin();
            return;
        }

        // If player lost and clicks left mouse, reset to initial state
        if (state == GameState.Lose && Input.GetMouseButtonDown(0))
        {
            ResetAll();
            return;
        }

        if (state == GameState.Running)
        {
            // countdown
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0f) timeLeft = 0f;
            UpdateCountdownText();

            // Win/lose check by time or sequence completion
            if (currentIndex >= sequence.Count)
            {
                state = GameState.Win;
                isBegun = false;
                statusText.text = "you win";
                isUnlocked = true;
                StartCoroutine(WaitAndFinish(1.0f));
                return;
            }
            if (timeLeft <= 0f)
            {
                state = GameState.Lose;
                isBegun = false;
                statusText.text = "You lose.Click to Restart";
                return;
            }

            bool anyArrowKeyDownEvent =
                Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow);

            if (anyArrowKeyDownEvent)
            {
                int matched = EvaluateCurrentPattern();
                if (matched == 1)
                {
                    return;
                }
                if (matched==2)
                {
                    currentIndex++;
                    statusText.text = "correct";
                    StartClearStatusCoroutine(correctMsgDuration);
                    if (currentIndex < sequence.Count)
                    {
                        imageController.ChangeSprite();
                        keyText.text = sequence[currentIndex].displayText;
                    }
                    else
                    {
                        state = GameState.Win;
                        isBegun = false;
                        statusText.text = "you win";
                        StopClearStatusCoroutine();
                    }
                }
                else
                {
                    timeLeft -= penalty;
                    if (timeLeft < 0f) timeLeft = 0f;
                    UpdateCountdownText();
                    statusText.text = "wrong! -" + penalty + "s";
                    StartClearStatusCoroutine(wrongMsgDuration);
                }
            }
        }

        if (state == GameState.Idle)
        {
            countdownText.text = $"{timeLeft:F1}s";
        }
    }

    private void StopClearStatusCoroutine()
    {
        if (clearStatusCoroutine != null)
        {
            StopCoroutine(clearStatusCoroutine);
            clearStatusCoroutine = null;
        }
    }

    private void StartClearStatusCoroutine(float delay)
    {
        StopClearStatusCoroutine();
        clearStatusCoroutine = StartCoroutine(ClearStatusAfterDelay(delay));
    }

    private IEnumerator ClearStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (state == GameState.Running)
        {
            statusText.text = "";
        }
        clearStatusCoroutine = null;
    }
    private void Begin()
    {
        if (sequence.Count == 0)
        {
            Debug.LogWarning("KeyChecker: sequence empty or CSV not loaded.");
            statusText.text = "no sequence";
            return;
        }
        isBegun = true;
        state = GameState.Running;
        statusText.text = "";
        timeLeft = Mathf.Clamp(timeLeft, 0f, maxTime);
        UpdateCountdownText();
        if (currentIndex < sequence.Count)
            keyText.text = sequence[currentIndex].displayText;
    }

    // Reset to initial state
    private void ResetAll()
    {
        timeLeft = maxTime;
        isBegun = false;
        currentIndex = 0;
        state = GameState.Idle;
        statusText.text = "";
        countdownText.text = $"{timeLeft:F1}s";
        if (sequence.Count > 0)
            keyText.text = sequence[0].displayText;
        else
            keyText.text = "";
    }

    private int EvaluateCurrentPattern()
    {
        if (currentIndex < 0 || currentIndex >= sequence.Count) return 0;
        string pat = sequence[currentIndex].pattern;
        if (string.IsNullOrEmpty(pat) || pat.Length != 4) return 0;

        bool upReq = pat[0] == '1';
        bool downReq = pat[1] == '1';
        bool leftReq = pat[2] == '1';
        bool rightReq = pat[3] == '1';
        int ReqCount = 0;
        if(upReq) ReqCount++; if (downReq) ReqCount++; if (leftReq) ReqCount++; if (rightReq) ReqCount++;
        bool upDown = Input.GetKey(KeyCode.UpArrow);
        bool downDown = Input.GetKey(KeyCode.DownArrow);
        bool leftDown = Input.GetKey(KeyCode.LeftArrow);
        bool rightDown = Input.GetKey(KeyCode.RightArrow);
        int DownCount = 0;
        if (upDown) DownCount++; if (downDown) DownCount++; if (leftDown) DownCount++; if (rightDown) DownCount++;
        if (ReqCount > DownCount)
        {
            return 1;
        }
        if (upReq != upDown||downReq != downDown||leftReq != leftDown||rightReq != rightDown) return 0;
        return 2;
    }

    private void UpdateCountdownText()
    {
        countdownText.text = $"{timeLeft:F1}s";
    }
    private IEnumerator WaitAndFinish(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        isFinished = true;
    }
    // 简单 CSV 解析
    private void LoadCsv()
    {
        sequence.Clear();
        if (keySequenceCsv == null)
        {
            Debug.LogWarning("KeyChecker: keySequenceCsv not set.");
            return;
        }

        string[] lines = keySequenceCsv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            int commaIndex = line.IndexOf(',');
            if (commaIndex < 0) continue;
            string left = line.Substring(0, commaIndex).Trim();
            string right = line.Substring(commaIndex + 1).Trim();
            if (left.StartsWith("\"") && left.EndsWith("\"") && left.Length >= 2)
                left = left.Substring(1, left.Length - 2);
            if (right.StartsWith("\"") && right.EndsWith("\"") && right.Length >= 2)
                right = right.Substring(1, right.Length - 2);

            right = right.Replace(" ", "");
            if (right.Length != 4) continue;
            bool okChars = true;
            foreach (char c in right) if (c != '0' && c != '1') okChars = false;
            if (!okChars) continue;

            sequence.Add(new SequenceEntry { displayText = left, pattern = right });
        }

        if (sequence.Count == 0)
        {
            Debug.LogWarning("KeyChecker: loaded sequence empty.");
        }
    }
}
