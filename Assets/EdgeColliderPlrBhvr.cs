using UnityEngine;
using System.Collections;

public class EdgeColliderPlrBhvr : MonoBehaviour {

	void OnCollisionEnter2D(Collision2D coll)
    {
        GetComponentInParent<BasicMove>().OnCollisionEnter2D(coll);
    }
}
