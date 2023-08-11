using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimBreakable : MonoBehaviour
{

    public enum HitBehavior
    {
        Shatter, Wobble
    };

    public int hitsToBreak;
    public float crackVelocity;
    public Sprite[] hitSprites;
    public GameObject shardOnDestroy;
    public AudioClip crackSound;
    public AudioClip breakSound;
    public bool plrCanDestroy = true;
    public float plrDamageMult;
    public string damageReason;
    public AmbushController ambushOnBreak;
    public Renderer mustBeOnScreenToBreak = null;
    public GameObject conditionalNotMetDialog;
    public bool breakWhenTouchingKinematicFullContacts = true;
    public GameObject hurtEffect;
    // if this exists, the block will burn if a burning player touches it
    public BurningPart burningComponent;
    public float shardZOffset = 0;

    public static AudioSource singleBreakAudio = null;

    private primExtraTags tags;
    private SpriteRenderer sr;

    private Dictionary<GameObject, int> debounce = new Dictionary<GameObject, int>();

    public void BreakIt(int amt, float particleRot)
    {
        BreakIt(amt, particleRot, Vector2.positiveInfinity);
    }

    public void BreakIt(int amt, float particleRot, Vector2 hitPos)
    {
        if (mustBeOnScreenToBreak != null && !mustBeOnScreenToBreak.isVisible) { return; }

        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        AudioSource asr = GetComponent<AudioSource>();
        hitsToBreak -= amt; //round down

        if (amt != 0)
        {
            if (conditionalNotMetDialog != null)
            {
                InGameConditional[] conditions = GetComponents<InGameConditional>();
                for (int i = 0; i < conditions.Length; ++i)
                {
                    if (!conditions[i].Evaluate())
                    {
                        MainTextsStuff.insertableStringValue1 = "Not eligible: " + conditions[i].GetInfo();
                        TextBoxGiverHandler.SpawnNewBox(ref conditionalNotMetDialog, null);
                        return;
                    }
                }
            }

            if (hitsToBreak >= 0)
            {
                hitsToBreak = Mathf.Min(hitsToBreak, hitSprites.Length - 1);
                if (sr)
                {
                    sr.sprite = hitSprites[hitsToBreak];
                }
                if (asr && crackSound)
                {
                    asr.Stop();
                    asr.clip = crackSound;
                    if (singleBreakAudio != asr)
                    {
                        if (singleBreakAudio != null) { singleBreakAudio.Stop(); }
                        singleBreakAudio = asr;
                    }
                    asr.Play();
                }
                if (hurtEffect)
                {
                    Vector2 hp = hitPos.magnitude < 10000000 ? hitPos : (Vector2)transform.position;
                    Instantiate(hurtEffect, hp, Quaternion.identity);
                }
            }
            else
            {
                //shatter
                if (shardOnDestroy)
                {
                    GameObject shard = Instantiate(
                        shardOnDestroy, 
                        transform.position + Vector3.forward * shardZOffset, 
                        Quaternion.AngleAxis(particleRot, Vector3.forward)
                    );
                    shard.transform.SetParent(transform.parent, true);
                }
                Destroy(gameObject, 1f);
                if (sr)
                {
                    sr.enabled = false;
                }
                for (int i = 0; i < mrs.Length; i++)
                {
                    mrs[i].enabled = false;
                }
                foreach (Collider2D c in GetComponents<Collider2D>())
                {
                    c.enabled = false;
                }
                if (asr && breakSound)
                {
                    asr.Stop();
                    asr.clip = breakSound;
                    if (singleBreakAudio != asr)
                    {
                        if (singleBreakAudio != null) { singleBreakAudio.Stop(); }
                        singleBreakAudio = asr;
                    }
                    asr.Play();
                }
                if (ambushOnBreak)
                {
                    ambushOnBreak.Activate();
                }
            }
        }
    }

    bool DetectPunch(Collider2D c)
    {
        Component cd = (Component)c.transform.parent ?? c;

        BasicMove bm = cd.GetComponent<BasicMove>();

        if (c.gameObject.layer == 19)
        {
            float mul = 1f;
            if (c.transform.parent)
            {
                if (bm)
                {
                    mul = bm.punchPowerMultiplier;
                }
            }
            if (plrCanDestroy)
            {
                Vector2 space = transform.position - c.transform.position;
                if (!debounce.ContainsKey(c.gameObject))
                {
                    BreakIt(Mathf.FloorToInt(300f * mul / crackVelocity), Mathf.Rad2Deg * Mathf.Atan2(space.y, space.x));
                }
                return true;
            }
        }
        
        KHealth kh = cd.GetComponent<KHealth>();
        if (burningComponent && kh && kh.overheat > 0f)
        {
            if (tags)
            {
                if (tags.tags.Contains("flammable"))
                {
                    tags.tags.Remove("flammable");
                    BurningPart newFire = gameObject.AddComponent<BurningPart>();
                    newFire.StartCoroutine(newFire.CatchFire(burningComponent));
                }
                if (tags.tags.Contains("superflammable"))
                {
                    tags.tags.Remove("superflammable");
                    BurningPart newFire = gameObject.AddComponent<BurningPart>();
                    newFire.StartCoroutine(newFire.CatchFire(burningComponent, true));
                }
            }
        }
        if (plrDamageMult > 0f && kh && bm && bm.momentumMode == false)
        {
            kh.ChangeHealth(-bm.Damage * plrDamageMult, damageReason);
            return true;
        }

        return false;
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (!debounce.ContainsKey(c.gameObject))
        {
            DetectPunch(c);
            debounce.Add(c.gameObject, c.gameObject.layer == 19 ? 16 : 8);
        }
    }

    private void BreakOnCertainCollisions(Collision2D col)
    {
        if (col.gameObject.GetComponent<PrimBreakable>() || col.gameObject.name == "SubMeld" || col.gameObject.GetComponent<beamBlock>())
        {
            Physics2D.IgnoreCollision(col.collider, col.otherCollider);
            return;
        }

        if (!debounce.ContainsKey(col.gameObject))
        {
            if (!DetectPunch(col.collider))
            {
                Vector2 norm = col.GetContact(0).normal;
                if (breakWhenTouchingKinematicFullContacts && col.rigidbody != null && col.rigidbody.isKinematic
                    && col.rigidbody.useFullKinematicContacts && !col.rigidbody.GetComponent<PrimBreakable>())
                {
                    BreakIt(99, Mathf.Rad2Deg * Mathf.Atan2(norm.y, norm.x), col.GetContact(0).point);
                }
                else if (col.rigidbody != null && (col.rigidbody.GetComponent<primDecorationMoving>() || col.rigidbody.GetComponent<PlatformControlButtonMain>()))
                {
                    BreakIt(99, Mathf.Rad2Deg * Mathf.Atan2(norm.y, norm.x));
                }
                else
                {
                    Vector2 rvel = col.relativeVelocity;
                    //float vforce = Vector2.Dot(rvel.normalized, norm);
                    float vforce = Mathf.Max(Mathf.Abs(rvel.x), Mathf.Abs(rvel.y));
                    if (Vector2.Dot(rvel.normalized, norm) < 0.2f) { vforce = 0f; }
                    int amt = (int)(vforce / crackVelocity);
                    BreakIt(amt, Mathf.Rad2Deg * Mathf.Atan2(norm.y, norm.x));
                }
            }
            debounce.Add(col.gameObject, col.gameObject.layer == 19 ? 16 : 8);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        BreakOnCertainCollisions(col);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        // if player is inside block, destroy the block
        if (col.collider.gameObject.layer == 20)
        {
            Vector3 ppt = col.collider.transform.position;
            if (col.otherCollider.bounds.Contains(ppt))
            {
                Vector3 d = ppt - transform.position;
                BreakIt(99, Mathf.Rad2Deg * Mathf.Atan2(d.y, d.x));
                return;
            }
        }

        BreakOnCertainCollisions(col);
    }

    void Start()
    {
        tags = GetComponent<primExtraTags>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Do1Damage(float dir)
    {
        if (hitsToBreak >= 0) { BreakIt(1, dir); }
    }

    void Update()
    {
        if (debounce.Count > 0) {
            foreach (GameObject k in new List<GameObject>(debounce.Keys))
            {
                --debounce[k];
                if (debounce[k] <= 0) { debounce.Remove(k); }
            }
        }
        if (!sr.isVisible) { return; }

        foreach (ShieldingCircle s in ShieldingCircle.all) {
            Vector2 closestPoint = Vector2.MoveTowards(transform.InverseTransformPoint(s.transform.position), Vector2.zero,s.radius);
            if (Mathf.Max(Mathf.Abs(closestPoint.x), Mathf.Abs(closestPoint.y)) < 8f && crackVelocity < 500) //inside the square
            {
                //Do1Damage((closestPoint.sqrMagnitude > 0) ? (Mathf.Atan2(-closestPoint.y, -closestPoint.x) * Mathf.Rad2Deg) : 0f);
                Do1Damage(Mathf.Atan2(s.fakeVelocity.y, s.fakeVelocity.x) * Mathf.Rad2Deg);
            }
        }

        foreach (LaserBullet l in LaserBullet.tetraLasers)
        {
            if (l.TetraLaserBlockCollision(transform.position, 11.4f))
            {
                Do1Damage(l.transform.eulerAngles.z);
            }
        }
    }
}
