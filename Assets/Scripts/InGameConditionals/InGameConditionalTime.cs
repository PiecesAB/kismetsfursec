using UnityEngine;

// returns true if the sprite is facing in a direction
public class InGameConditionalTime : MonoBehaviour, InGameConditional
{
    public enum Mode
    {
        Less = 1, Greater = 2, Equal = 4, LEqual = 5, GEqual = 6
    }

    public bool scaled = false;
    public Mode comparison = Mode.LEqual;
    public double compTime;

    public bool Evaluate()
    {
        double t = scaled ? DoubleTime.ScaledTimeSinceLoad : DoubleTime.UnscaledTimeSinceLoad;
        bool c = false;
        c |= ((int)comparison & 1) != 0 && t < compTime;
        c |= ((int)comparison & 2) != 0 && t > compTime;
        c |= ((int)comparison & 4) != 0 && t == compTime;
        return c;
    }

    public string GetInfo()
    {
        return "Wrong time"; // Maybe improve this later
    }
}
