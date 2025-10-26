using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BugAppButton : MonoBehaviour
{
    public bool isClicked = false;
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }
    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        if (!isClicked)
        {
            isClicked = true;
        }
    }
}
