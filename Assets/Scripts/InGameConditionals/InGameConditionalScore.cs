using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionalScore : MonoBehaviour, InGameConditional
{
    public enum Mode
    {
        ZeroScoreRun,
    }

    public Mode mode;
    public int x;

    public bool Evaluate()
    {
        switch (mode)
        {
            case Mode.ZeroScoreRun:
                return Utilities.loadedSaveData.totalScore <= 0;
        }
        return false;
    }

    public string GetInfo()
    {
        switch (mode)
        {
            case Mode.ZeroScoreRun:
                return "Must be zero score run";
        }
        return "???";
    }
}
