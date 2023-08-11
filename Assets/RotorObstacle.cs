using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotorObstacle : MonoBehaviour
{
    private Rigidbody2D r2;
    private CircleCollider2D cc2;
    private ConstantForce2D cf2;

    public Transform face;
    public bool reverseOnWallHit;
    public Transform eye;
    public Renderer destroyOnInvisible;
    public bool phaseThroughBeams = false;

    private int reverseGrace = 0;
    private int destroyGrace = 90;

    private AudioSource aud;
    private AudioSource hitSound;

    void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        cc2 = GetComponent<CircleCollider2D>();
        cf2 = GetComponent<ConstantForce2D>();
        aud = GetComponent<AudioSource>();
        hitSound = transform.Find("HitSound").GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 20)
        {
            Physics2D.IgnoreCollision(col, cc2);
            foreach (Collider2D c in col.GetComponentsInChildren<Collider2D>())
            {
                if (c.gameObject.layer == 19 || c.gameObject.layer == 20)
                {
                    Physics2D.IgnoreCollision(c, cc2);
                }
            }
        }
        else if (col.gameObject.layer == 11)
        {
            if (phaseThroughBeams) { Physics2D.IgnoreCollision(col, cc2); }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (reverseOnWallHit 
            && reverseGrace <= 0
            && (col.gameObject.layer == 8 || col.gameObject.layer == 9 || col.gameObject.layer == 11)
            && Vector2.Dot(cf2.force.normalized, col.GetContact(0).normal) < -0.85f)
        {
            cf2.force = new Vector2(-cf2.force.x, cf2.force.y);
            reverseGrace = 5;
        }
    }

    private Vector2 prevVel = Vector2.negativeInfinity;

    void Update()
    {
        if (Time.timeScale == 0) { return; }

        r2.angularVelocity = -Mathf.Rad2Deg * r2.velocity.x / cc2.radius;
        face.rotation = Quaternion.identity;

        if (destroyOnInvisible && !destroyOnInvisible.isVisible && destroyGrace <= 0)
        {
            Destroy(gameObject);
        }

        if (eye)
        {
            Encontrolmentation e = LevelInfoContainer.GetActiveControl();
            if (e)
            {
                Vector2 look = (e.transform.position - eye.position) / 16f;
                if (look.magnitude > 3f) { look = look.normalized * 3f; }
                eye.localPosition = look;
            }
        }

        float rMag = (prevVel - r2.velocity).magnitude;
        if (prevVel.x > -1e6 && rMag > 20f)
        {
            hitSound.Stop();
            hitSound.volume = Mathf.Clamp01(rMag * 0.01f);
            hitSound.Play();
        }
        prevVel = r2.velocity;
        if (r2.velocity.magnitude >= 20)
        {
            aud.volume = Mathf.Min(0.25f, 0.14f * Mathf.Log10(r2.velocity.magnitude));
            aud.pitch = 0.4f + 0.1f * Mathf.Log10(r2.velocity.magnitude);
        }
        else
        {
            aud.volume = 0f;
        }

        if (reverseGrace > 0) { --reverseGrace; }
        if (destroyGrace > 0) { --destroyGrace; }
    }
}
