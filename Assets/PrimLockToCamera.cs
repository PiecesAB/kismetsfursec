using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimLockToCamera : MonoBehaviour
{
    private Vector3 relativePos;
    private bool started = false;
    public bool xAxisLocked = true;
    public bool yAxisLocked = true;
    public Vector3 minBound = new Vector3(-1000000, -1000000, -1000000);
    public Vector3 maxBound = new Vector3(1000000, 1000000, 1000000);

    void Start()
    {
        if (!FollowThePlayer.main) { return; }
        started = true;
        relativePos = transform.position - FollowThePlayer.main.transform.position;
    }

    void Update()
    {
        if (!FollowThePlayer.main) { return; }
        if (!started)
        {
            started = true;
            relativePos = transform.position - FollowThePlayer.main.transform.position;
        }
        Vector3 oldPos = transform.position;
        Vector3 newPos = FollowThePlayer.main.transform.position + relativePos;
        oldPos = transform.position = new Vector3((xAxisLocked ? newPos.x : oldPos.x), (yAxisLocked ? newPos.y : oldPos.y), newPos.z);
        transform.position = new Vector3(
            Mathf.Clamp(oldPos.x, minBound.x, maxBound.x),
            Mathf.Clamp(oldPos.y, minBound.y, maxBound.y),
            Mathf.Clamp(oldPos.z, minBound.z, maxBound.z)
        );
    }
}
