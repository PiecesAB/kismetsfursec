using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeBlock : MonoBehaviour
{
    [Header("w is the fade wait time")]
    public float w;
    [Header("for fade block physical collider")]
    public Collider2D cd;

    private double trs;
    private bool fading;

    public bool connectToParent = false;

    private void Start()
    {
        name = "Fade";
        fading = false;
        trs = 0f;
        cd.enabled = true;
        Color cx = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = new Color(cx.r, cx.g, cx.b, 1f);
    }

    public void Activate()
    {
        if (!fading || (DoubleTime.ScaledTimeSinceLoad - w + 0.5f >= trs))
        {
            trs = DoubleTime.ScaledTimeSinceLoad;
            fading = true;

            if (connectToParent)
            {
                foreach (FadeBlock other in transform.parent.GetComponentsInChildren<FadeBlock>())
                {
                    if (other == this) { continue; }
                    other.Activate();
                }
            }
        }
    }

    private void Detect(Collision2D col)
    {
        Rigidbody2D or2 = col.gameObject.GetComponent<Rigidbody2D>();
        if (or2 && !or2.isKinematic && or2.gameObject.layer == 20) // we want only the player to fade blocks for now. enemies staying on them would make for cool puzzles.
        {
            Activate();
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Detect(col);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        Detect(col);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        Rigidbody2D or2 = col.gameObject.GetComponent<Rigidbody2D>();
        if (!or2 || or2.isKinematic) { return; }
        while (DoubleTime.ScaledTimeSinceLoad - w + 0.625f >= trs)
        {
            trs += 0.1f;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (fading)
        {
            float tx = Mathf.PingPong((float)(DoubleTime.ScaledTimeSinceLoad - trs), 0.5f * w);
            Color cx = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(cx.r, cx.g, cx.b, Mathf.Max(1f - (2f * tx), 0f));
            if (1f - (2f * tx) <= 0f)
            {
                cd.enabled = false;
            }
            else
            {
                cd.enabled = true;
            }
            if (DoubleTime.ScaledTimeSinceLoad - trs >= w)
            {
                fading = false;
                cd.enabled = true;
                cx = GetComponent<SpriteRenderer>().color;
                GetComponent<SpriteRenderer>().color = new Color(cx.r, cx.g, cx.b, 1f);
            }
        }
    }
}
