using UnityEngine;

// returns true if the player has achieved or not achieved the action in story mode.
public class InGameConditionalActionData : MonoBehaviour, InGameConditional
{
    public enum Mode
    {
        IsThere, IsNotThere
    }

    public Mode mode;
    public string action;

    public bool Evaluate()
    {
        return Utilities.GetActionData(action) ^ (mode == Mode.IsNotThere);
    }

    public string GetInfo()
    {
        switch (mode)
        {
            case Mode.IsNotThere:
                return "Player has done action " + action;
            case Mode.IsThere:
                return "Player has not done action " + action;
        }
        return "???";
    }
}
