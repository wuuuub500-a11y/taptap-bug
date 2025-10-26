using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport : MonoBehaviour
{
    public string start;
    public string end;
    public bool canProceed;
    void Awake()
    {
        canProceed = false;
    }
}
