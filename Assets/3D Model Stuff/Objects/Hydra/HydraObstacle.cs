using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraObstacle : GenericBlowMeUp
{
    private Rigidbody2D r2;

    public float baseVelocity = 48;
    public Animator bodyAnim;
    public Transform head;
    public float speed = 90f;
    public float initialAngleDegrees = 0f;

    public static GameObject commonClone = null;

    private float lerpAngle = 0f;

    private bool makingSure = false;
    private IEnumerator MakeSureToBlowUp()
    {
        HydraHead hh = head.GetComponentInChildren<HydraHead>();
        if (makingSure || hh.honking) { yield break; }
        makingSure = true;
        yield return new WaitForEndOfFrame();
        if (!hh.honking) { base.BlowMeUp(); }
        makingSure = false;
    }

    public override void BlowMeUp()
    {
        StartCoroutine(MakeSureToBlowUp());
    }

    void Start()
    {
        makingSure = false;
        if (commonClone == null)
        {
            commonClone = Instantiate(gameObject, transform.position, transform.rotation, null);
            commonClone.SetActive(false);
        }
        r2 = GetComponent<Rigidbody2D>();
        r2.velocity = new Vector2(speed * Mathf.Cos(initialAngleDegrees * Mathf.Deg2Rad), 
            speed * Mathf.Sin(initialAngleDegrees * Mathf.Deg2Rad)); //test
        lerpAngle = transform.eulerAngles.z;
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        bodyAnim.speed = r2.velocity.magnitude / baseVelocity;
        float angle = Mathf.Atan2(r2.velocity.y, r2.velocity.x) * Mathf.Rad2Deg;
        lerpAngle = Mathf.LerpAngle(lerpAngle, angle, 0.25f);
        r2.SetRotation(lerpAngle);
        if (r2.velocity.magnitude == 0)
        {
            r2.velocity = transform.right * speed;
        }
        else if (Mathf.Abs(r2.velocity.magnitude - speed) > 0.1f)
        {
            r2.velocity = r2.velocity.normalized * speed;
        }

        if (!LevelInfoContainer.GetActiveControl()) { return; }
        Vector3 target = LevelInfoContainer.GetActiveControl().transform.position;
        float headAngle = Mathf.Atan2(target.y - head.position.y, target.x - head.position.x) * Mathf.Rad2Deg;
        head.eulerAngles = new Vector3(0, 0, headAngle);
        float clampHeadAngle = (head.localEulerAngles.z + 1080f) % 360f;
        if (clampHeadAngle > 180f) { clampHeadAngle -= 360f; }
        head.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(clampHeadAngle, -75f, 75f));
    }
}
