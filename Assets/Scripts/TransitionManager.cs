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
    private IEnumerator LoadScene(string start, string end)
    {
        yield return SceneManager.UnloadSceneAsync(start);
        yield return SceneManager.LoadSceneAsync(end, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(end));
    }
}
