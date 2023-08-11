using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StringScript : MonoBehaviour {

    [System.Serializable]
    public struct PhysObjInfo
    {
        public GameObject obj;
        public float displacement;
        public double time;

        public PhysObjInfo(GameObject _obj, float _displacement)
        {
            obj = _obj;
            displacement = _displacement;
            time = DoubleTime.UnscaledTimeRunning;
        }
    }


    public Transform startPos;
    public Transform endPos;
    public float colWidth;
    public float strength;
    public float knockback = 1f;
    public List<PhysObjInfo> letsBouncing = new List<PhysObjInfo>();
    public BoxCollider2D colBox;
    public Vector2 normal;
    public Vector2 span;
    public Vector2 center;
    public float tightness = 96f;
    public LineRenderer rope;

    Vector2 ClosestPointOnLine(Vector2 point, Vector2 lineSpan)
    {
        float u = Vector2.Dot(point, lineSpan);
        float v = Vector2.Dot(lineSpan, lineSpan);
        return lineSpan * (u / v);
    }

    float ClosestPointValOnly(Vector2 point, Vector2 lineSpan)
    {
        float u = Vector2.Dot(point, lineSpan);
        float v = Vector2.Dot(lineSpan, lineSpan);
        return u / v;
    }

    int inLetsBouncing(GameObject g)
    {
        int onString = -1;
        for (int i = 0; i < letsBouncing.Count; i++)
        {
            if (letsBouncing[i].obj == g)
            {
                onString = i;
                break;
            }
        }
        return onString;
    }

	void Start () {
        rope.SetPositions(new Vector3[2] { startPos.position, endPos.position });
	}

    public void EnterTrig(Collider2D c)
    {
        if (c.gameObject.GetComponent<Rigidbody2D>() != null && inLetsBouncing(c.gameObject) == -1 && !c.gameObject.GetComponent<Rigidbody2D>().isKinematic)
        {
            //Rigidbody2D r = c.gameObject.GetComponent<Rigidbody2D>();
            Vector2 p1 = (Vector2)c.transform.position - center;
            p1 -= c.gameObject.GetComponent<Rigidbody2D>().velocity * 0.016666666f; // this correction makes it much harder to phase through.
            Vector2 p2 = ClosestPointOnLine(p1, span);
            Vector2 pt = p1 - p2;
            letsBouncing.Add(new PhysObjInfo(c.gameObject, pt.magnitude * Mathf.Sign(Vector2.Dot(pt, normal))));
        }
    }

    /*public void ExitTrig(Collider2D c)
    {

    }*/

    void Update () {
        center = Vector2.Lerp(startPos.position, endPos.position, 0.5f);
        colBox.transform.position = center;
        colBox.size = new Vector2((startPos.position - endPos.position).magnitude, colWidth);
        float angle = Mathf.Atan2(endPos.position.y - startPos.position.y, endPos.position.x - startPos.position.x);
        colBox.transform.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
        span = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        normal = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));

        List<Vector3> points = new List<Vector3>() {};
        foreach (var c in letsBouncing)
        {
            Vector2 tv = c.obj.transform.position - (Vector3)(normal * c.displacement);
            points.Add(tv);
            float tk = tightness*((tv- center/*ClosestPointOnLine(tv, span)*/).magnitude/ (endPos.position - startPos.position).magnitude);
            float multfrw = 1f;
            if (Vector2.Dot(c.obj.GetComponent<Rigidbody2D>().velocity,normal)*Mathf.Sign(c.displacement) > 0f)
            {
                multfrw = knockback;
            }
            if (c.obj.GetComponent<BasicMove>() != null)
            {
                BasicMove b = c.obj.GetComponent<BasicMove>();
                if (b.grounded == 0)
                {
                    b.doubleJump = true;
                    float maxNormVel = Vector2.Dot(b.fakePhysicsVel, normal);
                    if (maxNormVel < Mathf.Abs(b.maxFallSpeed) * 1.5f)
                    {
                        b.fakePhysicsVel += multfrw * strength * tk * normal * Mathf.Sign(c.displacement);
                    }
                }
            }
            else
            {
                c.obj.GetComponent<Rigidbody2D>().velocity += multfrw * strength * tk * normal * Mathf.Sign(c.displacement);
            }
        }
        points = points.OrderBy(x => ClosestPointValOnly((Vector2)x - center, span)).ToList();
        points.Insert(0, startPos.position);
        points.Insert(points.Count, endPos.position);
        Vector3[] pointsF = points.ToArray();
        rope.positionCount = pointsF.Length;
        rope.SetPositions(pointsF);

        //get rid of trash.
        bool t = true;
        while (t)
        {
            t = false;
            for (int i = 0; i < letsBouncing.Count; i++)
            {
                if (letsBouncing[i].obj == null)
                {
                    t = true;
                    letsBouncing.RemoveAt(i);
                    break;
                }
                if (DoubleTime.UnscaledTimeRunning - letsBouncing[i].time > 3f*Time.deltaTime)
                {
                    Vector2 lp = letsBouncing[i].obj.transform.position;
                    Vector2 p1 = lp - center;
                    Vector2 p2 = ClosestPointOnLine(p1, span);
                    Vector2 pt = p1 - p2;
                    float dp = letsBouncing[i].displacement;
                    if (Mathf.Sign(Vector2.Dot(pt, normal)) == Mathf.Sign(dp) && pt.magnitude > System.Math.Abs(dp))
                    {
                        t = true;
                        letsBouncing.RemoveAt(i);
                        break;
                    }
                }
            }
        }
	}
}
