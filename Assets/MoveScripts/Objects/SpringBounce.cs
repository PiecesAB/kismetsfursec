using UnityEngine;
using System.Collections;

public class SpringBounce : MonoBehaviour {

    public float bounceVelocity;

	// Use this for initialization
	void Start () {
	
	}
	
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.contacts[0].normal.y > 0.6f)
        {
                col.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(col.gameObject.GetComponent<Rigidbody2D>().velocity.x, bounceVelocity);
            if (col.gameObject.GetComponent<BasicMove>())
            {
                col.gameObject.GetComponent<BasicMove>().grounded = 0;
            }
        }
    }



	// Update is called once per frame
	void Update () {
	
	}
}
