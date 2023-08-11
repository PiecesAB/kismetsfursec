using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Linq;

public static class Utilities {

    public static int activeGameNumber = -1;

    public static bool replayLevel = false; //when true, we are playing from level select instead of story
    public static int replayMode = 1; // 0 is easy, 1 is medium, 2 is hard
    public static bool showTimer = false;

    public static bool thisIsTheSecondWindow = false; //for special puzzle

    public static DateTime releaseDate = new DateTime(2106, 9, 15);

    [System.Serializable]
    public class LevelInfoS
    {
        public Dictionary<int,int> persistIDs;
        public string levelName;
        public int levelBuildId;
        public LevelInfoContainer.Theme theme;
        public string levelDescription;
        public string sceneName;
        public string previewScreenshotPath;

        public int[] scoreTotal; //[0] for easy, [1] for medium, [2] for hard
        public int deathsTotal;
        public Dictionary<string, int> levelDeathReasons;
        public int flagsTotal; //can't collect flags in easy mode
        public int maxFlagsInLevel;
        public double[] winTime; //[0] for easy, [1] for medium, [2] for hard
        public int scorelessWin; // mask with 1 for easy, 2 for medium, 4 for hard
        public int hitlessWin; // mask with 1 for easy, 2 for medium, 4 for hard

        public DateTime discoveryTime;

        public LevelInfoS()
        {
            persistIDs = new Dictionary<int, int>() { };
            levelName = "";
            levelBuildId = -1;
            levelDescription = "???";
            sceneName = "";
            previewScreenshotPath = "";
            scoreTotal = new int[3] { 0, 0, 0 };
            deathsTotal = 0;
            levelDeathReasons = new Dictionary<string, int>();
            flagsTotal = 0;
            maxFlagsInLevel = 0;
            winTime = new double[3] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity };
            scorelessWin = 0;
            hitlessWin = 0;
            theme = LevelInfoContainer.Theme.Test;
            discoveryTime = DateTime.Now;
        }

        public LevelInfoS(string lName, int lBuildId, string scName, LevelInfoContainer.Theme thm)
        {
            persistIDs = new Dictionary<int, int>();
            levelName = lName;
            levelBuildId = lBuildId;
            levelDescription = "???";
            sceneName = scName;
            previewScreenshotPath = "";
            scoreTotal = new int[3] { 0, 0, 0 };
            deathsTotal = 0;
            levelDeathReasons = new Dictionary<string, int>();
            flagsTotal = 0;
            maxFlagsInLevel = 0;
            winTime = new double[3] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity };
            scorelessWin = 0;
            hitlessWin = 0;
            theme = thm;
            discoveryTime = DateTime.Now;
        }


        public LevelInfoS(LevelInfoS l)
        {
            persistIDs = new Dictionary<int, int>(l.persistIDs);
            levelName = l.levelName;
            levelBuildId = l.levelBuildId;
            levelDescription = l.levelDescription;
            sceneName = l.sceneName;
            previewScreenshotPath = l.previewScreenshotPath;
            scoreTotal = l.scoreTotal;
            deathsTotal = l.deathsTotal;
            levelDeathReasons = new Dictionary<string, int>(l.levelDeathReasons);
            flagsTotal = l.flagsTotal;
            maxFlagsInLevel = l.maxFlagsInLevel;
            winTime = l.winTime;
            scorelessWin = l.scorelessWin;
            hitlessWin = l.hitlessWin;
            theme = l.theme;
            discoveryTime = l.discoveryTime;
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public float SharedPlayersHP;
        public List<int> SharedPlayerItems;
        public int itemsUsed;
        public int CurrItemIndex;

        public string levelName;
        public int levelBuildId;
        public string levelInInfo; // e.g. the checkpoint the player spawned at
        public string levelBeforeThisName;
        public int deathsThisLevel;
        public int deathsLastLevel;
        public float deathTimeLastLevel;
        public Dictionary<string, int> allDeathReasons;
        public string previousDeathReason;
        public uint switchMask;

        public HashSet<string> actionsData; // track what the player did in story mode.
        public string[] corruptionDialog;
        public int score;
        public long totalScore;
        public float multiplier;
        public float multiplierMultiplier;
        public int highestLevel;
        public int deathsTotal;
        public int deathsTotalStory;
        public float entropy;

        public DateTime dateTimeCreated;
        public DateTime dateTimeLastSaved;
        public double gameTimePlayed;

        public string name;
        public int icon;
        public Dictionary<int, LevelInfoS> leveldatas;
        public bool needsSetup; // if true, the file has no name or icon yet; set up before playing

        public SaveData()
        {
            SharedPlayersHP = 0f;
            SharedPlayerItems = new List<int>();// {216, 103,104,102,105,105,216, 901, 217}; //test items...
            itemsUsed = 0;
            CurrItemIndex = -1;

            levelName = "";
            levelBuildId = 14;
            levelInInfo = "";

            levelBeforeThisName = "[No Level]";
            deathsThisLevel = 0;
            deathsLastLevel = 0;
            previousDeathReason = "None";
            deathTimeLastLevel = -1;
            allDeathReasons = new Dictionary<string, int>();
            switchMask = 0;

            actionsData = new HashSet<string>();
            corruptionDialog = new string[0];
            score = 0;
            totalScore = 0;
            multiplier = 1f;
            multiplierMultiplier = 1f;
            highestLevel = 0;
            deathsTotal = 0;
            deathsTotalStory = 0;
            entropy = UnityEngine.Random.value;
           dateTimeCreated = DateTime.Now;
           dateTimeLastSaved = DateTime.Now;
           gameTimePlayed = 0;
           nothing1 = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0    (    19 61 11 40    )    ";

            name = "Untitled";
            icon = -1;
            // cutscenes are to be stored in level data. however, they should not show up in the level select.
            leveldatas = new Dictionary<int, LevelInfoS>() {
                {14, new LevelInfoS("",14,"Cutscene1", LevelInfoContainer.Theme.Cutscene)},
            };

            needsSetup = true;
        }

        public SaveData(SaveData s)
        {
            SharedPlayersHP = s.SharedPlayersHP;
            SharedPlayerItems = new List<int>(s.SharedPlayerItems);
            itemsUsed = s.itemsUsed;
            CurrItemIndex = s.CurrItemIndex;

            levelName = s.levelName;
            levelBuildId = s.levelBuildId;
            levelInInfo = s.levelInInfo;

            levelBeforeThisName = s.levelBeforeThisName;
            deathsThisLevel = s.deathsThisLevel;
            deathsLastLevel = s.deathsLastLevel;
            previousDeathReason = s.previousDeathReason;
            allDeathReasons = new Dictionary<string, int>(s.allDeathReasons);
            deathTimeLastLevel = s.deathTimeLastLevel;
            switchMask = s.switchMask;

            actionsData = new HashSet<string>(s.actionsData);
            score = s.score;
            totalScore = s.totalScore;
            multiplier = s.multiplier;
            multiplierMultiplier = s.multiplierMultiplier;
            highestLevel = s.highestLevel;
            deathsTotal = s.deathsTotal;
            deathsTotalStory = s.deathsTotalStory;
            entropy = s.entropy;
            dateTimeCreated = s.dateTimeCreated;
            dateTimeLastSaved = s.dateTimeCreated;
            gameTimePlayed = s.gameTimePlayed;
            nothing1 = s.nothing1;

            name = s.name;
            icon = s.icon;
            leveldatas = new Dictionary<int, LevelInfoS>() { };
            for (int i = 0; i < s.leveldatas.Count; i++)
            {
                var c = s.leveldatas.ElementAt(i);
                leveldatas.Add(c.Key, new LevelInfoS(c.Value));
            }
            needsSetup = s.needsSetup;
        }
        /* 
        Subtracting the created from lastSaved gives 
        a TimeSpan class object, where you can detect the 
        number of minutes and hours that have been played
         */
        public string nothing1;
    }

    public static string lastCheatCode = "";
    public static bool FIDGETMODE = false;
    public static bool UBERSATURATED = false;
    public static float lastTimeChange = 0f;

    public static double tetrahedronTime = -5f;
    public static int tetrahedronCombo = 0;

    public static SaveData toSaveData = new SaveData();
    public static SaveData loadedSaveData = new SaveData();
    private static SaveData emergencySaveData = new SaveData();
    public static int currentSaveNumber = -1;
    public static bool canPauseGame = true;
    public static bool canUseInventory = true;

    public static void AddAction(string action)
    {
        loadedSaveData.actionsData.Add(action);
    }

    public static bool HasAction(string action)
    {
        return loadedSaveData.actionsData.Contains(action);
    }

    public static void ChangePlrMvtRequest(byte mvtData)
    {
        foreach (MvtBasedBlock g in MvtBasedBlock.all)
        {
            g.TRIGGERED(mvtData);
        }
    }

    public static bool IsSwitchSet(int id)
    {
        if (id < 0 || id > 31) { return false; }
        return ((1u << id) & loadedSaveData.switchMask) != 0u;
    }

    public static void ChangeSwitchRequest(uint nmask)
    {
        loadedSaveData.switchMask = nmask ^ loadedSaveData.switchMask;
        SwitchBlockBetter.ChangedAll(loadedSaveData.switchMask);
        foreach (SwitchChanger g in SwitchChanger.all)
        {
            g.Changed(loadedSaveData.switchMask);
        }
        FlipPanel.SwitchUpdate(nmask);
    }

    private static void EnsurePersistentDataObject(ref LevelInfoS main, SaveData save, int sc)
    {
        if (!save.leveldatas.ContainsKey(sc))
        {
            save.leveldatas.Add(sc, new LevelInfoS());
        }
        main = save.leveldatas[sc];
    }

    public static void ChangePersistentData(GameObject g, int i, bool toSaveAsWell = false)
    {
        int sc = SceneManager.GetActiveScene().buildIndex;
        int id = GetLocalIDInFile(g);
        LevelInfoS ls = null;
        EnsurePersistentDataObject(ref ls, loadedSaveData, sc);
        if (!ls.persistIDs.ContainsKey(id))
        {
            ls.persistIDs.Add(id, i);
        }
        else
        {
            ls.persistIDs[id] = i;
        }
        loadedSaveData.leveldatas[sc] = ls;
        if (toSaveAsWell)
        {
            toSaveData.leveldatas[sc] = new LevelInfoS(ls);
        }
    }

    public enum PersistentValueSpecialMode
    {
        LevelName, LevelDescription, Theme , Screenshot, ScoreTotal, DeathsTotal, FlagsTotal, WinTime, Hitless
    }

    public static void UpdateCurrentLevel(string lName)
    {
        loadedSaveData.levelName = toSaveData.levelName = lName;
        loadedSaveData.levelBuildId = toSaveData.levelBuildId = SceneManager.GetActiveScene().buildIndex;
    }

    public static Sprite GetSnap(int id, Sprite placeholderSnapshot, Texture2D snapTex)
    {
        string screenshotFile = screenshotPath;
        if (!Directory.Exists(screenshotFile))
        {
            Directory.CreateDirectory(screenshotFile);
        }
        string screenshotImage = screenshotFile + "\\" + id + ".png";

        if (!Directory.EnumerateFiles(screenshotFile, "*.png", SearchOption.AllDirectories).Contains(screenshotImage))
        {
            return placeholderSnapshot;
        }
        snapTex.LoadImage(File.ReadAllBytes(screenshotImage));
        return Sprite.Create(snapTex, new Rect(0f, 0f, snapTex.width, snapTex.height), Vector3.one * 0.5f, 1f);
    }

    public static Sprite GetSnap(int id, Sprite placeholderSnapshot)
    {
        return GetSnap(loadedSaveData.levelBuildId, placeholderSnapshot, new Texture2D(320,180));
    }

    public static Sprite GetSnap(Sprite placeholderSnapshot)
    {
        return GetSnap(loadedSaveData.levelBuildId, placeholderSnapshot, new Texture2D(320, 180));
    }

    public static void ChangeScore(int amt) //encapsulated at last!
    {
        loadedSaveData.score += amt;
        if (amt > 0) { loadedSaveData.totalScore += amt; }
    }

    public static void ChangePersistentDataSpecial<T>(PersistentValueSpecialMode mode, T val)
    {
        int sc = SceneManager.GetActiveScene().buildIndex;
        LevelInfoS ls = null;
        EnsurePersistentDataObject(ref ls, loadedSaveData, sc);
        LevelInfoS ts = null;

        if (replayLevel)
        {
            Debug.Log("Replay difficulty is: " + replayModeNames[replayMode]);
        }

        switch (mode)
        {
            case PersistentValueSpecialMode.LevelName:
                EnsurePersistentDataObject(ref ts, toSaveData, sc);
                if (typeof(T) == typeof(string)) {
                    ls.levelName = ts.levelName = val as string;
                    Debug.Log(ts.levelName + " is this level's name");
                }
                ls.sceneName = ts.sceneName = SceneManager.GetActiveScene().name;
                ls.levelBuildId = ts.levelBuildId = sc;
                ls.maxFlagsInLevel = GameObject.FindObjectsOfType<PrimCollectibleAux>().Where((a) => (a.type == PrimCollectibleAux.Type.YBFlag)).Count();
                break;
            case PersistentValueSpecialMode.Theme:
                EnsurePersistentDataObject(ref ts, toSaveData, sc);
                if (typeof(T) == typeof(string))
                {
                    ls.theme = ts.theme = (LevelInfoContainer.Theme)Enum.Parse(typeof(LevelInfoContainer.Theme), val as string, true);
                    Debug.Log(ts.theme + " is this level's theme");
                }
                break;
            case PersistentValueSpecialMode.LevelDescription:
                EnsurePersistentDataObject(ref ts, toSaveData, sc);
                if (typeof(T) == typeof(string))
                {
                    ls.levelDescription = ts.levelDescription = val as string;
                    Debug.Log(ts.levelDescription + " is this level's description");
                }
                break;
            case PersistentValueSpecialMode.Screenshot:
                EnsurePersistentDataObject(ref ts, toSaveData, sc);
                if (typeof(T) == typeof(string))
                {
                    ls.previewScreenshotPath = ts.previewScreenshotPath = val as string;
                    Debug.Log(ts.previewScreenshotPath + " is the path of the screenshot");
                }
                break;
            case PersistentValueSpecialMode.DeathsTotal:
                EnsurePersistentDataObject(ref ts, toSaveData, sc);
                ++ls.deathsTotal;
                ++ts.deathsTotal;
                if (typeof(T) == typeof(string))
                {
                    string sval = val as string;
                    Debug.Log(sval + " is the death reason");
                    if (ls.levelDeathReasons.ContainsKey(sval)) { ++ls.levelDeathReasons[sval]; }
                    else { ls.levelDeathReasons.Add(sval, 1); }
                    if (ts.levelDeathReasons.ContainsKey(sval)) { ++ts.levelDeathReasons[sval]; }
                    else { ts.levelDeathReasons.Add(sval, 1); }
                }
                Debug.Log("" + ts.deathsTotal + " total deaths in this level");
                break;
            case PersistentValueSpecialMode.Hitless:
                ls.hitlessWin |= (1 << replayMode);
                Debug.Log("This level is beaten without taking damage");
                break;
            case PersistentValueSpecialMode.ScoreTotal:
                if (typeof(T) == typeof(int))
                {
                    int valConv = Convert.ToInt32(val);
                    if (valConv == 0)
                    {
                        ls.scorelessWin |= (1 << replayMode);
                        Debug.Log("Level passed having collected no score");
                    }
                    else if (ls.scoreTotal[replayMode] < valConv) {
                        ls.scoreTotal[replayMode] = valConv;
                        Debug.Log("" + ls.scoreTotal[replayMode] + " is the score record for this level");
                    }
                }
                break;
            case PersistentValueSpecialMode.FlagsTotal:
                if (replayMode > 0 || !replayLevel)
                {
                    ++ls.flagsTotal;
                    Debug.Log("" + ls.flagsTotal + " flags collected in this level");
                }
                else
                {
                    Debug.Log("Flags don't count in Upper class mode");
                }
                break;
            case PersistentValueSpecialMode.WinTime:
                if (typeof(T) == typeof(double))
                {
                    double valConv = Convert.ToDouble(val);
                    if (valConv < ls.winTime[replayMode])
                    {
                        ls.winTime[replayMode] = valConv;
                        Debug.Log("" + ls.winTime[replayMode] + " sec. is a new record time");
                    }
                    else
                    {
                        Debug.Log("" + ls.winTime[replayMode] + " sec. is not a new record time");
                    }
                }
                break;
            default:
                break;
        }
    }

    public static bool GetPersistentData(GameObject g, int defaultValue, out int i)
    {
        int sc = SceneManager.GetActiveScene().buildIndex;
        int id = GetLocalIDInFile(g);

        if (loadedSaveData.leveldatas.ContainsKey(sc) && loadedSaveData.leveldatas[sc].persistIDs.ContainsKey(id))
        {
            i = loadedSaveData.leveldatas[sc].persistIDs[id];
            return true;
        }
        i = defaultValue;
        return false;
    }

    public static LevelInfoS GetPersistentGeneralLevelInfo()
    {
        int sc = SceneManager.GetActiveScene().buildIndex;
        LevelInfoS main = null;
        EnsurePersistentDataObject(ref main, loadedSaveData, sc);
        return main;
    }

    public static LevelInfoS GetPersistentGeneralLevelInfo(int sc)
    {
        LevelInfoS main = null;
        EnsurePersistentDataObject(ref main, loadedSaveData, sc);
        return main;
    }

    public static bool timerStoppedThisLevel = false;

    public static void StopGameTimerHere()
    {
        if (!timerStoppedThisLevel)
        {
            timerStoppedThisLevel = true;
            double t = DoubleTime.UnscaledTimeSinceLoad;
            toSaveData.gameTimePlayed += t;
            loadedSaveData.gameTimePlayed += t;
        }
    }

    public static void SetLevelInInfo(string s)
    {
        if (!replayLevel)  { toSaveData.levelInInfo = loadedSaveData.levelInInfo = s; /*Debug.Log("level in info set to " + s);*/ }
        else { /*Debug.Log("level in info NOT set to " + s);*/ }
    }

    public static string GetLevelInInfo()
    {
        if (replayLevel) { /*Debug.Log("level in info not retrieved");*/ return ""; }
        else { /*Debug.Log("level in info retrieved as " + loadedSaveData.levelInInfo);*/ return loadedSaveData.levelInInfo; }
    }

    public static void SetActionData(string s)
    {
        if (replayLevel) { return; }
        if (toSaveData.actionsData.Contains(s)) { return; }
        toSaveData.actionsData.Add(s);
    }

    // return true if the action data is present
    public static bool GetActionData(string s)
    {
        if (replayLevel) { return false; }
        return toSaveData.actionsData.Contains(s);
    }

    public static void ResetStoryProgress()
    {
        // assignment by reference is ok, we're not using defaultSave elsewhere
        SaveData defaultSave = new SaveData();
        toSaveData.SharedPlayersHP = defaultSave.SharedPlayersHP;
        toSaveData.SharedPlayerItems = defaultSave.SharedPlayerItems;
        toSaveData.CurrItemIndex = defaultSave.CurrItemIndex;

        toSaveData.levelName = defaultSave.levelName;
        toSaveData.levelBuildId = defaultSave.levelBuildId;
        toSaveData.levelInInfo = defaultSave.levelInInfo;
        toSaveData.levelBeforeThisName = defaultSave.levelBeforeThisName;
        toSaveData.deathsThisLevel = defaultSave.deathsThisLevel;
        toSaveData.deathsLastLevel = defaultSave.deathsLastLevel;
        toSaveData.deathTimeLastLevel = defaultSave.deathTimeLastLevel;
        toSaveData.previousDeathReason = defaultSave.previousDeathReason;
        toSaveData.switchMask = defaultSave.switchMask;

        toSaveData.actionsData = defaultSave.actionsData;
        toSaveData.corruptionDialog = defaultSave.corruptionDialog;
        toSaveData.score = defaultSave.score;
        toSaveData.multiplier = defaultSave.multiplier;
        toSaveData.multiplierMultiplier = defaultSave.multiplierMultiplier;
        toSaveData.highestLevel = defaultSave.highestLevel;
        toSaveData.deathsTotalStory = defaultSave.deathsTotalStory;
        toSaveData.entropy = defaultSave.entropy;

        foreach (int i in toSaveData.leveldatas.Keys)
        {
            LevelInfoS l = toSaveData.leveldatas[i];
            foreach (int j in l.persistIDs.Keys.ToList())
            {
                if (l.persistIDs[j] == PrimExaminableItem.ITEMGIVER_LEVELDATA_ID)
                {
                    l.persistIDs.Remove(j);
                }
            }
        }

        toSaveData.dateTimeLastSaved = DateTime.Now;
        loadedSaveData = new SaveData(toSaveData);
        SaveGame(currentSaveNumber);
    }

    public static void StartGame(int gameNumber)
    {
        SaveData currData = LoadGame(gameNumber);
        if (currData == null)
        {
            InitializeGame(gameNumber);
            LoadGame(gameNumber);
        }
        activeGameNumber = gameNumber;
    }

    public static SaveData InitializeGame(int gameNumber)
    {
        toSaveData = new SaveData();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + ("/kismet" + gameNumber + ".bin"));
        bf.Serialize(file, toSaveData);
        file.Close();
        return toSaveData;
    }

    // for when level is beaten or soft mid-level checkpoint is reached in story mode
    public static void SoftSave()
    {
        loadedSaveData.SharedPlayersHP = KHealth.health;
        toSaveData = new SaveData(loadedSaveData);
    }

    public static string SaveGame(int gameNumber)
    {
        try
        {
            //toSaveData = loadedSaveData;
            toSaveData.dateTimeLastSaved = DateTime.Now;
            toSaveData.leveldatas = new Dictionary<int, LevelInfoS>() { };
            for (int i = 0; i < loadedSaveData.leveldatas.Count; i++)
            {
                var c = loadedSaveData.leveldatas.ElementAt(i);
                toSaveData.leveldatas.Add(c.Key, new LevelInfoS(c.Value));
            }
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + ("/kismet" + gameNumber + ".bin"));
            bf.Serialize(file, toSaveData);
            file.Close();
            return "Game " + gameNumber + " saved";
        }
        catch
        {
            return "Couldn't save!";
        }
    }

    public static void DeleteGame(int gameNumber)
    {
        try
        {
            //toSaveData = loadedSaveData;
            File.Delete(Application.persistentDataPath + ("/kismet" + gameNumber + ".bin"));
            int gn = gameNumber + 1;
            while (File.Exists(Application.persistentDataPath + ("/kismet" + gn + ".bin")))
            {
                File.Move(Application.persistentDataPath + ("/kismet" + gn + ".bin"), Application.persistentDataPath + ("/kismet" + (gn - 1) + ".bin"));
                ++gn;
            }
            return;
        }
        catch
        {
            return;
        }
    }

    public static SaveData LoadGame(int gameNumber, bool dontLoadIntoActiveSlot = false, bool induceCorruption = false, string[] corruptionDialog = null)
    {
        SaveData dontLoadRet = null;
        try
        {
            if (File.Exists(Application.persistentDataPath + ("/kismet" + gameNumber + ".bin")))
            {
                try
                {
                    if (induceCorruption)
                    {
                        emergencySaveData = new SaveData(toSaveData);
                        SceneManager.LoadScene("Corrupted");
                        throw new Exception("you probably deserved this");
                    }

                    FileStream file = File.Open(Application.persistentDataPath + ("/kismet" + gameNumber + ".bin"), FileMode.Open);

                    BinaryFormatter bf = new BinaryFormatter();
                    if (!dontLoadIntoActiveSlot)
                    {
                        loadedSaveData = (SaveData)bf.Deserialize(file);
                        toSaveData = new SaveData(loadedSaveData);
                    }
                    else
                    {
                        dontLoadRet = (SaveData)bf.Deserialize(file);
                    }
                    file.Close();
                    currentSaveNumber = gameNumber;
                    if (loadedSaveData.SharedPlayerItems.Count > 25)
                    {
                        throw new OverflowException("hacked items: too many");
                    }
                }
                catch
                {
                    //file.Close();
                    toSaveData = new SaveData();
                    toSaveData.actionsData = new HashSet<string>();
                    toSaveData.entropy = 0;
                    toSaveData.gameTimePlayed = 359999;
                    toSaveData.levelName = "Corrupted";
                    toSaveData.SharedPlayersHP = 0;
                    toSaveData.SharedPlayerItems.Clear();
                    toSaveData.CurrItemIndex = -1;
                    toSaveData.nothing1 = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0See what you did?";
                    toSaveData.multiplier = 0;
                    toSaveData.multiplier = 0;
                    if (corruptionDialog != null)
                    {
                        toSaveData.corruptionDialog = (string[])corruptionDialog.Clone();
                    }
                    else
                    {
                        toSaveData.corruptionDialog = new string[] {
                            "Something's up with your save.",
                            "Did you naïvely edit something in there? Or was your drive corrupted?",
                            "Either way, it's gone forever now. Sorry...",
                            "......................",
                            "Here's a tip for your next \"hack\": don't forget the checksum.",
                        };
                    }
                    toSaveData.name = "Corrupted";
                    toSaveData.icon = -1;

                    loadedSaveData = new SaveData(toSaveData);
                    File.Delete(Application.persistentDataPath + ("/kismet" + gameNumber + ".bin"));
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = File.Create(Application.persistentDataPath + ("/kismet" + gameNumber + ".bin"));
                    bf.Serialize(file, toSaveData);
                    file.Close();
                }
                return (!dontLoadIntoActiveSlot)?loadedSaveData:dontLoadRet;
            }
            else
            {
                return null;
            }
        }
        catch
        {
            return loadedSaveData;
        }
    }

    public static void ReplayModeStatChanges()
    {
        if (replayLevel)
        {
            loadedSaveData.SharedPlayerItems = new List<int>();
            switch (replayMode)
            {
                case 0:
                    loadedSaveData.score = MenuLevelSelectConfirm.currScoreValues[0];
                    //inventory?
                    break;
                case 1:
                    loadedSaveData.score = MenuLevelSelectConfirm.currScoreValues[1];
                    //inventory?
                    break;
                case 2:
                    loadedSaveData.score = 0;
                    //loadedSaveData.SharedPlayerItems = new List<int>();
                    break;
                default:
                    break;
            }
        }
    }

    public static string[] replayModeNames = new string[]
    {
        "Upper class", "Middle class", "Lower class"
    };

    public static string[] nprefixes = new string[]
    {
        "", "thousand", "million", "billion","trillion", "quadrillion","quintillion", "sextillion","septillion", "octillion","nonillion", "decillion","undecillion", "duodecillion",
        "tredecillion", "quattuordecillion", "quindecillion", "sexdecillion", "septendecillion", "octodecillion", "novemdecillion", "vigintillion", "times a really huge exponent"
    };

    // obsolete!
    /*public static int[] RankRequirementInterval = {0,
       3092,9296,15568,21948,28486,35227,42220,49519,
57179,65258,73819,82934,92672,103118,114355,126483,
139603,153829,169285,186104,204434,224432,246266,270120,
296189,324675,355791,389757,426794,467118,510935,558431,
609756,665016,724248,787404,854322,924709,998100,1073849,
1151091,1228736,1305458,1379707,1449735,1513645,1569467,1615246,
1649163,1669645,1675491,1665976,1640921,1600735,1546397,1479414,
1401709,1315508,1223194,1127167,1029718,932927,838591,748175,
4759843,
        int.MaxValue
    };*/

    public static byte CalculateLevel()
    {
        for (byte i = 1;i<=RankRequirementTotal.Length-1;i++)
         {
             if (loadedSaveData.score < RankRequirementTotal[i])
             {
                 return (byte)(i-1);
             }
         }
         return (byte)(RankRequirementTotal.Length-1);
    }

    public static string[] secretFlagTexts =
    {
        "It found a <rainbow>signal<rainbow>. That shouldn't be possible...",
        "A secret <rainbow>signal<rainbow> was collected. That's no good for anyone.",
        "That was a <rainbow>signal<rainbow>. Probably not the only one, but why should we care?",
        "That wasn't a secret <rainbow>signal<rainbow>. Nothing was just collected. Go on now.",
        "Don't collect any more <rainbow>signals<rainbow>. Consider this a fair warning.",
        "Stop collecting <rainbow>signals<rainbow>. Legal action may be enforced.",
        "This isn't funny. Stop. <rainbow>Signals<rainbow> aren't meant to be found. Stop.",
        "No Being can collect every <rainbow>signal<rainbow>! Not even kidding! Just give it up.",
        "Score is better than <rainbow>signals<rainbow>. Go collect Score instead.",
        "<rainbow>Signal<rainbow> flags are confidential. Loose lips sink ships.",
        "Never consult Cyberspace for the location of <rainbow>signals<rainbow>! Cyberspace often misleads about leaked documents.",
        "All <rainbow>signals<rainbow> in the game found! Now you can stop looking forever.",
        "Are there any more <rainbow>signals<rainbow> left to find? Probably not. Stop looking now.",
    };

    public static string[] specialRankTexts = {"",
        "Rank Up to <!><rank>.<!> This is your first Success.",
        "Rank Up to <!><rank>.",
        "Rank Up to <!><rank>.",
        "Rank Up to <!><rank>.<!> The first Limit; unreachable from below.",
        "Rank Up to <!><rank>.",
        "Rank Up to <!><rank>.",
        "Rank Up to <!><rank>.",
        "Rank Up to <!><rank>.<!> Twice unreachable from below. You earned bonus Stock!",
        "Rank Up to <!><rank>.<!> Thrice unreachable from below.",
        "Rank Up to <!><rank>.<!> Yet a fourth addition of omega.",
        "Rank Up to <!><rank>.<!> Also known as Limit-Limit.",
        "Rank Up to <!><rank>.<!> Also known as Cubic Limit.",
        "Rank Up to <!><rank>.<!> Also known as Quartic Limit.",
        "Rank Up to <!><rank>.",
        "",
        "",
        "You are now Epsilon Ready. <wave>This is not a Rank.",

        "Rank Up to <!><rank>.<!> Actually an omega tall power-stack of omegas.",
        "Rank Up to <!><rank>.<!> An omega tall power-stack of epsilon naught.",
        "Rank Up to <!><rank>.<!> An omega tall power-stack of epsilon one.",
        "Rank Up to <!><rank>.<!> That's right, epsilon with subscript omega.",
        "Rank Up to <!><rank>.",
        "Rank Up to <!><rank>.",
        "",
        "",
        "",
        "You are now Zeta Ready. <wave>This is not a Rank.",

        "Rank Up to <!><rank>.<!> Actually an omega deep nesting of epsilons.", //zeta group
        "",
        "",

        "", //nested zeta group
        "",
        "",
        "",

        "", //eta group
        "",
        "",

        "", //binary phi group
        "",
        "",
        "",

        "", //gamma group
        "",
        "",

        "", //finite veblen group
        "",
        "",

        "", //infinite veblen group
        "",
        "",

        "", //bachmann-howard group
        "",
        
        "", //psi group
        "",
        "",
        
        "", //ultimate omega
    };

    public static string[] specialRankTexts2 = {"",
        "Keep Succeeding to start your <rainbow>Ultimate and Transfinite<rainbow> Journey into KISMET!",
        "As your Rank Succeeds, so does your damage capacity. You are slowly building a Tolerance.",
        "Next Rank is very special: get ready for the Power of a Limit Ordinal.",
        "You have real Potential, now privileged with the Inventory. Press [SELECT] to use it.",
        "Lemma 3 of KISMET is to succeed even past these Limits and discover even higher Powers: capacity is an illusion.",
        "Have you noticed an increase in Luck recently?",
        "Octahedrons are worth 16 thousand score.",
        "Stock multiplies your future Score. Use the \"Invest\" tab in the pause menu to buy it, but don't rank down.",
        "Now your Rank Succeeds by omega at a time.",
        "Don't let false friends keep you from Ranking Up to the Limit-Limit.",
        "Play the optional Extra Stage R-0 (in your save menu) to prepare Math skills for Epsilon levels.",
        "You added the Limit-Limit to itself omega times to Succeed this far.",
        "If you studied the Math correctly, you may anticipate what follows.",
        "",
        "",
        "",
        "Rid yourself of capacities, then pass Extra Stage R-1 to be Receptive to further Ordinals.",

        "You now have an Envelope in the Mirror World. Watch your Damage slowly decrease.",
        "Notice that adding omega to this Ordinal does virtually nothing; obviously, the Being must Rank Up in new ways.",
        "It's very dangerous to let that power-stack fall now. Would you fall from a large skyscraper?",
        "Your Inventory now has maximum slots.",
        "All Epsilon Ordinals are fixed points of the exponential map.",
        "Have you noticed an increase of physical strength?",
        "",
        "",
        "",
        "Rid yourself of capacities, then pass Extra Stage R-2 to be Receptive to further Ordinals.",

        "Your Electricity is now unlimited.", //zeta group
        "",
        "",

        "", //nested zeta group
        "",
        "",
        "",

        "", //eta group
        "",
        "",

        "", //binary phi group
        "",
        "",
        "",

        "", //gamma group
        "",
        "",

        "", //finite veblen group
        "",
        "",

        "", //infinite veblen group
        "",
        "",

        "", //bachmann-howard group
        "",

        "", //psi group
        "",
        "",

        "", //ultimate omega
    };

    public static readonly int[] RankRequirementTotal = {0,
    1,10000,20000, //fundamental group
    30000,50000,70000,90000, //omega group
    120000,160000,200000, //multiplied omega group
    240000,320000,400000, //polynomial omega group
    480000,640000,800000, //exponentiated omega group
    1000000,1250000,1500000, //epsilon group
    1800000,2400000,3000000, //omega epsilon group
    3600000,4800000,6000000, //nested epsilon group
    7200000,10000000,13000000, //zeta group
    16000000,21000000,26000000, //nested zeta group
    32000000,40000000,48000000,52000000, //eta group
    56000000,70000000,85000000, //binary phi group
    100000000,150000000, //gamma group
    200000000,250000000, //finite veblen group
    300000000,400000000, //infinite veblen group
    500000000,600000000, //bachmann-howard group
    700000000,800000000,900000000, //psi group
    999999999 //ultimate omega
    };

    public static readonly int[] scoreForInventorySlot = new int[25]
    {
        0, 0, 0, 30000, 40000,
        53000, 72000, 96000, 129000, 173000,
        232000, 311000, 417000, 559000, 750000,
        1000000, 1340000, 1800000, 2410000, 3240000,
        4340000, 5820000, 7750000, 10000000, 50000000,
    };

    public static int getInventorySlotCount()
    {
        int s = 0;
        while (s < 25 && scoreForInventorySlot[s] <= loadedSaveData.score)
        {
            s++;
        }
        return s;
    }

    public static string longLevelNames(int x)
    {
        // god damn it i added an extra omega character to the font
        if (x > 4) { ++x; }
        return Convert.ToChar(0xE000 + x).ToString();
    }

    public static string[] shortLevelNames =
    {
        "Rank 0",
        "Rank 1","Rank 2","Rank 3",
        "w","w+1","w+2","w+3",
        "w2","w3","w4",
        "w^2","w^3","w^4",
        "w^w","w^w^w","w^w^w^w",
        "e_0","e_1","e_2",
        "e_w","e_(w^w)","e_(w^w^w)",
        "e_e_0","e_e_e_0","e_e_e_e_0",
        "z_0","z_1","z_2",
        "z_z_0","z_z_z_0","z_z_z_z_0",
        "n_0","n_n_0","n_n_n_0", "n_n_n_n_0",
        "p(4,0)","p(w,0)","p(p(w,0),0)",
        "G_0","G_1",
        "t(O^3)","t(O^4)",
        "t(O^w)","t(O^O)",
        "t^(O^O^O)","t(e_(O+1))",
        "ps(O_w)","ps(e_(O_w+1))","ps(ps_M(0))",
        "OMEGA"
    };

    public static string SecondsToPlanckTimes(float seconds)
    {
        int fti = Mathf.FloorToInt(Mathf.Log10(seconds)/3f)*3;
        float n = (seconds/ Mathf.Pow(10f,fti)) * 1.855f;
        n *= 10f;
        int pow = 42+fti;
        if (n >= 1000f)
        {
            n /= 1000f;
            pow += 3;
        }

        if (pow <= 63)
        {
            return n + " " + nprefixes[Mathf.FloorToInt(pow / 3)];
        }
        else
        {
            return n + "x10^" + pow;
        }

        

    }

    public static Dictionary<Encontrolmentation.ActionButton, ulong> ActButtonToUL = new Dictionary<Encontrolmentation.ActionButton, ulong>()
    {
        {Encontrolmentation.ActionButton.AButton,16UL},
        {Encontrolmentation.ActionButton.BButton,32UL},
        {Encontrolmentation.ActionButton.XButton,64UL},
        {Encontrolmentation.ActionButton.YButton,128UL},
        {Encontrolmentation.ActionButton.LButton,256UL},
        {Encontrolmentation.ActionButton.RButton,512UL},
    };

    public static string GetAmericanDateString(DateTime dt)
    {
        return dt.ToString("MMMM d, yyy");
    }

    public static string GetInternationalDateString(DateTime dt)
    {
        return dt.ToString("d. MMMM yyy"); 
    }

    public static float Average(float[] nums)
    {
        float res = 0;
        foreach (float num in nums)
        {
            res += num;
        }
        res /= nums.Length;
        return res;
    }

    public static Color[] colorCycle = 
    {
        new Color32(0xe0,0x00,0x00,0xff),
        new Color32(0x00,0x70,0xff,0xff),
        new Color32(0xd0,0xd0,0x00,0xff),
        new Color32(0x00,0xd0,0x00,0xff),

        new Color32(0xe0,0x80,0x10,0xff),
        new Color32(0x80,0x00,0xff,0xff),
        new Color32(0xf0,0x00,0xd0,0xff),
        new Color32(0x00,0xe0,0xf0,0xff),

        new Color32(0xe0,0xe0,0xe0,0xff),
        new Color32(0x90,0x20,0x20,0xff),
        new Color32(0x30,0x30,0x90,0xff),
        new Color32(0x98,0x80,0x00,0xff),

        new Color32(0x20,0x70,0x50,0xff),
        new Color32(0x98,0x60,0x40,0xff),
        new Color32(0x80,0x80,0x98,0xff),
        new Color32(0xa0,0x30,0xa0,0xff),

        new Color32(0x00,0xc0,0xc0,0xff),
        new Color32(0x68,0x58,0x68,0xff),
        new Color32(0xe8,0x50,0x00,0xff),
        new Color32(0x00,0x00,0xe0,0xff),

        new Color32(0xe8,0xb0,0x00,0xff),
        new Color32(0xa0,0xd0,0x00,0xff),
        new Color32(0xff,0x98,0x70,0xff),
        new Color32(0xc8,0x78,0x90,0xff),

        new Color32(0xf0,0x78,0xff,0xff),
        new Color32(0xb0,0xd0,0xd0,0xff),
        new Color32(0xa8,0x98,0x90,0xff),
        new Color32(0xb0,0x58,0x80,0xff),

        new Color32(0x40,0x80,0xa0,0xff),
        new Color32(0x50,0xa0,0x80,0xff),
        new Color32(0xd0,0xb0,0x90,0xff),
        new Color32(0x70,0x90,0x28,0xff),

    };

    public static int GetLocalIDInFile(GameObject g)
    {
        if (g.GetComponent<primRevealLocalID>())
        {
            return g.GetComponent<primRevealLocalID>().ID;
        }
        else
        {
            Debug.LogError("local id isn't revealed. :<");
            return int.MinValue;
        }
    }

    public static string GetFormattedTimePlayed(long t)
    {
        t += 60;
        return (t / 3600).ToString() + ":" + ((t / 60) % 60).ToString("D2");
    }

    public static string GetFormattedPreciseTime(double t)
    {
        if (double.IsInfinity(t) || double.IsNaN(t))
        {
            return t.ToString();
        }

        if (t < 59.995)
        {
            return t.ToString("00.00");
        }

        if (t < 60) { t = 60; }

        if (t < 3599.995)
        {
            return (Math.Floor(t / 60.0)).ToString("0") + ":" + (t % 60.0).ToString("00.00");
        }

        if (t < 3600) { t = 3600; }

        return Math.Floor((t / 3600.0)).ToString("0") + ":" + (Math.Floor(t / 60.0) % 60.0).ToString("00") + ":" + (t % 60.0).ToString("00.00");
    }

    public static string imperviousBlockName = "metalBlocks13";
    public static string screenshotPath = Application.persistentDataPath + "/levelPreviews";


    public static float HealthDisplayScale(float x)
    {
        return Mathf.Pow(x, 0.65f);
    }

    // 0: no hits. 1: crumbed!
    public static float GetPlayerDamageRatio()
    {
        return HealthDisplayScale(KHealth.health / KHealth.maxHealth);
    }

    // [years, days, negative(0 false 1 true)]
    // think of it as "b - a"
    // NOTE: still may not work if b is leap year.
    public static int[] DateDiffRaw(DateTime a, DateTime b)
    {
        int[] res = { 0, 0, 0 };
        //ensure a is earlier
        if (DateTime.Compare(a,b) > 0) { DateTime c = a; a = b; b = c; res[2] = 1; }

        int oldYear = a.Year + 1;
        DateTime oldDay = a;

        int yearDif = (a.Year == b.Year)?0:(b.Year - a.Year - 1);
        a = a.AddYears(yearDif);
        if (DateTime.Compare(a.AddYears(1), b) <= 0) { ++yearDif; a = a.AddYears(1); }
        res[0] = yearDif;

        res[1] = (b - a).Days;

        if (oldDay.DayOfYear == 31 + 29) { --res[1]; }

        //this is about adding the extra day
        if (((oldYear % 4 == 0 && oldYear % 100 != 0) || oldYear % 400 == 0) 
            && a.DayOfYear > b.DayOfYear)
        {
            ++res[1];
        }
        else
        {
            --oldYear;
            if (((oldYear % 4 == 0 && oldYear % 100 != 0) || oldYear % 400 == 0)
            && a.DayOfYear < 31+29)
            {
                ++res[1];
            }
        }

        return res;
    }

    public static float StopTime()
    {
        float origTS = Time.timeScale;
        Time.timeScale = 0;
        canPauseGame = canUseInventory = false;
        return origTS;
    }

    public static void ResumeTime(float origTS)
    {
        canPauseGame = canUseInventory = true;
        Time.timeScale = origTS;
    }

    public static void DisablePlayerActions(GameObject plr)
    {
        BasicMove bm = plr.GetComponent<BasicMove>();
        bm.disabledAllControlMvt = true;
        bm.fakePhysicsVel = new Vector2(0, bm.fakePhysicsVel.y);

        SpecialGunTemplate sgt = plr.GetComponent<SpecialGunTemplate>();
        if (!sgt.enabled)
        {
            primExtraTags et = plr.GetComponent<primExtraTags>();
            if (!et) { et = plr.AddComponent<primExtraTags>(); et.tags = new List<string>(); }
            et.tags.Add("gun already disabled");
        }
        else
        {
            sgt.enabled = false;
        }

        PrimCharacterSwapper pcs = plr.GetComponent<PrimCharacterSwapper>();
        if (pcs) { pcs.enabled = false; }
    }

    public static void ReEnablePlayerActions(GameObject plr)
    {
        BasicMove bm = plr.GetComponent<BasicMove>();
        bm.disabledAllControlMvt = false;
        bm.fakePhysicsVel = new Vector2(0, bm.fakePhysicsVel.y);

        primExtraTags et = plr.GetComponent<primExtraTags>();
        if (et && et.tags.Contains("gun already disabled"))
        {
            et.tags.Remove("gun already disabled");
        }
        else
        {
            plr.GetComponent<SpecialGunTemplate>().enabled = true;
        }

        PrimCharacterSwapper pcs = plr.GetComponent<PrimCharacterSwapper>();
        if (pcs) { pcs.enabled = true; }
    }
}
