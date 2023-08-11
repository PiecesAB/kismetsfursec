using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IlMattoObstacle : GenericBlowMeUp
{
    public enum State
    {
        Scan, Chase, GoHome
    }

    public State state;
    public LineRenderer scanLine;
    [SerializeField]
    private Renderer visCheck;
    [SerializeField]
    private bool scanWhenOffscreen = false;
    [SerializeField]
    private float scanVelocity; // degrees
    [SerializeField]
    private bool useFixedStartScan = false;
    [SerializeField]
    private float fixedStartScan = 0f;
    [SerializeField]
    private GameObject chaseEyesBack;
    [SerializeField]
    private GameObject chaseEyesFront;
    
    [SerializeField]
    private float chaseMaxVelocity = 300f;
    [SerializeField]
    private float chaseAcceleration = 300f;
    [SerializeField]
    private float angleCorrection = 180f;
    [SerializeField]
    private float goHomeSpeed = 120f;
    [SerializeField]
    private Transform spikeSpinner;
    [SerializeField]
    private MeshRenderer mask;

    private float scanCurrAngle; // degrees
    private float chaseCurrAngle; // they're all degrees
    private float chaseCurrVelocity;

    private Encontrolmentation currPlr;
    private float plrLastRelAngle; // difference of 180 or more in one frame means the player crossed the beam.

    private Vector3 homePos;

    void Start()
    {
        state = State.Scan;
        homePos = transform.position;
        chaseEyesBack.SetActive(false);
        chaseEyesFront.SetActive(false);
        spikeSpinner.localScale = Vector3.zero;
        GetComponent<Collider2D>().enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (state != State.Chase) { return; }
        if (col.gameObject.layer == 11) { return; }
        GetComponent<Collider2D>().enabled = false;
        FollowThePlayer ftp = FollowThePlayer.main;
        ftp.vibSpeed = Mathf.Min(4.5f, ftp.vibSpeed + 3f);
        chaseEyesBack.SetActive(false);
        chaseEyesFront.SetActive(false);
        spikeSpinner.localScale = Vector3.zero;
        scanLine.positionCount = 0;
        state = State.GoHome;
    }

    private void SignalAllEnemiesOnScreen()
    {
        foreach (PrimEnemyHealth h in LevelInfoContainer.allEnemiesInLevel)
        {
            bool vis = false;
            if (h.rendererForDetectingTetraLasers) { vis = h.rendererForDetectingTetraLasers.isVisible; }
            else { vis = FollowThePlayer.main.PointIsOnScreen(h.mainObject.transform.position); }
            if (!vis) { continue; }
            BulletHellMakerFunctions[] bl = h.mainObject.GetComponentsInChildren<BulletHellMakerFunctions>();
            foreach (BulletHellMakerFunctions b in bl)
            {
                if (!b.shooting) { b.StartCoroutine(b.Test()); }
            }
        }
    }

    void Update()
    {
        if (Time.timeScale == 0 || (!visCheck.isVisible && state == State.Scan) && !scanWhenOffscreen) {
            scanLine.positionCount = 0;
            return;
        }

        if (Door1.levelComplete && (state == State.Chase || (visCheck.isVisible && state == State.Scan)))
        {
            BlowMeUp();
        }

        Encontrolmentation plr = LevelInfoContainer.GetActiveControl();
        if (!plr) {
            scanLine.positionCount = 0;
            return;
        }
        Vector2 plrDif = plr.transform.position - transform.position;

        switch (state)
        {
            case State.Scan:
                if (plr != currPlr)
                {
                    currPlr = plr;
                    if (useFixedStartScan)
                    {
                        scanCurrAngle = Mathf.Repeat(fixedStartScan, 360f);
                        plrLastRelAngle = Mathf.Rad2Deg * Mathf.Atan2(plrDif.y, plrDif.x) - scanCurrAngle;
                        plrLastRelAngle = Mathf.Repeat(plrLastRelAngle, 360f);
                    }
                    else //opposite of player
                    {
                        plrLastRelAngle = 180;
                        scanCurrAngle = Mathf.Rad2Deg * Mathf.Atan2(plrDif.y, plrDif.x) + 180;
                        scanCurrAngle = Mathf.Repeat(scanCurrAngle, 360f);
                    }
                }

                if (plr && visCheck.isVisible)
                {
                    float t = scanCurrAngle * Mathf.Deg2Rad;
                    Vector2 scanDir = new Vector2(Mathf.Cos(t), Mathf.Sin(t));
                    RaycastHit2D rh2 = Physics2D.Raycast(transform.position, scanDir, 400f, 256 + 512 + 1048576);
                    scanLine.positionCount = 2;
                    scanLine.SetPosition(0, transform.position);
                    scanLine.SetPosition(1, rh2.collider ? (Vector3)rh2.point : (400f * ((Vector3)scanDir) + transform.position));

                    // solve relative angle
                
                    float plrCurrRelAngle = Mathf.Rad2Deg * Mathf.Atan2(plrDif.y, plrDif.x);
                    while (plrCurrRelAngle < scanCurrAngle) { plrCurrRelAngle += 360f; }
                    plrCurrRelAngle -= scanCurrAngle;
                    bool rayHitPlr = (rh2.collider && rh2.collider.gameObject == plr.gameObject) 
                        || (rh2.distance >= plrDif.magnitude && rh2.distance < plrDif.magnitude + 20f);
                    float angleRange = Mathf.Min(360f, 360f / plrDif.magnitude);
                    if ((Mathf.Abs(plrCurrRelAngle - plrLastRelAngle) >= 180f || plrCurrRelAngle >= 360f - angleRange || plrCurrRelAngle <= angleRange) 
                        && rayHitPlr
                        && plrDif.magnitude > 16f)
                    {
                        state = State.Chase;
                        chaseEyesBack.SetActive(true);
                        chaseEyesFront.SetActive(true);
                        chaseCurrAngle = Mathf.Rad2Deg * Mathf.Atan2(plrDif.y, plrDif.x);
                        chaseCurrVelocity = 0;
                        GetComponent<Collider2D>().enabled = true;
                        SignalAllEnemiesOnScreen();
                        break;
                    }
                    plrLastRelAngle = plrCurrRelAngle;
                }
                else
                {
                    scanLine.positionCount = 0;
                }

                scanCurrAngle += scanVelocity * 0.016666666f * Time.timeScale;
                scanCurrAngle = Mathf.Repeat(scanCurrAngle, 360f);
                break;
            case State.Chase:
                chaseEyesFront.transform.position = transform.position + 3f * (Vector3)plrDif.normalized - Vector3.forward;
                spikeSpinner.localScale = Vector3.Lerp(spikeSpinner.localScale, Vector3.one, 0.3f);

                float tb = chaseCurrAngle * Mathf.Deg2Rad;
                transform.position += chaseCurrVelocity * (new Vector3(Mathf.Cos(tb), Mathf.Sin(tb))) * 0.016666666f * Time.timeScale;

                float plrCurrAngle = Mathf.Rad2Deg * Mathf.Atan2(plrDif.y, plrDif.x);
                chaseCurrAngle = Mathf.MoveTowardsAngle(chaseCurrAngle, plrCurrAngle, angleCorrection * Time.timeScale * 0.016666666f);
                chaseCurrVelocity += chaseAcceleration * 0.01666666f * Time.timeScale;
                if (chaseCurrVelocity > chaseMaxVelocity) { chaseCurrVelocity = chaseMaxVelocity; }

                tb = chaseCurrAngle * Mathf.Deg2Rad;
                Vector2 scanDirb = new Vector2(Mathf.Cos(tb), Mathf.Sin(tb));
                GetComponent<Collider2D>().enabled = false;
                RaycastHit2D rh2b = Physics2D.Raycast(transform.position, scanDirb, 400f, 256 + 512 + 1048576);
                GetComponent<Collider2D>().enabled = true;
                scanLine.positionCount = 2;
                scanLine.SetPosition(0, transform.position);
                scanLine.SetPosition(1, rh2b.collider ? (Vector3)rh2b.point : (400f * ((Vector3)scanDirb) + transform.position));
                if (DoubleTime.ScaledTimeSinceLoad % 0.2 >= 0.1) { mask.materials[3].color = Color.black; }
                else { mask.materials[3].color = Color.red; }
                break;
            case State.GoHome:
                transform.position = Vector3.MoveTowards(transform.position, homePos, goHomeSpeed * 0.016666666f * Time.timeScale);
                scanLine.positionCount = 2;
                scanLine.SetPosition(0, transform.position);
                scanLine.SetPosition(1, homePos);
                if ((transform.position - homePos).magnitude < 1f)
                {
                    transform.position = homePos;
                    currPlr = null;
                    state = State.Scan;
                }
                break;
        }
    }
}
