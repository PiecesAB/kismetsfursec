using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LoadingScreenScript : MonoBehaviour {

    [System.Serializable]
    public struct SpecialLoadingTextCatalog
    {
        public string catName;
        public string[] loadTexts;
        public string[] doneTexts;
    }

    public Coroutine WaitForRealSeconds(float time)
    {
        return StartCoroutine(WaitForRealSecondsImpl(time));
    }

    private IEnumerator WaitForRealSecondsImpl(float time)
    {
        double startTime = DoubleTime.UnscaledTimeRunning;
        while (DoubleTime.UnscaledTimeRunning - startTime < time)
            yield return 1;
    }

    public float beginDelay;
    public float artificialLoadTimeTest;
    public float finishDelay;
    public float minimumLoadingTime = 0f;
    public float progress;
    public Text loadProgNum;
    public Text regularText;
    public GameObject loadingCircle;
    public Image timeCardBackdrop;
    public bool deleteOld;
    public double realTimeAtStartseq;
    public double realTimeAtFinishload;
    public bool finished;
    public int random;
    public string[] duringLoadingNormalTexts;
    public string[] finishedLoadingTexts;
    public bool utilitiesLastDeathSpecialLoadingTexts;
    public SpecialLoadingTextCatalog[] specialTexts;
    public AudioSource playOnFinish; // probably won't work because the scene will no longer be the active one!
    public bool confirmToRetry = false; // for game beginning
    [HideInInspector]
    public bool confirmToRetryInput = true; // true = retry; scene not changed. otherwise play funny The End and go to main menu

    private SpecialLoadingTextCatalog chosen;

    public static string requestedLevel = "";

    public static List<LoadingScreenScript> all = new List<LoadingScreenScript>();

    void Start () {
        all.Add(this);
        progress = 0f;
        deleteOld = false;
        finished = false;
        SceneManager.SetActiveScene(gameObject.scene);
        if (BGMController.main)
        {
            BGMController.main.mustFollowLevel1MinTimer = false; // because we don't want to hear the same frame of a song after quitting a timed level during the timer
        }
        StartCoroutine(Load());
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    IEnumerator Load()
    {
        Time.timeScale = 1;
        Application.targetFrameRate = 60;
        primAddScene.loadProgressTracker = 100;
        if (utilitiesLastDeathSpecialLoadingTexts)
        {
            foreach (var i in specialTexts)
            {
                if (i.catName == Utilities.toSaveData.previousDeathReason)
                {
                    duringLoadingNormalTexts = i.loadTexts;
                    finishedLoadingTexts = i.doneTexts;
                }
            }
        }

        random = Fakerand.Int(0, duringLoadingNormalTexts.Length);
        print("LoadText: " + random);
        realTimeAtStartseq = DoubleTime.UnscaledTimeRunning;
        regularText.text = duringLoadingNormalTexts[random];
        regularText.text = regularText.text.Replace("<DTL>", Utilities.loadedSaveData.deathsThisLevel.ToString());

        yield return WaitForRealSeconds(beginDelay);

        if (confirmToRetry)
        {
            GetComponent<LoadingRetryConfirm>().FinishShowingConfirmation();
            yield return new WaitUntil(() => !confirmToRetry);
            if (!confirmToRetryInput) {
                requestedLevel = "InSaveMenu";
                GetComponent<LoadingRetryConfirm>().TheEnd();
                yield return WaitForRealSeconds(5f);
            }
        }

        {
            DontDestroyOnLoad(transform.root.gameObject);
            deleteOld = true;
            AsyncOperation a = null;
            a = SceneManager.LoadSceneAsync(requestedLevel, LoadSceneMode.Single);
            if (a == null)
            {
                GameEndingError.Throw("The level that was attempted to load is not part of the universe.");
                yield break;
            }
            //yield return new WaitUntil(() => a != null);
            a.priority = 1000000000;
            a.allowSceneActivation = true;
            int tick = 480;
            
            while (a.progress < 1f)
            {
                progress = a.progress;
                loadProgNum.text = ""+(100-Mathf.FloorToInt(a.progress*111));
                primAddScene.loadProgressTracker = 100 - (int)(a.progress * 111.112f);
                yield return new WaitForEndOfFrame();
                tick--;
                if (tick <= 0)
                {
                    int random2 = random;
                    while (random2 == random)
                    {
                        random2 = Fakerand.Int(0, duringLoadingNormalTexts.Length);
                    }
                    random = random2;
                    regularText.text = duringLoadingNormalTexts[random];
                    tick = 480;
                }
            }
            if (timeCardBackdrop)
            {
                while (timeCardBackdrop.color.a >= 0.3f)
                {
                    timeCardBackdrop.color = new Color(timeCardBackdrop.color.r, timeCardBackdrop.color.g, timeCardBackdrop.color.b, timeCardBackdrop.color.a - 0.05f);
                    yield return new WaitForEndOfFrame();
                }
                timeCardBackdrop.color = new Color(timeCardBackdrop.color.r, timeCardBackdrop.color.g, timeCardBackdrop.color.b, 0.3f);
            }
            if (artificialLoadTimeTest > 0)
            {
                yield return WaitForRealSeconds(artificialLoadTimeTest);
            }

            if (playOnFinish) { playOnFinish.PlayOneShot(playOnFinish.clip); }
            realTimeAtFinishload = DoubleTime.UnscaledTimeRunning;
            double elapsed = realTimeAtFinishload - realTimeAtStartseq;
            if (elapsed < minimumLoadingTime)
            {
                yield return WaitForRealSeconds((float)(minimumLoadingTime - elapsed));
            }
            loadProgNum.text = "0";
            primAddScene.loadProgressTracker = 0;
            regularText.text = finishedLoadingTexts[random];
            progress = 1f;
            finished = true;
            deleteOld = false;
            loadingCircle.SetActive(false);
            yield return WaitForRealSeconds(finishDelay);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(requestedLevel));

            Destroy(transform.root.gameObject);
        }
    }
}
