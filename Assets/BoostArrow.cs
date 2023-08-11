using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoostArrow : MonoBehaviour, IVisHelperMain {

    public float power;
    public float flipped = 1f;
    public bool swimBlock;
    public float swimDownwardPull;
    [Range(0.0f, 1.0f)]
    public float swimFriction;
    private float angle;
    private float size = 1f;
    private bool upAgain;
    public static HashSet<GameObject> swimMovedThisFrame = new HashSet<GameObject>() { };
    public bool isLava = false;
    public bool onlyWorksOnScreen = true;

    public static AudioSource fluxHSound;
    public static AudioSource fluxVSound;

    private Renderer rd;
    private Animator anim;
    private BoxCollider2D bc2;

    public static bool boostMovedThisFrame;

    Color MixColor(float l) // from 0 to 4 //
    {
        Color red = new Color(0.9f, 0.2f, 0.2f);
        Color yellow = new Color(0.9f, 0.78f, 0f);
        Color green = new Color(0.16f, 0.8f, 0.2f);
        Color blue = new Color(0f, 0.45f, 0.9f);
        Color newOne = Color.white;

        if (0f <= l && l < 1f)
        {
            newOne = Color.Lerp(red, yellow, l);
        }
        if (1f <= l && l < 2f)
        {
            newOne = Color.Lerp(yellow, green, l-1f);
        }
        if (2f <= l && l < 3f)
        {
            newOne = Color.Lerp(green, blue, l-2f);
        }
        if (3f <= l)
        {
            newOne = Color.Lerp(blue, red, l-3f);
        }

        newOne = Color.Lerp(Color.white, newOne, power / 40f);

        return newOne;
    }


    void Start()
    {
        boostMovedThisFrame = false;
        if (!fluxVSound)
        {
            fluxVSound = GameObject.Find("fluxVSound").GetComponent<AudioSource>();
        }
        if (!fluxHSound)
        {
            fluxHSound = GameObject.Find("fluxHSound").GetComponent<AudioSource>();
        }
        if (!swimBlock)
        {
            angle = ((transform.eulerAngles.z * Mathf.Deg2Rad) + (Mathf.PI / 2f));
            while (angle < 0f)
            {
                angle += 2f * Mathf.PI;
            }
            GetComponent<SpriteRenderer>().color = MixColor(Mathf.Repeat((angle * 2f) / Mathf.PI, 4f));
            size = 1f;
            upAgain = true;
        }
        rd = GetComponent<Renderer>();
        anim = GetComponent<Animator>();
        bc2 = GetComponent<BoxCollider2D>();
        vis = rd.isVisible;
        if (vis) { ((IVisHelperMain)this).Vis2(); }
        else { ((IVisHelperMain)this).Invis2(); }
    }
	
    void OnTriggerStay2D(Collider2D c)
    {
        /*if (c.gameObject.GetComponent<AmorphousGroundTileNormal>()false)
        {
            Physics2D.IgnoreCollision(c, GetComponent<BoxCollider2D>(), true);
        }
        else */
        if (upAgain && c.gameObject.GetComponent<Rigidbody2D>())
        {
            Rigidbody2D r2 = c.gameObject.GetComponent<Rigidbody2D>();
            if (!r2.isKinematic)
            {
                if (!swimBlock)
                {
                    upAgain = false;
                    Vector2 change = new Vector2(flipped * power * Mathf.Cos(angle), power * Mathf.Sin(angle));
                    if (c.GetComponent<BasicMove>())
                    {
                        BasicMove bm = c.GetComponent<BasicMove>();
                        if (bm.grounded > 0)
                        {
                            change.y = 0f;
                        }
                        if (!bm.boosted)
                        {
                            bm.fakePhysicsVel += change;
                            bm.boosted = true;
                        }
                    }
                    else
                    {
                        c.gameObject.GetComponent<Rigidbody2D>().velocity += change;
                        //c.GetComponent<BasicMove>().fakePhysicsVel += change;
                    }
                    boostMovedThisFrame = true;
                    if (fluxHSound)
                    {
                        if (!fluxHSound.isPlaying)
                        {
                            fluxHSound.Play();
                        }
                        fluxHSound.volume = EasingOfAccess.CubicIn(power*0.04f);
                        fluxHSound.pitch = 1f + 0.5f * (change.normalized.x);
                    }
                    if (fluxVSound)
                    {
                        if (!fluxVSound.isPlaying)
                        {
                            fluxVSound.Play();
                        }
                        fluxVSound.volume = EasingOfAccess.CubicIn(power * 0.04f);
                        fluxVSound.pitch = 1f + 0.5f * (change.normalized.y);
                    }
                    size = Mathf.Min(2f, size + 0.2f);
                }
                else
                {
                    //upAgain = false;
                    //Color col = GetComponent<SpriteRenderer>().color;
                    //GetComponent<SpriteRenderer>().color = new Color(col.r, col.g, col.b, Mathf.Clamp(col.a - 0.05f, 0.25f, 0.75f));
                    if (!swimMovedThisFrame.Contains(c.gameObject))
                    {
                        swimMovedThisFrame.Add(c.gameObject);
                        //c.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2((1f - swimFriction) * c.gameObject.GetComponent<Rigidbody2D>().velocity.x, (1f - swimFriction) * c.gameObject.GetComponent<Rigidbody2D>().velocity.y - swimDownwardPull);
                        BasicMove bm = c.GetComponent<BasicMove>();
                        Encontrolmentation e = c.GetComponent<Encontrolmentation>();
                        if (bm)
                        {
                            if (!(swimDownwardPull < 0f && bm.grounded > 0 && (e.currentState & 8UL) == 8UL))
                            {
                                bm.fakePhysicsVel = new Vector2((1f - swimFriction) * bm.fakePhysicsVel.x, (1f - swimFriction) * bm.fakePhysicsVel.y - swimDownwardPull);
                            }
                            bm.swimCount = 3;
                            bm.swimming = true;
                            bm.lavaCheck = false;

                            if (isLava)
                            {
                                c.GetComponent<KHealth>().overheat += 0.05f;
                                bm.lavaCheck = true;
                            }
                        }
                    }

                }
                
            }
        }
    }

    private bool vis;
    private static HashSet<BoostArrow> visList = new HashSet<BoostArrow>();

    void IVisHelperMain.Vis2()
    {
        if (anim) { anim.enabled = true; }
        bc2.enabled = true;
        vis = true;
        visList.Add(this);
    }

    void IVisHelperMain.Invis2()
    {
        if (anim) { anim.enabled = false; }
        bc2.enabled = !onlyWorksOnScreen;
        vis = false;
        visList.Remove(this);
    }

    private void OnBecameInvisible()
    {
        ((IVisHelperMain)this).Invis2();
    }

    private void OnBecameVisible()
    {
        ((IVisHelperMain)this).Vis2();
    }

    private void FakeUpdate()
    {
        if (upAgain)
        {
            if (!swimBlock)
            {
                size = Mathf.Max(1.0f, size - 0.1f);
            }
            else
            {
                //Color c = GetComponent<SpriteRenderer>().color;
                //GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, Mathf.Clamp(c.a + 0.05f, 0.25f, 0.75f));
            }
        }
        if (!swimBlock)
        {
            angle = ((transform.eulerAngles.z * Mathf.Deg2Rad) + (Mathf.PI / 2f));
            transform.localScale = new Vector3(size, size, 1f);
            GetComponent<BoxCollider2D>().size = new Vector2(16f / size, 16f / size);
            while (angle < 0f)
            {
                angle += 2f * Mathf.PI;
            }
        }
        upAgain = true;
        if (!boostMovedThisFrame)
        {
            fluxHSound.Stop();
            fluxVSound.Stop();
            boostMovedThisFrame = true;
        }
    }

    private void OnDestroy()
    {
        ((IVisHelperMain)this).Invis2();
    }

    public static void SharedUpdate()
    {
        visList.Remove(null);
        foreach (BoostArrow i in visList)
        {
            i.FakeUpdate();
        }
    }

    public bool GetAlwaysVisible()
    {
        return false;
    }
}
