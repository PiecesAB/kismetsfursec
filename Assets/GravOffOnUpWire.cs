using UnityEngine;
using System.Collections;

public class GravOffOnUpWire : MonoBehaviour {



	// Use this for initialization
	void Start () {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        other.gameObject.GetComponent<Rigidbody2D>().gravityScale = 45;
    }

    // Update is called once per frame
    void Update () {
	
	}
}
