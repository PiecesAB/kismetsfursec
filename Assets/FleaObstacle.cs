using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleaObstacle : GenericBlowMeUp
{
    public enum State
    {
        Grounded, Jumping, Attached
    }

    public enum Personality
    {
        Min, Max, MinMax, Random
    }

    public SkinnedMeshRenderer rend;
    public Collider2D enemCol;

    private Rigidbody2D rb;
    private BasicMove attachedPlr;
    private Collider2D col;
    private FollowThePlayer camMainFtp;

    private double timer;
    [HideInInspector]
    public State currState = State.Jumping;
    [HideInInspector]
    public Personality personality;
    [HideInInspector]
    public Vector2 currNormal;

    private double minJumpWait = 0.1;
    private double maxJumpWait = 0.25;
    private double minAttachWait = 4.0;
    private double maxAttachWait = 9.0;
    private double nextGroundWait = 0.05;
    private float minJumpPower = 270;
    private float maxJumpPower = 480;

    private int attachCoolDownFrames;

    private const float debuff = 0.8f;
    private const float plrRadius = 9f;
    private const float plrHeight = 16f;

    private int existFrames = 0;

    private double r1;
    private double r2;

    private double minMaxWeight = 0.5f;

    private Vector2 bmHitPos;

    private void Start()
    {
        currNormal = (transform.up + transform.right).normalized;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        attachedPlr = null;
        personality = (Personality)3;//(Personality)Fakerand.Int(0, 4);
        print(personality);
        minMaxWeight = 0.5;
        attachCoolDownFrames = 0;
        ChangeState(State.Jumping);
        camMainFtp = Camera.main.GetComponent<FollowThePlayer>();
        existFrames = 0;
    }

    private double RandomDouble(double a, double b)
    {
        switch (personality)
        {
            case Personality.Min:
                return Fakerand.Double(a, 0.75*a + 0.25*b);
            case Personality.Max:
                return Fakerand.Double(0.25*a + 0.75*b, b);
            case Personality.MinMax:
                double preRand = Fakerand.Double();
                double t = 2.0*Math.Abs(minMaxWeight - 0.5);
                preRand = ((1 - t) * preRand) + (t * minMaxWeight);
                minMaxWeight = (0.5 * minMaxWeight) + 0.5*(1.0 - preRand);
                return ((1 - preRand) * a )+ (preRand * b);
            case Personality.Random:
                return Fakerand.Double(a, b);
            default:
                return double.NaN;
        }
    }

    private void ChangeState(State s)
    {
        switch(s)
        {
            case State.Grounded:
                col.enabled = true;
                timer = DoubleTime.ScaledTimeSinceLoad + RandomDouble(minJumpWait, maxJumpWait);
                break;
            case State.Jumping:
                rb.isKinematic = false;
                rb.velocity = (float)RandomDouble(minJumpPower, maxJumpPower) * currNormal;
                col.enabled = true;
                timer = DoubleTime.ScaledTimeSinceLoad + nextGroundWait;
                break;
            case State.Attached:
                col.enabled = false;
                r1 = Fakerand.Double();
                r2 = Fakerand.Double();
                timer = DoubleTime.ScaledTimeSinceLoad + RandomDouble(minAttachWait, maxAttachWait);
                break;
            default:
                break;
        }

        if (attachedPlr)
        {
            foreach (Transform pt in attachedPlr.transform)
            {
                if (pt.GetComponent<Collider2D>() && pt.gameObject.layer == 19)
                {
                    Physics2D.IgnoreCollision(pt.GetComponent<Collider2D>(), enemCol, !col.enabled);
                }
            }
        }

        currState = s;
        //print(currState);
    }

    private void HitPlayer(BasicMove bm)
    {
        bm.jumpHeight *= debuff;
        bm.moveSpeed *= debuff;
        attachedPlr = bm;
        bmHitPos = bm.transform.InverseTransformPoint(transform.position);
        ChangeState(State.Attached);
    }

    private void HitBlock(Collision2D col)
    {
        if (currState != State.Jumping) { return; }
        if (DoubleTime.ScaledTimeSinceLoad < timer) { return; }
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        rend.SetBlendShapeWeight(0, 0);
        Vector2 norm = col.GetContact(0).normal;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, Mathf.Atan2(norm.y, norm.x) * Mathf.Rad2Deg - 90f);
        Vector2 plrPos = LevelInfoContainer.GetActiveControl().transform.position;
        Vector2 candNormalA = (transform.up + transform.right).normalized;
        Vector2 candNormalB = (transform.up - transform.right).normalized;
        if (((Vector2)transform.position + candNormalA - plrPos).sqrMagnitude > ((Vector2)transform.position + candNormalB - plrPos).sqrMagnitude)
        {
            transform.eulerAngles += new Vector3(0, 180, 0);
            currNormal = candNormalB;
        }
        else
        {
            currNormal = candNormalA;
        }
        
        ChangeState(State.Grounded);
    }

    private void Hit(Collision2D col)
    {
        if (currState != State.Attached && col.gameObject.layer == 20 && col.gameObject.GetComponent<BasicMove>()
            && attachCoolDownFrames == 0)
        {
            HitPlayer(col.gameObject.GetComponent<BasicMove>());
            return;
        }

        HitBlock(col);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Hit(col);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        Hit(col);
    }

    // ps... this might make some level designs impossible
    private bool OutOfBounds()
    {
        return (camMainFtp.cameraBounds.x > transform.position.x
            || camMainFtp.cameraBounds.z < transform.position.x
            || camMainFtp.cameraBounds.y > transform.position.y
            || camMainFtp.cameraBounds.w < transform.position.y)
            && !rend.isVisible;
    }

    private void Update()
    {
        if (Time.timeScale == 0 || !gameObject.activeSelf) { return; }

        ++existFrames;
        if (existFrames > 10010000)
        {
            existFrames = 10000;
        }

        attachCoolDownFrames = (attachCoolDownFrames <= 0) ? 0 : (attachCoolDownFrames - 1);

        if (currState == State.Grounded && DoubleTime.ScaledTimeSinceLoad >= timer && attachCoolDownFrames == 0)
        {
            ChangeState(State.Jumping);
            return;
        }

        if (currState == State.Attached)
        {
            if (DoubleTime.ScaledTimeSinceLoad < timer)
            {
                float t = (float)((r1 + DoubleTime.ScaledTimeSinceLoad) % 1.0)*Mathf.PI*2f;
                float h = (float)((r2 + (DoubleTime.ScaledTimeSinceLoad*0.4)) % 1.0) * Mathf.PI * 2f;
                transform.position = attachedPlr.transform.position + new Vector3(plrRadius*Mathf.Cos(t), Mathf.Cos(h), plrRadius*Mathf.Sin(t));
            }
            else
            {
                attachedPlr.jumpHeight /= debuff;
                attachedPlr.moveSpeed /= debuff;
                transform.position = attachedPlr.transform.TransformPoint(bmHitPos * 1.2f);
                attachCoolDownFrames = 90;
                currNormal = bmHitPos.normalized;
                ChangeState(State.Jumping);
                attachedPlr = null;
            }
            return;
        }

        if (camMainFtp && OutOfBounds() && existFrames > 10)
        {
            BlowMeUp();
        }
    }
}
