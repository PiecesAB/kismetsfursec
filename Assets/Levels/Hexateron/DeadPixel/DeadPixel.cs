using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadPixel : MonoBehaviour
{
    private SpriteRenderer r;
    private Collider2D c;

    private int touchCount = 0;

    private void Start()
    {
        r = GetComponent<SpriteRenderer>();
        c = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        ++touchCount;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        --touchCount;
    }

    private void Update()
    {
        if (r.isVisible)
        {
            if (CGICycleMover.AtLeastOneExists() || touchCount > 0) { c.isTrigger = true; r.color = new Color(1, 1, 1, 0.3f); }
            else { c.isTrigger = false; r.color = Color.white; }
        }
    }
}
