using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//update functions have moved to BeamBlockHelper

public class beamBlock : MonoBehaviour {

    [HideInInspector]
    public Collider2D trig;
    private float j;
    private static Collider2D[] inCols = new Collider2D[64];
    private BoxCollider2D thisCol;

    public bool horizontalWrap = false;
    public Renderer visibilityCheck = null;
    public bool getVisibilityCheckInParent = false;
    public bool playerAndVisibilityOnly = false;
    public float offsetCorrection = 0.5f;
    public bool checkIncludesVelocity = false;

    private static Transform mainCamTransform;

    [SerializeField]
    private bool globalRotationLocked = false;
    [SerializeField]
    private bool ignoreAllButPlayer = false;
    [SerializeField]
    private bool ignorePlayer = false;

    void Start () {
        thisCol = GetComponent<BoxCollider2D>();
        trig = GetComponent<CircleCollider2D>();
        if (!trig)
        {
            trig = GetComponent<CapsuleCollider2D>();
        }
        if (!trig)
        {
            trig = GetComponent<PolygonCollider2D>();
        }

        if (globalRotationLocked)
        {
            BeamBlockHelper newHelper = gameObject.AddComponent<BeamBlockHelper>();
            newHelper.globalRotationLocked = true;
            GetComponent<SpriteRenderer>().color = Color.cyan;
        }

        Transform ct = transform;
        if (getVisibilityCheckInParent)
        {
            while (ct.parent)
            {
                visibilityCheck = ct.GetComponent<Renderer>();
                if (visibilityCheck) { break; }
                ct = ct.parent;
            }
        }

        if (visibilityCheck && playerAndVisibilityOnly)
        {
            Destroy(trig);
            StartCoroutine(PlayerAndVisibilityUpdate());
        }
    }

    private IEnumerator PlayerAndVisibilityUpdate()
    {
        while (true)
        {
            if (visibilityCheck && !visibilityCheck.isVisible) { yield return new WaitForEndOfFrame(); continue; }
            foreach (GameObject g in LevelInfoContainer.allPlayersInLevel)
            {
                foreach (Collider2D c in g.GetComponents<Collider2D>())
                {
                    Check(c);
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Check(col);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        Check(col);
    }

    private void Check(Collider2D col)
    {
        if (!thisCol) { return; }
        if (visibilityCheck && !visibilityCheck.isVisible) { return; }
        if (ignoreAllButPlayer && col.gameObject.layer != 20) { Physics2D.IgnoreCollision(col, trig); Physics2D.IgnoreCollision(col, thisCol); return; }
        if (ignorePlayer && col.gameObject.layer == 20) { Physics2D.IgnoreCollision(col, trig); Physics2D.IgnoreCollision(col, thisCol); return; }
        if (trig && col is EdgeCollider2D) { Physics2D.IgnoreCollision(col, trig); return; }
        if (trig && col.gameObject.name.StartsWith("KhalSuperRay")) { Physics2D.IgnoreCollision(col, trig); return; }

        Rigidbody2D r = col.attachedRigidbody;
        if (trig && (r == null || r.isKinematic)) { Physics2D.IgnoreCollision(col, trig); return; }

        if (gameObject.layer != 11 && col.gameObject.tag == "Player" && !col.gameObject.GetComponent<BasicMove>().CanCollide) // tacks
        {
            foreach (Collider2D cc in col.GetComponents<Collider2D>())
            {
                Physics2D.IgnoreCollision(cc, thisCol, true);
            }
            return;
        }

        if (col.gameObject.tag == "Player" && col is BoxCollider2D && ((BoxCollider2D)col).size.y < 8f) // cushion between players standing on each other
        {
            Physics2D.IgnoreCollision(col, thisCol, true);
            return;
        }

        Vector2 newOrigin = transform.position + (transform.up * ((thisCol.size.y * 0.5f) - offsetCorrection));
        Vector2 cp = (Vector2)col.bounds.ClosestPoint(transform.position) - newOrigin;

        if (Vector2.Dot(cp, transform.up) > 0f && (!checkIncludesVelocity || Vector2.Dot(transform.up, r.velocity.normalized) <= 0.01f))
        {
            foreach (Collider2D cc in col.GetComponents<Collider2D>())
            {
                Physics2D.IgnoreCollision(cc, thisCol, false);
            }
        }
        else 
        {
            if (col.gameObject.tag == "Player" && col.GetComponent<BasicMove>().IsPunchingOrKicking()) // avoid phasing through beam by punching
            {
                return;
            }

            foreach (Collider2D cc in col.GetComponents<Collider2D>())
            {
                Physics2D.IgnoreCollision(cc, thisCol, true);
            }
        }
    }

    float sign(float a)
    {
        if (a > 0.0001f)
        {
            return 1f;
        }
        else if (a < -0.0001f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }
}
