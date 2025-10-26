using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingController : MonoBehaviour
{
    public PostProcessVolume volume;
    public static PostProcessingController instance;
    void Awake()
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

    public IEnumerator SetWeight(float target, float duration)
    {
        float fadeSpeed = Mathf.Abs(volume.weight - target) / duration;
        while (!Mathf.Approximately(volume.weight,target))
        {
            volume.weight = Mathf.MoveTowards(volume.weight, target, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
