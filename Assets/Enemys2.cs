using UnityEngine;
using System.Collections;

public class Enemys2 : GenericBlowMeUp { //physics type enemies

    public enum Type
    {
        Superball,
    }

    public Type type;
    public Transform target;
    public bool doTarget;
    public Vector3 myPositionOffset;
    public Vector2 velocity;
    public Vector2 maxSpeedXY;
    public Vector2 desiredSpeedXY;
    public bool onlyActiveWhenVisible;
    public AudioClip sfx1;
    public AudioClip sfx2;
    public AudioClip sfx3;
    public int i1;
    [HideInInspector]
    public int i1orig;
    public int i2;
    [HideInInspector]
    public int i2orig;
    public float damage;
    public PrimEnemyHealth enemyHealth;

    void Start () {
        GetComponent<Rigidbody2D>().velocity = velocity;
        i1orig = i1; i2orig = i2;
	}

    void OnCollisionEnter2D(Collision2D c)
    {
        AudioSource aso = GetComponent<AudioSource>();
        if (type == Type.Superball)
        {
            aso.clip = sfx1;
            aso.volume = c.relativeVelocity.magnitude * 0.001f;
            aso.pitch = Fakerand.Single(0.95f, 1.05f);
            aso.Play();
            bool hurtPlayer = false;
            bool destr = false;
            Rigidbody2D rg2 = GetComponent<Rigidbody2D>();

            if (c.contacts[0].normal.y > 0f && c.contacts[0].normal.y >= System.Math.Abs(c.contacts[0].normal.x)) //ground
            {
                Vector2 dist;
                if (target != null)
                {
                    dist = (target.position - transform.position);
                }
                else
                {
                    dist = desiredSpeedXY;
                }
                float c1 = 1f;
                float c2 = 1f;
                i1--;
                i2--;
                if (i1 == 0)
                {
                    i1 = i1orig;
                    c1 = -1f;
                }
                if (i2 == 0)
                {
                    i2 = i2orig;
                    c2 = 1.5f;
                }
                rg2.velocity = new Vector2(c1 * System.Math.Abs(desiredSpeedXY.x) * Mathf.Sign(dist.x), c2 * desiredSpeedXY.y);
                hurtPlayer = true;
            }
            else if (System.Math.Abs(c.contacts[0].normal.y) < System.Math.Abs(c.contacts[0].normal.x)) //wall
            {
                
                rg2.velocity = new Vector2(Mathf.Sign(rg2.velocity.x)*desiredSpeedXY.x,rg2.velocity.y);
                hurtPlayer = true;
            }
            else //ceiling
            {
                rg2.velocity = new Vector2(rg2.velocity.x, -desiredSpeedXY.y);
            }

            //part 2

            if (hurtPlayer && c.collider.GetComponent<KHealth>() != null)
            {
                aso.clip = sfx2;
                aso.volume = 0.75f;
                aso.pitch = 1f;
                aso.Play();
                if (damage != 0)
                {
                    c.collider.GetComponent<KHealth>().ChangeHealth(-damage, "superball");
                }
                c.collider.GetComponent<BasicMove>().fakePhysicsVel = -1f*rg2.velocity;
                //c.collider.GetComponent<Rigidbody2D>().velocity += -3f*rg2.velocity; //the last one is from being in physics
            }
            else if (c.collider.GetComponent<KHealth>() != null && c.collider.GetComponent<Encontrolmentation>() != null)
            {
                BasicMove btemp = c.collider.GetComponent<BasicMove>();
                btemp.fakePhysicsVel = new Vector2(btemp.fakePhysicsVel.x, desiredSpeedXY.y);
                if (enemyHealth)
                {
                    enemyHealth.DoDamage(1f, c.collider.transform.position);
                }
                /*destr = true;
                aso.clip = sfx3;
                aso.volume = 0.75f;
                aso.pitch = 1f;
                aso.Play();
                Destroy(rg2);
                Destroy(GetComponent<Collider2D>());
                Destroy(GetComponent<SpriteRenderer>());
                Destroy(gameObject, 1f); //ADD PARTICLES*/

            }

            if (c.collider.GetComponent<Rigidbody2D>() != null && !destr)
            {
                Rigidbody2D rg2c = c.collider.GetComponent<Rigidbody2D>();
                if (!rg2c.isKinematic)
                {
                    rg2c.velocity += -rg2.velocity;
                }
            }

        }
    }

    void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        Vector3 lpos = transform.InverseTransformPoint(ohi.pos);
        GetComponent<Rigidbody2D>().velocity -= 200f * (Vector2)lpos.normalized;
    }
	
	void Update () {
        bool vis = (GetComponent<Renderer>() != null)?GetComponent<Renderer>().isVisible:false;
        if (Time.timeScale > 0 && (!onlyActiveWhenVisible || vis))
        {
            if (doTarget)
            {
                float dist = Mathf.Infinity;
                target = null;
                if (vis)
                {
                    foreach (GameObject man in LevelInfoContainer.allBoxPhysicsObjects)
                    {
                        if (man != null && man.GetComponent<Encontrolmentation>())
                        {
                            float ndist = Fastmath.FastV2Dist(man.transform.position, transform.position + myPositionOffset);
                            if (ndist < dist)
                            {
                                dist = ndist;
                                target = man.transform;
                            }
                        }
                    }
                }
            }

            Rigidbody2D rg2 = GetComponent<Rigidbody2D>();
            if (maxSpeedXY.x >= 0 && System.Math.Abs(rg2.velocity.x) > maxSpeedXY.x)
            {
                rg2.velocity = new Vector2(maxSpeedXY.x*Mathf.Sign(rg2.velocity.x), rg2.velocity.y);
            }
            if (maxSpeedXY.y >= 0 && System.Math.Abs(rg2.velocity.y) > maxSpeedXY.y)
            {
                rg2.velocity = new Vector2(rg2.velocity.x ,maxSpeedXY.y * Mathf.Sign(rg2.velocity.y));
            }


        }
    }
}
