using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuLevelSelectConfirm : InSaveMenuBase
{
    [HideInInspector]
    public KeyValuePair<int,Utilities.LevelInfoS> levelInfo;
    [HideInInspector]
    public Sprite snap;
    public Image pic;
    public Text title;
    public Text desc;
    public Text deaths;
    public Text hiScore;
    public Text speed;
    public Image flawless;
    public Image derelict;
    public GameObject flagSample;
    public Transform[] selectionIcons;
    public Transform arrow;
    public int selection;
    public AudioSource changeSound;
    public AudioSource submitSound;

    private void UpdateDifficultySpecificStuff(int d)
    {
        if (d == 3) { return; } //No.
        hiScore.text = "$" + levelInfo.Value.scoreTotal[d].ToString();
        speed.text = Utilities.GetFormattedPreciseTime(levelInfo.Value.winTime[d]);
        int dmask = 1 << d;
        flawless.color = ((levelInfo.Value.hitlessWin & dmask) == dmask) ? Color.white : Color.black;
        derelict.color = ((levelInfo.Value.scorelessWin & dmask) == dmask) ? Color.white : Color.black;
    }

    private int minSelect = 0;
    private bool travail = false;

    // this is the score amount to start with in the level
    // first value is upper class, second value is middle class
    private Dictionary<LevelInfoContainer.Theme, int[]> scoreValues = new Dictionary<LevelInfoContainer.Theme, int[]>()
    {
        { LevelInfoContainer.Theme.Travail, new int[] {0, 0} },
        { LevelInfoContainer.Theme.Block_13, new int[] {666666, 66666} },
    };

    public static int[] scoreValuesDefault =
    {
        20000000, 1000000
    };

    // first value is upper class, second value is middle class
    public static int[] currScoreValues = new int[2];

    protected override void ChildOpen()
    {
        // travail levels have the player normally at 0 score so disallow all options but "lower class"
        var theme = levelInfo.Value.theme;
        travail = theme == LevelInfoContainer.Theme.Travail;
        minSelect = travail ? 2 : 0;
        selectionIcons[0].gameObject.SetActive(!travail);
        selectionIcons[1].gameObject.SetActive(!travail);

        currScoreValues = scoreValues.ContainsKey(theme) ? scoreValues[theme] : scoreValuesDefault;

        selection = (minSelect == 0) ? 1 : 2;
        pic.sprite = snap;
        title.text = levelInfo.Value.levelName;
        desc.text = levelInfo.Value.levelDescription;
        deaths.text = levelInfo.Value.deathsTotal.ToString();

        Transform flagHolder = flagSample.transform.parent;
        List<GameObject> deleteThese = new List<GameObject>();
        for (int i = 1; i < flagHolder.childCount; ++i) { deleteThese.Add(flagHolder.GetChild(i).gameObject); }
        for (int i = 0; i < deleteThese.Count; ++i) { Destroy(deleteThese[i]); }
        flagSample.SetActive(true);
        for (int i = 0; i < levelInfo.Value.maxFlagsInLevel; ++i)
        {
            GameObject newFlag = Instantiate(flagSample);
            newFlag.transform.SetParent(flagHolder);
            newFlag.transform.localPosition = new Vector3(12f * i, 0, 0);
            newFlag.transform.GetChild(0).gameObject.SetActive(i < levelInfo.Value.flagsTotal);
        }
        flagSample.SetActive(false);

        UpdateDifficultySpecificStuff(selection);
    }

    private void Submit()
    {
        if (selection == 3) {
            if (backSound)
            {
                GameObject bs2 = Instantiate(backSound.gameObject, null);
                Destroy(bs2, 1f);
                bs2.GetComponent<AudioSource>().Play();
            }
            Close();
            return;
        }
        Utilities.replayMode = selection;
        Utilities.replayLevel = true;
        LoadingScreenScript.requestedLevel = levelInfo.Value.sceneName;
        ReplayStatsScreen.InitStats();
        ReplayStatsScreen.SetOldStats(levelInfo.Value.levelBuildId, selection);
        Utilities.SetLevelInInfo("");
        SceneManager.LoadScene("NeutralLoader");
    }

    protected override void ChildUpdate()
    {
        if (myControl.ButtonDown(4UL, 12UL) && selection > minSelect) { --selection; changeSound.Stop(); changeSound.Play(); UpdateDifficultySpecificStuff(selection); }
        if (myControl.ButtonDown(8UL, 12UL) && selection < 3) { ++selection; changeSound.Stop(); changeSound.Play(); UpdateDifficultySpecificStuff(selection); }

        if (myControl.ButtonDown(16UL, 16UL)) { submitSound.Stop(); submitSound.Play(); Submit(); }

        Vector3 left = new Vector3(-18 + (float)(4 * Math.Sin(DoubleTime.UnscaledTimeSinceLoad*6.0)), 0, 0);
        arrow.localPosition = selectionIcons[selection].localPosition + left;
    }
}
