using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSmall : MonoBehaviour
{
    public GameObject whatThisContains;
    public GameObject popEffect;
    public float magnetSpeed = 240f;
    public Collider2D mainCol;
    public float reappearTime = 3f;

    private bool popped = false;
    private SpriteRenderer spr;

    private void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        mainCol.enabled = true;
    }

    public void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.rigidbody || col.rigidbody.isKinematic) { return; }
        if (popped) { return; }
        popped = true;

        BasicMove bm = col.rigidbody.GetComponent<BasicMove>();
        if (bm)
        {
            Vector2 norm = (col.transform.position - transform.position).normalized;
            Vector2 reflected = col.relativeVelocity - (2 * Vector2.Dot(col.relativeVelocity, norm) * norm);
            Vector2 plrUp = (Vector2)bm.transform.up * Mathf.Sign(bm.transform.localScale.y);
            float dUp = Vector2.Dot(reflected.normalized, plrUp);
            if (dUp > 0f)
            {
                reflected = Vector2.Lerp(reflected, plrUp * reflected.magnitude, dUp);
                Encontrolmentation e = bm.GetComponent<Encontrolmentation>();
                if (e.ButtonHeld(4UL, 12UL, 0f, out _) || e.ButtonHeld(16UL, 16UL, 0f, out _))
                {
                    reflected = new Vector2(reflected.x, 300f);
                }
            }
            bm.fakePhysicsVel = reflected;
            bm.doubleJump = true;
            if (Vector2.Dot(col.GetContact(0).normal,Vector2.down) > 0f) { bm.doubleJump = true; }
            if (whatThisContains != null)
            {
                StartCoroutine(LevelInfoContainer.ItemMagnet(col.transform, whatThisContains.transform, magnetSpeed));
            }
        }
        else
        {
            if (whatThisContains != null && whatThisContains.transform.parent == transform)
            {
                whatThisContains.transform.SetParent(null, true);
            }
        }

        Instantiate(popEffect, transform.position, Quaternion.identity);
        spr.color = Color.clear;
        mainCol.enabled = false;
        StartCoroutine(Reappear());
    }
    
    private IEnumerator Reappear()
    {
        float t = 0;
        while (t < reappearTime)
        {
            if (Time.timeScale > 0) { t += 0.0166667f; }
            spr.color = Color.Lerp(Color.clear, Color.white, 0.5f * (t / reappearTime));
            yield return new WaitForEndOfFrame();
        }
        while (insideCount > 0) { yield return new WaitForEndOfFrame(); }
        yield return null;

        spr.color = Color.white;
        mainCol.enabled = true;
        popped = false;
    }

    // count how many are in the bubble
    private int insideCount = 0;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.attachedRigidbody || col.attachedRigidbody.isKinematic) { return; }
        if (insideCount < 0) { insideCount = 0; }
        ++insideCount;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.attachedRigidbody || col.attachedRigidbody.isKinematic) { return; }
        --insideCount;
        if (insideCount < 0) { insideCount = 0; }
    }

}
