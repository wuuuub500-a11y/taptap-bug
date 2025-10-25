using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    [Header("场景视图")]
    public GameObject roomView;
    public GameObject computerView;
    public Image fadeImage;

    [Header("过渡设置")]
    public float fadeDuration = 1.0f;

    public enum ViewState { Room, Computer }
    private ViewState currentView = ViewState.Room;

    void Start()
    {
        // 初始化场景状态
        roomView.SetActive(true);
        computerView.SetActive(false);

        if (fadeImage != null)
        {
            fadeImage.color = Color.clear;
        }
    }

    /// <summary>
    /// 切换到房间视角
    /// </summary>
    public void SwitchToRoomView()
    {
        if (currentView == ViewState.Room) return;

        Debug.Log("切换到房间视角");
        FadeAndSwitch(() => {
            roomView.SetActive(true);
            computerView.SetActive(false);
            currentView = ViewState.Room;
        });
    }

    /// <summary>
    /// 切换到电脑视角
    /// </summary>
    public void SwitchToComputerView()
    {
        if (currentView == ViewState.Computer) return;

        Debug.Log("切换到电脑视角");
        FadeAndSwitch(() => {
            roomView.SetActive(false);
            computerView.SetActive(true);
            currentView = ViewState.Computer;

            // 通知应用管理器可以启动应用了
            if (GameManager.Instance.appManager != null)
            {
                GameManager.Instance.appManager.OnComputerViewActivated();
            }
        });
    }

    /// <summary>
    /// 淡入淡出切换场景
    /// </summary>
    private void FadeAndSwitch(System.Action onHalfFade)
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeCoroutine(onHalfFade));
        }
        else
        {
            // 如果没有淡入淡出图像，直接切换
            onHalfFade?.Invoke();
        }
    }

    private System.Collections.IEnumerator FadeCoroutine(System.Action onHalfFade)
    {
        // 淡出到黑色
        float timer = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(0, 0, 0, 1);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, endColor.a, timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 执行切换
        onHalfFade?.Invoke();

        // 淡入到透明
        timer = 0f;
        startColor = fadeImage.color;
        endColor = new Color(0, 0, 0, 0);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, endColor.a, timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    /// <summary>
    /// 获取当前视角
    /// </summary>
    public ViewState GetCurrentView()
    {
        return currentView;
    }
}