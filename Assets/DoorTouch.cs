using UnityEngine;
using System.Collections;

public class DoorTouch : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    void OnCollisionStay2D(Collision2D coll) // C#, type first, name in second
    {
        if (coll.gameObject.layer == 20)
        {

        }
    }
    // Update is called once per frame
    void Update () {
	
	}
}
