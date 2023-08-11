using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HudDieScreen : MonoBehaviour {

    //public GameObject deathDiamondTile;
    //public GameObject escapeDiamondTile;
    //public GameObject deathTextBanner;
   // public GameObject escapeTextBanner;
   public static bool finish;
    public string[] taunts;
    public string[] compliments;
    public float waitRowTime;
    public float existTime;
    public Image deathMsgBanner;
    public Text deathMsgText;
    public Image deathScreen;
    public PrimJunkDeleteFunction myContainer;
    public int testMsg;

   // private List<GameObject> tiles = new List<GameObject>();
    private List<float> times = new List<float>();
    private bool on;

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

    public IEnumerator FinishTSD()
    {
        yield return WaitForRealSeconds((existTime - 0.32f) / 2f);
        float za = 1;
        while (za > 0)
        {
            Color cr = deathScreen.color;
            deathScreen.color = new Color(cr.r, cr.g, cr.b, za);
            cr = deathMsgBanner.color;
            deathMsgBanner.color = new Color(cr.r, cr.g, cr.b, za);
            cr = deathMsgText.color;
            deathMsgText.color = new Color(cr.r, cr.g, cr.b, za);

            za -= 0.0625f;
            yield return new WaitForEndOfFrame();
        }

        /* if (tiles.Count > 0)
        {
            tiles.Clear();
        }
        if (times.Count > 0)
        {
            times.Clear();
        }
        on = true;
        deathTextBanner.SetActive(true);
        deathTextBanner.GetComponentInChildren<Text>().text = taunts[(int)Fakerand.Single(0, taunts.Length - 0.01f)];
        Utilities.loadedSaveData.score = Utilities.toSaveData.score;
        //Utilities.loadedSaveData = Utilities.toSaveData;
        Utilities.tetrahedronTime = -5f;
        for (int yPos = 240; yPos >= -240; yPos -= 40)
        {
            for (int xPos = -320+(yPos%80); xPos <= 320-(yPos % 80); xPos += 80)
            {
                GameObject newTile = (GameObject)Instantiate(deathDiamondTile, new Vector3(xPos, yPos), Quaternion.identity);
                newTile.transform.SetParent(transform, false);
                newTile.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 1);
                tiles.Add(newTile);
                times.Add(DoubleTime.UnscaledTimeRunning);
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }*/
    }

    public IEnumerator TileScreenDeath(string deadReason)
    {

        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.01666666f;
        primAddScene p = gameObject.AddComponent<primAddScene>();
        if (LevelInfoContainer.main?.levelTheme == LevelInfoContainer.Theme.Travail)
        {
            p.sceneName = "DeathScreenGiveUp";
            LoadingRetryConfirm.forceGiveUp = false;
            LoadingRetryConfirm.customSubtitle = "";
        }
        else
        {
            p.sceneName = "DeathScreenA";
        }
        p.loadMode = LoadSceneMode.Single;
        p.loadStoreIfYouWant = gameObject.scene.name;
        p.doorChange = false;
        yield return new WaitForEndOfFrame();
        p.activate = true;

        /*foreach (Transform child in transform.parent)
        {
            if (child != transform.parent && child != transform)
            {
                Destroy(child.gameObject);
            }
        }
        DontDestroyOnLoad(myContainer.gameObject);
        transform.parent.GetComponent<Canvas>().sortingOrder = 1000;
        transform.localPosition = Vector3.zero;
        if (testMsg > -1)
        {
            deathMsgText.text = taunts[testMsg];
        }
        else
        {
            deathMsgText.text = taunts[Fakerand.Int(0, taunts.Length)];
        }
        float za = 0;
        while (za < 1)
        {
            Color cr = deathScreen.color;
            deathScreen.color = new Color(cr.r, cr.g, cr.b, za);
            cr = deathMsgBanner.color;
            deathMsgBanner.color = new Color(cr.r, cr.g, cr.b, za);
            cr = deathMsgText.color;
            deathMsgText.color = new Color(cr.r, cr.g, cr.b, za);

            za += 0.0625f;
            yield return new WaitForEndOfFrame();
        }
        Color cr2 = deathScreen.color;
        deathScreen.color = new Color(cr2.r, cr2.g, cr2.b, 1);
        cr2 = deathMsgBanner.color;
        deathMsgBanner.color = new Color(cr2.r, cr2.g, cr2.b, 1);
        cr2 = deathMsgText.color;
        deathMsgText.color = new Color(cr2.r, cr2.g, cr2.b, 1);
        yield return WaitForRealSeconds(existTime *0.2f);
        finish = true;
        AsyncOperation asy = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name,LoadSceneMode.Single);
        while (!asy.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return WaitForRealSeconds(existTime * 0.8f);
        Destroy(myContainer.gameObject);
        //*/
    }

    public IEnumerator TileScreenEscape(string nextSceneName)
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.01666666f;
        GameObject plr = GameObject.FindGameObjectWithTag("Player");
        plr.GetComponent<Rigidbody2D>().isKinematic = true;
        print(0);
        yield return 1;
        DontDestroyOnLoad(transform.parent.parent.gameObject);
        /*foreach (Transform child in transform.parent)
        {
            if (child != transform.parent && child != transform)
            {
                Destroy(child.gameObject);
            }
        }*/
        transform.parent.gameObject.GetComponent<Canvas>().sortingOrder = 1000;
       /* if (tiles.Count > 0)
        {
            tiles.Clear();
        }
        if (times.Count > 0)
        {
            times.Clear();
        }
        on = true;
        escapeTextBanner.SetActive(true);
        escapeTextBanner.GetComponentInChildren<Text>().text = compliments[(int)Fakerand.Single(0, compliments.Length - 0.01f)];
        
        for (int yPos = 240; yPos >= -240; yPos -= 40)
        {
            for (int xPos = -320 + (yPos % 80); xPos <= 320 - (yPos % 80); xPos += 80)
            {
                GameObject newTile = (GameObject)Instantiate(escapeDiamondTile, new Vector3(xPos, yPos), Quaternion.identity);
                newTile.transform.SetParent(transform, false);
                newTile.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 1);
                tiles.Add(newTile);
                times.Add(DoubleTime.UnscaledTimeRunning);
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForSeconds(tileExistTime/2);
        SceneManager.LoadSceneAsync(nextSceneName);*/
    }


    // Use this for initialization
    void Start () {
        on = false;
        if (finish)
        {
            StartCoroutine(FinishTSD());
        }
        finish = false;
	}

    // Update is called once per frame
    void Update()
    {
       /* if (on)
        {
            if (tiles.Count > 0 && tiles.Count == times.Count)
            {
                for (int i = 0; i < tiles.Count; i++)
                {
                    if (i==0)
                    {
                        float timeSince = 0;
                        if (times.Count > 0)
                        {
                            timeSince = DoubleTime.UnscaledTimeRunning - times[0];
                            if (timeSince > tileExistTime + times[times.Count - 1])
                            {
                                Destroy(transform.parent.parent.gameObject);
                            }
                        }

                        float junkVariable = Mathf.Clamp(Mathf.Pow(Mathf.Lerp(-2,2,timeSince/tileExistTime),3),-1000,1000);
                        if (deathTextBanner.activeInHierarchy)
                        {
                            deathTextBanner.GetComponent<RectTransform>().position = new Vector3(deathTextBanner.GetComponent<RectTransform>().position.x, junkVariable * 180 + 240, deathTextBanner.GetComponent<RectTransform>().position.z);
                        }
                        if (escapeTextBanner.activeInHierarchy)
                        {
                            escapeTextBanner.GetComponent<RectTransform>().position = new Vector3(deathTextBanner.GetComponent<RectTransform>().position.x, junkVariable * 180 + 240, deathTextBanner.GetComponent<RectTransform>().position.z);
                        }

                    }
                    if (tiles[i] != null)
                    {
                        float timeSince = DoubleTime.UnscaledTimeRunning - times[i];
                        float size = Mathf.Clamp(Mathf.Sin((Mathf.PI * Mathf.Clamp(timeSince,0,4)) / tileExistTime) * 1.2f, 0, 1f);
                        if (size < 0f)
                        {
                            Destroy(tiles[i]);
                        }
                        else
                        {
                            tiles[i].GetComponent<RectTransform>().localScale = new Vector3(size, size, 1);
                            tiles[i].GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, Mathf.Clamp(135 - (size * 135),0,999));
                        }
                    }
                }
            }
        }*/
    }
}
