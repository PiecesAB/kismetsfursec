using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class SwitchButtonBhvrs : MonoBehaviour
{

    public enum Type
    {
        Turnstile, Normal, Locked, Heavy, HeavyLocked, Region, ToggleWhenDestroyed
    }

    [Range(0,31)]
    public int ID;
    public Type buttonType;
    public bool inverted;
    public bool on;
    [Header("Turnstile needs three sprites of left, middle, and right")]
    public Sprite[] sprites;

    public AudioClip onSound;
    public AudioClip offSound;

    private uint realID;
    private List<GameObject> collidingA = new List<GameObject>();
    private HashSet<Collider2D> collidingB = new HashSet<Collider2D>();

    private bool a;
    private bool b;

    private BoxCollider2D myBox;

    private static Vector2 normalButtonOffSize = new Vector2(12, 10);
    private static Vector2 normalButtonOnSize = new Vector2(12, 6);
    private static Vector2 normalButtonOffOffset = new Vector2(0, -3);
    private static Vector2 normalButtonOnOffset = new Vector2(0, -5);

    // Use this for initialization
    void Start()
    {
        if (Application.isPlaying)
        {
            a = false;
            b = GetComponent<BoxCollider2D>().enabled;
            GetComponent<Renderer>().material.SetColor("_RepColor", Utilities.colorCycle[ID]);
            realID = 1u << ID;
            if (buttonType == Type.Turnstile)
            {
                if ((!inverted && (realID & Utilities.loadedSaveData.switchMask) != 0) || (inverted && (realID & Utilities.loadedSaveData.switchMask) == 0))
                {
                    on = true;
                    GetComponent<SpriteRenderer>().sprite = sprites[2];
                }
                else
                {
                    on = false;
                    GetComponent<SpriteRenderer>().sprite = sprites[0];
                }
            }

            if (buttonType == Type.Normal)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[0];
                myBox = GetComponent<BoxCollider2D>();
                myBox.size = normalButtonOffSize;
                myBox.offset = normalButtonOffOffset;
            }
        }
    }

    private void TurnOn(Collider2D col)
    {
        if (!collidingB.Contains(col))
        {
            if (collidingB.Count == 0)
            {
                //print("switch on");
                GetComponent<SpriteRenderer>().sprite = sprites[1];
                GetComponent<AudioSource>().clip = onSound;
                GetComponent<AudioSource>().Play();
                Utilities.ChangeSwitchRequest(realID);

                //move object down
                myBox.size = normalButtonOnSize;
                myBox.offset = normalButtonOnOffset;
                col.transform.position += transform.up * (normalButtonOnSize.y - normalButtonOffSize.y);
            }
            collidingB.Add(col);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!Application.isPlaying) { return; }

        if (buttonType == Type.Normal)
        {
            float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
            if (Vector2.Dot(col.GetContact(0).normal, new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle))) > 0.9f)
            {
                Rigidbody2D tr2 = col.collider.GetComponent<Rigidbody2D>();
                if (tr2 && !tr2.isKinematic)
                {
                    TurnOn(col.collider);
                }
                else if (col.gameObject.GetComponent<SuperRay>())
                {
                    TurnOn(col.collider);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (Application.isPlaying)
        {
            if (buttonType == Type.Normal && collidingB.Count > 0)
            {
                Rigidbody2D tr2 = col.collider.GetComponent<Rigidbody2D>();
                if (tr2 && !tr2.isKinematic)
                {
                    Collider2D last = col.collider;
                    collidingB.Remove(col.collider);
                    if (collidingB.Count == 0)
                    {
                        //print("switch off");
                        GetComponent<SpriteRenderer>().sprite = sprites[0];
                        GetComponent<AudioSource>().clip = offSound;
                        GetComponent<AudioSource>().Play();
                        Utilities.ChangeSwitchRequest(realID);

                        //move object up
                        myBox.size = normalButtonOffSize;
                        myBox.offset = normalButtonOffOffset;
                        last.transform.position += transform.up * (normalButtonOffSize.y - normalButtonOnSize.y);
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (Application.isPlaying)
        {
            if (buttonType == Type.Turnstile)
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    GetComponent<SpriteRenderer>().sprite = sprites[1];
                    collidingA.Add(col.gameObject);
                }
            }
        }
    }


    void OnTriggerExit2D(Collider2D col)
    {
        if (Application.isPlaying)
        {
            if (buttonType == Type.Turnstile)
            {
                collidingA.Remove(col.gameObject);
                if (col.gameObject.CompareTag("Player") && collidingA.Contains(col.gameObject))
                {
                    bool did = false;
                    if (on && col.transform.position.x + 4 < transform.position.x)
                    {
                        GetComponent<SpriteRenderer>().sprite = sprites[0];
                        on = false;
                        did = true;
                        Utilities.ChangeSwitchRequest(realID);
                    }
                    if (!did && !on && col.transform.position.x - 4 >= transform.position.x)
                    {
                        GetComponent<SpriteRenderer>().sprite = sprites[2];
                        on = true;
                        Utilities.ChangeSwitchRequest(realID);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        if (Application.isPlaying)
        {
            if (b != GetComponent<BoxCollider2D>().enabled && Time.timeScale > 0)
            {
                Sprite sprx = sprites[0];
                sprites[0] = sprites[1];
                sprites[1] = sprx;
                a = GetComponent<BoxCollider2D>().enabled;
            }
            b = GetComponent<BoxCollider2D>().enabled;

            /*if (buttonType == Type.Normal && GetComponent<BoxCollider2D>().enabled)
            {
               RaycastHit2D[] r = Physics2D.BoxCastAll(transform.position, GetComponent<BoxCollider2D>().size, 0, new Vector2(0, 0), 0f, 1051392,transform.position.z-8, transform.position.z + 8);

                bool jaat = false;
                foreach (var rr in r)
                {
                    if (rr.collider.GetComponent<Rigidbody2D>() != null && !rr.collider.GetComponent<Rigidbody2D>().isKinematic)
                    {
                        jaat = true;
                        break;
                    }
                }

                if (r.Length >= 1 && !a && Time.timeScale > 0 && jaat)
                {
                    a = true;
                    GetComponent<SpriteRenderer>().sprite = sprites[1];
                    GetComponent<AudioSource>().clip = onSound;
                    GetComponent<AudioSource>().Play();
                    Utilities.ChangeSwitchRequest(realID);
                }

                if ((r.Length == 0 && a && Time.timeScale > 0) || (a && !jaat))
                {
                    a = false;
                    GetComponent<SpriteRenderer>().sprite = sprites[0];
                    GetComponent<AudioSource>().clip = offSound;
                    GetComponent<AudioSource>().Play();
                    Utilities.ChangeSwitchRequest(realID);
                }

            }*/

            if (buttonType == Type.Normal && !GetComponent<BoxCollider2D>().enabled && Time.timeScale > 0 && a)
            {
                a = false;
                GetComponent<SpriteRenderer>().sprite = sprites[0];
            }
        }
        else
        {
            if (GetComponent<Renderer>().isVisible)
            {
                //Material mat = new Material(GetComponent<Renderer>().sharedMaterial);
                //GetComponent<Renderer>().material.SetColor("_RepColor", Utilities.colorCycle[ID]);
            }
        }
    }
}

