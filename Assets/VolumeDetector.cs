using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeDetector : MonoBehaviour
{
    public List<AudioSource> listeningAudios = new List<AudioSource>();
    public float totalVolume = 0f;
    public RectTransform bar;
    public Image barRenderer;
    public float barFullWidth = 168f;
    public float barFullVolume = 0.5f;
    public Text text;
    public ModifyOtherText textMod;

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        float totalVolume2 = 0f;
        foreach (AudioSource a in listeningAudios)
        {
            if (!a) { continue; }
            if (!a.clip) { continue; }
            if (!a.isPlaying) { continue; }

            float[] sampleData = new float[64];
            a.clip.GetData(sampleData, Mathf.Max(0, a.timeSamples - 64));
            float maxVol = 0f;
            for (int i = 0; i < 64; ++i)
            {
                maxVol = Mathf.Max(Mathf.Abs(sampleData[i]), maxVol);
            }
            totalVolume2 += maxVol * a.volume;
        }
        float lastTotalVolume = totalVolume;
        totalVolume = Mathf.Lerp(totalVolume, totalVolume2, 0.2f);
        if (lastTotalVolume - totalVolume > 0.01f)
        {
            totalVolume = lastTotalVolume - 0.01f;
        }
        if (totalVolume / barFullVolume > 1.85f)
        {
            totalVolume = barFullVolume * 1.85f;
        }
        float rat = Mathf.Clamp01(totalVolume / barFullVolume);
        bar.sizeDelta = new Vector2(Mathf.Round(rat * barFullWidth / 2f) * 2f, bar.sizeDelta.y);
        barRenderer.material.SetColor("_BC", (rat == 1f) ? (Color)Fakerand.Color() : Color.HSVToRGB(0.7f - 0.7f * rat, 0.7f, 0.7f));
        text.color = (rat == 1f) ? (Color)Fakerand.Color() : Color.HSVToRGB(0.7f - 0.7f * rat, 0.3f, 0.9f);
        text.text = (rat > 0.6f) ? "BE QUIET!" : "plr volume";
        textMod.shake = (rat > 0.7f) ? 4f : 0f;

        BulletRankShot.rank = (rat > 0.6f) ? rat : 0f;
    }
}
