using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpSpike : MonoBehaviour
{
    public enum State
    {
        In, Warn, Out
    }

    public State state;
    public Vector2 inTime;
    public Vector2 warnTime;
    public Vector2 outTime;
    public float offset;
    public float scale;
    public float frameSpeed = 0.1f;
    public Transform spikeObj;
    public Collider2D standBlock;
    public Collider2D hurtBlock;

    private Vector3 cycle;
    private double t;
    

    private void CalculateNextCycle()
    {
        cycle = new Vector3(
                    Fakerand.Single(inTime.x, inTime.y),
                    Fakerand.Single(warnTime.x, warnTime.y),
                    Fakerand.Single(outTime.x, outTime.y));
    }

    private void Start()
    {
        t = DoubleTime.ScaledTimeSinceLoad;
        CalculateNextCycle();
        float cycleTime = cycle.x + cycle.y + cycle.z;
        offset = Mathf.Repeat(offset, cycleTime);

        if (offset < cycle.x) { state = State.In; t += cycle.x - offset; }
        else if (offset < cycle.x + cycle.y) { state = State.Warn; t += cycle.x + cycle.y - offset; }
        else { state = State.Out; t += cycleTime - offset; }

        hurtBlock.enabled = false;
    }

    private void Update()
    {
        if (spikeObj)
        {
            float v = frameSpeed * Time.timeScale;
            //print(state);
            switch (state)
            {
                case State.In:
                    spikeObj.localScale = new Vector3(1f, Mathf.MoveTowards(spikeObj.localScale.y,0f,v), 1f);
                    hurtBlock.enabled = standBlock.enabled = false;
                    break;
                case State.Warn:
                    spikeObj.localScale = new Vector3(1f, Mathf.MoveTowards(spikeObj.localScale.y, 0.15f + 3f*Mathf.PingPong((float)(DoubleTime.ScaledTimeSinceLoad%0.1), 0.05f), v), 1f);
                    hurtBlock.enabled = standBlock.enabled = false;
                    break;
                case State.Out:
                    spikeObj.localScale = new Vector3(1f, Mathf.MoveTowards(spikeObj.localScale.y, scale, v), 1f);
                    hurtBlock.enabled = standBlock.enabled = true;
                    break;
                default:
                    break;
            }
        }

        if (t <= DoubleTime.ScaledTimeSinceLoad)
        {
            switch (state)
            {
                case State.In:
                    state = State.Warn;
                    t += cycle.y;
                    break;
                case State.Warn:
                    t += cycle.z;
                    spikeObj.localScale = new Vector3(1f, 0.1f, 1f);
                    state = State.Out;
                    break;
                case State.Out:
                    state = State.In;
                    CalculateNextCycle();
                    t += cycle.x;
                    break;
                default:
                    break;
            }
        }
    }
}
