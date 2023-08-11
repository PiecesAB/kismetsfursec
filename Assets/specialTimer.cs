using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class specialTimer : MonoBehaviour
{
    private static float[] intervals = { 0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 20f ,30f, 45f, 60f };

    public AudioSource beepSource;
    public AudioClip beep1;
    public AudioClip beep2;
    public AudioClip beepfinal;

    private int timeIntv;
    private float oldTime = Mathf.Infinity;
    private Text myText;

    void Start()
    {
        oldTime = Mathf.Infinity;
        timeIntv = intervals.Length - 1;
        myText = GetComponent<Text>();
        if (myText)
        {
            myText.color = Color.clear;
        }
    }

    void Update()
    {
        myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, myText.color.a - 0.1f);
        if (myText && LevelInfoContainer.timerOn)
        {
            mainCheck:

            if (timeIntv >= 0)
            {
                if (LevelInfoContainer.timer <= oldTime)
                {
                    float c = intervals[timeIntv];
                    bool disp = false;
                    while (LevelInfoContainer.timer <= c && oldTime > c)
                    {
                        timeIntv--;
                        if (timeIntv >= 0)
                        {
                            c = intervals[timeIntv];
                        }
                        else
                        {
                            disp = true;
                            break;
                        }
                        disp = true;
                    }
                    if (disp)
                    {
                        myText.text = ((int)intervals[timeIntv+1]).ToString();
                        myText.color = (timeIntv < 11) ? new Color(1f, (timeIntv+1f) / 11f, 0f) : Color.white;

                        if (beepSource)
                        {
                            beepSource.Stop();
                            if (timeIntv < 10)
                            {
                                beepSource.clip = beepfinal;
                            }
                            else if (timeIntv < 12)
                            {
                                beepSource.clip = beep2;
                            }
                            else
                            {
                                beepSource.clip = beep1;
                            }
                            beepSource.Play();
                        }
                    }
                }
                else
                {

                    oldTime = LevelInfoContainer.timer + 1f;
                    while (timeIntv < intervals.Length && intervals[timeIntv] < LevelInfoContainer.timer)
                    {
                        timeIntv = Mathf.Min(timeIntv+1, intervals.Length-1); 
                        if (timeIntv == intervals.Length - 1)
                        {
                            break;
                        }
                    }
                    goto mainCheck;
                }
            }
            else
            {
                if (LevelInfoContainer.timer > 0)
                {
                    timeIntv++;
                    goto mainCheck;
                }
            }
            oldTime = LevelInfoContainer.timer;
        }
        
    }
}
