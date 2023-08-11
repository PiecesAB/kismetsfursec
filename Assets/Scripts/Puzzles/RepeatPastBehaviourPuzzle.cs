using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatPastBehaviourPuzzle : MonoBehaviour, ITripwire
{
    public Vector2 playbackOffset;
    public Transform target;
    public Vector2[] positions = new Vector2[4096];
    public bool recording = true;
    public int currFrame = 0;
    public int lastPlaybackFrame = 0;

    public void OnTrip()
    {
        if (recording)
        {
            currFrame = 0;
            recording = false;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (currFrame >= positions.Length) { return; }
        if (!recording && currFrame > lastPlaybackFrame) { return; }
        if (recording)
        {
            positions[currFrame] = (Vector2)target.position + playbackOffset;
            lastPlaybackFrame = currFrame;
        }
        else
        {
            transform.position = positions[currFrame];
        }
        ++currFrame;
    }
}
