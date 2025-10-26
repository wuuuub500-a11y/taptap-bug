using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimeManager : MonoBehaviour
{
    public Container[] containers;
    public Crime[] crimes;
    public bool isUnlocked;
    public bool isFinished;
    private void Awake()
    {
        isUnlocked = false;
    }
    void Start()
    {
        Container defaultContainer = Container.defaultContainer;
        for (int i = 0; i < crimes.Length; i++)
        {
            defaultContainer.AddItemToLeftmost(crimes[i]);
        }
    }

    void Update()
    {
        if (isUnlocked)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            bool isCorrect = true;
            for (int i = 0; i < containers.Length; i++) {
                if (!containers[i].IsCorrectlyPlaced())
                {
                    isCorrect = false;
                    break;
                }
            }
            if (isCorrect)
            {
                isUnlocked = true;
                StartCoroutine(WaitAndFinish(1.0f));
            }
            else
            {
                Debug.Log("wrong placement");
            }
        }
    }
    private IEnumerator WaitAndFinish(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        isFinished = true;
    }
}
