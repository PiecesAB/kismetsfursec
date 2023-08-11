using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WaveFunctions {

    /// <summary>
    /// Normalized sine wave with period of 1.
    /// </summary>
    /// <returns>
    /// Output of the function, [-1,1]
    /// </returns>
    public static float Sine(float x)
    {
        return Mathf.Sin(x * 6.2831853f);
    }

    /// <summary>
    /// Normalized square wave with period of 1.
    /// </summary>
    /// <returns>
    /// Output of the function, [-1,1]
    /// </returns>
    public static float Square(float x)
    {
        return (x%1 >= 0.5f)?-1f:1f;
    }

    /// <summary>
    /// Normalized triangle wave with period of 1.
    /// </summary>
    /// <returns>
    /// Output of the function, [-1,1]
    /// </returns>
    public static float Triangle(float x)
    {
        return (Mathf.PingPong(x + 0.25f, 0.5f) - 0.25f) * 4f;
    }

    /// <summary>
    /// Normalized sawtooth wave with period of 1.
    /// </summary>
    /// <returns>
    /// Output of the function, [-1,1]
    /// </returns>
    public static float Sawtooth(float x)
    {
        return (Mathf.Repeat(x + 0.5f, 1f) - 0.5f) * 2f;
    }

    /// <summary>
    /// Hex wave with period of 1.
    /// </summary>
    /// <returns>
    /// Output of the function, [-1,1]
    /// </returns>
    public static float Hex(float x)
    {
        return Mathf.Clamp(Triangle(x)*3f,-1f,1f);
    }

    /// <summary>
    /// Derivative of Hex(x); 0 where it would be otherwise non-differentiable
    /// </summary>
    /// <returns>
    /// Output of the function, -3,0,3
    /// </returns>
    public static float HexDerivative(float x)
    {
        float l = Mathf.Repeat(x, 0.5f);
        float m = Mathf.Repeat(x, 1f);
        if (l > 0.166666666f && l < 0.333333333f)
        {
            return 0f;
        }
        else if (m > 0.3f && m < 0.7f)
        {
            return -3f;
        }
        else
        {
            return 3f;
        }
    }

    /// <summary>
    /// LOL!
    /// </summary>
    /// <returns>
    /// 1
    /// </returns>
    public static float ConstantOne(float x)
    {
        return 1f;
    }

    public static float HealthFromScore(int x)
    {
        float r = Mathf.Infinity;
        if (x < 1)
        {
            r = 1f;
        }
        else if (x < 1000000)
        {
            r = 10f + 0.001f * Mathf.Pow(x, 0.8f);
        }
        else if (x < 100000000)
        {
            r = 75f + 0.00003f * Mathf.Pow(x - 1000000, 0.9f);
        }
        else if (x < 999999999)
        {
            r = -(5e11f / (x - 999999999));
        }

        if (x >= 1 && x < 999999999)
        {
            // balancing to make sure the game doesn't get too easy or hard
            r = Mathf.Lerp(r, 10f, 0.25f);
        }

        return r;
    }

}
