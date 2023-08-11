using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This shooter should work for sub-frame time precision
public class IncrementalShooter : MonoBehaviour
{
    public double startDelay = 0.0;
    public double waitBetweenShots = 0.1;
    public Formula2 position;
    public string rotation;
    public BulletHellMakerFunctions shooter;
    public Transform positionCenter;
    public int shotCount = -1;
    public IncrementalShooter nextShooter;
    public float nextShooterWait;
    public Renderer waitUntilOnscreen;

    public double existTime;
    private double startTime;
    private double nextShotTime;
    private int shotIndex = 0;
    private double prand;

    public void ResetShooter()
    {
        existTime = -startDelay;
        nextShotTime = 0.0;
        startTime = DoubleTime.ScaledTimeSinceLoad + startDelay;
        shotIndex = 0;
        prand = Fakerand.Double();
    }

    private void Start()
    {
        ResetShooter();
    }

    private void IncrementShot(double shotTime)
    {
        if (waitUntilOnscreen && !waitUntilOnscreen.isVisible)
        {
            return;
        }
        Dictionary<string, double> vars = new Dictionary<string, double>()
        {
            {"i", shotIndex},
            {"t", shotTime},
            {"prand", prand},
        };
        Vector3 pos = position.Evaluate(vars);
        float rot = (float)FormulaParser.Evaluate(rotation, vars);
        if (positionCenter)
        {
            transform.position = positionCenter.position + pos;
        }
        else
        {
            transform.localPosition = pos;
        }
        transform.localEulerAngles = new Vector3(0, 0, rot);
        shooter.FireAtTime(shotTime + startTime);
        ++shotIndex;
        if (shotCount > 0 && shotIndex >= shotCount)
        {
            if (nextShooter)
            {
                if (nextShooterWait > 0)
                {
                    StartCoroutine(NextShooterWait());
                }
                else
                {
                    StartNextShooter();
                }
            }
            enabled = false;
        }
    }

    private void StartNextShooter()
    {
        nextShooter.enabled = true;
        nextShooter.ResetShooter();
    }

    private IEnumerator NextShooterWait()
    {
        yield return new WaitForSeconds(nextShooterWait);
        StartNextShooter();
    }

    private void Update()
    {
        existTime += Time.timeScale / 60.0;
        while (existTime > nextShotTime)
        {
            IncrementShot(nextShotTime);
            nextShotTime += waitBetweenShots;
        }
    }
}
