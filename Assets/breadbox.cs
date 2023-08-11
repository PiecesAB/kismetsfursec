using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breadbox : GenericBlowMeUp
{
    public enum State
    {
        Walk, Charge, Thrust, Cooldown
    }

    public static List<GameObject> listOfPlayers = new List<GameObject>();
    public Renderer renderObj;
    public SkinnedMeshRenderer eyebrow;
    public Transform eyes;
    private Vector3 eyesOrigin;
    public float walkSpeed;
    public float chargeSpeed;
    public State state;
    public int chargeDir;
    public int stateHelp;
    public Transform collisionObj;

    private const int waitBuffer = 20;
    private const int chargeFrames = 30;
    private const float normalGravity = 50f;

    private static breadbox main;

    void Start()
    {
        if (main == null)
        {
            listOfPlayers = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
            main = this;
        }
        eyesOrigin = eyes.localPosition;
        state = State.Walk;
        stateHelp = 20;
    }

    GameObject GetClosestPlr(out Vector2 dir)
    {
        GameObject best = null;
        float dist = 100000000f;
        dir = Vector2.zero;
        foreach (var p in listOfPlayers)
        {
            Vector2 rx = (p.transform.position - transform.position);
            float nd = rx.sqrMagnitude;
            if (nd < dist)
            {
                dist = nd;
                best = p;
                dir = rx;
            }
        }
        return best;
    }

    void Update()
    {
        Rigidbody2D rg = GetComponent<Rigidbody2D>();
        Animator anim = GetComponent<Animator>();
        if (renderObj.isVisible)
        {
            Vector2 dir;
            GameObject closestPlr = GetClosestPlr(out dir);
            Vector2 dirN = dir.normalized;
            eyes.localPosition = eyesOrigin + (Vector3)(2f * dirN);
            float w = eyebrow.GetBlendShapeWeight(0);
            eyebrow.SetBlendShapeWeight(0, Mathf.MoveTowards(w, (Vector2.Dot(Vector2.right, dir) > 0f) ? 100f : 0f , 4f));
            collisionObj.GetComponent<Collider2D>().enabled = true;

            if (state == State.Walk)
            {
                anim.Play("breadwalk");
                transform.localEulerAngles = Vector3.zero;
                rg.gravityScale = normalGravity;
                if (System.Math.Abs(dir.x) > 48f)
                {
                    rg.velocity = new Vector2(Mathf.Sign(dir.x) * walkSpeed, rg.velocity.y);
                }
                else
                {
                    rg.velocity = new Vector2(0f, rg.velocity.y);
                }

                if (System.Math.Abs(dir.y) < 31f) //31 the succ number
                {
                    stateHelp--;
                    if (stateHelp == 0)
                    {
                        state = State.Charge;
                        chargeDir = (dir.x >= 0f) ? -1 : 1;
                    }
                }
            }

            if (state == State.Charge)
            {
                anim.Play("breadstand");
                rg.velocity = Vector2.right * chargeDir * walkSpeed * 0.5f;
                rg.gravityScale = 0f;
                stateHelp--;
                Vector3 eu = transform.localEulerAngles;
                transform.localEulerAngles = new Vector3(eu.x, Mathf.MoveTowardsAngle(eu.y, chargeDir * 90f, 270f / chargeFrames), eu.z);
                if (stateHelp == -chargeFrames)
                {
                    state = State.Thrust;
                    transform.localEulerAngles = new Vector3(eu.x, chargeDir * 90f, eu.z);
                }

            }

            if (state == State.Thrust)
            {
                anim.Play("breadstand");
                rg.velocity = Vector2.left*chargeDir*chargeSpeed;
                rg.gravityScale = 0f;
                Vector3 eu = transform.localEulerAngles;
                transform.localEulerAngles = new Vector3(eu.x, chargeDir * 90f, eu.z);
                //can only escape this by helper collider
            }

            if (state == State.Cooldown)
            {
                anim.Play("breadwalk");
                rg.velocity = Vector2.right * chargeDir * walkSpeed;
                rg.gravityScale = 0f;
                stateHelp++;
                Vector3 eu = transform.localEulerAngles;
                transform.localEulerAngles = new Vector3(eu.x, Mathf.MoveTowardsAngle(eu.y, 0f, 90f / chargeFrames), eu.z);
                if (stateHelp == 0)
                {
                    stateHelp = waitBuffer;
                    state = State.Walk;
                    transform.localEulerAngles = new Vector3(eu.x, 0f, eu.z);
                }

            }
        }
        else
        {
            if (state == State.Thrust)
            {
                state = State.Cooldown;
            }
            rg.velocity = Vector2.zero;
            rg.gravityScale = 0f;
            collisionObj.GetComponent<Collider2D>().enabled = false;
        }

        collisionObj.localEulerAngles = -transform.localEulerAngles;


    }
}
