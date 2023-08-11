using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGICycleMover : MonoBehaviour
{
    [SerializeField]
    private Transform cycleTransform;
    [SerializeField]
    private Renderer cycleRenderer;
    [SerializeField]
    private TrailRenderer trail;
    [SerializeField]
    private BulletHellMakerFunctions shooter;
    [SerializeField]
    private float distanceBetweenShots = 16f;
    [SerializeField]
    private int shotsHeadStart = 3;
    [SerializeField]
    private AudioClip stopSound;
    [SerializeField]
    private bool rotateGun;
    private Vector2 currDir;
    private GameObject myPlayer;
    private KHealth kh;
    private int khInitHits;
    private GameObject sprintParticles;
    private Encontrolmentation control;
    private Rigidbody2D r2;
    private float speed;
    private CGICycleEntry myEntry;
    private bool moving = true;

    private List<Vector3> firePositions;
    private float d = 0f;

    private HashSet<CGICycleLapTracker> lapTrackersCrossed = new HashSet<CGICycleLapTracker>();

    private AudioSource aud;

    private static HashSet<CGICycleMover> all = new HashSet<CGICycleMover>();

    private Quaternion GetTargetRotation()
    {
        return Quaternion.LookRotation(Vector3.back, Vector3.Cross(currDir, Vector3.back));
    }

    public void GetDataFromEntry(CGICycleEntry entry)
    {
        myEntry = entry;
        Color c = entry.GetCycleColor();
        cycleRenderer.materials[2].SetColor("_EmissionColor", c);
        currDir = entry.initDir;
        myPlayer = entry.myPlayer;
        sprintParticles = myPlayer.transform.Find("SprintParticles").gameObject;
        kh = myPlayer.GetComponent<KHealth>();
        kh.enabled = true;
        khInitHits = KHealth.hitsThisLevel;
        if (sprintParticles) { sprintParticles.SetActive(false); }
        control = myPlayer.GetComponent<Encontrolmentation>();
        cycleTransform.rotation = GetTargetRotation();
        speed = entry.GetSpeed();
        Gradient gr = new Gradient();
        gr.alphaKeys = new GradientAlphaKey[2]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
        };
        gr.colorKeys = new GradientColorKey[2]
        {
            new GradientColorKey(c, 0f),
            new GradientColorKey(c, 1f)
        };
        trail.colorGradient = gr;
        shooter.bulletData.color = c;

        ElectronTracker et = control.GetComponent<ElectronTracker>();
        if (et)
        {
            ElectronTracker et2 = gameObject.AddComponent<ElectronTracker>();
            et2.CopyFrom(et);
            Destroy(et);
        }
    }

    public static bool AtLeastOneExists()
    {
        return all.Count > 0;
    }

    private void Start()
    {
        all.Add(this);
        r2 = GetComponent<Rigidbody2D>();
        firePositions = new List<Vector3>();
        firePositions.Add(transform.position);
        aud = GetComponent<AudioSource>();
    }

    public void AddLapTracker(CGICycleLapTracker lt)
    {
        if (lapTrackersCrossed.Contains(lt)) { return; }
        lapTrackersCrossed.Add(lt);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    private void Stop()
    {
        if (!moving) { return; }
        moving = false;
        r2.velocity = Vector2.zero;
        myEntry.ReturnToInactivated();

        // reposition player to not get smushed, if possible
        GetComponent<Collider2D>().enabled = false;
        Vector3 r = myPlayer.transform.right;
        Vector3 u = myPlayer.transform.up;
        RaycastHit2D rminx = Physics2D.Raycast(transform.position, -r, 12f, 256 + 512 + 1048576);
        RaycastHit2D rmaxx = Physics2D.Raycast(transform.position, r, 12f, 256 + 512 + 1048576);
        RaycastHit2D rminy = Physics2D.Raycast(transform.position, -u, 28f, 256 + 512 + 1048576);
        RaycastHit2D rmaxy = Physics2D.Raycast(transform.position, u, 28f, 256 + 512 + 1048576);
        float dminx = rminx.collider ? rminx.distance : 12f;
        float dmaxx = rmaxx.collider ? rmaxx.distance : 12f;
        float dminy = rminy.collider ? rminy.distance : 28f;
        float dmaxy = rmaxy.collider ? rmaxy.distance : 28f;
        float avgx = 0.5f * (-dminx + dmaxx);
        float avgy = 0.5f * (-dminy + dmaxy);
        myPlayer.transform.position += 0.5f * avgx * r + avgy * u;

        myPlayer.transform.localScale = Vector3.one;
        StartCoroutine(LauncherEnemy.TrajectoryRestoreControl(currDir, myPlayer, speed));
        cycleRenderer.enabled = false;
        if (sprintParticles) { sprintParticles.SetActive(true); }
        Destroy(shooter.gameObject);
        Destroy(gameObject, Time.timeScale);

        aud.Stop();
        aud.pitch = 1.6f;
        aud.volume *= 3.3f;
        aud.clip = stopSound;
        aud.loop = false;
        aud.Play();

        foreach (CGICycleLapTracker lt in lapTrackersCrossed) { lt.RevertLaps(this); }
        lapTrackersCrossed.Clear();

        ElectronTracker et = GetComponent<ElectronTracker>();
        if (et)
        {
            ElectronTracker et2 = control.gameObject.AddComponent<ElectronTracker>();
            et2.CopyFrom(et);
            Destroy(et);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (Time.timeScale == 0) { return; }
        if (col.gameObject.layer == 11 && col.gameObject.GetComponent<beamBlock>())
        {
            beamBlock beam = col.gameObject.GetComponent<beamBlock>();
            Physics2D.IgnoreCollision(col.otherCollider, col.collider);
            Physics2D.IgnoreCollision(col.otherCollider, beam.trig);
        }
        if (((1 << col.gameObject.layer) & (256 + 512 + 1048576)) != 0)
        {
            Stop();
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (KHealth.hitsThisLevel != khInitHits) { Stop(); }
        if (control.ButtonDown(32UL, 32UL)) { Stop(); }
        if (!moving) { return; }
        BulletController.GetHelper().forceCollisionCheckingFrames = 3;
        myPlayer.transform.position = transform.position;
        cycleTransform.rotation = Quaternion.Lerp(cycleTransform.rotation, GetTargetRotation(), 0.35f);

        float neutral = control.AnalogDirection().magnitude <= 0.1f ? 0.7f : 1f;

        r2.velocity = (Vector3)currDir * speed * neutral;
        d += speed * neutral * Time.timeScale * 0.016666666f;
        while (d >= distanceBetweenShots)
        {
            firePositions.Add(transform.position);
            if (firePositions.Count >= shotsHeadStart)
            {
                shooter.transform.position = firePositions[0];
                if (rotateGun)
                {
                    Quaternion rgGoal = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.back, currDir));
                    shooter.transform.rotation = Quaternion.Lerp(shooter.transform.rotation, rgGoal, 0.15f);
                }
                shooter.Fire();
                firePositions.RemoveAt(0);
            }
            d -= distanceBetweenShots;
        }
        //FollowThePlayer.main.targetOffset = 64 * currDir;

        Vector2 newDir = control.AnalogDirection(0.382f); //sin 22.5 degrees
        float xd = Mathf.Abs(newDir.x) < 0.3f ? 0f : Mathf.Sign(newDir.x);
        float yd = Mathf.Abs(newDir.y) < 0.3f ? 0f : Mathf.Sign(newDir.y);
        newDir = new Vector2(xd, yd); // diagonal snap
        newDir = newDir.normalized;

        control.eventAbutton = Encontrolmentation.ActionButton.DPad;
        control.eventAName = "Drive";
        control.eventBbutton = Encontrolmentation.ActionButton.BButton;
        control.eventBName = "Eject";

        if (Vector2.Dot(newDir, currDir) > -0.8f && newDir.magnitude > 0.1f)
        {
            currDir = newDir;
        }

        aud.pitch = (Mathf.Log10(speed) * 0.5f) + (Mathf.Round(newDir.y * 2f) * 0.085f);
    }
}
