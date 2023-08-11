using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExpBar : MonoBehaviour {

    public float fullWidth;
    //public KHealth player1Health;
    public GameObject levelUpDialog;
    public byte oldLevel;
    public Text rankText;
    public Text remainText;
    private int startDelayFrames = 5;
    private AudioSource aso;
    public Image scoreCounterBack;
    private Color scoreCounterBackOrigColor;
    private long flashCounter = 0;

    public static bool levelUpBannerEnabled = false; // removed from demo

    void Die()
    {
        byte z = Utilities.CalculateLevel();
        float frac = 1f;
        if (z <= Utilities.RankRequirementTotal.Length - 2)
        {
            frac = ((float)(Utilities.loadedSaveData.score - Utilities.RankRequirementTotal[z])) / ((float)(Utilities.RankRequirementTotal[z + 1] - Utilities.RankRequirementTotal[z]));
        }
        GetComponent<RectTransform>().sizeDelta = new Vector2(frac * fullWidth, GetComponent<RectTransform>().sizeDelta.y);
    }

    private IEnumerator Flash()
    {
        long currFlash = ++flashCounter;
        int framesLeft = 60;
        while (currFlash == flashCounter)
        {
            float fadeprog = 1f - framesLeft / 60f;
            fadeprog *= fadeprog;
            float h = (float)((DoubleTime.UnscaledTimeSinceLoad * 3.0f) % 1.0);
            scoreCounterBack.color = Color.Lerp(Color.HSVToRGB(h, 1f, 1f), scoreCounterBackOrigColor, fadeprog);
            yield return new WaitForEndOfFrame();
            --framesLeft;
            if (framesLeft == 0)
            {
                scoreCounterBack.color = scoreCounterBackOrigColor;
                break;
            }
        }
        yield return null;
    }

	void Start () {
        startDelayFrames = 5;
        aso = GetComponent<AudioSource>();
        scoreCounterBackOrigColor = scoreCounterBack.color;
        oldLevel = Utilities.CalculateLevel();
        Die();
	}
	
	void Update () {
        if (startDelayFrames > 0) { --startDelayFrames; }
        byte z = Utilities.CalculateLevel();
        if (startDelayFrames <= 0 && z>oldLevel && z>Utilities.loadedSaveData.highestLevel && !KHealth.someoneDied)
        {
            //player1Health.maxHealth = Utilities.RankHPValues[Utilities.CalculateLevel()];
            KHealth.health = 0;
            aso.Stop(); aso.Play();
            StartCoroutine(Flash());
            // special level up messages are not in demo. will they be in actual game? i don't know
            if (!Utilities.replayLevel /*&& levelUpBannerEnabled*/)
            {
                GameObject go = GameObject.FindGameObjectWithTag("DialogueArea");
                if (go.transform.childCount == 0 && Time.timeScale > 0)
                {
                    GameObject made = Instantiate(levelUpDialog);
                    made.SetActive(true);
                    made.transform.SetParent(go.transform, false);
                }
            }
            Utilities.loadedSaveData.highestLevel = z;
        }

        rankText.text = Utilities.shortLevelNames[z];

        if (z == Utilities.RankRequirementTotal.Length-1)
        {
            remainText.text = "MAX";
        }
        else
        {
            int remain = (Utilities.RankRequirementTotal[z + 1] - Utilities.loadedSaveData.score);
            if (remain < 1000000)
            {
                remainText.text = remain + "";
            }
            else
            {
                remainText.text = (remain/1000) + "K";
            }
        }
        oldLevel = z;

        Die();
	}
}
