using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    // 单例
    public static PlacementManager Instance { get; private set; }
    public bool isUnlocked;
    public bool isFinished;
    public float finishTime=0.1f;
    public float waitTime = 1.0f;
    [Header("场景中需要放置的物体总数")]
    public int totalItems = 5;

    int placedCount = 0;

    void Awake()
    {
        isUnlocked = false;
        isFinished = false;
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    public void ItemPlaced()
    {
        placedCount++;
        if (placedCount >= totalItems&&!isUnlocked)
        {
            isUnlocked = true;
            StartCoroutine(WaitAndFinish(finishTime));
        }
    }
    public void ResetAll()
    {
        placedCount = 0;
    }
    private IEnumerator WaitAndFinish(float finishTime)
    {
        yield return new WaitForSeconds(finishTime);
        isFinished = true;
    }
}
