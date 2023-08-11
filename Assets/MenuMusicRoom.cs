using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMusicRoom : SecondaryTitleMenu
{
    [System.Serializable]
    public struct MusicInfo
    {
        public string name;
        public AudioClip clip;
        [Multiline]
        public string description;
    }

    public AudioClip normalBackgroundMusic;
    private float oldBGMTime = 0;
    public MusicInfo[] musics;
    private int selection = 0;
    public GameObject scroller;
    public GameObject playerBox;
    public GameObject descBox;
    public RectTransform scrollerInset;
    public GameObject volumeUpMessage;
    public GameObject nameTagSample;
    public Color nameTagActiveColor;
    public Color playerBoxPlayingColor;
    private GameObject[] nameTags;
    private Image[] nameTagImages;
    public Text description;
    private Color nameTagNormalColor;
    private Color playerBoxNormalColor;
    public Text timerNumbers;
    public RectTransform timerBar;
    public RectTransform seekCircle;
    private float timerBarFullWidth;

    public AudioSource changeSound;
    public AudioSource playSound;
    public AudioSource seekSound;

    private int holdScrollCooldown = 0;
    private int oldSelection = 0;
    private bool volumeIncreased = false;
    private AudioSource aso;
    private bool seeking = false;
    private Image seekCircleImage;

    protected override void ChildOpen()
    {
        open = true;
        aso = GetComponent<AudioSource>();
        aso.Stop();
        aso.loop = true;
        aso.time = 0;
        seeking = false;
        selection = 0;
        oldSelection = selection;
        oldBGMTime = BGMController.main.aso.time;
        BGMController.main.InstantMusicChange(null, false);
        volumeIncreased = false;
        if ((float)Settings.data["volumeBGM"] <= 0f)
        {
            scroller.SetActive(false);
            volumeUpMessage.SetActive(true);
            playerBox.SetActive(false);
            descBox.SetActive(false);
            return;
        }
        else if ((float)Settings.data["volumeBGM"] <= 80f)
        {
            Settings.data["volumeBGM"] = (float)Settings.data["volumeBGM"] + 10f; // temporary increase
            Settings.UpdateSound();
            volumeIncreased = true;
        }
        scroller.SetActive(true);
        scrollerInset.localPosition = Vector3.zero;
        volumeUpMessage.SetActive(false);
        playerBox.SetActive(true);
        descBox.SetActive(true);
        nameTagSample.SetActive(true);
        timerBar.gameObject.SetActive(true);
        seekCircleImage = seekCircle.GetComponent<Image>();
        seekCircleImage.color = Color.clear;
        nameTagNormalColor = nameTagSample.GetComponentInChildren<Image>().color;
        playerBoxNormalColor = playerBox.GetComponent<Image>().color;
        nameTags = new GameObject[musics.Length];
        nameTagImages = new Image[musics.Length];
        for (int i = 0; i < musics.Length; ++i)
        {
            GameObject nnt = Instantiate(nameTagSample, nameTagSample.transform.parent);
            nnt.GetComponentInChildren<Text>().text = musics[i].name;
            nameTags[i] = nnt;
            nameTagImages[i] = nnt.GetComponentInChildren<Image>();
        }
        nameTagSample.SetActive(false);
        description.text = musics[selection].description;
        timerBarFullWidth = timerBar.parent.GetComponent<RectTransform>().sizeDelta.x;
        aso.clip = musics[selection].clip;
    }

    private string SecondsToTimeDisplay(int x)
    {
        return (x / 60) + ":" + (x % 60 < 10 ? "0" : "") + (x % 60);
    }

    private string SecondsToTimeDisplay(float x)
    {
        return SecondsToTimeDisplay(Mathf.FloorToInt(x));
    }

    private void StopSeeking()
    {
        seeking = false;
        seekCircleImage.color = Color.clear;
        timerBar.gameObject.SetActive(true);
    }

    protected override void ChildUpdate()
    {
        if (!scroller.activeSelf) { return; }

        bool upHold = myControl.ButtonHeld(4UL, 12UL, 0.45f, out _);
        bool downHold = myControl.ButtonHeld(8UL, 12UL, 0.45f, out _);
        if (myControl.ButtonDown(4UL, 12UL) || (upHold && holdScrollCooldown <= 0))
        {
            if (upHold) { holdScrollCooldown = 10; }
            if (selection > 0) { --selection; }
        }
        else if (myControl.ButtonDown(8UL, 12UL) || (downHold && holdScrollCooldown <= 0))
        {
            if (downHold) { holdScrollCooldown = 10; }
            if (selection < musics.Length - 1) { ++selection; }
        }
        if (holdScrollCooldown > 0) { --holdScrollCooldown; }

        if (oldSelection != selection)
        {
            if (seeking) { StopSeeking(); }
            nameTagImages[oldSelection].color = nameTagNormalColor;
            changeSound.Stop(); changeSound.Play();
            aso.Stop();
            aso.time = 0;
            aso.clip = musics[selection].clip;
        }
        nameTagImages[selection].color = Color.Lerp(nameTagActiveColor, 1.25f * nameTagActiveColor, (0.5f * (float)System.Math.Sin(DoubleTime.UnscaledTimeSinceLoad * 2.09)) + 0.5f);
        scrollerInset.localPosition = Vector3.Lerp(scrollerInset.localPosition, Vector3.up * (Mathf.Max(selection - 1, 0) * 18 + 40f), 0.4f);
        description.text = musics[selection].description;

        if (myControl.ButtonDown(16UL, 16UL))
        {
            if (seeking) { StopSeeking(); }
            if (!aso.isPlaying) { aso.Play(); }
            else { aso.Pause(); }
            playSound.Stop(); playSound.Play();
        }

        float cl = aso.clip?.length ?? 0f;

        double holdTime = 0.0;
        bool l1Down = myControl.ButtonHeld(256UL, 256UL + 512UL, 0f, out holdTime);
        bool r1Down = myControl.ButtonHeld(512UL, 256UL + 512UL, 0f, out holdTime);
        if (l1Down || r1Down)
        {
            if (!seeking)
            {
                seekSound.Stop(); seekSound.Play();
                seeking = true;
                seekCircleImage.color = Color.white;
                if (aso.isPlaying) { aso.Pause(); }
                timerBar.gameObject.SetActive(false);
            }
            float newTime = aso.time;
            float v = 0.0166666f * 20f * ((float)holdTime + 0.2f);
            if (l1Down) { newTime -= v; }
            else if (r1Down) { newTime += v; }
            newTime = Mathf.Clamp(newTime, 0f, cl - 0.01f);
            aso.time = newTime;
        }

        seekCircle.anchoredPosition = new Vector2((aso.time / cl) * timerBarFullWidth, 0f);
        timerNumbers.text = SecondsToTimeDisplay(aso.time) + " / " + SecondsToTimeDisplay(cl);
        timerBar.sizeDelta = new Vector2((aso.time / cl) * timerBarFullWidth, timerBar.sizeDelta.y);
        timerBar.GetComponent<Image>().material.SetColor("_BC", Color.HSVToRGB((float)((DoubleTime.UnscaledTimeSinceLoad * 0.05f) % 1.0), 0.8f, 0.5f));
        playerBox.GetComponent<Image>().color = Color.Lerp(
            playerBox.GetComponent<Image>().color,
            aso.isPlaying ? playerBoxPlayingColor : playerBoxNormalColor,
            0.4f
        );

        oldSelection = selection;
    }

    protected override void ChildClose()
    {
        if (nameTags != null) { foreach (GameObject g in nameTags) { Destroy(g); } }
        if (volumeIncreased)
        {
            Settings.data["volumeBGM"] = (float)Settings.data["volumeBGM"] - 10f; // reverse temporary increase
            Settings.UpdateSound();
            volumeIncreased = false;
        }
        BGMController.main.InstantMusicChange(normalBackgroundMusic, true, oldBGMTime);
        aso.Stop();
        aso.time = 0f;
        playerBox.GetComponent<Image>().color = playerBoxNormalColor;
        open = false;
    }
}
