using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarChange : MonoBehaviour
{
    public Image bar;
    public Text secondsLeft;
    public Image[] minis;
    public Sprite[] miniSprites;
    public AudioSource lowTimeSiren;

    public static BossHealthBarChange main = null;

    private float lerpHealth = 0f;
    private int lastHealthBar = 0;

    public static bool CheckDisplay(BossController data)
    {
        return !data.defeated && data.useLargeHealthDisplay;
    }

    public static void TryUpdateBar(BossController data)
    {
        if (main != null ) { main.gameObject.SetActive(CheckDisplay(data)); main.UpdateBar(data); }
    }

    public void UpdateMinis(BossController data)
    {
        int barsLeft = data.barCount - data.currentBar - 1; //test
        for (int i = 0; i < minis.Length; ++i)
        {
            if (barsLeft == 0) { minis[i].sprite = miniSprites[0]; minis[i].color = Color.black; continue; }

            int rep = 1;
            if (barsLeft >= 100) { rep = 100; }
            else if (barsLeft >= 25) { rep = 25; }
            else if (barsLeft >= 10) { rep = 10; }
            else if (barsLeft >= 5) { rep = 5; }

            barsLeft -= rep;
            minis[i].color = Color.white;
            switch (rep)
            {
                case 1: minis[i].sprite = miniSprites[0]; break;
                case 5: minis[i].sprite = miniSprites[1]; break;
                case 10: minis[i].sprite = miniSprites[2]; break;
                case 25: minis[i].sprite = miniSprites[3]; break;
                case 100: minis[i].color = Fakerand.Color(); minis[i].sprite = miniSprites[4]; break;
                default: break;
            }
        }
    }

    public void UpdateBar(BossController data)
    {
        if (!CheckDisplay(data)) { return; }

        float rat = data.HealthRatio();
        float num = 1f - rat;

        if (num >= 0.55f && data.remainingTime >= 16f)
        {
            bar.material.SetColor("_BarColor", Color.HSVToRGB(0.333f, 0.7f, 0.8f));
        }
        else if (num >= 0.25f && data.remainingTime >= 11f)
        {
            bar.material.SetColor("_BarColor", Color.HSVToRGB(0.153f, 0.7f, 0.8f));
        }
        else if (data.remainingTime >= 6f)
        {
            bar.material.SetColor("_BarColor", Color.HSVToRGB(0f, 0.7f, 0.8f));
        }
        else
        {
            float t = 0.5f + 0.5f * (float)System.Math.Sin(DoubleTime.ScaledTimeSinceLoad * Mathf.PI * 2.0);
            bar.material.SetColor("_BarColor", Color.Lerp(Color.HSVToRGB(0f, 0.7f, 0.8f), Color.white, t));
        }

        int disp = Mathf.FloorToInt(data.remainingTime);
        secondsLeft.text = (data.timerNotYetStarted)?"":disp.ToString();

        if (disp >= 11) { secondsLeft.color = Color.white; }
        else if (disp >= 6) { secondsLeft.color = Color.yellow; }
        else { secondsLeft.color = new Color(1f, disp / 10f, 0); }

        float dangerHighlight = (data.currentBar >= data.barCount - 1) ? 1f : 0.91f;

        float v = Utilities.HealthDisplayScale(rat) * dangerHighlight;
        bar.material.SetFloat("_Val", v);
        lerpHealth = Mathf.MoveTowards(lerpHealth, v, 1f / 216f);
        bar.material.SetFloat("_LerpVal", lerpHealth);

        if (lastHealthBar != data.currentBar)
        {
            bar.material.SetFloat("_Val", 0f);
            bar.material.SetFloat("_LerpVal", 0f);
            lastHealthBar = data.currentBar;
            lerpHealth = 0f;
        }

        bar.material.SetFloat("_DangerHighlight", dangerHighlight);
        if (secondsLeft.text != "" && data.remainingTime <= 6f)
        {
            if (!lowTimeSiren.isPlaying) { lowTimeSiren.Play(); }
        }
        else
        {
            if (lowTimeSiren.isPlaying) { lowTimeSiren.Stop(); }
        }

        UpdateMinis(data);
    }

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        lastHealthBar = 0;
        lerpHealth = 0f;
        bar.material.SetFloat("_Val", 0f);
        bar.material.SetFloat("_LerpVal", 0f);
        bar.material.SetFloat("_DangerHighlight", 0.91f);
        bar.material.SetColor("_BarColor", Color.HSVToRGB(0.5f, 0.7f, 0.8f));
    }

    private bool verifiedBossLevel = false;

    private void LateUpdate()
    {
        if (!verifiedBossLevel)
        {
            if (BossController.main == null) { gameObject.SetActive(false); return; }
            verifiedBossLevel = true;
        }
    }
}
