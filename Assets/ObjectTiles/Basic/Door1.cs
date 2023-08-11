using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Door1 : MonoBehaviour {
    
    public enum Lock
    {
        None,Money,Time,Special
    }

    public enum SpecialLockType
    {
        None,TouchLimitClear,ClonesMade,OnTurnZero
    }

    //public string sceneName;

    //public Encontrolmentation player;

    private GameObject tutorialObj;//???

    public GameObject prefabSpeedBonusUI;

    public static int collected;
    public Lock locke;
    public SpecialLockType specLockType = SpecialLockType.None;
    public float x;
    public bool mustAlignWithDoor = true;
    public bool attractAndInsertPlayer = true;
    public float extraWaitBeforeLoad = 0f;
    public string forcePlayerAnimation = "";
    public string contextAction = "Exit";
    public Transform doorLeft;
    public Transform doorRight;
    public Transform counter;
    public SpriteRenderer tens;
    public SpriteRenderer ones;
    public Sprite[] digits;
    private int countie;
    private bool con;
    private bool open = true;
    public string specialLockMsg;
    public Material doorMaterial;
    public AudioSource doorChangeStateSound;
    public AudioSource doorEnterSound;
    public AudioClip changeBGM = null;
    private bool what = false;
    private bool byeBye = false;
    public bool trivialCompletion;
    public string trivialCompletionMessage = "";
    public static bool levelComplete = false;
    public static bool completionWasTrivial = false;
    public static double levelCompleteTime = -1;
    public static string trivialCompletionMessageNow;

    private bool givenTime;
    private static bool stoppedTimer;

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
    //public GameObject speedBonusBox;

    //private List<Encontrolmentation> e = new List<Encontrolmentation>() { };


    // Use this for initialization
    void Start () {
        givenTime = false;
        stoppedTimer = false;
        collected = 0;
        con = false;
        what = false;
        byeBye = false;
        levelComplete = false;
        completionWasTrivial = false;
        trivialCompletionMessageNow = "";

        //if the audios are absent this will try to find and get them
        if (doorEnterSound == null)
        {
            doorEnterSound = GameObject.Find("doorEnter").GetComponent<AudioSource>();
        }
        ////

	if (locke != Lock.None)
        {
            open = false;
            doorLeft.localPosition = new Vector3(-2, -6, 0);
            doorRight.localPosition = new Vector3(2, -6, 0);
            counter.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            doorLeft.localPosition = new Vector3(-22, -6, 0);
            doorRight.localPosition = new Vector3(22, -6, 0);
            counter.localScale = new Vector3(1, 0, 1);
        }

            
        if (locke == Lock.Time)
        {
            Color cc = new Color(0.75f, 0.5f, 1f);
            doorLeft.GetComponent<SpriteRenderer>().color = cc;
            doorRight.GetComponent<SpriteRenderer>().color = cc;
            tens.color = cc;
            ones.color = cc;
        }
        if (locke == Lock.Money)
        {
            Color cc = new Color(0.8f, 0.76f, 0.5f);
            doorLeft.GetComponent<SpriteRenderer>().color = cc;
            doorRight.GetComponent<SpriteRenderer>().color = cc;
            tens.color = cc;
            ones.color = cc;
        }
        if (locke == Lock.Special)
        {
            /*Color cc = new Color(0.8f, 0.76f, 0.5f);
            doorLeft.GetComponent<SpriteRenderer>().color = cc;
            doorRight.GetComponent<SpriteRenderer>().color = cc;
            tens.color = cc;
            ones.color = cc;*/  //Nothing for now :<
        }


    }

    IEnumerator LoadNextLevel()
    {
        if (!byeBye)
        {
            byeBye = levelComplete = true;
            completionWasTrivial = trivialCompletion;
            //Utilities.canPauseGame = false;
            //Utilities.canUseInventory = false;
            if (!trivialCompletion)
            {
                doorEnterSound.Stop(); //???
                doorEnterSound.Play();
                yield return WaitForRealSeconds(0.5f);
            }
            else
            {
                trivialCompletionMessageNow = trivialCompletionMessage;
            }
            open = false;
            yield return WaitForRealSeconds(0.9f);
            while (doorEnterSound.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            if (extraWaitBeforeLoad > 0.01f)
            {
                yield return new WaitForSecondsRealtime(extraWaitBeforeLoad);
            }

            BannerText bt = FindObjectOfType<BannerText>();
            while (Time.timeScale == 0 /*paused*/|| bt.texts.Count != 0) //can't leave level until all texts are read
            {
                yield return new WaitForEndOfFrame();
            }

            if (levelComplete)
            {
                // saving stuff has moved to primAddScene

                GetComponent<primAddScene>().activate = true;
            }
        }
    }

    public void Open()
    {
        open = true;
    }

    public void Close()
    {
        open = false;
    }

    private void SpecialLockStuff()
    {
        switch (specLockType)
        {
            case SpecialLockType.TouchLimitClear:
                countie = Mathf.Clamp(TouchLimitBlock.touchLimitCount, 0, 99);
                tens.sprite = digits[countie / 10];
                ones.sprite = digits[countie % 10];
                break;
            case SpecialLockType.ClonesMade:
            case SpecialLockType.OnTurnZero:
                countie = Mathf.Clamp((int)x - DeadlyCloneMaker.clonesMadeThisLevel, 0, 99);
                tens.sprite = digits[countie / 10];
                ones.sprite = digits[countie % 10];
                if (countie <= 0 && !what)
                {
                    open = true;
                }
                break;
            default:
            case SpecialLockType.None:
                break;
        }
    }

    public void SetNumber(int x)
    {
        if (x < 0 || x > 99)
        {
            print("door1 says setnumber was too big or small. modulo 100 will be performed");
            x = (int)Mathf.Repeat(x, 100);
        }
        tens.sprite = digits[x / 10];
        ones.sprite = digits[x % 10];
    }

    void Update () {
        Encontrolmentation player = LevelInfoContainer.GetActiveControl();
        //open = (locke == Lock.None);
        bool moving = true;
        if (open)
        {
            counter.localScale = Vector3.Lerp(counter.localScale, new Vector3(1, 0, 1), 0.25f);

            if (counter.localScale.y < 0.1f)
            {
                counter.localScale = new Vector3(1, 0, 1);
            }
            doorLeft.localPosition -= new Vector3(1, 0, 0);
            if (doorLeft.localPosition.x < -22f)
            {
                doorLeft.localPosition = new Vector3(-22, -6, 0);
                moving = false;
                doorChangeStateSound.Stop();
            }
            doorRight.localPosition += new Vector3(1, 0, 0);
            if (doorRight.localPosition.x > 22f)
            {
                doorRight.localPosition = new Vector3(22, -6, 0);
                moving = false;
                doorChangeStateSound.Stop();
            }
        }
        else
        {
            if (!what)
            {
                counter.localScale = Vector3.Lerp(counter.localScale, new Vector3(1, 1, 1), 0.25f);
            }
            if (what && attractAndInsertPlayer)
            {
                player.transform.position = Vector3.Lerp(player.transform.position, transform.position - transform.up * 12, 0.25f);
            }
            if (counter.localScale.y > 0.9f)
            {
                counter.localScale = new Vector3(1, 1, 1);
            }
            doorLeft.localPosition += new Vector3(1, 0, 0);
            if (doorLeft.localPosition.x > -2f)
            {
                doorLeft.localPosition = new Vector3(-2, -6, 0);
                moving = false;
                doorChangeStateSound.Stop();
            }
            doorRight.localPosition -= new Vector3(1, 0, 0);
            if (doorRight.localPosition.x < 2f)
            {
                doorRight.localPosition = new Vector3(2, -6, 0);
                moving = false;
                doorChangeStateSound.Stop();
            }

            if (locke == Lock.Time)
            {
                countie = Mathf.Clamp(Mathf.CeilToInt((float)(x - DoubleTime.ScaledTimeSinceLoad)), 0, 99);
                tens.sprite = digits[countie / 10];
                ones.sprite = digits[countie % 10];
                if (countie <= 0 && !what)
                {
                    open = true;
                }
            }
            if (locke == Lock.Money)
            {
                countie = (int)Mathf.Clamp(x - collected, 0, 99);
                tens.sprite = digits[countie / 10];
                ones.sprite = digits[countie % 10];
                if (countie <= 0 && !what)
                {
                    open = true;
                }
            }
            if (locke == Lock.Special)
            {
                SpecialLockStuff();
            }
        }

        if (moving)
        {
            if (!doorChangeStateSound.isPlaying)
            {
                doorChangeStateSound.Play();
            }
        }

        try
        {
            Vector2 trp = player.transform.position;
            Vector2 trm = transform.InverseTransformPoint(trp);

            BasicMove bm = player.GetComponent<BasicMove>();
            if (System.Math.Abs(trm.x) <= 28f && (!mustAlignWithDoor || System.Math.Abs(trm.y +12f) <= 8f) && open && bm.grounded > 0 && bm.CanCollide)
            {
                player.eventBbutton = Encontrolmentation.ActionButton.XButton;
                player.eventBName = contextAction;
                player.givenObjIdentifier = gameObject.GetInstanceID();
                con = true;
            }
            /*else if (player.eventBName == contextAction || !open)
            {
                if (con)
                {
                    player.eventBbutton = Encontrolmentation.ActionButton.Nothing;
                    player.eventBName = "";
                    player.givenObjIdentifier = 0;
                    con = false;
                }
            }*/
        }
        catch
        {
            //   :<
        }

        if (con && ((player.flags & 64UL) == 64UL) && ((player.currentState & 64UL) == 64UL) && player.givenObjIdentifier == gameObject.GetInstanceID() && Time.timeScale > 0)
        {
            Animator an = player.GetComponent<Animator>();
            int sb = SceneManager.GetActiveScene().buildIndex;
            if (!trivialCompletion) //&& Utilities.loadedSaveData.leveldatas[sb].winTime[Utilities.replayMode] == Mathf.Infinity)
            {
                if (Utilities.replayLevel && !givenTime) {
                    givenTime = true;
                    levelCompleteTime = DoubleTime.UnscaledTimeSinceLoad - 0.016666666666666666;
                    Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.WinTime, levelCompleteTime);
                }
                GameObject ne = PlrUI.MakeStatusBox(prefabSpeedBonusUI, player.transform);
                ne.GetComponent<PlrUIBones>().player = player.gameObject;
            }
            if (changeBGM && BGMController.main) { BGMController.main.InstantMusicChange(changeBGM, true); }
            Utilities.tetrahedronTime = -5f;
            if (!stoppedTimer)
            {
                Utilities.StopGameTimerHere();
                if (!trivialCompletion)
                {
                    BGMController.main?.DuckOutAtSpeedForFrames(20f, 55);
                }
                stoppedTimer = true;
            }
            player.enabled = false;
            if (player.GetComponent<BasicMove>())
            {
                player.GetComponent<BasicMove>().enabled = false;
                player.GetComponent<BasicMove>().momentumMode = false;
                player.GetComponent<BasicMove>().fakePhysicsVel = Vector2.zero;
                player.GetComponent<AudioSource>().Stop();
            }
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            player.GetComponent<Rigidbody2D>().isKinematic = true;
            if (attractAndInsertPlayer)
            {
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                sr.material = doorMaterial;
                sr.sortingLayerName = "FG";
                sr.sortingOrder = 16384;
            }
            what = true;
            LevelInfoContainer li = LevelInfoContainer.main ?? FindObjectOfType<LevelInfoContainer>();
            if (Utilities.replayLevel && KHealth.hitsThisLevel == 0 && forcePlayerAnimation == "")
            {
                an.CrossFade("superWin", 0f);
            }

            if (forcePlayerAnimation != "")
            {
                an.CrossFade(forcePlayerAnimation, 0f);
            }

            if (player.gameObject.CompareTag("Player")) //is khal. change this later to work for choice of boss fighter
            {
                GetComponent<primAddScene>().doorPlayer = 0;
            }
            GetComponent<primAddScene>().doorChange = true;
            StartCoroutine(LoadNextLevel());
        }

        /*List<Encontrolmentation> ee = new List<Encontrolmentation>() { };
        foreach (var col in Physics2D.OverlapCircleAll(transform.position, 12, 1048576))
        {
            if (col.GetComponent<Encontrolmentation>())
            {
                ee.Add(col.GetComponent<Encontrolmentation>());
            }
        }
        List<Encontrolmentation> e2 = new List<Encontrolmentation>() { };
        foreach (var r in ee)
        {
            if (!e.Contains(r))
            {
                r.eventBbutton = Encontrolmentation.ActionButton.XButton;
                r.eventBName = "Leave";
            }
        }
        foreach (var r in e)
        {
            if (!ee.Contains(r))
            {
                r.eventBbutton = Encontrolmentation.ActionButton.Nothing;
                r.eventBName = "";
            }
        }
        e = ee;*/
	}
}
