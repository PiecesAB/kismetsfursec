using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameConditionalCamerasAndPlayers : MonoBehaviour, InGameConditional
{
    public enum EvalType
    {
        PlayersOnScreenGeq,
    }

    public EvalType evalType;
    public float comp;

    private float lastValue = 0;

    public bool Evaluate()
    {
        switch (evalType)
        {
            case EvalType.PlayersOnScreenGeq:
                lastValue = 0;
                foreach (GameObject plr in LevelInfoContainer.allPlayersInLevel)
                {
                    if (plr.GetComponent<Renderer>().isVisible) { lastValue += 1f; }
                }
                return (lastValue >= comp);
        }

        return false;
    }

    public string GetInfo()
    {
        switch (evalType)
        {
            case EvalType.PlayersOnScreenGeq:
                return "There must be <!>" + comp.ToString() + "<!> playable character" + ((comp == 1f)?"":"s") + " present on the screen." +
                    " It looks like there are only " + lastValue.ToString() + "." ;
        }

        return "???";
    }
}
