using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension4DSpace
{
    public static class Extension4D
    {
        public static Vector4 right = new Vector4(1, 0, 0, 0);
        public static Vector4 left = new Vector4(-1, 0, 0, 0);
        public static Vector4 up = new Vector4(0, 1, 0, 0);
        public static Vector4 down = new Vector4(0, -1, 0, 0);
        public static Vector4 forward = new Vector4(0, 0, 1, 0);
        public static Vector4 back = new Vector4(0, 0, -1, 0);
        public static Vector4 ana = new Vector4(0, 0, 0, 1);
        public static Vector4 kata = new Vector4(0, 0, 0, -1);

        public static float Dot(this Vector4 i, Vector4 other)
        {
            return (i.x * other.x) + (i.y * other.y) + (i.z * other.z) + (i.w * other.w);
        }

        public static Vector4 Lerp(this Vector4 a, Vector4 b, float t)
        {
            float mt = 1 - t;
            return new Vector4(mt * a.x + t * b.x, mt * a.y + t * b.y, mt * a.z + t * b.z, mt * a.w + t * b.w);
        }

        public static Vector3 StereographicTo3D(this Vector4 i)
        {
            Vector4 n = i.normalized;
            return new Vector3(n.x, n.y, n.z) / (1 - n.w);
        }

        public static Vector3 ProjectTo3D(this Vector4 i)
        {
            return i; // is this function even necessary?
        }

        public static bool IsMultiple(this Vector4 i, Vector4 other)
        {
            return Mathf.Abs(Mathf.Abs(i.Dot(other)) - i.magnitude * other.magnitude) < 1e-6;
        }

        //returns table of three vectors. the first and second vectors will be in w=0 space
        //omitLast means the third vector, which is the 4D one, is omitted.
        public static Vector4[] NormalSpace(this Vector4 i, bool omitLast)
        {
            Vector4[] res = new Vector4[3] { Vector4.zero, Vector4.zero, Vector4.zero };
            if (i.x == 0) { res[0] = right; }
            else if (i.y == 0) { res[0] = up; }
            else if (i.z == 0) { res[0] = forward; }
            else { res[0] = (new Vector4(1, 1, -(i.x + i.y) / i.z, 0)).normalized; }

            Vector4[] nres = NormalPlane(i, res[0], omitLast);
            res[1] = nres[0];
            res[2] = nres[1];
            return res;
        }

        //returns table of two vectors. the first vector will be in w=0 space
        public static Vector4[] NormalPlane(this Vector4 i, Vector4 v0, bool omitLast)
        {
            Vector4[] res = new Vector4[2] { Vector4.zero, Vector4.zero };

            if (!omitLast && i == v0) {
                Debug.Log("There are redundant vectors while trying to calculate 4D normals");
                if (i.x == 0) { res[0] = right; }
            }
            else if (!omitLast && i.ProjectTo3D() == Vector3.zero) // rare but good
            {
                if (v0.x == 0) { res[0] = right; }
                else if (v0.y == 0) { res[0] = up; }
                else if (v0.z == 0) { res[0] = forward; }
                else { res[0] = (new Vector4(1, 1, -(i.x + i.y) / i.z, 0)).normalized; }
            }
            else
            {
                res[0] = -Vector3.Cross(i, v0).normalized;
            }

            if (!omitLast)
            {
                res[1] = NormalLine(i, v0, res[0]);
            }

            return res;
        }

        public static Vector4 NormalLine(this Vector4 i, Vector4 v0, Vector4 v1)
        {
            if (i == v0 || v0 == v1 || i == v1)
            {
                Debug.Log("There are redundant vectors while trying to calculate 4D normals");
            }

            //cofactor expansion sucks!

            return -(new Vector4(
            i.y * (v0.z * v1.w - v0.w * v1.z) - i.z * (v0.y * v1.w - v0.w * v1.y) + i.w * (v0.y * v1.z - v0.z * v1.y),
            -i.x * (v0.z * v1.w - v0.w * v1.z) + i.z * (v0.x * v1.w - v0.w * v1.x) - i.w * (v0.x * v1.z - v0.z * v1.x),
            i.x * (v0.y * v1.w - v0.w * v1.y) - i.y * (v0.x * v1.w - v0.w * v1.x) + i.w * (v0.x * v1.y - v0.y * v1.x),
            -i.x * (v0.y * v1.z - v0.z * v1.y) + i.y * (v0.x * v1.z - v0.z * v1.x) - i.z * (v0.x * v1.y - v0.y * v1.x)
            ).normalized);
        }
    }
}
