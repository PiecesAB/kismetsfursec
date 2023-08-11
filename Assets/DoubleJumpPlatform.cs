using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumpPlatform : MonoBehaviour
{
    private BasicMove bm;
    private SpriteRenderer sr;
    private Vector3 origOffset;
    private Transform t;
    private Color c;

    private Vector3 lastPos;

    void Start()
    {
        t = transform;
        bm = t.parent.GetComponent<BasicMove>();
        sr = GetComponent<SpriteRenderer>();
        origOffset = t.localPosition;
        c = sr.color;
        sr.color = Color.clear;
        lastPos = t.position;
    }

    void Update()
    {
        bool vis = (bm.grounded == 0) && bm.doubleJump;
        bool fix = !bm.doubleJump;

        sr.color = new Color(c.r, c.g, c.b, Mathf.Clamp(sr.color.a + (vis ? 0.1f : -0.1f), 0f, c.a));
        if (fix) { t.position = lastPos; } else { t.localPosition = origOffset; }

        lastPos = t.position;
    }
}
