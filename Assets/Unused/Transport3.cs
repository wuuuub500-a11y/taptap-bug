using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport3 : Transport
{
    float time;
    float shakeTime;
    public float SceneTime;
    public ScreenShaker shaker;
    public float shakeFrequency;
    public float amplitude;
    public float frequency;
    public float duration;
    // Start is called before the first frame update
    void Awake()
    {
        canProceed = true;
        time = 0.0f;
        shakeTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        shakeTime += Time.deltaTime;
        if (shakeTime > 1.0f)
        {
            shakeTime=0.0f;
            shaker.Shake(amplitude, frequency, duration);
        }
        if (Input.GetMouseButton(0) && canProceed && time > SceneTime)
        {
            canProceed = false;
            TransitionManager.Instance.Transition(start, end);
        }
    }
}
