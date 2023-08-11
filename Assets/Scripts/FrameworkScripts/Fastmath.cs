using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public static class Fastmath
{

    // partially original functions to do rough and dirty math.
    static float halfpi = 1.5707963f;
    static float tau = 6.2831853f;
    [StructLayout(LayoutKind.Explicit)]
    public struct FloatUInt
    {
        [FieldOffset(0)]
        public readonly float f;
        [FieldOffset(0)]
        public readonly uint ui;

        public FloatUInt(uint _ui)
        {
            f = 0f;
            ui = _ui;
        }

        public FloatUInt(float _f)
        {
            ui = 0;
            f = _f;
        }

        public float GetFloat()
        {
            return f;
        }

        public uint GetUInt()
        {
            return ui;
        }
    }

    public static float FastAtan(float x) //~0.5 degrees off at worst.
    {
        float n = new FloatUInt(((new FloatUInt(x)).GetUInt() & 0x80000000) | 0x3F800000).GetFloat();
        //lol. probably how Mathf.Sign() is actually implemented.
        x = new FloatUInt((new FloatUInt(x)).GetUInt() & 0x7FFFFFFF).GetFloat();
        //LOL! abs
        float optcon = 0.28f;
        if (x <= optcon)
        {
            return n * x;
        }
        else if (x <= 1f)
        {
            return n * x / (1f + optcon * x * x);
        }
        else
        {
            return n*(halfpi - x / (optcon + x * x));
        }
    }

    public static float FastAtan2(float y = 0f, float x = 0f)
    {
        if (x == 0f)
        {
            if (y == 0f)
            {
                return float.NaN;
            }
            else
            {
                return -halfpi * Mathf.Sign(y) + Mathf.PI;
            }
        }
        else
        {
            if (y == 0f)
            {
                return -halfpi * Mathf.Sign(x) + halfpi;
            }
            else
            {
                if (x > 0f)
                {
                    return FastAtan(y / x);
                }
                else
                {
                    return -FastAtan(y / -x) + Mathf.PI;
                }
            }
        }

    }

    public static float FastV2Dist(Vector2 a, Vector2 b) //it's probably somehow faster than the default distance function. Let's try it
    {
        Vector2 r = new Vector2(a.x - b.x, a.y - b.y);
        return (float)Math.Sqrt((r.x * r.x) + (r.y * r.y));
    }

    public static float FastSqrt(float x, byte newtonIterations = 1) // you already know 1/sqrt(x); here's its meaningless cousin. 
                                                                    //~0.1% off at worst with 1 newton iteration.
    {
        if (x < 0f)
        {
            return float.NaN;
        }
        else
        {
            FloatUInt q = new FloatUInt(x);
            uint u = q.GetUInt();
            sbyte d = (sbyte)((byte)((u & 0x7F800000) >> 23) - 0x7F);
            byte nx = (byte)(0x7F + ((d + (d >> 7)) / 2));
            uint nm = (((u & 0x007FFFFF) + ((uint)(d & 0x01) << 23)) >> 1) - 0x00017D86;
            q = new FloatUInt((uint)(nx << 23) + nm);
            float a = q.GetFloat();
            for (byte i = 0; i < newtonIterations; i++)
            {
                a = a - ((a * a - x) / (a * 2f));
            }
            return a; //1 newton
        }
    }

    public static float FastLog2(float x, bool extraAccuracy = true) // ~0.008 (absolute) off at worst, with extra accuracy. without it, ~0.09 at worst.
    {
        FloatUInt u = new FloatUInt((new FloatUInt(x)).GetUInt() & 0xFF800000);
        //the lower power of two. it just removes the mantissa
        float z = u.GetFloat();
        sbyte f = (sbyte)(((byte)(u.GetUInt() >> 23)) - 0x7F);
        float r = (x / z) + f - 1; //implicit function turns the sbyte into float.
        float b = 0f;
        if (extraAccuracy)
        {
            b = (x - 1.5f * z) / (1.7f * z);
            b = -b * b + 0.08657f;
        }
        return r + b;
    }

    public static float FastInvNorm(float x) 
    {
        float n = new FloatUInt(((new FloatUInt(x)).GetUInt() & 0x80000000) | 0x3F800000).GetFloat();

        x = new FloatUInt((new FloatUInt(x)).GetUInt() & 0x7FFFFFFF).GetFloat();

        if (x <= 0.5f) // 99% accurate at worst when 0<x<0.5
        {
            return n*Mathf.Max(1.35f * x - 0.01f, 0f);
        }
        else if (x < 1f)// 95% accurate at worst when 0.5<x<0.9999 and the worst error is: 22% too large at x=0.9999999 (then it's basically 1.000000)
        {
            return n*(0.885f * x - 0.242f * FastLog2(1f - x, true));
        }
        else
        {
            return n*Mathf.Infinity;
        }
    }

    public const float log2tologE = 0.69314718f; //multiply the base 2 log by this to get ln (base e log).
    public const float log2tolog10 = 0.30103000f; //same but for base 10

}
