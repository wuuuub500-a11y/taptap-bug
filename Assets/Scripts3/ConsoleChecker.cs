// PasswordChecker.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleChecker : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("显示当前输入的 Text（UnityEngine.UI.Text）")]
    public GameObject console;
    public TextMeshProUGUI ConsoleText;
    public TextMeshProUGUI ResultText;
    public string correctMessage= "Successful Retrieval";
    public string incorrectMessage="Failed Retrieval";
    public string template="select*from userlog where user=";
    public string GuanQin="guanqinnn121";
    public string FuMi="fumi0103";
    public string FuJian= "fujian3366";
    public string ManYing = "998manying";
    private string currentInput = "";
    public bool isUnlocked = false;
    public GameObject[] games;
    private int currentActive;
    public int activeCount;
    private void Start()
    {
        console.SetActive(true);
        currentActive = -1;
        activeCount = 0;
        for (int i = 0; i < games.Length; i++)
        {
            games[i].SetActive(false);
        }
        UpdateDisplay();
    }
    private void Update()
    {
        if(currentActive != -1)
        {
            if (games[currentActive].GetComponent<GameController>().succeed)
            {
                int cur= currentActive;
                currentActive = -1;
                StartCoroutine(WaitAndQuit(1.0f, cur));
            }
        }
        if (activeCount == 4)
        {
            activeCount++;
            StartCoroutine(WaitAndUnlock(1.0f));
        }
    }
    public void HandleInput(string input)
    {
        if (currentActive != -1)
        {
            return;
        }
        if (input == "Backspace")
        {
            ResultText.text = "";
            if(currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            }
                UpdateDisplay();
            return;
        }
        if (input == "Return")
        {
            if (currentInput == GuanQin)
            {
                ResultText.text = correctMessage;
                currentActive = 0;
                StartCoroutine(WaitAndStart(1.0f, 0));
            }
            else if (currentInput == FuMi)
            {
                ResultText.text = correctMessage;
                currentActive = 1;
                StartCoroutine(WaitAndStart(1.0f, 1));
            }
            else if (currentInput == FuJian)
            {
                ResultText.text = correctMessage;
                currentActive = 2;
                StartCoroutine(WaitAndStart(1.0f, 2));
            }
            else if (currentInput == ManYing) 
            {
                ResultText.text = correctMessage;
                currentActive = 3;
                StartCoroutine(WaitAndStart(1.0f, 3));
            }
            else
            {
                ResultText.text = incorrectMessage;
            }

            UpdateDisplay();
            return;
        }
        if (input.Length == 1)
        {
            if (currentInput.Length < 20)
            {
                currentInput += input;
                UpdateDisplay();
            }
        }
    }

    private void UpdateDisplay()
    {
        if (ConsoleText != null)
        {
            ConsoleText.text = template + currentInput;
        }
    }
    private IEnumerator WaitAndStart(float waitTime,int gameid)
    {
        yield return new WaitForSeconds(waitTime);
        games[gameid].SetActive(true);
    }
    private IEnumerator WaitAndQuit(float waitTime,int gameid)
    {
        yield return new WaitForSeconds(waitTime);
        games[gameid].SetActive(false);
        ResultText.text = "";
        currentInput = "";
        activeCount++;
    }
    private IEnumerator WaitAndUnlock(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        console.SetActive(false);
        isUnlocked = true;
    }
}
