using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class primAddScene : MonoBehaviour, ITextBoxDeactivate {

    public bool activate;
    public string sceneName;
    public LoadSceneMode loadMode;
    public string loadStoreIfYouWant;
    public string levelInInfo1; //set to nothing for default level start
    public string setCardTitle = "";
    public string setCardSubtitle = "";
    public string giveUpText = "";
    public bool forceGiveUp = false;
    public int doorPlayer = -1;
    public bool doorChange;
    public bool autostart = false;
    public static int loadProgressTracker;
    public static bool started;
    public bool physicsTrigger = false; // if true, it will activate by OnTriggerEnter2D.
    public bool autowin = false; //if true, it will assume the level was complete even without a door, so save stats.
    public bool deactivatesByTextBox = true;

    void Start()
    {
        activate = false;
        doorPlayer = -1;
        if (GetComponent<Door1>())
        {
            doorChange = false;
        }
        loadProgressTracker = 0;
        started = false;
    }

    public IEnumerator TrackProgress(AsyncOperation a)
    {
        while (a.progress < 1f)
        {
            //print(a.progress * 111.112f);
            loadProgressTracker = 100 - (int)(a.progress * 111.112f);
            yield return new WaitForEndOfFrame();
        }
        loadProgressTracker = 0;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }
	
	void Update ()
    {
	if (!started && (activate || autostart))
        {
            if (loadStoreIfYouWant != "") { LoadingScreenScript.requestedLevel = loadStoreIfYouWant; }
            if (setCardTitle != "") { LoadingTransitionalScreen.cardTitle = setCardTitle; }
            if (setCardSubtitle != "") { LoadingTransitionalScreen.cardSubtitle = setCardSubtitle; }
            LoadingRetryConfirm.customSubtitle = giveUpText;
            LoadingRetryConfirm.forceGiveUp = forceGiveUp;
            if (doorChange)
            {
                Utilities.SetLevelInInfo(levelInInfo1);
            }
            if (((Door1.levelComplete && !KHealth.someoneDied) || autowin) && LevelInfoContainer.main) // if level is win, it's time to save stats
            {
                Door1.levelComplete = true; // this should always be true if the level was beaten (so the replay stats screen knows when to display)
                Utilities.StopGameTimerHere(); // don't worry, double stopping has no effect because LevelInfoContainer only un-stops once per level
                if (!Utilities.replayLevel)
                {
                    Utilities.toSaveData.deathsThisLevel = Utilities.loadedSaveData.deathsThisLevel = 0;
                    Utilities.SoftSave(); //great more spaghetti code.
                }
                else //we are in replay mode
                {
                    // change the inventory back, otherwise it will save and ruin the story mode inventory.
                    Utilities.loadedSaveData.SharedPlayerItems = new List<int>(Utilities.toSaveData.SharedPlayerItems);
                    Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.ScoreTotal, LevelInfoContainer.nonMultipliedScoreInLevel);
                    if (KHealth.hitsThisLevel == 0)
                    {
                        Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.Hitless, 1);
                    }
                    if (autowin) // normally the door handles record times, but otherwise we use this.
                    {
                        Door1.levelCompleteTime = DoubleTime.UnscaledTimeSinceLoad - 0.016666666666666666;
                        Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.WinTime, Door1.levelCompleteTime);
                    }
                    bool replayNewLevelUnlocked = false;
                    {
                        // this tries to get the next level, so players may easily unlock levels from alternate paths w/o playing through the story again.
                        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
                        {
                            string path = SceneUtility.GetScenePathByBuildIndex(i);
                            string[] spath = path.Split('/', '\\');
                            if (spath.Length < 3) { continue; }
                            string currSceneName = spath[spath.Length - 1].Split('.')[0];
                            if (currSceneName.ToUpper() != loadStoreIfYouWant.ToUpper()) { continue; }
                            string levelCategory = spath[spath.Length - 2];
                            string theStringLevels = spath[spath.Length - 3];
                            if (theStringLevels != "Levels") { continue; }
                            LevelInfoContainer.Theme currTheme = LevelInfoContainer.Theme.Invalid;
                            if (!System.Enum.TryParse(levelCategory, out currTheme)) { continue; }
                            // there is now one scene to add
                            if (!Utilities.loadedSaveData.leveldatas.ContainsKey(i))
                            {
                                replayNewLevelUnlocked = true;
                                Utilities.loadedSaveData.leveldatas.Add(i, new Utilities.LevelInfoS("???", i, currSceneName, currTheme));
                            }
                            break;
                        }
                    }
                    Utilities.SaveGame(Utilities.activeGameNumber);
                    ReplayStatsScreen.SetNewStats(
                        SceneManager.GetActiveScene().buildIndex, 
                        Utilities.replayMode, 
                        LevelInfoContainer.nonMultipliedScoreInLevel, 
                        Door1.levelCompleteTime, 
                        replayNewLevelUnlocked
                    );
                    sceneName = "InSaveMenu";
                    // go back to the level select screen
                }
            }
            AsyncOperation a = SceneManager.LoadSceneAsync(sceneName, loadMode);
            a.allowSceneActivation = true;
            a.priority = 1000000000;
            started = true;
            if (LevelInfoContainer.main)
            {
                Time.timeScale = 0;
            }
            if (BGMController.main)
            {
                BGMController.main.mustFollowLevel1MinTimer = false;
            }
            StartCoroutine(TrackProgress(a));
            //Destroy(this);
        }
	}

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (physicsTrigger && col.gameObject.layer == 20)
        {
            activate = true;
        }
    }

    public void OnTextBoxDeactivate()
    {
        if (deactivatesByTextBox)
        {
            activate = true;
        }
    }
}
