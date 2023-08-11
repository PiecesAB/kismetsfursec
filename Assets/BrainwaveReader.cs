using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BrainwaveReader
{
    
    //(-1, 1)
    public static float internalValue = 0f;
    public static float hertz = 1f;
    public static string brainState = "???";
    public static int brainStateInt = 0;
    private static bool idle;
    private static bool playableLevel;

    private static double lastUpdateTime;

    private const float defaultSpeed = 0.25f;
    private const float defaultPeriod = 0.65f;

    

    private static float LastPress(Encontrolmentation e)
    {
        return (float)(DoubleTime.UnscaledTimeSinceLoad - e.eventsTableUnscaled[e.eventsTableUnscaled.Count - 1].Item1);
    }

    private static float UlongToFloat(ulong u, ulong mask)
    {
        return ((u & mask)!=0UL)?1f:-1f;
    }

    private static float Speed(Encontrolmentation e)
    {
        float res = 0f;
        int amt = Mathf.Min(21, e.eventsTableUnscaled.Count);
        if (amt < 2) { return defaultSpeed; }
        res += (float)(DoubleTime.UnscaledTimeSinceLoad - e.eventsTableUnscaled[e.eventsTableUnscaled.Count - 1].Item1);
        for (int i = 1; i < Mathf.Min(21,e.eventsTableUnscaled.Count); ++i)
        {
            res += (float)(e.eventsTableUnscaled[e.eventsTableUnscaled.Count - i].Item1 - e.eventsTableUnscaled[e.eventsTableUnscaled.Count - i - 1].Item1);
        }
        return res / amt;
    }

    private const int terms = 16;
    private static float Periodicity(Encontrolmentation e, ulong mask)
    {
        float[] inData = new float[terms];
        float total = 0f;
        int si = e.eventsTableUnscaled.Count - 1;
        for (int i = 0; i < terms; ++i)
        {
            if (i < e.eventsTableUnscaled.Count)
            {
                while (si >= 0 && e.eventsTableUnscaled[si].Item2 == 0UL) { --si; }
                if (si < 0) { inData[i] = 0.8f + Fakerand.Single(-0.2f,0.2f); continue; }
                inData[i] = UlongToFloat(e.eventsTableUnscaled[si].Item2, mask);
                --si;
            }
            else { inData[i] = 0.8f + Fakerand.Single(-0.2f, 0.2f); }
            total += inData[i];
        }

        float[] y = new float[terms/2];
        float ymax = 0f;

        for (int i = 1; i < y.Length; ++i)
        {
            float t2 = 0f;
            for (int j = i; j < terms; ++j)
            {
                t2 += inData[j-i]*inData[j];
            }
            y[i] = Mathf.Abs(t2/(terms-i));
            if (y[i] > ymax) { ymax = y[i]; }
        }

        /*if (ymax == 0f) { return 1f; }

        float yavg = (ytotal / y.Length) / ymax;
        float resmax = 0f;
        for (int i = 0; i < y.Length; ++i)
        {
            y[i] /= ymax;
            y[i] = Mathf.Abs(y[i] - yavg);
            if (y[i] > resmax)
            {
                resmax = y[i];
            }
        }*/

        return ymax;
    }

    public static void Reset()
    {
        lastUpdateTime = -10000f;
        internalValue = 0f;
        brainState = "???";
        hertz = 0;
        playableLevel = (Object.FindObjectOfType<LevelInfoContainer>() != null);
    }

    public static void UpdateString()
    {
        if (idle) { brainState = "???"; brainStateInt = 0; return; }
        if (internalValue > 0.99f) { brainState = "CRITICAL HIGH"; brainStateInt = 1; return; }
        if (internalValue > 0.4f) { brainState = "Gamma"; brainStateInt = 2; return; }
        if (internalValue > -0.4f) { brainState = "Beta"; brainStateInt = 3; return; }
        if (internalValue > -0.7f) { brainState = "Alpha"; brainStateInt = 4; return; }
        if (internalValue > -0.9f) { brainState = "Theta"; brainStateInt = 5; return; }
        if (internalValue > -0.999f) { brainState = "Delta"; brainStateInt = 6; return; }
        brainState = "CRITICAL LOW"; brainStateInt = 7;
    }

    public static string GetFullString()
    {
        return hertz.ToString("F2") + " Hz, " + brainState;
    }

    public static void Update(Encontrolmentation e)
    {
        if (!playableLevel || Door1.levelComplete || Time.timeScale == 0) { return; }
        if (DoubleTime.UnscaledTimeSinceLoad - lastUpdateTime < 1.0 && DoubleTime.UnscaledTimeSinceLoad - lastUpdateTime > -1.0) { return; }

        lastUpdateTime = DoubleTime.UnscaledTimeSinceLoad;

        if (e.eventsTableUnscaled.Count == 1 
            || LastPress(e) >= 9.0)
        {
            idle = true;
            UpdateString();
            //Debug.Log(hertz.ToString("F2") + " Hz: " + brainState);
            return;
        }

        //i dont know how this works
        //but assume it's usually around 0.65
        float periodic = (Periodicity(e, 1UL) + Periodicity(e, 2UL) + Periodicity(e, 16UL))*0.3333333f;
        //Debug.Log(periodic);
        float perRat = (periodic - defaultPeriod)*-10f;
        float speed = Mathf.Max(Speed(e),0.01f);
        float speedRat = defaultSpeed / speed;
        if (speedRat < 1) { speedRat = 1 / speedRat; }
        speedRat = Fastmath.FastLog2(speedRat);
        if (speed > defaultSpeed) { speedRat *= -1f; }
        //pressing way too fast
        if (speedRat > 1.75f) { speedRat *= -0.5f; }

        if (perRat < 0f) { internalValue = Mathf.LerpUnclamped(internalValue, -1f, -perRat * 0.2f); }
        else { internalValue = Mathf.LerpUnclamped(internalValue, 1f, perRat * 0.2f); }

        if (speedRat < 0f) { internalValue = Mathf.LerpUnclamped(internalValue, -1f, -speedRat * 0.2f); }
        else { internalValue = Mathf.LerpUnclamped(internalValue, 1f, speedRat * 0.2f); }

        if (internalValue > 0.99f) { hertz = 100*Mathf.Pow(10,Fakerand.Single()); }
        else if (internalValue > 0f) { hertz = Mathf.Pow(10, internalValue + 1); }
        else if (internalValue > -0.999f) { hertz = Mathf.Pow(25, internalValue + 1) * 0.4f; }
        else { hertz = 0.4f*Fakerand.Single(); }

        idle = false;
        UpdateString();
        //Debug.Log(hertz.ToString("F2") + " Hz: " + brainState);
    }
}
