using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bezier //fun with cubic beziers
{
    [System.Serializable]
    public struct Point
    {
        public Vector3 position;
        public Vector3 prevHandle;
        public Vector3 nextHandle;

        public Point(Vector3 _position, Vector3 _prevHandle, Vector3 _nextHandle)
        {
            position = _position;
            prevHandle = _prevHandle;
            nextHandle = _nextHandle;
        }

        public Point(Vector3 _position)
        {
            position = prevHandle = nextHandle = _position;
        }

        public static bool operator ==(Point a, Point b)
        {
            return (a.position == b.position) && (a.prevHandle == b.prevHandle) && (a.nextHandle == b.nextHandle);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            var point = (Point)obj;
            return position.Equals(point.position) &&
                   prevHandle.Equals(point.prevHandle) &&
                   nextHandle.Equals(point.nextHandle);
        }

        public override int GetHashCode()
        {
            int hashCode = 732087598;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(position);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(prevHandle);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(nextHandle);
            return hashCode;
        }
    }

    public List<Point> points = new List<Point>();
    public Vector3 offset = Vector3.zero;
    public bool loop = false;

    private float[] segmentLengths;
    private float totalLength;

    private List<GameObject> pivots = new List<GameObject>();

    private bool BadSegmentIndex(int index)
    {
        if (index >= points.Count || index < 0)
        {
            return true;
        }

        if (!loop && index == points.Count - 1)
        {
            return true;
        }

        return false;
    }

    public Vector3 EvaluateOnSegment(int index, float t)
    {
        if (BadSegmentIndex(index))
        {
            return Vector3.negativeInfinity;
        }

        Point a = points[index];
        Point b = loop ? (points[(index+1)%(points.Count)]) : (points[index+1]);
        bool changeA = false;
        bool changeB = false;

        if (a.position == a.nextHandle)
        {
            a.nextHandle = Vector3.Lerp(a.position, b.position, 0.333333333f);
            changeA = true;
        }

        if (b.position == b.prevHandle)
        {
            b.prevHandle = Vector3.Lerp(b.position, a.position, 0.333333333f);
            changeB = true;
        }

        t = Mathf.Clamp01(t);

        float ti = 1f - t;
        float t2 = t * t;
        float ti2 = ti * ti;

        Vector3 final = (ti2*ti*a.position + 3f*(ti2*t*a.nextHandle + ti*t2*b.prevHandle) + t2*t*b.position) + offset;

        if (changeA)
        {
            a.nextHandle = a.position;
        }

        if (changeB)
        {
            b.prevHandle = b.position;
        }

        return final;

    }

    public float SegmentApproxLength(int index, int precision = 32)
    {
        if (BadSegmentIndex(index))
        {
            return 0f;
        }

        if (precision < 0)
        {
            return 0f;
        }

        Point a = points[index];
        Point b = points[(index + 1) % (points.Count)];

        List<Vector3> pts = new List<Vector3>();
        pts.Add(a.position + offset);
        for (int i = 1; i <= precision; i++)
        {
            pts.Add(EvaluateOnSegment(index, (float)i / (precision + 1)));
        }
        pts.Add(b.position + offset);

        float d = 0f;
        for (int i = 0; i < pts.Count - 1; i++)
        {
            d += Vector3.Distance(pts[i], pts[i + 1]);
        }

        pts.Clear();

        return d;
    }

    public void GenerateSegmentLengths()
    {
        int c = loop ? points.Count : (points.Count - 1);
        segmentLengths = new float[c];
        totalLength = 0;
        for (int i = 0; i < c; ++i)
        {
            segmentLengths[i] = SegmentApproxLength(i);
            totalLength += segmentLengths[i];
        }
    }

    public List<Vector3> GetEquallySpacedPoints(float spacing, Vector3 translation)
    {
        List<Vector3> ret = new List<Vector3>();
        GenerateSegmentLengths();

        ret.Add(points[0].position + translation);

        int idx = 0;
        float prog = 0f;

        while (true)
        {
            float remainingLength = (1 - prog) * segmentLengths[idx];
            if (remainingLength <= spacing)
            {
                ++idx;
                ret.Add(points[idx % points.Count].position + translation);
                prog = 0f;
                if ((loop && idx == points.Count) || (!loop && idx == points.Count - 1))
                {
                    break;
                }
            }
            else
            {
                prog += spacing / segmentLengths[idx];
                ret.Add(EvaluateOnSegment(idx, prog) + translation);
            }
        }

        return ret;
    }

    public void DrawInSceneView(int precision = 32)
    {
        int limit = points.Count - 1;
        if (loop)
        {
            limit++;
        }

        for (int i = 0; i < limit; i++)
        {
            Point a = points[i];
            Point b = points[(i + 1) % (points.Count)];
    
            List<Vector3> pts = new List<Vector3>();
            pts.Add(a.position + offset);
            for (int j = 1; j <= precision; j++)
            {
                pts.Add(EvaluateOnSegment(i, (float)j / (precision + 1)));
            }
            pts.Add(b.position + offset);

            for (int j = 0; j < pts.Count - 1; j++)
            {
                Debug.DrawLine(pts[j], pts[j + 1], Color.HSVToRGB((float)j/pts.Count, 1f, 1f), 1f);
            }
        }
    }

    private void AddLRPoint(ref int iter, ref LineRenderer wire, Vector3 pos)
    {
        wire.positionCount++;
        wire.SetPosition(iter, pos + new Vector3(0,0,4));
        iter++;
    }

    private void AddSprPivot(ref GameObject pivot, Vector3 pos)
    {
        if (pivot)
        {
            GameObject n = Object.Instantiate(pivot, pos, pivot.transform.rotation, pivot.transform.parent);
            n.SetActive(true);
            pivots.Add(n);
        }
    }

    public void DestroyPivots()
    {
        for (int i = 0; i < pivots.Count; i++)
        {
            Object.DestroyImmediate(pivots[i]);
        }
        pivots.Clear();
    }

    public void DrawUsingStuff(ref GameObject pivotSample, ref LineRenderer wire, int precision = 8)
    {
        DestroyPivots();

        if (!wire.useWorldSpace)
        {
            Debug.Log("make sure the line renderer is in world space");
        }
        wire.useWorldSpace = true;
        wire.loop = loop;


        int limit = points.Count - 1;
        if (loop)
        {
            limit++;
        }

        int u = 0;
        wire.positionCount = 0;
        for (int i = 0; i < limit; i++)
        {
            Point a = points[i];
            Point b = points[(i + 1) % (points.Count)];

            List<Vector3> pts = new List<Vector3>();
            Vector3 apos = a.position + offset;
            AddLRPoint(ref u, ref wire, apos);
            //AddSprPivot(ref pivotSample, apos);

            if (!(a.position == a.nextHandle && b.position == b.prevHandle))
            {
                for (int j = 1; j <= precision; j++)
                {
                    AddLRPoint(ref u, ref wire, EvaluateOnSegment(i, (float)j / (precision + 1)));
                }
            }
            
            if (!loop && i == limit-1)
            {
                Vector3 bpos = b.position + offset;
                AddLRPoint(ref u, ref wire, bpos);
                //AddSprPivot(ref pivotSample, bpos);
            }
        }

    }

    ~Bezier()
    {
        DestroyPivots();
    }
}
