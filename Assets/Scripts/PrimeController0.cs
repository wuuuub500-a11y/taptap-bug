using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrimeController0 : MonoBehaviour
{
    public inTheBeginning beginning;
    public onAirplane onAirplane;
    public onTaxi onTaxi;
    public inTaxi inTaxi;
    public PlacementManager placementManager;
    public CheckpointManager checkpointManager;
    public float[] waitTimes;
    private int procession;
    void Start()
    {
        procession = 0;
        beginning.gameObject.SetActive(true);
    }
    void Update()
    {
        if (procession == 0 && beginning.isFinished)
        {
            procession++;
            StartCoroutine(transport0(waitTimes[0]));
        }
        if (procession == 1 && onAirplane.isFinished)
        {
            procession++;
            StartCoroutine(transport1(waitTimes[1]));
        }
        if(procession == 2 && onTaxi.isFinished)
        {
            procession++;
            StartCoroutine(transport2(waitTimes[2]));
        }
        if (procession == 3 && inTaxi.isFinished)
        {
            procession++;
            StartCoroutine (transport3(waitTimes[3]));
        }
        if(procession == 4 && placementManager.isFinished)
        {
            procession++;
            StartCoroutine(transport4(waitTimes[4]));
        }
        if (procession == 5 && checkpointManager.isFinished)
        {
            procession++;
            StartCoroutine(transport5(waitTimes[5]));
        }
    }
    private IEnumerator transport0(float waitTime)
    {
        yield return ScreenFader.instance.Fade(1f, waitTime);
        beginning.gameObject.SetActive(false);
        onAirplane.gameObject.SetActive(true);
        yield return ScreenFader.instance.Fade(0f, waitTime);
    }
    private IEnumerator transport1(float waitTime)
    {
        yield return PostProcessingController.instance.SetWeight(1.0f, 1.6f);
        yield return ScreenFader.instance.Fade(1f, 1.0f);
        yield return ScreenFader.instance.Fade(0f, 1.0f);
        yield return ScreenFader.instance.Fade(1f, 1.0f);
        yield return ScreenFader.instance.Fade(0f, 1.0f);
        yield return PostProcessingController.instance.SetWeight(0.0f, 1.6f);
        yield return ScreenFader.instance.Fade(1f, 0.5f);
        yield return new WaitForSecondsRealtime(1.5f);
        onAirplane.gameObject.SetActive(false);
        onTaxi.gameObject.SetActive(true);
        yield return ScreenFader.instance.Fade(0f, 0.5f);
    }
    private IEnumerator transport2(float waitTime)
    {
        yield return ScreenFader.instance.Fade(1f, waitTime);
        onTaxi.gameObject.SetActive(false);
        inTaxi.gameObject.SetActive(true);
        yield return ScreenFader.instance.Fade(0f, waitTime);
    }
    private IEnumerator transport3(float waitTime) {
        yield return ScreenFader.instance.Fade(1f, waitTime);
        inTaxi.gameObject.SetActive(false);
        placementManager.gameObject.SetActive(true);
        yield return ScreenFader.instance.Fade(0f, waitTime);
    }
    private IEnumerator transport4(float waitTime)
    {
        yield return ScreenFader.instance.Fade(1f, waitTime);
        placementManager.gameObject.SetActive(false);
        checkpointManager.gameObject.SetActive(true);
        yield return ScreenFader.instance.Fade(0f, waitTime);
    }
    private IEnumerator transport5(float waitTime)
    {
        yield return ScreenFader.instance.Fade(1f, waitTime);
        checkpointManager.gameObject.SetActive(false);

        yield return ScreenFader.instance.Fade(0f, waitTime);
    }
}
