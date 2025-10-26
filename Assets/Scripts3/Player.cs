using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameController controller;
    public GameObject startPoint;
    public float moveSpeed = 20f;
    private Image player;
    private Rigidbody2D rb;
    private Vector2 speed;
    // Start is called before the first frame update
    void Start()
    {
        player= GetComponent<Image>();
        rb = player.GetComponent<Rigidbody2D>();
        if (startPoint != null)
        {
            player.rectTransform.position = startPoint.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.fail)
        {
            rb.velocity = Vector2.zero;
            if (Input.GetMouseButtonDown(0))
            {
                controller.GameReset();
            }
            return;
        }
        if (controller.succeed)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (Input.GetKey(KeyCode.W))
        {
            speed = new Vector2(0, moveSpeed);
            player.rectTransform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            speed = new Vector2(0, -moveSpeed);
            player.rectTransform.eulerAngles = new Vector3(0, 0, 180);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            speed = new Vector2(-moveSpeed, 0);
            player.rectTransform.eulerAngles = new Vector3(0, 0, 90);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            speed=new Vector2(moveSpeed, 0);
            player.rectTransform.eulerAngles = new Vector3(0, 0, -90);
        }
        else
        {
            speed = Vector2.zero;
        }
        rb.velocity = speed;
    }
    public void Reset()
    {
        rb.velocity = Vector2.zero;
        player.rectTransform.position = startPoint.transform.position;
    }
}
