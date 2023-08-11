using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Formula2
{
    public string x;
    public string y;

    public Vector2 Evaluate(Dictionary<string, double> vars)
    {
        return new Vector2(
            (float)FormulaParser.Evaluate(x, vars),
            (float)FormulaParser.Evaluate(y, vars));
    }

    Formula2(string x, string y)
    {
        this.x = x;
        this.y = y;
    }
}
