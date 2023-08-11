using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionalRef : MonoBehaviour, InGameConditional
{
    public enum Mode
    {
        Single, MultiAnd, MultiOr
    }

    public bool negate = false;
    public Mode mode = Mode.Single;
    public GameObject reference;

    public bool Evaluate()
    {
        bool ret = false;
        switch (mode)
        {
            case Mode.Single:
                ret = reference.GetComponent<InGameConditional>().Evaluate();
                break;
            case Mode.MultiAnd:
                ret = true;
                foreach (InGameConditional igc in reference.GetComponents<InGameConditional>())
                {
                    ret &= igc.Evaluate();
                    if (!ret) { break; }
                }
                break;
            case Mode.MultiOr:
                ret = false;
                foreach (InGameConditional igc in reference.GetComponents<InGameConditional>())
                {
                    ret |= igc.Evaluate();
                    if (ret) { break; }
                }
                break;
        }
        return ret ^ negate;
    }

    public string GetInfo()
    {
        return "???"; // TODO?
    }
}
