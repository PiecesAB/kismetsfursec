using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laserPin : MonoBehaviour
{

    public Mode mode = Mode.Repulse;
    public SkinnedMeshRenderer laserCannonPart;
    public Renderer visibleCheck;
    public SpriteRenderer laserCircle;
    public LineRenderer laserLineRend;
    public float laserSegLenMax;
    public int laserSegMax;
    public float chargeFrameTime;
    public bool on;
    public Material activeMatFire;
    public Material activeMatAim;
    public bool[] activeFireOn;
    public float[] activeFirePattern;
    public bool activeFireLoop;
    public float damageMult = 1f;
    public AudioClip lightLaserSound;
    public AudioClip hardLaserSound;

    private int acti = 0;
    private double actt = 0.0;
    private AudioSource aud;

    private float currCharge;
    private List<GameObject> wallModeColliders;
    private bool activeDamaging;

    public enum Mode
    {
       Repulse,Active,Wall
    }

    void Start()
    {
        aud = GetComponent<AudioSource>();
        wallModeColliders = new List<GameObject>(laserSegMax);
        acti = 0;
        actt = 0.0;
        if (mode == Mode.Active)
        {
            activeDamaging = activeFireOn[0];
        }
    }

    Vector3 v3_v2(Vector2 v)
    {
        return new Vector3(v.x, v.y, -32);
    }

    void Update()
    {
        if (mode == Mode.Wall)
        {
            for (int col = 0; col < wallModeColliders.Count; col++)
            {
                Destroy(wallModeColliders[col]);
            }
            wallModeColliders.Clear();
        }

        //update raycast
        if (on && visibleCheck.isVisible)
        {
            if (currCharge == 0)
            {
                Vector2 offset = laserLineRend.transform.position;
                List<Vector2> points = new List<Vector2>() { offset };
                Vector2 ldir = transform.right;

                for (int i = 0; i < laserSegMax; i++)
                {
                    RaycastHit2D rh = Physics2D.Raycast(offset, ldir, laserSegLenMax, 4195072 + 1048576 /*8 and 9 and 22 and 20*/);

                    //collision stuff
                    if (rh.collider != null)
                    {
                        if (mode == Mode.Repulse)
                        {
                            float mag = (Time.timeScale == 0f) ? 0f : 15f / Time.timeScale;
                            if (rh.collider.gameObject.GetComponent<BasicMove>() != null)
                                rh.collider.gameObject.GetComponent<BasicMove>().fakePhysicsVel += mag * ldir;
                            else if (rh.collider.gameObject.GetComponent<Rigidbody2D>() != null && !rh.collider.gameObject.GetComponent<Rigidbody2D>().isKinematic)
                                rh.collider.gameObject.GetComponent<Rigidbody2D>().velocity += mag * ldir;
                        }

                        if (mode == Mode.Active)
                        {
                            KHealth kh = rh.collider.gameObject.GetComponent<KHealth>();
                            if (kh && activeDamaging)
                            {
                                kh.ChangeHealth(-5f * damageMult, "laser");
                                if (aud && hardLaserSound && (!aud.isPlaying || aud.clip != hardLaserSound))
                                {
                                    aud.Stop();
                                    aud.clip = hardLaserSound;
                                    aud.volume = 0.7f;
                                    aud.Play();
                                }
                            }
                            else
                            {
                                if (aud && lightLaserSound && (!aud.isPlaying || aud.clip != lightLaserSound))
                                {
                                    aud.Stop();
                                    aud.clip = lightLaserSound;
                                    aud.volume = 0.25f;
                                    aud.Play();
                                }
                            }
                        }
                    }

                    if (rh.collider == null)
                    {
                        points.Add(offset + (laserSegLenMax * ldir));
                        if (aud && lightLaserSound && (!aud.isPlaying || aud.clip != lightLaserSound))
                        {
                            aud.Stop();
                            aud.clip = lightLaserSound;
                            aud.volume = 0.25f;
                            aud.Play();
                        }
                    }
                    else
                    {
                        points.Add(rh.point);
                    }

                    if (mode == Mode.Wall)
                    {
                        GameObject gc = new GameObject("Collider Junk");
                        gc.layer = 8;
                        Transform gct = gc.transform;
                        gct.SetParent(transform);
                        gct.position = Vector3.Lerp(points[i], points[i + 1], 0.5f);
                        float gcAngle = Mathf.Atan2(ldir.y, ldir.x)*Mathf.Rad2Deg;
                        gct.eulerAngles = new Vector3(0f, 0f, gcAngle);
                        BoxCollider2D gcb = gc.AddComponent<BoxCollider2D>();
                        gcb.offset = Vector2.zero;
                        gcb.size = new Vector2((points[i + 1] - points[i]).magnitude, laserLineRend.startWidth);
                        wallModeColliders.Add(gc);
                    }

                    if (rh.collider != null &&
                        rh.collider.GetComponent<primExtraTags>() != null &&
                        rh.collider.GetComponent<primExtraTags>().tags.Contains("mirror"))
                    {
                        offset = rh.point - ldir; //-ldir is to not get stuck
                        ldir = ldir - (2f * Vector2.Dot(ldir, rh.normal) * rh.normal);
                    }
                    else
                    {
                        break;
                    }
                }

                laserLineRend.positionCount = points.Count;
                laserLineRend.SetPositions(System.Array.ConvertAll(points.ToArray(), v3_v2));
                laserCannonPart.SetBlendShapeWeight(0,100f);
                laserCircle.transform.localScale = Vector3.one;
            }
            else
            {
                float l = Mathf.Clamp01(1f - (currCharge / chargeFrameTime));
                laserCircle.transform.localScale = Vector3.one * l;
                laserCannonPart.SetBlendShapeWeight(0, l*100f);
                currCharge -=Time.timeScale;
            }

            laserCircle.enabled = true;
        }
        else
        {
            laserLineRend.positionCount = 0;
            laserCircle.enabled = false;
            laserCannonPart.SetBlendShapeWeight(0, Mathf.MoveTowards(laserCannonPart.GetBlendShapeWeight(0),0f,10f));
            currCharge = chargeFrameTime;

            if (aud) { aud.Stop(); }
        }
        
        if (mode == Mode.Active)
        {
            if (activeDamaging)
            {
                laserLineRend.material = activeMatFire;
                laserLineRend.startWidth = laserLineRend.endWidth = 8;
            }
            else
            {
                laserLineRend.material = activeMatAim;
                laserLineRend.startWidth = laserLineRend.endWidth = 1;
                if (laserCircle.transform.localScale == Vector3.one)
                {
                    laserCircle.transform.localScale = Vector3.one * 0.5f;
                }
            }

            actt += 0.016666666666666666666 * Time.timeScale;

            if (acti < activeFirePattern.Length)
            {
                while (actt >= activeFirePattern[acti])
                {
                    actt -= activeFirePattern[acti];
                    ++acti;
                    if (activeFireLoop && acti >= activeFirePattern.Length) { acti = 0; }
                    if (acti < activeFirePattern.Length)
                    {
                        activeDamaging = activeFireOn[acti];
                    }
                }
            }
        }
    }
}
