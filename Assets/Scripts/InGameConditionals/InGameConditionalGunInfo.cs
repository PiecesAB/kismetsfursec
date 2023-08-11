using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionalGunInfo : MonoBehaviour, InGameConditional
{
    public enum EvalType
    {
        LessOrEqual, GreaterOrEqual, WasNeverOver
    }

    public EvalType evalType;
    public float comp;

    private float maxGunHealth = 0f;

    public bool Evaluate()
    {
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (e == null) { return false; }
        SpecialGunTemplate s = e.GetComponent<SpecialGunTemplate>();
        if (s == null) { return false; }
        switch (evalType)
        {
            case EvalType.LessOrEqual: return (s.gunHealth <= comp);
            case EvalType.GreaterOrEqual: return (s.gunHealth >= comp);
            case EvalType.WasNeverOver: return (maxGunHealth <= comp);
        }

        return false;
    }

    public void Update()
    {
        switch (evalType)
        {
            case EvalType.WasNeverOver:
                Encontrolmentation e = LevelInfoContainer.GetActiveControl();
                if (e == null) { return; }
                SpecialGunTemplate s = e.GetComponent<SpecialGunTemplate>();
                if (s == null) { return; }
                maxGunHealth = Mathf.Max(maxGunHealth, s.gunHealth);
                break;
        }
    }

    public string GetInfo()
    {
        switch (evalType)
        {
            case EvalType.GreaterOrEqual:
                return "Weapon energy must be at least <!>" + comp.ToString() + "<!>.";
            case EvalType.LessOrEqual:
                return "Weapon energy must be at most <!>" + comp.ToString() + "<!>.";
            case EvalType.WasNeverOver:
                return "Weapon energy must have never exceeded <!>" + comp.ToString() + "<!>.";
        }

        return "???";
    }
}
