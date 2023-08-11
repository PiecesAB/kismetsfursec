using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimEndAmbush : MonoBehaviour
{
    public AmbushController ambushController;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { return; }
        ambushController.ExternalTrigger();
    }
}
