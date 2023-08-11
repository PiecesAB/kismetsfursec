using UnityEngine;
using System.Collections;

public class GunBehaviors : MonoBehaviour {

    public enum Type
    {
        MachineGun, LaserCannon
    }

    public Type gunType;
    [Header("Firing speed in shots per second")]
    public float fireSpeed;
    public float rotationSpeed;
    public float range;
    public GameObject attachment;
    public LineRenderer line;
    public ParticleSystem bulletMaker;
    public float bulletSpeed;
    [Header("This is not Bullet Damage. Set that in the primitive behavior")]
    public float gunDamage;

    public bool enabledShoot;
    private GameObject target;
    

	// Use this for initialization
	void Start () {
        line.sortingLayerName = "Objects";
        if (gunType == Type.MachineGun)
        StartCoroutine(FireMachine());
	}
	
    public IEnumerator FireMachine()
    {
        while (0 < 1) //if this ever happens i'll eat my hat (figuratively)
        {
            if (target && GetComponent<Renderer>().isVisible && Time.timeScale > 0 && enabledShoot)
            {
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                p.position = transform.TransformPoint(bulletMaker.transform.localPosition);
                float zz = transform.eulerAngles.z-90;
                p.velocity = new Vector3(bulletSpeed * Mathf.Cos(zz * Mathf.Deg2Rad), bulletSpeed * Mathf.Sin(zz * Mathf.Deg2Rad), 0);

                bulletMaker.Emit(p, 1);
            }
            double tt = DoubleTime.ScaledTimeSinceLoad;
            double t2 = tt + (1 / fireSpeed);
            while (t2 > DoubleTime.ScaledTimeSinceLoad)
            {
                float z = Mathf.InverseLerp((float)tt,(float)t2,(float)DoubleTime.ScaledTimeSinceLoad);
                Color cc = (z < 0.5f) ? (Color.Lerp(Color.white, Color.yellow, z * 2)) : (Color.Lerp(Color.yellow, Color.red, z * 2 - 0.5f));
                line.SetColors(cc-new Color(0,0,0,0.7f), cc - new Color(0, 0, 0, 0.9f));
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    // Update is called once per frame
    void Update()
    {

        ///////////////////////////////////////////////////////// MachineGun
        if (gunType == Type.MachineGun || gunType == Type.LaserCannon)
        {
            float dist = range;
            target = null;
            if (GetComponent<Renderer>().isVisible)
            {
                foreach (var i in GameObject.FindGameObjectsWithTag("Player")) //picks the closest target.
                {
                    float dist2 = Fastmath.FastV2Dist(i.transform.position, attachment.transform.position);
                    if (dist2 < range)
                    {
                        dist = dist2;
                        target = i;
                    }
                }


                if (target != null && Time.timeScale > 0)
                {
                    line.enabled = true;
                    float ma = transform.eulerAngles.z;
                    //Vector3.down* Mathf.Max(0, Fastmath.FastV2Dist(target.transform.position, transform.position) - 24) + Vector3.forward
                    if (gunType == Type.MachineGun)
                    {
                        line.SetPosition(1, Vector3.down * Mathf.Max(0, range - 24) + Vector3.forward);
                    }
                    if (gunType == Type.LaserCannon)
                    {
                        line.transform.eulerAngles = Vector3.zero;
                        Vector2 rot = (new Vector2(Mathf.Cos((ma - 90) * Mathf.Deg2Rad), Mathf.Sin((ma - 90) * Mathf.Deg2Rad))).normalized;
                      RaycastHit2D rh2 = Physics2D.Raycast(line.transform.position, rot, range, 6294272, transform.position.z - 1, transform.position.z + 1);
                        Vector3[] points = new Vector3[11];
                        
                            points[0] = line.transform.position + Vector3.forward;
                        float d2 = range;
                        if (rh2.collider)
                        {
                            d2 = rh2.distance;
                            points[10] = (Vector3)rh2.point + Vector3.forward;
                        }
                        else
                        {
                            points[10] = points[0] + range * ((Vector3)rot);
                        }
                            for (float i = 1f; i <= 9f; i++)
                            {
                                points[(int)i] = Vector3.Lerp(points[0], points[10], i / 10f) + new Vector3(Mathf.Clamp(250f * (Fakerand.Single() - 0.5f) / d2,-30f, 30f), Mathf.Clamp(250f * (Fakerand.Single() - 0.5f) / d2,-30f, 30f), 0);
                            }
                        
                        line.SetPositions(points);
                        if (rh2.collider)
                        {
                            if (rh2.collider.GetComponentInParent<KHealth>())
                            {
                                rh2.collider.transform.parent.GetComponent<KHealth>().ChangeHealth(-500f*gunDamage/dist,"laser gun");
                            }
                        }
                    }

                    while (ma <= 0f)
                    {
                        ma += 360f;
                    }
                    float ta = Mathf.Atan2(transform.position.y - target.transform.position.y, transform.position.x - target.transform.position.x) * Mathf.Rad2Deg - 90;
                    while (ta < ma)
                    {
                        ta += 360f;
                    }

                    float dif = ta - ma;
                    float rr = rotationSpeed * Time.timeScale;
                    if (dif > 360f - rotationSpeed || dif < rr)
                    {
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, ta);
                    }
                    else
                    {
                        transform.eulerAngles += new Vector3(0, 0, rr - 2 * rr * Mathf.Max(0, Mathf.Sign(dif - 180)));
                    }
                }
            }
            else
            {
                line.enabled = false;
            }

        }
        ///////////////////////////////////////////////////////// end of MachineGun



    }
}
