using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class EndingController : MonoBehaviour
{
    public VideoPlayer[] players;
    public RenderTexture rt;
    private int endingIndex;
    void Start()
    {
        endingIndex = Ending.instance.endingIndex;
        PlayEndingVideo();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void PlayEndingVideo()
    {
        if (endingIndex == 0)
        {
            StartCoroutine(ending0());
        }
        else if (endingIndex == 1)
        {
            StartCoroutine(ending1());
        }
        else if (endingIndex == 2)
        {
            StartCoroutine(ending2());
        }
    }
    private IEnumerator ending0()
    {
        yield return ScreenFader.instance.Fade(1f, 1f);
        resetTexture();
        players[0].gameObject.SetActive(true);
        yield return new WaitForSeconds((float)players[0].length);
        yield return ScreenFader.instance.Fade(0f, 1f);
    }
    private IEnumerator ending1() {
        //yield return ScreenFader.instance.Fade(1f, 1f);
        resetTexture();
        players[1].gameObject.SetActive(true);
        yield return new WaitForSeconds((float)players[1].length);
        players[1].gameObject.SetActive(false);
        resetTexture();
        players[2].gameObject.SetActive(true);
        yield return new WaitForSeconds((float)players[2].length);
        //yield return ScreenFader.instance.Fade(0f, 1f);
    }
    private IEnumerator ending2() {
        yield return ScreenFader.instance.Fade(1f, 1f);
        resetTexture();
        players[1].gameObject.SetActive(true);
        yield return new WaitForSeconds((float)players[1].length);
        players[1].gameObject.SetActive(false);
        resetTexture();
        players[3].gameObject.SetActive(true);
        yield return new WaitForSeconds((float)players[3].length);
        yield return ScreenFader.instance.Fade(0f, 1f);
    }
    private void resetTexture()
    {
        if(rt != null)
        {
            rt.Release();
        }
        else
        {
            rt = RenderTexture.GetTemporary(1920, 1080);
        }
    }
}
