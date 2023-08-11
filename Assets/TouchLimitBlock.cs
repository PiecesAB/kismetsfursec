using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchLimitBlock : MonoBehaviour
{
    [Header("i don't know lol. it depends what type")]
    public GameObject[] lol;
    [Header("v1 is explosion for mines but also other crap")]
    public float v1 = 1800f;

    public AudioSource soundObj;
    [Header("like lol but for music")]
    public AudioSource[] moreaudio;

    private bool toush = false;
    private bool toushB = false;
    private int int1 = 0;
    private int cooldown = 0;

    public static int touchLimitCount = 0;

    private void OnLevelWasLoaded(int level)
    {
        touchLimitCount = 0;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Vector2 pressdir = new Vector2(Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad));
        if (System.Math.Abs(col.contacts[0].normal.x - pressdir.x) <= 0.25f && System.Math.Abs(col.contacts[0].normal.y - pressdir.y) <= 0.25f)
        {
            toush = true;
        }
    }

    void OnCollisionStay2D(Collision2D hi)
    {
        Vector2 pressdir = new Vector2(Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad));
        if (System.Math.Abs(hi.contacts[0].normal.x - pressdir.x) <= 0.25f && System.Math.Abs(hi.contacts[0].normal.y - pressdir.y) <= 0.25f)
        {
            toush = true;
        }
    }

    void OnTriggerEnter2D(Collider2D hi)
    {
        toushB = true;
    }

    void OnTriggerExit2D(Collider2D hi)
    {
        toushB = false;
    }

    void OnTriggerStay2D(Collider2D hi)
    {
        toushB = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        toush = false;
        cooldown = 2;
    }

    private void Start()
    {
        name = "TouchLimit";
        ++touchLimitCount;
        toush = toushB = false;
        int1 = 0;
        lol[0].transform.localPosition = Vector3.up * 8f;
        lol[1].GetComponent<TextMesh>().text = v1.ToString();
    }

    private void Update()
    {
        if (lol[1] == null) { return; }

        TextMesh tm = lol[1].GetComponent<TextMesh>();
        if (tm == null) { return; }
        tm.color = Color.Lerp(tm.color, Color.white, 0.1f);
        if (cooldown > 0)
        {
            cooldown--;
        }
        else
        {
            if (toush)
            {
                lol[0].transform.localPosition = Vector3.up * 4f;
                if (int1 < 2)
                {
                    v1--;
                    moreaudio[0].pitch = Mathf.Max(Mathf.Pow(2f, -(v1 - 3f) / 12f), 0.66741993f);
                    moreaudio[0].Play();
                    tm.color = Color.red;
                    tm.text = v1.ToString();
                    if (v1 <= 0)
                    {
                        GetComponent<SpriteRenderer>().color = lol[0].GetComponent<SpriteRenderer>().color = Color.red;
                    }
                    int1 = 2;
                }
            }
            else if (toushB)
            {
                lol[0].transform.localPosition = Vector3.up * 6f;
                lol[1].GetComponent<TextMesh>().color = Color.yellow;
                if (int1 == 0)
                {
                    soundObj.Play();
                    int1 = 1;
                }
            }
            else
            {
                lol[0].transform.localPosition = Vector3.up * 8f;
                if (v1 <= 0)
                {
                    moreaudio[1].Play();
                    Destroy(transform.GetChild(1).gameObject);
                    Destroy(transform.GetChild(0).gameObject);
                    Destroy(GetComponent<SpriteRenderer>());
                    --touchLimitCount;
                    //print(touchLimitCount);
                    foreach (var i in GetComponents<Collider2D>())
                    {
                        Destroy(i);
                    }
                    Destroy(gameObject, 1f); //ADD EFFECTS
                }
                else if (int1 > 0)
                {
                    soundObj.Play();
                    int1 = 0;
                }
            }
        }
    }
}
