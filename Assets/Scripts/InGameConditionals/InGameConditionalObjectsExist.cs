using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionalObjectsExist : MonoBehaviour, InGameConditional
{
    public enum EvalType
    {
        AllNull, AnyNotNull
    }

    public EvalType evalType;
    public GameObject[] objects;

    public bool Evaluate()
    {
        switch (evalType)
        {
            case EvalType.AllNull:
                foreach (GameObject g in objects)
                {
                    if (g != null) { return false; }
                }
                return true;
            case EvalType.AnyNotNull:
                foreach (GameObject g in objects)
                {
                    if (g != null) { return true; }
                }
                return false;
        }

        return false;
    }

    public string GetInfo()
    {
        return "???";
    }
}
