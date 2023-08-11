using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonCanCollideHelper : MonoBehaviour
{
    private BasicMove bm;
    private Collider2D c2;
    private List<Collider2D> ignores = new List<Collider2D>();

    private void Start()
    {
        bm = transform.parent.GetComponent<BasicMove>();
        c2 = GetComponent<Collider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.isTrigger) { return; }
        ignores.Add(other);
        Physics2D.IgnoreCollision(c2, other, true);
        bm.IgnoreCollisionFromOutside(other);
    }

    private void Update()
    {
        if (!bm || !c2) { return; }
        c2.enabled = !bm.CanCollide;
        if (bm.CanCollide && ignores.Count > 0)
        {
            for (int i = 0; i < ignores.Count; ++i)
            {
                if (ignores[i] == null) { continue; }
                Physics2D.IgnoreCollision(c2, ignores[i], false);
            }
            ignores.Clear();
        }
    }
}
