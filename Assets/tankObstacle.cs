using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tankObstacle : GenericBlowMeUp
{
    public BulletHellMakerFunctions bulletShooter;

    private int direction; //-1 left, 1 right
    private int currentMoveDir;
    private bool changedDirThisFrame;
    private bool tankStoppedThisFrame;
    private bool dontMoveThisFrame;
    private bool didntMoveThisFrameForUpdate;
    private Rigidbody2D rg2;
    private SkinnedMeshRenderer smr;

    public Collider2D directionChangeTrigger;
    public Collider2D insideGroundCollider;
    public GameObject destroyEffect;
    public MeshRenderer meshToAnimate;
    public Transform gunTransform;

    private const float trigOffsetX = 32f;
    private const float trigOffsetY = 16f;
    private const float moveSpeed = 30f;

    private float tread;
    private float pitch;

    private Vector2 internalVel;

    private double nextShotTime;
    private int nextShotSeriesCount;
    private double nextShotSeriesInterval;
    private Color nextShotColor;

    private const float gunLength = 32f;

    private static readonly double[] shotInfo1 = new double[]{ 0.6, 0.8, 1.0, 1.2 };
    private static readonly double[] shotInfo2 = new double[]{ 0.1, 0.2, 0.3, 0.4 };

    /*public void BlowMeUp()
    {
        if (destroyEffect)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }*/

    public void CalcNextShotStuff()
    {
        nextShotTime += shotInfo1[Fakerand.Int(0, shotInfo1.Length)];
        nextShotSeriesCount = Fakerand.Int(1, 6);
        nextShotSeriesInterval = shotInfo2[Fakerand.Int(0, shotInfo1.Length)];
        nextShotColor = Color.HSVToRGB(Fakerand.Single(), 0.8f, 1f);
    }

    public void ShotLogic()
    {
        if (bulletShooter == null)
        {
            return;
        }

        double remainingTime = nextShotTime - DoubleTime.ScaledTimeSinceLoad;
        if (remainingTime <= 0f)
        {
            if (smr && smr.isVisible)
            {
                bulletShooter.Fire();
            }
            nextShotSeriesCount--;
            if (nextShotSeriesCount == 0)
            {
                CalcNextShotStuff();
            }
            else
            {
                nextShotTime += nextShotSeriesInterval;
            }
        }
        else if (remainingTime < 0.1)
        {
            smr.SetBlendShapeWeight(0, 100f - (float)(remainingTime * 1000.0));
        }
        else
        {
            smr.SetBlendShapeWeight(0, 0f);
        }
    }

    private void Start()
    {
        direction = -1;
        if (Mathf.Abs(Mathf.Repeat(transform.localEulerAngles.y,360f)-180f) < 90f)
        {
            direction = 1;
        }
        currentMoveDir = direction;
        changedDirThisFrame = false;
        directionChangeTrigger.offset = new Vector2(-trigOffsetX * currentMoveDir * direction, trigOffsetY);
        rg2 = GetComponent<Rigidbody2D>();
        tread = 0f;
        dontMoveThisFrame = didntMoveThisFrameForUpdate = tankStoppedThisFrame = false;
        nextShotTime = DoubleTime.ScaledTimeSinceLoad;
        smr = gunTransform.GetComponent<SkinnedMeshRenderer>();
        CalcNextShotStuff();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if ( ((1<<col.gameObject.layer)&(256+512+2048)) != 0 && !changedDirThisFrame)
        {
            changedDirThisFrame = true;
            currentMoveDir = -currentMoveDir;
            directionChangeTrigger.offset = new Vector2(-trigOffsetX * currentMoveDir * direction, trigOffsetY);
        }
    }

    private void OnTriggerStay2D(Collider2D col) //stop if the player is blocking the tank
    {
        if (!tankStoppedThisFrame && currentMoveDir == direction && col.gameObject.layer == 20)
        {
            dontMoveThisFrame = didntMoveThisFrameForUpdate = tankStoppedThisFrame = true;
            nextShotTime += Time.fixedDeltaTime*0.925;
        }
    }

    private void OnCollisionEnter2D(Collision2D col) //blow up when struck with too much non-player force, or tank is inside the ground
    {
        if (col.collider.gameObject.layer != 20 && col.collider.gameObject.layer != 19)
        {
            if (col.relativeVelocity.sqrMagnitude > 280f * 280f || col.otherCollider == insideGroundCollider)
            {
                BlowMeUp();
            }
        }
    }

    private const float fallVelLimit = -300f;

    private void FixedUpdate()
    {
        if (rg2)
        {
            float angle = transform.localEulerAngles.z * Mathf.Deg2Rad * ((direction == -1)?1f:-1f);
            float sa = Mathf.Sin(angle);
            float ca = Mathf.Cos(angle);

            internalVel = new Vector2(ca * rg2.velocity.x + sa * rg2.velocity.y, -sa * rg2.velocity.x + ca * rg2.velocity.y);

            if (dontMoveThisFrame)
            {
                internalVel = new Vector2(0f, internalVel.y);
                dontMoveThisFrame = false;
                didntMoveThisFrameForUpdate = true;
            }
            else
            {
                internalVel = new Vector2(moveSpeed * currentMoveDir, internalVel.y);
            }

            internalVel = new Vector2(internalVel.x, Mathf.Clamp(internalVel.y + Physics2D.gravity.y, fallVelLimit, -fallVelLimit));

            
            rg2.velocity = new Vector2(ca*internalVel.x - sa*internalVel.y, sa*internalVel.x + ca*internalVel.y);
        }
    }

    private static readonly int[] wheelRimMatIndex = new int[2] { 2,3 };

    private void Update()
    {
        if (Time.timeScale > 0)
        {
            float s = currentMoveDir * direction * -0.005f;
            if (didntMoveThisFrameForUpdate)
            {
                s = 0f;
            }
            tread += s*Time.timeScale;
            tread = Mathf.Repeat(tread, 1f);

            for (int i = 0; i < wheelRimMatIndex.Length; i++)
            {
                meshToAnimate.materials[wheelRimMatIndex[i]].SetVector("_DetailAlbedoMap_ST", new Vector4(1f, 1f, tread, 0f));
            }

            if (!didntMoveThisFrameForUpdate)
            {
                pitch += (1f + 3f * Mathf.Abs((float)System.Math.Cos(DoubleTime.ScaledTimeSinceLoad * 1.3f))) * Time.timeScale;
                pitch %= 180f;
                float gunAngle = -Mathf.PingPong(pitch, 90f);
                gunTransform.localEulerAngles = new Vector3(0, 0, gunAngle);
            }
            else
            {
                pitch = (pitch > 90) ? (180 - pitch) : pitch;
                pitch = Mathf.Lerp(pitch, 0, 0.2f * Time.timeScale);
                gunTransform.localEulerAngles = new Vector3(0, 0, -pitch);
            }

            ShotLogic();
            changedDirThisFrame = didntMoveThisFrameForUpdate = tankStoppedThisFrame = false;
        }
    }
}
