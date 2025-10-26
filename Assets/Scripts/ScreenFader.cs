using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;
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
    public CanvasGroup cg;
    private bool isFade;
    public IEnumerator Fade(float alpha, float fadeDuration)
    {
        //Debug.Log(alpha);
        isFade = true;
        cg.blocksRaycasts = true;
        float fadeSpeed = Mathf.Abs(cg.alpha - alpha) / fadeDuration;
        while (!Mathf.Approximately(cg.alpha, alpha))
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        cg.blocksRaycasts = false;
        isFade = false;
    }
}
