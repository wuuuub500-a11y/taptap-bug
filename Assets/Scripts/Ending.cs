using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ending: MonoBehaviour
{
    public static Ending instance;
    public int endingIndex;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
