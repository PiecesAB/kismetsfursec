using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplaySymbolAndTimer : MonoBehaviour
{
    public Sprite[] difficultySprites = new Sprite[3];
    public Text timerText;

    private double lastTime;
    
    void Start()
    {
        if (Utilities.replayLevel)
        {
            Image thisSprite = GetComponent<Image>();
            thisSprite.sprite = difficultySprites[Utilities.replayMode];
            //transform.GetChild(0).GetComponent<Image>().enabled = true;
        }
        else
        {
            GetComponent<Image>().enabled = false;
        }

        if (Utilities.showTimer)
        {
            timerText.enabled = true;
        }
    }

    void Update()
    {
        timerText.enabled = Utilities.showTimer;

        if (Utilities.showTimer && !KHealth.someoneDied && !Door1.levelComplete)
        {
            if (Utilities.replayLevel)
            {
                timerText.text = Utilities.GetFormattedPreciseTime(DoubleTime.UnscaledTimeSinceLoad);
            }
            else
            {
                timerText.text = Utilities.GetFormattedPreciseTime(DoubleTime.UnscaledTimeSinceLoad + Utilities.toSaveData.gameTimePlayed);
            }
        }
        
    }
}
