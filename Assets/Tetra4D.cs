using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extension4DSpace;

public class Tetra4D
{
    public Vector4 a;
    public Vector4 b;
    public Vector4 c;
    public Vector4 d;

    private Vector4[][] edges;

    private void MakeEdges()
    {
        edges = new Vector4[6][]{
            new Vector4[2] { a, b },
            new Vector4[2] { a, c },
            new Vector4[2] { a, d },
            new Vector4[2] { b, c },
            new Vector4[2] { b, d },
            new Vector4[2] { c, d } };
    }

    public Tetra4D()
    {
        this.a = this.b = this.c = this.d = Vector4.zero;
        MakeEdges();
    }
    public Tetra4D(Vector4 a, Vector4 b)
    {
        this.a = a;
        this.b = b;
        this.c = this.d = Vector4.zero;
        MakeEdges();
    }
    public Tetra4D(Vector4 a, Vector4 b, Vector4 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = Vector4.zero;
        MakeEdges();
    }
    public Tetra4D(Vector4 a, Vector4 b, Vector4 c, Vector4 d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        MakeEdges();
    }

    public static Tetra4D operator +(Tetra4D i, Vector4 pos) { return new Tetra4D(i.a + pos, i.b + pos, i.c + pos, i.d + pos); }
    public static Tetra4D operator +(Vector4 pos, Tetra4D i) { return i + pos; }
    public static Tetra4D operator -(Tetra4D i, Vector4 pos) { return i + (-pos); }

    public bool Is4D()
    {
        return Mathf.Abs(a.w) > 1e-6 || Mathf.Abs(b.w) > 1e-6 || Mathf.Abs(c.w) > 1e-6 || Mathf.Abs(d.w) > 1e-6;
    }

    public bool IntersectsView()
    {
        float bside = Mathf.Sign(b.w);
        float cside = Mathf.Sign(c.w);
        return (Mathf.Sign(a.w) != bside) || (bside != cside) || (cside != Mathf.Sign(d.w));
    }

    public Vector4 Centroid()
    {
        return new Vector4(
            0.25f * (a.x + b.x + c.x + d.x),
            0.25f * (a.y + b.y + c.y + d.y),
            0.25f * (a.z + b.z + c.z + d.z),
            0.25f * (a.w + b.w + c.w + d.w)
            );
    }

    private void DrawTri(Vector3 a3, Vector3 b3, Vector3 c3, MeshRend4DExtension ext, int mat)
    {
        //clockwise? The camera always looks forward (0,0,1) so i want it backwards
        Vector3 n1 = b3 - a3;
        Vector3 n2 = c3 - a3;
        Vector3 norm = Vector3.Cross(n1, n2);
        float orientation = Vector3.Dot(ext.transform.InverseTransformDirection(Vector3.back), norm);
        if (orientation < 0) { Vector3 temp = c3; c3 = b3; b3 = temp; norm = -norm; }

        ext.normals.Add(norm); ext.normals.Add(norm); ext.normals.Add(norm);
        ext.verts.Add(a3); ext.verts.Add(b3); ext.verts.Add(c3);
        ext.tris[mat].Add(ext.verts.Count - 3);
        ext.tris[mat].Add(ext.verts.Count - 2);
        ext.tris[mat].Add(ext.verts.Count - 1);
    }

    public void Draw(MeshRend4DExtension ext, int mat)
    {
        //if it's completely within w=0, draw its four triangles
        if (!Is4D()) {
            DrawTri(a, b, c, ext, mat);
            DrawTri(a, d, b, ext, mat);
            DrawTri(a, c, d, ext, mat);
            DrawTri(c, b, d, ext, mat); return;
        }

        //if it doesn't intersect with w=0, render nothing
        if (!IntersectsView()) { return; }

        //now it may only appear as a triangle or convex trapezoid. find that and draw it

        //catalog all intersections
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < 6; ++i)
        {
            float t = -edges[i][0].w / (edges[i][1].w - edges[i][0].w);
            if (!float.IsNaN(t) && 0 <= t && t <= 1)
            {
                points.Add((1 - t) * edges[i][0] + t * edges[i][1]);
            }
        }

        if (points.Count > 4) //too many... some of them are definitely repeats.
        {
            for (int i = 0; i < points.Count - 1; ++i)
            {
                for (int j = i+1; j < points.Count; ++j)
                {
                    if ((points[i] - points[j]).magnitude < 1e-6)
                    {
                        points.RemoveAt(j); --j;
                    }
                }
            }
        }

        if (points.Count == 3)
        {
            DrawTri(points[0], points[1], points[2], ext, mat);
        }

        if (points.Count == 4)
        {
            DrawTri(points[0], points[1], points[2], ext, mat);
            Vector3 ab = points[1] - points[0];
            Vector3 ac = points[2] - points[0];
            Vector3 ad = points[3] - points[0];

            // test A
            Vector3 norm = Vector3.Cross(ab, ac);
            float trip1 = -Vector3.Dot(ad, Vector3.Cross(norm, ac));
            float trip2 = Vector3.Dot(ad, Vector3.Cross(norm, ab));
            if (trip1 > 0 && trip2 > 0) // opposite point is A
            {
                DrawTri(points[1], points[3], points[2], ext, mat);
                return;
            }

            // test B
            Vector3 ba = -ab;
            Vector3 bc = points[2] - points[1];
            Vector3 bd = points[3] - points[1];
            norm = Vector3.Cross(ba, bc);
            trip1 = -Vector3.Dot(bd, Vector3.Cross(norm, bc));
            trip2 = Vector3.Dot(bd, Vector3.Cross(norm, ba));
            if (trip1 > 0 && trip2 > 0) // opposite point is B
            {
                DrawTri(points[0], points[2], points[3], ext, mat);
                return;
            }

            // opposite point is C
            DrawTri(points[0], points[3], points[1], ext, mat);
            return;
        }

        return;
    }

}
