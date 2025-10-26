using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CheckpointManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PasswordChecker passwordChecker;
    public TextController textController;
    public BugAppButton bugAppButton;
    public GameObject dialogue;
    public int checkpoint;
    [Tooltip("自动模式下设置为0.8,非自动模式设置为1000")]
    public float waitTime = 1.0f;
    public GameObject passwordInput;
    public GameObject desktop;
    public bool isUnlocked;
    public bool isFinished;
    void Start()
    {
        isUnlocked = false;
        isFinished = false;
        checkpoint = 0;
        passwordInput.SetActive(true);
        desktop.SetActive(false);
        dialogue.gameObject.SetActive(true);
    }
    void Update()
    {
        if (isUnlocked) return;
        if (checkpoint == 0)
        {
            if (passwordChecker.isUnlocked)
            {
                checkpoint++;
                StartCoroutine(coroutine1());
            }
        }
        else if (checkpoint == 1)
        {
            if (bugAppButton.isClicked)
            {
                checkpoint++;
                StartCoroutine(textController.StartShowing(waitTime, 1));
            }
        }
        else if(checkpoint==-1)
        {
            isUnlocked = true;
            StartCoroutine(coroutine2(waitTime));
        }
    }
    private IEnumerator coroutine1()
    {
        yield return new WaitForSeconds(1.0f);
        passwordInput.SetActive(false);
        desktop.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        bugAppButton.button.interactable = false;
        yield return textController.StartShowing(waitTime, 2);
        bugAppButton.button.interactable = true;
        checkpoint = -1;
    }
    private IEnumerator coroutine2(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        dialogue.SetActive(false);  
        bugAppButton.gameObject.SetActive(false);
        desktop.SetActive(false);
        passwordInput.SetActive(false);
        isFinished = true;
    }

}
