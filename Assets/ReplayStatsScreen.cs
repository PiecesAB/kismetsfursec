using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayStatsScreen : MonoBehaviour
{
    public class ReplayStats
    {
        public string levelName;
        public LevelInfoContainer.Theme levelTheme; // this mainly helps to find the level in level select.
        public int sceneId;
        public int oldDeaths;
        public int newDeaths;
        public long oldScore;
        public long newScore;
        public double oldTime;
        public double newTime;
        public bool oldFlawless;
        public bool newFlawless;
        public bool oldDerelict;
        public bool newDerelict;
        public bool newLevelUnlocked;
    }

    [System.Serializable]
    public struct Row
    {
        public Text before;
        public Text after;
        public Text change;
        public Text to;
    }

    public Sprite[] difficultySprites = new Sprite[3];
    public Image difficulty;
    public Color goodColor;
    public Color badColor;
    public Color neutralColor;
    public string arrow;
    public string cross;
    public string equals;
    public string[] improvementFlavor;
    public string[] noImprovementFlavor;
    public Text levelNameItem;
    public Row deathsItem;
    public Row scoreItem;
    public Row timeItem;
    public GameObject flawlessBox;
    public GameObject derelictBox;
    public Text flawlessText;
    public Text derelictText;
    public Text flavorText;
    public GameObject mainHolder;
    public GameObject trivialHolder;
    public Text trivialText;
    public GameObject altUnlocked;

    private static ReplayStats stats = null;

    public static void InitStats()
    {
        stats = new ReplayStats();
    }

    public static LevelInfoContainer.Theme GetStatsTheme() { return stats.levelTheme; }
    public static int GetStatsSceneId() { return stats.sceneId; }

    public static void SetOldStats(int sceneId, int replayMode) // upon entering the replay level
    {
        Utilities.LevelInfoS info = Utilities.GetPersistentGeneralLevelInfo(sceneId);
        stats.levelName = info.levelName;
        stats.levelTheme = info.theme;
        stats.sceneId = sceneId;
        stats.oldDeaths = info.deathsTotal;
        stats.oldScore = info.scoreTotal[replayMode];
        stats.oldTime = info.winTime[replayMode];
        stats.oldDerelict = ((info.scorelessWin & (1 << replayMode)) != 0);
        stats.oldFlawless = ((info.hitlessWin & (1 << replayMode)) != 0);
    }

    public static void SetNewStats(int sceneId, int replayMode, long newScore, double newTime, bool newLevelUnlocked) // upon completing the replay level
    {
        Utilities.LevelInfoS info = Utilities.GetPersistentGeneralLevelInfo(sceneId);
        stats.newDeaths = info.deathsTotal;
        stats.newScore = newScore;
        stats.newTime = newTime;
        stats.newDerelict = (newScore == 0);
        stats.newFlawless = (KHealth.hitsThisLevel == 0);
        stats.newLevelUnlocked = newLevelUnlocked;
    }

    [HideInInspector]
    public List<Text> rainbowTexts = new List<Text>();

    public static long TimeToLong(double t)
    {
        return (long)System.Math.Floor(t * 100);
    }

    private void Start()
    {
        if (!Utilities.replayLevel) { return; }

        if (Door1.completionWasTrivial)
        {
            mainHolder.SetActive(false);
            trivialHolder.SetActive(true);
            if (Door1.trivialCompletionMessageNow != "") { trivialText.text = Door1.trivialCompletionMessageNow; }
            return;
        }

        difficulty.sprite = difficultySprites[Utilities.replayMode];

        levelNameItem.text = stats.levelName;

        deathsItem.before.text = stats.oldDeaths.ToString();
        deathsItem.after.text = stats.newDeaths.ToString();
        int newDeaths = (stats.newDeaths - stats.oldDeaths);
        Color deathsToColor = (newDeaths > 0) ? goodColor : neutralColor;
        deathsItem.change.color = deathsToColor;
        deathsItem.change.text = "+" + (stats.newDeaths - stats.oldDeaths).ToString();
        deathsItem.to.color = deathsToColor;

        bool improvement = false;

        long scoreDiff = stats.newScore - stats.oldScore;
        Color scoreToColor; string scoreToArrow;
        if (scoreDiff > 0) { scoreToColor = goodColor; scoreToArrow = arrow; improvement = true; }
        else if (scoreDiff == 0) { scoreToColor = neutralColor; scoreToArrow = equals; }
        else { scoreToColor = badColor; scoreToArrow = cross; }
        scoreItem.to.color = scoreToColor;
        scoreItem.to.text = scoreToArrow;
        scoreItem.before.text = stats.oldScore.ToString();
        scoreItem.after.text = stats.newScore.ToString();
        if (scoreDiff > 0) { rainbowTexts.Add(scoreItem.after); }

        long timeDiff = TimeToLong(stats.newTime) - TimeToLong(stats.oldTime);
        Color timeToColor; string timeToArrow;
        if (timeDiff < 0) { timeToColor = goodColor; timeToArrow = arrow; improvement = true; }
        else if (timeDiff == 0) { timeToColor = neutralColor; timeToArrow = equals; }
        else { timeToColor = badColor; timeToArrow = cross; }
        timeItem.to.color = timeToColor;
        timeItem.to.text = timeToArrow;
        timeItem.before.text = Utilities.GetFormattedPreciseTime(stats.oldTime);
        timeItem.after.text = Utilities.GetFormattedPreciseTime(stats.newTime);
        if (timeDiff < 0) { rainbowTexts.Add(timeItem.after); }

        
        if (stats.newFlawless)
        {
            flawlessBox.SetActive(true);
            if (!stats.oldFlawless)
            {
                rainbowTexts.Add(flawlessText);
                improvement = true;
            }
        }
        else { flawlessBox.SetActive(false); }

        if (stats.newDerelict)
        {
            derelictBox.SetActive(true);
            if (!stats.oldDerelict)
            {
                rainbowTexts.Add(derelictText);
                improvement = true;
            }
        }
        else { derelictBox.SetActive(false); }

        if (improvement)
        {
            flavorText.text = improvementFlavor[Fakerand.Int(0, improvementFlavor.Length)];
        }
        else
        {
            flavorText.text = noImprovementFlavor[Fakerand.Int(0, noImprovementFlavor.Length)];
        }

        altUnlocked.SetActive(stats.newLevelUnlocked);
    }

    private void Update()
    {
        Color rainbowColor = Color.HSVToRGB(Fakerand.Single(), 0.4f, 1f);
        foreach (Text t in rainbowTexts)
        {
            t.color = rainbowColor;
        }
    }
}
