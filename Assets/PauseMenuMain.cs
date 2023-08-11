using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class PauseMenuMain : MonoBehaviour {

    public GameObject pauseMenu;
    public GameObject pauseMenuText;
    public GameObject[] pauseMenuButtons;
    public GameObject pauseMenuArrow;

    public int currentChoice;
    public float longPressDelay;
    public float longPressRepeatDelay;

    private int max;
    private float origTimeScale;

    public static double[] lastPauseScaledTimes = new double[3] { -100000, -100000, -100000 };

    public AudioSource selectChangeSound;
    public AudioSource pauseOnSound;
    public AudioSource pauseOffSound;
    public AudioSource pauseFakeSound;

    public MenuConfirmBox confirmBox;
    public Text confirmText;

    public GameObject pausingTooQuicklyMessage;
    public GameObject graspControllerMessage;

    private Encontrolmentation encmt;

    private int lastFrameAllowInput = 2;

    private GameObject setMen;

    public static bool gameIsPausedThroughMenu;

    public static PauseMenuMain main;

    public enum MenuItems
    {
        Resume,
        Map,
        ReturnToHubLevel,
        MusicSettings,
        ControllerSettings,
        Save,
        Shutdown
    };

    public string[] buttonNames;

    void Start () {
        main = this;
        gameIsPausedThroughMenu = false;
        pauseMenu.SetActive(false);
        max = Enum.GetNames(typeof(MenuItems)).Length -1;
        encmt = GetComponent<Encontrolmentation>();
        lastFrameAllowInput = 2;
        lastPauseScaledTimes = new double[3] { -100000, -100000, -100000 };
    }

    public IEnumerator TooQuickMessage(float t)
    {
        pausingTooQuicklyMessage.SetActive(true);
        yield return new WaitForSecondsRealtime(t);
        pausingTooQuicklyMessage.SetActive(false);
    }

    public IEnumerator GraspControllerMessage()
    {
        graspControllerMessage.SetActive(true);
        yield return new WaitUntil(() => BrainwaveReader.internalValue >= -0.99f);
        graspControllerMessage.SetActive(false);
    }

    public bool PausingTooQuickCheck()
    {
        if (DoubleTime.UnscaledTimeSinceLoad - lastPauseScaledTimes[0] < 10f)
        {
            if (!pausingTooQuicklyMessage.activeSelf)
            {
                StartCoroutine(TooQuickMessage((float)(10f - DoubleTime.UnscaledTimeSinceLoad + lastPauseScaledTimes[0])));
            }
            return true;
        }

        if (DoubleTime.UnscaledTimeSinceLoad - lastPauseScaledTimes[2] < 1f)
        {
            if (!pausingTooQuicklyMessage.activeSelf)
            {
                StartCoroutine(TooQuickMessage((float)(1f - DoubleTime.UnscaledTimeSinceLoad + lastPauseScaledTimes[2])));
            }
            return true;
        }

        lastPauseScaledTimes[0] = lastPauseScaledTimes[1];
        lastPauseScaledTimes[1] = lastPauseScaledTimes[2];
        lastPauseScaledTimes[2] = DoubleTime.UnscaledTimeSinceLoad;

        return false;
    }

    public void Open()
    {
        if (PausingTooQuickCheck()) { return; }

        gameIsPausedThroughMenu = true;
        pauseOnSound.Stop();
        pauseOffSound.Stop();

        //lower bgm volume
        origTimeScale = Time.timeScale;
        Time.timeScale = 0;
        currentChoice = 0;
        pauseMenu.SetActive(true);
        //KHealth[] healthStuff = FindObjectsOfType<KHealth>();
        bool pauseBroke = false;
        //foreach (KHealth healthStuffi in healthStuff)
        {
            if (KHealth.someoneDied)
            {
                foreach (var item in pauseMenuButtons)
                {
                    Destroy(item);
                }
                Destroy(pauseMenuArrow);
                //pauseMenu.transform.Find("PAUSEDsignHolder").gameObject.SetActive(false);
                Time.timeScale = 1;
                pauseMenuText.GetComponent<Text>().text = "No escape!";
                pauseFakeSound.Play();
                pauseBroke = true;
            //break;
            }
        }
        if (!pauseBroke)
        {
            pauseOnSound.Play();
        }
    }

    public void Close()
    {
        if (!gameIsPausedThroughMenu) { return; }
        gameIsPausedThroughMenu = false;
        if (pauseMenu.activeInHierarchy)
        {
            pauseOnSound.Stop();
            pauseOffSound.Stop();
            pauseOffSound.Play();
        }
        //return bgm volume
        Time.timeScale = origTimeScale;
        pauseMenu.SetActive(false);
    }

    public void OnPressFunction()
    {
        switch (currentChoice)
        {
            case 0:
                //resume
                Close(); break;
            case 1:
                //suicide
                Close();
                foreach (KHealth kh in FindObjectsOfType<KHealth>())
                {
                    Encontrolmentation e = kh.GetComponent<Encontrolmentation>();
                    if (e && e.allowUserInput && kh.enabled)
                    {
                        kh.ChangeHealth(Mathf.NegativeInfinity, "suicide");
                    }
                }
                break;
            case 2:
                //invest
                currentChoice = 0;
                AsyncOperation a = SceneManager.LoadSceneAsync("InvestUI", LoadSceneMode.Additive); //change this to a resource!
                a.allowSceneActivation = true;
                StartCoroutine(A1(a));
                break;
            case 3:
                //stats
                setMen = Instantiate(Resources.Load<GameObject>("StatsMenu"));
                setMen.SetActive(true);
                setMen.transform.SetParent(transform);
                setMen.transform.localScale = Vector3.one;
                setMen.transform.localPosition = Vector3.zero;
                setMen.GetComponent<InSaveMenuBase>().Open(encmt);
                break;
            case 4:
                //mini settings
                setMen = Instantiate(Resources.Load<GameObject>("SettingsBox"));
                setMen.transform.SetParent(transform.parent);
                setMen.transform.localPosition = Vector3.zero;
                setMen.transform.Find("Panel").GetComponent<InSaveMenuBase>().Open(encmt);
                break;
            case 5:
                // to save menu
                confirmText.text = "Save and quit to file menu?";
                confirmBox.toSaveMenu = true;
                confirmBox.Open(encmt);
                break;
            case 6:
                // to first menu
                confirmText.text = "Save and quit to title menu?";
                confirmBox.toSaveMenu = false;
                confirmBox.Open(encmt);
                break;
            default:
                break;
        }
    }

    private IEnumerator A1(AsyncOperation a)
    {
        
        Encontrolmentation e = GetComponent<Encontrolmentation>();
        e.allowUserInput = false;
        while (!a.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        foreach (var i in SceneManager.GetSceneByName("InvestUI").GetRootGameObjects()) //the investing scene contains something we must set
        {
            if (i.name == "Canvas")
            {
                i.transform.GetChild(0).GetComponent<Encontrolmentation>().possiblePreviousFocus = e;
            }
        }
    }




    public void RedrawMenu()
    {
        if (pauseMenu != null && pauseMenu.activeInHierarchy && encmt.allowUserInput)
        {
            string buttonName = buttonNames[currentChoice];
            //KHealth z = FindObjectOfType<KHealth>(); //update this to work for more mans
            if (KHealth.someoneDied)
            {
                pauseMenuText.GetComponent<Text>().text = "No escape!";
            }
            else
            {//<quad material=0 size=60 x=0.0 y=0.75 width=0.25 height=0.25/><material=1>: Resume </material>
                pauseMenuText.GetComponent<Text>().text = buttonNames[currentChoice];
            }
            GameObject buttonChosen = null;
            for (int i = 0; i < pauseMenuButtons.Length; i++)
            {
                buttonChosen = pauseMenuButtons[i];
                //using the shader "Sprites/RodBandW"
                if (i == currentChoice)
                {
                    Material m = Instantiate(buttonChosen.GetComponent<CanvasRenderer>().GetMaterial());
                    m.SetFloat("_B", 0.5f);
                    m.SetFloat("_I", 0f);
                    buttonChosen.GetComponent<CanvasRenderer>().SetMaterial(m, 0);
                    Vector3 buttonPos = buttonChosen.GetComponent<RectTransform>().position;
                    pauseMenuArrow.GetComponent<RectTransform>().position = buttonPos + new Vector3(0, 32 - 3 * (float)Math.Cos(DoubleTime.UnscaledTimeRunning * 6.28f));
                }
                else
                {
                    Material m = Instantiate(buttonChosen.GetComponent<CanvasRenderer>().GetMaterial());
                    m.SetFloat("_B", 0.07f);
                    m.SetFloat("_I", 1f);
                    buttonChosen.GetComponent<CanvasRenderer>().SetMaterial(m, 0);
                }
            }


            //Controls

            if (encmt.ButtonDown(2UL, 3UL))
            {
                if (currentChoice != max)
                {
                    currentChoice = Mathf.Clamp(currentChoice + 1, 0, max);
                }
                else
                {
                    currentChoice = 0;
                }
                selectChangeSound.Stop();
                selectChangeSound.pitch = 1.1f;
                selectChangeSound.panStereo = 0.5f;
                selectChangeSound.Play();
            }

            if (encmt.ButtonDown(1UL, 3UL))
            {

                if (currentChoice != 0)
                {
                    currentChoice = Mathf.Clamp(currentChoice - 1, 0, max);
                }
                else
                {
                    currentChoice = max;
                }
                selectChangeSound.Stop();
                selectChangeSound.pitch = 0.9f;
                selectChangeSound.panStereo = -0.5f;
                selectChangeSound.Play();

            }
        }
    }

	// Update is called once per frame
	void Update () {

        if (setMen && !setMen.activeSelf) { Destroy(setMen); }

        RedrawMenu();
        bool done = false;
        
        if (encmt.ButtonDown(1024UL, 3072UL) /*start*/ && !pauseMenu.activeInHierarchy && !done && Utilities.canPauseGame && Time.timeScale > 0)
        {
            Open();
            done = true;
        }
        if (((encmt.ButtonDown(1024UL, 3072UL) || encmt.ButtonDown(32UL, 240UL)) && pauseMenu.activeInHierarchy && !done) || !Utilities.canPauseGame)
        {
            Close();
            done = true;
        }

        if (lastFrameAllowInput == 0)
        {
            if (encmt.ButtonDown(16UL, 240UL) && pauseMenu.activeInHierarchy)
            {
                OnPressFunction();
            }
        }
        lastFrameAllowInput = (encmt.allowUserInput) ? Mathf.Max(lastFrameAllowInput - 1, 0) : 2;

        if (gameIsPausedThroughMenu)
        {
            lastPauseScaledTimes[0] += 1.0 / 60.0;
            lastPauseScaledTimes[1] += 1.0 / 60.0;
            lastPauseScaledTimes[2] += 1.0 / 60.0;
        }

        if (BrainwaveReader.internalValue < -0.99f && Fakerand.Int(0, 1000000) == 3435 && !graspControllerMessage.activeSelf)
        {
            StartCoroutine(GraspControllerMessage());
        }
    }
}
