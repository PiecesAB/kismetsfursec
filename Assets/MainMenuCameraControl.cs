using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.SceneManagement;

public class MainMenuCameraControl : MonoBehaviour {

    public Vector3 startAngle;
    public Vector3 endAngle;
    public double startPanTime;
    public GameObject selec;
    private Quaternion vs;
    public float there;
    private float fl1;
    private bool noLongerLookingUp = false;
    private bool startedMovingToOption;
    private int dir;
    private Vector3 movedir;
    //private float smoothDampVal = 0f;
    public int lockedIn = -1;
    public Encontrolmentation[] guiControllers = new Encontrolmentation[4];
    private Encontrolmentation e;
    public SpriteRenderer fadeToBlack;
    public string nextSceneName;
    public bool removeFadeBlack;

    public GameObject startControlPanel;
    public GameObject moveControlPanel;
    public GameObject menuControlPanel;
    public RectTransform gamepadUnavailable;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    public AudioSource movementSound;

	void Start () {
        Screen.SetResolution(640, 480, true);
        Settings.PrepareAll(true);
        Settings.UpdateAll();
        Utilities.replayLevel = false;
        selec.GetComponent<Renderer>().material.color = Color.clear;
        startPanTime += DoubleTime.UnscaledTimeRunning;
        transform.localEulerAngles = startAngle;
        vs = Quaternion.Euler(transform.eulerAngles);
        there = 0f;
        fl1 = 0;
        noLongerLookingUp = false;
        startedMovingToOption = false;
        lockedIn = -1;
        e = GetComponent<Encontrolmentation>();
        removeFadeBlack = true;
        fadeToBlack.color = Color.black;
        startControlPanel.SetActive(true);
        moveControlPanel.SetActive(false);
        menuControlPanel.SetActive(false);
        //smoothDampVal = 0f;
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    IEnumerator SelectGameAnim(int saveNum)
    {
        removeFadeBlack = false;
        ColorCorrectionCurves c = e.GetComponent<ColorCorrectionCurves>();
        c.enabled = true;
        c.saturation = 5f;
        while (fadeToBlack.color.a < 1f)
        {
            c.saturation = Mathf.Lerp(c.saturation, 0.5f, 0.005f);
            fadeToBlack.color = new Color(0f, 0f, 0f, fadeToBlack.color.a + 0.005f);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        Utilities.StartGame(saveNum);

        //make loading real menu scene
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);

    }

    IEnumerator GamepadUnavailable()
    {
        for (int i = 0; i < 30; ++i)
        {
            gamepadUnavailable.sizeDelta = Vector2.Lerp(gamepadUnavailable.sizeDelta, new Vector2(480, 64), 0.3f);
            yield return new WaitForEndOfFrame();
        }
        gamepadUnavailable.sizeDelta = new Vector2(480, 64);
        for (int i = 0; i < 600; ++i)
        {
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < 30; ++i)
        {
            gamepadUnavailable.sizeDelta = Vector2.Lerp(gamepadUnavailable.sizeDelta, Vector2.zero, 0.3f);
            yield return new WaitForEndOfFrame();
        }
        gamepadUnavailable.sizeDelta = Vector2.zero;
        yield return null;
    }

    public void SelectGame(int saveNum)
    {
        e.allowUserInput = false;
        StartCoroutine(SelectGameAnim(saveNum));
    }
	
	void Update () {
        //Encontrolmentation e = GetComponent<Encontrolmentation>();
        if (fadeToBlack.color.a > 0f && removeFadeBlack)
        {
            fadeToBlack.color = new Color(0f, 0f, 0f, fadeToBlack.color.a - 0.02f);
        }
        if (e.currentState >= 4UL && !noLongerLookingUp)
        {
            startPanTime = 0f;
            BGMController bgc = FindObjectOfType<BGMController>();
            noLongerLookingUp = true;
            startControlPanel.SetActive(false);
            moveControlPanel.SetActive(true);
            menuControlPanel.SetActive(false);
        }
        if (startPanTime != 0f)
        {
            fl1 = Mathf.Lerp(fl1, 0f, 0.1f);
            Vector3 le = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(le.x, le.y + fl1, le.z);
            there = 100f;
        }
        if ((there < 0.07f || there > 359.3f) && DoubleTime.UnscaledTimeRunning > startPanTime+0.4f)
        {
            if (lockedIn == -1 && transform.localPosition.sqrMagnitude < 16f)
            {
                selec.GetComponent<Renderer>().material.color = Color.Lerp(selec.GetComponent<Renderer>().material.color, new Color32(56, 51, 66, 255), 0.1f);
            }
            else
            {
                selec.GetComponent<Renderer>().material.color = Color.Lerp(selec.GetComponent<Renderer>().material.color, Color.clear, 0.17f);
            }
        }
        else
        {
            selec.GetComponent<Renderer>().material.color = Color.Lerp(selec.GetComponent<Renderer>().material.color, Color.clear, 0.17f);
        }

        if (!startedMovingToOption && lockedIn == -1)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, 0.05f);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, 0.2f);
        }

        if ((e.currentState & 15UL) == 1UL && lockedIn == -1)
        {
            startedMovingToOption = false;
            fl1 = Mathf.Lerp(fl1, -2f, 0.05f);
            Vector3 le = transform.localEulerAngles;
            endAngle = new Vector3(0, Mathf.Round(transform.localEulerAngles.y / 90f) * 90f, 0);
            vs = Quaternion.Euler(endAngle);
            transform.localEulerAngles = new Vector3(le.x, le.y + fl1, le.z);
            there = 100f;
        }
        else if ((e.currentState & 15UL) == 2UL && lockedIn == -1)
        {
            startedMovingToOption = false;
            fl1 = Mathf.Lerp(fl1, 2f, 0.05f);
            Vector3 le = transform.localEulerAngles;
            endAngle = new Vector3(0, Mathf.Round(transform.localEulerAngles.y / 90f) * 90f, 0);
            vs = Quaternion.Euler(endAngle);
            transform.localEulerAngles = new Vector3(le.x, le.y + fl1, le.z);
            there = 100f;
        }
        else if ((e.currentState & 15UL) == 3UL && lockedIn == -1)
        {
            startedMovingToOption = false;
            fl1 = Mathf.Lerp(fl1, 0f, 0.05f);
            Vector3 le = transform.localEulerAngles;
            endAngle = new Vector3(0, transform.localEulerAngles.y, 0);
            vs = Quaternion.Euler(endAngle);
            transform.localEulerAngles = new Vector3(le.x, le.y + fl1, le.z);
            there = 100f;
        }
        else 
        {
            if (DoubleTime.UnscaledTimeRunning > startPanTime)
            {
                Quaternion v1 = transform.localRotation;
                if (startedMovingToOption && lockedIn == -1)
                {
                    transform.localPosition += movedir*3f;
                    selec.GetComponent<Renderer>().material.color = Color.Lerp(selec.GetComponent<Renderer>().material.color, Color.clear, 0.17f);
                }

                if (transform.localPosition.sqrMagnitude > 102400f && lockedIn == -1) //320
                {
                    transform.localPosition = transform.localPosition.normalized * 320f;
                    lockedIn = dir;
                    if (dir > -1 && guiControllers[dir])
                    {
                        guiControllers[dir].allowUserInput = true;
                    }

                    if (lockedIn == 2)
                    {
                        PlayerPrefs.DeleteKey("gameIsOpen");
                        Application.Quit();
                        print("Quit");
                    }
                    else
                    {
                        startControlPanel.SetActive(false);
                        moveControlPanel.SetActive(false);
                        menuControlPanel.SetActive(true);
                    }

                    if (lockedIn == 3 || lockedIn == 1)
                    {
                        GetComponent<BloomOptimized>().enabled = false;
                    }
                }

                if ((e.currentState & 4UL) == 4UL)
                {
                    float ft = transform.localEulerAngles.y % 90f;
                    if (ft > 45f)
                    {
                        ft -= 90f;
                    }
                    float ta = transform.localEulerAngles.y - ft;
                    float ty = Mathf.Lerp(transform.localEulerAngles.y, ta, 0.1f);
                    ty = Mathf.MoveTowards(ty, ta, 0.2f);
                    transform.localEulerAngles = new Vector3(Mathf.LerpAngle(transform.localEulerAngles.x,0f,0.05f),ty,transform.localEulerAngles.z);
                    movedir = new Vector3(Mathf.Sin(ta * Mathf.Deg2Rad), 0f, Mathf.Cos(ta * Mathf.Deg2Rad));
                    if (transform.localPosition.sqrMagnitude < 16f)
                    {
                        dir = ((int)((ta + 405f)/90f))&3;
                        startedMovingToOption = true;
                    }
                }
                else if ((e.currentState & 15UL) == 0UL)
                {
                    
                    startedMovingToOption = false;
                    
                }

                fl1 = 0f;
                Quaternion ve = Quaternion.Euler(endAngle);
                vs = Quaternion.Lerp(vs, ve, 0.03f);
                transform.localRotation = Quaternion.Lerp(v1, vs, 0.03f);
                there = (transform.localEulerAngles - v1.eulerAngles).magnitude;
            }
        }

        if ((e.flags & 32UL) == 32UL && !InSaveRemapControls.open)
        {
            if (dir > -1 && guiControllers[dir])
            {
                guiControllers[dir].allowUserInput = false;
            }
            dir = lockedIn = -1;
            GetComponent<BloomOptimized>().enabled = true;
            startControlPanel.SetActive(false);
            moveControlPanel.SetActive(true);
            menuControlPanel.SetActive(false);
        }

        float totalVel = Quaternion.Angle(lastRotation, transform.rotation) * 2f + (transform.position - lastPosition).magnitude;
        if (totalVel >= 0.3f)
        {
            if (!movementSound.isPlaying) { movementSound.Play(); }
            movementSound.volume = Mathf.Clamp(totalVel * 0.12f, 0f, 0.6f);
        }
        else
        {
            if (movementSound.isPlaying) { movementSound.Stop(); }
        }
        lastPosition = transform.position;
        lastRotation = transform.rotation;

        if (CrossSceneControlHelper.fellBackToDefaults)
        {
            CrossSceneControlHelper.fellBackToDefaults = false;
            StartCoroutine(GamepadUnavailable());
        }
    }
}
