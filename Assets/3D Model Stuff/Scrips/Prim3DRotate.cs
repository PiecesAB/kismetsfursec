using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Prim3DRotate : MonoBehaviour, IVisHelperMain {

    public enum RotateType
    {
        Linear,SineZ,CircularZ
    }

    public enum Axis
    {
        X,Y,Z
    }

    public Axis axis;
    public RotateType rotationType;
    public Vector3 sineRotAxis = Vector3.forward;
    public float speed;
    public float offset;
    public float period;
    public float velStrength;
    public float timeInfluence;
    public float fastestAngleForCircular;
    public float alternateLinearSignTime = Mathf.Infinity;
    public bool noSuddenJumps = false;
    [Header("offScreenChecker null, if not stop when off screen")]
    public Renderer offScreenChecker;
    public bool playAxeSoundIfAvailable = true;
    public bool alwaysVisible = false;

    [HideInInspector]
    public Func<bool, bool> CallOnVisibleChange = null;
    
    private float prevTime;
    private Rigidbody2D r2; //cache
    private AudioSource sound;
    private Vector3 oeu;

    //private Vector2 lcm;
    [HideInInspector]
    public float vv;

	// Use this for initialization
	void Start () {
        prevTime = 0f;
        r2 = GetComponent<Rigidbody2D>();
        sound = GetComponent<AudioSource>();
        if (rotationType == RotateType.Linear)
        {
            if (axis == Axis.X)
            {
                transform.localEulerAngles += new Vector3(offset, 0, 0);
            }
            if (axis == Axis.Y)
            {
                transform.localEulerAngles += new Vector3(0, offset, 0);
            }
            if (axis == Axis.Z)
            {
                transform.localEulerAngles += new Vector3(0, 0, offset);
            }
        }
        if (rotationType == RotateType.SineZ)
        {
            oeu = transform.localEulerAngles;
            vv = 0;
            transform.localEulerAngles = oeu + sineRotAxis*speed*(float)System.Math.Sin(vv*period*6.2831853f);
            if (r2)
            {
                r2.centerOfMass = transform.localPosition;
            }
            //lcm = GetComponent<Rigidbody2D>().worldCenterOfMass;
        }
        if (!offScreenChecker || offScreenChecker.isVisible || alwaysVisible) { ((IVisHelperMain)this).Vis2(); }
        else { ((IVisHelperMain)this).Invis2(); }
        if (offScreenChecker && !alwaysVisible)
        {
            VisHelper ph = offScreenChecker.gameObject.AddComponent<VisHelper>();
            ph.main = this;
        }
    }

    private bool vis;
    private static HashSet<Prim3DRotate> visList = new HashSet<Prim3DRotate>();

    private void FakeUpdate()
    {
        float lol = Mathf.LerpUnclamped(1f, Time.timeScale, timeInfluence);
        double lol2 = (1f - timeInfluence)*DoubleTime.UnscaledTimeSinceLoad + (timeInfluence)*DoubleTime.ScaledTimeSinceLoad;
        if (rotationType == RotateType.Linear && this)
        {
            float nextTime = prevTime + (0.016666666f * lol);
            if (axis == Axis.X)
            {
                transform.localEulerAngles += new Vector3(speed * lol, 0, 0);
            }

            if (axis == Axis.Y)
            {
                transform.localEulerAngles += new Vector3(0, speed * lol, 0);
            }

            if (axis == Axis.Z)
            {
                transform.localEulerAngles += new Vector3(0, 0, speed * lol);
            }

            if ((prevTime % alternateLinearSignTime) > (nextTime % alternateLinearSignTime))
            {
                speed = -speed;
            }
            prevTime = nextTime;
            if (r2)
            {
                r2.angularVelocity = speed * 60f;
                r2.MoveRotation(transform.eulerAngles.z);
            }
        }

        if (rotationType == RotateType.CircularZ)
        {
            /*float z = transform.eulerAngles.z;
            float an = z - fastestAngleForCircular;

            an = System.Math.Abs((Mathf.Repeat(an, 360f) - 180f) / 180f);
            transform.localEulerAngles += new Vector3(0, 0, (speed + (velStrength * (0.5f + an))) * lol);*/

            float num = Mathf.Abs(velStrength);
            float coeff = Mathf.Abs(velStrength) / (Mathf.Abs(speed) + Mathf.Abs(velStrength));
            double x = lol2 * (velStrength + speed) + offset*Mathf.Deg2Rad;
            float y = (float)((x + coeff * System.Math.Sin(x)) % 6.2831853);
            transform.localEulerAngles = y*Mathf.Rad2Deg*Vector3.forward;
        }

        if (rotationType == RotateType.SineZ && Time.timeScale != 0)
        {
            double larg = (offset + vv) * period * 6.2831853f;
            vv += 0.01666666f * lol;
            double arg = (offset + vv) * period * 6.2831853f;
            if (r2)
            {
                /*float dr = oeu.z + speed * Mathf.Sin((offset + vv) * period * 6.2831853f) - transform.localEulerAngles.z;
                print(Mathf.Abs(Mathf.Repeat(dr + 180f, 360f) - 180f));
                if (Mathf.Abs(Mathf.Repeat(dr + 180f, 360f) - 180f) < 5f * Time.timeScale)*/
                {
                    r2.MoveRotation(oeu.z + speed * (float)System.Math.Sin(arg));
                    r2.angularVelocity = Mathf.Cos(vv * period * speed * 6.2831853f) * Mathf.Rad2Deg;
                }
            }
            else
            {
                transform.localEulerAngles = oeu + sineRotAxis * speed * (float)System.Math.Sin(arg);
            }

            if (sound && (int)((larg + 1.5707963) / 3.14159265) < (int)((arg + 1.5707963) / 3.14159265) && playAxeSoundIfAvailable)
            {
                sound.Stop(); sound.Play();
            }
        }

        /*if (rotationType == RotateType.PerlinNoiseAxis)
        {

        }*/
    }

    void IVisHelperMain.Vis2()
    {
        vis = true;
        visList.Add(this);
        CallOnVisibleChange?.Invoke(true);
    }

    void IVisHelperMain.Invis2()
    {
        vis = false;
        visList.Remove(this);
        CallOnVisibleChange?.Invoke(false);
    }

    private void OnDestroy()
    {
        ((IVisHelperMain)this).Invis2();
    }

    public static void SharedUpdate()
    {
        visList.Remove(null);
        foreach (Prim3DRotate i in visList)
        {
            if (!i.enabled) { continue; }
            i.FakeUpdate();
        }
    }

    bool IVisHelperMain.GetAlwaysVisible()
    {
        return alwaysVisible;
    }

    // Update is called once per frame
}
