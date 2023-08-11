using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyClone : GenericBlowMeUp
{
    private bool bye = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<KHealth>() && !bye)
        {
            bye = true;
            col.gameObject.GetComponent<KHealth>().ChangeHealth(-10f, "clone");
            BlowMeUp();
        }
    }
}
