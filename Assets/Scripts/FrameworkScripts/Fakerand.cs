using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public static class Fakerand {

    public static uint i1 = (uint)(UnityEngine.Random.value * 100000000f); //2379310932; //seed

    public static uint input1 = 0;

	public static int Int(int min, int max) //excludes max
    {
        if (min == max) { return min; }
        if (min + 1 == max) { return min; } 
        if (min > max) { var temp = max; max = min; min = temp; }
        unchecked
        {
            i1 ^= i1 >> 20;
        i1 += input1; //rigged
        i1 = (uint)((1530334531UL * i1) % 4294967296);
        return (int)(i1%(max - min))+min;
        }
	}

    [StructLayout(LayoutKind.Explicit)]
    struct FloatInt
    {
        [FieldOffset(0)]
        float f;
        [FieldOffset(0)]
        int i;

        public FloatInt(int _i)
        {
            f = 0f; //LOL!!
            i = _i;
        }

        public float GetFloat()
        {
            return f;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct DoubleLong
    {
        [FieldOffset(0)]
        double d;
        [FieldOffset(0)]
        long l;

        public DoubleLong(long _l)
        {
            d = 0.0; //LOL!!
            l = _l;
        }

        public double GetDouble()
        {
            return d;
        }
    }

    public static float Single(float min = 0f, float max = 1f) //excludes max
    {
        if (min == max){ return min; }
        if (min > max) { var temp = max; max = min; min = temp; }
        FloatInt x = new FloatInt(0x3F800000 ^ Int(0, 8388608));
        float y = x.GetFloat() - 1f;
        return (y * (max - min)) + min;
    }

    public static double Double(double min = 0f, double max = 1f) //excludes max
    {
        if (min == max) { return min; }
        if (min > max) { var temp = max; max = min; min = temp; }
        DoubleLong x = new DoubleLong((0x3FF0000000000000 ^ Int(0, 67108864)) ^ ((long)Int(0, 67108864)<<26) );
        double y = x.GetDouble() - 1f;
        return (y * (max - min)) + min;
    }

    public static Vector2 UnitCircle(bool surfaceOnly = false) //polar disorder
    {
        float a = Single(0f, 6.2831853f);
        Vector2 x = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        return (surfaceOnly)? (x): (x* Fastmath.FastSqrt(Single()));
    }

    public static Vector3 UnitSphere(bool surfaceOnly = false)
    {
        float r = -2f * Single() + 1f;
        float b = Mathf.Cos(Mathf.Acos(r) - 1.5707963f);
        Vector3 x = (Vector3)UnitCircle(true)*b + new Vector3(0, 0, -r);
        return (surfaceOnly) ? (x) : (x * Mathf.Pow(Single(),0.33333333f));
    }

    public static Vector4 UnitGlome(bool surfaceOnly = false)
    {
        float r = -2f * Single() + 1f;
        float b = Mathf.Cos(Mathf.Acos(r) - 1.5707963f);
        Vector4 x = (Vector4)UnitSphere(true)*b + new Vector4(0,0,0,-r);
        return (surfaceOnly) ? (x) : (x * Mathf.Pow(Single(), 0.25f));
    }

    // gram-schmidt process
    public static Vector3[] Basis3D()
    {
        Vector3[] r = new Vector3[3];

        r[0] = UnitSphere(true);

        r[1] = UnitSphere(true);
        r[1] = (r[1] - Vector3.Dot(r[1], r[0]) * r[0]).normalized;

        r[2] = UnitSphere(true);
        r[2] = (r[2] - Vector3.Dot(r[2], r[0]) * r[0] - Vector3.Dot(r[2], r[1]) * r[1]).normalized;

        return r;
    }

    public static Vector4[] Basis4D()
    {
        Vector4[] r = new Vector4[4];

        r[0] = UnitGlome(true);

        r[1] = UnitGlome(true);
        r[1] = (r[1] - Vector4.Dot(r[1], r[0]) * r[0]).normalized;

        r[2] = UnitGlome(true);
        r[2] = (r[2] - Vector4.Dot(r[2], r[0]) * r[0] - Vector4.Dot(r[2], r[1]) * r[1]).normalized;

        r[3] = UnitGlome(true);
        r[3] = (r[3] - Vector4.Dot(r[3], r[0]) * r[0] - Vector4.Dot(r[3], r[1]) * r[1] - Vector4.Dot(r[3], r[2]) * r[2]).normalized;

        return r;
    }

    public static float NormalDist(float mean, float sd, float min = float.NegativeInfinity, float max = float.PositiveInfinity)
    {
        return Mathf.Clamp((Int(0, 2) * 2 - 1) * Fastmath.FastInvNorm(Single()) * sd + mean, min, max);
    }

    /// <summary>
    /// Random Color
    /// </summary>
    /// <param name="transparencySet">Set this out of the range [0,255] for random colors. Otherwise it is the transparency</param>
    /// <returns>A random color duh</returns>
    public static Color32 Color(int transparencySet = 255)
    {
        return new Color32((byte)Int(0, 256), (byte)Int(0, 256), (byte)Int(0, 256), (transparencySet<= -1 || transparencySet>=256) ? (byte)Int(0, 256) : (byte)transparencySet);
    }

}
