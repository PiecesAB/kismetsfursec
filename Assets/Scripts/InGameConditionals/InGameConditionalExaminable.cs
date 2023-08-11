using UnityEngine;

public class InGameConditionalExaminable : MonoBehaviour, InGameConditional
{
    public enum Mode
    {
        TimesOpenedAtLeast
    }

    public Mode mode;
    public PrimExaminableItem pei;
    public int val;
    public Collider2D offsetOnConditionsFailed; // offset into the null zone

    private bool Evaluate2()
    {
        switch (mode)
        {
            case Mode.TimesOpenedAtLeast:
                if (pei == null) { return true; } // if it doesn't exist, it surpassed all messages
                return pei.textboxesGiven >= val;
        }
        return false;
    }

    public bool Evaluate()
    {
        bool ret = Evaluate2();
        if (offsetOnConditionsFailed)
        {
            offsetOnConditionsFailed.offset = ret ? Vector2.zero : new Vector2(1000000, 1000000);
        }
        return ret;
    }

    public string GetInfo()
    {
        return "Examinable conditions not met"; // bad error; maybe add real errors if necessary
    }
}
