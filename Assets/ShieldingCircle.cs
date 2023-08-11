using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldingCircle : MonoBehaviour
{
    public float radius;

    public static HashSet<ShieldingCircle> all = new HashSet<ShieldingCircle>();

    private SpriteRenderer sr;
    public Transform t;
    private const float spriteRadius = 64;
    public bool invisible;
    public float shrinkSelfSpeed = 0f;
    public bool placedBehaviour = false;
    // how long until it stops clearing bullets
    // if starts negative, it will clear bullets forever
    public double clearBulletsTime = -1.0;
    public Vector2 fakeVelocity;
    private Vector2 lastPos;

    private void Start()
    {
        all.Add(this);
        t = transform;
        sr = GetComponent<SpriteRenderer>();
        if (invisible) { sr.color = Color.clear; }
        if (placedBehaviour)
        {
            radius = t.localScale.x * spriteRadius;
        }
        else
        {
            lastPos = transform.position;
        }
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    private void Update()
    {
        t.localScale = Vector3.one * (radius / spriteRadius);
        if (clearBulletsTime > 0.0)
        {
            clearBulletsTime -= Time.timeScale * 0.0166666666666666f;
            if (clearBulletsTime <= 0.0)
            {
                clearBulletsTime = 0.0;
                all.Remove(this);
                sr.color = new Color(sr.color.r * 0.5f, sr.color.g, sr.color.b * 0.5f, sr.color.a);
            }
        }
        if (invisible) { sr.color = Color.clear; }
        if (shrinkSelfSpeed > 0f && Time.timeScale > 0f)
        {
            radius -= shrinkSelfSpeed * Time.deltaTime;
            if (radius <= 0f) { Destroy(gameObject); }
        }

        if (placedBehaviour)
        {
            if (sr.isVisible && !all.Contains(this)) { all.Add(this); }
            if (!sr.isVisible && all.Contains(this)) { all.Remove(this); }
        }
        else if (Time.timeScale > 0)
        {
            fakeVelocity = ((Vector2)transform.position - lastPos) / Time.deltaTime;
            lastPos = transform.position;
        }
    }
}
