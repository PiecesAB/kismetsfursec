using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ColliderTracer3D : MonoBehaviour
{
    private MeshFilter mf;
    private MeshRenderer mr;

    public int edgePoolStartCount = 32;
    public GameObject colliderObj;
    public Vector2 vOffset = new Vector2(0f, 3f);

    private Matrix4x4 lastMat;
    private int currEdge = 0;
    private List<EdgeCollider2D> edgePool;

    private void MakeNewEdge()
    {
        EdgeCollider2D newEdge = colliderObj.AddComponent<EdgeCollider2D>();
        newEdge.points = new Vector2[0];
        edgePool.Add(newEdge);
    }

    private void Start()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        edgePool = new List<EdgeCollider2D>();
        for (int i = 0; i < edgePoolStartCount; ++i)
        {
            MakeNewEdge();
        }
        currEdge = 0;
        lastMat = Matrix4x4.zero;
    }

    private Vector2 GetIntersectionPoint(Vector3 a, Vector3 b)
    {
        if (Mathf.Sign(a.z) == Mathf.Sign(b.z)) { return Vector2.positiveInfinity; }
        if (a.z == 0) { return Vector2.positiveInfinity; }
        if (b.z == 0) { return b; }
        return Vector2.Lerp(a, b, -a.z / (b.z - a.z));
    }

    private List<Vector2> GetEdges(ref Vector3[] verts, ref int[] tris)
    {
        // this list is pairs, that define edges without regards to orientation
        // (doesn't matter for EdgeCollider2D)
        List<Vector2> resv = new List<Vector2>();
        for (int i = 0; i < tris.Length/3; ++i)
        {
            Vector3 p0 = transform.TransformPoint(verts[tris[3 * i]]);
            Vector3 p1 = transform.TransformPoint(verts[tris[(3 * i) + 1]]);
            Vector3 p2 = transform.TransformPoint(verts[tris[(3 * i) + 2]]);

            //no intersection with z = 0
            if (p0.z != 0 && Mathf.Sign(p0.z) == Mathf.Sign(p1.z) && Mathf.Sign(p1.z) == Mathf.Sign(p2.z))
            {
                continue;
            }

            int zeroAcc = ((p0.z == 0) ? 1 : 0) + ((p1.z == 0) ? 1 : 0) + ((p2.z == 0) ? 1 : 0);

            // the whole triangle is in z = 0
            if (zeroAcc == 3)
            {
                resv.Add(p0); resv.Add(p1);
                resv.Add(p1); resv.Add(p2);
                resv.Add(p0); resv.Add(p2);
                continue;
            }

            // two of the points in z = 0?
            if (zeroAcc == 2)
            {
                if (p0.z == 0 && p1.z == 0) { resv.Add(p0); resv.Add(p1); continue; }
                if (p1.z == 0 && p2.z == 0) { resv.Add(p1); resv.Add(p2); continue; }
                // 0, 2
                resv.Add(p0); resv.Add(p2);
                continue;
            }

            //the intersection with z = 0 is another line. find the two intersection points and add them
            List<Vector2> intersections = new List<Vector2>()
            {
                GetIntersectionPoint(p0,p1),
                GetIntersectionPoint(p1,p2),
                GetIntersectionPoint(p0,p2)
            };

            //remove the non-intersecting edge
            int j = 0;
            while (j < intersections.Count)
            {
                if (float.IsInfinity(intersections[j].x))
                {
                    intersections.RemoveAt(j);
                    --j;
                }
                ++j;
            }

            if (intersections.Count == 2) // it always should be 2, but just in case
            {
                resv.Add(intersections[0]); resv.Add(intersections[1]);
            }
            
        }
        return resv;
    }

    private void Update()
    {
        if (!mr.isVisible) { return; }

        if (lastMat == transform.worldToLocalMatrix) { return; }

        Vector3[] verts = mf.mesh.vertices;
        int[] tris = mf.mesh.triangles;
        List<Vector2> edges = GetEdges(ref verts, ref tris);

        if (colliderObj.transform.parent == transform)
        {
            colliderObj.name = "3D Tracer Collider";
            colliderObj.transform.SetParent(null, false);
            colliderObj.transform.position = Vector3.zero;
            colliderObj.transform.rotation = Quaternion.identity;
            colliderObj.transform.localScale = Vector3.one;
        }
        

        //colliderObj.transform.localEulerAngles = new Vector3(-transform.eulerAngles.x, -transform.eulerAngles.y, 0f);

        currEdge = 0;
        for (int i = 0; i < edges.Count/2; ++i)
        {
            while (currEdge >= edgePool.Count)
            {
                MakeNewEdge();
            }
            edgePool[currEdge].enabled = true;
            edgePool[currEdge].points = new Vector2[2]
            {
                edges[2 * i] + vOffset,
                edges[2 * i + 1] + vOffset
            };
            ++currEdge;
        }

        while (currEdge < edgePool.Count)
        {
            if (edgePool[currEdge].enabled) { edgePool[currEdge].enabled = false; }
            ++currEdge;
        }

        lastMat = transform.worldToLocalMatrix;
    }
}
