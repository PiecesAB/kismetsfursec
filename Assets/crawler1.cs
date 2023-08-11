using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crawler1 : GenericBlowMeUp {
    
    public enum CrawlMode
    {
        Convex, Concave
    }

    public enum State
    {
        Fall, Walk, Turn
    }

    public CrawlMode crawlMode = CrawlMode.Convex;
    public GameObject[] legs;
    public Renderer body;
    public float speed;
    private const float speedToAnimRatio = 64;
    private Animator[] animators;
    private float gravVel;
    private const float maxGravVel = 4f;
    private const float quarterPi = .78539816f; //that's all the precision you get JERK
    private const float halfPi = 1.5707963f;
    private const float colSize = 8f;
    public State state;
    public BoxCollider2D ground; //only used while turning, for going around the corner.
    public Vector2 groundCorner; //you know
    private float oldRotation; //for measuring a right angle
    public int framesSinceChangedDir;
    public Rigidbody2D r2;
    public AudioSource crawlSound;
    //public GameObject explodeEffect;

    public AllDirectionController beingControlled;

    private Vector3 lastFramePos;

    private List<Collider2D> walkingGround = new List<Collider2D>();
    private Collider2D finalGround;

    private Collider2D bumpedWallThisFrame;
    private Vector2 bumpedWallDist;

    private Collider2D[] myMainColliders;

    private long framesExisted = 0;

    //fix this to work when rotated

    RaycastHit2D GroundTest(Vector2 origin, float rot)
    {
        PrimMovingPlatform pmp = null;
        if (ground)
        {
            pmp = ground.GetComponent<PrimMovingPlatform>();
        }
        Vector2 of = pmp ? pmp.dif : Vector2.zero;
       return Physics2D.Raycast((Vector2)transform.localPosition+origin+of, new Vector2(Mathf.Cos(rot),Mathf.Sin(rot)), colSize+4f, 2816, transform.localPosition.z - 16, transform.localPosition.z + 16);
    }

    private void InitStuff()
    {
        if (!enabled) { return; }
        walkingGround = new List<Collider2D>();
        lastFramePos = transform.localPosition;
        r2 = GetComponent<Rigidbody2D>();
        framesSinceChangedDir = 2;
        animators = new Animator[legs.Length];
        ground = null;
        int i = 0;
        for (int v = 0; v < legs.Length; v++)
        {
            Animator ani = legs[v].transform.GetChild(0).GetComponent<Animator>();
            ani.Play(0, -1, Fakerand.Single());
            animators[i++] = ani;
        }

        StateUpdate(speed * speedToAnimRatio, transform.eulerAngles.z * Mathf.Deg2Rad);
    }

    private bool ambushDisabledOnce = false;

    private void OnEnable()
    {
        InitStuff();
    }

    private void Start () {
        bumpedWallThisFrame = null;
        if (gameObject.activeInHierarchy)
        {
            InitStuff();
        }
	}

    public State StateUpdate (float rs, float rot)
    {

        if (crawlMode == CrawlMode.Convex)
        {
            State newState = State.Fall;

            //RaycastHit2D rc1 = GroundTest(Vector2.zero, rot - halfPi);
            if (state != State.Turn)
            {
                //if (rc1.collider == null || rc1.collider as BoxCollider2D == null)
                if (walkingGround.Count == 0)
                {
                    //depending on this second check, you are either falling or turning.
                    //float t1 = -1.1f * Time.timeScale * speed;
                    //RaycastHit2D rc2 = GroundTest(new Vector2(t1 * Mathf.Cos(rot), t1 * Mathf.Sin(rot)), rot - halfPi);
                    if (framesExisted > 2)
                    {
                        if (finalGround == null || finalGround as BoxCollider2D == null || !finalGround.enabled)
                        {
                            newState = State.Fall;
                            ground = null;
                        }
                        else
                        {
                            newState = State.Turn;
                            ground = finalGround as BoxCollider2D;
                            oldRotation = -90f * Mathf.Sign(speed);
                            //which corner (choose the closest)
                            groundCorner = ground.bounds.ClosestPoint(transform.position);
                        }
                    }
                    else
                    {
                        newState = State.Walk;
                    }
                }
                else
                {
                    newState = State.Walk;
                    ground = walkingGround[0] as BoxCollider2D;
                    if (state == State.Fall)
                    {
                        groundCorner = ground.bounds.ClosestPoint(transform.position);
                        transform.position = groundCorner + colSize * new Vector2(-Mathf.Sin(rot), Mathf.Cos(rot));
                        lastFramePos = transform.position;
                    }

                }
            }
            else
            {
                return State.Turn;
            }

            return newState;
        }
        else //concave
        {
            /*RaycastHit2D rc0 = Physics2D.Raycast(transform.position, transform.right, 1f, 256 + 512);
            if (rc0.collider)
            {
                BlowMeUp();
                return State.Walk;
            }
            else*/
            {
                if (state == State.Walk)
                {
                    //RaycastHit2D rc1 = GroundTest(Vector2.zero + (Vector2)transform.right * -2f * Mathf.Sign(speed), rot + ((speed < 0) ? Mathf.PI : 0f));
                    //if (rc1.collider)
                    if (bumpedWallThisFrame)
                    {
                        transform.position = (Vector2)bumpedWallThisFrame.bounds.ClosestPoint((Vector2)transform.position) + (Vector2)transform.right * -8f * Mathf.Sign(speed);
                        oldRotation = 90f * Mathf.Sign(speed);
                        return State.Turn;
                    }
                    else
                    {
                        return State.Walk;
                    }
                }
                else
                {
                    return State.Turn;
                }
            }
        }

    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        Vector3 lpos = transform.InverseTransformPoint(ohi.pos);
        if (lpos.x < 0f)
        {
            speed = Mathf.Abs(speed);
            speed += ohi.amt * 0.5f;
        }
        else
        {
            speed = -Mathf.Abs(speed);
            speed -= ohi.amt * 0.5f;
        }

        if (crawlSound == null) { return; }
        crawlSound.pitch += ohi.amt * 0.25f;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<KHealth>() != null && col.gameObject.GetComponent<BasicMove>() != null && col.collider.gameObject.layer != 19)
        {
            KHealth k = col.gameObject.GetComponent<KHealth>();
            BasicMove b = col.gameObject.GetComponent<BasicMove>();
            float dmamt = LevelInfoContainer.GetScalingSpikeDamage();
            if (crawlMode == CrawlMode.Convex)
            {
                k.ChangeHealth(-dmamt, "crawler");
            }
            else
            {
                k.ChangeHealth(-dmamt, "gnat");
            }
            float rs = speed * speedToAnimRatio;
            float rot = transform.eulerAngles.x * Mathf.Deg2Rad;
            GetComponent<AudioSource>().Play();
            b.fakePhysicsVel = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot))*rs - (col.contacts[0].normal * 200f);
        }
        if (((1 << col.collider.gameObject.layer) & (256+512+2048)) != 0)
        {
            if (Vector2.Dot(transform.up, col.GetContact(0).normal) > 0.99f)
            {
                if (!walkingGround.Contains(col.collider))
                {
                    walkingGround.Add(col.collider);
                }
            }
            else
            {
                bumpedWallThisFrame = col.collider;
                bumpedWallDist = -col.GetContact(0).separation * col.GetContact(0).normal;
                if (crawlMode == CrawlMode.Concave) {
                    StateUpdate(speed * speedToAnimRatio, transform.eulerAngles.z * Mathf.Deg2Rad);
                }
            }

            //gnat special check to destroy if inside the ground
            if ((col.otherCollider is CircleCollider2D) && (col.otherCollider as CircleCollider2D).radius < 2f && beingControlled == null)
            {
                /*print("crawler1 destroyed in ground by " + col.collider.gameObject.name);
                if (state == State.Walk) { print("while walking"); }
                if (bumpedWallThisFrame) { print("bumped wall this frame"); }*/
                //Destroy(this);
                BlowMeUp();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (beingControlled)
        {
            if (col.rigidbody.isKinematic)
            {
                transform.localPosition += (Vector3)(speed * speedToAnimRatio * 0.03333333333f * Time.timeScale * col.GetContact(0).normal);
                
            }
            else
            {
                transform.localPosition += (Vector3)(speed * speedToAnimRatio * 0.01666666666f * Time.timeScale * col.GetContact(0).normal);
            }
            lastFramePos = transform.localPosition;
        }
        if (((1 << col.collider.gameObject.layer) & (256 + 512 + 2048)) != 0)
        {
            if (Vector2.Dot(transform.up, col.GetContact(0).normal) > 0.99f)
            {
                if (!walkingGround.Contains(col.collider)) { walkingGround.Add(col.collider); }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (walkingGround.Contains(col.collider)) {
            if (walkingGround.Count == 1)
            {
                finalGround = col.collider;
            }
            walkingGround.Remove(col.collider);
        }
    }

    void Forth(float rs, float rot)
    {
        Vector3 rotvec = new Vector3(Mathf.Cos(rot), Mathf.Sin(rot));
        if (r2)
        {
            //r2.velocity = Vector2.zero;
            Vector2 ab = lastFramePos + rotvec * rs * 0.016666666f * Time.timeScale;
            transform.localPosition = ab;
            //r2.MovePosition(ab);
            lastFramePos = transform.localPosition;
        }
        else {
            transform.localPosition += rotvec * rs * 0.016666666f * Time.timeScale; //  64/60
        }
    }

    // Update is called once per frame
    void Update () {
        if (myMainColliders == null) { myMainColliders = GetComponents<Collider2D>(); }
        if (Time.timeScale == 0 && gameObject.activeInHierarchy && crawlMode == CrawlMode.Concave)
        {
            for (int i = 0; i < myMainColliders.Length; ++i)
            {
                myMainColliders[i].enabled = false;
            }
        } 
        if (Time.timeScale > 0 && body /*&& body.isVisible*/ && gameObject.activeInHierarchy)
        {
            ++framesExisted;

            for (int i = 0; i < myMainColliders.Length; ++i)
            {
                myMainColliders[i].enabled = true;
            }

            float rs = speed * speedToAnimRatio;
            float rot = transform.eulerAngles.z * Mathf.Deg2Rad;
            float speedMult = 1f;

            if (!beingControlled)
            {
                State newState = StateUpdate(rs, rot);
                state = newState;

                if (newState == State.Walk)
                {
                    if (ground && ground.attachedRigidbody && ground.attachedRigidbody.velocity.magnitude >= 0.1f)
                    {
                        Vector3 s = (Vector3)ground.attachedRigidbody.velocity * Time.deltaTime;
                        lastFramePos += s;
                        transform.localPosition += s;
                    }

                    if (crawlMode == CrawlMode.Convex)
                    {
                        //RaycastHit2D rch = GroundTest(Vector2.zero, rot + halfPi - (halfPi * Mathf.Sign(speed)));
                        //if (rch.collider != null)
                        if (bumpedWallThisFrame)
                        {
                            speed = -speed;
                            lastFramePos = transform.position = transform.position + (Vector3)bumpedWallDist;
                            if (framesSinceChangedDir < 2 && beingControlled == null)
                            {
                                BlowMeUp();
                            }
                            framesSinceChangedDir = 0;
                        }
                    }
                    Forth(rs, rot);
                    gravVel = 0f;
                }
                else if (newState == State.Turn)
                {
                    if (ground && ground.attachedRigidbody && ground.attachedRigidbody.velocity.magnitude >= 0.1f)
                    {
                        Vector3 s = (Vector3)ground.attachedRigidbody.velocity * Time.deltaTime;
                        lastFramePos += s;
                        groundCorner += (Vector2)s;
                    }

                    if (System.Math.Abs(oldRotation) > Time.timeScale * 5f * System.Math.Abs(speed))
                    {
                        if (crawlMode == CrawlMode.Convex)
                        {
                            float tr = Time.timeScale * 5f * System.Math.Abs(speed) * Mathf.Sign(oldRotation);
                            oldRotation -= tr;
                            transform.rotation *= Quaternion.AngleAxis(tr, Vector3.forward);
                            rot = transform.eulerAngles.z * Mathf.Deg2Rad;
                            transform.position = groundCorner + colSize * new Vector2(-Mathf.Sin(rot), Mathf.Cos(rot));
                        }
                        else //concave
                        {
                            float tr = Time.timeScale * 5f * System.Math.Abs(speed) * Mathf.Sign(oldRotation);
                            oldRotation -= tr;
                            transform.rotation *= Quaternion.AngleAxis(tr, Vector3.forward);
                            rot = transform.eulerAngles.z * Mathf.Deg2Rad;
                        }
                        // Debug.DrawLine(groundCorner, transform.position, Fakerand.Color());
                    }
                    else
                    {
                        transform.rotation *= Quaternion.AngleAxis(oldRotation, Vector3.forward);
                        oldRotation = 0f;
                        state = State.Walk;
                        rot = transform.eulerAngles.z * Mathf.Deg2Rad;
                        if (crawlMode == CrawlMode.Convex)
                        {
                            transform.position = groundCorner + colSize * new Vector2(-Mathf.Sin(rot), Mathf.Cos(rot));
                        }
                        lastFramePos = transform.position;
                        Forth(rs, rot);
                    }
                    gravVel = 0f;
                }
                else //fall. no compare needed
                {
                    gravVel = Mathf.Min(gravVel + 0.1f, maxGravVel);
                    transform.localPosition += -transform.up * gravVel;
                    speedMult = 0f;
                }
            }
            else //controlled by player
            {
                state = State.Walk;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,0f);
                //r2.velocity = Vector2.zero;
                Vector2 ab = (Vector2)lastFramePos + 2f * Mathf.Abs(rs) * 0.016666666f * Time.timeScale * beingControlled.direction;
                transform.localPosition = ab;
                
                //r2.MovePosition(ab);
                lastFramePos = transform.localPosition;
            }

            if (body.isVisible)
            {
                float animFakeSpeed = speed;
                if (beingControlled) { animFakeSpeed = beingControlled.direction.x; }

                if (crawlMode == CrawlMode.Convex)
                {
                    for (int i = 0; i < animators.Length; i++)
                    {
                        if (i < 2 || i == 5) //left
                        {
                            animators[i].SetFloat("FakeSpeed", -animFakeSpeed * speedMult);
                        }
                        else //right
                        {
                            animators[i].SetFloat("FakeSpeed", animFakeSpeed * speedMult);
                        }
                    }
                }
                else if (body is SkinnedMeshRenderer) //concave
                {
                    (body as SkinnedMeshRenderer).SetBlendShapeWeight(0, (float)DoubleTime.DoublePong(DoubleTime.ScaledTimeSinceLoad * animFakeSpeed * 1400.0, 100.0));

                    if (speed >= 0f)
                    {
                        body.transform.localPosition = new Vector3(-4f, -4f, 0f);
                        body.transform.localEulerAngles = new Vector3(-90f, 30f, 0f);
                    }
                    else
                    {
                        body.transform.localPosition = new Vector3(4f, -4f, 0f);
                        body.transform.localEulerAngles = new Vector3(-90f, 150f, 0f);
                    }
                }
            }
        }

        if (crawlMode == CrawlMode.Convex && Time.timeScale > 0)
        {
            framesSinceChangedDir = Mathf.Min(framesSinceChangedDir + 1, 2);
        }
        //lastFramePos = transform.localPosition;
        if (Time.timeScale > 0)
        {
            bumpedWallThisFrame = null;
        }
    }
}
