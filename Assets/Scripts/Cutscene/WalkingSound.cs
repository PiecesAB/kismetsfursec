using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingSound : MonoBehaviour
{
    public AudioSource walkingSource;
    public float normalWalkingSpeed = 32;
    public bool on = true;

    private float currTime = 0;
    private Vector3 lastPos;
    private int lastCycle = -1;

    private void Start()
    {
        if (!walkingSource) { walkingSource = GetComponent<AudioSource>(); }
        lastPos = transform.position;
    }

    private void Update()
    {
        Vector3 noYPos = new Vector3(transform.position.x, 0, transform.position.z);
        float v = (noYPos - lastPos).magnitude * Application.targetFrameRate;
        if (v >= 0.1f * normalWalkingSpeed && v <= 5.0f * normalWalkingSpeed && on)
        {
            float pitch = v / normalWalkingSpeed;
            currTime += (pitch / Application.targetFrameRate) % 1f;
            int cycle = (int)(currTime * 2f) % 2;
            if (cycle != lastCycle)
            {
                walkingSource.Stop();
                walkingSource.time = cycle * 0.5f;
                walkingSource.Play();
                walkingSource.SetScheduledEndTime(AudioSettings.dspTime + 0.3f);
                lastCycle = cycle;
            }
        }
        else
        {
            if (walkingSource.isPlaying) { walkingSource.Stop(); lastCycle = -1; }
        }
        lastPos = noYPos;
    }
}
