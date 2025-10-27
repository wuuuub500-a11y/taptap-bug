using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonOn : MonoBehaviour
{
    public GameObject gameObject;
    public bool isOnButtion;
    private Button uiButton;
    void Awake()
    {
        uiButton = GetComponent<Button>();
    }
    private void OnEnable()
    {
        uiButton.onClick.AddListener(OnButtonClick);
    }
    private void OnDisable()
    {
        uiButton.onClick.RemoveListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        if (isOnButtion)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
