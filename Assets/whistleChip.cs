using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whistleChip : MonoBehaviour
{

    public TurnstileSwitch[] turnTriggers;
    public float[] turnTriggerStrengths;
    public float pitch;
    private FollowThePlayer cameraThing;

    private float breakTimer;
    private const float timerLoop = 0.4f;
    private bool timerTrigger;

    private static bool oneOnScreen = false;

    void Start()
    {
        cameraThing = FollowThePlayer.main;
        breakTimer = timerLoop;
    }

    float convertPitches(float x)
    {
        return Mathf.Exp(0.1025f * Mathf.Round(x*40f) - 3f);
    }

    void Update()
    {
        if (!cameraThing) { cameraThing = FollowThePlayer.main; }
        if (!cameraThing) { return; }
        AudioSource aud = GetComponent<AudioSource>();
        oneOnScreen = false;
        if (Time.timeScale > 0 && GetComponent<Renderer>().isVisible)
        {
            oneOnScreen = true;

            if (!aud.isPlaying)
            {
                aud.Play();
            }

            for (int i = 0; i < turnTriggers.Length; i++)
            {
                TurnstileSwitch ts = turnTriggers[i];
                if (ts)
                {
                    pitch -= turnTriggerStrengths[i] * ts.dx;

                }
            }

            pitch = Mathf.Clamp01(pitch);

            if (pitch < 0.15f)
            {
                cameraThing.vibSpeed += (0.25f - pitch) * 0.2f;
                if (timerTrigger)
                {
                    primExtraTags[] pt = FindObjectsOfType<primExtraTags>();
                    for (int i = 0; i < pt.Length; i++)
                    {
                        if (pt[i].GetComponent<Renderer>() && pt[i].GetComponent<Renderer>().isVisible && pt[i].tags.Contains("styrofoam"))
                        {
                            pt[i].GetComponent<PrimBreakable>().BreakIt(1, 90f);
                        }
                    }
                }
            }

            if (timerTrigger && pitch > 0.85f)
            {
                primExtraTags[] pt = FindObjectsOfType<primExtraTags>();
                for (int i = 0; i < pt.Length; i++)
                {
                    if (pt[i].GetComponent<Renderer>() && pt[i].GetComponent<Renderer>().isVisible && pt[i].tags.Contains("glass"))
                    {
                        pt[i].GetComponent<PrimBreakable>().BreakIt(1, 90f);
                    }
                }
            }

            aud.pitch = convertPitches(pitch);

            breakTimer -= Time.timeScale*0.01666666f;
            timerTrigger = false;
            while (breakTimer <= 0)
            {
                breakTimer += timerLoop;
                timerTrigger = true;
            }
        }
        else
        {
            if (aud.isPlaying)
            {
                aud.Stop();
            }
        }
    }

    void LateUpdate()
    {
        if (oneOnScreen)
        {
            BGMController.main?.DuckOutAtSpeed(1f);
        }
    }
}
