using UnityEngine;
using System.Collections;

public class Sniper1 : MonoBehaviour {

    public float originalRotation;
    public Renderer gunObj;
    public LineRenderer laser;
    public BasicMove[] targets;
    public bool followsTargets;
    public float followSpeedMultiplier;
    private bool lol = false;
    private float oldAngle;
    public double discharge;
    public AudioSource sniperSound;
    private bool zx = false;
    private byte bt = 0xF0;

	void Start () {
        originalRotation = transform.eulerAngles.z;
        targets = FindObjectsOfType<BasicMove>();
        discharge = -5f;
        oldAngle = 9001f;
    }
	
    float at2(float y, float x)
    {
        float a = Fastmath.FastAtan2(y, x); //quantity over quality.
        if (a < 0f)
        {
            a += 6.2831853f;
        }
        return a;
    }

	void Update () {
	    if (zx && (!followsTargets || (System.Math.Abs(transform.position.x-Camera.main.transform.position.x) <= 208f &&
            System.Math.Abs(transform.position.y - Camera.main.transform.position.y) <= 168f)))
        {
            RaycastHit2D r = Physics2D.Raycast(transform.position + transform.right * 76f, transform.right, 2000f, 1049344, transform.position.z - 64f, transform.position.z + 64f);

            if (followsTargets && DoubleTime.UnscaledTimeRunning - discharge >= 0.5f && Time.timeScale > 0f)
            {
                BasicMove victim = null;
                float d = 2000f;
                bt = 0xF0;
                foreach (var t in targets)
                {
                    if (t != null)
                    {
                        float dt = Fastmath.FastV2Dist(transform.position, t.transform.position);
                        if (dt < d /*&& dt >= 68*/)
                            victim = t;
                        d = dt;
                    }
                }
                float my = transform.eulerAngles.z * Mathf.Deg2Rad;
                float wa = originalRotation * Mathf.Deg2Rad;

                if (victim != null)
                {
                    wa = at2(victim.transform.position.y - transform.position.y,
                        victim.transform.position.x - transform.position.x);

                    if ((System.Math.Abs(wa - my) <= 0.03f && r.collider.gameObject.layer == 20 /*|| (oldAngle != 9001f && Mathf.Sign(wa-my) != Mathf.Sign(wa - oldAngle))*/) && d >= 68)
                    {
                        KHealth k = victim.GetComponent<KHealth>();
                        if (k != null)
                        {
                            sniperSound.Stop();
                            sniperSound.Play();
                            victim.AddBlood(victim.transform.position, Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.forward));
                            k.ChangeHealth(-36f, "sniper");
                            discharge = DoubleTime.UnscaledTimeRunning;
                        }
                    }
                    oldAngle = wa;
                }
                else
                {
                    oldAngle = 9001f;
                }

                float x = wa - my;
                if (x < 0f)
                {
                    x += 6.2831853f;
                }
                float sp = (((x - 6.2831853f) * -x + 1.2f) / 6f) * -Mathf.Sign(x - Mathf.PI);
                sp = sp * (followSpeedMultiplier / d) * Time.timeScale;
                float spd = sp * Mathf.Deg2Rad;
                if (spd >= x)
                {
                    transform.eulerAngles = Vector3.forward * wa * Mathf.Rad2Deg;
                }
                else
                {
                    transform.Rotate(Vector3.forward * sp);
                }

            }
            else if (DoubleTime.UnscaledTimeRunning - discharge >= 0.5f && Time.timeScale > 0f)
            {
                BasicMove victim = null;
                float d = 2000f;
                bt = (byte)Mathf.Max(0,bt-10);
                foreach (var t in targets)
                {
                    if (t != null)
                    {
                        float dt = Fastmath.FastV2Dist(transform.position, t.transform.position);
                        if (dt < d /*&& dt >= 68*/)
                            victim = t;
                        d = dt;
                    }
                }
                float my = transform.eulerAngles.z * Mathf.Deg2Rad;
                float wa = originalRotation * Mathf.Deg2Rad;

                if (victim != null)
                {
                    wa = at2(victim.transform.position.y - transform.position.y,
                        victim.transform.position.x - transform.position.x);
                    if (System.Math.Abs(wa - my) <= 0.4f || System.Math.Abs(wa - my) >= 5.89f)
                    {
                        float we = -System.Math.Abs(System.Math.Abs(wa - my) - Mathf.PI) + Mathf.PI;
                        bt = (byte)(-648.6486f * we + 259.4595f);
                    }
                    float dddd = 3f / Fastmath.FastV2Dist(transform.position,victim.transform.position);
                        if (((System.Math.Abs(wa - my) <= dddd || System.Math.Abs(wa - my) >= 6.2831853f-dddd) || (oldAngle != 9001f && System.Math.Abs(Mathf.Sign(wa - my) - Mathf.Sign(wa - oldAngle)) >= 5.5f)) && r.collider.gameObject.layer == 20 && d >= 68)
                    {
                        KHealth k = victim.GetComponent<KHealth>();
                        if (k != null)
                        {
                            sniperSound.Stop();
                            sniperSound.Play();
                            victim.AddBlood(victim.transform.position, Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.forward));
                            k.ChangeHealth(-victim.GetComponent<BasicMove>().Damage*5f, "sniper");
                            discharge = DoubleTime.UnscaledTimeRunning;
                        }
                    }
                    oldAngle = wa;
                }
                else
                {
                    oldAngle = 9001f;
                }
            }


            double el = DoubleTime.UnscaledTimeRunning - discharge;
            if (el >= 0.5)
            {
                laser.startWidth = laser.endWidth = 1f;
                laser.startColor = new Color32(0xFF, 0x80, 0x80, bt);
                laser.endColor = new Color32(0xFF, 0x00, 0x00, bt);
            }
            else
            {
                float ee = (float)System.Math.Min(0.5 / el,60.0);
                laser.startWidth = laser.endWidth = ee;
                laser.startColor = laser.endColor = new Color32(0xC0, 0xB0, 0x00, bt);
            }

            if (r.collider)
            {
                laser.SetPosition(0, new Vector3(0, 0, 12));
                laser.SetPosition(1, new Vector3(-r.distance - 38, 0, 12));
            }
            else
            {
                laser.SetPosition(0, new Vector3(0, 0, 12));
                laser.SetPosition(1, new Vector3(-2000, 0, 12));
            }
            lol = true;
        }
        else
        {
            float zz = transform.eulerAngles.z;
            transform.eulerAngles = Vector3.forward*originalRotation;
            lol = false;
            laser.SetPosition(0, Vector3.zero);
            laser.SetPosition(1, Vector3.zero);
            bt = 0x00;
            oldAngle = 9001f;
        }

        zx = true;
    }
}
