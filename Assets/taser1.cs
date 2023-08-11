using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class taser1 : MonoBehaviour
{
    public enum Status
    {
        Hidden, Out, Shocked
    }

    public Status status;
    public Transform meshObj;
    public LineRenderer laser;
    public Rigidbody2D colBody;
    public AudioClip electricAmbience;
    public AudioClip stunSound;
    public SpriteRenderer electricCircle;

    private const float showDistance = 96f;
    private const float shockDistance = 26f;
    private const float shockTime = 3.6f;
    private static Vector3 outPos = new Vector3(0,0,0);
    private static Vector3 hiddenPos = new Vector3(0, -27, 0);
    private static Vector3 lineEndLeft = new Vector3(-6, 0, 0);
    private static Vector3 lineEndRight = new Vector3(6, 0, 0);
    private static Vector3 shockOffset = new Vector3(0, 27, 0);
    private const int stunFrames = 160;


    private double endAftershockTime = Mathf.Infinity;

    void Start()
    {

    }

    Vector2 rotateVec(Vector2 v, float t)
    {
        float ct = Mathf.Cos(t);
        float st = Mathf.Sin(t);
        return new Vector2(v.x * ct - v.y * st, v.x * st + v.y * ct);
    }

    void Update()
    {
        if (Time.timeScale > 0)
        {
            //update status
            bool shockStartThisFrame = false;
            bool near = false;
            float nearestDist = showDistance;
            Vector3 shockOffset2 = rotateVec(shockOffset, transform.eulerAngles.z * Mathf.Deg2Rad);
            Vector3 nearest = Vector3.negativeInfinity;

            AudioSource asr = GetComponent<AudioSource>();

            if (!meshObj.GetComponent<Renderer>().isVisible)
            {
                status = Status.Hidden;
            }
            else
            {
                foreach (GameObject g in LevelInfoContainer.allBoxPhysicsObjects)
                {
                    if (g)
                    {
                        float d = (g.transform.position - shockOffset2 - transform.position).magnitude;
                        if (d <= showDistance)
                        {
                            near = true;
                            if (d <= shockDistance)
                            {
                                shockStartThisFrame = true;
                            }
                        }

                        if (d <= nearestDist)
                        {
                            nearestDist = d;
                            nearest = transform.InverseTransformPoint(g.transform.position - shockOffset2);
                        }
                    }
                }

                if (status == Status.Shocked)
                {
                    if (endAftershockTime < DoubleTime.ScaledTimeSinceLoad)
                    {
                        if (near)
                        {
                            status = Status.Out;
                        }
                        else
                        {
                            status = Status.Hidden;
                        }
                    }
                }
                else
                {
                    if (shockStartThisFrame)
                    {
                        status = Status.Shocked;
                        endAftershockTime = DoubleTime.ScaledTimeSinceLoad + shockTime;
                        // oh boy this will make a lot of garbage
                        RaycastHit2D[] ccr = Physics2D.CircleCastAll(transform.position + shockOffset2, shockDistance, Vector2.right, 0.0000001f, 1051392);
                        Debug.DrawLine(transform.position + shockOffset2 - transform.right * shockDistance,
                            transform.position + shockOffset2 + transform.right * shockDistance, Color.red, 1f);
                        Debug.DrawLine(transform.position + shockOffset2 - transform.up * shockDistance,
                            transform.position + shockOffset2 + transform.up * shockDistance, Color.red, 1f);
                        //
                        foreach (RaycastHit2D ob in ccr)
                        {
                            //print(ob.collider.gameObject.name);
                            KHealth kh = ob.collider.GetComponent<KHealth>();
                            if (kh)
                            {
                                kh.stunnedCantMove = stunFrames;
                            }
                        }

                        asr.Stop();
                        asr.clip = stunSound;
                        asr.volume = 0.7f;
                        asr.loop = false;
                        asr.Play();

                        Color ec2 = electricCircle.color;
                        electricCircle.transform.localEulerAngles = new Vector3(0, 0, Fakerand.Single(0f, 360f));
                        electricCircle.color = new Color(ec2.r, ec2.g, ec2.b, 1f);
                    }
                    else if (near && status == Status.Hidden)
                    {
                        status = Status.Out;
                    }
                    else if (!near && status == Status.Out)
                    {
                        status = Status.Hidden;
                    }
                }
            }

            
            //update all stuff
            switch (status)
            {
                case Status.Hidden:
                    meshObj.localPosition = Vector3.MoveTowards(meshObj.localPosition, hiddenPos, 3f);
                    for (int i = 1; i < 7; i++)
                    {
                        float l = i / 8f;
                        Vector2 r = Fakerand.UnitCircle();
                        laser.SetPosition(i, Vector2.Lerp(lineEndLeft, lineEndRight, l) + (1 * r));
                    }
                    laser.material.color = Color.white;
                    asr.Stop();
                    asr.clip = null;
                    break;
                case Status.Out:
                    meshObj.localPosition = Vector3.MoveTowards(meshObj.localPosition, outPos, 3f);
                    float t1 = 0f;
                    for (int i = 1; i < 7; i++)
                    {
                        float l = i / 8f;
                        Vector2 r = Fakerand.UnitCircle();
                        r = Vector2.Lerp(lineEndLeft, lineEndRight, l) + (4 * r);
                        //print(nearestDist);
                        if (nearest.x != Mathf.NegativeInfinity)
                        {
                            t1 = (-0.01315789f * nearestDist) + 1.3421053f;
                            t1 = 1f - Fastmath.FastSqrt(1f - t1);
                            r = Vector2.Lerp(r, nearest, t1);
                        }
                        laser.SetPosition(i, r);
                    }
                    laser.material.color = Color.white;
                    
                    if (asr.clip != electricAmbience)
                    {
                        asr.Stop();
                        asr.clip = electricAmbience;
                        asr.loop = true;
                    }
                    if (!asr.isPlaying)
                    {
                        asr.Play();
                    }
                    asr.volume = Mathf.Lerp(0.1f,0.4f,t1);
                    break;
                case Status.Shocked:
                    meshObj.localPosition = outPos;
                    for (int i = 1; i < 7; i++)
                    {
                        float l = i / 8f;
                        Vector2 r = Fakerand.UnitCircle();
                        laser.SetPosition(i, Vector2.Lerp(lineEndLeft, lineEndRight, l) + (1 * r));
                    }
                    double recharge = 1.0 - ((endAftershockTime - DoubleTime.ScaledTimeSinceLoad) / shockTime);
                    //print(recharge);
                    laser.material.color = new Color(1f, 1f, 1f, (float)recharge);
                    break;
                default:
                    break;
            }

            colBody.MovePosition(colBody.transform.position);
            Color ec = electricCircle.color;
            electricCircle.color = new Color(ec.r, ec.g, ec.b, Mathf.MoveTowards(ec.a, 0f, 0.033333f));
            shockStartThisFrame = false;
        }
    }
}
