using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpEnemy : MonoBehaviour {

    public enum State
    {
        Dormant, MovingFrom, MovingTo, ConfrontTarget, Vanish
    }

    public int attemptsLeft = 2;
    //public float flightTime;
    public float gravityPerFrame;
    public float terminalVelocity;
    public float[] targetDistanceFromPlayer;
    public float minimumTargetDistance = 40;
    public State state;
    public string targetTag;
    public Transform currentTarget;
    public Vector3 currentTargPos;
    public float rayRadSize = 32;
    public int framesToMoveFrom = 20;
    public int framesToMoveTo = 20;
    public MeshRenderer heliWings;
    public MeshRenderer tvScreen;
    public SpriteRenderer transition;
    public bool lastsForever;
    public MeshRenderer screenMesh;
    public AudioSource screenBreakSound;
    public Texture2D screenBrokePic;
    public int framesDelayOnBroke;
    public bool screenBroken;
    public AudioSource teleSound;
    public Collider2D myCollider;
    private int move;
    private GameObject[] tagmans;
    private Vector2 norm;
    private int delay;

	void Start () {
        state = State.Dormant;
        currentTarget = null;
        currentTargPos = transform.position;
        move = 0;
        screenBroken = false;
        tagmans = GameObject.FindGameObjectsWithTag(targetTag);
        delay = 0;
    }

    public void Collided(Collision2D c)
    {
        if (!screenBroken && Vector2.Dot(c.contacts[0].normal,norm.normalized) < -0.7f) //roughly 90 degress
        {
            screenMesh.materials[1].mainTexture = screenBrokePic;
            screenMesh.materials[1].SetTexture("_EmissionMap", screenBrokePic);
            screenBreakSound.Play();
            if (c.collider.GetComponent<KHealth>() != null)
            {
                c.collider.GetComponent<KHealth>().electrocute += 0.5f;
            }
            screenBroken = true;
            attemptsLeft = 0;
            delay += framesDelayOnBroke;
        }
    }

    void CalcTargetPos()
    {
        Vector2 vel = currentTarget.GetComponent<Rigidbody2D>().velocity; //assuming the player has a rigidbody2d
        Vector3 ctp = currentTarget.position;
        vel = (vel == Vector2.zero) ? Vector2.right : vel;
        Vector3 x1 = ctp + (Vector3)(vel.normalized * targetDistanceFromPlayer[attemptsLeft]);
        int framesUntilImpact = Mathf.Min((int)(((Vector2)currentTarget.transform.position - (Vector2)transform.position).magnitude / vel.magnitude),25);
        float gt = vel.y;
        float ht = Mathf.Max(gt - gravityPerFrame, terminalVelocity);
        for (int i = framesUntilImpact - 1; i > 0; i--)
        {
            x1 += Vector3.down * (ht - gt);
            ht = Mathf.Max(ht - gravityPerFrame, terminalVelocity);
        }

        if ((x1- ctp).magnitude < minimumTargetDistance)
        {
            x1 = ((x1 - ctp).normalized * minimumTargetDistance) + ctp;
        }
        currentTargPos = x1;
    }

    void SetScreenRotation()
    {
        float ang = Mathf.Atan2(norm.y, norm.x) + Mathf.PI;
        transform.eulerAngles = new Vector3(0,(60*Mathf.Cos(ang))-90,60*Mathf.Sin(ang));
        myCollider.transform.eulerAngles = new Vector3(0, 0, ang * Mathf.Rad2Deg);
    }
	
	void Update () {
		if (state == State.Dormant && tvScreen.isVisible)
        {
            float closest = 10000f;
            foreach (GameObject p in tagmans)
            {
                if (p != null)
                {
                    Vector2 s0 = p.transform.position - transform.position;
                    Vector2 s = s0.normalized;
                    if (s0.magnitude < closest && Physics2D.Raycast((Vector2)transform.position + (rayRadSize * s), s, 10000f, 1049344).transform == p.transform)
                    {
                        closest = s0.magnitude;
                        currentTarget = p.transform;
                    }
                }
            }

            if (currentTarget != null)
            {
                state = State.MovingFrom;
                move = Mathf.Max((int)(framesToMoveFrom / Time.timeScale),4);
                transform.localScale = Vector3.one;
                attemptsLeft--;
                heliWings.enabled = false;
                tvScreen.enabled = false;
                transition.enabled = true;
                teleSound.Stop();
                teleSound.Play();
                myCollider.enabled = false;
            }
        }

        if (state == State.MovingFrom)
        {
            float rat = 1f / move;
            transform.position = Vector3.Lerp(transform.position, currentTarget.position, rat);
            transform.localEulerAngles = new Vector3(0, 0, 720f * (float)(DoubleTime.ScaledTimeSinceLoad % 1f));
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, rat);
            move--;
            if (move == 0)
            {
                if (attemptsLeft > 0)
                {
                    state = State.MovingTo;
                    transform.localScale = Vector3.zero;
                    move = Mathf.Max((int)(framesToMoveTo / Time.timeScale), 4);
                    CalcTargetPos();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        if (state == State.MovingTo)
        {
            float rat = 1f / move;
            CalcTargetPos();
            transform.position = Vector3.Lerp(transform.position, currentTargPos, rat);
            transform.localEulerAngles = new Vector3(0, 0, 720f * (float)(DoubleTime.ScaledTimeSinceLoad % 1f));
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, rat);
            move--;
            if (move == 0)
            {
                transform.localScale = Vector3.one;
                norm = currentTarget.position - transform.position;
                state = State.ConfrontTarget;
                heliWings.enabled = true;
                tvScreen.enabled = true;
                transition.enabled = false;
                SetScreenRotation();
                myCollider.enabled = true;
            }
        }

        if (state == State.ConfrontTarget)
        {
            Vector2 vel = currentTarget.GetComponent<Rigidbody2D>().velocity; //assuming the player has a rigidbody2d
            Vector2 np = currentTarget.position - transform.position;
            if (Vector2.Dot(vel,norm) > 0 || Vector2.Dot(np, norm) < 0)
            {
                if (delay > 0)
                {
                    delay--;
                }
                else
                {
                    state = State.MovingFrom;
                    move = Mathf.Max((int)(framesToMoveFrom / Time.timeScale), 4);
                    transform.localScale = Vector3.one;
                    if (!lastsForever)
                    {
                        attemptsLeft--;
                    }
                    heliWings.enabled = false;
                    tvScreen.enabled = false;
                    transition.enabled = true;
                    norm = Vector2.zero;
                    teleSound.Stop();
                    teleSound.Play();
                    myCollider.enabled = false;
                }
            }
        }


    }
}
