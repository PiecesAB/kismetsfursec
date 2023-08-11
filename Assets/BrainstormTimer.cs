using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainstormTimer : MonoBehaviour
{
    public enum Display
    {
        Analog, Digital
    }

    public Display display = Display.Analog;
    private SpriteRenderer sr;
    [SerializeField]
    private Sprite[] digits;

    public AudioSource ticker;

    private float tickTime = 61f;

    private SpriteRenderer minutesDigit;
    private SpriteRenderer tenSecondsDigit;
    private SpriteRenderer secondsDigit;
    private SpriteRenderer colon;
    private SpriteRenderer endOfNumber;

    private bool timerStartedEver = false;

    private void UpdateDigitalDisplay(int i)
    {
        if (display != Display.Digital) { return; }
        if (i <= 0 || i >= 60)
        {
            if (timerStartedEver && i <= 0)
            {
                minutesDigit.color = tenSecondsDigit.color = secondsDigit.color = colon.color = Color.clear;
                endOfNumber.color = Color.red;
                return;
            }
            else { i = 60; }
        }
        endOfNumber.color = Color.clear;
        Color clockColor = Color.HSVToRGB(i / 180f, 0.8f, 0.8f);
        minutesDigit.sprite = digits[i / 60];
        tenSecondsDigit.sprite = digits[(i % 60) / 10];
        secondsDigit.sprite = digits[i % 10];
        minutesDigit.color = tenSecondsDigit.color = secondsDigit.color = colon.color = clockColor;
    }

    void Start()
    {
        if (display == Display.Analog)
        {
            sr = GetComponent<SpriteRenderer>();
            sr.material.SetFloat("_Seconds", 60f);
            sr.material.SetColor("_ClockColor", Color.gray);
        }
        else
        {
            minutesDigit = transform.Find("Minutes").GetComponent<SpriteRenderer>();
            tenSecondsDigit = transform.Find("10Seconds").GetComponent<SpriteRenderer>();
            secondsDigit = transform.Find("Seconds").GetComponent<SpriteRenderer>();
            colon = transform.Find("Colon").GetComponent<SpriteRenderer>();
            endOfNumber = transform.Find("EndOfNumber").GetComponent<SpriteRenderer>();
            UpdateDigitalDisplay(60);
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (!LevelInfoContainer.timerOn) { UpdateDigitalDisplay(0); return; }
        timerStartedEver = true;

        float f = LevelInfoContainer.timer % 1f;
        float i = LevelInfoContainer.timer - f;
        float f2 = EasingOfAccess.BounceOut(f, 0.96f);

        float tickSpace = (LevelInfoContainer.timer > 30f) ? 1f : ((LevelInfoContainer.timer > 10f) ? 0.5f : 0.25f);
        float tickVol = (LevelInfoContainer.timer > 30f) ? 0.1f : ((LevelInfoContainer.timer > 10f) ? 0.2f : 0.4f);
        while (tickTime - LevelInfoContainer.timer >= tickSpace)
        {
            tickTime -= tickSpace;
            ticker.Stop();
            ticker.volume = tickVol;
            ticker.Play();
        }

        if (display == Display.Analog)
        {
            Color clockColor = Color.HSVToRGB(LevelInfoContainer.timer / 120f, 0.8f, 0.8f);
            if (LevelInfoContainer.timer < 10f)
            {
                clockColor += 0.5f * Fakerand.Single() * Color.white;
            }
            sr.material.SetFloat("_Seconds", i + f2);
            sr.material.SetColor("_ClockColor", clockColor);
        }
        else
        {
            UpdateDigitalDisplay(LevelInfoContainer.timer == 0 ? 0 : (int)i + 1);
        }
    }
}
