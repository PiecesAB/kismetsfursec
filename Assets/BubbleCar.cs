using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleCar : GenericBlowMeUp
{
    public float speed = 60f;
    public float speedDangerIncrease = 20f;
    public float stopTime = 1f;
    public float[] distanceBetweenShotsSchedule = new float[1] { 40f };
    public double lastStopPoint = -5.0;
    public Renderer mainRenderer;
    public Renderer bulletWand;
    public Renderer visibleTest;
    [Header("This shooter is not part of the model, it's in the level")]
    public BulletHellMakerFunctions bulletShooter;
    public bool enableShootWhileMoveInstead = false;
    public Transform bulletShooterPositioner;
    public bool destroyOffScreen = false;

    private float shotPedometer = 0f;
    private int shotIndex = 0;

    private Rigidbody2D r2;
    private AudioSource sound;

    private bool db = false;

    private void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        sound = GetComponent<AudioSource>();
        bulletWand.enabled = (bulletShooter != null);
        ToggleLight(speed);
    }

    private IEnumerator StopAndReverse()
    {
        float oldSpeed = speed;
        speed = 0f;
        lastStopPoint = DoubleTime.ScaledTimeSinceLoad;
        if (stopTime > 0f) { yield return new WaitForSeconds(stopTime*0.5f); }
        ToggleLight(-oldSpeed);
        if (stopTime > 0f) { yield return new WaitForSeconds(stopTime*0.5f); }
        speed = -oldSpeed;
        db = false;
        yield return null;
    }

    private void ToggleLight(float s)
    {
        Vector3 v = mainRenderer.transform.localScale;
        mainRenderer.transform.localScale = (s > 0)?Vector3.one:new Vector3(-1,1,1);
    }

    private void ColCheck(Collision2D col)
    {
        if (db) { return; }
        if (Vector2.Dot(col.GetContact(0).normal,Mathf.Sign(speed)*transform.right) < -0.7f)
        {
            db = true;
            sound.Stop();
            sound.pitch = 1f / stopTime;
            sound.Play();
            StartCoroutine(StopAndReverse());
        }
    }

    private IEnumerator IncreaseSpeed(float t)
    {
        yield return new WaitUntil(() => speed != 0);
        float oldSpeed = speed;
        speed += Mathf.Sign(speed)*t;
        stopTime *= oldSpeed / speed;
        yield return null;
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        StartCoroutine(IncreaseSpeed(speedDangerIncrease * ohi.amt));
        if (Vector2.Dot(ohi.pos - transform.position, Mathf.Sign(speed) * transform.right) > 0f)
        {
            speed *= -1;
            ToggleLight(speed);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        ColCheck(col);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        ColCheck(col);
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (destroyOffScreen && !visibleTest.isVisible) { Destroy(gameObject); return; }

        r2.velocity -= Vector2.Dot(transform.right, r2.velocity) * (Vector2)transform.right;
        r2.velocity = r2.velocity + (Vector2)(speed * transform.right) + (Vector2)(Physics2D.gravity.y * transform.up);

        if (bulletShooter != null)
        {
            if (enableShootWhileMoveInstead)
            {
                bulletShooter.enabled = visibleTest.isVisible && Mathf.Abs(speed) > 0.01f;
            }
            else
            {
                shotPedometer += Mathf.Abs(speed) * Time.timeScale * 0.016666666666666f;
                while (shotPedometer >= distanceBetweenShotsSchedule[shotIndex])
                {
                    if (visibleTest.isVisible)
                    {
                        bulletShooter.transform.position = bulletShooterPositioner.position;
                        bulletShooter.transform.eulerAngles = mainRenderer.transform.eulerAngles;
                        // fire to the left when moving right
                        if (speed < 0) { bulletShooter.transform.eulerAngles += new Vector3(0, 0, 180); }
                        bulletShooter.Fire();
                    }
                    shotPedometer -= distanceBetweenShotsSchedule[shotIndex];
                    shotIndex = (shotIndex + 1) % distanceBetweenShotsSchedule.Length;
                }
            }
        }

        float elap = (float)(DoubleTime.ScaledTimeSinceLoad - lastStopPoint);
        if (elap < stopTime)
        {
            mainRenderer.transform.localEulerAngles = new Vector3(-90, (elap / stopTime) * 360, 0);
        }
        else
        {
            mainRenderer.transform.localEulerAngles = new Vector3(-90, 0, 0);
        }
    }
}
