using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport1 : Transport
{
    float time;
    // Start is called before the first frame update
    void Awake()
    {
        canProceed = true;
        time = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        time+= Time.deltaTime;
        if (Input.GetMouseButton(0) && canProceed && time > 1.5f)
        {
            canProceed = false;
            TransitionManager.Instance.Transition(start, end);
        }
    }
}
