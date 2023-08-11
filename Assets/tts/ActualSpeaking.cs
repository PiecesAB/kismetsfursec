using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.UI;

public class ActualSpeaking : MonoBehaviour {

    public float timeDifference;
    public List<string> sounds = new List<string>()
    {
        "uh", "ah", "aa", "eh", "schwa",
        "er", "ih", "ee", "au", "oo",
        "uu", "ai", "ou", "ei", "oh",
        "oi", "b",  "d",  "f",  "h",
        "g",  "j",  "k",  "l",  "m",
        "n",  "ng", "p",  "r",  "s",
        "sh", "t",  "tch","theta","thorn",
        "v",  "w",  "z",  "zh", "dzh",
        "-n", "-l",

    };

    public List<float> soundTimes = new List<float>()
    {
        0.14f, 0.14f, 0.14f, 0.12f, 0.075f,
        0.14f, 0.12f, 0.155f,0.13f, 0.115f,
        0.14f, 0.21f, 0.21f, 0.175f, 0.175f,
        0.2f, 0f, 0f, 0.02f, 0.02f,
        0f, 0.07f, 0f, 0.07f, 0.07f,
        0.07f, 0.07f, 0f, 0.05f, 0.02f,
        0.05f, 0f, 0.05f, 0.03f, 0.05f,
        0.05f, 0.07f, 0.05f, 0.05f, 0.05f,
        0.14f, 0.14f
    };

    // info that turns a box image into what it ought to sound like
    [System.Serializable]
    public class BoxToSoundInfo
    {
        public Sprite sprite;
        public AudioClip voice;
        public bool intelligible; // lol not really
    }

    // not a dictionary, but I don't care; the game doesn't have more than a hundred voices i'm sure
    public BoxToSoundInfo[] boxToSoundInfo;
    public float spaceTime;
    public float atlasitemDistance = 0.75f;
    public float stressEmphasis = 1f;
    public float speedMultiplier = 1f;
    public float speedMultiplier2 = 1f;
    public float soundOverlap = 0.055f;
    public string message;
    public AudioSource consonantMan;
    public AudioSource vowelMan;
    public bool autoSpeak = false;
    public bool skipping = false;
    [HideInInspector]
    public string emotion = "";
    [HideInInspector]
    public string currentPhoneme = "";
    [HideInInspector]
    public List<HumanMouthSync> syncedMouths = new List<HumanMouthSync>();

    public static string[] thousandNames = new string[]
    {
        "","thousand*","million*","billion*","trillion*","quadrillion*","quintillion*" //a long doesn't go any farther
    };

    public static string[] oneNames = new string[]
    {
        "","one","two","three","four","five","six","seven","eight","nine"
    };

    public static string[] tenNames = new string[]
    {
        "","ten","twenty","thirty","forty","fifty","sixty","seventy","eighty","ninety"
    };

    public static string[] teenNames = new string[]
    {
        "","eleven","twelve","thirteen","fourteen","fifteen","sixteen","seventeen","eighteen","nineteen"
    };

    public const string hundredName = "hundred"; //is this really necessary?
    public const string zeroName = "zero";
    public const string negativeName = "negative";

    public static string LongIntToWord(long num, int thousandsPlace = 0)
    {
        StringBuilder s = new StringBuilder(64);
        if (num > 0)
        {
            int a0 = (int)(num % 1000L);
            int a1 = (int)(num % 100L);

            if (num >= 1000)
            {
                s.Append(LongIntToWord(num / 1000L, thousandsPlace + 1));
                if (s.Length < 1 || s[s.Length - 1] != '*')
                {
                    s.Append(' ');
                    s.Append(thousandNames[thousandsPlace + 1]);
                }
                s.Append(' ');
            }

            if (a0 >= 100)
            {
                int b0 = (int)((num % 1000L) / 100L);
                s.Append(oneNames[b0]);
                s.Append(' ');
                s.Append(hundredName);
                s.Append(' ');
            }

            if (a1 == 0)
            {
                //do nothing :>
            }
            else if (a1 < 10)
            {
                s.Append(oneNames[a1]);
                s.Append(' ');
            }
            else if (a1 >= 11 && a1 <= 19)
            {
                s.Append(teenNames[a1-10]);
                s.Append(' ');
            }
            else if (a1 % 10 == 0)
            {
                s.Append(tenNames[a1/10]);
                s.Append(' ');
            }
            else
            {
                s.Append(tenNames[a1 / 10]);
                s.Append('-');
                s.Append(oneNames[a1 % 10]);
                s.Append(' ');
            }
            return s.ToString().TrimEnd(' ');
        }
        else if (num < 0)
        {
            return negativeName + " " + LongIntToWord(-num);
        }
        return zeroName;
    }

    public static string[] rankNames = new string[]
    {
        "nullity",
        "one", "two", "three",
        "omega", "omega plus", "omega plus two", "omega plus three",
        "double omega", "treble omega", "quadruple omega",
        "quadratic omega", "cubic omega", "quartic omega",
        "super omega", "turbo omega", "ultra omega",
        "epsilon zero", "epsilon one", "epsilon two",
        "epsilon omega", "epsilon super omega", "epsilon turbo omega",
        "super epsilon", "turbo epsilon", "ultra epsilon",
        "zeta zero", "zeta one", "zeta two",
        "super zeta", "turbo zeta", "ultra zeta",
        "eta zero", "super eta", "turbo eta",
        "fixed point four", "fixed point omega", "super fixed point omega",
        "feferman", "ackermann",
        "theta of cubic true omega", "theta quartic true omega",
        "small veblen", "large veblen",
        "theta turbo true omega", "bachmann", 
        "psi true omega number omega",
        "psi epsilon true omega number omega plus",
        "super psi number mahlo zero",
        "true omega"
    };

    public static void RankCharToWord(ref string s)
    {
        int y = 0;
        try { y = Convert.ToInt32(s[0]); }
        catch { }
        y -= 0xE000;
        if (y >= 0 && y < rankNames.Length)
        {
            s = rankNames[y];
        }
    }

    public static void FormatToSay(ref string msg)
    {
        string[] words = msg.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            RankCharToWord(ref words[i]);
            try
            {
                words[i] = LongIntToWord(Convert.ToInt64(words[i]));
            }
            catch
            {
                //lol
            }
            // replace remaining numbers with digit words
            words[i] = words[i].Replace("0", " zero ");
            for (int k = 1; k <= 9; ++k)
            {
                words[i] = words[i].Replace(k.ToString(), " " + oneNames[k] + " ");
            }
        }
        msg = string.Join(" ", words);
    }

    // only say one thing at once
    private int sayCounter = int.MinValue;

    private IEnumerator Say(string msg, bool testRun)
    {
        int mySayCounter = ++sayCounter;
        FormatToSay(ref msg);
        List<int> stresses;
        List<string> phonemes = orthography.main(msg, true, out stresses);
        if (testRun) { yield break; }

        for (int i = 0; i < phonemes.Count; i++)
        {
            if (phonemes[i] == "e-wild")
            {
                phonemes[i] = "eh";
            }
            if (phonemes[i] == "o-wild")
            {
                phonemes[i] = "schwa";
            }
            if (phonemes[i] == "a-wild")
            {
                phonemes[i] = "schwa";
            }
            if (phonemes[i] == "y-wild")
            {
                phonemes[i] = "ee";
            }
            if (phonemes[i] == "u-wild")
            {
                phonemes[i] = "uh";
            }
            if (phonemes[i] == "-r")
            {
                phonemes[i] = "r";
            }
            if (phonemes[i] == "er-wild")
            {
                phonemes[i] = "er";
            }

            if (phonemes[i] == "_")
            {
                if (phonemes.Count > i+2)
                {
                if (phonemes[i+1] == "aa" && phonemes[i+2] == "_") // A bug
                {
                    phonemes[i + 1] = "ei";
                }
                }
            }

            if (phonemes[i] == "silent-e")
            {
                phonemes.RemoveAt(i);
                i--;
            }
        }
        int vowelt = 0;

        for (int i = 0; i < phonemes.Count; i++)
        {
            if (sayCounter != mySayCounter) { yield break; }
            float qq = 1f;
            vowelMan.pitch = 0.94f + (0.12f * Mathf.PerlinNoise(-(float)DoubleTime.UnscaledTimeRunning * 2f, (float)DoubleTime.UnscaledTimeRunning * 2f));
            if (emotion == "angry") { vowelMan.pitch -= 0.15f; }
            else if (emotion == "concerned") { vowelMan.pitch -= 0.08f; }
            else if (emotion == "happy") { vowelMan.pitch += 0.08f; }
            if (vowelt < stresses.Count)
            {
                if (stresses[vowelt] != int.MinValue)
                {
                    int xx = Mathf.Clamp(stresses[vowelt], -4, 4);
                    vowelMan.pitch += 0.15f * (xx / 4f) * stressEmphasis;
                    qq = 0.95f + 0.3f * (xx / 4f);
                }
                else
                {
                    int xx = 4;
                    vowelMan.pitch += 0.15f * (xx / 4f) * stressEmphasis;
                    qq = 0.95f + 0.3f * (xx / 4f);
                }
            }
            int dex = sounds.IndexOf(phonemes[i]);
            if (dex <= 15 && dex >= 0) //vowel
            {
                vowelMan.time = Mathf.Repeat(((float)dex) * atlasitemDistance, vowelMan.clip.length);
                vowelMan.Play();
                vowelMan.SetScheduledEndTime(AudioSettings.dspTime+soundTimes[dex]);
                currentPhoneme = phonemes[i];
                yield return new WaitForSecondsRealtime((soundTimes[dex]/speedMultiplier - soundOverlap + 0.04f)*qq* vowelMan.pitch * (skipping ? 0.1f : 1f) / speedMultiplier2);
                if (sayCounter != mySayCounter) { yield break; }
                vowelt++;
            }
            else if(dex == -1) //other
            {
                if (phonemes[i] == "_") //space
                {
                    currentPhoneme = "";
                    yield return new WaitForSecondsRealtime(spaceTime * (skipping ? 0.1f : 1f) / speedMultiplier2);
                    if (sayCounter != mySayCounter) { yield break; }
                }
            }
            else //consonant
            {
                float lol = 1f;
                float man = 0f;
                if (phonemes.Count > i + 1)
                {
                    if (sounds.IndexOf(phonemes[i + 1]) <= 15 && sounds.IndexOf(phonemes[i + 1]) >= 0)
                    {
                        lol *= 1.75f;
                    }
                    else
                    {
                        man += soundTimes[dex];
                    }
                }
                consonantMan.time = Mathf.Repeat(((float)dex) * atlasitemDistance, consonantMan.clip.length);
                consonantMan.Play();
                consonantMan.SetScheduledEndTime((AudioSettings.dspTime + (soundTimes[dex] * lol* 2f / speedMultiplier) + 0.03f + man) * (skipping ? 0.1f : 1f));
                currentPhoneme = phonemes[i];
                yield return new WaitForSecondsRealtime((soundTimes[dex]*lol / speedMultiplier - soundOverlap + man) * (skipping ? 0.1f : 1f) / speedMultiplier2);
                if (sayCounter != mySayCounter) { yield break; }
            }
        }

        currentPhoneme = "";
        foreach (HumanMouthSync ms in syncedMouths) { if (ms.mode == HumanMouthSync.Mode.TTS) { ms.Deactivate(); } }

        yield return null;
    }

    private bool currentVoiceIsIntelligible = false;

    public void SetVoiceFromBoxImage(Image im)
    {
        Sprite sp = im.sprite;
        foreach (BoxToSoundInfo si in boxToSoundInfo)
        {
            if (si.sprite == sp)
            {
                consonantMan.clip = vowelMan.clip = si.voice;
                currentVoiceIsIntelligible = si.intelligible;
            }
        }
    }

    public void SetVoice(AudioClip clip, bool intelligible)
    {
        consonantMan.clip = vowelMan.clip = clip;
        currentVoiceIsIntelligible = intelligible;
    }

    public bool CurrentVoiceIsIntelligible()
    {
        return currentVoiceIsIntelligible;
    }

    public void Say(string msg)
    {
        StartCoroutine(Say(msg, false));
    }


    // Use this for initialization
    void Start () {
        //print(LongIntToWord(-405060201040));
        if (autoSpeak)
        {
            Say(message);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
