using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatchetPlatform : MonoBehaviour
{
    [SerializeField]
    private PrimBezierMove wire;
    public float maxPlatformSpeed = 200f;
    public float accel = 400f;
    public float stallTime = 0.5f;
    public AudioClip moveSound;
    public AudioClip stopSound;
    private int lastNode;
    private float currSpeed = 0f;
    private float currStallTime = 0f;
    private Vector2 lastV;

    private primDecorationMoving pdm;
    private SpriteRenderer sr;
    private AudioSource aso;

    private void Start()
    {
        lastNode = wire.lastNodes[0];
        pdm = GetComponent<primDecorationMoving>();
        sr = GetComponent<SpriteRenderer>();
        aso = GetComponent<AudioSource>();
        foreach (Collider2D d in GetComponents<Collider2D>()) // make box very slightly small to avoid catching
        {
            if (d.isTrigger) { continue; }
            if (!(d is BoxCollider2D)) { continue; }
            (d as BoxCollider2D).size = new Vector2((d as BoxCollider2D).size.x - 0.1f, (d as BoxCollider2D).size.y - 0.1f);
        }
    }

    public void TurnACorner()
    {
        lastNode = wire.lastNodes[0];
        currSpeed = 1e-5f;
        wire.ChangeObjectSpeed(0, currSpeed);
        currStallTime = stallTime;
        if (!pdm) { return; }
        pdm.v = Vector3.zero;
        pdm.SetPosition(pdm.transform.position, Vector3.zero);
        pdm.SetLateVelocitation(lastV);
        if (sr.isVisible)
        {
            FollowThePlayer.main.vibSpeed = 1.3f;
        }
        if (sr.isVisible && aso.clip != stopSound)
        {
            aso.Stop();
            aso.clip = stopSound;
            aso.volume = 0.15f;
            aso.pitch = 1.3f;
            aso.loop = false;
            aso.Play();
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<beamBlock>())
        {
            Collider2D[] mycols = GetComponents<Collider2D>();
            foreach (Collider2D c in col.gameObject.GetComponents<Collider2D>())
            {
                foreach (Collider2D d in mycols)
                {
                    Physics2D.IgnoreCollision(c, d);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0) { return; }
        float cs = currSpeed;
        if (Mathf.Abs(cs) < 1e-5f) { cs = Mathf.Sign(cs) * 1e-5f; }
        wire.ChangeObjectSpeed(0, cs);
        if (currStallTime <= 0f)
        {
            currSpeed += 0.01666666666f * Time.timeScale * accel;
            currSpeed = Mathf.Clamp(currSpeed, -maxPlatformSpeed, maxPlatformSpeed);
            if (aso.clip != moveSound)
            {
                aso.Stop();
                aso.clip = moveSound;
                aso.loop = true;
                aso.Play();
            }
            aso.volume = Mathf.Min(0.04f, currSpeed / 600f);
            aso.pitch = Mathf.Min(2f, currSpeed / 100f);
        }
        else
        {
            currStallTime -= 0.01666666666f * Time.timeScale;
            if (currStallTime < 0f) { currStallTime = 0f; }
        }
        lastV = pdm.v;
    }
}
