using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inTaxi : MonoBehaviour
{
    public float waitTime;
    public float finishTime = 0.1f;
    public bool isUnlocked;
    public bool isFinished;
    public ButtonController4 button;
    private float time;
    void Start()
    {
        time = 0.0f;
        button.gameObject.SetActive(true);
        isUnlocked = false;
        isFinished = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUnlocked || time < waitTime)
        {
            return;
        }
        time += Time.deltaTime;
        if (button.isFinished)
        {
            isFinished = true;
            StartCoroutine(WaitAndFinish(finishTime));
        }
    }
    private IEnumerator WaitAndFinish(float finishTime)
    {
        yield return new WaitForSeconds(finishTime);
        isFinished = true;
        button.gameObject.SetActive(false);
    }
}
