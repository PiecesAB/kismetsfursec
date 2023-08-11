using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//NormalCircle(GameObject bulletObj,float deletTime, Vector3 pos, 
//Color color,float offsetRotation, float speed, int number, float torque, float acceleration, float changeInTorque, bool isAccelerationMultiplicative,bool isTorqueSineWave, float damage)

[Serializable]
public class BulletData
{
    public float deletTime;
    public float fadeInTime = 0.5f;
    public Color color;
    public float speed;
    public float torque;
    public float acceleration;
    public float changeInTorque;
    public float sineMotionSpeed = 0f;
    public bool isAccelerationMultiplicative;
    public bool isTorqueSineWave;
    public float simulationSpeedMult = 1f;
    public float damage;
    public bool customSprite;
    public Sprite sprite;
    public Vector2 scale;
    public float killRadiusRatio = 1f;
    public bool rotateSprite = true;
    public Material material;
    public bool squareHitbox;
    public bool collisionDisabled = false;
    public bool grazeDisabled = false;
    public bool destroyOffScreen = true;
    public bool destroyOnScreenScroll = false;
    public int bitToggleOnJump = -1; // -1: none. 0: start off. 1: start on.

    public void TransferBasicInfo(BulletObject o, out Texture binTex)
    {
        o.deletTime = deletTime;
        o.fadeInTime = fadeInTime;
        o.color = color;
        o.startingVelocity = speed;
        o.startingTorque = torque;
        o.acceleration = acceleration;
        o.changeInTorque = changeInTorque;
        o.isAccelerationMultiplicative = isAccelerationMultiplicative;
        o.isTorqueSineWave = isTorqueSineWave;
        o.sineMotionSpeed = sineMotionSpeed;
        o.simulationSpeedMult = simulationSpeedMult;
        o.damage = damage;

        o.killRadiusRatio = killRadiusRatio;
        o.rotateWithMovementAngle = rotateSprite;
        o.squareHitbox = squareHitbox;
        o.collisionDisabled = collisionDisabled;
        o.grazeDisabled = grazeDisabled;
        o.destroyOnLeaveScreen = destroyOffScreen;
        o.destroyOnScreenScroll = destroyOnScreenScroll;

        BinaryBullet.Initialize(o, bitToggleOnJump, out binTex);
        //this bullet is not registered, and its position/rotation/scale hasn't been set.
        // will still need to do this in every bullet-creating script.
        // may also need to set origin position and direction.

    }

    public void TransferBasicInfo(BulletObject o)
    {
        Texture binTex;
        TransferBasicInfo(o, out binTex);
    }
}

[Serializable]
public class BulletShooterData
{
    public int numberInCircle;
    public float offsetRotation;
    public float offsetRotationAfterPosition = 0f;
    public float polygonSides;
    public float spreadAngle;
    public Vector3 posOffset;
    public float radialStartDistance = 0f;
    public float speedVariation = 0f;
    public float flowerConstant;
    public float flowerOpenness;
    public float flowerThetaRangeTimesPi;
}

[Serializable]
public class BulletMakerChangePerRepeat
{
    public int numberInCircle = 0;
    public float bulletSpeed = 0f;
    public float hueShift = 0f;
}


public class BulletHellMakerFunctions : MonoBehaviour {

    

    public enum Behaviors
    {
        ShootForward,
        ShootForwardNervously,
        ShootForwardAndRotate,
        ShootTowardsPlayer,
        ShootTowardsPlayerNervously,
        ShootTowardsWherePlayerWillBe,
        DoNothing,
    };

    public enum Patterns
    {
        NormalCircle,
        SpreadShot,
        FlowerShot,
        Heartbreaker,
        NormalPolygon,
        StarPolygon,
        RandomInCircle,
        MeshShot,
    };

    public enum FromOtherBulletsStyle
    {
        Keep, Replace, OnBulletUnregister
    }

    [HideInInspector]
    public GameObject testBullet; // unused?
    public BulletData bulletData;
    public GameObject bulletSpecialObject = null;
    public bool rotateSpecialObject = false;
    public BulletShooterData bulletShooterData;
    public GameObject player;
    public Behaviors behavior;
    public Patterns pattern;

    public Mesh meshForMeshShot;
    public Vector3 meshShotOffset = Vector3.zero;
    public Vector3 meshShotScale = Vector3.one;
    private HashSet<Vector3> meshShotVertices;

    public bool thisStartsItself;
    public bool thisMakesItsOwnBullets;
    public float startDelay = 0f;
    public float waitTime;
    public int setNumberOfRepeats = -1;
    [HideInInspector]
    public int origNumberOfRepeats;
    public float startUpAgainAfterAllRepeats = 0f;
    [Header("Make sure below is -1 to disable this counter")]
    public int numberOfSuperRepeats = -1;
    private int origNumberOfSuperRepeats = -1;
    [Header("---Rhythm stuf---")]
    public bool shootToRhythm;
    public bool notFollowsBeat;
    public int startRhythmBeat;
    public int stopRhythmBeat;
    public int skipHowManyBeats;
    [Header("------------------")]
    public bool rainbowBullets;
    public float[] behaveData;
    [Header("Symbol color is next, if Color.clear, then defaults to bullet color.")]
    public Color symbolColor = Color.clear;
    public Renderer tangibleRenderer;
    public bool dontShootWhenTangRendOffscreen = true;
    public bool waitUntilTangRendOnscreenToStart = false;
    public bool randomBitToggle = false;
    public bool shootFromAllChildPositions = false;
    public BulletMakerTracker shootFromBulletsOfAnother = null;
    public FromOtherBulletsStyle shootFromOtherBulletsStyle = FromOtherBulletsStyle.Keep;
    public bool shootFromBulletsAddRotation = false;
    public BulletHellMakerFunctions nextInShootSequence = null;
    public bool moveJustBeforeShot = false;

    public static GameObject symbolPrefab;

    private BulletHellAddedTimingFunctions timingFunctions;

    private Material bulletMaterial;
    private GameObject mySymbol;
    private BulletMakerTracker bulletTracker;
    [HideInInspector]
    public BulletDiscreteOverlay discreteOverlay;
    private IBulletMakerResetOnSuperRepeat[] specialBehaviorsDependingOnSuperReset = new IBulletMakerResetOnSuperRepeat[0];
    private IBulletMakerOnShot[] specialBehaviorsOnShot = new IBulletMakerOnShot[0];

    private List<Transform> childTransforms = new List<Transform>();

    [HideInInspector]
    public double lastShotTime;

    public static HashSet<BulletHellMakerFunctions> all = new HashSet<BulletHellMakerFunctions>();

    private AudioSource sound;

    // For subframe special shots
    private double subframeTimeOffset = 0.0;
    public void FireAtTime(double t)
    {
        subframeTimeOffset = t - DoubleTime.ScaledTimeSinceLoad;
        Fire();
        subframeTimeOffset = 0.0;
    }

    private bool disableFireRightBeforeShot = false;
    public void DisableFireRightBeforeShot()
    {
        disableFireRightBeforeShot = true;
    }

    public void FireWhilePaused()
    {
        Fire(true);
    }

    public void Fire(bool ignorePaused = false)
    {
        if (moveJustBeforeShot) { Move(ignorePaused); }
        if (!enabled) { return; }
        if (Door1.levelComplete) { return; }
        if (dontShootWhenTangRendOffscreen && tangibleRenderer != null && !tangibleRenderer.isVisible) { return; }
        if (randomBitToggle) { bulletData.bitToggleOnJump = Fakerand.Int(0, 2); }

        /*if (GetComponent<BulletHellAddedTimingFunctions>() != null)
        {
            GetComponent<BulletHellAddedTimingFunctions>().ExtraFire();
        }*/

        //GameObject testBullet2 = BulletPool.Pop();
        //testBullet2.name = "Energy Bullet";

        for (int i = 0; i < specialBehaviorsOnShot.Length; ++i)
        {
            specialBehaviorsOnShot[i].BeforeShot();
        }

        if (disableFireRightBeforeShot)
        {
            disableFireRightBeforeShot = false;
            return;
        }

        if (bulletShooterData.numberInCircle > 0)
        {
            if (sound && !soundPlayedThisFrame) { sound.Stop(); sound.Play(); soundPlayedThisFrame = true; }
            lastShotTime = DoubleTime.ScaledTimeSinceLoad;

            if (pattern == Patterns.NormalCircle)
            {
                NormalCircle();
            }
            if (pattern == Patterns.SpreadShot)
            {
                SpreadShot();
            }
            if (pattern == Patterns.FlowerShot)
            {
                FlowerShot();
            }
            if (pattern == Patterns.NormalPolygon)
            {
                Polygonal(false);
            }
            if (pattern == Patterns.StarPolygon)
            {
                Polygonal(true);
            }
            if (pattern == Patterns.RandomInCircle)
            {
                RandomInCircle();
            }
            if (pattern == Patterns.MeshShot)
            {
                MeshShot();
            }
        }

        for (int i = 0; i < specialBehaviorsOnShot.Length; ++i)
        {
            specialBehaviorsOnShot[i].OnShot();
        }

        //Destroy(testBullet2);
    }

    public void AddToBulletTracker(BulletObject b)
    {
        if (bulletTracker != null) { bulletTracker.myBullets.Add(b); }
        if (discreteOverlay != null) { discreteOverlay.MakeNew(b); }
    }

    public void RepositionToOtherBullet(BulletObject b, bool useRotationDegrees = false)
    {
        if (b == null) { return; }
        transform.position = b.GetPosition() - Vector3.forward * 4f;
        if (shootFromBulletsAddRotation) { transform.eulerAngles = new Vector3(0, 0, useRotationDegrees ? b.GetRotationDegrees() : b.startingDirection); }
    }

    public void SChange()
    {
        for (int i = 0; i < specialBehaviorsDependingOnSuperReset.Length; ++i)
        {
            specialBehaviorsDependingOnSuperReset[i].MakerReset();
        }  
    }

    private static Transform mainCamTrans = null;

    private bool VisibleCenter(Renderer r)
    {
        if (mainCamTrans == null) { mainCamTrans = Camera.main.transform; }

        Vector2 p = r.transform.position;
        Vector2 c = mainCamTrans.position;
        return (Mathf.Abs(p.x - c.x) < 160f) && (Mathf.Abs(p.y - c.y) < 108f);
    }

    public bool shooting = false;

    public IEnumerator Test(bool repeated = false)
    {
        shooting = true;

        if (numberOfSuperRepeats == 0)
        {
            numberOfSuperRepeats = origNumberOfSuperRepeats;
            shooting = false;
            yield break;
        }
        --numberOfSuperRepeats;

        if (GetComponent<SpriteRenderer>() != null) { GetComponent<SpriteRenderer>().sprite = null; }
        if (!repeated && startDelay > 0) { yield return new WaitForSeconds(startDelay); }
        if (waitUntilTangRendOnscreenToStart && tangibleRenderer != null && !VisibleCenter(tangibleRenderer)) {
            yield return new WaitUntil(() => VisibleCenter(tangibleRenderer));
        }
        RhythmTimer r = null;
        if (shootToRhythm)
        {
            r = RhythmTimer.main;
        }
        // These are default behaviors.
        if (thisMakesItsOwnBullets)
        {
            float wait = 0;
            if (!shootToRhythm)
            {
                wait = 0;
            }
            else
            {

                while (r.currentBeat < startRhythmBeat)
                {
                    yield return new WaitForFixedUpdate();
                }
                if (!notFollowsBeat)
                {
                    wait = r.currentBeat;
                }
                else
                {
                    wait = (float)DoubleTime.ScaledTimeSinceLoad;
                }
            }
            while (true)
            {
                if (shootFromBulletsOfAnother)
                {
                    switch (shootFromOtherBulletsStyle) {
                        case FromOtherBulletsStyle.Keep:
                            for (int i = 0; i < shootFromBulletsOfAnother.myBullets.Count; ++i)
                            {
                                BulletObject b = shootFromBulletsOfAnother.myBullets[i];
                                RepositionToOtherBullet(b);
                                Fire();
                            }
                            break;
                        case FromOtherBulletsStyle.Replace:
                            for (int i = 0; i < shootFromBulletsOfAnother.myBullets.Count; ++i)
                            {
                                BulletObject b = shootFromBulletsOfAnother.myBullets[i];
                                RepositionToOtherBullet(b);
                                Fire();
                                BulletRegister.MarkToDestroy(b);
                            }
                            shootFromBulletsOfAnother.myBullets.Clear();
                            break;
                        default: break;
                    }
                }
                else if (shootFromAllChildPositions)
                {
                    Transform t = transform;
                    Vector3 origin = t.position;
                    Quaternion origRot = t.rotation;
                    List<Vector3> childPositions = new List<Vector3>();
                    List<Quaternion> childRotations = new List<Quaternion>();
                    foreach (Transform child in childTransforms)
                    {
                        childPositions.Add(child.position);
                        childRotations.Add(child.rotation);
                    }
                    for (int i = 0; i < childRotations.Count; ++i)
                    {
                        t.position = childPositions[i];
                        t.rotation = childRotations[i];
                        Fire();
                    }
                    t.position = origin;
                    t.rotation = origRot;
                }
                else
                {
                    Fire();
                }

                if (!shootToRhythm || notFollowsBeat)
                    wait = waitTime;

                if (wait == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!shootToRhythm || notFollowsBeat)
                    {
                        wait += (float)DoubleTime.ScaledTimeSinceLoad;
                        while (wait - DoubleTime.ScaledTimeSinceLoad > 0)
                        {
                            yield return new WaitForFixedUpdate();
                        }
                    }
                    else
                    {
                        while (wait + skipHowManyBeats >= r.currentBeat)
                        {
                            yield return new WaitForFixedUpdate();
                        }

                        wait = r.currentBeat;
                        if (wait > stopRhythmBeat && stopRhythmBeat != 0)
                        {
                            break;
                        }
                    }
                }
                yield return 1; // just in case crud happens
                if (setNumberOfRepeats > 0)
                {
                    --setNumberOfRepeats;
                    if (setNumberOfRepeats == 0)
                    {
                        if (startUpAgainAfterAllRepeats < 0.00001f)
                        {
                            Destroy(mySymbol);
                            numberOfSuperRepeats = 0;
                            shooting = false;
                            yield break;
                        }
                        else
                        {
                            if (startUpAgainAfterAllRepeats > 0) { yield return new WaitForSeconds(startUpAgainAfterAllRepeats); }
                            setNumberOfRepeats = origNumberOfRepeats;
                            SChange();
                            if (nextInShootSequence != null)
                            {
                                StartCoroutine(nextInShootSequence.Test(true));
                            }
                            else
                            {
                                StartCoroutine(Test(true));
                            }
                            yield break;
                        }
                    }
                }
            }
        }
        shooting = false;
    }

    private float baseOffset = Mathf.Infinity;

    public void Move(bool ignorePaused = false)
    {
        if (Time.timeScale == 0 && !ignorePaused) { return; }
        //if (player == null)
        {
            Encontrolmentation e = LevelInfoContainer.GetActiveControl();
            if (e == null) { return; }
            player = e.gameObject;
        }


        if (behavior == Behaviors.ShootForwardAndRotate) // two variables: 0 is the rotation amount. 1 is the cosine wave motion offset and qualifier. 2 is acceleration over time
        {
            if (System.Math.Abs(behaveData[1]) == 0)
            {
                transform.Rotate(new Vector3(0, 0, behaveData[0] * Time.timeScale));
            }
            if (System.Math.Abs(behaveData[1]) > 0)
            {
                transform.Rotate(new Vector3(0, 0, Time.timeScale * behaveData[0] * Mathf.Sin(behaveData[1] + (float)DoubleTime.ScaledTimeSinceLoad)));
            }
            if (behaveData.Length >= 3)
            {
                behaveData[0] += behaveData[2] * Time.deltaTime;
            }
            if (behaveData.Length >= 4)
            {
                bulletShooterData.offsetRotationAfterPosition += behaveData[3] * Time.deltaTime;
            }


        }

        if (behavior == Behaviors.ShootTowardsPlayer) // one variable: 0 is the offset from the player in degrees.
        {
            float t1 = Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x - transform.position.x);
            transform.eulerAngles = new Vector3(0, 0, 0);
            transform.Rotate(new Vector3(0, 0, t1 * Mathf.Rad2Deg + behaveData[0]));
        }

        if (behavior == Behaviors.ShootTowardsPlayerNervously) // two variables: 0 is the offset from the player in degrees. 1 is the amount of glitchiness.
        {
            float t1 = Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x - transform.position.x);
            transform.eulerAngles = new Vector3(0, 0, 0);
            transform.Rotate(new Vector3(0, 0, (t1 * Mathf.Rad2Deg + behaveData[0]) + Fakerand.Single(-behaveData[1], behaveData[1])));
        }

        if (behavior == Behaviors.ShootForwardNervously) // one variable: 0 is the amount of glitchiness. 1 is same but adds base offset
        {
            //transform.eulerAngles = new Vector3(0, 0, 0);
            if (baseOffset == Mathf.Infinity)
            {
                baseOffset = bulletShooterData.offsetRotation;
            }

            bulletShooterData.offsetRotation = Fakerand.Single(-behaveData[0], behaveData[0]);
            if (behaveData.Length >= 2 && behaveData[1] > 0)
            {
                bulletShooterData.offsetRotation = baseOffset + Fakerand.Single(-behaveData[1], behaveData[1]);
            }
        }
    }

    private bool soundPlayedThisFrame = false;

    void Update()
    {
        soundPlayedThisFrame = false;
        if (!moveJustBeforeShot)
        {
            Move();
        }
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    void Start () {

        BulletParticleTest1 transfer = GetComponent<BulletParticleTest1>();
        if (transfer != null)
        {
            bulletData.damage = transfer.damage;
            waitTime = transfer.repeatAttackTime;
            bulletShooterData.numberInCircle = transfer.bulletNumber;
            bulletData.speed = transfer.generalVelocity;
            startDelay = transfer.delayBeforeStart;
        }

        bulletTracker = GetComponent<BulletMakerTracker>();
        specialBehaviorsDependingOnSuperReset = GetComponents<IBulletMakerResetOnSuperRepeat>();
        specialBehaviorsOnShot = GetComponents<IBulletMakerOnShot>();
        sound = GetComponent<AudioSource>();

        lastShotTime = 0.0;

        all.Add(this);
        origNumberOfRepeats = setNumberOfRepeats;
        origNumberOfSuperRepeats = numberOfSuperRepeats;
        if (symbolPrefab == null) { symbolPrefab = Resources.Load<GameObject>("BulletSymbol"); }
        if (tangibleRenderer != null)
        {
            tangibleRenderer.materials[0].color = bulletData.color;
        }

        if (meshForMeshShot != null)
        {
            Vector3[] nonScaleMesh = (Vector3[])meshForMeshShot.vertices.Clone();
            meshShotVertices = new HashSet<Vector3>();
            for (int i = 0; i < nonScaleMesh.Length; ++i)
            {
                Vector3 pos = nonScaleMesh[i];
                Vector3 vPos = new Vector3(pos.x * meshShotScale.x, pos.y * meshShotScale.y, pos.z * meshShotScale.z);
                meshShotVertices.Add(vPos);
            }
        }

        if (shootFromAllChildPositions)
        {
            foreach (Transform child in transform)
            {
                if (child == transform) { continue; }
                childTransforms.Add(child.transform);
            }
        }

        mySymbol = Instantiate(symbolPrefab, transform.position + (Vector3.forward *5f), Quaternion.identity, transform);
        Color c = bulletData.color;
        mySymbol.GetComponent<SpriteRenderer>().color = (symbolColor == Color.clear) ? (new Color(c.r, c.g, c.b, 0.35f)): symbolColor;

        if (!moveJustBeforeShot)
        {
            Move();
        }

        if (shootFromBulletsOfAnother && shootFromOtherBulletsStyle == FromOtherBulletsStyle.OnBulletUnregister)
        {
            if (GetComponent<SpriteRenderer>() != null) { GetComponent<SpriteRenderer>().sprite = null; }
            shootFromBulletsOfAnother.nextMakersOnBulletUnregister.Add(this);
            return;
        }

        if (thisStartsItself)
        {
            StartCoroutine(Test());
        }
	}

    private void InitBulletA(ref BulletObject b, out Texture binImg, bool addToTracker = true)
    {
        b.startingTorque = bulletData.torque;
        b.acceleration = bulletData.acceleration;
        b.changeInTorque = bulletData.changeInTorque;
        b.deletTime = bulletData.deletTime;
        b.fadeInTime = bulletData.fadeInTime;
        b.isTorqueSineWave = bulletData.isTorqueSineWave;
        b.sineMotionSpeed = bulletData.sineMotionSpeed;
        b.isAccelerationMultiplicative = bulletData.isAccelerationMultiplicative;
        b.atWhatDistanceFromCenterIsAHit = 8f; //obsolete
        b.simulationSpeedMult = bulletData.simulationSpeedMult;
        b.damage = bulletData.damage;
        b.killRadiusRatio = bulletData.killRadiusRatio;

        b.UpdateTransform(b.originPosition, 0, bulletData.scale);
        b.squareHitbox = bulletData.squareHitbox;
        b.rotateWithMovementAngle = bulletData.rotateSprite;
        b.destroyOnLeaveScreen = bulletData.destroyOffScreen;
        b.destroyOnScreenScroll = bulletData.destroyOnScreenScroll;
        b.grazeDisabled = bulletData.grazeDisabled;
        b.collisionDisabled = bulletData.collisionDisabled;

        b.timeThisWasMade_ScriptsOnly += subframeTimeOffset;

        if (rainbowBullets)
        {
            b.color = Color.HSVToRGB((float)((DoubleTime.ScaledTimeSinceLoad / 4) % 1), 1, 1);
        }
        else
        {
            b.color = bulletData.color;
        }
        if (bulletData.customSprite)
        {
            //bulletObj.GetComponent<SpriteRenderer>().sprite = bulletData.sprite;
        }

        // This makes the first frame looks correct.
        b.UpdateTransform(b.originPosition, b.startingDirection);

        BinaryBullet.Initialize(b, bulletData.bitToggleOnJump, out binImg);
        if (addToTracker) { AddToBulletTracker(b); }
    }

    public Vector2 Rotate2(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    private IEnumerator RigidbodySetVelocity(Rigidbody2D r2, Vector2 v)
    {
        yield return new WaitForEndOfFrame();
        r2.velocity = v;
    }

    public BulletObject MakeBulletForOtherObject(bool addToTracker = true)
    {
        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }

        BulletObject b = new BulletObject();
        float angleDeg = bulletShooterData.offsetRotation + transform.eulerAngles.z;
        Vector3 radVec = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
        b.originPosition = transform.position + bulletShooterData.posOffset + (bulletShooterData.radialStartDistance * radVec);
        b.startingDirection = angleDeg + bulletShooterData.offsetRotationAfterPosition;
        b.startingVelocity = v;
        Texture binImg;
        InitBulletA(ref b, out binImg, addToTracker);
        BulletRegister.Register(ref b, bulletData.material, (binImg != null) ? binImg : bulletData.sprite.texture);
        return b;
    }

    public void NormalCircle()
    {
        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }

        for (float a = 0; a < 359.99f; a += (360f / bulletShooterData.numberInCircle))
        {
            float angleDeg = a + bulletShooterData.offsetRotation + transform.eulerAngles.z;
            Vector3 radVec = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
            if (bulletSpecialObject != null)
            {
                GameObject bd = Instantiate(bulletSpecialObject, transform.position + bulletShooterData.posOffset + (bulletShooterData.radialStartDistance * radVec), Quaternion.identity);
                float ar = a + bulletShooterData.offsetRotation; // + transform.eulerAngles.z;
                if (bd.GetComponent<primDecorationMoving>())
                {
                    bd.GetComponent<primDecorationMoving>().v = bulletData.speed * Rotate2(transform.right, ar);
                }
                else if (bd.GetComponent<Rigidbody2D>())
                {
                    StartCoroutine(RigidbodySetVelocity(bd.GetComponent<Rigidbody2D>(), bulletData.speed * Rotate2(transform.right, ar)));
                }
                if (rotateSpecialObject) { bd.transform.eulerAngles = new Vector3(0, 0, ar + transform.eulerAngles.z); }
                bd.transform.SetParent(bulletSpecialObject.transform.parent, true);
                bd.SetActive(true);
                continue;
            }

            BulletObject b = new BulletObject();
            b.originPosition = transform.position + bulletShooterData.posOffset + (bulletShooterData.radialStartDistance*radVec);
            b.startingDirection = angleDeg + bulletShooterData.offsetRotationAfterPosition;
            b.startingVelocity = v;
            Texture binImg;
            InitBulletA(ref b, out binImg);
            BulletRegister.Register(ref b, bulletData.material, (binImg != null)?binImg:bulletData.sprite.texture);

            //GameObject b2 = (GameObject)Instantiate(bulletObj, transform.position+bulletShooterData.posOffset, Quaternion.identity);
            //b2.transform.parent = transform;
        }
    }

    public void SpreadShot()
    {
        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }

        float spread = bulletShooterData.spreadAngle/2;
        float a = -spread;
        for (int i = 0; i < bulletShooterData.numberInCircle; ++i)
        {
            float angleDeg = a + bulletShooterData.offsetRotation + transform.eulerAngles.z;
            Vector3 radVec = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
            if (bulletSpecialObject != null)
            {
                GameObject bd = Instantiate(bulletSpecialObject, transform.position + bulletShooterData.posOffset + (bulletShooterData.radialStartDistance * radVec), Quaternion.identity);
                float ar = a + bulletShooterData.offsetRotation; // + transform.eulerAngles.z;
                if (bd.GetComponent<primDecorationMoving>())
                {
                    bd.GetComponent<primDecorationMoving>().v = bulletData.speed * Rotate2(transform.right, ar);
                }
                else if (bd.GetComponent<Rigidbody2D>())
                {
                    StartCoroutine(RigidbodySetVelocity(bd.GetComponent<Rigidbody2D>(), bulletData.speed * Rotate2(transform.right, ar)));
                }
                if (rotateSpecialObject) { bd.transform.eulerAngles = new Vector3(0, 0, ar + transform.eulerAngles.z); }
                bd.SetActive(true);
                a += (spread * 2 / (bulletShooterData.numberInCircle - 1));
                continue;
            }

            BulletObject b = new BulletObject();
            b.originPosition = transform.position + bulletShooterData.posOffset + (bulletShooterData.radialStartDistance * radVec);
            b.startingDirection = angleDeg + bulletShooterData.offsetRotationAfterPosition;
            b.startingVelocity = v;
            Texture binImg;
            InitBulletA(ref b, out binImg);
            BulletRegister.Register(ref b, bulletData.material, (binImg != null) ? binImg : bulletData.sprite.texture);

            a += (spread * 2 / (bulletShooterData.numberInCircle - 1));
        }
    }

    public void MeshShot()
    {
        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }

        foreach (Vector3 pos in meshShotVertices)
        {
            Vector3 vPos = pos;
            vPos += meshShotOffset;
            if (vPos.magnitude > 0.0001)
            {
                if (bulletSpecialObject != null)
                {
                    GameObject bd = Instantiate(bulletSpecialObject, transform.position + bulletShooterData.posOffset, Quaternion.identity);
                    float ar = Mathf.Atan2(vPos.y, vPos.x) * Mathf.Rad2Deg + bulletShooterData.offsetRotation; // + transform.eulerAngles.z;
                    if (bd.GetComponent<primDecorationMoving>())
                    {
                        bd.GetComponent<primDecorationMoving>().v = vPos.magnitude * bulletData.speed * Rotate2(transform.right, ar);
                    }
                    else if (bd.GetComponent<Rigidbody2D>())
                    {
                        StartCoroutine(RigidbodySetVelocity(bd.GetComponent<Rigidbody2D>(), bulletData.speed * Rotate2(transform.right, ar)));
                    }
                    if (rotateSpecialObject) { bd.transform.eulerAngles = new Vector3(0, 0, ar + transform.eulerAngles.z); }
                    bd.SetActive(true);
                    continue;
                }

                BulletObject b = new BulletObject();
                b.originPosition = transform.position + bulletShooterData.posOffset;
                b.startingDirection = Mathf.Atan2(vPos.y, vPos.x) * Mathf.Rad2Deg + bulletShooterData.offsetRotation + transform.eulerAngles.z;
                b.startingVelocity = vPos.magnitude * v;
                Texture binImg;
                InitBulletA(ref b, out binImg);
                BulletRegister.Register(ref b, bulletData.material, (binImg != null) ? binImg : bulletData.sprite.texture);
            }
        }

    }

    public void RandomInCircle()
    {
        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }

        for (int i = 0; i < bulletShooterData.numberInCircle; ++i)
        {
            float a = (Fakerand.Single() * bulletShooterData.spreadAngle) - (bulletShooterData.spreadAngle * 0.5f);
            float angleDeg = a + bulletShooterData.offsetRotation + transform.eulerAngles.z;
            Vector3 radVec = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
            if (bulletSpecialObject != null)
            {
                GameObject bd = Instantiate(bulletSpecialObject, transform.position + bulletShooterData.posOffset + (bulletShooterData.radialStartDistance * radVec), Quaternion.identity);
                float ar = a + bulletShooterData.offsetRotation; // + transform.eulerAngles.z;
                if (bd.GetComponent<primDecorationMoving>())
                {
                    bd.GetComponent<primDecorationMoving>().v = bulletData.speed * Rotate2(transform.right, ar);
                }
                else if (bd.GetComponent<Rigidbody2D>())
                {
                    StartCoroutine(RigidbodySetVelocity(bd.GetComponent<Rigidbody2D>(), bulletData.speed * Rotate2(transform.right, ar)));
                }
                if (rotateSpecialObject) { bd.transform.eulerAngles = new Vector3(0, 0, ar + transform.eulerAngles.z); }
                bd.SetActive(true);
                continue;
            }

            BulletObject b = new BulletObject();
            b.originPosition = transform.position + bulletShooterData.posOffset + (bulletShooterData.radialStartDistance * radVec);
            b.startingDirection = angleDeg + bulletShooterData.offsetRotationAfterPosition;
            b.startingVelocity = v;
            Texture binImg;
            InitBulletA(ref b, out binImg);
            BulletRegister.Register(ref b, bulletData.material, (binImg != null) ? binImg : bulletData.sprite.texture);
        }
    }

    public void FlowerShot()
    {

        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }


        for (float a = 0; a < 360 *bulletShooterData.flowerThetaRangeTimesPi; a += (360 * bulletShooterData.flowerThetaRangeTimesPi / bulletShooterData.numberInCircle))
        {
            float dist = (Mathf.Cos(Mathf.Deg2Rad * a * bulletShooterData.flowerConstant) + bulletShooterData.flowerOpenness) /2;

            BulletObject b = new BulletObject();
            b.originPosition = transform.position + bulletShooterData.posOffset;
            b.startingDirection = a+bulletShooterData.offsetRotation+transform.eulerAngles.z;
            b.startingVelocity = v * dist;
            b.startingTorque = bulletData.torque;
            b.acceleration = bulletData.acceleration;
            b.changeInTorque = bulletData.changeInTorque;
            b.deletTime = bulletData.deletTime;
            b.isTorqueSineWave = bulletData.isTorqueSineWave;
            b.isAccelerationMultiplicative = bulletData.isAccelerationMultiplicative;
            b.atWhatDistanceFromCenterIsAHit = 8f;
            b.simulationSpeedMult = bulletData.simulationSpeedMult;
            b.damage = bulletData.damage;
            b.killRadiusRatio = bulletData.killRadiusRatio;

            //b.position = b.originPosition;
            //b.rotationDegrees = 0;
            //b.scale = bulletData.scale;
            b.UpdateTransform(b.originPosition, 0, bulletData.scale);
            b.squareHitbox = bulletData.squareHitbox;
            b.rotateWithMovementAngle = bulletData.rotateSprite;
            b.destroyOnLeaveScreen = bulletData.destroyOffScreen;

            if (rainbowBullets)
            {
                b.color = Color.HSVToRGB((float)((DoubleTime.ScaledTimeSinceLoad/4)%1),1,1);
            }
            if (!rainbowBullets)
            {
                b.color = bulletData.color;
            }
            if (bulletData.customSprite)
            {
                //bulletObj.GetComponent<SpriteRenderer>().sprite = bulletData.sprite;
            }
            //Instantiate(bulletObj, transform.position + bulletShooterData.posOffset, Quaternion.identity);
            AddToBulletTracker(b);
            BulletRegister.Register(ref b, bulletData.material, bulletData.sprite.texture);
        }
    }

    public void Heartbreaker(float deletTime, Vector3 pos, Color color, float offsetRotation, float speed, int number, float torque, float acceleration, float changeInTorque, bool isAccelerationMultiplicative, bool isTorqueSineWave, float damage)
    {
        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }

        for (float aa = 0; aa < 360; aa += (360 / number))
        {
            float a = Mathf.Deg2Rad * aa;

            float partA = Mathf.Pow(System.Math.Abs(Mathf.Cos(a)), 0.5f);
            float partB = Mathf.Sin(a) + 1.9f;

            float dist = Mathf.Sin(a) * (partA / partB) - Mathf.Sin(a) + 1;

            BulletObject b = new BulletObject();
            b.originPosition = pos;
            b.startingDirection = aa + offsetRotation + transform.eulerAngles.z;
            b.startingVelocity = v * dist;
            b.startingTorque = torque;
            b.acceleration = acceleration;
            b.changeInTorque = changeInTorque;
            b.deletTime = deletTime;
            b.isTorqueSineWave = isTorqueSineWave;
            b.isAccelerationMultiplicative = isAccelerationMultiplicative;
            b.atWhatDistanceFromCenterIsAHit = 8f;
            b.simulationSpeedMult = bulletData.simulationSpeedMult;
            b.damage = damage;
            b.squareHitbox = bulletData.squareHitbox;
            b.rotateWithMovementAngle = bulletData.rotateSprite;
            b.killRadiusRatio = bulletData.killRadiusRatio;
            b.destroyOnLeaveScreen = bulletData.destroyOffScreen;

            b.UpdateTransform(b.originPosition, 0, bulletData.scale);
            //bulletObj.GetComponent<SpriteRenderer>().color = color;
            //Instantiate(bulletObj, pos, Quaternion.identity);
            AddToBulletTracker(b);
            BulletRegister.Register(ref b, bulletData.material, bulletData.sprite.texture);
        }
    }

    public void Polygonal(bool isItStar)
    {
        float fullCircle = 1;
        if (isItStar)
        {
            fullCircle = 2;
        }

        float v = bulletData.speed;
        if (bulletShooterData.speedVariation > 0.001f) { v += (Fakerand.Single() * 2f - 1f) * bulletShooterData.speedVariation; }

        for (float aa = 0; aa < 360*fullCircle; aa += (360 / bulletShooterData.numberInCircle))
        {
            float a = Mathf.Deg2Rad * aa;
            float pi = Mathf.PI;
            float sides = bulletShooterData.polygonSides;
            float partA = Mathf.Cos(pi/ sides);
            float partB = Mathf.Cos(a - (((2 * pi) / sides) * Mathf.Floor(((sides * a) + pi) / (2 * pi))));

            float dist = partA / partB;
            BulletObject b = new BulletObject();
            b.originPosition = transform.position + bulletShooterData.posOffset;
            b.startingDirection = (a * Mathf.Rad2Deg) + bulletShooterData.offsetRotation + transform.eulerAngles.z;
            b.startingVelocity = v * dist;
            b.startingTorque = bulletData.torque;
            b.acceleration = bulletData.acceleration;
            b.changeInTorque = bulletData.changeInTorque;
            b.isTorqueSineWave = bulletData.isTorqueSineWave;
            b.deletTime = bulletData.deletTime;
            b.isAccelerationMultiplicative = bulletData.isAccelerationMultiplicative;
            b.atWhatDistanceFromCenterIsAHit = 8f;
            b.simulationSpeedMult = bulletData.simulationSpeedMult;
            b.damage = bulletData.damage;
            b.squareHitbox = bulletData.squareHitbox;
            b.rotateWithMovementAngle = bulletData.rotateSprite;
            b.killRadiusRatio = bulletData.killRadiusRatio;
            b.destroyOnLeaveScreen = bulletData.destroyOffScreen;


            b.UpdateTransform(b.originPosition, 0, bulletData.scale);

            if (rainbowBullets)
            {
                b.color = Color.HSVToRGB((float)((DoubleTime.ScaledTimeSinceLoad / 4) % 1), 1, 1);
            }
            if (!rainbowBullets)
            {
                b.color = bulletData.color;
            }
            if (bulletData.customSprite)
            {
                //bulletObj.GetComponent<SpriteRenderer>().sprite = bulletData.sprite;
            }
            //GameObject b2 = (GameObject)Instantiate(bulletObj, transform.position + bulletShooterData.posOffset, Quaternion.identity);
            //b2.transform.parent = transform;
            AddToBulletTracker(b);
            BulletRegister.Register(ref b, bulletData.material, bulletData.sprite.texture);
        }

    }

}
