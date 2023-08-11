using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class primDecorationMoving : MonoBehaviour, IAmbushController
{
    public enum MvtType
    {
        CircleWithNoise, Constant, SuddenJerk
    }

    public MvtType mvtType;
    public Vector4 rangeForSomeTypes;
    public double startDelay;
    public double jerkDuration;
    public EasingOfAccess.EasingType jerkEasingType;
    public Vector3 r; //for radius (radii)??
    public Vector3 v; //for velocity??
    public Vector3 angularOffset;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 noiseIntensity; //wow a descriptive name
    public Vector3 noiseVelocity; // ...
    private Vector3 originalPos;
    public Vector2 minPosition = new Vector2(-1e6f,-1e6f);
    public Vector2 maxPosition = new Vector2(1e6f, 1e6f);
    public Renderer onlyUpdateOnVisible = null;
    public bool autoReflectOnBounds = false;
    public bool wrapOnBounds = false;
    public bool rigidbodyPositionUpdate = true;
    public bool constantUseRigidbodyPhysics = false;
    public bool randomizeVelocityDirection = false;

    public int rigidbodyReflect = 0;
    public float rigidbodyReflectElasticity = 1f;

    public bool ignorePlayerCollision = false;

    private bool rbReflectThisFrame = false;

    public float radiusToUpdateRigidbody = 480f;
    public float radiusToUpdateAtAll = Mathf.Infinity;

    public string makeParentOfThisTag = "";

    private List<Rigidbody2D> velMaker = new List<Rigidbody2D>();

    private double existTime = 0.0;
    private float jerkRat;

    private static Transform mainCamTrans;

    private GameObject playerObj = null;

    private Vector3 lastFakeLocalPosition;
    private Vector3 fakeLocalPosition;

    private List<primDecorationMoving> others;

    void Start()
    {
        if (mainCamTrans == null)
        {
            mainCamTrans = Camera.main.transform;
        }

        existTime = 0.0;
        jerkRat = 0f;
        originalPos = lastFakeLocalPosition = fakeLocalPosition = transform.localPosition;

        if (randomizeVelocityDirection)
        {
            v = v.magnitude * Fakerand.UnitCircle(true);
        }

        if (makeParentOfThisTag != "")
        {
            primExtraTags[] pet = FindObjectsOfType<primExtraTags>();
            for (int i = 0; i < pet.Length; i++)
            {
               if (pet[i].tags.Contains(makeParentOfThisTag))
               {
                    pet[i].transform.SetParent(transform, true);
                    Rigidbody2D r2p = pet[i].GetComponent<Rigidbody2D>();
                    /*if (r2p)
                    {
                        velMaker.Add(r2p);
                    }*/
               }
            }
        }

        others = new List<primDecorationMoving>(GetComponents<primDecorationMoving>());
        others.Remove(this);

        if (GetComponent<Rigidbody2D>() && !GetComponentInChildren<beamBlock>() && !GetComponent<StaticBulletsOnVertices>()) // default for moving platforms now
        {
            constantUseRigidbodyPhysics = true;
            GetComponent<Rigidbody2D>().isKinematic = false;
            GetComponent<Rigidbody2D>().mass = 1000000f;
            GetComponent<Rigidbody2D>().gravityScale = 0f;
            GetComponent<Rigidbody2D>().freezeRotation = true;
        }
    }

    private void Reflect(Collision2D col)
    {
        if (!rbReflectThisFrame && rigidbodyReflect != 0)
        {
            --rigidbodyReflect;
            if (rigidbodyReflect == 0)
            {
                if (GetComponent<Rigidbody2D>())
                {
                    GetComponent<Rigidbody2D>().isKinematic = true;
                    GetComponent<Rigidbody2D>().useFullKinematicContacts = false;
                }
            }
            rbReflectThisFrame = true;
            v = rigidbodyReflectElasticity * Vector2.Reflect(v, col.GetContact(0).normal);
        }
    }

    /*private void OnCollisionStay2D(Collision2D col)
    {
        Reflect(col);
    }*/

    private void OnCollisionEnter2D(Collision2D col)
    {
        Reflect(col);
    }

    public void OnAmbushBegin()
    {
        enabled = !enabled;
    }

    public void OnAmbushComplete()
    {
        enabled = !enabled;
    }

    private int lateVelocitationFramesTotal = 8;
    private int lateVelocitationFramesCurrent = 0;
    private Vector2 lateVelocitationVector = Vector2.zero;

    public Vector2 GetVelocitation(bool canBeLate = false) // uses a 1 frame delay to make player stick on better
    {
        if (lateVelocitationFramesCurrent > 0 && canBeLate) { return lateVelocitationVector; }
        return (fakeLocalPosition - lastFakeLocalPosition) / Time.deltaTime;
    }

    public void SetLateVelocitation(Vector2 v)
    {
        lateVelocitationVector = v;
        lateVelocitationFramesCurrent = lateVelocitationFramesTotal;
    }

    Rigidbody2D r2;

    private bool positionChangedManuallyThisFrame = false;

    public void SetPosition(Vector3 pos, Vector3? dif = null)
    {
        positionChangedManuallyThisFrame = true;
        if (constantUseRigidbodyPhysics && !r2) { r2 = GetComponent<Rigidbody2D>(); }
        if (dif == null)
        {
            dif = fakeLocalPosition - lastFakeLocalPosition;
        }
        else
        {
            v = dif.Value * 60f;
        }
        lastFakeLocalPosition = pos - dif.Value;
        fakeLocalPosition = pos;
        if (constantUseRigidbodyPhysics)
        {
            r2.MovePosition(pos);
        }
        else
        {
            transform.position = pos;
        }
    }

    void Update() // this used to be FixedUpdate; but this causes issues because FixedUpdate is not run every frame at <1 timescale (blame LevelInfoContainer)
    {
        if (Time.timeScale == 0 || !enabled) { return; }
        if (lateVelocitationFramesCurrent > 0) { --lateVelocitationFramesCurrent; }
        if (!positionChangedManuallyThisFrame) { lastFakeLocalPosition = fakeLocalPosition; }
        Vector2 last = transform.localPosition;
        if (!constantUseRigidbodyPhysics) { transform.localPosition = fakeLocalPosition; }
        if (onlyUpdateOnVisible && !onlyUpdateOnVisible.isVisible) { return; }

        existTime += Time.deltaTime;

        bool glitchedOut = 
            (mvtType == MvtType.CircleWithNoise && Mathf.Abs(transform.localPosition.x - originalPos.x) > r.x + 16)
            || (mvtType == MvtType.CircleWithNoise && Mathf.Abs(transform.localPosition.y - originalPos.y) > r.y + 16);
        
        if (glitchedOut) { fakeLocalPosition = lastFakeLocalPosition = transform.localPosition = originalPos; }

        float camDist = ((Vector2)mainCamTrans.position - (Vector2)transform.position).magnitude;
        if (camDist > radiusToUpdateAtAll) {
            return;
        }
        bool updateR2 = (camDist <= radiusToUpdateRigidbody) && rigidbodyPositionUpdate;

        if (mvtType == MvtType.CircleWithNoise)
        {
            double t = existTime;
            fakeLocalPosition = originalPos
                + new Vector3(noiseIntensity.x * Mathf.PerlinNoise(noiseVelocity.x * (float)t, 869f) + r.x * (float)Math.Cos(v.x * t + angularOffset.x * Mathf.PI * 2f)
                , noiseIntensity.y * Mathf.PerlinNoise(544f, noiseVelocity.y * (float)t) + r.y * (float)Math.Sin(v.y * t + angularOffset.y * Mathf.PI * 2f)
                , noiseIntensity.z * Mathf.PerlinNoise(544f, noiseVelocity.z * (float)t) + r.z * (float)Math.Sin(v.z * t + angularOffset.z * Mathf.PI * 2f));

            Rigidbody2D r2 = GetComponent<Rigidbody2D>();
            Vector2 mv = ((Vector2)fakeLocalPosition - last) / Time.deltaTime;
            if (r2 && updateR2 && mv.magnitude < 1200)
            {
                if (constantUseRigidbodyPhysics)
                {
                    r2.velocity = mv;
                }
                else
                {
                    r2.MovePosition(transform.position);
                    r2.velocity = mv;
                }
                
            }
        }

        if (mvtType == MvtType.Constant && existTime >= startDelay) {
            if (!constantUseRigidbodyPhysics)
            {
                fakeLocalPosition += 0.0166666666f * Time.timeScale * v;
            }
            if (autoReflectOnBounds)
            {
                Vector2 norm = Vector2.zero;
                if (fakeLocalPosition.x > maxPosition.x) { norm = Vector2.left; }
                else if (fakeLocalPosition.y > maxPosition.y) { norm = Vector2.down; }
                else if (fakeLocalPosition.y < minPosition.y) { norm = Vector2.up; }
                else if (fakeLocalPosition.x < minPosition.x) { norm = Vector2.right; }
                else { goto endReflect; }

                last = fakeLocalPosition = new Vector3(
                    Mathf.Clamp(fakeLocalPosition.x, minPosition.x, maxPosition.x),
                    Mathf.Clamp(fakeLocalPosition.y, minPosition.y, maxPosition.y),
                    fakeLocalPosition.z);
                v = Vector2.Reflect(v, norm);
                fakeLocalPosition += 0.0166666666f * Time.timeScale * v;
            }

            if (wrapOnBounds)
            {
                Vector2 mvt = Vector2.zero;
                if (fakeLocalPosition.x > maxPosition.x) { mvt = (maxPosition.x - minPosition.x) * Vector2.left; }
                if (fakeLocalPosition.x < minPosition.x) { mvt = (maxPosition.x - minPosition.x) * Vector2.right; }
                if (fakeLocalPosition.y > maxPosition.y) { mvt = (maxPosition.y - minPosition.y) * Vector2.down; }
                if (fakeLocalPosition.y < minPosition.y) { mvt = (maxPosition.y - minPosition.y) * Vector2.up; }
                if (mvt != Vector2.zero)
                {
                    last += mvt;
                    fakeLocalPosition += (Vector3)mvt;
                    lastFakeLocalPosition += (Vector3)mvt;
                }
            }

        endReflect:

            fakeLocalPosition = new Vector3(
                Mathf.Clamp(fakeLocalPosition.x, minPosition.x, maxPosition.x),
                Mathf.Clamp(fakeLocalPosition.y, minPosition.y, maxPosition.y),
                fakeLocalPosition.z);

            if (GetComponent<Rigidbody2D>()) {
                Rigidbody2D r2 = GetComponent<Rigidbody2D>();
                if (updateR2)
                {
                    if (constantUseRigidbodyPhysics)
                    {
                        r2.velocity = v;
                        if (!positionChangedManuallyThisFrame)
                        {
                            lastFakeLocalPosition = fakeLocalPosition;
                            fakeLocalPosition = transform.position + v * Time.deltaTime; // dont change this to localPosition!!
                        }
                    }
                    else 
                    {
                        r2.MovePosition(transform.position);
                        r2.velocity = ((Vector2)transform.localPosition - last) * 60f;
                    }
                    
                }
                else
                {
                    r2.velocity = Vector2.zero;
                }
            }

            v += acceleration * 0.01666666666f * Time.timeScale;
        }

        if (mvtType == MvtType.SuddenJerk && existTime >= startDelay && jerkRat < 1f)
        {
            float t = Mathf.InverseLerp((float)startDelay, (float)(startDelay + jerkDuration), (float)existTime);
            float v = EasingOfAccess.Evaluate(jerkEasingType, t);
            float dif = v - jerkRat;

            fakeLocalPosition += dif * r;

            jerkRat = v;
            Rigidbody2D r2 = GetComponent<Rigidbody2D>();
            if (r2 && updateR2)
            {
                r2.MovePosition(transform.position);
                r2.velocity = ((Vector2)transform.localPosition - last) * 60f;
            }
        }

        if (rbReflectThisFrame) { rbReflectThisFrame = false; }

        if (ignorePlayerCollision && LevelInfoContainer.GetActiveControl())
        {
            GameObject g = LevelInfoContainer.GetActiveControl().gameObject;
            if (g != playerObj)
            {
                playerObj = g;
                foreach (Collider2D c in g.GetComponentsInChildren<Collider2D>())
                {
                    foreach (Collider2D c2 in GetComponents<Collider2D>())
                    {
                        Physics2D.IgnoreCollision(c, c2);
                    }
                }
            }
        }

        // different coexisting mvt modifiers need to know what the fake position is.
        for (int i = 0; i < others.Count; ++i)
        {
            others[i].lastFakeLocalPosition = lastFakeLocalPosition;
            others[i].fakeLocalPosition = fakeLocalPosition;
        }

        if (positionChangedManuallyThisFrame) { positionChangedManuallyThisFrame = false; }
    }
}
