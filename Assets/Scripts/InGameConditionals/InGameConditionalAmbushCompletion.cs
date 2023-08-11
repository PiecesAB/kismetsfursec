using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionalAmbushCompletion : MonoBehaviour, InGameConditional
{
    public enum EvalType
    {
        AllAmbushesOver
    }

    public EvalType evalType;
    public AmbushController[] ambushes;

    private float count = 0;

    public bool Evaluate()
    {
        count = 0;
        for (int i = 0; i < ambushes.Length; ++i)
        {
            if (!ambushes[i]) { continue; }
            if (ambushes[i].statusMessage == "win") { continue; }
            ++count;
        }

        switch (evalType)
        {
            case EvalType.AllAmbushesOver:
                return count == 0;
        }

        return false;
    }

    public string GetInfo()
    {
        switch (evalType)
        {
            case EvalType.AllAmbushesOver:
                return "There " + (count == 1 ? "is" : "are")  + " <!>" + count.ToString() + "<!> more gauntlet" + (count == 1 ? "" : "s") + " to solve before this one.";
        }

        return "???";
    }
}
