using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class OtherBrainstormBehaviour : MonoBehaviour
{
    public bool siren;
    public AudioSource sirenSound;
    public AudioSource explodeSound;

    private ColorCorrectionCurves ccc;

    private int explodedFrames = 50;

    public GameObject[] destroyWhenTimeUp;
    public GameObject[] enableWhenTimeUp;

    void Start()
    {
        
    }

    private void TimeUp()
    {
        explodeSound.Play();
        foreach (GameObject g in destroyWhenTimeUp)
        {
            if (!g) { continue; }
            Destroy(g);
        }
        foreach (GameObject g in enableWhenTimeUp)
        {
            g.SetActive(true);
        }
    }
    
    void Update()
    {
        if (Time.timeScale == 0 || !LevelInfoContainer.timerOn) { return; }
        if (siren)
        {
            if (!ccc) { ccc = FollowThePlayer.main.GetComponent<ColorCorrectionCurves>(); }
            if (LevelInfoContainer.timer <= 0)
            {
                float flash = explodedFrames * 0.02f;
                flash *= flash;
                ccc.redChannel = AnimationCurve.Linear(flash, flash, 1f, 1f);
                ccc.greenChannel = ccc.blueChannel = AnimationCurve.Linear(flash, flash, 0.6f + 0.4f * flash, 0.6f + 0.4f * flash);
                ccc.UpdateTextures();
                sirenSound.Stop();
                if (explodedFrames == 50) {
                    TimeUp();
                }
                if (explodedFrames > 0) { --explodedFrames; }
                return;
            }
            float d = 1f;
            if (LevelInfoContainer.timer <= 10)
            {
                d = 0.8f + 0.2f * Mathf.Sin(LevelInfoContainer.timer * 4f * Mathf.PI);
                if (!sirenSound.isPlaying)
                {
                    sirenSound.Play();
                }
            }
            else if (LevelInfoContainer.timer <= 24)
            {
                d = 0.8f + 0.2f * Mathf.Sin(LevelInfoContainer.timer * Mathf.PI);
            }
            ccc.greenChannel = ccc.blueChannel = AnimationCurve.Linear(0f, 0f, 1f, d);
            ccc.UpdateTextures();
        }
    }
}
