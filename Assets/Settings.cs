using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public static class Settings
{
    public static Dictionary<string, object> data = new Dictionary<string, object>()
    {
        {"volumeBGM", 65f},
        {"volumeObj", 90f},
        {"volumePlr", 90f},

        {"dialogSpeedMult", "4"},
        {"bloodAndViscera", 1}, // note: playerprefs doesn't like bool. 1 is true, and 0 is false now. if you don't like it, leave
        //{"levelStartScreen", 0},
        {"displayTimer", 0},
        {"bulletHitpointVisible", 0},
    };

    private static AudioMixer volBGMMixer = Resources.Load<AudioMixer>("AudioMixers/BGM");
    private static AudioMixer volObjMixer = Resources.Load<AudioMixer>("AudioMixers/ObjectSFX");
    private static AudioMixer volPlrMixer = Resources.Load<AudioMixer>("AudioMixers/PlayerSFX");

    private static bool everPrepared = false; // if yes, then settings have been loaded from PlayerPrefs and are accurate to the player's wishes

    private static float VolFunc(float x)
    {
        if (x < 1f) { return -80f; }
        return ((x / 1.27f)*0.7f + 30f) - 80f;
    }

    public static void UpdateSound()
    {
        volBGMMixer.SetFloat("Volume", VolFunc((float)data["volumeBGM"]));
        volObjMixer.SetFloat("Volume", VolFunc((float)data["volumeObj"]));
        volPlrMixer.SetFloat("Volume", VolFunc((float)data["volumePlr"]));
    }

    public static void UpdateUI()
    {
        Utilities.showTimer = ((int)data["displayTimer"] == 1);

        string spMul = (string)data["dialogSpeedMult"];
        if (spMul == "inf") { MainTextsStuff.speedMultiplier = 100000f; }
        else { MainTextsStuff.speedMultiplier = float.Parse(spMul)*0.25f; }

        ExpBar.levelUpBannerEnabled = false; // removed from demo
    }

    public static void PrepareAll(bool onlyIfNeverPrepared = false)
    {
        if (everPrepared && onlyIfNeverPrepared) { return; }
        List<KeyValuePair<string, object>> l = data.ToList();
        for (int i = 0; i < l.Count; ++i)
        {
            KeyValuePair<string, object> sv = l[i];
            if (PlayerPrefs.HasKey(sv.Key))
            {
                if (sv.Value is string) { data[sv.Key] = PlayerPrefs.GetString(sv.Key); }
                else if (sv.Value is int) { data[sv.Key] = PlayerPrefs.GetInt(sv.Key); }
                else if (sv.Value is float) { data[sv.Key] = PlayerPrefs.GetFloat(sv.Key); }
            }
        }
        everPrepared = true;
    }

    public static void UpdatePlayers()
    {
        KHealth.hitpointVisible = ((int)data["bulletHitpointVisible"] == 1);
    }

    public static bool ShowGore()
    {
        if (!everPrepared) { PrepareAll(); }
        return (int)data["bloodAndViscera"] == 1;
    }

    public static void UpdateAll()
    {
        UpdateSound();
        UpdateUI();
        UpdatePlayers();
    }
}
