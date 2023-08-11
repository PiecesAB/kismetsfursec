using UnityEngine;
using System.Collections;

public class VelocityLimiter2D : MonoBehaviour {

    public float minVelocityY = Mathf.NegativeInfinity;
    public float maxVelocityY = Mathf.Infinity;
	
	// Update is called once per frame
	void Update () {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(Mathf.Max(rb.velocity.y, minVelocityY), maxVelocityY));
	}
}
