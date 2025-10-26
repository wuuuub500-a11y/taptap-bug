using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimeController : MonoBehaviour
{
    public ConsoleChecker checker;
    public LoadingController loadingController;
    public CrimeManager crimeManager;
    public KeyChecker keyChecker;
    private int procession;
    void Awake()
    {
        procession = 0;
        checker.gameObject.SetActive(true);
        loadingController.gameObject.SetActive(false);
        crimeManager.gameObject.SetActive(false);
    }
    void Update()
    {
        if (procession==0&&checker.isUnlocked == true)
        {
            procession++;
            checker.gameObject.SetActive(false);
            loadingController.gameObject.SetActive(true);
        }
        if(procession==1&&loadingController.isFinished == true)
        {
            procession++;
            loadingController.gameObject.SetActive(false);
            crimeManager.gameObject.SetActive(true);
        }
        if(procession==2&&crimeManager.isFinished == true)
        {
            procession++;
            crimeManager.gameObject.SetActive(false);
            keyChecker.gameObject.SetActive(true);
        }
        if(procession==3&&keyChecker.isFinished == true)
        {
            procession++;
            keyChecker.gameObject.SetActive(false);
        }
    }
}
