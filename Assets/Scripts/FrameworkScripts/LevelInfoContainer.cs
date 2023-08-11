using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityStandardAssets.ImageEffects;
using System.IO;
using System.Threading;
using UnityEngine.Experimental.Rendering;
using System;
using UnityEngine.InputSystem;

public class LevelInfoContainer : MonoBehaviour {

    public enum RatingLetters
    {
        None,Plrs1,Plrs2,Plrs3,Plrs4,Boss,Cheatful,Disablement,Joke,Money,NewFeature,Puzzle,Scenic,Timebased,Violent,Xtra
    }

    public enum Theme
    {
        Travail, Flashback, Block_13, Columnspace, SMW, Cybercyl, Hexateron, Triakulus, E8_Convention, Safezone, Coelom, Xerxes, Nowhere, End, Extra, Test, Cutscene, Invalid
    }

    public string levelName = "Please Insert Level";

    public string levelDescription = "Honestly who cares";
    public Rect mapPosition = new Rect(0,0,9,7);
    public int backgroundPrefab;
    [Range(0,32)]
    public int rating;
    [HideInInspector]
    public RatingLetters[] descriptors = new RatingLetters[4]; // no longer used
    public Vector2 levelStartGravity = new Vector2(0, -8);
    public string playerSortingString = "KTMR";
    public bool khal = true;
    public bool tetra = false;
    public bool myst = false;
    public bool ravel = false;
    public float kStartEnergy = 100f;
    // These should not be used, oops
    [HideInInspector]
    public float tStartEnergy = 100f;
    [HideInInspector]
    public float mStartEnergy = 100f;
    [HideInInspector]
    public float rStartEnergy = 100f;
    public float specialLimitMovement = 0f;
    [HideInInspector]
    public List<float> timeRankings = new List<float>() { // no longer used
        30f, //ultra time
        60f, //super time
    };
    [HideInInspector]
    public List<int> scoreRankings = new List<int>() { // no longer used
        100000, //ultra score
        50000, //super score
    };
    public static int nonMultipliedScoreInLevel;
    [HideInInspector]
    public List<float> deathRankings = new List<float>() { // no longer used
        0f, //ultra flawless
        3f, //super flawless
    };
    [HideInInspector]
    public List<float> collectibleRankings = new List<float>() { // no longer used
        //depends on the number of these things in the level
        0f, //ultra find
        0f, //super find
    };
    public AudioMixer objectSFX;
    public List<Key> cheatkeys = new List<Key>() { };
    public GameObject cheatDialog;
    public List<string> otherStartPlaceValues = new List<string>() { };
    [Header("two for each value. the first is player position and second is camera.")]
    public List<Vector3> otherStartPlaces = new List<Vector3>() { };
    public List<float> otherStartPlaceRotations = new List<float>();
    public List<string[]> otherStartPlaceExtraPlayers = new List<string[]>();

    public static GameObject[] allGameObjects;
    public static List<GameObject> allBoxPhysicsObjects = new List<GameObject>();
    public static List<AudioSource> allAudioSources = new List<AudioSource>();

    public static List<GameObject> allPlayersInLevel = new List<GameObject>();
    public static Encontrolmentation[] allCtsInLevel = new Encontrolmentation[0];
    public static string[] allPlayersNames = new string[0];
    public static int playableCount = 0;

    public static List<PrimEnemyHealth> allEnemiesInLevel = new List<PrimEnemyHealth>();

    [HideInInspector]
    public static FollowThePlayer mainFtp = null;

    public static float timer = 60f;
    public static bool timerOn = false;

    public bool allowPlayerRotation = false;

    [HideInInspector]
    public uint switchResets = 0;
    public int flipOnRandomSwitch = 0;
    public Vector2 flipOnRandomSwitchRange;

    public GameObject timerBulletRing;
    private bool gaveTimerBulletRing;

    private GameObject go = null;
    private BGMController bgmCont = null;

    [HideInInspector]
    public static AudioClip onDeathBGM;

    public static Texture2D screenshotStaticTex = null;

    public Theme levelTheme;

    private static bool didSettings;

    private static bool updatedActivePlayerThisFrame;
    private static Encontrolmentation activePlayerCache;

    public static LevelInfoContainer main;

    public class NameComparer : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            string sorter = main.playerSortingString;
            int ix = 100000;
            if (x.GetComponent<PrimPlayableCharacter>())
            {
                ix = sorter.IndexOf(x.GetComponent<PrimPlayableCharacter>().myName[0]);
                if (ix == -1) { ix = 100000; }
            }
            int iy = 100000;
            if (y.GetComponent<PrimPlayableCharacter>())
            {
                iy = sorter.IndexOf(y.GetComponent<PrimPlayableCharacter>().myName[0]);
                if (iy == -1) { iy = 100000; }
            }

            return ix - iy;

        }
    }

    public static float GetScalingSpikeDamage()
    {
        switch (main?.levelTheme ?? Theme.Invalid)
        {
            case Theme.Block_13:
            case Theme.Travail:
                return 1f;
            case Theme.Columnspace:
                return 2f;
            case Theme.SMW:
                return 3f;
            case Theme.Cybercyl:
                return 4f;
            case Theme.Hexateron:
                return 5f;
            case Theme.Triakulus:
                return 6f;
            case Theme.E8_Convention:
                return 7f;
            case Theme.Safezone:
                return 8f;
            case Theme.Coelom:
                return 9f;
            case Theme.Xerxes:
            case Theme.End:
                return 10f;
            default:
                return 7f;
        }
    }

    public static void SetUpPlayerList()
    {
        mainFtp = Camera.main.GetComponent<FollowThePlayer>();

        allPlayersInLevel = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        allPlayersInLevel.Sort(new NameComparer());
        allCtsInLevel = new Encontrolmentation[allPlayersInLevel.Count];
        allPlayersNames = new string[allPlayersInLevel.Count];
        playableCount = 0;
        for (int i = 0; i < allPlayersInLevel.Count; ++i)
        {
            Encontrolmentation e = allPlayersInLevel[i].GetComponent<Encontrolmentation>();
            if (e)
            {
                PrimPlayableCharacter ppc = e.GetComponent<PrimPlayableCharacter>();
                if (ppc && ppc.playable)
                {
                    allCtsInLevel[i] = e;
                    allPlayersNames[i] = ppc.myName;
                    ++playableCount;

                    BasicMove bm = e.GetComponent<BasicMove>();
                    if (bm) { bm.allowRotation = main.allowPlayerRotation; }
                }

                if (e.allowUserInput) //assume only one per level. this is the initial player
                {
                    mainFtp.target = e.transform;
                    mainFtp.refPlayer = e.gameObject;
                    e.gameObject.AddComponent<PrimCharacterSwapper>();
                }
            }
        }
    }

    void Start()
    {
        // this item occurs only in levels which should have 60 fps and vsync if possible
        // cutscenes have 24 fps but no vsync because 60/24 is not a whole number
        Application.targetFrameRate = 60;
        if (Screen.currentResolution.refreshRate > 59 && Screen.currentResolution.refreshRate < 61) { QualitySettings.vSyncCount = 1; }
        else { QualitySettings.vSyncCount = 0; }
        // the real copy of this is in Encont. this thing does it for the testing a level by itself
        if (Application.isEditor && !didSettings)
        {
            didSettings = true;
            Settings.PrepareAll();
        }

        Settings.UpdateAll();
        nonNullPlayerListCache = null;
        allEnemiesInLevel.Clear();
        Utilities.ReplayModeStatChanges();
        Utilities.tetrahedronTime = -5f;
        Door1.levelComplete = false;
        Door1.completionWasTrivial = false;
        onDeathBGM = null;
        Utilities.timerStoppedThisLevel = false;
        timer = 60f;
        timerOn = false;
        go = GameObject.FindGameObjectWithTag("DialogueArea");
        bgmCont = BGMController.main;
        mainFtp = Camera.main.GetComponent<FollowThePlayer>();

        Physics2D.gravity = levelStartGravity;
        if (levelStartGravity != new Vector2(0f, -8f))
        {
            Camera.main.GetComponent<ColorCorrectionCurves>().DiscoStart();
        }
        timerOn = false;
        if (!Utilities.replayLevel)
        {
            Utilities.UpdateCurrentLevel(levelName);
            Utilities.loadedSaveData = new Utilities.SaveData(Utilities.toSaveData); // new: hopefully doesn't cause bad problems
        }
        //int id = SceneManager.GetActiveScene().buildIndex;
        //if (!Utilities.loadedSaveData.leveldatas.ContainsKey(id))
        {
            //Utilities.loadedSaveData.leveldatas.Add(id, new Utilities.LevelInfoS());
            Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.LevelName, levelName);
            Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.LevelDescription, levelDescription);
            Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.Theme, levelTheme.ToString());
            //if (!Utilities.replayLevel)
            {
                StartCoroutine(TakeLevelSnapshot());
            }
            //print(Application.persistentDataPath);
        }
        nonMultipliedScoreInLevel = 0;
        allBoxPhysicsObjects = new List<GameObject>() { };
        allAudioSources = new List<AudioSource>() { };
        for (int i = 0; i < otherStartPlaceValues.Count; i++)
        {
            if (Utilities.GetLevelInInfo() == otherStartPlaceValues[i])
            {
                int j = 2 * i;
                GameObject[] plrs = GameObject.FindGameObjectsWithTag("Player");
                // multiplayer levels firstly checkpoint the initally active one
                GameObject plr = plrs.Where((g) => g.GetComponent<Encontrolmentation>().allowUserInput).ToArray()[0]; 
                plr.transform.position = otherStartPlaces[j];
                if (otherStartPlaceRotations.Count > i)
                {
                    plr.transform.eulerAngles = Vector3.forward * otherStartPlaceRotations[i];
                }
                FollowThePlayer.SetCameraPosition(otherStartPlaces[j+1]);
                Vector3 sp = otherStartPlaces[j] + 28 * plr.transform.up;
                if (otherStartPlaceExtraPlayers.Count > i && otherStartPlaceExtraPlayers[i].Length > 0)
                {
                    foreach (GameObject po in plrs)
                    {
                        HashSet<string> nameHash = new HashSet<string>(otherStartPlaceExtraPlayers[i]);
                        if (nameHash.Contains(po.GetComponent<PrimPlayableCharacter>().myName))
                        {
                            float rot = otherStartPlaceRotations.Count > 1 ? otherStartPlaceRotations[i] : 0;
                            po.transform.eulerAngles = Vector3.forward * rot;
                            po.transform.position = sp;
                            sp += 28 * plr.transform.up;
                        }
                    }
                }
            }
        }
        UpdateBoxPhysicsObjects();
        UpdateAudioSources();

        foreach (MeshFilter mf in FindObjectsOfType<MeshFilter>()) //resize fake perspective blocks bounds
        {
            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr && mr.material.HasProperty("_FakeZ") && mr.material.GetVector("_FakeZ") == Vector4.zero) { continue; }
            Vector3 mfs = mf.mesh.bounds.size;
            mf.mesh.bounds = new Bounds(mf.mesh.bounds.center, new Vector3(mfs.x*32f,mfs.y*32f,mfs.z*32f));
        }

        switchResets = 65535;
        if (switchResets != 0)
        {
            //print(Utilities.loadedSaveData.switchMask);
            Utilities.loadedSaveData.switchMask &= (~switchResets);
            //print(Utilities.loadedSaveData.switchMask);
        }

        if (flipOnRandomSwitch != 0)
        {
            Utilities.loadedSaveData.switchMask |= (uint)(1 << (Fakerand.Int((int)flipOnRandomSwitchRange.x, 1 + ((int)flipOnRandomSwitchRange.y))));
        }

        SetUpPlayerList();
        
        gaveTimerBulletRing = false;

        if (specialLimitMovement > 0f)
        {
            KHealth kh = FindObjectOfType<KHealth>();
            kh.limitedMovement = specialLimitMovement / 0.0508f;
        }

        //gameObject.AddComponent<LevelInfoSharedTrigger>();
        BulletRankShot.rank = 1f;
        CheatCodes.Init();
    }

    void OnDestroy() //goodbye scene
    {
        TangramLaser.allLasers.Clear();
        TangramLaser.visibleNodes.Clear();
        TangramLaser.pairs.Clear();
        TangramLaser.pairLCs.Clear();
        TangramLaser.pairFramesToActive.Clear();

        BulletRegister.Clear();
        main = null;
    }

    void Awake()
    {
        main = this;
        // Putting this in Start will cause a "race condition" with static bullet spawners.
        BulletRegister.Clear();
    }

    public static IEnumerator ItemMagnet(Transform target, Transform item, float speed = 240f)
    {
        while (item && target && (item.position - target.position).magnitude > 1f)
        {
            item.position = Vector3.MoveTowards(item.position, target.position, speed * 0.16666666f * Time.timeScale);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public IEnumerator TakeLevelSnapshot()
    {
        yield return new WaitForSeconds(1.5f);
        string screenshotFile = Utilities.screenshotPath;
        if (!Directory.Exists(screenshotFile))
        {
            Directory.CreateDirectory(screenshotFile);
        }

        string screenshotImage = screenshotFile + "\\" + SceneManager.GetActiveScene().buildIndex + ".png";

        if (!Directory.EnumerateFiles(screenshotFile, "*.png", SearchOption.AllDirectories).Contains(screenshotImage))
        {
            yield return new WaitForEndOfFrame();
            //RenderTexture oldCameraTex = Camera.main.targetTexture;
            RenderTexture cameraTex = Camera.main.targetTexture; //new RenderTexture(oldCameraTex);
            //RenderTexture.active = cameraTex;
            //Camera.main.targetTexture = cameraTex;
            //Camera.main.Render();
            //cameraTex.Release();
            //Texture2D screenshotTex = new Texture2D(320, 216);//, TextureFormat.RGB565 , false, false);
            //screenshotTex.Apply();
            //Graphics.CopyTexture(cameraTex, screenshotTex);
            RenderTexture.active = cameraTex;
            cameraTex.Release();
            float aspect = Screen.width / Screen.height;
            int newWidth = (int)(Screen.height * 1.333333333f);
            int newXStart = (int)((Screen.width - newWidth) * 0.5f);
            screenshotStaticTex = new Texture2D(newWidth, Screen.height*3/4);
            screenshotStaticTex.ReadPixels(new Rect(newXStart, Screen.height*1/8, newWidth, Screen.height*3/4), 0, 0);
            //Camera.main.targetTexture = oldCameraTex;

            yield return new WaitForEndOfFrame();

            //screenshotTex.Resize(320, 240);
            screenshotStaticTex.Apply();

            yield return new WaitForEndOfFrame();

            Texture2D smaller = new Texture2D(320, 180);
            for (int u = 0; u < 320; ++u)
            {
                for (int v = 0; v < 180; ++v)
                {
                    float uRat = u / 320f;
                    float vRat = v / 180f;
                    Color col = screenshotStaticTex.GetPixel(Mathf.RoundToInt(newWidth * uRat), Mathf.RoundToInt(Screen.height * vRat * 0.75f));
                    smaller.SetPixel(u, v, col);
                }
            }

            yield return new WaitForEndOfFrame();

            //byte[] screenshotRaw = screenshotTex.GetRawTextureData();
            byte[] screenshotRaw = smaller.EncodeToPNG();

            yield return new WaitForEndOfFrame();

            new Thread(() =>
            {
                Thread.Sleep(50);
                File.WriteAllBytes(screenshotImage, screenshotRaw);
            }).Start();

            Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.Screenshot, screenshotImage);
        }

        yield return null;
    }

    // outdated?
    public static void UpdateBoxPhysicsObjects() //CALL THIS WHENEVER YOU MAKE MORE OR LESS BOXES!!!! event won't work for built in
    {
        allBoxPhysicsObjects.Clear();
        foreach (BoxCollider2D s in FindObjectsOfType<BoxCollider2D>())
        {
            Rigidbody2D r = s.GetComponent<Rigidbody2D>();
            if (r != null && !r.isKinematic && !allBoxPhysicsObjects.Contains(s.gameObject))
            {
                allBoxPhysicsObjects.Add(s.gameObject);
            }
        }
    }

    public static Encontrolmentation GetActiveControl()
    {
        if (main == null) { return null; }

        if (updatedActivePlayerThisFrame)
        {
            return activePlayerCache;
        }

        updatedActivePlayerThisFrame = true;

        for (int i = 0; i < allCtsInLevel.Length; ++i)
        {
            Encontrolmentation e = allCtsInLevel[i];
            if (e && e.allowUserInput)
            {
                activePlayerCache = e;
                return e;
            }
        }

        activePlayerCache = null;
        return null;
    }

    public static Encontrolmentation[] nonNullPlayerListCache = null;

    public static Encontrolmentation[] GetNonNullPlayerList()
    {
        if (allCtsInLevel.Length == 0) { SetUpPlayerList(); }

        if (nonNullPlayerListCache == null)
        {
            List<Encontrolmentation> el = new List<Encontrolmentation>();
            for (int i = 0; i < allCtsInLevel.Length; ++i)
            {
                Encontrolmentation e = allCtsInLevel[i];
                if (e) { el.Add(e); }
            }
            nonNullPlayerListCache = el.ToArray();
        }

        return nonNullPlayerListCache;
    }

    public static void UpdateAudioSources() //CALL THIS WHENEVER YOU MAKE MORE OR LESS AUDIOS!!!! event won't work for built in
    {
        allAudioSources.Clear();
        foreach (AudioSource a in FindObjectsOfType<AudioSource>())
        {
            allAudioSources.Add(a);
        }
    }

    private void LateUpdate()
    {
        //BoostArrow static stuff... why is that here? becuase this object is a singleton probably
        BoostArrow.swimMovedThisFrame.Clear();
        BoostArrow.boostMovedThisFrame = false;
        updatedActivePlayerThisFrame = false;
    }

    private void Update()
    {
        if (Utilities.lastCheatCode != "" && Time.timeScale > 0 && go.transform.childCount == 0)
        {
            GameObject made = Instantiate(cheatDialog);
            made.SetActive(true);
            made.transform.SetParent(go.transform,false);
        }

        if (Utilities.lastCheatCode != "") { TextBoxGiverHandler.SpawnNewBox(ref cheatDialog, null); }

        while (cheatkeys.Count > 50)
        {
            cheatkeys.Remove(cheatkeys[0]);
        }

        float t3 = Time.timeScale * 0.016666667f;
        if (Time.timeScale == 0f)
        {
            t3 = 0.016666667f;
        }
        if (timerOn)
        {
            if (Time.timeScale > 0)
            {
                timer = Mathf.Max(0f, timer - t3);
            }
            if (timerBulletRing && !gaveTimerBulletRing)
            {
                Instantiate(timerBulletRing, Vector3.zero, Quaternion.identity, transform.parent);
                gaveTimerBulletRing = true;
            }
        }
        Time.maximumDeltaTime = Mathf.Max(t3, 0.01666666f);
        Time.fixedDeltaTime = Mathf.Max(t3, 0.01666666f);
        objectSFX.SetFloat("ObjectPitch", Fastmath.FastSqrt(Time.timeScale));
            //objectSFX.SetFloat("ObjPitchB", 1 / Mathf.Sqrt(Time.timeScale));
            //objectSFX.SetFloat("ObjPitchC", 1 / Mathf.Sqrt(Time.timeScale));
        if (Time.timeScale == 0f)
        {
            objectSFX.SetFloat("ObjectPitch", 1);
            //objectSFX.SetFloat("ObjPitchB", 1);
            //objectSFX.SetFloat("ObjPitchC", 1);
        }

        if (KHealth.someoneDied)
        {
            bgmCont = BGMController.main;
            if (onDeathBGM && bgmCont)
            {
                if (onDeathBGM != bgmCont.GetComponent<AudioSource>().clip)
                {
                    bgmCont.InstantMusicChange(onDeathBGM, true);
                }
                bgmCont.mustFollowLevel1MinTimer = false;
                onDeathBGM = null;
            }
            timerOn = false;
        }

        if (Door1.levelComplete && timerOn)
        {
            timerOn = false;
            bgmCont.InstantMusicChange(null, false);
        }
    }

}
