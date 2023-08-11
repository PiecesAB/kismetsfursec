using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerAttacks : MonoBehaviour
{
    public enum Attack
    {
        MedianOfThree // probably won't be given name, test attack
    }

    public Attack attack;
    public GameObject[] objects;
    public BulletData[] bullets;
    [HideInInspector]
    public BossController bossController;

    private Transform cameraTransform;

    private IEnumerator MedianOfThreeBullets(Vector3 laserStartPos, Vector3 dir, BulletData bData, float wait)
    {
        yield return new WaitForSeconds(wait);
        float arg = 0f;
        for (int i = 0; i <= 320; i += 24)
        {
            float bulletRot1 = (Mathf.Atan2(-dir.x, dir.y) + (Mathf.Cos(arg) * Mathf.PI * 0.25f)) * Mathf.Rad2Deg;
            float bulletRot2 = (Mathf.Atan2(dir.x, -dir.y) - (Mathf.Cos(arg) * Mathf.PI * 0.25f)) * Mathf.Rad2Deg;
            Vector2 thisBulletPos = laserStartPos + (i * dir);
            BulletObject b = new BulletObject();
            b.destroyOnLeaveScreen = true;
            b.doesntMoveOnItsOwn = false;
            InitializeBaseBulletInfo(ref b, thisBulletPos, bulletRot1, ref bData);
            b = new BulletObject();
            b.destroyOnLeaveScreen = true;
            b.doesntMoveOnItsOwn = false;
            InitializeBaseBulletInfo(ref b, thisBulletPos, bulletRot2, ref bData);

            arg += Mathf.PI * 0.08333333f;
        }

        yield return null;
    }

    private IEnumerator MedianOfThreeLaser(Transform laserTransform, Vector2 dir, BulletData bData)
    {
        Vector2 laserStartPos = laserTransform.position;
        float speed = 0.1f;
        float dist = 0f;
        while (laserTransform != null && dist < 320f)
        {
            laserTransform.position += (Vector3)dir * speed * Time.timeScale;
            dist += speed*Time.timeScale;
            speed = Mathf.MoveTowards(speed, 1.5f, 0.4f);
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    public void InitializeBaseBulletInfo(ref BulletObject b, Vector3 pos, float rot, ref BulletData data) // we can likely move and reuse this later to prevent spaghetti code
    {
        b.deletTime = data.deletTime;
        b.color = data.color;
        b.originPosition = pos;
        b.startingDirection = rot;
        b.startingVelocity = data.speed;
        b.startingTorque = data.torque;
        b.acceleration = data.acceleration;
        b.changeInTorque = data.changeInTorque;
        b.isAccelerationMultiplicative = data.isAccelerationMultiplicative;
        b.isTorqueSineWave = data.isTorqueSineWave;
        b.damage = data.damage;
        b.UpdateTransform(pos, rot, data.scale);
        b.killRadiusRatio = data.killRadiusRatio;
        b.rotateWithMovementAngle = data.rotateSprite;
        b.squareHitbox = data.squareHitbox;
        b.collisionDisabled = data.collisionDisabled;

        BulletRegister.Register(ref b, data.material, data.sprite.texture);
        b.renderGroup = new BulletControllerHelper.RenderGroup(b.materialInternalIdx, b.textureInternalIdx, b.color);
    }

    private IEnumerator MedianOfThree() // object 0 is a red laser
    {
        while (BossController.main == null) { yield return new WaitForEndOfFrame(); }
        bossController = BossController.main;

        GameObject redLaserSample = objects[0];
        GameObject blueLaserSample = objects[1];
        GameObject yellowLaserSample = objects[2];
        bool blueFlipped = (Fakerand.Int(0,2) == 0);

        float waitTime = 6f;

        while (gameObject != null)
        {
            Encontrolmentation e = LevelInfoContainer.GetActiveControl();
            if (e == null) { yield break; }
            Vector3 plrPos = e.transform.position;
            Bounds camRect = new Bounds(cameraTransform.position, new Vector3(320, 216, 1));

            Vector2 topPosition = camRect.ClosestPoint(plrPos + new Vector3(0, 1000, 0));
            GameObject redLaser = Instantiate(redLaserSample, new Vector3(topPosition.x, topPosition.y, -16), Quaternion.identity, transform);
            redLaser.SetActive(true);
            Vector2 redDir = (-topPosition + (Vector2)plrPos).normalized;
            StartCoroutine(MedianOfThreeLaser(redLaser.transform, redDir, bullets[0]));
            StartCoroutine(MedianOfThreeBullets(redLaser.transform.position, redDir, bullets[0], 2f));

            Vector2 rightPosition = camRect.ClosestPoint(plrPos + new Vector3(blueFlipped?-1000:1000, 0, 0));
            GameObject blueLaser = Instantiate(blueLaserSample, new Vector3(rightPosition.x, rightPosition.y, -16), Quaternion.identity, transform);
            blueLaser.SetActive(true);
            Vector2 blueDir = (-rightPosition + (Vector2)plrPos).normalized;
            StartCoroutine(MedianOfThreeLaser(blueLaser.transform, blueDir, bullets[1]));
            StartCoroutine(MedianOfThreeBullets(blueLaser.transform.position, blueDir, bullets[1], 3f));

            Vector2 randPart = 480*Fakerand.UnitCircle(true);
            //randPart = new Vector2(randPart.x, Mathf.Abs(randPart.y));
            Vector2 randPosition = camRect.ClosestPoint(plrPos + (Vector3)randPart);
            GameObject yellowLaser = Instantiate(yellowLaserSample, new Vector3(randPosition.x, randPosition.y, -16), Quaternion.identity, transform);
            Vector2 yellowDir = (-randPosition + (Vector2)plrPos).normalized;
            yellowLaser.SetActive(true);
            StartCoroutine(MedianOfThreeLaser(yellowLaser.transform, yellowDir, bullets[2]));
            StartCoroutine(MedianOfThreeBullets(yellowLaser.transform.position, yellowDir, bullets[2], 2.5f));

            blueFlipped = !blueFlipped;

            yield return new WaitForSeconds(waitTime);
            waitTime = Mathf.MoveTowards(waitTime, 3.5f, 0.55f);
        }

        yield return null;
    }

    void Start()
    {
        cameraTransform = Camera.main.transform;
        switch (attack)
        {
            case Attack.MedianOfThree:
                StartCoroutine(MedianOfThree());
                break;
        }
    }
}
