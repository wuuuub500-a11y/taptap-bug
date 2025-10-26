using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameController : MonoBehaviour
{
    public GameObject failText;
    public GameObject succeedText;
    public Player player;
    public LightController[] lights;
    public bool fail = false;
    public bool succeed = false;
    void Start()
    {
        fail = false;
        succeed = false;
        failText.SetActive(false);
        succeedText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (fail == true)
        {
            failText.SetActive(true);
        }
        if (succeed == true) {
            succeedText.SetActive(true);
        }
    }
    public void GameReset()
    {
        fail = false;
        succeed = false;
        failText.SetActive(false);
        succeedText.SetActive(false);
        player.Reset();
        for (int i = 0; i < lights.Length; i++) { 
            lights[i].Reset();
        }
    }
}
