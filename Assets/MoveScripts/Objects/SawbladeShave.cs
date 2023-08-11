using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SawbladeShave : MonoBehaviour {

    public Mesh noBlood;
    public Mesh slightBlood;
    public Mesh moderateBlood;
    public Mesh heavyBlood;
    public string spikeDeathName = "sawblade";
    public bool knockback = false;
    public float knockbackAmt = 100;
    public int cooldown = 0;
    public bool scalingDamage = false; // damage amount that adapts to the level theme
    public float damageMultiplier;
    public bool punchToBreak;
    public SkinnedMeshRenderer shapeKeyToAnimateIfExists;
    public MeshFilter meshedObject;
    public AudioClip sound;
    public bool isTrueSpike = false; // sawblade or spike or spinning wire

    public float burnPlayer = 0f;

    public static int idcount = -1;
    public int myID = -1;
    private List<SawbladeShave> objs = new List<SawbladeShave>();
    private static float anim1;
    private const float anim1Speed = 4f;
    private static List<int> thisframehits = new List<int>();

    public int hits;

    public int destroyIfTooManyHits = -1;

    private static SawbladeShave main;

    public void BlowMeUp()
    {
        //makes sure player isnt destroyed
        foreach (Transform c in transform)
        {
            if (c.GetComponent<BasicMove>())
            {
                c.GetComponent<BasicMove>().Unparent();
            }
        }
        //Destroy(gameObject);
        GenericBlowMeUp e = gameObject.AddComponent<GenericBlowMeUp>();
        e.BlowMeUp();
    }

    public void ChangeMesh()
    {
        if (meshedObject)
        {
            if (noBlood != null && hits <= 0)
            {
                meshedObject.mesh = noBlood;
            }
            else if (slightBlood != null && hits <= 2)
            {
                meshedObject.mesh = slightBlood;
            }
            else if (moderateBlood != null && hits <= 4)
            {
                meshedObject.mesh = moderateBlood;
            }
            else if (heavyBlood != null)
            {
                meshedObject.mesh = heavyBlood;
            }
        }
    }

    public void HitsTooManyCheck()
    {
        if (destroyIfTooManyHits > 0 && hits >= destroyIfTooManyHits)
        {
            BlowMeUp();
        }
    }

    // Use this for initialization
    void Start () {
        cooldown = 0;
        idcount++;
        myID = idcount;
        objs.Add(this);
        ChangeMesh();
        main = this;
       /* foreach (Collider2D coll in GameObject.Find("The Player, I Think").GetComponents<Collider2D>())
        {
            if (GetComponent<BoxCollider2D>())
            {
                Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), coll);
            }

        }*/
       
	}

    private void OnDestroy()
    {
        idcount--;
        for (int i = 0; i < objs.Count; i++)
        {
            if (objs[i].myID > myID)
            {
                print(objs[i].myID);
                objs[i].myID--;
            }
        }
        objs.Remove(this);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.rigidbody && other.rigidbody.isKinematic)
        {
            return;
        }
        
        if (other.collider.gameObject.layer == 19) //punch
        {
            if (punchToBreak)
            {
                if (thisframehits.Contains(other.gameObject.GetInstanceID())) { return; }
                thisframehits.Add(other.gameObject.GetInstanceID());
                BlowMeUp();
            }
            return;
            //add effects
        }
        else if (damageMultiplier > 0 && other.rigidbody && !other.rigidbody.isKinematic && cooldown == 0 && other.collider.gameObject.GetComponent<KHealth>() && other.collider.gameObject.GetComponent<BasicMove>())
        {
            BasicMove bm = other.gameObject.GetComponent<BasicMove>();
            if (bm.CanCollide)
            {
                KHealth kh = other.gameObject.GetComponent<KHealth>();
                if (isTrueSpike) { kh.lastSpikeTouched = this; }

                if (thisframehits.Contains(other.gameObject.GetInstanceID())) { return; }
                thisframehits.Add(other.gameObject.GetInstanceID());

                beamBlock beam = GetComponent<beamBlock>();
                float mvtDirCheck = -1;
                if (beam) { mvtDirCheck = Vector2.Dot(transform.up, bm.GetComponent<Rigidbody2D>().velocity); }
                print("!");
                print(kh.justFiredBulletInvincibility);
                if (mvtDirCheck < 1e-3 && kh.justFiredBulletInvincibility == 0)
                {
                    float dmamt = bm.Damage * damageMultiplier;
                    if (scalingDamage) { dmamt = LevelInfoContainer.GetScalingSpikeDamage(); }
                    kh.ChangeHealth(-dmamt, spikeDeathName);
                    if (burnPlayer != 0f)
                    {
                        kh.overheat += burnPlayer;
                    }
                    hits = Mathf.Min(100, hits + 1);
                    HitsTooManyCheck();
                    ChangeMesh();

                    //other.gameObject.GetComponent<Rigidbody2D>().AddForce(-other.gameObject.GetComponent<BasicMove>().fakePhysicsVel);
                    AudioClip sound2 = (sound != null) ? sound : (other.gameObject.GetComponent<BasicMove>().spikeTouchSound);
                    other.gameObject.GetComponent<AudioSource>().PlayOneShot(sound2);
                    cooldown = 6;
                }
            }
        }

        if (knockback && other.gameObject.GetComponent<BasicMove>())
        {
            BasicMove bm = other.gameObject.GetComponent<BasicMove>();
            if (bm.CanCollide && cooldown == 6)
            {
                bm.fakePhysicsVel -= other.GetContact(0).normal * knockbackAmt;
            }
        }

        thisframehits.Add(other.gameObject.GetInstanceID());
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!thisframehits.Contains(other.gameObject.GetInstanceID()))
        {
            if (other.collider.gameObject.layer == 19) //punch
            {
                if (punchToBreak)
                {
                    BlowMeUp();
                }
                return;
                //add effects
            }
            else if (damageMultiplier > 0 && other.rigidbody && !other.rigidbody.isKinematic && cooldown == 0 && other.gameObject.GetComponent<KHealth>() && other.gameObject.GetComponent<BasicMove>())
            {
                BasicMove bm = other.gameObject.GetComponent<BasicMove>();
                if (bm.CanCollide)
                {
                    KHealth kh = other.gameObject.GetComponent<KHealth>();
                    if (isTrueSpike) { kh.lastSpikeTouched = this; }

                    beamBlock beam = GetComponent<beamBlock>();
                    float mvtDirCheck = -1;
                    if (beam) { mvtDirCheck = Vector2.Dot(transform.up, bm.GetComponent<Rigidbody2D>().velocity); }
                    if (mvtDirCheck < 1e-3 && kh.justFiredBulletInvincibility == 0)
                    {
                        float dmamt = bm.Damage * damageMultiplier;
                        if (scalingDamage) { dmamt = LevelInfoContainer.GetScalingSpikeDamage(); }
                        kh.ChangeHealth(-dmamt, spikeDeathName);
                        if (burnPlayer != 0f)
                        {
                            kh.overheat += burnPlayer;
                        }
                        hits = Mathf.Min(100, hits + 1);
                        HitsTooManyCheck();
                        bm.AddBlood(other.GetContact(0).point, Quaternion.LookRotation(Fakerand.UnitCircle(), Vector3.up));
                        AudioClip sound2 = (sound != null) ? sound : (other.gameObject.GetComponent<BasicMove>().spikeTouchSound);
                        other.gameObject.GetComponent<AudioSource>().PlayOneShot(sound2);
                        cooldown = 6;
                    }
                }
            }

            if (knockback && other.gameObject.GetComponent<BasicMove>())
            {
                BasicMove bm = other.gameObject.GetComponent<BasicMove>();
                if (bm.CanCollide)
                {
                    bm.fakePhysicsVel -= other.GetContact(0).normal * knockbackAmt;
                }
            }

            thisframehits.Add(other.gameObject.GetInstanceID());
        }
    }


    // Update is called once per frame
    void Update () {
        if (thisframehits.Count > 0)
        {
            thisframehits.Clear();
        }
	    if (cooldown > 0)
        {
            cooldown--;
        }
        if (shapeKeyToAnimateIfExists)
        {
            shapeKeyToAnimateIfExists.SetBlendShapeWeight(0, Mathf.PingPong(anim1,100f));
        }

        if (myID == 0)
        {
            anim1 += anim1Speed;
            anim1 %= 200f;
        }
	}
}
