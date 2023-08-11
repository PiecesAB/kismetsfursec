using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuConfirmBox : MenuTwoChoice
{
    public bool toSaveMenu = true;

    private void Save()
    {
        if (!LevelInfoContainer.main) { return; } // this box is used in other menus, so only save level info if this is a level.
        if (!Door1.levelComplete) { Utilities.StopGameTimerHere(); }
        if (!Utilities.replayLevel)
        {
            int bid = SceneManager.GetActiveScene().buildIndex;
            Utilities.loadedSaveData.leveldatas[bid] = new Utilities.LevelInfoS(Utilities.toSaveData.leveldatas[bid]);
            Utilities.SaveGame(Utilities.activeGameNumber);
        }
    }

    protected override void MakeSelection()
    {
        if (selection == 1) { Close(); if (backSound) { backSound.Stop(); backSound.Play(); } return; }
        // 0
        if (toSaveMenu)
        {
            LoadingScreenScript.requestedLevel = "InSaveMenu";
            Save();
            Utilities.loadedSaveData = new Utilities.SaveData(Utilities.toSaveData);
        }
        else
        {
            Save();
            LoadingScreenScript.requestedLevel = "TitleMenu";
            Utilities.loadedSaveData = new Utilities.SaveData();
            Utilities.toSaveData = new Utilities.SaveData();
        }

        SceneManager.LoadScene("NeutralLoader");
    }
}
