using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LoadingController : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public Image bar;
    public float loadTime;
    public float stopPercent;
    public float clickPercent;
    public bool isFinished;
    public int maxClick;
    private int clickCount;
    private bool finished;
    private bool canClick;
    private float time;
    private float currentPercent;
    void Start()
    {
        time = 0.0f;
        currentPercent = 0.0f;
        isFinished = false;
        finished= false;
        canClick = true;
    }
    void Update()
    {
        if (finished) return;
        time += Time.deltaTime;
        currentPercent = -stopPercent * Mathf.Exp(-time / loadTime) + stopPercent;
        bar.fillAmount = currentPercent;
        textMeshPro.text = "Loading... " + currentPercent * 100 + "%";
        if (currentPercent < clickPercent)
        {
            return;
        }
        if (canClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //这里可以播放一个音效
                clickCount++;
                if (clickCount > maxClick)
                {
                    canClick = false;
                }
                currentPercent = 0.99f;
                bar.fillAmount = currentPercent;
                textMeshPro.text = "Loading... " + currentPercent * 100 + "%";
            }
            return;
        }
        currentPercent = 1.0f;
        bar.fillAmount = currentPercent;
        textMeshPro.text = "Loading... " + currentPercent * 100 + "%";
        finished = true;
        StartCoroutine(WaitAndFinish(1.0f));
    }
    private IEnumerator WaitAndFinish(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        isFinished = true;
    }
}
