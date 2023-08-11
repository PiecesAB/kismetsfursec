using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRankShot : MonoBehaviour
{
    public static float rank = 1f;
    // LevelInfoContainer.Start() ensures the rank is 1 at the beginning of every level. Puzzles can do what they will to the rank.
    public string numberInCircleFormula = "";
    public string bulletSpeedFormula = "";
    public string waitTimeFormula = "";

    private float oldRank = -1f;
    private BulletHellMakerFunctions maker;

    private void Start()
    {
        RealUpdate();
    }

    private void RealUpdate()
    {
        if (!maker) { maker = GetComponent<BulletHellMakerFunctions>(); }
        if (!maker) { return; } // :(
        Dictionary<string, double> vars = new Dictionary<string, double>()
        {
            { "r", rank }
        };

        if (numberInCircleFormula != "")
        {
            maker.bulletShooterData.numberInCircle = Mathf.RoundToInt((float)FormulaParser.Evaluate(numberInCircleFormula, vars));
        }
        if (bulletSpeedFormula != "")
        {
            maker.bulletData.speed = (float)FormulaParser.Evaluate(bulletSpeedFormula, vars);
        }
        if (waitTimeFormula != "")
        {
            maker.waitTime = (float)FormulaParser.Evaluate(waitTimeFormula, vars);
        }
        oldRank = rank;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) { return; }
        if (rank != oldRank)
        {
            RealUpdate();
        }
    }
}
