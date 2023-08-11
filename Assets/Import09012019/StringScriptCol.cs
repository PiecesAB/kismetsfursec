using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringScriptCol : MonoBehaviour {

    public StringScript mom;

    private void OnTriggerStay2D(Collider2D c)
    {
        mom.EnterTrig(c);
    }

    /*private void OnTriggerExit2D(Collider2D c)
    {
        mom.ExitTrig(c);
    }*/
}
