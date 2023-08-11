using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialBase : MonoBehaviour
{
    public Transform rotor;

    [HideInInspector]
    public float dx;
    public float multiplier = 1f;
    public float rotorSpeedMult = 1f;
    public float minValue;
    public float maxValue;
    public float startAngle = 90f;
    public bool autoRound = false;
    public AudioSource rotatingSound;
    public BulletHellMakerFunctions shooter;
    public float shooterChangeToFire = 1f;
    private float shooterCooldown;
    [Header("-----")]
    public float myValue;

    protected BasicMove lockedPlayer;
    protected Encontrolmentation lockedControl;

    protected int releaseCooldown;

    protected float radius;

    protected virtual void ChildStart()
    {

    }

    protected virtual void ChildUpdate()
    {

    }


    private void Start()
    {
        releaseCooldown = 0;
        dx = 0;
        rotor.eulerAngles = new Vector3(0, 0, startAngle);
        radius = GetComponent<CircleCollider2D>().radius + 4;
        if (shooter) { shooterCooldown = shooterChangeToFire; }
        ChildStart();
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        ColCheck(col);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        ColCheck(col);
    }

    protected void ReleasePlayer()
    {
        if (lockedPlayer == null) { return; }
        lockedPlayer.fakePhysicsVel = new Vector2(0f, lockedPlayer.fakePhysicsVel.y);
        lockedPlayer = null;
        
        lockedControl = null;
        releaseCooldown = 4;
    }

    protected void ColCheck(Collision2D col)
    {
        if (lockedPlayer) { return; }
        if (releaseCooldown > 0) { return; }
        if (col.gameObject.layer != 20) { return; }
        BasicMove bm = col.gameObject.GetComponent<BasicMove>();
        if (!bm) { return; }
        Vector3 fakeUp = col.gameObject.transform.up * ((col.gameObject.transform.localScale.y > 0) ? 1f : -1f);
        if (Vector2.Dot(-fakeUp, col.GetContact(0).normal.normalized) < 0.9f) { return; }
        lockedPlayer = bm;
        lockedControl = bm.GetComponent<Encontrolmentation>();
    }

    private void Update()
    {
        
        releaseCooldown = (releaseCooldown <= 0) ? 0 : (releaseCooldown - 1);

        if (!lockedPlayer && rotatingSound)
        {
            rotatingSound.Stop();
        }
        if (!lockedPlayer || Time.timeScale == 0) { return; }

        Transform plrtr = lockedPlayer.transform;
        Vector3 fakeUp = plrtr.up * ((plrtr.localScale.y > 0) ? 1f : -1f);
        Vector3 dialPos = transform.position + fakeUp * (radius - 4f + 18f * Mathf.Abs(plrtr.localScale.y));
        dialPos = new Vector3(dialPos.x, dialPos.y, plrtr.position.z);
        plrtr.position = dialPos;

        dx = (lockedPlayer.fakePhysicsVel.x / radius) * Mathf.Rad2Deg * Time.deltaTime * -0.5f * Mathf.Abs(plrtr.localScale.x);

        if (rotatingSound) {
            rotatingSound.pitch = Mathf.Max(0.6f, Mathf.Log10(Mathf.Abs(dx)));
            rotatingSound.volume = Mathf.Max(0.2f, Mathf.Abs(dx) / 60f);
            if (Mathf.Abs(dx) >= 1f && !rotatingSound.isPlaying)
            {
                rotatingSound.Play();
            }
            else if (Mathf.Abs(dx) < 1f && rotatingSound.isPlaying)
            {
                rotatingSound.Stop();
            }
        }

        if (shooter)
        {
            shooterCooldown -= Mathf.Abs(dx);
            bool fired = false;
            while (shooterCooldown < 0f)
            {
                shooterCooldown += shooterChangeToFire;
                if (!fired) { shooter.Fire(); fired = true; }
            }
        }

        float newVal = Mathf.Clamp(myValue + (multiplier * dx), minValue, maxValue);
        if (autoRound && Mathf.Abs(dx) < 0.1f)
        {
            newVal = Mathf.Round(newVal);
        }

        //correct for clamped value
        if (multiplier != 0) { dx = (newVal - myValue) / multiplier; }

        if (newVal != myValue)
        {
            rotor.eulerAngles += new Vector3(0, 0, -dx*rotorSpeedMult);
            myValue = newVal;
        }

        if (lockedControl.ButtonDown(16UL, 16UL) || !lockedPlayer.CanCollide)
        {
            ReleasePlayer();
        }

        ChildUpdate();
    }
}
