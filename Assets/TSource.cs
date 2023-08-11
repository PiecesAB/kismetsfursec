using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSource : GenericBlowMeUp
{
    [SerializeField]
    private Renderer[] renderers;
    [SerializeField]
    private Transform rotationRod;
    [SerializeField]
    private Rigidbody2D r2;
    [SerializeField]
    private LineRenderer line;

    public Transform start;
    public Transform end;

    public float speed = 360f;
    public AnimationCurve offset = AnimationCurve.Constant(0, 1, 0);

    private Vector2 startPoint;
    private Vector2 endPoint;

    private Transform t;
    private Vector2 dir;
    private float rangeMax;

    private Vector2 SnapToProjection(Vector2 x)
    {
        Vector2 v = x - startPoint;
        float d = Mathf.Clamp(Vector2.Dot(v, dir), 0, rangeMax);
        return startPoint + (d * dir);
    }

    void Start()
    {
        t = transform;
        startPoint = start.position;
        endPoint = end.position;
        start.SetParent(null);
        end.SetParent(null);
        dir = (endPoint - startPoint).normalized;
        rangeMax = (endPoint - startPoint).magnitude;
        transform.position = SnapToProjection(t.position);
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        bool vis = false;
        for (int i = 0; i < renderers.Length; ++i)
        {
            if (renderers[i].isVisible) { vis = true; break; }
        }
        if (!vis) { return; }
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (!e) { return; }

        // aim
        Vector3 dif = e.transform.position - t.position;
        rotationRod.localEulerAngles = Vector3.forward * Mathf.Rad2Deg * Mathf.Atan2(dif.y, dif.x);

        //move
        Vector3 offsetVal = offset.Evaluate((float)(DoubleTime.ScaledTimeSinceLoad % 100000.0)) * dir;
        Vector2 goal = Vector2.MoveTowards(t.position, e.transform.position + offsetVal, speed * Time.timeScale * 0.01666666f);
        t.position = SnapToProjection(goal);
        line.positionCount = 2;
        line.SetPosition(0, SnapToProjection((Vector2)t.position - (40 * dir)));
        line.SetPosition(1, SnapToProjection((Vector2)t.position + (40 * dir)));
    }
}
