using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageBumpBox : MonoBehaviour, IChoiceUIResponse
{

    public GameObject myBox;
    public bool oneTimeHit;
    public bool hit;
    public string output;
    public Sprite hit0;
    public Sprite hit1;


    private double hitTime;
    private bool inhit;
    private float vel;
    private Vector3 origPos;
    private bool z;

    public void Bump(float velocity)
    {
        GameObject ne = Instantiate(myBox, Vector3.zero, Quaternion.identity) as GameObject;
        z = !z;
        GetComponent<SpriteRenderer>().sprite = z ? hit1 : hit0;
        ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
        ne.SetActive(true);
        vel = velocity;
        if (oneTimeHit)
        {
            GetComponent<Collider2D>().enabled = false;
        }
        hit = true;
        inhit = false;
    }

    public GameObject ChoiceResponse(string text)
    {
        output = text;
        return null;
    }

    void Start()
    {
        hit = inhit = z = false;
        origPos = transform.position;
        GetComponent<SpriteRenderer>().sprite = hit0;
    }


    void Update () {
        if (hit && inhit)
        {

            if (vel > 0f)
            {
                Vector3 nv = Vector3.up * vel * (float)System.Math.Cos((DoubleTime.UnscaledTimeRunning - hitTime) * 10f);
                transform.position = origPos + nv;
                GetComponent<Collider2D>().offset = -nv;
                vel *= 0.99f;
                if (vel < 1f)
                {
                    vel = 0f;
                }
            }
            else
            {
                transform.position = origPos;
                GetComponent<Collider2D>().offset = Vector2.zero;
            }

            if (oneTimeHit)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Color c = sr.color;
                if (c.a > 0f)
                {
                    sr.color = new Color(c.r, c.g, c.b, Mathf.Max(c.a-0.015f, 0));
                }
            }
        }

        if (hit && !inhit)
        {
            hitTime = DoubleTime.UnscaledTimeRunning;
            inhit = true;
        }
	}



}
