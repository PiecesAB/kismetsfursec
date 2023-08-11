using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPlatformBalance : MonoBehaviour {

    public List<SliderJoint2D> joints;

	// Use this for initialization
	void Start () {
	
	}

    void OnCollisionExit2D(Collision2D c)
    {
        foreach (SliderJoint2D i in joints)
        {
            if (i.connectedBody == c.rigidbody)
            {
                joints.Remove(i);
                Destroy(i);
            }
        }
    }


    void OnCollisionStay2D(Collision2D c)
    {
        bool add = true;
        foreach (SliderJoint2D i in joints)
        {
            if (i.connectedBody == c.rigidbody)
            {
                add = false;
            }
        }
        if (c.gameObject.GetComponent<BasicMove>())
        {
            if (c.gameObject.GetComponent<BasicMove>().running)
            {
                add = false;
            }
        }
        if (add && c.contacts[0].normal == new Vector2(0, -1))
        {
            float colX = transform.InverseTransformPoint(c.contacts[0].point).x;
            SliderJoint2D hi = gameObject.AddComponent<SliderJoint2D>();
            hi.enableCollision = true;
            hi.connectedBody = c.rigidbody;
            hi.autoConfigureAngle = false;
            hi.angle = 90;
            hi.anchor = new Vector2(colX, 0);
            hi.connectedAnchor = new Vector2(colX, -24);
            joints.Add(hi);
        }

        foreach (SliderJoint2D i in joints)
        {
            if (c.gameObject.GetComponent<BasicMove>())
            {
                if (c.gameObject.GetComponent<BasicMove>().running)
                {
                    joints.Remove(i);
                    Destroy(i);
                }
            }
        }
    }


        void OnCollisionEnter2D(Collision2D c)
    {
        bool add = true;
        foreach (SliderJoint2D i in joints)
        {
            if (i.connectedBody == c.rigidbody)
            {
                add = false;
            }
        }
        if (c.gameObject.GetComponent<BasicMove>())
        {
            if (c.gameObject.GetComponent<BasicMove>().running)
            {
                add = false;
            }
        }
        if (add && c.contacts[0].normal == new Vector2(0,-1))
        {
            float colX = transform.InverseTransformPoint(c.contacts[0].point).x;
            SliderJoint2D hi = gameObject.AddComponent<SliderJoint2D>();
            hi.enableCollision = true;
            hi.connectedBody = c.rigidbody;
            hi.autoConfigureAngle = false;
            hi.angle = 90;
            hi.anchor = new Vector2(colX, 0);
            hi.connectedAnchor = new Vector2(colX, -24);
            joints.Add(hi);
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
