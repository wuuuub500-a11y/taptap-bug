using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FileImageController : MonoBehaviour
{
    public Sprite[] sprites;
    public int currentIndex = 0;
    public Image image;
    // Start is called before the first frame update
    void Start()
    {
        currentIndex = 0;
        image.sprite=sprites[currentIndex];
    }
    public void ChangeSprite()
    {
        if (image != null && sprites.Length > 0)
        {
            currentIndex = (currentIndex + 1) % sprites.Length;
            image.sprite = sprites[currentIndex];
        }
    }
}
