using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport0 : Transport
{
    // Start is called before the first frame update
    void Awake()
    {
        canProceed = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)&&canProceed)
        {
            canProceed=false;
            TransitionManager.Instance.Transition(start, end);
        }
    }
}
