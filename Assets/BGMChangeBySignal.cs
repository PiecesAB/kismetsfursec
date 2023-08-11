using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMChangeBySignal : MonoBehaviour
{
    public bool instantChange = true;
    public List<AudioClip> bgms;
    public void PopNextBGM()
    {
        if (instantChange)
        {
            BGMController.main.InstantMusicChange(bgms[0], true);
        }
        else
        {
            BGMController.main.nextMusic = bgms[0];
        }
        bgms.RemoveAt(0);
    }
}
