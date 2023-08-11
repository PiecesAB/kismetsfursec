using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class keeps track of double-precision time.

public static class DoubleTime
{
    public static double ScaledTimeSinceLoad = 0f;
    public static double UnscaledTimeSinceLoad = 0f;
    public static double ScaledTimeRunning = 0f;
    public static double UnscaledTimeRunning = 0f;
    public static double timeScaleCopy = 0f;

    private const double _pi = 3.141592653589793;

    private static GameObject myCaller = null;

    public static void Load()
    {
        // does nothing but run the static constructor
    }

    public static void AddToTime(double amt)
    {
        timeScaleCopy = Time.timeScale;
        double amts = amt * Time.timeScale;
        ScaledTimeSinceLoad += amts;
        UnscaledTimeSinceLoad += amt;
        ScaledTimeRunning += amts;
        UnscaledTimeRunning += amt;
    }

    public static double DoublePong(double v, double t)
    {
        return System.Math.Acos(System.Math.Cos(_pi * v / t)) * t / _pi;
    }

    static DoubleTime()
    {
        myCaller = new GameObject();
        myCaller.name = "DoubleTime Updater";
        myCaller.AddComponent<DoubleTimeUpdater>();
    }
    
}
