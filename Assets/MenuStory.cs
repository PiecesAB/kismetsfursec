using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

public class MenuStory : MenuTwoChoice
{

    public Image snapshotObj;
    public Text snapName;
    public Text snapScore;
    public Sprite placeholderSnapshot;

    public static void LoadCurrentLevelInStoryMode()
    {
        Utilities.replayLevel = false;
        LoadingScreenScript.requestedLevel = Utilities.GetPersistentGeneralLevelInfo(Utilities.loadedSaveData.levelBuildId).sceneName;
        SceneManager.LoadScene("NeutralLoader");
    }

    protected override void MakeSelection()
    {
        if (selection == 1) { Close(); if (backSound) { backSound.Stop(); backSound.Play(); } return; }
        LoadCurrentLevelInStoryMode();
    }

    protected override void ChildOpen()
    {
        selection = 0;
        UpdateArrow();
        snapName.text = Utilities.toSaveData.levelName;
        snapScore.text = "$" + Utilities.toSaveData.score;
        snapshotObj.sprite = Utilities.GetSnap(placeholderSnapshot);
    }
}
