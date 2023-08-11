using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLightBlock : MonoBehaviour
{
    [Range(0, 31)]
    public int ID;
    private uint realID;

    public float totalTimeOn = 3;
    private float timeOn = 0;

    public AudioClip onSound;
    public AudioClip offSound;
    public AudioClip tickSound;

    [SerializeField]
    private Sprite offSprite;
    // 0 --> 7 : full on --> almost off
    [SerializeField]
    private Sprite[] onSprites;
    [SerializeField]
    private SpriteRenderer aura;

    private SpriteRenderer spr;
    private AudioSource aso;

    private int collidingCount = 0;

    private int cycleID = 0;

    private bool on = false;

    private static int[] switchTouchTrackers = null;
    private static AudioSource[] audibleBlocks = null;

    private bool CollisionValid(Collider2D col)
    {
        Rigidbody2D r2 = col.gameObject.GetComponent<Rigidbody2D>();
        return r2 && !r2.isKinematic;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!CollisionValid(col)) { return; }
        ++collidingCount;
        if (collidingCount == 1 && !KHealth.someoneDied && !Door1.levelComplete)
        {
            spr.sprite = onSprites[0];
            ++cycleID;
            spr.material.SetColor("_RepColor", Utilities.colorCycle[ID]);
            aura.material.SetColor("_Color", Utilities.colorCycle[ID]);
            if (!on) {
                ++switchTouchTrackers[ID];
                if (switchTouchTrackers[ID] == 1) { Utilities.ChangeSwitchRequest(realID); }
            }
            on = true;

            audibleBlocks[ID] = aso;
            aso.Stop();
            aso.clip = onSound;
            aso.Play();

            StartCoroutine(EnterContingencyHandler(cycleID));
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!CollisionValid(col)) { return; }
        --collidingCount;
        if (collidingCount == 0 && !KHealth.someoneDied && !Door1.levelComplete)
        {
            timeOn = totalTimeOn;
            ++cycleID;
            audibleBlocks[ID] = aso;
            StartCoroutine(ExitTimer(cycleID));
        }
    }

    private void TurnOffNow()
    {
        spr.sprite = offSprite;
        on = false;
        
        spr.material.SetColor("_RepColor", Utilities.colorCycle[ID]);
        aura.material.SetColor("_Color", Color.clear);

        if (audibleBlocks[ID] == aso)
        {
            aso.Stop();
            aso.clip = offSound;
            aso.Play();
        }
    }

    private IEnumerator EnterContingencyHandler(int currCycleID)
    {
        while (cycleID == currCycleID)
        {
            if (on && (KHealth.someoneDied || Door1.levelComplete))
            {
                --switchTouchTrackers[ID];
                if (switchTouchTrackers[ID] == 0) { Utilities.ChangeSwitchRequest(realID); }
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator ExitTimer(int currCycleID)
    {
        int lastFrame = 0;
        while (cycleID == currCycleID)
        {
            timeOn -= 0.0166666666f * Time.timeScale;
            if (timeOn <= 0 || KHealth.someoneDied || Door1.levelComplete)
            {
                --switchTouchTrackers[ID];
                if (switchTouchTrackers[ID] == 0) { Utilities.ChangeSwitchRequest(realID); }

                TurnOffNow();
                break;
            }
            else
            {
                float rat = timeOn / totalTimeOn;
                float sqrtrat = Mathf.Sqrt(rat);
                int frame = Mathf.Clamp((int)((0.95f - sqrtrat) * 8), 0, 7);
                spr.sprite = onSprites[frame];
                spr.material.SetColor("_RepColor", Color.Lerp(Utilities.colorCycle[ID], Color.white, (Mathf.Sin(timeOn * 12.56f) + 1) * 0.4f));

                if (lastFrame != frame)
                {
                    if (audibleBlocks[ID] == aso)
                    {
                        aso.Stop();
                        aso.clip = tickSound;
                        aso.Play();
                    }
                }

                lastFrame = frame;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private void Start()
    {
        aso = GetComponent<AudioSource>();
        spr = GetComponent<SpriteRenderer>();
        spr.sprite = offSprite;
        spr.material.SetColor("_RepColor", Utilities.colorCycle[ID]);
        aura.material.SetColor("_Color", Color.clear);
        realID = 1u << ID;
        if (switchTouchTrackers == null)
        {
            switchTouchTrackers = new int[32];
            for (int i = 0; i < 32; ++i) { switchTouchTrackers[i] = 0; }
        }
        if (audibleBlocks == null)
        {
            audibleBlocks = new AudioSource[32];
            for (int i = 0; i < 32; ++i) { audibleBlocks[i] = null; }
        }
    }


}
