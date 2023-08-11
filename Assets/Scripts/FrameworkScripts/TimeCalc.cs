using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCalc : MonoBehaviour {

    public float normalTime = 1f;
    public static float extraPerFrameTime = 0f;
    public float regularSRayThickness = 16f;
    public float regularSRayVelocity = 80f;
    public float timeMinimum = 0.125f;
    public float timeMaximum = 8f;
    public BGMController bgmControl;
    public bool noBGMHere = false;
    public bool freePitchSpeed = false;

    public AudioClip speedUp;
    public AudioClip speedDown;

    private static int extraTimer = 0;

    private float lastFrameTime = 0f;
    private AudioSource myAudio;

    private static TimeCalc main;

    void Start()
    {
        main = this;
        extraTimer = 0;
        myAudio = GetComponent<AudioSource>();
    }

    public static void SetExtraPerFrameTime(float t)
    {
        extraPerFrameTime += t;
    }

    public static void ChangeNormalTime(float increment)
    {
        main.normalTime += increment;
    }

    void Update() {

        if (bgmControl == null && !noBGMHere)
        {
            BGMController nb = FindObjectOfType<BGMController>();
            if (nb != null)
            bgmControl = nb;
        }

        if (Time.timeScale > 0)
        {
            Time.timeScale = normalTime;
            float nt = normalTime;
            float tfac = extraPerFrameTime;
            foreach (SuperRay r in SuperRay.allRays)
            {
                if (r.clonedFrom) { continue; }
                tfac += (r.currentThickness / regularSRayThickness) * (r.cursorVelocity.y / regularSRayVelocity);
            }

            if (tfac > 0f)
            {
                nt += tfac * normalTime;
            }
            else if (tfac < 0f)
            {
                nt = normalTime / (1f - tfac);
            }
            Time.timeScale = Mathf.Round(Mathf.Clamp(nt, timeMinimum, timeMaximum)*1000f)/1000f;
            Time.fixedDeltaTime = Time.maximumDeltaTime = Time.maximumParticleDeltaTime = (Time.timeScale * 0.016666666f);
            if (bgmControl != null && !freePitchSpeed)
            {
                bgmControl.speed = Mathf.Clamp(Fastmath.FastSqrt(Time.timeScale), 0.5f, 2.0f);
            }

            extraPerFrameTime = 0f;

            if (lastFrameTime == 1f && Time.timeScale != 1f)
            {
                if (Time.timeScale > 1f)
                {
                    myAudio.clip = speedUp;
                }
                else
                {
                    myAudio.clip = speedDown;
                }
                myAudio.Stop();
                myAudio.Play();
            }

            if (lastFrameTime != 1f && lastFrameTime != 0f && Time.timeScale == 1f)
            {
                if (lastFrameTime < 1f)
                {
                    myAudio.clip = speedUp;
                }
                else
                {
                    myAudio.clip = speedDown;
                }
                myAudio.Stop();
                myAudio.Play();
            }
        }

        lastFrameTime = Time.timeScale;
	}
}
