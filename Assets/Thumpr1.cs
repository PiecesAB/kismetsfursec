using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Thumpr1 : GenericBlowMeUp
{
    public enum State
    {
        Walk,Jump,Smash
    }

    private static BasicMove[] allPlayers = new BasicMove[0];
    private Rigidbody2D r2;
    private Animator an;
    public int dir = -1;
    public float speedMult = 1f;
    public float damage = 11f;
    public State state;
    public Vector2 targetPoint;

    private Transform edc;
    private const float basicSpeed = 90f;
    private double changeDirCooldown = -500f;
    private double smashCooldown = -500f;
    private float gravDown = 0f;
    private float mostGravDown = -300f;
    private const double changeDirCooldownWait = 0.15f;
    //private double smashCooldownWait = 2f;
    private const float smashDist = 96f;
    private float smashHeight = 32f;
    private HashSet<KHealth> smashedMans = new HashSet<KHealth>();
    private static Thumpr1 first;

    private void Start()
    {
        if (first == null)
        {
            allPlayers = FindObjectsOfType<BasicMove>();
            first = this;
        }
        r2 = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();
        gravDown = 0f;
        state = State.Walk;
        edc = transform.Find("EnemyDamageCollider");
        //smashCooldownWait += Fakerand.Single();
        //dir = -1;
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        Vector3 lpos = transform.InverseTransformPoint(ohi.pos);
        speedMult += 0.3f;
        state = State.Jump;
        if (lpos.x < 0f)
        {
            targetPoint = (Vector2)(transform.right*smashDist) + (Vector2)(transform.up * smashHeight);
        }
        else
        {
            targetPoint = (Vector2)(transform.right * -smashDist) + (Vector2)(transform.up * smashHeight);
        }
    }

        private IEnumerator WalkAfterSmash()
    {
        double st = DoubleTime.ScaledTimeSinceLoad;
        yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad - st >= 0.3f));
        state = State.Walk;
        smashedMans.Clear();
        if (KHealth.someoneDied)
        {
            allPlayers = new BasicMove[0];
        }
    }

    private IEnumerator AboutToSmash()
    {
        double st = DoubleTime.ScaledTimeSinceLoad;
        yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad - st >= 0.07f));
        if (state == State.Jump)
        {
            state = State.Smash;
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        Vector2 norm = transform.InverseTransformDirection(col.GetContact(0).normal);
        if (norm.y > 0.707f)
        {
            gravDown = 0f;
            if (state == State.Smash)
            {
                KHealth kh = col.gameObject.GetComponent<KHealth>();
                if (kh && !smashedMans.Contains(kh))
                {
                    smashedMans.Add(kh);
                    kh.ChangeHealth(-damage, "thumpr");
                    
                }
                StartCoroutine(WalkAfterSmash());
            }
        }
        else if (norm.y < -0.9f)
        {
            gravDown = mostGravDown;
            state = State.Smash;
        }
        else if (norm.x*dir < -0.8f && DoubleTime.UnscaledTimeSinceLoad - changeDirCooldown >= changeDirCooldownWait)
        {
            changeDirCooldown = DoubleTime.UnscaledTimeSinceLoad;
            dir = -dir;
            if (state == State.Jump)
            {
                state = State.Walk;
            }
        }
    }

    private void Update()
    {
        transform.localScale = new Vector3(dir * -1f, 1f, 1f);
        edc.localScale = transform.localScale;
        an.speed = speedMult*Time.timeScale;

        if (state == State.Walk)
        {
            an.SetTrigger("Walk");
            Vector2 mvt = new Vector2(basicSpeed * speedMult * dir, gravDown);
            r2.velocity = transform.TransformDirection(mvt);
            gravDown -= 2f;
            gravDown = Mathf.Max(gravDown, mostGravDown);
            for (int i = 0; i < allPlayers.Length; ++i)
            {
                Vector2 ppos = allPlayers[i].transform.position;
                Vector2 tpos = transform.InverseTransformPoint(ppos);
                if ( Vector2.Dot(-1f*transform.right,tpos.normalized) > 0.707f && Mathf.Abs(tpos.x) < smashDist)
                {
                    smashHeight = 32f + Fakerand.Single() * 32f;
                    state = State.Jump;
                    targetPoint = ppos + (Vector2)(transform.up * smashHeight);
                }
            }
        }

        if (state == State.Jump)
        {
            an.SetTrigger("Smash");
            r2.MovePosition(Vector2.MoveTowards(transform.position, targetPoint, 5f*speedMult*Time.timeScale));
            if (((Vector2)transform.position - targetPoint).sqrMagnitude < 4f)
            {
                StartCoroutine(AboutToSmash());
            }
        }

        if (state == State.Smash)
        {
            an.SetTrigger("Smash2");
            Vector2 mvt = new Vector2(basicSpeed * speedMult * dir * 0.07f, mostGravDown);
            r2.velocity = transform.TransformDirection(mvt);
        }
    }
}
