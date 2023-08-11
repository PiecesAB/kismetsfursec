using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimDeleteOnLeaveScreen : MonoBehaviour
{
    [Header("You need to set the following")]
    public Renderer myRenderer;
    [Header("You don't need to set the following (default is this gameObject)")]
    public GameObject deleteThis;
    public bool addToBulletPoolInstead = false;

    private bool seen;
    private double activateTime;

    public float waitTimeBeforeCheck = 0.5f;

    void Start()
    {
        seen = false;
        activateTime = DoubleTime.ScaledTimeSinceLoad;
        if (waitTimeBeforeCheck == 0f && !myRenderer.isVisible)
        {
            GetReadyToDestroy(deleteThis);
        }
    }

    private void OnEnable()
    {
        activateTime = DoubleTime.ScaledTimeSinceLoad;
    }

    void GetReadyToDestroy(GameObject o)
    {
        if (addToBulletPoolInstead) // when this is an advanced bullet
        {
            BulletPool.Push(o);
        }
        else
        {
            Destroy(o);
        }
    }

    void Update()
    {
        if (Time.timeScale == 0 || !enabled) { return; }

        double timeSince = DoubleTime.ScaledTimeSinceLoad - activateTime;

        if (!seen & myRenderer.isVisible)
        {
            seen = true;
        }

        if (timeSince > waitTimeBeforeCheck && !seen)
        {
            if (deleteThis == null) { deleteThis = gameObject; }
            GetReadyToDestroy(gameObject);
        }

        if (seen & !myRenderer.isVisible)
        {
            if (deleteThis == null) { deleteThis = gameObject; }
            GetReadyToDestroy(deleteThis);

        }

    }
}
