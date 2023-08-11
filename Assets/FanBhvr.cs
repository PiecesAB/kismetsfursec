using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanBhvr : MonoBehaviour {

    public float power;
    public float maximumMovementSpeed;
    public float distance;
    public bool suction;

    public static List<GameObject> succedThisFrame = new List<GameObject>();

    void OnTriggerEnter2D(Collider2D collider)
    {
        /*try
        {
            Rigidbody2D rb = collider.gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(rb.velocity.x,0);
        }
        catch
        {

        }*/
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        Rigidbody2D rg2 = collider.gameObject.GetComponent<Rigidbody2D>();
        if (rg2 && !rg2.isKinematic && !succedThisFrame.Contains(collider.gameObject))
        {
            float angel = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(-Mathf.Sin(angel), Mathf.Cos(angel));
            float objDist = (collider.gameObject.transform.position - transform.position).magnitude;

            Vector2 newForce = (dir * power * Mathf.Max(distance - objDist, 0));
            if (newForce.magnitude > maximumMovementSpeed)
            {
                newForce = newForce.normalized * maximumMovementSpeed;
            }
            if (suction)
            {
                newForce = -newForce;
            }

            BasicMove bm = collider.GetComponent<BasicMove>();
            if (bm)
            {
                bm.fakePhysicsVel += newForce;
            }
            else
            {
                rg2.velocity += newForce;
            }
            succedThisFrame.Add(collider.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        try
        {
            ConstantForce2D rbHit = collider.gameObject.GetComponent<ConstantForce2D>();
            rbHit.force = Vector2.zero;
        }
        catch
        {

        }
    }

    void Start()
    {
        float power2 = power * 300f;
        GetComponent<Animator>().speed = Mathf.Min(power2 / 16f,2);
        var succ = GetComponentInChildren<ParticleSystem>().velocityOverLifetime;
        var pmain = GetComponentInChildren<ParticleSystem>().main;
        succ.x = succ.z = new ParticleSystem.MinMaxCurve(0, 0);

        pmain.startLifetime = (distance / power2) / 8;
        if (suction)
        {
            GetComponentInChildren<ParticleSystem>().gameObject.transform.localPosition =  new Vector3(0, distance, 0);
            succ.y = new ParticleSystem.MinMaxCurve(power2 * -6, power2 * -8);
            
        }
        else
        {
            succ.y = new ParticleSystem.MinMaxCurve(power2 * 6, power2 * 8);
        }
            pmain.startLifetime = (distance / power2) / 8;

        foreach (BoxCollider2D item in GetComponents(typeof(BoxCollider2D)))
        {
            if (item.isTrigger)
            {
                item.size = new Vector2(15, distance);
               item.offset = new Vector2(0, distance / 2);
            }
        }
    }
	// fans can ruin a game!
	void Update () {
        float angel = transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(-Mathf.Sin(angel), Mathf.Cos(angel));
        GetComponentInChildren<SpikeDirectionSetter>().directionToDie = dir;
        succedThisFrame.Clear();

    }
}
