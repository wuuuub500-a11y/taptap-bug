using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndPoint : MonoBehaviour
{
    public GameController controller;
    public Image player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (player != null && other.gameObject == player.gameObject)
        {
            controller.fail = false;
            controller.succeed = true;
        }
    }
}
