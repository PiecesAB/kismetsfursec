using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breadboxHelper : MonoBehaviour
{
    public breadbox hub;
    private int cooldown = 0;

    public void OnCollisionEnter2D(Collision2D c)
    {
        if (hub.state == breadbox.State.Thrust && c.GetContact(0).normal.y < 0.1f)
        {
            KHealth kh = c.gameObject.GetComponent<KHealth>();
            BasicMove bm = c.gameObject.GetComponent<BasicMove>();
            if (!c.gameObject.CompareTag("Player"))
            {
                hub.state = breadbox.State.Cooldown;
            }
            else if (kh && bm && cooldown == 0)
            {
                kh.ChangeHealth(-bm.Damage * 1.3f, "breadbox");
                bm.fakePhysicsVel += 1.3f * hub.GetComponent<Rigidbody2D>().velocity;
                cooldown = 14;
            }
        }
    }

    public void OnCollisionStay2D(Collision2D c)
    {
        if (hub.state == breadbox.State.Thrust && c.GetContact(0).normal.y < 0.1f && c.gameObject.CompareTag("Player"))
        {
            KHealth kh = c.gameObject.GetComponent<KHealth>();
            BasicMove bm = c.gameObject.GetComponent<BasicMove>();
            if (kh && bm && cooldown == 0)
            {
                kh.ChangeHealth(-bm.Damage * 1.3f, "breadbox");
                bm.fakePhysicsVel += 1.3f * hub.GetComponent<Rigidbody2D>().velocity;
                cooldown = 14;
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (cooldown > 0) { --cooldown; }
    }
}
