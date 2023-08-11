using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LoadingScreenScript))]
public class LoadingRetryConfirm : MonoBehaviour
{
    // 16UL (x) = Retry
    // 64UL (o) = Give up

    public GameObject confirmation;
    public GameObject confirmationButtonInfo;
    public GameObject theEndStuff;
    public AudioClip endMusic;
    public Text theEndSubtitleObject;
    public string[] possibleSubtitles;
    public GameObject tryAgainText;

    public static string customSubtitle;
    public static bool forceGiveUp;

    private LoadingScreenScript lts;
    private Encontrolmentation e;
    private void Start()
    {
        lts = GetComponent<LoadingScreenScript>();
    }

    private void OnDestroy()
    {
        if (e) { Destroy(e); }
    }

    private void Update()
    {
        if ((e?.allowUserInput ?? false) && forceGiveUp)
        {
            bool retry = false;
            lts.confirmToRetryInput = retry;
            lts.confirmToRetry = false;

            confirmation.SetActive(false);
            Destroy(e);
            return;
        }

        if (e?.AnyButtonDown(80UL) ?? false)
        {
            bool retry = e.ButtonDown(16UL, 80UL);
            lts.confirmToRetryInput = retry;
            lts.confirmToRetry = false;
            
            confirmation.SetActive(false);
            Destroy(e);
        }
    }

    public void TheEnd()
    {
        BGMController.main.InstantMusicChange(endMusic, false);
        theEndStuff.SetActive(true);
        if (customSubtitle != "") { theEndSubtitleObject.text = customSubtitle; }
        else { theEndSubtitleObject.text = possibleSubtitles[Fakerand.Int(0, possibleSubtitles.Length)]; }
    }

    public void FinishShowingConfirmation()
    {
        tryAgainText.SetActive(true);
        e = gameObject.AddComponent<Encontrolmentation>();
        e.allowUserInput = true;
        confirmationButtonInfo.SetActive(true);
    }
}
