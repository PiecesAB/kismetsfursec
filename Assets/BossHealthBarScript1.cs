using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;




[System.Obsolete("Old boss health script", true)]
public class BossHealthBarScript1 : MonoBehaviour {

    /*public float howMuchHpInAHealthBar;
    public BossStats bossStatsZeug;
    [Header("This goes in the back bar, so no need to specify that!")]
    public Image damageBar;
    public Image frontBossBar;

    public float MaxHpChangePerUpdate;

    private int oldNumberOfBarsLeft;

    [Serializable]
    public class HealthBarsData
    {
        public Color color;
        public Sprite pattern;
    }

    public HealthBarsData[] healthBars;

    // Use this for initialization
    void Start () {
        int numBarsLeft = (int)Mathf.Floor(bossStatsZeug.HP / howMuchHpInAHealthBar);
        oldNumberOfBarsLeft = numBarsLeft;
    }
	
	// Update is called once per frame
	void Update () {
        int numBarsLeft = (int)Mathf.Floor(bossStatsZeug.HP / howMuchHpInAHealthBar);
        float currBarPosition = (bossStatsZeug.HP % howMuchHpInAHealthBar) / (howMuchHpInAHealthBar/600);

        //allahu backbar
        if (numBarsLeft >= 1)
        {
            GetComponent<Image>().color = Color.Lerp(healthBars[numBarsLeft-1].color,Color.black,0.5f);
            GetComponent<Image>().sprite = healthBars[numBarsLeft - 1].pattern;
        }
        else
        {
            GetComponent<Image>().color = Color.black;
            GetComponent<Image>().sprite = healthBars[0].pattern;
        }

        //front bar
        frontBossBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, currBarPosition);
        frontBossBar.GetComponent<Image>().color = healthBars[numBarsLeft].color;
        frontBossBar.GetComponent<Image>().sprite = healthBars[numBarsLeft].pattern;

        float tempW = damageBar.GetComponent<RectTransform>().rect.width;
        damageBar.GetComponent<Image>().sprite = healthBars[numBarsLeft].pattern;
        if (oldNumberOfBarsLeft == numBarsLeft)
        {
            damageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, Mathf.Lerp(currBarPosition, tempW, 0.95f));
        }
        else
        {
            if (oldNumberOfBarsLeft > numBarsLeft)
            {
                damageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, 600);
            }
            if (oldNumberOfBarsLeft < numBarsLeft)
            {
                damageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, 0);
            }
        }

        oldNumberOfBarsLeft = numBarsLeft;
    }*/
}
