using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlueBlock : MonoBehaviour
{
    [HideInInspector]
    public AudioSource soundObj;

    private List<GameObject> colObjs = new List<GameObject>();

    private static AudioSource glueSoundCache = null;

    private void GetGlueSound()
    {
        if (glueSoundCache == null) { glueSoundCache = GameObject.FindGameObjectWithTag("GlueSound").GetComponent<AudioSource>(); }
        soundObj = glueSoundCache;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        GetGlueSound();

        if (col.GetComponent<BasicMove>())
        {
            if (!colObjs.Contains(col.gameObject) && (!soundObj.isPlaying || soundObj.time >= 0.25f))
            {
                soundObj.Stop();
                soundObj.Play();
                colObjs.Add(col.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D hi)
    {
        colObjs.Remove(hi.gameObject);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.GetComponent<BasicMove>() == null) { return; }

        BasicMove bm = col.GetComponent<BasicMove>();
        Vector2 dif = col.transform.position - transform.position;
        if (System.Math.Abs(dif.x) >= 8 + 6 * col.transform.localScale.x && System.Math.Abs(dif.y) <= 8 + 14 * col.transform.localScale.y)
        {
            bm.fakePhysicsVel = new Vector2(-220f / Time.timeScale * Mathf.Sign(dif.x), bm.fakePhysicsVel.y);
        }
        else if (System.Math.Abs(dif.x) <= 16 * col.transform.localScale.x && System.Math.Abs(dif.y) >= 8 + 14 * col.transform.localScale.y)
        {
            if (col.GetComponent<Rigidbody2D>().velocity.y <= 0)
                bm.fakePhysicsVel = new Vector2(bm.fakePhysicsVel.x, -50f / Time.timeScale);
            bm.glued = 2;
        }
        else if (col.GetComponent<Encontrolmentation>())
        {
            Encontrolmentation encmt = col.GetComponent<Encontrolmentation>();
            if ((encmt.currentState & 8UL) != 8UL)
            {
                bm.fakePhysicsVel = new Vector2(bm.fakePhysicsVel.x, bm.jumpHeight / Time.timeScale);
                bm.glued = 2;
            }
        }
    }

}
