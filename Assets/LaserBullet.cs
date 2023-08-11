using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBullet : MonoBehaviour
{

    // head of laser is transform's position, at start
    private Transform t;

    public LineRenderer lineRend;
    public float pointDist = 32;
    public Bezier startingBezier; // make sure the first point is 0,0,0
    public float damage;
    [Header("this is HALF the linerenderer's width.")]
    public float maxWidth;
    public float currWidth;

    public float warnTime;
    public float existTime;
    public float resizeSpeed;
    public float lineMatCycleSpeed = 0f;

    public bool specialTetraLaser = false;
    public static HashSet<LaserBullet> tetraLasers = new HashSet<LaserBullet>();

    public const float tetraLaserDist = 160f;

    private float grazeStore = 1;

    private List<Vector3> linePoints;

    private bool dirtyLine = false;

    private float distSinceLast = 0;

    private Vector3 lastPos;

    private bool activated = false;
    private double startTime;

    public bool positionChangedExternally = true;

    private void UpdateLine()
    {
        lineRend.positionCount = linePoints.Count;
        lineRend.SetPositions(linePoints.ToArray());
        lineRend.widthMultiplier = 1f;
        lineRend.widthCurve = AnimationCurve.Constant(0f, 1f, currWidth * 2f);
    }

    public void MakePoint()
    {
        distSinceLast = 0;
        linePoints[linePoints.Count - 1] = t.position;
        linePoints.Add(t.position);
    }

    public void MoveCursorRelative(Vector3 d)
    {
        if (!positionChangedExternally)
        {
            t.position += d;
        }
        distSinceLast += d.magnitude;
        if (distSinceLast >= pointDist)
        {
            MakePoint();
        }
        else
        {
            linePoints[linePoints.Count - 1] = t.position;
        }

        dirtyLine = true;
    }

    public void MoveCursorAbsolute(Vector3 d)
    {
        MoveCursorRelative(d - lastPos);
    }

    public bool TetraLaserBlockCollision(Vector2 blockPos, float blockRadius)
    {
        bool collided = false;

        // point check
        for (int i = 0; i < linePoints.Count; ++i)
        {
            Vector3 pt = linePoints[i];
            float dist = (blockPos - (Vector2)pt).magnitude;
            if (dist < currWidth + blockRadius) // within the circle of the point
            {
                collided = true;
                break;
            }
        }

        // segment check
        for (int i = 0; i < (startingBezier.loop ? (linePoints.Count) : (linePoints.Count - 1)); ++i)
        {
            Vector2 seg = linePoints[(i + 1) % linePoints.Count] - linePoints[i];
            Vector2 segNorm = seg.normalized;
            Vector2 plrPosL = blockPos - (Vector2)linePoints[i];
            float proj = Vector2.Dot(segNorm, plrPosL);
            float perp = (plrPosL - (proj * segNorm)).magnitude;
            if (proj > 0 && proj < seg.magnitude)
            {
                if (Mathf.Abs(perp) < currWidth + blockRadius)
                {
                    collided = true;
                    break;
                }
            }
        }

        return collided;
    }

    private void CheckCollision()
    {
        if (damage <= 0) { return; }

        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (e == null) { return; }
        Vector3 plrPos = e.transform.position;
        bool collided = false;
        bool grazed = false;

        // point check
        for (int i = 0; i < linePoints.Count; ++i)
        {
            Vector3 pt = linePoints[i];
            float dist = ((Vector2)(plrPos - pt)).magnitude;
            if (dist < currWidth - 4f && currWidth > maxWidth * 0.9f) // within the circle of the point
            {
                collided = true;
                break;
            }
            else if (dist < currWidth + 16) // grazing distance
            {
                grazed = true;
            }
        }

        // segment check
        for (int i = 0; i < (startingBezier.loop?(linePoints.Count):(linePoints.Count - 1)); ++i)
        {
            Vector2 seg = linePoints[(i + 1) % linePoints.Count] - linePoints[i];
            Vector2 segNorm = seg.normalized;
            Vector2 plrPosL = plrPos - linePoints[i];
            float proj = Vector2.Dot(segNorm, plrPosL);
            float perp = (plrPosL - (proj * segNorm)).magnitude;
            if (proj > 0 && proj < seg.magnitude)
            {
                if (Mathf.Abs(perp) < currWidth - 4f && currWidth > maxWidth * 0.9f)
                {
                    collided = true;
                    break;
                }
                else if (Mathf.Abs(perp) < currWidth + 16f)
                {
                    grazed = true;
                }
            }
        }


        if (collided) {
            if (!e.GetComponent<KHealth>()) { return; }
            e.GetComponent<KHealth>().ChangeHealth(-damage, "laser bullet");
            // add effect?
            Destroy(gameObject);
            return;
        }

        if (grazed)
        {
            grazeStore += 0.125f;
            if (grazeStore >= 1f)
            {
                if (!e.GetComponent<SpecialGunTemplate>()) { return; }
                int oldGH = (int)e.GetComponent<SpecialGunTemplate>().gunHealth;
                e.GetComponent<SpecialGunTemplate>().gunHealth += 1f;
                grazeStore -= 1f;
                BulletController.GetHelper().PlayGrazeSound(oldGH, oldGH + 1);
            }
        }
        
    }

    private void Start()
    {
        startTime = DoubleTime.ScaledTimeSinceLoad;
        activated = false;
        dirtyLine = false;
        distSinceLast = 0;
        grazeStore = 1;
        currWidth = 1;
        t = transform;

        if (specialTetraLaser) // try to render in front of everything
        {
            t.position += Vector3.back * 100;
        }
        
        if (startingBezier.points.Count > 0) // probably dont want to move the transform if this is a generated bezier.
        {
            linePoints = startingBezier.GetEquallySpacedPoints(pointDist, t.position);
            lineRend.loop = startingBezier.loop;
            t.position = startingBezier.loop ? (startingBezier.points[0].position + t.position) : (startingBezier.points[startingBezier.points.Count - 1].position + t.position);
        }
        else
        {
            linePoints = new List<Vector3>();
            linePoints.Add(t.position);
            linePoints.Add(t.position);
        }

        lastPos = t.position;

        if (specialTetraLaser)
        {
            tetraLasers.Add(this);
            t.position += tetraLaserDist * transform.right;
        }
    }

    private void OnDestroy()
    {
        tetraLasers.Remove(this);
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (lineMatCycleSpeed != 0f)
        {
            Vector4 vo = lineRend.material.GetVector("_TexSize");
            vo -= new Vector4(0,0,1,0) * lineMatCycleSpeed * Time.timeScale;
            lineRend.material.SetVector("_TexSize", vo);
        }

        if (t.hasChanged) { MoveCursorAbsolute(t.position); }
        if (dirtyLine) { UpdateLine(); }

        double tm = DoubleTime.ScaledTimeSinceLoad - startTime;

        if (tm >= warnTime)
        {
            if (!activated)
            {
                activated = true;
            }
            CheckCollision();
            if (tm <= existTime + warnTime)
            {
                currWidth = Mathf.MoveTowards(currWidth, maxWidth, resizeSpeed);
            }
            else
            {
                currWidth = Mathf.MoveTowards(currWidth, 0f, resizeSpeed);
                if (currWidth == 0f)
                {
                    Destroy(gameObject);
                }
            }
            
        }
        
    }

    private void LateUpdate()
    {
        lastPos = t.position;
    }

}
