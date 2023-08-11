using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRing : MonoBehaviour
{
    private Transform[] mices = new Transform[0];
    private float[] distances = new float[0];
    private const float initDist = 1000f;
    private float currTimeLeft;

    private float extraAngle;
    private float phaseProgress;

    public GameObject mouseObject;

    [Range(1,100)]
    public int amount;
    public AnimationCurve velocity;
    public float startTime = 60f;

    private Transform targetPlayer;

    public float SquareMaker(float angle)
    {
        return 1f / (Mathf.Abs(Mathf.Cos(0.5f * Mathf.Asin(Mathf.Sin(2f * angle)))));
    }
    
    public int GetPhase()
    {
        float rat = currTimeLeft / startTime;
        if (rat > 0.95f) { phaseProgress = 1f - Mathf.InverseLerp(0.95f, 1f, rat); return 0; }
        else if (rat > 0.5f) { phaseProgress = 1f - Mathf.InverseLerp(0.5f, 0.95f, rat); return 1; }
        else if (rat > 0.166666f) { phaseProgress = 1f - Mathf.InverseLerp(0.166666f, 0.5f, rat); return 2; }
        else if (rat > 0.0001f) { phaseProgress = 1f - Mathf.InverseLerp(0.0001f, 0.1666666f, rat); return 3; }

        phaseProgress = 1f;
        return 4;
    }

    public Transform GetActivePlayer()
    {
        for (int i = 0; i < LevelInfoContainer.allPlayersInLevel.Count; ++i)
        {
            GameObject man = LevelInfoContainer.allPlayersInLevel[i];
            if (man == null)
            {
                continue;
            }
            if (man.GetComponent<Encontrolmentation>().allowUserInput)
            {
                return man.transform;
            }
        }
        return null;
    }

    public Vector3 MakeLocationOfMouse(int i)
    {
        float thisAngle = (2f * Mathf.PI * ((float)i / amount)) + extraAngle;
        return targetPlayer.position + 
            (new Vector3(Mathf.Cos(thisAngle), Mathf.Sin(thisAngle)))*SquareMaker(thisAngle)*distances[i];
    }

    public void GenerateMices()
    {
        mices = new Transform[amount];
        distances = new float[amount];
        for (int i = 0; i < amount; ++i)
        {
            mices[i] = Instantiate(mouseObject, Vector3.zero, Quaternion.identity, transform).transform;
            distances[i] = initDist;
            mices[i].position = MakeLocationOfMouse(i);
        }
    }

    public void UpdateMicesPosition()
    {
        extraAngle += velocity.Evaluate(currTimeLeft/startTime) * Mathf.PI * 0.0333333333333333333f * Time.timeScale;
        extraAngle = Mathf.Repeat(extraAngle, Mathf.PI * 2f);

        for (int i = 0; i < amount; ++i)
        {
            int phase = GetPhase();
            float wave = 0f;
            switch (phase)
            {
                case 0: //enter
                    if (i < 1.3f * phaseProgress * amount)
                    {
                        distances[i] = Mathf.Lerp(distances[i], 128f, 0.1f);
                    }
                    break;
                case 1: //first phase
                    wave = 8f*Mathf.Clamp(2f * Mathf.Cos(currTimeLeft * Mathf.PI * 2f), -1f, 1f);
                    wave *= (i % 2 == 0) ? 1 : -1;
                    distances[i] = Mathf.MoveTowards(distances[i], Mathf.Lerp(128f, 88f, phaseProgress) + wave, 2f);
                    break;
                case 2: //second phase
                    wave = 5f * Mathf.Clamp(2f * Mathf.Cos(currTimeLeft * Mathf.PI * 3.5f), -1f, 1f);
                    wave *= (i % 2 == 0) ? 1 : -1;
                    distances[i] = Mathf.MoveTowards(distances[i], Mathf.Lerp(76f, 40f, phaseProgress) + wave, 2f);
                    break;
                case 3:
                    wave = 2f * Mathf.Cos(currTimeLeft * Mathf.PI * 5f);
                    wave *= (i % 5) - 2;
                    distances[i] = Mathf.MoveTowards(distances[i], Mathf.Lerp(36f, 10f, phaseProgress) + wave, 2f);
                    break;
                case 4:
                    distances[i] = Mathf.MoveTowards(distances[i], 14f*Mathf.Cos(i + Time.timeSinceLevelLoad*7f), 2f);
                    break;
                default:
                    break;
            }
            

            if (mices[i])
            {
                mices[i].position = MakeLocationOfMouse(i);
                float thisAngle = (360f * ((float)i / amount)) + extraAngle*Mathf.Rad2Deg;
                if (phase == 2 || phase == 3)
                {
                    mices[i].rotation = Quaternion.Lerp(mices[i].rotation,Quaternion.AngleAxis(90f + thisAngle, Vector3.forward),0.05f);
                }
            }
        }
    }

    public void HurtPlayer()
    {
        KHealth kh = targetPlayer.GetComponent<KHealth>();
        if (GetPhase() == 4 && kh)
        {
            kh.ChangeHealth(-1f, "mouse ring");
        }
    }

    void Start()
    {
       // if (targetPlayer == null) { targetPlayer = GetActivePlayer(); }
    }
    
    void Update()
    {
        if (Time.timeScale > 0 && LevelInfoContainer.timerOn)
        {
            currTimeLeft = LevelInfoContainer.timer;

            if (targetPlayer == null) { targetPlayer = GetActivePlayer(); }
            if (mices.Length == 0) { GenerateMices(); }
            UpdateMicesPosition();
            HurtPlayer();
        }
    }
}
