using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformControlButtonMain : MonoBehaviour
{
    [Header("Make sure the collider is slightly smaller than the desired widths")]
    [SerializeField]
    public PlatformControlButton pcb;
    private PlatformControlButton[] myButtons;
    [HideInInspector]
    public primDecorationMoving dm;
    public float upToSpeedTime = 0.25f;
    public AudioClip buttonBeep;
    public AudioClip stopSound;
    public AudioSource beepSource;
    public AudioSource whirSource;
    public float startDelay = 0f;
    public SpriteRenderer[] mainSprites; // only need to set this if the platform has delayed movement
    public SpriteRenderer boxSpriteSample;
    public bool clipCorrection = true;
    private SpriteRenderer[] boxSprites;

    private float currStartDelay = 0f;

    private Dictionary<GameObject, int> plrTouches = new Dictionary<GameObject, int>();
    private HashSet<GameObject> plrObjects = new HashSet<GameObject>();

    private double lastChangeTime;

    private int hasCorrected = 0;

    private Vector2 lastNormal;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == 20)
        {
            if (!plrTouches.ContainsKey(col.gameObject))
            {
                plrTouches.Add(col.gameObject, 1);
                plrObjects.Add(col.gameObject);
            }
            else { ++plrTouches[col.gameObject]; }
            return;
        }
        OnTouch(col);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        OnTouch(col);
    }

    private bool AnyChildVisible()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.isVisible) { return true; }
        }
        return false;
    }

    private void OnTouch(Collision2D col)
    {
        if (!pcb) { return; }
        if (col.collider.isTrigger) { return; }
        if (col.gameObject.layer != 8 && col.gameObject.layer != 9 && col.gameObject.layer != 11) { return; }
        if (col.rigidbody && !col.rigidbody.isKinematic && !col.rigidbody.GetComponent<PlatformControlButtonMain>()) { return; }
        if (col.gameObject.tag == "SuperRay") { Physics2D.IgnoreCollision(col.collider, col.otherCollider, true); return; }
        if (col.gameObject.GetComponent<beeDrone>()) { Physics2D.IgnoreCollision(col.collider, col.otherCollider, true); return; }

        for (int i = 0; i < col.contactCount; ++i)
        {
            if (Vector2.Dot(col.GetContact(i).normal, dm.v) >= 0) { continue; }
            if (clipCorrection)
            {
                MoveToRoundedPosition(col.GetContact(i));
                if (hasCorrected == 0)
                {
                    hasCorrected = 30;
                    return;
                }
            }
            ActivateSwitch(null);

            bool anyChildVisible = AnyChildVisible();
            if (anyChildVisible) { FollowThePlayer.main.vibSpeed = 2f; }
            whirSource.Stop();
            beepSource.Stop();
            beepSource.clip = stopSound;
            if (anyChildVisible) { beepSource.Play(); }
            return;
        }
    }
    
    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.layer == 20) { --plrTouches[col.gameObject]; }
    }

    // assume the platform is composed of only axis-aligned rectangles.
    public bool CanIMoveThisDirection(Vector2 dir)
    {
        if (dir.x != 0 && dir.y != 0)
        {
            return CanIMoveThisDirection(new Vector2(dir.x, 0)) && CanIMoveThisDirection(new Vector2(0, dir.y));
        }
        dir = dir.normalized;
        Vector2 absDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

        // now there's only (1,0), (-1,0), (0,1), (0,-1)
        // find the colliders to the maximum extent of this direction.
        // i.e. find the colliders that make up this platform's right side in the (1,0) case.

        float maxSide = -1e9f;
        List<BoxCollider2D> maxCols = new List<BoxCollider2D>();
        foreach (BoxCollider2D bc in GetComponentsInChildren<BoxCollider2D>())
        {
            if (bc.isTrigger || bc.gameObject.layer != 8) { continue; }
            Vector2 td = (Vector2)bc.transform.position + bc.offset + Vector2.Dot(bc.size * 0.5f, absDir) * dir;
            float mySide = Vector2.Dot(td, dir);
            if (mySide > maxSide + 1f)
            {
                maxCols.Clear();
                maxSide = mySide;
                maxCols.Add(bc);
            }
            else if (mySide > maxSide - 1f)
            {
                maxCols.Add(bc);
            }
        }

        // boxcast from those platforms and make sure there's nothing but this object.

        Vector2 dirPerp = Vector3.Cross(Vector3.forward, dir);
        foreach (BoxCollider2D bc in maxCols)
        {
            Vector2 td = (Vector2)bc.transform.position + bc.offset + Vector2.Dot(bc.size * 0.5f, absDir) * dir;
            Vector2 p = Vector2.Dot(bc.size * 0.98f, dirPerp) * dirPerp;
            RaycastHit2D[] rhs = Physics2D.BoxCastAll(td,
                new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) + absDir,
                0f,
                dir,
                2f,
                256 + 512 + 2048
            );
            foreach (RaycastHit2D rh in rhs)
            {
                if (rh.transform != transform && rh.transform.parent != transform && rh.transform.GetComponent<beeDrone> () == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void ActivateSwitch(PlatformControlButton newPcb)
    {
        pcb = newPcb;
        hasCorrected = 0;
        dm.v = Vector3.zero;
        lastChangeTime = DoubleTime.ScaledTimeSinceLoad + startDelay;
        for (int i = 0; i < myButtons.Length; ++i)
        {
            if (myButtons[i] != pcb) { myButtons[i].Off(); }
        }
        whirSource.Stop();
        whirSource.pitch = 0;
        whirSource.Play();
        if (pcb != null)
        {
            beepSource.Stop();
            beepSource.clip = buttonBeep;
            beepSource.Play();
        }
    }

    private void MoveToRoundedPosition(ContactPoint2D pt)
    {
        Vector3 currPos = transform.position - (Vector3)(pt.normal * pt.separation);
        Vector3 newPos = new Vector3(Mathf.Round(currPos.x / 8f) * 8f, Mathf.Round(currPos.y / 8f) * 8f, currPos.z);
        Vector2 dif = newPos - transform.position;
        dm.SetPosition(newPos);
        foreach (GameObject plr in plrObjects)
        {
            if (plrTouches.ContainsKey(plr) && plrTouches[plr] > 0)
            {
                plr.transform.position += (Vector3)dif;
                plr.GetComponent<Rigidbody2D>().MovePosition(plr.transform.position);
                //plr.GetComponent<BasicMove>().extraPerFrameVel += dif / Time.deltaTime;
            }
        }
        dm.SetLateVelocitation(pcb.velocity);
        dm.v = Vector3.zero;
        lastChangeTime = DoubleTime.ScaledTimeSinceLoad;
    }

    private void InitSprings()
    {
        if (!boxSpriteSample) { return; } // assume springs can only be on 1-direction platforms
        PlatformControlButton pcbSurrogate = GetComponentInChildren<PlatformControlButton>(); 
        foreach (SpringBhvr s in GetComponentsInChildren<SpringBhvr>())
        {
            s.pcbSurrogate = pcbSurrogate;
        }
    }

    void Start()
    {
        myButtons = GetComponentsInChildren<PlatformControlButton>();
        dm = GetComponent<primDecorationMoving>();
        dm.constantUseRigidbodyPhysics = true;
        Rigidbody2D r2 = GetComponent<Rigidbody2D>();
        r2.isKinematic = false;
        r2.freezeRotation = true;
        lastChangeTime = DoubleTime.ScaledTimeSinceLoad;
        InitSprings();
    }

    private bool vibrating = false;
    private Vector2 vibOffset = Vector2.zero;

    private float BoxGrabCurve(float x)
    {
        return 32f * Mathf.Max(0f, 1f - 3f * x * x * x);
    }

    private void Vibrate()
    {
        if (lastChangeTime > DoubleTime.ScaledTimeSinceLoad)
        {
            vibrating = true;
            Vector2 oldVibOffset = vibOffset;
            vibOffset = Fakerand.UnitCircle();
            
            if (boxSprites == null || boxSprites.Length != mainSprites.Length)
            {
                boxSprites = new SpriteRenderer[mainSprites.Length];
                for (int i = 0; i < mainSprites.Length; ++i)
                {
                    GameObject ng = Instantiate(boxSpriteSample.gameObject, mainSprites[i].transform);
                    ng.SetActive(true);
                    boxSprites[i] = ng.GetComponent<SpriteRenderer>();
                }
            }
            int j = 0;
            float rat = (float)(lastChangeTime - DoubleTime.ScaledTimeSinceLoad) / startDelay;
            foreach (SpriteRenderer s in mainSprites)
            {
                s.transform.localPosition -= (Vector3)oldVibOffset;
                // rounding error shouldn't be a problem
                s.transform.localPosition += (Vector3)vibOffset;
                boxSprites[j].size = boxSprites[j].transform.parent.GetComponent<SpriteRenderer>().size + BoxGrabCurve(1f - rat) * Vector2.one;
                ++j;
            }
        }
        else if (vibrating)
        {
            vibrating = false;
            if (boxSprites != null)
            {
                for (int i = 0; i < boxSprites.Length; ++i)
                {
                    Destroy(boxSprites[i].gameObject);
                }
                boxSprites = null;
            }
            foreach (SpriteRenderer s in mainSprites)
            {
                s.transform.localPosition -= (Vector3)vibOffset;
            }
            vibOffset = Vector2.zero;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0 || !pcb) { return; }

        Vibrate();

        float t = Mathf.Max(0f, (float)(DoubleTime.ScaledTimeSinceLoad - lastChangeTime));
        if (t >= upToSpeedTime)
        {
            dm.v = pcb.velocity;
        }
        else
        {
            dm.v = pcb.velocity * (t / upToSpeedTime);
        }
        --hasCorrected;
        if (hasCorrected <= 0) { hasCorrected = 0; }
        whirSource.pitch = dm.v.magnitude / 90f;
    }
}
