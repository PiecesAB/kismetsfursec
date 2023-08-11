using UnityEngine;
using System.Collections;

public class TimedBlock : MonoBehaviour {

    public enum Type
    {
        Mercury,
    }

    public Type type;
    public float startDelay;
    public Vector2[] TimeOnRange;
    public Vector2[] TimeOffRange;
    public float warnTime;
    public float fadeTime;
    public Gradient colorAndWarningGradient;
    public Vector2[] positions;
    public bool tryNotToOverlap;
    public bool assureSynchronization;
    [Header("set true for random, false for ordered. positions are local")]
    public bool randomPosition;
    [Header("when true, positions list should be 2 long, lower upper bounds")]
    public bool randomInArea;
    [Header("the following is also starting state")]
    public bool on;
    private Vector3 origPos;
    private double t;
    public int currentState;
    public int RangeIter;
    public int PositionIter;
    private float delay;

	void Start () {
        t = startDelay;
        origPos = transform.position;
        delay = 0;
	}

    void MoveA()
    {
        if (randomInArea)
        {
            transform.position = origPos + new Vector3(16f * Mathf.Round(Fakerand.Int((int)positions[0].x, (int)positions[1].x) / 16f),
                16f * Mathf.Round(Fakerand.Int((int)positions[0].y, (int)positions[1].y) / 16f));
        }
        else
        {
            if (randomPosition)
            {
                transform.position = origPos + (Vector3)positions[Fakerand.Int(0, positions.Length)];
            }
            else
            {
                PositionIter++; PositionIter %= positions.Length;
                transform.position = origPos + (Vector3)positions[PositionIter];
            }
        }
    }
	
	void Update () {
        if (Time.timeScale > 0 && type == Type.Mercury)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            switch (currentState)
            {
                case 0: // off; change position
                    sr.material.SetFloat("_PI", -1f);
                    col.enabled = false;
                    col.isTrigger = false;
                    col.size = new Vector2(16f, 16f);
                    break;
                case 1: // off; position is changed
                    sr.material.SetFloat("_PI", -1f);
                    col.enabled = false;
                    col.isTrigger = false;
                    col.size = new Vector2(16f, 16f);
                    break;
                case 2: // turning on
                    float ev = EasingOfAccess.SineSmooth(1f - ((float)(t - DoubleTime.ScaledTimeSinceLoad) / fadeTime));
                    sr.material.SetFloat("_PI", ev - 1f);
                    sr.color = colorAndWarningGradient.Evaluate(ev);
                    col.enabled = true;
                    col.isTrigger = true;
                    col.size = new Vector2(15f, 15f);
                    break;
                case 3: // on
                    sr.material.SetFloat("_PI", 0f);
                    col.enabled = true;
                    col.isTrigger = false;
                    col.size = new Vector2(16f, 16f);
                    break;
                case 4: // warning before turning off
                    sr.material.SetFloat("_PI", 0.1f * (float)System.Math.Cos(22f * DoubleTime.ScaledTimeSinceLoad));
                    sr.color = colorAndWarningGradient.Evaluate((float)(t - DoubleTime.ScaledTimeSinceLoad)/warnTime);
                    col.enabled = true;
                    col.isTrigger = false;
                    col.size = new Vector2(16f, 16f);
                    break;
                case 5: // turning off
                    sr.material.SetFloat("_PI", EasingOfAccess.SineSmooth((float)(t - DoubleTime.ScaledTimeSinceLoad)/fadeTime) - 1f);
                    sr.color = colorAndWarningGradient.Evaluate(0f);
                    col.enabled = false;
                    col.isTrigger = false;
                    col.size = new Vector2(16f, 16f);
                    break;
                default: //sorry nothing
                    break;
            }

            if (currentState == 2 && col.IsTouchingLayers(1051392))
            {
                if (tryNotToOverlap)
                {
                    MoveA();
                }
                t += 0.016666666f;
                if (assureSynchronization)
                {
                    delay += 0.016666666f;
                }
            }

            while (t - DoubleTime.ScaledTimeSinceLoad <= 0f)
            {
                currentState++; currentState %= 6;
                switch (currentState)
                {
                    case 0:
                        RangeIter++; RangeIter %= TimeOffRange.Length;
                        MoveA();
                        t += Fakerand.Single(TimeOffRange[RangeIter].x, TimeOffRange[RangeIter].y);
                        currentState = 1;
                        break;
                    case 1:
                        //lol
                        break;
                    case 2:
                        t += fadeTime;
                        break;
                    case 3:
                        t += Fakerand.Single(TimeOnRange[RangeIter].x, TimeOnRange[RangeIter].y);
                        t -= delay;
                        delay = 0;
                        break;
                    case 4:
                        t += warnTime;
                        break;
                    case 5:
                        t += fadeTime;
                        break;

                    default:
                        break;
                }
            }

        }
    }
}
