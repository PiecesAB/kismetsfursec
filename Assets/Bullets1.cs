using UnityEngine;
using System.Collections;

public class Bullets1 : MonoBehaviour {

    public Vector2 initVel;
    public bool staysWhenHitsGround;
    public GameObject effectOnTurnInvisible;
    public bool followsMovementDirection;
    public float bounceWhenHitsGround;
    public int waitFramesBeforeFade;
    public float damage;
    public string damageReason;
    public bool hitSomething;
    public AudioSource hitGroundSound;
    public AudioSource hitPlayerSound;
    public float tranquilizerMult = 1f;
    [Header("1: speed, 2: jumpheight, 4: punchpower")]
    public int tranquilizeMask = 7;

    private bool damaged = false;

    private float origRot;

	void Start () {
        LevelInfoContainer.allBoxPhysicsObjects.Add(gameObject);
        GetComponent<Rigidbody2D>().velocity = initVel;
        hitSomething = false; //just in case lol
	}

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.GetComponent<Bullets1>() == null && !c.isTrigger && (c.gameObject.layer == 8 || c.gameObject.layer == 9 || c.gameObject.layer == 11 || c.gameObject.layer == 20))
        {
            if (staysWhenHitsGround)
            {
                Destroy(GetComponent<Rigidbody2D>());
                Destroy(GetComponent<Collider2D>());
                followsMovementDirection = false;
                hitSomething = true;
                origRot = transform.eulerAngles.z;
                LevelInfoContainer.allBoxPhysicsObjects.Remove(gameObject);
                transform.SetParent(c.transform);

                if (c.gameObject.layer == 20)
                {
                    transform.SetParent(c.transform,true);
                    waitFramesBeforeFade = -1;
                    if (c.GetComponent<KHealth>() != null && !damaged)
                    {
                        damaged = true;
                        KHealth k = c.GetComponent<KHealth>();
                        k.ChangeHealth(-damage,damageReason);
                        BasicMove bm = k.GetComponent<BasicMove>();
                        bm.AddBlood(transform.position, Quaternion.Euler(0f,0f,transform.eulerAngles.z+90f));
                        if (tranquilizerMult != 1f)
                        {
                            float rms = -bm.moveSpeed * tranquilizerMult;
                            float rmj = -bm.jumpHeight * tranquilizerMult * tranquilizerMult;
                            float rmp = -bm.punchPowerMultiplier * tranquilizerMult;
                            TranquilizerSpeedMod tsm = k.GetComponent<TranquilizerSpeedMod>();
                            if (!tsm) { tsm = k.gameObject.AddComponent<TranquilizerSpeedMod>(); }
                            if ((tranquilizeMask & 1) == 1)
                            {
                                tsm.speedChange += rms;
                                bm.moveSpeed += rms;
                            }
                            if ((tranquilizeMask & 2) == 2)
                            {
                                if (tranquilizerMult > 0.99)
                                {
                                    bm.youCanJump = false;
                                }
                                else
                                {
                                    tsm.jumpHeightChange += rmj;
                                    bm.jumpHeight += rmj;
                                }
                            }
                            if ((tranquilizeMask & 4) == 4)
                            {
                                tsm.punchPowerMultChange += rmp;
                                bm.punchPowerMultiplier += rmp;
                            }
                        }
                    }
                    if (hitPlayerSound != null)
                    {
                        hitPlayerSound.Play();
                    }
                }
                else
                {
                    if (hitGroundSound != null)
                    {
                        hitGroundSound.Play();
                    }
                }
            }
        }
    }
	
	void Update () {
        if (Time.timeScale > 0)
        {
            if (followsMovementDirection)
            {
                Rigidbody2D r2 = GetComponent<Rigidbody2D>();
                transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(r2.velocity.y, r2.velocity.x) * Mathf.Rad2Deg);
            }
            if (hitSomething)
            {
                if (System.Math.Abs(bounceWhenHitsGround) > 0.1f)
                {
                    bounceWhenHitsGround *= -0.9f;
                    transform.eulerAngles = new Vector3(0f, 0f, origRot + bounceWhenHitsGround);
                }
                else
                {
                    bounceWhenHitsGround = 0f;
                    transform.eulerAngles = new Vector3(0f, 0f, origRot);
                    if (waitFramesBeforeFade > 0)
                    {
                        waitFramesBeforeFade--;
                    }
                    else if (waitFramesBeforeFade == 0)
                    {
                        Color cc = GetComponent<SpriteRenderer>().color;
                        if (cc.a == 0f)
                        {
                            Destroy(gameObject);
                        }
                        GetComponent<SpriteRenderer>().color = new Color(cc.r, cc.g, cc.b, Mathf.Max(cc.a - 0.00390625f, 0f));
                    }
                }
            }
        }
	}
}
