using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonController4 : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite image1;
    [SerializeField] private Sprite image2;
    [SerializeField] private Sprite image3;

    [Header("clickCooldown")]
    [SerializeField] private float cooldownTime;
    public string start;
    public string end;
    public bool isFinished;
    private Button uiButton;
    private int clickCount = 0;
    private bool canClick = false;
    private Coroutine cooldownCoroutine;

    private void Awake()
    {
        isFinished = false;
        uiButton = GetComponent<Button>();
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
            if (targetImage == null)
                targetImage = GetComponentInChildren<Image>();
        }

        // 初始
        if (targetImage != null && image1 != null)
            targetImage.sprite = image1;
    }

    private void OnEnable()
    {
        uiButton.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        uiButton.onClick.RemoveListener(OnButtonClick);
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);
    }

    private void OnButtonClick()
    {
        if (clickCount == 0)
        {
            if (image2 != null && targetImage != null)
                targetImage.sprite = image2;

            clickCount = 1;
            canClick = false;

            if (cooldownCoroutine != null)
                StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = StartCoroutine(CooldownCoroutine());
        }
        else if (clickCount == 1)
        {
            if (!canClick)
            {
                return;
            }
            if (image3 != null && targetImage != null)
                targetImage.sprite = image3;

            clickCount = 2;
            canClick = false;
            if (cooldownCoroutine != null)
                StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = StartCoroutine(CooldownCoroutine());
        }
        else if(clickCount==2)
        {
            if (!canClick) return;
            Debug.Log("u");
            uiButton.interactable = false;
            uiButton.onClick.RemoveListener(OnButtonClick);
            isFinished = true;
        }
        else
        {
            return;
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, cooldownTime));
        canClick = true;
        cooldownCoroutine = null;
    }

    /// <summary>
    /// 可从外部调用来复位按纽。
    /// </summary>
    public void ResetToInitial()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        clickCount = 0;
        canClick = false;

        if (targetImage != null && image1 != null)
            targetImage.sprite = image1;

        uiButton.interactable = true;
        uiButton.onClick.RemoveListener(OnButtonClick);
        uiButton.onClick.AddListener(OnButtonClick);
    }
}
