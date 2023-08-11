using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PrimMovingPlatform : MonoBehaviour {

    public enum Type
    {
        Constant,Sine,Circle,ConstantList, ConstantGoalsAndSpeeds, ConstantTimesAndGoals
    }

    private Rigidbody2D r2;
    private ConveyorHelp convHelp;

    public Type type;
    public Vector2 velocity;
    public double t;
    public float o;
    public Transform optionalCenter;
    private Vector3 lp;
    public Vector2 dif;
    public Vector2[] velocityList;
    public float[] speedsList;
    public float[] timesList;
    public bool actuallyMoves = true;
    public bool autoDif = false;
    public bool moveRigidbody = true;
    public bool autoChangePlatVel = false;
    public bool pushPlayer = true;
    private float baseAngle1;
    private float timeOffset;
    private float baseDistance1;
    private bool s1;
    public Vector2 originalPosition;
    public int listIter;
    /*public static Sprite wireX;
    public static Sprite wireH;
    public static Sprite wireV;
    public static Sprite[] wireSmallCurve;
    public static Sprite[] wireLargeCurve;
    public static Sprite[] wireDiagonal;
    public static Sprite[] wireSmallAngle;*/
    //private Dictionary<Transform,Vector2> objv = new Dictionary<Transform, Vector2>();
    private List<Rigidbody2D> pushedPlrThisFrame = new List<Rigidbody2D>();

    void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        convHelp = GetComponent<ConveyorHelp>();
        lp = transform.localPosition;
        dif = Vector2.zero;
        timeOffset = 0;
        listIter = 0;
        s1 = false;
        pushedPlrThisFrame = new List<Rigidbody2D>();
        originalPosition = transform.localPosition;
        if (type == Type.Circle)
        {
            if (optionalCenter != null)
            {
                Vector2 v = transform.localPosition - optionalCenter.localPosition;
                baseAngle1 = Mathf.Atan2(v.y, v.x);
                baseDistance1 = v.magnitude;
            }
            else
            {
                baseAngle1 = Mathf.Atan2(velocity.y, velocity.x);
                baseDistance1 = velocity.magnitude;
            }
        }
    }

    void Push(Collision2D col)
    {
        if (pushPlayer && !GetComponent<ConveyorHelp>())
        {
            Rigidbody2D rb = col.rigidbody;
            BasicMove bm = null;
            if (rb)
            {
                bm = rb.GetComponent<BasicMove>();
            }
            if (rb && bm && !(bm.extraPlat == this) && !pushedPlrThisFrame.Contains(rb))
            {
                pushedPlrThisFrame.Add(rb);
                //rb.transform.position += (Vector3)(dif);
                //rb.MovePosition(rb.transform.position);
                bm.extraPerFrameVel = dif/Time.deltaTime;
            }
        }
    }

    //void OnCollisionStay2D(Collision2D col)
    //{
    //    Push(col);
    //}

    //void OnCollisionEnter2D(Collision2D col)
    //{
    //    Push(col);
    //}

    void Update () {
        
        if (Time.timeScale > 0 && enabled)
        {
            /*foreach (Transform t in transform)
            {
                if (t.gameObject.GetComponent<Rigidbody2D>() != null && t != transform)
                {
                    objv.Add(t, t.gameObject.GetComponent<Rigidbody2D>().velocity);
                    //t.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                }
            }*/
            if (actuallyMoves)
            {
                if (type == Type.Constant)
                {
                    //transform.position = lp + new Vector3(velocity.x * Time.timeScale * 0.01666666f, velocity.y * Time.timeScale * 0.01666666f);
                    if (GetComponent<Rigidbody2D>() != null)
                    {
                        GetComponent<Rigidbody2D>().velocity = velocity * Time.timeScale;
                    }
                    else
                    {
                        transform.localPosition += (Vector3)velocity * Time.deltaTime;
                    }
                }
                if (type == Type.Sine)
                {
                    //transform.position = lp + new Vector3(velocity.x * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f, velocity.y * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f);
                    double x1 = (DoubleTime.ScaledTimeSinceLoad - timeOffset) * t + o;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(velocity.x * (float)System.Math.Cos(x1), velocity.y * (float)System.Math.Cos(x1));
                }
                if (type == Type.Circle)
                {
                    //transform.position = lp + new Vector3(velocity.x * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f, velocity.y * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f);
                    double x1 = (DoubleTime.ScaledTimeSinceLoad - timeOffset) * t + baseAngle1;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(-baseDistance1 * (float)System.Math.Sin(x1), baseDistance1 * (float)System.Math.Cos(x1));
                }
                if (type == Type.ConstantList)
                {
                    //transform.position = lp + new Vector3(velocity.x * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f, velocity.y * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f);
                    while (DoubleTime.ScaledTimeSinceLoad - t >= timesList[listIter])
                    {
                        t += timesList[listIter];
                        listIter = (listIter + 1) % velocityList.Length;
                    }
                    GetComponent<Rigidbody2D>().velocity = velocityList[listIter];
                }
                if (type == Type.ConstantGoalsAndSpeeds || type == Type.ConstantTimesAndGoals)
                {
                    //transform.position = lp + new Vector3(velocity.x * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f, velocity.y * Mathf.Cos(DoubleTime.ScaledTimeSinceLoad * t) * Time.timeScale * 0.01666666f);

                    if (!s1)
                    {
                        Vector2 newPos = originalPosition;
                        if (type == Type.ConstantGoalsAndSpeeds)
                        {
                            newPos = Vector2.MoveTowards(transform.localPosition, velocityList[listIter] + originalPosition, speedsList[listIter] * Time.timeScale);
                        }
                        if (type == Type.ConstantTimesAndGoals)
                        {
                            newPos = Vector2.Lerp(originalPosition+velocityList[(listIter+velocityList.Length-1)%velocityList.Length], originalPosition + velocityList[listIter], Mathf.Clamp01((float)(DoubleTime.ScaledTimeSinceLoad - t)/speedsList[listIter]));

                        }
                        Rigidbody2D rig = GetComponent<Rigidbody2D>();
                        if (Time.fixedDeltaTime > 0.0001f)
                        {
                            rig.velocity = (newPos - (Vector2)transform.localPosition) / Time.fixedDeltaTime;
                            transform.localPosition = newPos;
                            rig.MovePosition(transform.position);
                            
                        }
                        if (type == Type.ConstantGoalsAndSpeeds && (Vector2)transform.localPosition == velocityList[listIter] + originalPosition)
                        {
                            t = DoubleTime.ScaledTimeSinceLoad;
                            s1 = true;
                        }
                        if (type == Type.ConstantTimesAndGoals && DoubleTime.ScaledTimeSinceLoad-t >= speedsList[listIter])
                        {
                            t += speedsList[listIter];
                            s1 = true;
                        }
                    }
                    else
                    {
                        Rigidbody2D rig = GetComponent<Rigidbody2D>();
                        rig.velocity = Vector2.zero;
                        rig.MovePosition((Vector2)transform.parent.position + originalPosition + velocityList[listIter]);
                    }
                    /*else*/ if (s1 && DoubleTime.ScaledTimeSinceLoad-t >= timesList[listIter])
                    {
                        Rigidbody2D rig = GetComponent<Rigidbody2D>();
                        rig.velocity = Vector2.zero;
                        rig.MovePosition(originalPosition+velocityList[listIter]+(Vector2)transform.parent.position);
                        if (type == Type.ConstantTimesAndGoals)
                        {
                            t += timesList[listIter];
                        }
                        listIter = (listIter + 1) % velocityList.Length;
                        s1 = false;
                    }
                }
            }
            /*int ii = 0;
            foreach (Transform t in objv.Keys.ToArray())
            {
                t.gameObject.transform.position += transform.position - lp;
                t.gameObject.GetComponent<Rigidbody2D>().velocity = objv.Values.ToArray()[ii];
                ii++;
            }*/
            //Rigidbody2D r2 = GetComponent<Rigidbody2D>();
            if (autoDif) //conveyor belts have to do something different
            {
                dif = velocity * Time.deltaTime;
                if (moveRigidbody && r2 && r2.bodyType != RigidbodyType2D.Static)
                {
                    r2.velocity = velocity;
                }
            }
            else
            {
                dif = transform.position - lp;
                if (moveRigidbody && r2)
                {
                    r2.velocity = dif / Time.deltaTime;
                }
            }

            if (autoChangePlatVel)
            {
                velocity = dif / Time.deltaTime;
            }
            lp = transform.position;
            
            //objv.Clear();
        }
        else
        {
            //GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        if (convHelp == null)
        {
            pushedPlrThisFrame.Clear();
        }
    }
}
