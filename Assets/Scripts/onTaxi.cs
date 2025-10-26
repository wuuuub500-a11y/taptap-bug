using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onTaxi : MonoBehaviour
{
    public float waitTime;
    public float finishTime = 0.1f;
    public bool isUnlocked;
    public bool isFinished;
    public ScreenShaker shaker;
    public float shakeFrequency;
    public float amplitude;
    public float frequency;
    public float duration;
    private float time;
    private float shakeTime;
    void Start()
    {
        time = 0.0f;
        shakeTime = 0.0f;
        isUnlocked = false;
        isFinished = false;
    }

    // Update is called once per frame
    void Update()
    {
        shakeTime += Time.deltaTime;
        if (shakeTime > shakeFrequency)
        {
            shakeTime = 0.0f;
            shaker.Shake(amplitude, frequency, duration);
        }
        if (isUnlocked || time < waitTime)
        {
            return;
        }
        time += Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            isUnlocked = true;
            StartCoroutine(WaitAndFinish(finishTime));
        }
    }
    private IEnumerator WaitAndFinish(float finishTime)
    {
        yield return new WaitForSeconds(finishTime);
        isFinished = true;
    }
}
