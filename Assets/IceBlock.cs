using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBlock : MonoBehaviour
{
    private Collider2D trigger;

    private void Start()
    {
        foreach (Collider2D c in GetComponents<Collider2D>())
        {
            if (c.isTrigger) { trigger = c; break; }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        KHealth kh = col.gameObject.GetComponent<KHealth>();
        if (kh)
        {
            kh.overheat = 0f;
        }
    }

    void OnTriggerStay2D(Collider2D hi)
    {
        if (hi.attachedRigidbody == null) { return; }
        if (hi.attachedRigidbody.isKinematic) { Physics2D.IgnoreCollision(hi, trigger); }

        BasicMove bm = hi.GetComponent<BasicMove>();
        FakeFrictionOnObject ff = hi.GetComponent<FakeFrictionOnObject>();

        if (bm && bm.CanCollide)
        {
            bm.iced = hi.GetComponent<BasicMove>().fakePhysicsVel.x;
            // make sure the player doesn't move way too slowly (unless they want to)
            ulong cs = bm.GetComponent<Encontrolmentation>().currentState;
            if (bm.iced < -1f && bm.iced > -60f && (cs & 3UL) == 1UL)
            {
                bm.iced -= 2f;
            }
            if (bm.iced > 1f && bm.iced < 60f && (cs & 3UL) == 2UL)
            {
                bm.iced += 2f;
            }
            bm.iced2 = 4;
        }

        if (ff)
        {
            ff.iced = 4;
        }

    }
}
