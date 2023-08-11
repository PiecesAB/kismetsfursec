using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class ShutdownButton : MonoBehaviour {

    public Sprite onSprite;
    public Sprite shutoffSprite;
    public int delay = 130;
    public bool touched = false;

    

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!touched && c.gameObject.layer == 20) //player
        {
            GetComponent<SpriteRenderer>().sprite = shutoffSprite;
            touched = true;
            GetComponent<AudioSource>().Play();
        }
    }

    private void Update()
    {
        if (touched && delay > 0)
        {
            if (delay % 6 >= 3)
            {
                GetComponent<SpriteRenderer>().sprite = onSprite;
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = shutoffSprite;
            }
            delay--;
            if (delay == 0)
            {
                MetaBehaviour.Shutdown();
            }
        }
    }
}
