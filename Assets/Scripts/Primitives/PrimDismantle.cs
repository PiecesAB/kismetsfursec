using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimDismantle : MonoBehaviour
{
    private Renderer rd;
    private Vector3 rotAxis;

    private void Start()
    {
        foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
        {
            c.enabled = false;
        }

        foreach (WheelJoint2D w in GetComponentsInChildren<WheelJoint2D>())
        {
            Destroy(w);
        }

        if (GetComponent<FrontPanel>() != null)
        {
            GetComponent<FrontPanel>().OnDismantle();
        }
        else
        {
            foreach (IPrimDismantle p in GetComponentsInChildren<IPrimDismantle>())
            {
                p.OnDismantle();
            }
        }

        rd = GetComponent<Renderer>();
        if (rd == null) { rd = GetComponentInChildren<Renderer>(); };
        if (rd == null) { Destroy(gameObject); return; }

        if (gameObject.GetComponent<Rigidbody2D>()) { DestroyImmediate(gameObject.GetComponent<Rigidbody2D>()); }
        Rigidbody2D rg = gameObject.AddComponent<Rigidbody2D>();
        rg.gravityScale = 50;
        rg.mass = 1;

        transform.position += new Vector3(0, 0, -24);

        rotAxis = Fakerand.UnitSphere(true);
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (rd.isVisible)
        {
            transform.rotation *= Quaternion.AngleAxis(3f * Time.timeScale, rotAxis);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
