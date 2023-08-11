using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBlock : MonoBehaviour
{

    public bool on;
    public double delayBeforeMove;
    public float currSpeed = 0f;
    public float speedIncrement = 0.2f;
    public float maxSpeed = 3f;
    public GameObject rocketParticleObj;
    public GameObject explosionEffect;

    private Rigidbody2D rg2;
    private const float collisionTestDist = 9f;

    void Start()
    {
        currSpeed = 0f;
        rg2 = GetComponent<Rigidbody2D>();
    }

    public void BlowMeUp()
    {
        //makes sure player isnt destroyed
        foreach (Transform c in transform)
        {
            if (c.GetComponent<BasicMove>())
            {
                c.GetComponent<BasicMove>().Unparent();
            }
        }
        Destroy(GetComponent<Collider2D>());
        Destroy(GetComponent<SpriteRenderer>());
        Destroy(rocketParticleObj);
        Destroy(gameObject, 1f);
        Destroy(this);
    }

    void FixedUpdate()
    {
        if (rocketParticleObj)
        {
            rocketParticleObj.SetActive(on);
        }

        if (!on)
        {
            currSpeed = 0f;
        }

        if (Time.timeScale > 0 && on && rg2)
        {
            if (delayBeforeMove > 0.00001)
            {
                delayBeforeMove -= 0.01666666666666666666666;
                if (delayBeforeMove < 0.001)
                {
                    delayBeforeMove = 0.0;
                }
            }
            else
            {
                currSpeed += speedIncrement * Time.timeScale;
                currSpeed = Mathf.Min(maxSpeed, currSpeed);
                //rg2.MovePosition(transform.position + (transform.up * currSpeed * Time.timeScale));
                rg2.velocity = ((Vector2)transform.up) * currSpeed * 60f;
                Collider2D ct = Physics2D.OverlapPoint(transform.position + (transform.up * collisionTestDist), 256 + 512);
                if (ct)
                {
                    BlowMeUp();
                }
            }
        }
    }
}
