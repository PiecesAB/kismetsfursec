using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManAI : MonoBehaviour
{
    public enum WeaponType
    {
        HandToHand,
        Pistol,
        Crowbar,
    }

    public enum State
    {
        Resting,
        Vigilant,
        Alarmed,
        RunFrom,
        RunTo,
        Pause,
        Pause2,
        Punch,
    }

    public State currState;
    public Transform meshObj;
    [HideInInspector]
    public Animator myAnimator;
    [HideInInspector]
    public Rigidbody2D rg2;
    public Renderer myRenderer;
    public Vector2 dir;
    public Vector2 extraVel = Vector2.zero;
    public float runSpeed;
    public float myDamage;

    double t1 = 0.0;
    double t2 = 0.0;


    bool lastFrameVis = false;
    int myLayer;

    Vector2 internalVel = Vector2.zero;

    void Start()
    {
        myAnimator = GetComponentInChildren<Animator>();
        rg2 = GetComponent<Rigidbody2D>();
        myLayer = gameObject.layer;
        StateChange();
        //myRenderer = meshObj.GetComponent<Renderer>();
        lastFrameVis = myRenderer.isVisible;
        if (lastFrameVis)
        {
            Vis();
        }
        else
        {
            Invis();
        }
    }

    public void StateChange()
    {
        //print(currState.ToString());
        string s = currState.ToString();
        if (!myAnimator.GetCurrentAnimatorStateInfo(0).IsName(s))
        {
            myAnimator.Play(s);
            //myAnimator.SetTrigger("To" + s);
        }

        switch (currState)
        {
            case State.Resting:
                break;
            case State.Vigilant:
                //dir = Vector2.one;
                MakeNextT1();
                t2 = DoubleTime.ScaledTimeSinceLoad + 15f;
                break;
            case State.Alarmed:
                MakeNextT1(1);
                t2 = DoubleTime.ScaledTimeSinceLoad + 15f;
                //myAnimator.Play("Alarmed");
                break;
            case State.RunFrom:
                dir.x = -dir.x;
                currState = State.RunTo;
                MakeNextT1(2);
                //myAnimator.Play("RunFrom");
                break;
            case State.RunTo:
                MakeNextT1(3);
                //myAnimator.Play("RunTo");
                break;
            case State.Punch:
                //myAnimator.Play("Punch");
                MakeNextT1(1);
                break;
            case State.Pause:
            case State.Pause2:
                //myAnimator.Play("Pause");
                MakeNextT1(1);
                break;
            default:
                break;
        }
    }

    void Vis()
    {
        if (currState == State.Resting)
        {
            currState = State.Vigilant;
            StateChange();
        }
    }

    void Invis()
    {
        currState = State.Resting;
        StateChange();
    }

    void MakeNextT1(int f = 0)
    {
        switch(f)
        {
            case 0:
                t1 = DoubleTime.ScaledTimeSinceLoad + Fakerand.NormalDist(3f, 0.8f, 1f, 5f);
                break;
            case 1:
                t1 = DoubleTime.ScaledTimeSinceLoad + 0.5f;
                break;
            case 2:
                t1 = DoubleTime.ScaledTimeSinceLoad + Fakerand.NormalDist(0.7f, 0.5f, 0.1f, 3f);
                break;
            case 3:
                t1 = double.PositiveInfinity;
                break;
            default:
                break;
        }
    }

    bool Elapsed(double t)
    {
        return DoubleTime.ScaledTimeSinceLoad > t;
    }

    const float eyeHeight = 24f;
    const float lookAhead = 8f;
    const float ySize = 0f;
    const float yLookDownDist = 48f;

    bool LookForPlayer() //
    {
        gameObject.layer = 4;
        RaycastHit2D rh = Physics2D.Raycast(transform.position + transform.up * eyeHeight, transform.right*dir.x, 320f, 1048576 + 256 + 512);
        //Debug.DrawRay((transform.position + transform.up * eyeHeight), new Vector2(dir.x, 0f) * 320f, Color.red, 2f);
        gameObject.layer = myLayer;
        if (rh.collider && rh.collider.gameObject.layer == 20)
        {
            if (currState == State.Vigilant || currState == State.Resting)
            {
                currState = State.Alarmed;
                
                StateChange();
            }
            return true;
        }
        return false;
    }

    bool LookForHindrance() //
    {
        gameObject.layer = 4;
        RaycastHit2D rh1 = Physics2D.Raycast(transform.position + transform.up * eyeHeight, dir.x*transform.right, lookAhead, 1048576 + 256 + 512 + 2048);
        RaycastHit2D rh2 = Physics2D.Raycast(transform.position + dir.x * transform.right * lookAhead - transform.up * ySize, -transform.up, yLookDownDist, 1048576 + 256 + 512 + 2048);
        gameObject.layer = myLayer;
        //print(rh1.collider != null);
        if (rh2.collider == null)
        {
            currState = State.Pause;
            StateChange();
        }
        else if (rh1.collider)
        {
            if (currState == State.RunTo)
            {
                GameObject g = null;
                if (rh1.collider)
                {
                    g = rh1.collider.gameObject;
                }
                bool test1 = (g.layer == 20);
                bool test2 = (g.GetComponent<ManAI>() != null);
                if (rh1.collider && (test1 || test2))
                {
                    if (test1)
                    {
                        currState = State.Punch;
                        if (g.GetComponent<KHealth>() && g.GetComponent<BasicMove>())
                        {
                            g.GetComponent<KHealth>().ChangeHealth(-g.GetComponent<BasicMove>().Damage * myDamage, "man punch");
                        }
                    }

                    if (test2)
                    {
                        ManAI otherman = g.GetComponent<ManAI>();
                        if (otherman.currState == State.Vigilant || otherman.currState == State.Resting)
                        {
                            otherman.currState = State.Alarmed;
                        }
                        otherman.StateChange();
                        currState = State.Pause;
                    }
                }
                else
                {
                    currState = State.Pause;
                    
                }

                StateChange();
            }
            return true;
        }
        return false;
    }

    const float gravMult = 1f;
    const float maxYSpeed = 275f;

    void FixedUpdate()
    {
        if (Time.timeScale > 0)
        {
            if (rg2.velocity.sqrMagnitude > 1f && currState == State.Resting)
            {
                currState = State.Vigilant;
                StateChange();
                if (rg2.velocity.x != 0f)
                {
                    dir.x = -Mathf.Sign(rg2.velocity.x);
                }
            }

            switch (currState)
            {
                case State.Resting:
                case State.Vigilant:
                case State.Alarmed:
                case State.Pause:
                case State.Pause2:
                case State.Punch:
                    internalVel = new Vector2(0f, internalVel.y);
                    break;
                case State.RunFrom:
                case State.RunTo:
                    internalVel = new Vector2(dir.x * runSpeed, internalVel.y);
                    break;
                default:
                    break;
            }

            internalVel = new Vector2(internalVel.x + Physics2D.gravity.x, internalVel.y + Physics2D.gravity.y);
            internalVel = new Vector2(internalVel.x, Mathf.Clamp(internalVel.y, -maxYSpeed, maxYSpeed));
            float angle = -transform.eulerAngles.z * Mathf.Deg2Rad;
            float ca = Mathf.Cos(angle);
            float sa = Mathf.Sin(angle);
            rg2.velocity = new Vector2(internalVel.x * ca + internalVel.y * sa,
                                      -internalVel.x * sa + internalVel.y * ca); //rotate
            rg2.velocity += extraVel;
        }
    }

    void Update()
    {
        //print(currState);
        if (Time.timeScale > 0)
        {
            meshObj.localScale = new Vector3(1f, dir.y, dir.x);
            State oldState = currState;
            switch (currState)
            {
                case State.Resting:
                    LookForPlayer();
                    break;
                case State.Vigilant:
                    if (Elapsed(t1))
                    {
                        dir.x = -dir.x;
                        MakeNextT1();
                    }
                    if (Elapsed(t2))
                    {
                        currState = State.Resting;
                    }
                    LookForPlayer();
                    break;
                case State.Alarmed:
                    if (Elapsed(t1))
                    {
                        int d = Fakerand.Int(0, 3);
                        currState = (d == 0) ? State.RunTo : State.RunFrom;
                    }
                    break;
                case State.RunFrom:
                case State.RunTo:
                    if (KHealth.someoneDied)
                    {
                        currState = State.Vigilant;
                    }
                    LookForHindrance();
                    if (Elapsed(t1))
                    {
                        currState = State.Pause2;
                    }
                    if (Elapsed(t2))
                    {
                        currState = State.Vigilant;
                    }
                    break;
                case State.Pause:
                case State.Pause2:
                    if (Elapsed(t1))
                    {
                        int d = Fakerand.Int(0, 3);
                        if (d == 0)
                        {
                            dir.x = -dir.x;
                            currState = State.RunTo;
                        }
                        else
                        {
                            currState = State.RunFrom;
                        }
                    }
                    break;
                case State.Punch:
                    if (Elapsed(t1))
                    {
                        currState = State.RunTo;
                    }
                    break;
                default:
                    break;
            }

            if (currState != oldState)
            {
                StateChange();
            }

            if (myRenderer.isVisible != lastFrameVis)
            {
                if (lastFrameVis)
                {
                    Invis();
                }
                else
                {
                    Vis();
                }
                lastFrameVis = myRenderer.isVisible;
            }
        }
    }
}
