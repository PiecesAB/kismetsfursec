using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionalChoice : MonoBehaviour, InGameConditional, IChoiceUIResponse
{
    public enum Mode
    {
        Equal, NotEqual
    }

    public Mode mode;
    public string match = "";
    public string response = "";

    public GameObject ChoiceResponse(string r)
    {
        response = r;
        return null;
    }

    public bool Evaluate()
    {
        switch (mode)
        {
            case Mode.Equal:
                return match == response;
            case Mode.NotEqual:
                return match != response;
        }
        return false;
    }

    public string GetInfo()
    {
        switch (mode)
        {
            case Mode.Equal:
                return "A choice was not equal";
            case Mode.NotEqual:
                return "A choice was equal";
        }
        return "???";
    }
}
