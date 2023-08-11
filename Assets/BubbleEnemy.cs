using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleEnemy : MonoBehaviour
{
    public Vector2 defaultSize = Vector2.one;
    public float variationInSize = 0.05f;
    private float randOffset;
    public float knockback;
    public bool touched;
    public ParticleSystem popEffect;
    public ParticleSystem beforePopParticles;
    public ParticleSystem afterPopParticles;

    void Start()
    {
        randOffset = Fakerand.Single(0f, 6.2831853f);
        touched = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 8 || col.gameObject.layer == 9 || col.gameObject.layer == 20)
        {
            Rigidbody2D r2 = col.GetComponent<Rigidbody2D>();
            if (r2 != null)
            {
                r2.velocity += (Vector2)(col.transform.position - transform.position).normalized * knockback;
            }
            BasicMove bm = col.GetComponent<BasicMove>();
            if (bm != null)
            {
                bm.fakePhysicsVel += (Vector2)(col.transform.position - transform.position).normalized * knockback;
            }
            GetComponent<AudioSource>().Play();
            Destroy(GetComponent<SpriteRenderer>());
            Destroy(GetComponent<CircleCollider2D>());
            Destroy(beforePopParticles);
            popEffect.Play();
            afterPopParticles.Play();
        }
    }

    void Update()
    {
        double t = 6.2831853f*(DoubleTime.ScaledTimeSinceLoad+randOffset);
        transform.localScale = defaultSize + variationInSize*(new Vector2((float)System.Math.Cos(t), (float)System.Math.Sin(t)));
    }
}
