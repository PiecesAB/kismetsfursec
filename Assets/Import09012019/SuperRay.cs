using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Experimental.U2D;
//using UnityEngine.U2D;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SuperRay : MonoBehaviour
{

    private MeshFilter mf;
    private PolygonCollider2D pc2;
    //[HideInInspector]
    //public SpriteShapeRenderer ssr;
    [Header("Local position cursor")]
    public Vector2 cursor;
    public Vector2 cursorVelocity;
    public Vector2 cursorAccel;
    [Range(0f, 1f)]
    public float accelRetentionPerFrame;
    public List<Vector2> points;
    public List<float> thickness;
    public float currentThickness;
    public float changeCurrThickness;
    public float changeAllThickness;
    public float minimumThickness;
    public float DistanceBeforeNewPoint;
    public float simplifyDistance = 0.05f;
    public static List<SuperRay> allRays = new List<SuperRay>() { };
    public bool isAdditive;
    public Material subtractiveMat;
    public Material additiveMat;
    public bool deleteWhenOffscreen = true;
    public bool staticLaser = false;
    [HideInInspector]
    public bool dontMove;
    [HideInInspector]
    public Vector3 fakeRight = Vector3.right;

    public static GameObject shieldPrefab = null;
    public static GameObject bulletMeltCutout = null;
    private float currCutoutDistance = 0f;
    private const float placeNewCutoutDistance = 16f;
    public Transform myShieldTransform;
    public ShieldingCircle myShieldSC;

    [HideInInspector]
    public SuperRay clonedFrom = null;
    private List<SuperRay> myClones = new List<SuperRay>();

    private bool wasEverVisible = false;

    private const float shieldRadiusMultiplier = 64f/12f;

    void Start()
    {
        if (shieldPrefab == null)
        {
            shieldPrefab = Resources.Load<GameObject>("ShieldingCircle");
        }
        
        if (BulletMeltZone.main != null)
        {
            if (bulletMeltCutout == null)
            {
                bulletMeltCutout = Resources.Load<GameObject>("BulletMeltCutout");
            }
            currCutoutDistance = placeNewCutoutDistance;
        }

        allRays.Add(this);
        if (!clonedFrom && !staticLaser)
        {
            points.Clear();
            points.Add(cursor);
            thickness.Add(currentThickness);
            points.Add(cursor + (cursorVelocity * Time.deltaTime));
            thickness.Add(currentThickness);
        }
        MakeMode();
        dontMove = false;
        mf = GetComponent<MeshFilter>();
        pc2 = GetComponent<PolygonCollider2D>();
        //ssr = GetComponent<SpriteShapeRenderer>();

        GameObject newShield = Instantiate(shieldPrefab, new Vector3(cursor.x + transform.position.x, cursor.y + transform.position.y, 4f), Quaternion.identity);
        myShieldTransform = newShield.transform;
        myShieldSC = newShield.GetComponent<ShieldingCircle>();
        myShieldSC.radius = 0;

        // accelerate motion
        if (!clonedFrom)
        {
            for (int i = 0; i < Mathf.Min(5 / Time.timeScale, 10); ++i)
            {
                MovementUpdate();
            }
        }
        VisualAndColliderUpdate();
    }

    private void OnDestroy()
    {
        allRays.Remove(this);
        if (myShieldSC) { Destroy(myShieldSC.gameObject); }
        foreach (SuperRay c in myClones)
        {
            c.clonedFrom = null;
        }
    }

    public void DontPassImperviousBlock()
    {
        DontPassImperviousBlock(null, true);
    }

    public void DontPassImperviousBlock(Collider2D c, bool fromOther = false)
    {
        if (dontMove) { return; }
        if (fromOther)
        {
            cursorVelocity = cursorVelocity.normalized * 0.001f;
            dontMove = true;
        }
        else
        {
            SpriteRenderer sr = c.GetComponent<SpriteRenderer>();
            ElectricZone ez = c.GetComponent<ElectricZone>();
            primExtraTags pet = c.GetComponent<primExtraTags>();
            //if (sr == null) { return; }
            if ((pet != null && pet.tags.Contains("ImperviousBlock")) 
                || (sr != null && (sr.sprite?.name.StartsWith(Utilities.imperviousBlockName) ?? false)) 
                || (ez != null && ez.mode == ElectricZone.Mode.Remove))
            {
                Vector2 curGlobalPos = cursor + (Vector2)transform.position;
                Vector2 closestDif = (Vector2)c.bounds.ClosestPoint(curGlobalPos) - curGlobalPos;
                // if it's inside the block, or the direction it would need to move to get closer to the block correlates to the direction it's going, stop
                if (closestDif.magnitude < 1f || Vector2.Dot(closestDif.normalized, cursorVelocity.normalized) > 0.001f)
                {
                    cursorVelocity = cursorVelocity.normalized * 0.001f;
                    dontMove = true;
                }
            }
        }
    }

    private void MakePlayerPhase(Collider2D c)
    {
        //if (dontMove) { return; }

        BasicMove bm = c.gameObject.GetComponent<BasicMove>();
        PrimPlayableCharacter ppc = c.gameObject.GetComponent<PrimPlayableCharacter>();
        if (bm && ppc)
        {
            //print("s");
            //print(bm.CanCollide);
            bm.TurnOffCollision();

        }
    }

    private void AssurePlayable(Collider2D c, Collider2D other)
    {
        PrimPlayableCharacter ppc = c.gameObject.GetComponent<PrimPlayableCharacter>();
        if (!ppc)
        {
            Physics2D.IgnoreCollision(c, other, true);
        }
    }

    private void PlaceNewCutout()
    {
        GameObject newCutout = Instantiate(bulletMeltCutout, (Vector2)transform.position + cursor, Quaternion.identity, null);
        Vector2 v = Vector2.one * 2f * shieldRadiusMultiplier * currentThickness;
        foreach (SpriteRenderer s in newCutout.GetComponentsInChildren<SpriteRenderer>())
        {
            s.size = v;
        }
        newCutout.GetComponentInChildren<StaticBulletsOnVertices>().bulletData.scale = v;
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        DontPassImperviousBlock(c);
        MakePlayerPhase(c);
    }

    private void OnTriggerStay2D(Collider2D c)
    {
        MakePlayerPhase(c);
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        DontPassImperviousBlock(c.collider);
        AssurePlayable(c.collider, c.otherCollider);
        if (c.gameObject.layer == 20
            && ((Vector2)transform.TransformPoint(cursor) - c.GetContact(0).point).magnitude < 32f
            && Vector2.Dot(c.GetContact(0).normal, cursorVelocity.normalized) < -0.8f
            && Vector2.Dot(((Vector2)(c.transform.position) - (cursor + (Vector2)transform.position)).normalized, cursorVelocity.normalized) >= -0.0001f)
        {
            cursorVelocity = cursorVelocity.normalized * 0.001f;
            dontMove = true;
            c.gameObject.transform.position += (Vector3)cursorVelocity * 0.016666666f * Time.timeScale;
        }
    }

    void MakeMode()
    {
        //additive or subtractive?
        float dot = Vector2.Dot(cursorVelocity.normalized, fakeRight);
        Vector2 fakeUp = Vector3.Cross(fakeRight, Vector3.back);
        if (System.Math.Abs(dot) <= 0.01f) //straight up or down
        {
            if (Vector2.Dot(cursorVelocity, fakeUp) > 0f) //up
            {
                isAdditive = true;
            }
            else //down
            {
                isAdditive = false;
            }
        }
        else if (dot > 0f) //right-ish
        {
            isAdditive = true;
        }
        else //left-ish
        {
            isAdditive = false;
        }

        PolygonCollider2D pc2 = GetComponent<PolygonCollider2D>();

        pc2.isTrigger = !isAdditive;
        gameObject.layer = isAdditive ? 8 : 23;

        GetComponent<MeshRenderer>().material = isAdditive ? additiveMat : subtractiveMat;
    }

    private void MovementUpdate()
    {
        //new element
        if (points.Count > 1 && ((cursor - points[points.Count - 2]).magnitude >= DistanceBeforeNewPoint) && !staticLaser)
        {
            points.Add(cursor);
            thickness.Add(currentThickness);
        }

        if (BulletMeltZone.main)
        {
            currCutoutDistance += cursorVelocity.magnitude * Time.deltaTime;
            float mod = currCutoutDistance % placeNewCutoutDistance;
            if (currCutoutDistance != mod)
            {
                PlaceNewCutout();
            }

            currCutoutDistance = mod;
        }

        //move cursor
        if (currentThickness >= minimumThickness)
        {
            if (!staticLaser)
            {
                cursor += cursorVelocity * Time.deltaTime;
                cursorVelocity += cursorAccel * Time.deltaTime;
                cursorAccel *= accelRetentionPerFrame;
            }
        }
        else
        {
            Destroy(gameObject);
        }

        //last position is always current
        if (points.Count > 0 && !staticLaser)
        {
            points[points.Count - 1] = cursor;
            thickness[thickness.Count - 1] = currentThickness;
        }
        
    }

    private void VisualAndColliderUpdate()
    {
        //render new path
        mf.mesh.Clear();
        List<Vector3> verts = new List<Vector3>();

        Vector2 grad = Vector2.right;
        Vector2 norm = Vector2.up;
        int j = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (i + 1 < points.Count)
            {
                grad = (points[i + 1] - points[i]).normalized;
                norm = new Vector2(-grad.y, grad.x);
            }
            Vector2 npt1 = points[i] - (norm * thickness[i]);
            Vector2 npt2 = points[i] + (norm * thickness[i]);
            if (((i >= 1 && (npt1 - (points[i - 1] - (norm * thickness[i - 1]))).magnitude >= simplifyDistance) || verts.Count < 2) && thickness[i] >= minimumThickness)
            {
                verts.Insert(i + j, npt1);
            }
            if (((i >= 1 && (npt2 - (points[i - 1] + (norm * thickness[i - 1]))).magnitude >= simplifyDistance) || verts.Count < 2) && thickness[i] >= minimumThickness)
            {
                verts.Insert(i + j, npt2);
            }
            else
            {
                j--;
            }
        }

        //generate the mesh and collider
        Vector2[] pts2 = new Vector2[verts.Count];
        for (int i = 0; i < verts.Count; ++i) { pts2[i] = verts[i]; }
        pc2.points = pts2;
        mf.mesh = pc2.CreateMesh(false, true);
        mf.mesh.RecalculateBounds();
        Vector3[] internalVerts = mf.mesh.vertices;
        Vector2[] uvs = new Vector2[internalVerts.Length];
        for (int i = 0; i < uvs.Length; ++i) { uvs[i] = internalVerts[i] * 0.5f; }
        mf.mesh.uv = uvs;

        //thinner
        for (int i = 0; i < thickness.Count; i++)
        {
            thickness[i] += changeAllThickness * 0.01666666f * Time.timeScale;
        }
        currentThickness += changeCurrThickness * 0.01666666f * Time.timeScale;
        //delete all thickness zeroes at the beginning
        while (thickness.Count > 0 && thickness[0] < minimumThickness)
        {
            points.RemoveAt(0);
            thickness.RemoveAt(0);
        }

        MakeMode();

        if (myShieldSC)
        {
            // shield
            if (dontMove)
            {
                myShieldSC.radius = Mathf.Max(myShieldSC.radius - 0.5f * Time.timeScale, 0f);
                if (myShieldSC.radius == 0f) { Destroy(myShieldSC.gameObject); }
            }
            else
            {
                myShieldSC.radius = Mathf.Lerp(myShieldSC.radius, shieldRadiusMultiplier * currentThickness, 0.3f);
            }
            myShieldTransform.position = new Vector3(cursor.x + transform.position.x, cursor.y + transform.position.y, myShieldTransform.position.z);
        }
    }

    public void Clone(Vector3 offset)
    {
        Clone(offset, Vector2.one);
    }

    public void Clone(Vector3 offset, Vector2 dirScale)
    {
        SuperRay oc = clonedFrom;
        clonedFrom = this;
        wasEverVisible = false;
        GameObject c = Instantiate(gameObject, transform.position + offset, transform.rotation, transform.parent);
        SuperRay cs = c.GetComponent<SuperRay>();
        myClones.Add(cs);
        cs.cursorVelocity *= dirScale;
        cs.cursorAccel *= dirScale;
        clonedFrom = oc;
        wasEverVisible = true;
    }

    FollowThePlayer ftp = null;
    SpecialWrapping spw = null;

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (dontMove) { cursorVelocity = cursorVelocity.normalized * 0.001f; }

        bool vis = GetComponent<Renderer>().isVisible;
        if (vis && !wasEverVisible) { wasEverVisible = true; }
        if (!vis && (!clonedFrom || wasEverVisible) && (!myShieldSC || !myShieldSC.GetComponent<Renderer>().isVisible)) {
            if (deleteWhenOffscreen)
            {
                Destroy(gameObject);
            }
        }
        MovementUpdate();
        VisualAndColliderUpdate();

        // wrapping
        if (!ftp) { ftp = FollowThePlayer.main; }
        if (ftp)
        {
            Vector3 fpos = ftp.transform.position;
            Vector3 rpos = (Vector3)cursor + transform.position - fpos;
            if (!spw) { spw = ftp.GetComponent<SpecialWrapping>(); }
            if (spw && spw.enabled && myClones.Count == 0 && wasEverVisible)
            {
                if (spw.horizontal && Mathf.Abs(rpos.x) >= 152f && cursorVelocity.x * Mathf.Sign(rpos.x) > 0f)
                {
                    Vector3 tx = Vector3.right * 320f * -Mathf.Sign(rpos.x);
                    if (spw.horizontalTwist) { Clone(tx - Vector3.up * 2f * rpos.y, new Vector2(1, -1)); }
                    else { Clone(tx); }
                }

                if (spw.vertical && Mathf.Abs(rpos.y) >= 100f && cursorVelocity.y * Mathf.Sign(rpos.y) > 0f)
                {
                    Vector3 ty = Vector3.up * 216f * -Mathf.Sign(rpos.y);
                    if (spw.verticalTwist) { Clone(ty - Vector3.right * 2f * rpos.x, new Vector2(-1, 1)); }
                    else { Clone(ty); }
                }
            }
        }
    }
}
