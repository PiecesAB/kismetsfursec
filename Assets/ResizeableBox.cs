using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeableBox : MonoBehaviour
{
    public enum MovementMode
    {
        Never,Alternate
    }

    private SpriteRenderer sr;
    private BoxCollider2D left;
    private BoxCollider2D bottom;
    private BoxCollider2D right;
    private BoxCollider2D top;
    public Vector4 edges = Vector4.zero;
    public Vector2[] extraResizeJunk;
    public double[] extraResizeDelays;
    public float extraResizeSpeed = 2f;
    public MovementMode mode = MovementMode.Never;
    private Vector2 cSize = Vector2.zero;
    private double t;
    private int i;
    public bool activeMovement = true;

    void AdjustColliders()
    {
        cSize = sr.size;
        left.offset = new Vector2((-cSize.x + edges.x) * 0.5f, 0f);
        left.size = new Vector2(edges.x, sr.size.y);
        right.offset = new Vector2((cSize.x - edges.z) * 0.5f, 0f);
        right.size = new Vector2(edges.z, sr.size.y);
        top.offset = new Vector2(0f, (cSize.y - edges.w) * 0.5f);
        top.size = new Vector2(sr.size.x, edges.w);
        bottom.offset = new Vector2(0f, (-cSize.y + edges.y) * 0.5f);
        bottom.size = new Vector2(sr.size.x, edges.y);
    }

    /*GameObject NewGameObjCollider()
    {
        GameObject g = new GameObject();
        g.transform.SetParent(transform);
        g.transform.localPosition = Vector3.zero;
        Rigidbody2D r2 = g.AddComponent<Rigidbody2D>();
        r2.mass = 1000000f;
        r2.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2.interpolation = RigidbodyInterpolation2D.Interpolate;
        r2.gravityScale = 0f;
        r2.constraints = RigidbodyConstraints2D.FreezeAll;
        return g;
    }*/

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        left = gameObject.AddComponent<BoxCollider2D>();
        right = gameObject.AddComponent<BoxCollider2D>();
        top = gameObject.AddComponent<BoxCollider2D>();
        bottom = gameObject.AddComponent<BoxCollider2D>();

        i = 0;
        t = DoubleTime.ScaledTimeSinceLoad + extraResizeDelays[0];
        AdjustColliders();
        if (mode == MovementMode.Never)
        {
            Destroy(this);
        }
    }

    void Update()
    {
        if (activeMovement)
        {
            if (mode == MovementMode.Alternate)
            {
                while (t <= DoubleTime.ScaledTimeSinceLoad)
                {
                    if (++i >= extraResizeDelays.Length) { i = 0; }
                    t += extraResizeDelays[i];
                }
                Vector2 targetSize = extraResizeJunk[i];
                if (sr.size != targetSize)
                {
                    sr.size = new Vector2(Mathf.MoveTowards(sr.size.x, targetSize.x, extraResizeSpeed * Time.timeScale),
                                          Mathf.MoveTowards(sr.size.y, targetSize.y, extraResizeSpeed * Time.timeScale));
                    sr.material.SetFloat("_TOH", -0.15f * Mathf.Clamp01(Mathf.Sin((float)(DoubleTime.UnscaledTimeSinceLoad % 0.4) * Mathf.PI * 5f)));
                }
                else
                {
                    sr.material.SetFloat("_TOH", 0f);
                }
            }
        }
        else
        {
            t += 0.0166666666666666667;
        }

        if (sr.size != cSize)
        {
            AdjustColliders();
        }
    }
}
