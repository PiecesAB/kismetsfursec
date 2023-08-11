using UnityEngine;
using System.Collections;

public class FakeFrictionOnObject : MonoBehaviour {

    public float frictionForce = 3f;
    public float punchBump;
    public int iced = 0;
    public bool dustShrink = false;
    public float dustLeft = 0f;
    public float maxFallSpeed = 200f;
    public float maxRiseSpeed = 300f;
    public ParticleSystem dustParticles;
    public AudioSource hitSound;

    private int punchCooldown = 0;
    public bool reactsToPunch = true;
    private Collider2D punchedCol;
    private BoxCollider2D myBox;
    private float origDustLeft = 0f;
    private Vector3 origScale = Vector3.zero;
    private Rigidbody2D r2;

    private const float dustWearCoeff = 0.00075f;
    private const float dustParticleSpawnCoeff = 32f;
    private const float maxParticleRate = 96f;

    private Vector2 fakeVelocity;
    private Vector2 lastPos;

	// Use this for initialization
	void Start () {
        punchCooldown = iced = 0;
        myBox = GetComponent<BoxCollider2D>();
        dustLeft = origDustLeft = myBox.size.y;
        origScale = transform.localScale;
        r2 = GetComponent<Rigidbody2D>();
        lastPos = transform.position;
    }

    private void OnCollisionStay2D(Collision2D c)
    {
        // this stops the player from pushing the platform that they're standing on (causing a bad feedback loop)
        if (punchCooldown == 0 && c.gameObject.layer == 20)
        {
            BasicMove bm = c.gameObject.GetComponent<BasicMove>();
            Rigidbody2D r2 = GetComponent<Rigidbody2D>();
            if (bm.fakePlatform == r2 && r2)
            {
                r2.velocity = new Vector2(0, r2.velocity.y);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (punchCooldown == 0 && c.collider.gameObject.layer == 19) { // punch
            if (reactsToPunch)
            {
                Vector2 vp = c.gameObject.transform.localPosition - transform.localPosition;
                Vector2 ct = (Vector2.Dot(Vector2.right, vp) >= 0f) ? (new Vector2(-1f, 1f)) : (new Vector2(1f, 1f));
                r2.velocity = punchBump * ct;
                fakeVelocity = punchBump * ct;
                punchedCol = c.collider;
                Physics2D.IgnoreCollision(punchedCol, myBox, true);
                punchCooldown = 19;

                // bounce correction if one raycast succeeds, showing that it's next to a wall right now
                float boxHWidth = myBox.size.x * 0.5f;
                float boxHHeight = myBox.size.y * 0.5f;
                RaycastHit2D left = Physics2D.Raycast(transform.position - (boxHHeight - 4) * transform.up - (boxHWidth + 1) * transform.right, -transform.right, 1f, 256 + 512 + 2048);
                RaycastHit2D right = Physics2D.Raycast(transform.position - (boxHHeight - 4) * transform.up + (boxHWidth + 1) * transform.right, transform.right, 1f, 256 + 512 + 2048);
                if (left.collider ^ right.collider)
                {
                    Vector3 bounceCorrection = new Vector3(3 * Mathf.Sign(vp.x), 0);
                    transform.localPosition += bounceCorrection;
                    BasicMove bm = c.gameObject.GetComponent<BasicMove>();
                    if (bm && Time.timeScale > 0)
                    {
                        Vector2 ev = (Vector2)transform.TransformDirection(bounceCorrection) / Time.deltaTime;
                        GetComponent<Rigidbody2D>().velocity += 1.5f * ev;
                        bm.extraPerFrameVel += 2f * ev;
                    }
                }
            }
            else
            {
                Physics2D.IgnoreCollision(c.collider, c.otherCollider);
            }
        }

        Vector2 norm = c.GetContact(0).normal;

        if (((1 << c.collider.gameObject.layer) & 2816) != 0) {
            if (punchCooldown > 0)
            {
                Vector2 newVel = Vector2.Reflect(fakeVelocity, norm);
                r2.velocity = newVel;
                fakeVelocity = newVel;
                Vector2 dif = lastPos - (Vector2)transform.position;
                lastPos = (Vector2)transform.position - newVel * 0.0166666f * Time.timeScale;
            }
        }

        if (hitSound && !hitSound.isPlaying)
        {
            hitSound.volume = Mathf.Clamp(c.relativeVelocity.magnitude / 300f, 0f, 0.5f);
            hitSound.Play();
        }

        if (c.gameObject.layer == 20)
        {
            if (Vector2.Dot(norm, Vector2.up) > 0.7f && transform.localPosition.y > c.transform.localPosition.y) //bumped from below
            {
                Vector2 vp = c.gameObject.transform.localPosition - transform.localPosition;
                r2.velocity = punchBump * ((Vector2)transform.up);
                fakeVelocity = punchBump * ((Vector2)transform.up);
                if (System.Math.Abs(vp.x) >= 2f * transform.localScale.x)
                {
                    r2.velocity += -((Vector2)transform.right) * punchBump * (vp.x / 12f);
                }
            }
        }

        if (r2.velocity.y + c.relativeVelocity.y < -maxFallSpeed)
        {
            r2.velocity += new Vector2(0, (-maxFallSpeed - r2.velocity.y - c.relativeVelocity.y));
        }
    }

    // Update is called once per frame
    void Update () {
        if (Time.timeScale == 0f) { return; }
        if (r2)
        {
            Vector2 v = fakeVelocity; //for some reason, trying to get r2.velocity updates like the whole world

            if (iced <= 0)
            {
                r2.AddForce(new Vector2(-frictionForce*r2.mass*v.x, 0));
            }
            else
            {
                iced--;
            }

            if (r2.velocity.y < -maxFallSpeed)
            {
                r2.velocity += new Vector2(0, (-maxFallSpeed - r2.velocity.y));
            }

            if (r2.velocity.y > maxRiseSpeed)
            {
                r2.velocity -= new Vector2(0, (r2.velocity.y - maxRiseSpeed));
            }

            if (punchedCol != null && punchCooldown >= 1)
            {
                if (punchCooldown > 1)
                {
                    //Physics2D.IgnoreCollision(punchedCol, myBox, true);
                }
                if (punchCooldown == 1)
                {
                    Physics2D.IgnoreCollision(punchedCol, myBox, false);
                    punchedCol = null;
                }
                punchCooldown = Mathf.Max(punchCooldown - 1, 0);
            }

            if (dustShrink)
            {
                float velFake = r2.velocity.magnitude;
                var e = dustParticles.emission;
                if (velFake > 0.01f)
                {
                    dustLeft -= dustWearCoeff * velFake;

                    //particle effect
                    e.rateOverTime = Mathf.Min(maxParticleRate, dustParticleSpawnCoeff * velFake);
                    var s = dustParticles.shape;
                    s.position = Vector3.up * dustLeft;


                    float newY = origScale.y * (dustLeft / origDustLeft);
                    float dif = transform.localScale.y - newY;
                    transform.localScale = new Vector3(transform.localScale.x, newY, transform.localScale.z);
                    //transform.localPosition += Vector3.down * 0.5f * dif;
                    //r2.MovePosition(transform.position);

                    if (dustLeft < 4f)
                    {
                        //add effect soon
                        dustParticles.transform.SetParent(transform.parent);
                        e.rateOverTime = 0f;
                        Destroy(dustParticles.gameObject, 1f);
                        Destroy(gameObject);
                    }
                }
                else
                {
                    e.rateOverTime = 0f;
                }
            }
        }
        fakeVelocity = ((Vector2)transform.position - lastPos)*60f / Time.timeScale;
        lastPos = transform.position;
        //wallPress = 0;
    }
}
