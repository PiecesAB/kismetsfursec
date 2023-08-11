using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UfoObstacle : GenericBlowMeUp
{

    public enum MovementMode
    {
        Static, Linear, ErraticLinear, LinearStop, Loopy, ErraticLoopy
    }

    public enum ShootMode
    {
        None, Interval, FastInterval, SuperfastInterval, Erratic, OnLinearDirectionChange
    }

    public enum ShootShape
    {
        StraightDown, LeftBurst
    }

    public SkinnedMeshRenderer armsToAnim = null;
    public Renderer mainRendererWhenArmsAbsent = null;
    public Transform rotationHandle = null;
    public MovementMode mvtMode;
    public ShootMode shootMode;
    public ShootShape shootShape = ShootShape.StraightDown;
    public Rigidbody2D myColliderBody;
    public BulletData bulletSample;
    public float activeRadius = 480;

    private BoxCollider2D mvtRange;

    public float mvtMainSpeed = 120f;
    public float mvtErraticMin = 60f;
    public float mvtErraticMax = 180f;
    public double nextMvtTimeWait = 0.8;
    private const float shootSpeed0 = 1.2f;
    private const float shootSpeed1 = 0.6f;
    private const float shootSpeed2 = 0.3f;
    private const float shootErraticMin = 0.1f;
    private const float shootErraticMax = 0.5f;


    private Vector2 currMvtDir;
    private float mvtErraticSpeed;
    private double nextMvtTime;
    private float shootSpeedMultiplier;
    private float random1;
    public AudioClip shootSound;

    private Vector2 dif;

    private Transform camTrans;

    private AudioSource aud;

    private void Start()
    {
        camTrans = Camera.main.transform;
        mvtRange = GetComponent<BoxCollider2D>();
        currMvtDir = Fakerand.UnitCircle(true);
        AdjustMvtDir();
        nextMvtTime = -5.0;
        shootSpeedMultiplier = 1f;
        random1 = Fakerand.Single();
        StartCoroutine(ShootLoop());

        aud = GetComponent<AudioSource>();
    }

    private void Shoot()
    {
        /*GameObject newBullet = Instantiate(bulletPrefab, (Vector2)(transform.position - 8f * transform.up), Quaternion.Euler(transform.eulerAngles + Vector3.back * 90f));
        newBullet.SetActive(true);
        NormalBulletBehavior behav = newBullet.GetComponent<NormalBulletBehavior>();
        behav.speed /= shootSpeedMultiplier;*/

        Vector3 spawn;
        BulletObject b = null;
        Texture nothing;

        switch (shootShape) {
            case ShootShape.StraightDown:
                spawn = transform.position - 8f * transform.up;

                b = new BulletObject();
                bulletSample.TransferBasicInfo(b, out nothing);
                b.UpdateTransform(spawn, 0, bulletSample.scale);
                b.originPosition = spawn;
                b.startingDirection = transform.eulerAngles.z - 90;
                BulletRegister.Register(ref b, bulletSample.material, bulletSample.sprite.texture);
                break;
            case ShootShape.LeftBurst:
                if (dif == Vector2.zero) { break; }
                for (float angle = 90f; angle <= 270f; angle += 30f)
                {
                    Vector3 dir = (Quaternion.AngleAxis(angle, Vector3.forward)*dif).normalized;
                    spawn = transform.position + 8f * dir;

                    b = new BulletObject();
                    bulletSample.TransferBasicInfo(b, out nothing);
                    b.UpdateTransform(spawn, 0, bulletSample.scale);
                    b.originPosition = spawn;
                    b.startingDirection = Mathf.Atan2(dir.y, dir.x)*Mathf.Rad2Deg;
                    BulletRegister.Register(ref b, bulletSample.material, bulletSample.sprite.texture);
                }
                break;
        }

        if (aud)
        {
            aud.Stop();
            aud.clip = shootSound;
            aud.Play();
        }
    }

    private IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(Fakerand.Single());
        while (gameObject != null)
        {
            Vector2 oldCurrMvtDir = currMvtDir;
            switch (shootMode)
            {
                case ShootMode.Interval: yield return new WaitForSeconds(shootSpeed0*shootSpeedMultiplier); break;
                case ShootMode.FastInterval: yield return new WaitForSeconds(shootSpeed1*shootSpeedMultiplier); break;
                case ShootMode.SuperfastInterval: yield return new WaitForSeconds(shootSpeed2*shootSpeedMultiplier); break;
                case ShootMode.Erratic: yield return new WaitForSeconds(Fakerand.Single(shootErraticMin, shootErraticMax) * shootSpeedMultiplier); break;
                case ShootMode.OnLinearDirectionChange: yield return new WaitUntil(() => (currMvtDir != oldCurrMvtDir && currMvtDir.sqrMagnitude > 0.01f)); break;
                default: yield return new WaitForSeconds(1f); break;
            }
            bool visible = (armsToAnim != null && armsToAnim.isVisible) || (mainRendererWhenArmsAbsent != null && mainRendererWhenArmsAbsent.isVisible);
            if (visible && shootMode != ShootMode.None) { Shoot(); }
        }
        yield return null;
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        shootSpeedMultiplier *= Mathf.Pow(0.85f, ohi.amt);
    }

    private void AdjustMvtDir()
    {
        while (Mathf.Abs(Vector2.Dot(currMvtDir, Vector2.right)) > 0.9f ||
               Mathf.Abs(Vector2.Dot(currMvtDir, Vector2.up)) > 0.9f)
        {
            currMvtDir = Fakerand.UnitCircle(true);
        }

        if (mvtRange.size.x < 3f)
        {
            currMvtDir = new Vector2(0f, Mathf.Sign(currMvtDir.y));
        }

        if (mvtRange.size.y < 3f)
        {
            currMvtDir = new Vector2(Mathf.Sign(currMvtDir.x), 0f);
        }

        if (mvtMode == MovementMode.ErraticLinear)
        {
            mvtErraticSpeed = Fakerand.Single(mvtErraticMin, mvtErraticMax);
        }
    }

    private void OnDestroy()
    {
        Destroy(transform.parent.gameObject);
    }

    private void FixedUpdate()
    {
        bool visible = (armsToAnim != null && armsToAnim.isVisible) || (mainRendererWhenArmsAbsent != null && mainRendererWhenArmsAbsent.isVisible);
        if (visible && myColliderBody != null)
        {
            myColliderBody.position = transform.position;
        }
    }

    private void Update()
    {
        if (((Vector2)camTrans.position - (Vector2)transform.position).magnitude > activeRadius) { return; }
        bool visible = (armsToAnim != null && armsToAnim.isVisible) || (mainRendererWhenArmsAbsent != null && mainRendererWhenArmsAbsent.isVisible);

        if (Time.timeScale > 0 && mvtRange != null)
        {
            Vector2 outOfBoundsDir = mvtRange.bounds.ClosestPoint(transform.position) - transform.position;
            bool outOfBounds = ((outOfBoundsDir.sqrMagnitude > 0.0001f) && Vector2.Dot(outOfBoundsDir.normalized, currMvtDir) < 0f);
            if (outOfBounds)
            {
                if (mvtMode == MovementMode.LinearStop && nextMvtTime <= DoubleTime.ScaledTimeSinceLoad)
                {
                    nextMvtTime = DoubleTime.ScaledTimeSinceLoad + nextMvtTimeWait;
                }
                else
                {
                    currMvtDir = Vector2.Reflect(currMvtDir, outOfBoundsDir.normalized).normalized;
                    AdjustMvtDir();
                }
            }

            switch (mvtMode)
            {
                case MovementMode.Linear:
                    LinearLabel:
                    dif = currMvtDir * mvtMainSpeed * Time.deltaTime;
                    transform.position += (Vector3)dif;
                    mvtRange.offset -= dif;
                    if (visible)
                    {
                        myColliderBody.gameObject.SetActive(true);
                        myColliderBody.velocity = currMvtDir * mvtMainSpeed;
                    }
                    else
                    {
                        myColliderBody.gameObject.SetActive(false);
                    }
                    break;
                case MovementMode.Loopy:
                case MovementMode.ErraticLoopy:
                    Vector2 center = mvtRange.bounds.center;
                    float erraticCoeff = (mvtMode != MovementMode.ErraticLoopy) ? (0f) : (500f * (float)Math.Acos(Math.Cos(DoubleTime.ScaledTimeSinceLoad * 0.0025f + 326f)));
                    float newX = Mathf.PerlinNoise(1000f * (float)Math.Acos(Math.Cos(DoubleTime.ScaledTimeSinceLoad * 0.001f)) - erraticCoeff, (random1*1000f) + erraticCoeff);
                    float newY = Mathf.PerlinNoise(-1000f * (float)Math.Acos(Math.Cos(DoubleTime.ScaledTimeSinceLoad * 0.001f)) + erraticCoeff, (random1 * 1000f)+100f - erraticCoeff);
                    Vector2 newPos = center - (Vector2)mvtRange.bounds.extents + new Vector2(mvtRange.bounds.size.x * newX, mvtRange.bounds.size.y * newY);
                    dif = newPos - (Vector2)transform.position;
                    transform.position += (Vector3)dif;
                    mvtRange.offset -= dif;
                    if (visible)
                    {
                        myColliderBody.gameObject.SetActive(true);
                        myColliderBody.velocity = dif * 60f;
                    }
                    else
                    {
                        myColliderBody.gameObject.SetActive(false);
                    }
                    break;
                case MovementMode.LinearStop:
                    if (nextMvtTime <= DoubleTime.ScaledTimeSinceLoad)
                    {
                        goto LinearLabel;
                    }
                    else
                    {
                        if (visible)
                        {
                            myColliderBody.gameObject.SetActive(true);
                            myColliderBody.velocity = Vector2.zero;
                        }
                        else
                        {
                            myColliderBody.gameObject.SetActive(false);
                        }
                    }
                    break;
                case MovementMode.ErraticLinear:
                    dif = currMvtDir * mvtErraticSpeed * Time.deltaTime;
                    transform.position += (Vector3)dif;
                    mvtRange.offset -= dif;
                    if (visible)
                    {
                        myColliderBody.gameObject.SetActive(true);
                        myColliderBody.velocity = currMvtDir * mvtErraticSpeed;
                    }
                    else
                    {
                        myColliderBody.gameObject.SetActive(false);
                    }
                    break;
                case MovementMode.Static:
                default:
                    break;
            }
        }

        if (visible)
        {
            if (armsToAnim != null)
            {
                armsToAnim.SetBlendShapeWeight(0, 100f * Mathf.Abs((float)Math.Cos(DoubleTime.UnscaledTimeSinceLoad * 6.28)));
            }
            if (rotationHandle != null && mvtMode != MovementMode.Static && dif.sqrMagnitude > 0.000001f)
            {
                rotationHandle.rotation = Quaternion.Lerp(rotationHandle.rotation,Quaternion.LookRotation(dif, Vector3.Cross(dif, Vector3.forward*((dif.x > 0)?-1:1))),0.25f);
            }
        }
    }
}
