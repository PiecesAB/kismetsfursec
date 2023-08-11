using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflyObstacle : GenericBlowMeUp
{
    private Rigidbody2D r2;

    public float baseVelocity = 90;
    public Animator bodyAnim;
    public Transform body;
    public float speed = 90f;
    public float initialAngleDegrees = 0f;
    public float angleChange = 0f;
    public bool randomAngleChangeDirection = true;
    public bool moveOffScreen;
    public SpriteRenderer offScreenChecker;
    private Vector2 prevDir = Vector2.right;

    void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        r2.velocity = new Vector2(speed * Mathf.Cos(initialAngleDegrees * Mathf.Deg2Rad),
            speed * Mathf.Sin(initialAngleDegrees * Mathf.Deg2Rad)); //test
        if (randomAngleChangeDirection)
        {
            angleChange *= Fakerand.Int(0, 2) * 2 - 1;
        }
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        speed *= Mathf.Pow(1.5f, ohi.amt);
        angleChange *= Mathf.Pow(1.5f, ohi.amt);
        Vector3 lpos = transform.InverseTransformPoint(ohi.pos);
        r2.velocity = -lpos.normalized * speed;
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (!moveOffScreen && !offScreenChecker.isVisible) {
            prevDir = (r2.velocity.magnitude > 0) ? r2.velocity.normalized : (Vector2)transform.right;
            return;
        }
        bodyAnim.speed = r2.velocity.magnitude / baseVelocity;
        if (r2.velocity.magnitude == 0)
        {
            r2.velocity = prevDir * speed;
        }
        else if (Mathf.Abs(r2.velocity.magnitude - speed) > 0.1f)
        {
            r2.velocity = r2.velocity.normalized * speed;
        }
        body.localEulerAngles = new Vector3(15, Mathf.LerpAngle(body.localEulerAngles.y, (r2.velocity.x > 0) ? 90 : -90, 0.25f));

        if (angleChange != 0f)
        {
            float angle = Mathf.Atan2(r2.velocity.y, r2.velocity.x);
            angle += angleChange * Time.timeScale * 0.016666666f * Mathf.Deg2Rad;
            r2.velocity = speed * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}
