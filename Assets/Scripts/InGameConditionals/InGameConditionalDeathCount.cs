using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameConditionalDeathCount : MonoBehaviour, InGameConditional
{
    public enum EvalType
    {
        DeathsThisLevelInRange, DeathsThisLevelOutOfRange, DeathOccurredOfType
    }

    public EvalType evalType;
    public int min;
    public int max;
    public string deathType;
    public bool displaceOnConditionsFailed = true;
    public Collider2D disableColliderOnConditionsFailed = null;

    private void Start()
    {
        if (!Evaluate() && displaceOnConditionsFailed)
        {
            if (displaceOnConditionsFailed) { disableColliderOnConditionsFailed.enabled = false; }
            transform.position = new Vector3(10000000, 10000000, 10000000);
        }
    }

    public bool Evaluate()
    {
        int dl = Utilities.loadedSaveData.deathsThisLevel;
        switch (evalType)
        {
            case EvalType.DeathsThisLevelInRange:
                return dl >= min && dl <= max;
            case EvalType.DeathsThisLevelOutOfRange:
                return dl < min || dl > max;
            case EvalType.DeathOccurredOfType:
                Utilities.LevelInfoS levelInfo = Utilities.loadedSaveData.leveldatas[SceneManager.GetActiveScene().buildIndex];
                return levelInfo.levelDeathReasons.ContainsKey(deathType)
                    && levelInfo.levelDeathReasons[deathType] >= min && (levelInfo.levelDeathReasons[deathType] <= max || max == 0);
        }

        return false;
    }

    public string GetInfo()
    {
        return "???";
    }
}
