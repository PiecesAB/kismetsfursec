using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discombobulator : GenericBlowMeUp
{
    public Renderer visibleCheck;
    public float timeSeen = 0f;
    public float fillTimerDuration = 3f;
    public SpriteRenderer reverseR;
    public GameObject electric;
    public GameObject effectScreen;
    public Prim3DRotate[] gyros;
    public AudioClip warnSound;
    public AudioClip activateSound;
    public AudioSource electricHum;
    public bool infinite = false;

    private int flashTimer = 0;
    private AudioSource aso;

    //private static List<Transform> allReversedPlayers = new List<Transform>();
    //private List<Transform> myReversedPlayers = new List<Transform>();

    private bool wasCompleteLastFrame = false;
    private bool wasNearlyCompleteLastFrame = false;

    private void Start()
    {
        aso = GetComponent<AudioSource>();
    }

    private void ReverseAllVisiblePlayers()
    {
        foreach (GameObject pg in LevelInfoContainer.allPlayersInLevel)
        {
            Renderer pr = pg.GetComponent<Renderer>();
            if (!pr.isVisible) { continue; }
            Transform pt = pg.transform;
            //if (allReversedPlayers.Contains(pt)) { continue; }
            //allReversedPlayers.Add(pt);
            //myReversedPlayers.Add(pt);
            pt.localScale = new Vector3(-pt.localScale.x, pt.localScale.y, pt.localScale.z);
        }

        foreach (Prim3DRotate r in gyros) {
            r.speed = -r.speed;
        }
    }

    /*private void UnreverseMyPlayers()
    {
        if (myReversedPlayers.Count == 0) { return; }
        foreach (Transform pt in myReversedPlayers)
        {
            pt.localScale = new Vector3(-pt.localScale.x, pt.localScale.y, pt.localScale.z);
            allReversedPlayers.Remove(pt);
        }
        myReversedPlayers.Clear();
    }*/

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (!visibleCheck.isVisible)
        {
            timeSeen = 0f;
            //UnreverseMyPlayers();
        }
        else
        {
            timeSeen += Time.timeScale * 0.0166666666f;
        }

        bool complete = fillTimerDuration - timeSeen <= 0f;
        bool nearlyComplete = fillTimerDuration - timeSeen <= 1.5f && !complete;
        
        electric.SetActive(nearlyComplete);

        ++flashTimer;
        if (flashTimer >= 16) { flashTimer = 0; }
        bool flash = flashTimer >= 8;
        reverseR.material.SetInt("_InvertColor", 1);
        reverseR.material.SetColor("_Color", Color.white);
        reverseR.material.SetFloat("_H", (flash && nearlyComplete) ? -1f : (Mathf.Clamp01(timeSeen / fillTimerDuration) - 1f));

        if (nearlyComplete)
        {
            if (!electricHum.isPlaying)
            {
                electricHum.Play();
            }
        }
        else
        {
            if (electricHum.isPlaying)
            {
                electricHum.Stop();
            }
        }

        if (nearlyComplete && !wasNearlyCompleteLastFrame)
        {
            aso.Stop();
            aso.clip = warnSound;
            aso.Play();
        }

        if (complete && !wasCompleteLastFrame)
        {
            ReverseAllVisiblePlayers();
            aso.Stop();
            aso.clip = activateSound;
            aso.Play();
        }

        if (complete && fillTimerDuration - timeSeen > -0.5f)
        {
            float st = -2f * (fillTimerDuration - timeSeen);
            effectScreen.SetActive(true);
            Transform et = effectScreen.transform;
            et.position = FollowThePlayer.main.transform.position;
            Vector2 etcpos = transform.position;
            Vector2 etcsize = new Vector2(1f, 0.75f) * 640f * EasingOfAccess.CubicOut(st);
            et.GetComponent<Renderer>().material.SetVector("_Cutout", new Vector4(
                etcpos.x - 0.5f * etcsize.x,
                etcpos.y - 0.5f * etcsize.y,
                etcpos.x + 0.5f * etcsize.x,
                etcpos.y + 0.5f * etcsize.y
            ));
        }
        else
        {
            effectScreen.SetActive(false);
        }

        if (infinite && fillTimerDuration - timeSeen <= -0.5f)
        {
            timeSeen = 0f;
        }

        wasCompleteLastFrame = complete;
        wasNearlyCompleteLastFrame = nearlyComplete;
    }
}
