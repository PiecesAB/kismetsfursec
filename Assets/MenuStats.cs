using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;

public class MenuStats : InSaveMenuBase
{
    public Text deathCount;
    public Text deathDesc;

    public Text nameText;

    public Text[] deathTypeTexts = new Text[6];
    public RectTransform[] deathTypeBars = new RectTransform[6];

    public Text netScore;
    public Text totScore;
    public Text stockMult;
    public Text optMult;
    public Text itemsUsed;

    public Text currOrdinal;
    public Text highOrdinal;

    public Text conceptDate;
    public Text saveDate;
    public Transform switchBar;

    public GameObject waveMenu;
    public Sprite[] waveIconSprites;
    public Image waveIcon;
    public string[] waveDescTextStrings;
    public Text waveDescText;
    public Text waveTopText;


    private string DeathPlural()
    {
        return (Utilities.loadedSaveData.deathsTotal == 1) ? "" : "s";
    }

    private bool IsInMainMenu()
    {
        return SceneManager.GetActiveScene().name == "InSaveMenu";
    }

    protected override void ChildOpen()
    {
        deathCount.text = Utilities.loadedSaveData.deathsTotal.ToString();

        List<KeyValuePair<string, int>> sortedDeathTypes = null;

        waveMenu.SetActive(true);

        if (IsInMainMenu())
        {
            deathDesc.text = "playable being" + DeathPlural() + ".";
            sortedDeathTypes = Utilities.loadedSaveData.allDeathReasons.OrderBy(x => 1234567890-x.Value).ToList();

            int[] dateData1 = Utilities.DateDiffRaw(Utilities.loadedSaveData.dateTimeCreated, Utilities.releaseDate);
            conceptDate.text = Utilities.loadedSaveData.name + " was concieved\n" + dateData1[0] + " years, " + dateData1[1] + " days " + ((dateData1[2] == 1) ? "in the future" : "ago") + ".";

            int[] dateData2 = Utilities.DateDiffRaw(Utilities.loadedSaveData.dateTimeLastSaved, Utilities.releaseDate);
            saveDate.text = Utilities.loadedSaveData.name + " was saved\n" + dateData2[0] + " years, " + dateData2[1] + " days " + ((dateData2[2] == 1) ? "in the future" : "ago") + ".";
        }
        else
        {
            Utilities.LevelInfoS levelInfo = Utilities.loadedSaveData.leveldatas[SceneManager.GetActiveScene().buildIndex];
            deathDesc.text = "being" + DeathPlural() + " in this level.";
            sortedDeathTypes = levelInfo.levelDeathReasons.OrderBy(x => 1234567890 - x.Value).ToList();

            int[] dateData1 = Utilities.DateDiffRaw(Utilities.loadedSaveData.dateTimeCreated, Utilities.releaseDate);
            conceptDate.text = "Level was encountered\n" + dateData1[0] + " years, " + dateData1[1] + " days " + ((dateData1[2] == 1) ? "in the future" : "ago") + ".";

            saveDate.text = "";

            if (Utilities.replayLevel)
            {
                saveDate.text = "HS: $" + levelInfo.scoreTotal[Utilities.replayMode].ToString()  + "\nQC: " + Utilities.GetFormattedPreciseTime(levelInfo.winTime[Utilities.replayMode]);
            }

            waveIcon.sprite = waveIconSprites[BrainwaveReader.brainStateInt];
            waveDescText.text = waveDescTextStrings[BrainwaveReader.brainStateInt];
            waveTopText.text = BrainwaveReader.GetFullString();
        }

        waveMenu.SetActive(false);

        for (int i = sortedDeathTypes.Count; i < 6; ++i)
        {
            sortedDeathTypes.Add(new KeyValuePair<string, int>("---", 0));
        }

        nameText.text = "You are <color=#ffccff>" + Utilities.loadedSaveData.name + "</color>.";

        int deathTypeMax = sortedDeathTypes[0].Value;

        for (int i = 0; i < 6; ++i)
        {
            Text ti = deathTypeTexts[i];
            RectTransform ri = deathTypeBars[i];
            KeyValuePair<string, int> ci = sortedDeathTypes[i];
            ti.text = (i + 1) + ". " + ci.Key + " (" + ci.Value + ")";
            ti.color = Color.Lerp(Utilities.colorCycle[i + 1], Color.white, 0.8f);
            if (deathTypeMax == 0) { ri.sizeDelta = new Vector2(8f, 0f); }
            else { ri.sizeDelta = new Vector2(8f, 50f*sortedDeathTypes[i].Value/deathTypeMax); }
            ri.GetComponent<Image>().color = Color.Lerp(Utilities.colorCycle[i + 1], Color.white, 0.5f);
        }

        netScore.text = "Score: $"+Utilities.loadedSaveData.score.ToString();
        totScore.text = "Total Score: $"+(IsInMainMenu() ? Utilities.toSaveData : Utilities.loadedSaveData).totalScore.ToString();
        stockMult.text = "Stock Mult: x" + Utilities.loadedSaveData.multiplier.ToString("F3");
        optMult.text = "Option Mult: x" + Utilities.loadedSaveData.multiplierMultiplier.ToString("F3");
        itemsUsed.text = "Items used: " + Utilities.loadedSaveData.itemsUsed;

        int cl = Utilities.CalculateLevel();
        currOrdinal.text = Utilities.longLevelNames(cl);
        highOrdinal.text = Utilities.longLevelNames(Utilities.loadedSaveData.highestLevel);

        for (int i = 0; i < 32; ++i)
        {
            Image ii = switchBar.GetChild(i).GetComponent<Image>();
            ii.color = ((Utilities.loadedSaveData.switchMask & (1u << i)) != 0u) ? Color.Lerp(Utilities.colorCycle[i],Color.white,0.5f) : Color.black;
        }

    }

    protected override void ChildUpdate()
    {
        if (!IsInMainMenu() && myControl.ButtonDown(128UL, 128UL))
        {
            waveMenu.SetActive(!waveMenu.activeSelf);
        }
    }
}
