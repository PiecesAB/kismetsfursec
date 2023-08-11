using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMaker2 : MonoBehaviour
{
    public AnimationCurve windMulOverTime;
    public ParticleSystem windParticles;

    void Start()
    {
        
    }

    void Update()
    {
        if (Time.timeScale == 0 || !LevelInfoContainer.timerOn) { return; }
        float t = windMulOverTime.Evaluate(LevelInfoContainer.timer * 0.01666666f);
        Physics2D.gravity = new Vector2(16 * t, -8);
        ParticleSystem.MainModule pm = windParticles.main;
        ParticleSystem.EmissionModule pe = windParticles.emission;
        pm.startLifetime = Mathf.Min(0.36f / t, 1.5f);
        pm.startSpeed = 1280 * t;
        pe.rateOverTime = 256 * t;
        AudioSource asc = GetComponent<AudioSource>();
        if (t < 0.2f) { if (asc.isPlaying) { asc.Stop(); } }
        else if (!asc.isPlaying) { asc.Play(); }
        asc.pitch = 1.5f * t;

    }
}
