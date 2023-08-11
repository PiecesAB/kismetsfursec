using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherEnemy : MonoBehaviour
{
    public enum State
    {
        Wait, Throwing, Cooldown
    }

    public float strength = 400f;
    public Renderer visCheck;
    public State state;
    public AudioClip graspSound;
    public AudioClip throwSound;
    [HideInInspector]
    public Transform target;

    private Vector3 homePos;
    private Animator animator;
    private Collider2D myTrigger;
    private AudioSource myAS;

    private void Start()
    {
        state = State.Wait;
        homePos = transform.position;
        animator = GetComponent<Animator>();
        myTrigger = GetComponent<Collider2D>();
        myAS = GetComponent<AudioSource>();
    }

    public enum RemoveControlMode
    {
        Default, Flipwire
    }

    public static List<SpecialGunTemplate> gunsIDisabled = new List<SpecialGunTemplate>(); 

    public static void RemovePlayerControl(GameObject plr, RemoveControlMode removeControlMode = RemoveControlMode.Default)
    {
        SpecialGunTemplate sgt = plr.GetComponent<SpecialGunTemplate>();
        if (sgt && sgt.enabled)
        {
            gunsIDisabled.Add(sgt);
            sgt.enabled = false;
        }
        
        if (removeControlMode == RemoveControlMode.Default)
        {
            plr.GetComponent<BasicMove>().notFlipwired = false;
            plr.GetComponent<KHealth>().enabled = false;
            plr.GetComponent<Rigidbody2D>().isKinematic = true;
        }
        else
        {
            plr.GetComponent<BasicMove>().notFlipwired = false;
        }
    }

    public static void ImmediateRestoreControl(MonoBehaviour caller, GameObject plr)
    {
        caller.StartCoroutine(TrajectoryRestoreControl(Vector3.zero, plr, 0f, 1));
    }

    public static IEnumerator TrajectoryRestoreControl(Vector3 right, GameObject plr, float strength, int framesToMove = 10)
    {
        plr.GetComponent<BasicMove>().enabled = true;
        plr.GetComponent<BasicMove>().notFlipwired = true;
        plr.GetComponent<Rigidbody2D>().velocity = (Vector2)right * strength;
        plr.GetComponent<BasicMove>().fakePhysicsVel = Vector2.zero;
        plr.GetComponent<BasicMove>().extraPerFrameVel = (Vector2)right * strength;
        plr.GetComponent<BasicMove>().doubleJump = false;
        plr.GetComponent<KHealth>().enabled = true;
        plr.GetComponent<Rigidbody2D>().isKinematic = false;
        SpecialGunTemplate sgt = plr.GetComponent<SpecialGunTemplate>();
        if (sgt && gunsIDisabled.Contains(sgt))
        {
            sgt.enabled = true;
            gunsIDisabled.Remove(sgt);
        }

        yield return new WaitForEndOfFrame();

        bool doubleJumpDone = false; // prevent super velocitation
        if (plr.GetComponent<BasicMove>()) { plr.GetComponent<BasicMove>().doubleJump = true; }
        else { yield break; }
        if (framesToMove < 1) { framesToMove = 1; }

        for (int i = 0; i < framesToMove; ++i)
        {
            Vector2 v = (Vector2)right * strength;
            if (!(plr.GetComponent<BasicMove>()?.doubleJump ?? false) && !doubleJumpDone)
            {
                doubleJumpDone = true;
                float verAmt = Vector2.Dot(v, plr.transform.up);
                if (verAmt > plr.GetComponent<BasicMove>().jumpHeight)
                {
                    plr.GetComponent<BasicMove>().fakePhysicsVel += (verAmt - plr.GetComponent<BasicMove>().jumpHeight) * Vector2.down;
                }
            }
            plr.GetComponent<BasicMove>().extraPerFrameVel += (Vector2)right * strength;
            yield return new WaitForEndOfFrame();
        }

        plr.GetComponent<BasicMove>().fakePhysicsVel += (Vector2)right * strength;
    }

    private IEnumerator Throw(GameObject g)
    {
        if (state != State.Wait) { yield break; }
        target = g.transform;
        state = State.Throwing;

        RemovePlayerControl(g);

        animator.SetTrigger("Grasp");
        myAS.Stop();
        myAS.clip = graspSound;
        myAS.Play();


        double st = DoubleTime.ScaledTimeSinceLoad;
        while (DoubleTime.ScaledTimeSinceLoad - st < 0.5)
        {
            float prog = (float)(DoubleTime.ScaledTimeSinceLoad - st) / 0.5f;
            float posMul = 4 * prog * prog * (prog * prog - 1);
            transform.position = homePos + transform.right * posMul * 0.125f * strength;
            g.transform.position = transform.position;
            yield return new WaitForEndOfFrame();
        }

        animator.SetTrigger("LetGo");
        myAS.Stop();
        myAS.clip = throwSound;
        myAS.Play();

        transform.position = homePos;
        state = State.Cooldown;

        StartCoroutine(TrajectoryRestoreControl(transform.right, g, strength));

        yield return new WaitForSeconds(2.4f);

        animator.SetTrigger("Point");

        yield return new WaitForSeconds(0.5f);

        state = State.Wait;

        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { Physics2D.IgnoreCollision(col, myTrigger); return; }
        if (!col.gameObject.GetComponent<BasicMove>().CanCollide) { return; }
        StartCoroutine(Throw(col.gameObject));
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { Physics2D.IgnoreCollision(col, myTrigger); return; }
        if (!col.gameObject.GetComponent<BasicMove>().CanCollide) { return; }
        StartCoroutine(Throw(col.gameObject));
    }
}
