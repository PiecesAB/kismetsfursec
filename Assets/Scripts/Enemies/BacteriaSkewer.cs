using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriaSkewer : GenericBlowMeUp
{
    public enum State
    {
        Charge, Retract
    }

    public Vector2[] waitList;
    private int i = 0;
    private int side = 0;

    private State state;
    private double stateStartTime;
    private double startTime;
    public double startDelay;

    public MeshRenderer main;
    public Transform needle;
    public Transform needleTarget;
    public bool extendToGroundNotTarget = false;
    public Animator dust;
    public float damageMultiplier = 1f;
    public Transform aim;
    private SpriteRenderer dustSpr;
    public BulletHellMakerFunctions shooter;
    public float shootStart = 8;
    public float shootInterval = 32;
    public AudioClip warnSound;
    public AudioClip shotSound;
    public bool onlyAddTimeOnVisible;

    private float needleLength;

    private AudioSource aud;
    private bool warned = false;

    void Start()
    {
        state = State.Charge;
        startTime = stateStartTime = DoubleTime.ScaledTimeSinceLoad + startDelay;
        needleLength = needleTarget.localPosition.x;
        needleTarget.gameObject.SetActive(false);
        dustSpr = dust.GetComponent<SpriteRenderer>();
        aud = GetComponent<AudioSource>();
        warned = false;
    }

    private RaycastHit2D GroundCast()
    {
        return Physics2D.Raycast(transform.position + 0.1f*transform.right, transform.right, 320f, 256 + 512 + 2048);
    }

    private float SwapState()
    {
        float stateTime = (side == 0) ? waitList[i].x : waitList[i].y;
        float prog = (float)((DoubleTime.ScaledTimeSinceLoad - stateStartTime) / stateTime);
        while (prog >= 1f)
        {
            stateStartTime += stateTime;
            if (side == 0) { side = 1; state = State.Retract; }
            else { side = 0; i = (i + 1) % waitList.Length; state = State.Charge; }
            stateTime = (side == 0) ? waitList[i].x : waitList[i].y;
            prog = (float)((DoubleTime.ScaledTimeSinceLoad - stateStartTime) / stateTime);
            warned = false;
            if (extendToGroundNotTarget && state == State.Retract)
            {
                RaycastHit2D r = GroundCast();
                needleLength = r.collider ? r.distance : 320f;
            }
            if (state == State.Retract)
            {
                if (!dust.gameObject.activeSelf)
                {
                    dust.gameObject.SetActive(true);
                }
                dust.gameObject.SetActive(true);
                dust.Play("main", -1, 0f);

                aud.Stop();
                aud.clip = shotSound;
                aud.Play();

                RaycastHit2D rplr = Physics2D.Raycast(transform.position, transform.right, needleLength, 1048576);
                if (rplr.collider)
                {
                    BasicMove bm = rplr.collider.GetComponent<BasicMove>();
                    if (bm && bm.CanCollide)
                    {
                        rplr.collider.GetComponent<KHealth>().ChangeHealth(-damageMultiplier * bm.Damage, "bacteriophage");
                        int d = Fakerand.Int(0, 2) * 2 - 1;
                        bm.AddBlood(rplr.point - Vector2.right * (d * 4f), transform.rotation);
                    }
                }

                if (shooter)
                {
                    for (float p = shootStart; p <= needleLength; p += shootInterval)
                    {
                        shooter.transform.position = transform.position + transform.right * p;
                        shooter.Fire();
                    }
                }
            }
        }
        return prog;
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (onlyAddTimeOnVisible && !main.isVisible)
        {
            startTime += Time.deltaTime;
            stateStartTime += Time.deltaTime;
        }
        if (DoubleTime.ScaledTimeSinceLoad < startTime) { return; }
        float prog = SwapState();
        switch (state)
        {
            case State.Charge:
            default:
                main.materials[0].SetVector("_MainTex_ST", new Vector4(0, 1f - 0.9f * prog, 0, 0));
                needle.localScale = new Vector3(3, 3, 0);
                if ((prog >= 0.75f || (1f - prog) * waitList[i].x <= 0.2f) && !warned)
                {
                    warned = true;
                    aud.Stop();
                    aud.clip = warnSound;
                    aud.Play();
                }
                break;
            case State.Retract:
                main.materials[0].SetVector("_MainTex_ST", new Vector4(0, 1f - 0.9f *  Mathf.Clamp01((1f - prog) * 5f), 0, 0));
                needle.localScale = new Vector3(3, 3, needleLength * 0.125f * Mathf.Clamp01((1f - prog) * 1.25f));
                needle.localPosition = new Vector3(needleLength * 0.5f * Mathf.Clamp01((1f - prog) * 1.25f), 0, 0);
                break;
        }
        dustSpr.transform.localPosition = new Vector3(needleLength * 0.5f, 0, 0);
        dustSpr.size = new Vector2(needleLength, 32f);
        if (aim)
        {
            if (extendToGroundNotTarget && transform.hasChanged && state == State.Charge && main.isVisible)
            {
                transform.hasChanged = false;
                RaycastHit2D r = GroundCast();
                needleLength = r.collider ? r.distance : 320f;
            }
            aim.localPosition = new Vector3(needleLength * 0.5f, 0, 0);
            aim.localScale = new Vector3(needleLength, aim.localScale.y, 1);
        }
    }
}
