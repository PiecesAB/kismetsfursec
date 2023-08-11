using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtremelyHeavyHammer : MonoBehaviour
{
    private float myTimer;

    const float waitBeforeDropping = 0.2f;
    const float dropTime = 0.5f;
    public AudioClip creakingNoise;
    public AudioClip crashNoise;

    private AudioSource aso;

    bool hasFell = false;
    bool wasSlowed = false;

    void Start()
    {
        myTimer = -waitBeforeDropping;
        hasFell = false;
        wasSlowed = false;
        aso = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Door1.levelComplete)
        {
            myTimer += 0.016666666f * Time.timeScale;
            if (myTimer > 0)
            {
                if (!aso.isPlaying)
                {
                    aso.clip = creakingNoise;
                    aso.Play();
                }
                float dt = myTimer / dropTime;
                if (Time.timeScale < 1f || wasSlowed)
                {
                    wasSlowed = true;
                    dt = myTimer / Time.timeScale / 2.6f;
                }
                transform.eulerAngles = Vector3.forward * Mathf.Lerp(-90, 0, Mathf.Clamp01(dt));
                bool oldHasFell = hasFell;
                if (dt >= 1f) { hasFell = true; }
                if (oldHasFell != hasFell)
                {
                    foreach (GameObject plr in LevelInfoContainer.allPlayersInLevel)
                    {
                        plr.GetComponent<KHealth>().ChangeHealth(-Mathf.Infinity, "giant hammer");
                    }
                    FollowThePlayer.main.vibSpeed = 12f;
                    aso.clip = crashNoise;
                    aso.Play();
                }
            }
        }
    }
}
