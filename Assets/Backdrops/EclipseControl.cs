using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// branch cut: -pi to pi
public class EclipseControl : MonoBehaviour
{
    private const float moonRadius = 18f;
    private const float sunRadius = 18f;

    public Transform tsun;
    public SpriteRenderer sunSpr;
    public SpriteRenderer moonSpr;
    public Transform tmoon;
    public bool testUpdate = false;

    private Vector2 RayClosestPoint(Vector2 dir, Vector2 moonrpos)
    {
        return Vector2.Dot(dir, moonrpos) * dir;
    }

    private bool SunRayIntersects(Vector2 dir, Vector2 moonrpos)
    {
        return Vector2.Distance(RayClosestPoint(dir, moonrpos), moonrpos) <= moonRadius;
    }

    private bool SunRayIntersects(Vector2 dir, Vector2 sun, Vector2 moon)
    {
        if ((moon - sun).magnitude < moonRadius) { return true; }
        return SunRayIntersects(dir, moon - sun);
    }

    private Vector2 FindBorder()
    {
        if (Vector2.Distance(tmoon.position, tsun.position) < 1f)
        {
            return new Vector2(999, -999);
        }

        float center = Mathf.Atan2(tmoon.position.y - tsun.position.y, tmoon.position.x - tsun.position.x);
        float low = center;
        float lowstep = -0.15f;
        float high = center;
        float highstep = 0.15f;

        if (Vector2.Distance(tmoon.position, tsun.position) < sunRadius + 0.1f)
        {
            low = center - 0.5f * Mathf.PI;
            high = center + 0.5f * Mathf.PI;
            low = Mathf.Repeat(low + Mathf.PI, Mathf.PI * 2) - Mathf.PI;
            high = Mathf.Repeat(high + Mathf.PI, Mathf.PI * 2) - Mathf.PI;
            return new Vector2(low, high);
        }

        // change low
        for (int i = 0; i < 40; ++i)
        {
            Vector2 ld = new Vector2(Mathf.Cos(low), Mathf.Sin(low));
            Vector2 lsp = (Vector2)tsun.position + sunRadius * ld;
            low += lowstep;
            lowstep *= 1.1f;
            if (lowstep <= 0f)
            {
                if (!SunRayIntersects(ld, lsp, tmoon.position)) { lowstep *= -0.3f; }
            }
            else
            {
                if (SunRayIntersects(ld, lsp, tmoon.position)) { lowstep *= -0.3f; }
            }
        }

        // change high
        for (int i = 0; i < 40; ++i)
        {
            Vector2 ud = new Vector2(Mathf.Cos(high), Mathf.Sin(high));
            Vector2 usp = (Vector2)tsun.position + sunRadius * ud;
            high += highstep;
            highstep *= 1.1f;
            if (highstep >= 0f)
            {
                if (!SunRayIntersects(ud, usp, tmoon.position)) { highstep *= -0.3f; }
            }
            else
            {
                if (SunRayIntersects(ud, usp, tmoon.position)) { highstep *= -0.3f; }
            }
        }

        low = Mathf.Repeat(low + Mathf.PI, Mathf.PI * 2) - Mathf.PI;
        high = Mathf.Repeat(high + Mathf.PI, Mathf.PI * 2) - Mathf.PI;

        return new Vector2(low, high);
    }

    private void RealUpdate()
    {
        Vector2 border = FindBorder();
        sunSpr.material.SetVector("_Bounds", border);
        Vector2 dif = tmoon.position - tsun.position;
        float dl = dif.magnitude;
        float da = Mathf.Atan2(dif.y, dif.x) + Mathf.PI;
        moonSpr.material.SetFloat("_LAngle", da);
        moonSpr.material.SetColor("_ACol", ((dl < 36f) ? (dl / 36f) : (36f / dl)) * Color.white);
        moonSpr.material.SetColor("_BCol", ((dl < 48f) ? (1f - dl / 48f) : 0f) * Color.white);
    }

    private void Start()
    {
        RealUpdate();
        if (!testUpdate || !Application.isEditor)
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        RealUpdate();
    }
}
