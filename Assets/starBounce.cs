using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class starBounce : GenericBlowMeUp
{
    public Vector2 initVel;
    //public GameObject bullet;
    //public float bulletSpeed;
    public BulletHellMakerFunctions shot;
    public Transform eye;

    private float movSpeed;
    private bool collidedThisFrame;

    public bool lockHorizontal = false;

    private Rigidbody2D r2;

    private static Vector2[] allDirections = new Vector2[8] {
        Vector2.right,
        new Vector2(0.70710678f,0.70710678f),
        Vector2.up,
        new Vector2(-0.70710678f,0.70710678f),
        Vector2.left,
        new Vector2(-0.70710678f,-0.70710678f),
        Vector2.down,
        new Vector2(0.70710678f,-0.70710678f)
    };

    private const float bulletSpawnDist = 16f;

    Transform getClosestPlayerToEyes(out Vector2 dist)
    {
        Transform o = null;
        float d = 1000000f;
        Vector2 t1 = Vector2.zero;
        foreach (GameObject t in LevelInfoContainer.allPlayersInLevel)
        {
            t1 = (t.transform.position - transform.position);
            float ez = t1.sqrMagnitude;
            if (ez < d * d)
            {
                o = t.transform;
                d = Fastmath.FastSqrt(ez);
            }
        }
        dist = t1;
        return o;
    }

    void CollisionJunk(Collision2D col)
    {
        if (!collidedThisFrame)
        {
            //Vector2 norm = col.GetContact(0).normal;
            //if (Vector2.Dot(GetComponent<Rigidbody2D>().velocity.normalized, norm) > -0.9f) // no parallel
            shot.Fire();
            collidedThisFrame = true;
            /*Vector2 oldVel = GetComponent<Rigidbody2D>().velocity;
            Vector2 finalVel = (oldVel - 2f * Vector2.Dot(oldVel, norm) * norm);
            GetComponent<Rigidbody2D>().velocity = new Vector2(finalVel.x*(lockHorizontal?0f:1f), finalVel.y).normalized * movSpeed;
            collidedThisFrame = true;*/
        }
    }

    /*void OnCollisionStay2D(Collision2D col)
    {
        CollisionJunk(col);
    }*/

    void OnCollisionEnter2D(Collision2D col)
    {
        CollisionJunk(col);
    }

    void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        r2.velocity = initVel;
        movSpeed = initVel.magnitude;
        collidedThisFrame = false;
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        Vector3 lpos = transform.InverseTransformPoint(ohi.pos);
        if (lpos.magnitude > 0.1f)
        {
            r2.velocity = -lpos.normalized * movSpeed;
        }
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) { r2.velocity = Vector2.zero; return; }
        else if (r2.velocity.sqrMagnitude < 0.1f) { r2.velocity = initVel; }

        if (r2.velocity.magnitude < movSpeed) { r2.velocity = r2.velocity.normalized * movSpeed; }

        collidedThisFrame = false;
        Vector2 closestDir;
        Transform closest = getClosestPlayerToEyes(out closestDir);
        Vector2 closestN = closestDir.normalized;
        
        eye.localPosition = new Vector3(eye.localPosition.x, 2.5f * closestN.y, -2.5f * closestN.x);

        transform.rotation = Quaternion.identity;
    }
}
