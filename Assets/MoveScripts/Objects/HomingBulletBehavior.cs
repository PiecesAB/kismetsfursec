using UnityEngine;
using System.Collections;

public class HomingBulletBehavior : GenericBlowMeUp {


    public float speed;
    public float accel;
    [Header("Misnomer!!! This should be \"Max speed\"")]
    public float maxAccel;
    public float damageMultiplier;
    [Range(0f,1f)]
    public float rotateSpeed;
    public bool surpriseMe = false;
    public ParticleSystem particlesForSurprise;
    [Header("Icicle mode: doesn't delete bullet as it hits player.")]
    public bool icicleMode;
    public float icicleDisappearSpeed = 1f;

    private Transform playerPos;

    private bool damagedAThing = false;
    private bool vis = false;
    private float inLiquid = 1f;
    private long framesSinceSubmerge = 0;
    private long framesSinceVisible = 0;

    private bool surprising;
    private int surpriseBuildup = 0;
    private int timeForASurprise = 0;

    private Rigidbody2D r2;
    private SpriteRenderer spr;
    private Renderer rd;

    private int framesWhileNew = 5;
    private bool icicleHit;

    void SetTargetPlayer()
    {
        playerPos = null;
        if (LevelInfoContainer.allPlayersInLevel.Count == 1)
        {
            playerPos = LevelInfoContainer.allPlayersInLevel[0].transform;
        }
        else
        {
            float shortest = 100000000f;
            for (int i = 0; i < LevelInfoContainer.allPlayersInLevel.Count; i++)
            {
                Transform t = LevelInfoContainer.allPlayersInLevel[i].transform;
                float d = (t.position - transform.position).sqrMagnitude;
                if (d < shortest)
                {
                    shortest = d;
                    playerPos = t;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!damagedAThing)
        {
            
            if (other.gameObject.GetComponent<KHealth>() && other.gameObject.GetComponent<BasicMove>())
            {
                damagedAThing = true;
                other.gameObject.GetComponent<KHealth>().ChangeHealth(-other.gameObject.GetComponent<BasicMove>().Damage * damageMultiplier, "homing bullet");

                other.gameObject.GetComponent<BasicMove>().AddBlood(other.gameObject.transform.position, Quaternion.LookRotation(-100 * transform.right, Vector3.up));
                other.gameObject.GetComponent<BasicMove>().fakePhysicsVel += 150f * ((Vector2)transform.right);
                other.gameObject.GetComponent<AudioSource>().PlayOneShot(other.gameObject.GetComponent<BasicMove>().spikeTouchSound);
                BlowMeUp();
            }

            if (!surpriseMe && (other.gameObject.layer == 8 || other.gameObject.layer == 9))
            {
                damagedAThing = true;
                BlowMeUp();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        BoostArrow ba = col.GetComponent<BoostArrow>();
        SuperRay sr = col.GetComponent<SuperRay>();
        if (ba)
        {
            inLiquid = 0.35f - 0.35f * ba.swimFriction;
            framesSinceSubmerge = 0;
            transform.position += Vector3.down * ba.swimDownwardPull * 0.03333333f;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        //if (!icicleMode) { return; }
        // only an icicle would have this collider anyway
        if (framesWhileNew > 0) { return; }
        //if (col.gameObject.GetComponent<BasicMove>()) { return; }

        if (Vector2.Dot(col.GetContact(0).normal, -transform.right) > 0.7f)
        {
            speed = accel = maxAccel = 0;
            r2.velocity = Vector2.zero;
            transform.position = new Vector3(Mathf.Round(transform.position.x / 4f) * 4f, Mathf.Round(transform.position.y / 4f) * 4f, transform.position.z);
            icicleHit = true;
        }
    }

    void Start()
    {
        //playerPos = GameObject.FindGameObjectWithTag("Player").transform;
        damagedAThing = vis = false;
        inLiquid = 1f;
        framesSinceSubmerge = 0;
        surpriseBuildup = 0;
        framesWhileNew = 5;
        icicleHit = false;
        timeForASurprise = Fakerand.Int(20, 51);
        r2 = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        rd = GetComponentInChildren<Renderer>();
    }

    IEnumerator SurpriseDirective(Vector2 dir, float dist)
    {
        particlesForSurprise.Pause();
        Vector2 targ = dir * dist;
        float getReady = 0.15f;
        while (getReady > 0f)
        {
            if (Time.timeScale > 0)
            {
                Vector2 offset = transform.position - playerPos.position;
                float angle = Fastmath.FastAtan2(offset.y, offset.x);
                float targAngle = Fastmath.FastAtan2(targ.y, targ.x);
                angle = Mathf.MoveTowardsAngle(angle * Mathf.Rad2Deg, targAngle * Mathf.Rad2Deg, Mathf.Min(12f, 12f * Time.timeScale)) * Mathf.Deg2Rad;
                transform.position = new Vector3(playerPos.position.x + dist * Mathf.Cos(angle), playerPos.position.y + dist * Mathf.Sin(angle), transform.position.z);
                transform.eulerAngles = Vector3.forward * (angle * Mathf.Rad2Deg + 180f);

                if (Mathf.Approximately(angle,targAngle))
                {
                    getReady -= Time.deltaTime;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        surprising = false;
        particlesForSurprise.Play();
    }

    void Update()
    {
        if (icicleHit) {
            if (rd == null) { Destroy(gameObject); }
            if (rd is SpriteRenderer)
            { 
                // not needed?
            }
            else
            {
                Color c1 = rd.material.GetColor("_Color");
                c1 = new Color(c1.r, c1.g, c1.b, c1.a - (1f / 100f) * icicleDisappearSpeed * Time.timeScale);
                rd.material.SetColor("_Color", c1);
                if (c1.a <= -2f) { Destroy(gameObject); }
            }
            return;
        }

        --framesWhileNew;
        if (framesWhileNew <= 0) { framesWhileNew = 0; }

        if (rd == null || rd.isVisible)
        {
            if (Time.timeScale > 0)
            {
                SetTargetPlayer();
                if (playerPos && !surprising)
                {
                    if (!vis)
                    {
                        vis = true;
                    }
                    /*if (speed < maxAccel)*/
                    float z = Mathf.Rad2Deg * Fastmath.FastAtan2(playerPos.position.y - transform.position.y, playerPos.position.x - transform.position.x);
                    float oz = transform.eulerAngles.z;
                    //transform.LookAt(playerPos);
                    transform.eulerAngles = Vector3.forward * Mathf.LerpAngle(oz, z, rotateSpeed * inLiquid);
                    /*float nz = z - oz;
                    if (nz < 0f)
                    {
                        nz += 360f;
                    }
                    if (nz >= rotateSpeed)
                    {
                        Mathf.L
                    }
                    else
                    {
                        transform.eulerAngles = Vector3.forward * z;
                    }*/
                    transform.Translate(Vector3.right * speed * Time.deltaTime * inLiquid);
                    //kinematic??
                    if (r2 != null) { r2.velocity = transform.right * speed * inLiquid; }

                    if (spr != null) { spr.flipY = (transform.eulerAngles.z > 135 && transform.eulerAngles.z <= 315); }

                    if (speed < maxAccel) { speed += accel*Time.timeScale; }
                    if (speed > maxAccel) { speed = maxAccel; }

                    inLiquid += 0.05f;
                    inLiquid = Mathf.Clamp01(inLiquid);
                    framesSinceSubmerge++;
                    framesSinceVisible++;

                    if (surpriseMe && !surprising)
                    {
                        Rigidbody2D r2a = playerPos.GetComponent<Rigidbody2D>();
                        if (Vector2.Dot(r2a.velocity,transform.right) > 0.3f)
                        {
                            surpriseBuildup++;
                            if (surpriseBuildup > timeForASurprise)
                            {
                                surpriseBuildup = 0;
                                timeForASurprise = Fakerand.Int(20, 51);
                                surprising = true;
                                StartCoroutine(SurpriseDirective(r2a.velocity.normalized, Vector2.Distance(playerPos.position,transform.position)));
                            }
                        }
                    }
                }

                
            }
        }
        else
        {
            if (Time.timeScale > 0 && vis && framesSinceVisible > 9 && !surprising)
            {
                if (icicleMode) { return; }
                BlowMeUp();
            }
        }
    }
}
