using UnityEngine;
using System.Collections;

public class WirePlatformStand : MonoBehaviour {

    // Use this for initialization
    void OnCollisionEnter2D(Collision2D col)
    {
        col.transform.parent = transform;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        col.transform.parent = null;
    }
}
