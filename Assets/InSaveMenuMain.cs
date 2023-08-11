using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InSaveMenuMain : MonoBehaviour
{
    public bool test = false; // if true, skip setup
    public Image backgroundImage;
    public Sprite[] backgroundImageSelection = new Sprite[5];
    public GameObject keyboardMenuPrefab;
    public GameObject iconMenuPrefab;
    public GameObject startSelectDispPrefab;
    public GameObject[] optionDisplays;
    private float scrollDist;
    public int selection;
    public GameObject optionBGBar;
    public Image colorArrow;
    public RectTransform[] dottedInfoBoxes;
    public GameObject[] mainInfoBoxes;

    public GameObject welcomeBox;
    public GameObject replayStatsBox;

    public Text quitText;
    public MenuConfirmBox quitBox;

    public AudioSource initSound;
    public AudioSource changeSound;
    public AudioSource selectSound;

    private const float centerDist = 176f;
    private const float centerOffset = 80f;
    private const float optionDist = 45f;
    private const float movingVelCutoff = 0.1f;

    private const int dottedAnimFrames = 12;

    private int dottedTracker1 = -1;
    private int dottedTracker2 = 0;

    private bool lastFrameAllowInput = false;

    public GameObject[] nextUIs;

    public enum ControlMode
    {
        Intro, Cycle, Exit, Setup
    }

    [HideInInspector]
    public ControlMode controlMode;

    public Text welcomeText; // we might want to change it later

    private Encontrolmentation encmt;

    private void InitWheel()
    {
        optionBGBar.transform.eulerAngles = Vector3.back * 179f;
        for (int i = 0; i < mainInfoBoxes.Length; ++i) { mainInfoBoxes[i].SetActive(false); }
        for (int i = 0; i < dottedInfoBoxes.Length; ++i) { dottedInfoBoxes[i].gameObject.SetActive(false); }
    }

    private void MoveWheel()
    {
        if (controlMode != ControlMode.Cycle) { return; }

        optionBGBar.transform.eulerAngles = Vector3.forward * Mathf.LerpAngle(optionBGBar.transform.eulerAngles.z, 0f, 0.2f);
        float oldScrollDist = scrollDist;
        scrollDist = Mathf.Lerp(scrollDist, selection * optionDist, 0.2f);
        float scrollVel = scrollDist - oldScrollDist;

        //dotted info box animation
        if (selection == dottedTracker1 && Mathf.Abs(scrollVel) < movingVelCutoff)
        {
            if (dottedTracker2 < dottedAnimFrames)
            {
                float rat = (float)dottedTracker2 / dottedAnimFrames;
                dottedInfoBoxes[dottedTracker1].gameObject.SetActive(true);
                RectTransform optRect = optionDisplays[dottedTracker1].GetComponent<RectTransform>();
                RectTransform infoRect = mainInfoBoxes[dottedTracker1].GetComponent<RectTransform>();
                dottedInfoBoxes[dottedTracker1].sizeDelta = Vector2.Lerp(Vector2.zero, infoRect.sizeDelta, rat);
                dottedInfoBoxes[dottedTracker1].position = Vector3.Lerp(optRect.position, infoRect.position, rat);
                ++dottedTracker2;
                if (dottedTracker2 == dottedAnimFrames)
                {
                    mainInfoBoxes[dottedTracker1].SetActive(true);
                    dottedInfoBoxes[dottedTracker1].gameObject.SetActive(false);
                }
            }
        }

        if (selection != dottedTracker1)
        {
            mainInfoBoxes[dottedTracker1].SetActive(false);
            if (dottedTracker2 > 0)
            {
                float rat = (float)dottedTracker2 / dottedAnimFrames;
                dottedInfoBoxes[dottedTracker1].gameObject.SetActive(true);
                RectTransform optRect = optionDisplays[dottedTracker1].GetComponent<RectTransform>();
                RectTransform infoRect = mainInfoBoxes[dottedTracker1].GetComponent<RectTransform>();
                dottedInfoBoxes[dottedTracker1].sizeDelta = Vector2.Lerp(Vector2.zero, infoRect.sizeDelta, rat);
                dottedInfoBoxes[dottedTracker1].position = Vector3.Lerp(optRect.position, infoRect.position, rat);
                --dottedTracker2;
                if (dottedTracker2 == 0)
                {
                    dottedInfoBoxes[dottedTracker1].gameObject.SetActive(false);
                    dottedTracker1 = selection;
                }
            }
        }

        if (dottedTracker2 == 0) //extra check...
        {
            dottedTracker1 = selection;
        }


        Vector3 centerPos = transform.position + Vector3.left * centerDist;
        colorArrow.color = Color.Lerp(colorArrow.color,optionDisplays[selection].GetComponent<Image>().color,0.2f);
        optionBGBar.GetComponent<Image>().color = Color.Lerp(colorArrow.color, Color.white, 0.75f);
        for (int i = 0; i < optionDisplays.Length; ++i)
        {
            float thisDist = scrollDist - optionDist * i;
            float thisAngularDist = thisDist / centerDist;
            optionDisplays[i].transform.position = (centerPos + (Vector3.left * centerOffset))
                + (centerDist * new Vector3(Mathf.Cos(thisAngularDist), Mathf.Sin(thisAngularDist), 0));
            optionDisplays[i].transform.eulerAngles = Vector3.forward * thisAngularDist * Mathf.Rad2Deg;
        }
    }

    void MoveMain()
    {

    }

    public void BackgroundUpdate()
    {
        backgroundImage.transform.position = new Vector3(0,0,240);
        DateTime now = DateTime.Now;
        int id = 0;
        switch (now.Hour)
        {
            case 22: case 23: case 0: case 1: case 2: case 3: case 4: id = 4; break;
            case 5: case 6: case 7: case 8: case 9: id = 0; break;
            case 10: case 11: case 12: case 13: case 14: case 15: case 16: id = 1; break;
            case 17: case 18: case 19: id = 2; break;
            case 20: case 21: id = 3; break;
            default: break;
        }
        backgroundImage.sprite = backgroundImageSelection[id];
    }

    private bool Setup()
    {
        if (!Utilities.loadedSaveData.needsSetup || test) { return false; }
        transform.position = new Vector3(9999, 9999);
        KeyboardOpenForNaming("Welcome. Register your name.", false, null, OnSetupNameRegister);
        return true;
    }

    private void OnSetupNameRegister()
    {
        // choose icon
        IconSelectOpen(false, null, OnSetupIconRegister);
    }

    private void OnSetupIconRegister()
    {
        GameObject imen = Instantiate(startSelectDispPrefab);
        imen.transform.SetParent(transform.parent);
        imen.transform.position = Vector3.zero;
        imen.GetComponent<InSaveMenuBase>().Open(encmt);
    }

    void Start()
    {
        controlMode = ControlMode.Intro;
        transform.position = new Vector3(0,400,0);
        BackgroundUpdate();
        selection = dottedTracker1 = 0;
        scrollDist = centerDist * Mathf.PI;
        encmt = GetComponent<Encontrolmentation>();
        bool inSetup = Setup();
        if (!inSetup && Utilities.replayLevel && Door1.levelComplete)
        {
            welcomeBox.SetActive(false);
            replayStatsBox.SetActive(true);
        }
    }

    IEnumerator GetNextUIUpToSize(Transform nextUItr)
    {
        while (nextUItr.localScale.x < 0.99f)
        {
            nextUItr.localScale = Vector3.MoveTowards(nextUItr.localScale, Vector3.one, 0.066666f);
            float t = 1f - nextUItr.localScale.x;
            nextUItr.localEulerAngles = new Vector3(70*t, 220*t, -45*t);
            yield return new WaitForEndOfFrame();
        }
        nextUItr.localScale = Vector3.one;
        nextUItr.localEulerAngles = Vector3.zero;
        yield return null;
    }

    void ShowNextUI(bool replayDirectToLevel = false)
    {
        GameObject nextUI = nextUIs[selection];
        nextUI.SetActive(true);
        nextUI.transform.localScale = new Vector3(0.05f, 0.05f, 1);
        InSaveMenuBase menu = nextUI.GetComponent<InSaveMenuBase>();
        menu.Open(encmt);
        if (replayDirectToLevel)
        {
            (menu as MenuLevelSelect).replayDirectToLevel = true;
            menu.transform.localScale = Vector3.one;
            menu.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            StartCoroutine(GetNextUIUpToSize(nextUI.transform));
        }
    }

    void ShowQuitUI()
    {
        quitText.text = "Quit to title menu?";
        quitBox.toSaveMenu = false;
        quitBox.transform.position = Vector3.zero;
        quitBox.Open(encmt);
    }

    private void PassIntro()
    {
        controlMode = ControlMode.Cycle;
        transform.position = Vector3.zero;
        initSound.Stop(); initSound.Play();
        if (replayStatsBox.activeSelf)
        {
            // a level replay was finished, so go in the menu for that level automatically.
            selection = 1;
            ShowNextUI(true);
        }
        replayStatsBox.SetActive(false);
        InitWheel();
    }

    private long framesExisted = 0;

    void Update()
    {
        ++framesExisted;
        if (encmt && lastFrameAllowInput)
        {
            switch (controlMode)
            {
                case ControlMode.Intro:
                    if ((encmt.ButtonDown(32UL, 32UL) || encmt.ButtonDown(1024UL, 1024UL)) && framesExisted > 3) { ShowQuitUI(); break; }
                    if (encmt.flags > 0UL && framesExisted > 3) { PassIntro(); }
                    break;
                case ControlMode.Cycle:
                    if (encmt.ButtonDown(32UL, 32UL) || encmt.ButtonDown(1024UL, 1024UL)) { ShowQuitUI(); break; }
                    if (encmt.ButtonDown(4UL, 12UL) && selection > 0) {
                        changeSound.Stop();
                        changeSound.Play();
                        --selection;
                    }
                    if (encmt.ButtonDown(8UL, 12UL) && selection < optionDisplays.Length - 1) {
                        changeSound.Stop();
                        changeSound.Play();
                        ++selection;
                    }
                    if (encmt.ButtonDown(16UL, 16UL))
                    {
                        selectSound.Stop();
                        selectSound.Play();
                        ShowNextUI();
                    }
                    break;
                default:
                    break;
            }
        }

        switch (controlMode)
        {
            case ControlMode.Cycle:
                MoveWheel();
                break;
            default:
                break;
        }

        MoveMain();
        BackgroundUpdate();
        lastFrameAllowInput = encmt.allowUserInput;
    }

    private IEnumerator IconSelectListen(GameObject imen, Action Callback = null)
    {
        while (imen.activeSelf) { yield return new WaitForEndOfFrame(); }
        if (IconSelectController.result == -1) { yield break; }
        Utilities.toSaveData.icon = Utilities.loadedSaveData.icon = IconSelectController.result;
        Utilities.SaveGame(Utilities.currentSaveNumber);
        Callback?.Invoke();
        yield return null;
    }

    private IEnumerator KeyboardListen(GameObject kmen, Action Callback = null)
    {
        while (kmen.activeSelf) { yield return new WaitForEndOfFrame(); }
        if (KeyboardController.result == "") { yield break; }
        Utilities.toSaveData.name = Utilities.loadedSaveData.name = KeyboardController.result;
        Utilities.SaveGame(Utilities.currentSaveNumber);
        Callback?.Invoke();
        yield return null;
    }

    public void KeyboardOpenForNaming(string text, bool canClose = true, Encontrolmentation myControl = null, Action Callback = null)
    {
        if (myControl == null) { myControl = encmt; }
        KeyboardController.censor = true;
        KeyboardController.defaultText = text;
        KeyboardController.minLength = 3;
        KeyboardController.maxLength = 13;
        KeyboardController.canClose = canClose;
        KeyboardController.result = "";
        GameObject kmen = Instantiate(keyboardMenuPrefab);
        kmen.transform.SetParent(transform.parent);
        kmen.transform.position = Vector3.zero;
        kmen.GetComponent<KeyboardController>().Open(myControl);
        StartCoroutine(KeyboardListen(kmen, Callback));
    }

    public void IconSelectOpen(bool canClose = true, Encontrolmentation myControl = null, Action Callback = null)
    {
        if (myControl == null) { myControl = encmt; }
        IconSelectController.canClose = canClose;
        GameObject imen = Instantiate(iconMenuPrefab);
        imen.transform.SetParent(transform.parent);
        imen.transform.position = Vector3.zero;
        imen.GetComponent<IconSelectController>().Open(myControl);
        StartCoroutine(IconSelectListen(imen, Callback));
    }
}
