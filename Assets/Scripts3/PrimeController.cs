using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimeController : MonoBehaviour
{
    public ConsoleChecker checker;
    public LoadingController loadingController;
    public CrimeManager crimeManager;
    public KeyChecker keyChecker;
    public GameObject consoleOn;
    public GameObject noteOn;
    public GameObject fileOn;
    public GameObject webOn;
    private int procession;
    void Awake()
    {
        procession = 0;
        consoleOn.SetActive(true);
    }
    void Update()
    {
        if (procession==0&&checker.isUnlocked == true)
        {
            procession++;
            consoleOn.SetActive(false);
            checker.gameObject.SetActive(false);
            loadingController.gameObject.SetActive(true);
        }
        if(procession==1&&loadingController.isFinished == true)
        {
            procession++;
            loadingController.gameObject.SetActive(false);
            noteOn.SetActive(true);
        }
        if(procession==2&&crimeManager.isFinished == true)
        {
            procession++;
            crimeManager.gameObject.SetActive(false);
            noteOn.SetActive(false);
            webOn.SetActive(true);
        }
        if(procession==3&&keyChecker.isFinished == true)
        {
            procession++;
            keyChecker.gameObject.SetActive(false);
            webOn.SetActive(false);
        }
    }
}
