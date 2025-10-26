using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Transition(string start, string end)
    {
       
       StartCoroutine(LoadScene(start, end));
    }
    private IEnumerator LoadScene(string start,string end)
    {
        switch (start)
        {
            case "0.1":
                yield return ScreenFader.instance.Fade(1f,0.6f);
                yield return ScreenFader.instance.Fade(0f,0.6f);
                break;
            case "0.2":
                yield return PostProcessingController.instance.SetWeight(1.0f, 1.6f);
                yield return ScreenFader.instance.Fade(1f, 1.0f);
                yield return ScreenFader.instance.Fade(0f, 1.0f);
                yield return ScreenFader.instance.Fade(1f, 1.0f);
                yield return ScreenFader.instance.Fade(0f, 1.0f);
                yield return PostProcessingController.instance.SetWeight(0.0f, 1.6f);
                yield return ScreenFader.instance.Fade(1f, 0.5f);
                yield return new WaitForSecondsRealtime(1.5f);
                yield return SceneManager.UnloadSceneAsync(start);
                yield return SceneManager.LoadSceneAsync(end, LoadSceneMode.Additive);
                yield return ScreenFader.instance.Fade(0f, 0.5f);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(end));
                break;
            case "0.3":
                yield return ScreenFader.instance.Fade(1f, 0.6f);
                yield return SceneManager.UnloadSceneAsync(start);
                yield return SceneManager.LoadSceneAsync(end, LoadSceneMode.Additive);
                yield return ScreenFader.instance.Fade(0f, 0.6f);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(end));
                break;
            case "0.4":
                yield return ScreenFader.instance.Fade(1f, 0.6f);
                yield return SceneManager.UnloadSceneAsync(start);
                yield return SceneManager.LoadSceneAsync(end, LoadSceneMode.Additive);
                yield return ScreenFader.instance.Fade(0f, 0.6f);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(end));
                break;
            case "0.5":
                yield return ScreenFader.instance.Fade(1f, 0.6f);
                yield return SceneManager.UnloadSceneAsync(start);
                yield return SceneManager.LoadSceneAsync(end, LoadSceneMode.Additive);
                yield return ScreenFader.instance.Fade(0f, 0.6f);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(end));
                break;
        }

       
    }
}
