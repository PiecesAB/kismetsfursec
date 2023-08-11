using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameConditionalCurrPlayer : MonoBehaviour, InGameConditional
{
    public enum EvalType
    {
        IsOnList
    }

    public EvalType evalType;
    public Encontrolmentation[] players;

    public bool Evaluate()
    {
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        switch (evalType)
        {
            case EvalType.IsOnList:
                for (int i = 0; i < players.Length; ++i) { if (players[i] == e) { return true; } }
                return false;
        }

        return false;
    }

    public string GetInfo()
    {
        return "Not right player";
    }
}
