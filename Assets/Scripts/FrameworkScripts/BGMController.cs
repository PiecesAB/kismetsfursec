using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BGMController : MonoBehaviour {

    [Range(0.5f, 2.0f)]
    public float speed;
    [Range(0.5f, 2.0f)]
    public float pitch;
    public AudioMixer bgmixer;
    public AudioClip nextMusic;
    public bool nextMusicLoops;
    public float nextMusicStartTime = 0f;
    public AudioClip nextMusicOnAlternateSpawn = null;
    [Range(0.3f, 20.0f)]
    public float fadeOutSpeed;
    [Range(0.3f, 20.0f)]
    public float fadeInSpeed;
    private float duckOutSpeed = 0f;
    private float duckMultiplier;
    private int duckHoldFrames = 0;

    public bool pitchVolumeCorrection;
    public bool autoScalePitchBySpeed;
    public bool regularPitchScaling;
    public bool mustFollowLevel1MinTimer;

    private AudioSource crossfadeSource;
    private float crossfadeTimeAtDeletion;

    public bool pauseMusicDuringPauseMenu;

    private float semitone = Mathf.Pow(2f, 0.08333333f);

    public float myVol;
    public bool autoSetVolumeOnStart;

    public BasicMove mainPlr;

    public AudioLowPassFilter lowpass1;

    public static BGMController main;

    private float lastTime;
    private Vector2 myRange = new Vector2(0f, Mathf.Infinity);
    private static Vector2 defaultRange = new Vector2(0f, Mathf.Infinity);
    private static Dictionary<string, Vector2> looptimes = new Dictionary<string, Vector2>()
    {
        {"absurd abduction", new Vector2(0.7207207f, 52.612612f) },
        {"fantasia on drowning puppies", new Vector2(36f, 298f) },
        {"insanity defense", new Vector2(4.5652174f, 77.608696f) },
        {"remember when you died", new Vector2(3.4285714f, 72.5714285f) },
        {"the perfect family", new Vector2(1.7647059f, 107.647059f) },
        {"triakulus", new Vector2(2.5f, 167.5f) },
        {"wagtail nocturne", new Vector2(21.1764706f, 80.4705879f) },
        {"worthless waltz", new Vector2(28.91f, 235.56f) },
        {"demonic ascension ii", new Vector2(17.007874f, 198.4252f) },
        {"karyotype", new Vector2(22.37288f, 100.67797f) },
        {"worthless wargames", new Vector2(29.42307f, 217.5f) },
    };

    [HideInInspector]
    public AudioSource aso;

    private bool paused;

    void LoopRangeSet()
    {
        if (!aso.clip) { return; }
        if (aso && looptimes.ContainsKey(aso.clip.name))
        {
            myRange = looptimes[aso.clip.name];
        }
        else
        {
            myRange = defaultRange;
        }
    }

    private BasicMove FindPlr()
    {
        if (LevelInfoContainer.allPlayersInLevel == null || LevelInfoContainer.allPlayersInLevel.Count == 0)
        {
            return null;
        }
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (e == null) { return null; }
        return e.GetComponent<BasicMove>();
    }

    void Start () {
        if (fadeOutSpeed >= 19.9f) { fadeOutSpeed = 10000f; }
        if (fadeInSpeed >= 19.9f) { fadeInSpeed = 10000f; }
        if (fadeInSpeed < 0.3f) { fadeInSpeed = 0.3f; } // ???
        if (fadeOutSpeed < 0.3f) { fadeOutSpeed = 0.3f; } // ???

        aso = GetComponent<AudioSource>(); //never null. IT BETTER NOT BE NULL
        lastTime = -1f;
        //mainPlr = FindPlr();
        lowpass1 = Camera.main?.GetComponent<AudioLowPassFilter>();
        if (lowpass1) { lowpass1.cutoffFrequency = 4500f; }
        if (autoSetVolumeOnStart)
        {
            myVol = aso.volume;
        }

        //deal with singleton
        bool altSpawn = Utilities.GetLevelInInfo() != "";
        if (main != null)
        {
            AudioClip musComp = (altSpawn && nextMusicOnAlternateSpawn) ? nextMusicOnAlternateSpawn : nextMusic;
            if (musComp != main.aso?.clip || (main.nextMusic && musComp != main.nextMusic))
            {
                main.nextMusic = musComp;
                main.nextMusicLoops = nextMusicLoops;
                main.nextMusicStartTime = nextMusicStartTime;
            }
            main.nextMusicOnAlternateSpawn = nextMusicOnAlternateSpawn;
            main.pauseMusicDuringPauseMenu = pauseMusicDuringPauseMenu;
            main.mustFollowLevel1MinTimer = mustFollowLevel1MinTimer; // hopefully always false
            main.fadeOutSpeed = fadeOutSpeed;
            main.fadeInSpeed = fadeInSpeed;
            Destroy(this);
            return;
        }
        main = this;

        if (aso.clip == null)
        {
            aso.clip = nextMusic;
            aso.loop = nextMusicLoops;
            aso.volume = 0f;
            nextMusic = null;
            aso.Pause();
            if (nextMusicStartTime > 0) { aso.time = nextMusicStartTime; }
            else { aso.time = 0f; }
            lastTime = aso.time;
            aso.Play();
            LoopRangeSet();
        }

        /*if (transform.parent && transform.parent.parent)
        {
            transform.SetParent(transform.parent.parent);
        }*/
        transform.parent = null;
        DontDestroyOnLoad(gameObject);

        duckMultiplier = 1f;

        paused = false;

        Settings.UpdateAll();
    }

    public void SetMusicTime(float time)
    {
        if (aso.clip.length <= time) { aso.time = 0; return; }
        aso.time = time;
    }

    public void InstantMusicChange(AudioClip nm, bool loop, float optionalTime = 0f, bool timed = false)
    {
        if (nm == aso.clip) { return; }
        aso.clip = nm;
        aso.loop = loop;
        aso.volume = myVol*duckMultiplier;
        nextMusic = null;
        aso.Pause();
        if (optionalTime > 0 && aso.clip.length > optionalTime) { aso.time = optionalTime; }
        else { aso.time = 0f; }
        lastTime = aso.time;
        aso.Play();
        LoopRangeSet();
        mustFollowLevel1MinTimer = timed;
    }

    private static Dictionary<string, int> crossfadePairs = new Dictionary<string, int>()
    {
        {"fenrirs dance a", 2},
        {"fenrirs dance b", 2},
    };

    private bool ValidCrossfadePair(AudioClip a, AudioClip b)
    {
        int ia = crossfadePairs.ContainsKey(a?.name ?? "") ? crossfadePairs[a.name] : -1;
        int ib = crossfadePairs.ContainsKey(b?.name ?? "") ? crossfadePairs[b.name] : -1;
        return ia == ib && ia != -1;
    }

    private void CreateOrSetCrossfadeSource()
    {
        if (!ValidCrossfadePair(aso.clip, nextMusic)) { return; }
        if (!crossfadeSource)
        {
            GameObject g = new GameObject();
            g.name = "CrossfadeTrack";
            g.transform.SetParent(transform);
            crossfadeSource = g.AddComponent<AudioSource>();
            crossfadeSource.spatialBlend = aso.spatialBlend;
            crossfadeSource.priority = aso.priority;
            crossfadeSource.mute = false;
            crossfadeSource.bypassEffects = false;
            crossfadeSource.bypassReverbZones = false;
            crossfadeSource.reverbZoneMix = aso.reverbZoneMix;
            crossfadeSource.rolloffMode = aso.rolloffMode;
            crossfadeSource.minDistance = aso.minDistance;
            crossfadeSource.maxDistance = aso.maxDistance;
            crossfadeSource.dopplerLevel = aso.dopplerLevel;
            crossfadeSource.spread = aso.spread;
            crossfadeSource.clip = nextMusic;
            crossfadeSource.outputAudioMixerGroup = aso.outputAudioMixerGroup;
            crossfadeSource.loop = nextMusicLoops;
            crossfadeSource.Play();
            if (nextMusic.length <= aso.time)
            {
                crossfadeSource.time = 0f;
            }
        }
        crossfadeSource.volume = adjustedVol - aso.volume;
        if (Mathf.Abs(crossfadeSource.time - aso.time) > 0.02f && nextMusic.length > aso.time)
        {
            crossfadeSource.time = aso.time;
        }
        crossfadeSource.pitch = aso.pitch;
    }

    private bool DeleteCrossfadeSource()
    {
        if (crossfadeSource)
        {
            crossfadeTimeAtDeletion = crossfadeSource.time;
            Destroy(crossfadeSource.gameObject);
            crossfadeSource = null;
            return true;
        }
        return false;
    }

    public void DuckOutAtSpeed(float speed)
    {
        DuckOutAtSpeedForFrames(speed, 1);
    }

    public void DuckOutAtSpeedForFrames(float speed, int frames)
    {
        if (speed > duckOutSpeed && duckHoldFrames <= frames)
        {
            duckOutSpeed = speed;
            duckHoldFrames = frames;
        }
    }

    float adjustedVol = 0f;

    void Update () {
        float timeScaleMultiplier = (Time.timeScale == 0f && LevelInfoContainer.main && !(FollowThePlayer.main?.scrollingIndicator ?? true)) ? 0.35f : 1f;
        adjustedVol = myVol * duckMultiplier * timeScaleMultiplier;

        mainPlr = FindPlr();
        if (nextMusic != null)
        {
            if (aso.volume > 0f)
            {
                CreateOrSetCrossfadeSource();
                aso.volume = Mathf.Clamp01(aso.volume - (fadeOutSpeed * 0.0166666f * myVol));
                if ((aso.clip?.name ?? "nothing") == "nothing") { aso.volume = 0f; }
            }
            else
            {
                aso.clip = nextMusic;
                aso.loop = nextMusicLoops;
                aso.volume = 0f;
                nextMusic = null;
                aso.Pause();
                if (nextMusicStartTime > 0 && aso.clip.length > nextMusicStartTime) { aso.time = nextMusicStartTime; }
                else { aso.time = 0f; }
                if (DeleteCrossfadeSource())
                {
                    aso.volume = adjustedVol;
                    aso.time = crossfadeTimeAtDeletion;
                }
                lastTime = aso.time;
                aso.Play();
                LoopRangeSet();
            }
        }
        else
        {
            if (nextMusic == aso.clip)
            {
                nextMusic = null;
            }

            if (aso.volume < adjustedVol)
            {
                aso.volume = Mathf.Clamp(aso.volume + (fadeInSpeed * 0.0166666f * myVol), 0f, adjustedVol);
            }
            else
            {
                aso.volume = adjustedVol;
            }
        }

        if (pitchVolumeCorrection && System.Math.Abs(speed - 1f) > 0.001f)
        {
            //???
        }

        if (autoScalePitchBySpeed)
        {
            float o = speed;
            float n = speed;
            int moves = 0;
            pitch = 1f;
            while (System.Math.Abs(n-1f) >= 0.1f && moves < 12)
            {
                n = Mathf.MoveTowards(n, 1f, 0.0999998f);
                if (o < n)
                {
                    pitch /= semitone;
                }
                else
                {
                    pitch *= semitone;
                }
                o = n;
                moves++;
            }
        }

        bgmixer.SetFloat("PitchM", speed);
        if (!regularPitchScaling)
        {
            bgmixer.SetFloat("PitchS", pitch / speed);
        }

        if (mainPlr) //do plr things
        {
            if (lowpass1)
            {
                lowpass1.enabled = (mainPlr.swimCount > 0 && !mainPlr.ghost);
            }
        }
        else
        {
            if (lowpass1)
            {
                lowpass1.enabled = false;
            }
            else
            {
                if (Camera.main && Camera.main.GetComponent<AudioLowPassFilter>())
                {
                    lowpass1 = Camera.main.GetComponent<AudioLowPassFilter>();
                }
            }
        }

        if (aso.loop)
        {
            if (aso.pitch >= 0)
            {
                if (aso.time < lastTime || aso.time > myRange.y)
                {
                    aso.time = myRange.x;
                }
            }
        }

        if (mustFollowLevel1MinTimer)
        {
            if (Time.timeScale == 0)
            {
                aso.time = 61f;
            }
            else
            {
                float cTime = 60f - LevelInfoContainer.timer;
                if (cTime < 60f)
                {
                    if (System.Math.Abs(aso.time - cTime) > 0.02f)
                    {
                        aso.time = cTime;
                    }
                }

            }
        }

        if (pauseMusicDuringPauseMenu)
        {
            bool newPaused = (Time.timeScale == 0);
            if (newPaused != paused)
            {
                if (newPaused)
                {
                    aso.Pause();
                }
                else
                {
                    aso.UnPause();
                }

                paused = newPaused;
            }
        }

        // duckOutSpeed must be set every frame unless duckHoldFrames keeps it for longer; this is handled internally to BGMController
        if (duckOutSpeed > 0f) { duckMultiplier -= duckOutSpeed / 60f; }
        else { duckMultiplier += 1f / 60f; }
        if (duckHoldFrames > 0)
        {
            --duckHoldFrames;
            if (duckHoldFrames == 0)
            {
                duckHoldFrames = 0;
                duckOutSpeed = 0f;
            }
        }
        

        duckMultiplier = Mathf.Clamp01(duckMultiplier);

        lastTime = aso.time;
    }
}
