using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionCollider : MonoBehaviour, InGameConditional
{
    public enum EvalType
    {
        HasEverTouched
    }

    public EvalType evalType;
    public Collider2D other;
    public string customReasonForFail = "";
    private bool hasEverTouched = false;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider == other)
        {
            hasEverTouched = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == other)
        {
            hasEverTouched = true;
        }
    }

    public bool Evaluate()
    {
        switch (evalType)
        {
            case EvalType.HasEverTouched:
                return hasEverTouched;
        }
        return false;
    }

    public string GetInfo()
    {
        if (customReasonForFail != "") { return customReasonForFail; }
        switch (evalType)
        {
            case EvalType.HasEverTouched:
                return "Did not yet touch collider named \"" + other.name + "\"";
        }
        return "???";
    }
}
