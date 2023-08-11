using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minister1 : GenericBlowMeUp
{
    public enum State
    {
        Idle, TeleportIn, TeleportOut, Charge, Throw, TeleportSoon
    }

    public State myState;
    public Vector2 targPos;
    public Vector2 halfTargPos;
    public SkinnedMeshRenderer smr;
    public Animator an;
    public SpriteRenderer teleDots;
    public SpriteRenderer chargeOrb;
    public GameObject[] bulletPrefabs;
    public float speedMult = 1f;

    private AudioSource aso;
    public AudioClip chargeSound;
    public AudioClip releaseSound;
    
    private double t0;
    private double t1;

    private int ti1;

    private double[] actionTimes = new double[4] { 0.6, 0.8, 1.0, 1.2 };
    private Vector2[] attackDirs = new Vector2[4] { new Vector2(128f, 0f), new Vector2(0f, 96f), new Vector2(-128f, 0f), new Vector2(0f, -96f) };
    private int currAttackDirIndex = -1;
    public int lastAttackDirIndex = -1;
    private Vector2 shotPos = new Vector2(-20f, 16f);
    private Vector2[] shotCorrection = new Vector2[4] { new Vector2(0f, -1f), new Vector2(-1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f) };

    private static List<GameObject> plrs;

    void Start()
    {
        t1 = DoubleTime.ScaledTimeSinceLoad;
        myState = State.Idle;
        plrs = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        aso = GetComponent<AudioSource>();
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        speedMult += 0.25f;
        myState = State.TeleportIn;
        MakeTargets();
        ti1 = 20;
    }

        void MakeTargets()
        {
        List<GameObject> visiblePlrs = new List<GameObject>();
        lastAttackDirIndex = currAttackDirIndex;
        for (int i = 0; i < plrs.Count; i++)
        {
            if (plrs[i] && plrs[i].GetComponent<Renderer>() && plrs[i].GetComponent<Renderer>().isVisible)
            {
                visiblePlrs.Add(plrs[i]);
            }
        }

        if (visiblePlrs.Count > 0)
        {
            Transform targPlr = visiblePlrs[Fakerand.Int(0, visiblePlrs.Count)].transform;
            List<Vector2> allowedDirs = new List<Vector2>();
            for (int i = 0; i < attackDirs.Length; i++)
            {
                Vector3 pl = Camera.main.WorldToViewportPoint(targPlr.position + (Vector3)attackDirs[i]);
                if (pl.x == Mathf.Clamp01(pl.x) && pl.y == Mathf.Clamp01(pl.y))
                {
                    allowedDirs.Add(attackDirs[i]);
                }
            }

            if (allowedDirs.Count > 0)
            {
                currAttackDirIndex = Fakerand.Int(0, allowedDirs.Count);
                Vector2 corr = new Vector2(shotCorrection[currAttackDirIndex].x * shotPos.x, shotCorrection[currAttackDirIndex].y * shotPos.y);
                targPos = (Vector2)targPlr.position + corr + allowedDirs[currAttackDirIndex];
                halfTargPos = Vector2.Lerp(transform.position, targPos, 0.5f);
            }
            else
            {
                currAttackDirIndex = Fakerand.Int(0, attackDirs.Length);
                Vector2 corr = new Vector2(shotCorrection[currAttackDirIndex].x * shotPos.x, shotCorrection[currAttackDirIndex].y * shotPos.y);
                targPos = (Vector2)targPlr.position + corr + attackDirs[currAttackDirIndex];
                halfTargPos = Vector2.Lerp(transform.position, targPos, 0.5f);
            }
        }
    }

    void Update()
    {
        if (Time.timeScale > 0 && smr && an && teleDots && (myState != State.Idle || smr.isVisible))
        {
            switch (myState)
            {
                case State.Idle:
                    smr.enabled = true;
                    teleDots.enabled = false;
                    chargeOrb.enabled = false;
                    if (smr.isVisible)
                    {
                        myState = State.TeleportSoon;
                        t1 = DoubleTime.ScaledTimeSinceLoad + actionTimes[Fakerand.Int(0,actionTimes.Length)]* (1f/speedMult);
                    }
                    break;
                case State.TeleportIn:
                    smr.enabled = false;
                    teleDots.enabled = true;
                    chargeOrb.enabled = false;
                    transform.position = Vector3.Lerp(transform.position, new Vector3(halfTargPos.x, halfTargPos.y, transform.position.z), 1f / ti1);
                    float f1 = ti1 * 0.05f;
                    teleDots.transform.localScale = new Vector3(f1, f1, 1f);
                    teleDots.transform.localEulerAngles = new Vector3(0f, 0f, (float)((DoubleTime.ScaledTimeSinceLoad % 0.5) * 720.0));
                    transform.eulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(currAttackDirIndex * 90f,lastAttackDirIndex *90f,0.5f + f1*0.5f));
                    ti1--;
                    if (ti1 == 0)
                    {
                        myState = State.TeleportOut;
                        ti1 = 20;
                    }
                    break;
                case State.TeleportOut:
                    smr.enabled = false;
                    teleDots.enabled = true;
                    chargeOrb.enabled = false;
                    transform.position = Vector3.Lerp(transform.position, new Vector3(targPos.x, targPos.y, transform.position.z), 1f / ti1);
                    float f2 = (20-ti1) * 0.05f;
                    teleDots.transform.localScale = new Vector3(f2, f2, 1f);
                    teleDots.transform.localEulerAngles = new Vector3(0f, 0f, (float)((DoubleTime.ScaledTimeSinceLoad % 0.5) * -720.0));
                    transform.eulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(lastAttackDirIndex * 90f, currAttackDirIndex * 90f, f2 * 0.5f + 0.5f));
                    ti1--;
                    if (ti1 == 0)
                    {
                        myState = State.Charge;
                        t0 = DoubleTime.ScaledTimeSinceLoad;
                        t1 = DoubleTime.ScaledTimeSinceLoad + actionTimes[Fakerand.Int(0, actionTimes.Length)] * (1f / speedMult);
                        transform.eulerAngles = new Vector3(0f, 0f, currAttackDirIndex * 90f);
                        an.Play("minister charge");
                    }
                    break;
                case State.Charge:
                    smr.enabled = true;
                    teleDots.enabled = false;
                    chargeOrb.enabled = true;
                    float s1 = (float)( ((DoubleTime.ScaledTimeSinceLoad - t0) / (t1 - t0)) + 0.1*System.Math.Sin(DoubleTime.ScaledTimeSinceLoad*80.0));
                    chargeOrb.transform.localScale = new Vector3(s1, s1, 1f);
                    if (t1 <= DoubleTime.ScaledTimeSinceLoad)
                    {
                        myState = State.Throw;
                        GameObject newBullet = Instantiate(
                                                bulletPrefabs[Fakerand.Int(0, bulletPrefabs.Length)], 
                                                transform.position + transform.right*shotPos.x + transform.up*shotPos.y, 
                                                Quaternion.AngleAxis(180f+transform.eulerAngles.z, Vector3.forward)
                                                );
                        aso.Stop();
                        aso.clip = releaseSound;
                        aso.Play();
                        an.Play("minister throw");
                    }
                    if (myState != State.Throw && (!aso.isPlaying || aso.clip != chargeSound))
                    {
                        aso.Stop();
                        aso.clip = chargeSound;
                        aso.Play();
                    }
                    break;
                case State.Throw:
                    smr.enabled = true;
                    teleDots.enabled = false;
                    chargeOrb.enabled = false;
                    AnimatorStateInfo a1 = an.GetCurrentAnimatorStateInfo(0);
                    if (a1.IsName("minister idle"))
                    {
                        myState = State.TeleportSoon;
                        t1 = DoubleTime.ScaledTimeSinceLoad + actionTimes[Fakerand.Int(0, actionTimes.Length)] * (1f / speedMult);
                    }
                    break;
                case State.TeleportSoon:
                    smr.enabled = true;
                    teleDots.enabled = false;
                    chargeOrb.enabled = false;
                    if (t1 <= DoubleTime.ScaledTimeSinceLoad)
                    {
                        myState = State.TeleportIn;
                        MakeTargets();
                        ti1 = 20;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
