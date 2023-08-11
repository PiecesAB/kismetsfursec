using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pulley2Helper : MonoBehaviour
{
    public Pulley2 hub;
    public float impulseTotal;
    public List<int> colThisFrame = new List<int>();
    private bool addedBumpThisFrame;

    void Start()
    {
        addedBumpThisFrame = false;
        colThisFrame = new List<int>();
    }

    private void OnTriggerStay2D(Collider2D c) // something rests on top
    {
        Rigidbody2D r2 = c.GetComponent<Rigidbody2D>();
        int idNum = c.gameObject.GetInstanceID();
        if (c.gameObject.CompareTag("Player") || (!colThisFrame.Contains(idNum)) && r2 && !r2.isKinematic)
        {
            colThisFrame.Add(idNum);
            impulseTotal += r2.mass;
            if (c.GetComponent<BasicMove>() == null) // not player
            {
                r2.AddForce(Vector2.down*5000f); // stay here
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D c) // check bump from bottom
    {
        if (!addedBumpThisFrame)
        {
            addedBumpThisFrame = true;
            if (c.gameObject.CompareTag("Player") && Vector2.Dot(c.GetContact(0).normal, Vector2.up) > 0.99f)
            {
                hub.AddPlrBump(this);
            }
        }
    }

    void Update()
    {
        colThisFrame.Clear();
        addedBumpThisFrame = false;
        if (impulseTotal != 0f)
        {
            hub.Impulse(this, impulseTotal);
            impulseTotal = 0f;
        }
    }
}
