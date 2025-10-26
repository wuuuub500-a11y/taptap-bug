using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onAirplane : MonoBehaviour
{
    public float waitTime;
    public float finishTime = 0.1f;
    public bool isUnlocked;
    public bool isFinished;
    private float time;
    void Start()
    {
        time = 0.0f;
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
