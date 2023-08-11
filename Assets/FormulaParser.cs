using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormulaParser
{
    private static List<string> operators = new List<string>()
    {
        "*", "/", "+", "-"
    };

    private static List<string> functions = new List<string>()
    {
        "sin", "cos", "min", "max", "sqrt"
    };

    private static Dictionary<string, double> constants = new Dictionary<string, double>()
    {
        {"pi", 3.141592653589793},
        {"e", 2.718281828459045}
    };

    public static double Evaluate(string formula)
    {
        return Evaluate(formula, new Dictionary<string, double>());
    }

    private static void AssertArgCount(ref Stack<double> stack, int count)
    {
        if (stack.Count < count)
        {
            throw new System.Exception("Too few numbers");
        }
    }

    public static double Evaluate(string formula, Dictionary<string, double> vars)
    {
        if (vars.ContainsKey(formula))
        {
            return vars[formula];
        }

        double justNumber = 0.0;
        if (double.TryParse(formula, out justNumber))
        {
            return justNumber;
        }

        Stack<string> opStack = new Stack<string>();
        List<string> postfix = new List<string>();

        // Separate tokens
        List<string> tokens = new List<string>();
        string currToken = "";
        foreach (char c in formula)
        {
            if (!char.IsLetterOrDigit(c) && c != '.')
            {
                if (currToken != "") { tokens.Add(currToken); }
                tokens.Add(c.ToString());
                currToken = "";
            } else
            {
                currToken += c;
            }
        }
        if (currToken != "") { tokens.Add(currToken); }

        // Convert to postfix
        foreach (string t in tokens)
        {
            double v = 0.0; // useless value
            if (double.TryParse(t, out v))
            {
                postfix.Add(t);
            }
            else if (vars.ContainsKey(t))
            {
                postfix.Add(vars[t].ToString());
            }
            else if (constants.ContainsKey(t))
            {
                postfix.Add(constants[t].ToString());
            }
            else if (functions.Contains(t))
            {
                opStack.Push(t);
            }
            else if (operators.Contains(t))
            {
                while (opStack.Count > 0 
                    && operators.Contains(opStack.Peek())
                    && operators.IndexOf(opStack.Peek()) <= operators.IndexOf(t))
                {
                    postfix.Add(opStack.Pop());
                }
                opStack.Push(t);
            }
            else if (t == "(")
            {
                opStack.Push(t);
            }
            else if (t == ")")
            {
                while (opStack.Count > 0 && opStack.Peek() != "(")
                {
                    postfix.Add(opStack.Pop());
                }
                if (opStack.Count == 0)
                {
                    throw new System.Exception("Parentheses imbalanced");
                }
                opStack.Pop();
                if (opStack.Count > 0 && functions.Contains(opStack.Peek()))
                {
                    postfix.Add(opStack.Pop());
                }
            }
            else if (t == ",")
            {
                continue;
            }
            else
            {
                throw new System.Exception("Unknown token: " + t);
            }
        }
        while (opStack.Count > 0)
        {
            if (opStack.Peek() == "(")
            {
                throw new System.Exception("Parentheses imbalanced");
            }
            postfix.Add(opStack.Pop());
        }

        // Solve postfix
        Stack<double> numStack = new Stack<double>();
        foreach (string t in postfix)
        {
            double v = 0.0; // useless value
            if (double.TryParse(t, out v))
            {
                numStack.Push(v);
            }
            else if (t == "+")
            {
                AssertArgCount(ref numStack, 2);
                numStack.Push(numStack.Pop() + numStack.Pop());
            }
            else if (t == "-")
            {
                AssertArgCount(ref numStack, 2);
                numStack.Push(-numStack.Pop() + numStack.Pop());
            }
            else if (t == "*")
            {
                AssertArgCount(ref numStack, 2);
                numStack.Push(numStack.Pop() * numStack.Pop());
            }
            else if (t == "/")
            {
                AssertArgCount(ref numStack, 2);
                numStack.Push((1.0/numStack.Pop()) * numStack.Pop());
            }
            else if (t == "sin")
            {
                AssertArgCount(ref numStack, 1);
                numStack.Push(System.Math.Sin(numStack.Pop()));
            }
            else if (t == "cos")
            {
                AssertArgCount(ref numStack, 1);
                numStack.Push(System.Math.Cos(numStack.Pop()));
            }
            else if (t == "sqrt")
            {
                AssertArgCount(ref numStack, 1);
                numStack.Push(System.Math.Sqrt(numStack.Pop()));
            }
            else if (t == "min")
            {
                AssertArgCount(ref numStack, 2);
                numStack.Push(System.Math.Min(numStack.Pop(), numStack.Pop()));
            }
            else if (t == "max")
            {
                AssertArgCount(ref numStack, 2);
                numStack.Push(System.Math.Max(numStack.Pop(), numStack.Pop()));
            }
        }

        if (numStack.Count != 1)
        {
            throw new System.Exception("Weird amount of numbers or operations");
        }

        return numStack.Pop();
    }
}
