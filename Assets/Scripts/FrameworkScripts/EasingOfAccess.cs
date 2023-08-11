using UnityEngine;
using System.Collections;

public static class EasingOfAccess {

    public enum EasingType
    {
        Constant, Linear, QuadraticIn, QuadraticOut, CubicIn, CubicOut, QuinticIn, QuinticOut, NinthDegreeIn, NinthDegreeOut,
        SineSmooth, ElasticIn, ElasticOut, BounceIn, BounceOut, LinearStepIn, LinearStepOut, SineStepIn, SineStepOut
    }

    private static float PrepareInputA(float i, bool loop = false)
    {
        float o = i - Mathf.Floor(i);
        if (!loop)
        {
            if (i < 0f)
            {
                o = 0f;
            }

            if (i >= 1f)
            {
                o = 1f;
            }
        }
        return o;
    }

    public static float Constant(float i, float step = 0.5f, bool equalstep = true)
    {
        float o = PrepareInputA(i);
        o = ((i > step) || ((i == step) && (equalstep))) ? 1f : 0f;
        return o;
    }

    public static float Linear(float i) //why does this exist?
    {
        float o = PrepareInputA(i);
        return o;
    }

    public static float QuadraticIn(float i)
    {
        float o = PrepareInputA(i);
        o -= 1f;
        o = (-o * o) + 1f;
        return o;
    }

    public static float QuadraticOut(float i)
    {
        float o = PrepareInputA(i);
        o *= o;
        return o;
    }

    public static float CubicIn(float i)
    {
        float o = PrepareInputA(i);
        o -= 1f;
        o = (o * o * o) + 1f;
        return o;
    }

    public static float CubicOut(float i)
    {
        float o = PrepareInputA(i);
        o *= o * o;
        return o;
    }

    public static float QuinticIn(float i)
    {
        float o = PrepareInputA(i);
        o -= 1f;
        float h = o * o;
        o = (o * h * h) + 1f;
        return o;
    }

    public static float QuinticOut(float i)
    {
        float o = PrepareInputA(i);
        float h = o * o;
        o *= h * h;
        return o;
    }

    public static float NinthDegreeIn(float i)
    {
        float o = PrepareInputA(i);
        o -= 1f;
        float h = o * o * o;
        o = (h * h * h) + 1f;
        return o;
    }

    public static float NinthDegreeOut(float i)
    {
        float o = PrepareInputA(i);
        float h = o * o * o;
        o = h * h * h;
        return o;
    }

    public static float SineSmooth(float i, float iterations = 1f) //more iterations = tighter curve
    {
        float o = PrepareInputA(i);
        float h = 0.5f;
        for (byte u = 1; u <= iterations; u++)
        {
            o -= h;
            o = (0.5f * Mathf.Sin(3.1415927f * o)) + 0.5f;
        }
        return o;
    }

    public static float ElasticIn(float i, float factor = 40f)
    {
        float o = PrepareInputA(i);
        float p = o * o * o;
        o -= 1f;
        float q = o * o;
        o = q * q;
        o *= o;
        o *= o;
        o = (0.5f * q * Mathf.Sin(factor * p)) - o + 1f;
        return o;
    }

    public static float ElasticOut(float i, float factor = 40f)
    {
        float o = 1f-ElasticIn(1f - i, factor);
        return o;
    }

    public static float BounceIn(float i, float factor = 0.75f)
    {
        float o = PrepareInputA(i);
        float p = (factor * o) + 1.1195151f;
        p *= p;
        p *= p;
        o = ((1f - o) * (1f - System.Math.Abs(Mathf.Sin(p)))) + o;
        return o;
    }

    public static float BounceOut(float i, float factor = 0.75f)
    {
        float o = 1f - BounceIn(1f - i, factor);
        return o;
    }

    public static float LinearStepIn(float i, float steps = 4f, float slope = 4f)
    {
        float o = PrepareInputA(i);
        float p = Mathf.Floor(o * steps) / steps;
        float q = 1 / steps;
        o = p + Mathf.Min(q,slope*(o%q));
        return o;
    }

    public static float LinearStepOut(float i, float steps = 4f, float slope = 4f)
    {
        float o = 1f - LinearStepIn(1f - i, steps, slope);
        return o;
    }

    public static float SineStepIn(float i, float steps = 4f, float intensity = 0.125f)
    {
        float o = PrepareInputA(i);
        float p = Mathf.Sin(2f * steps * Mathf.PI * (o - (0.25f / steps)));
        o = (intensity * p) + o + intensity;
        return o;
    }

    public static float SineStepOut(float i, float steps = 4f, float intensity = 0.125f)
    {
        float o = 1f - SineStepIn(1f - i, steps, intensity);
        return o;
    }


    public static float Evaluate(EasingType e, float i)
    {
        switch (e)
        {
            case EasingType.BounceIn: return BounceIn(i);
            case EasingType.BounceOut: return BounceOut(i);
            case EasingType.Constant: return Constant(i);
            case EasingType.CubicIn: return CubicIn(i);
            case EasingType.CubicOut: return CubicOut(i);
            case EasingType.ElasticIn: return ElasticIn(i);
            case EasingType.ElasticOut: return ElasticOut(i);
            case EasingType.Linear: return Linear(i);
            case EasingType.LinearStepIn: return LinearStepIn(i);
            case EasingType.LinearStepOut: return LinearStepOut(i);
            case EasingType.NinthDegreeIn: return NinthDegreeIn(i);
            case EasingType.NinthDegreeOut: return NinthDegreeOut(i);
            case EasingType.QuadraticIn: return QuadraticIn(i);
            case EasingType.QuadraticOut: return QuadraticOut(i);
            case EasingType.QuinticIn: return QuinticIn(i);
            case EasingType.QuinticOut: return QuinticOut(i);
            case EasingType.SineSmooth: return SineSmooth(i);
            case EasingType.SineStepIn: return SineStepIn(i);
            case EasingType.SineStepOut: return SineStepOut(i);
        }
        return -1f;
    }


    //overshoot

    //antismooth

}
