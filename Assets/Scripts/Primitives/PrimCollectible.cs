using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PrimCollectible : MonoBehaviour, IVisHelperMain {

    public int score;
    public bool scoreCounter;
    public float delay;
    public bool tetrahedron;
    public Font textfont;
    public int[] tetrahedronOnlyNumbers;
    public AudioClip[] tetrahedronOnlySounds;
    public Image[] scoreCountOnlyNumbers;
    public Sprite[] digitImages;
    public RectTransform timeBar;
    public AudioClip collectSound;
    public Text multText;
    //public bool alwaysDisplay = false;

    private MeshRenderer mesh;
    private Collider2D col;

    private int scoreAdd;
    public bool got = false;

    public static PrimCollectible mainScoreCounter;

    private int counterUpdateId = 0;

	// Use this for initialization
	void Start () {
        got = false;
        if (scoreCounter)
        {
            //Utilities.loadedSaveData = Utilities.toSaveData;
            /*Utilities.loadedSaveData.score = Utilities.toSaveData.score;
            Utilities.loadedSaveData.leveldatas = Utilities.toSaveData.leveldatas;
            Utilities.loadedSaveData.multiplier = Utilities.toSaveData.multiplier;
            Utilities.loadedSaveData.multiplierMultiplier = Utilities.toSaveData.multiplierMultiplier;*/
            scoreAdd = Utilities.loadedSaveData.score;
            mainScoreCounter = this;
            StartCoroutine(CounterUpdate());
        }

        if (!scoreCounter)
        {
            mesh = GetComponentInChildren<MeshRenderer>();
            col = GetComponent<Collider2D>();
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), transform.position.z);
            /*if (GetComponent<primRevealLocalID>() && !Utilities.replayLevel)
            {
                int dat = -1;
                if (Utilities.GetPersistentData(gameObject, -1, out dat) && dat != 0)
                {
                    if (tetrahedron)
                    {
                        score = tetrahedronOnlyNumbers[Utilities.tetrahedronCombo];
                        Door1.collected++;
                    }
                    LevelInfoContainer.nonMultipliedScoreInLevel += dat;
                    Destroy(gameObject);
                }
                //GetScoreDisplay(score);
            }
            else*/
            {
                if (mesh.isVisible) { ((IVisHelperMain)this).Vis2(); }
                else { ((IVisHelperMain)this).Invis2(); }
                VisHelper vh = mesh.gameObject.AddComponent<VisHelper>();
                vh.main = this;
            }
        }
	}
	
    string GetScoreDisplay(int x)
    {
        string n = "";
        if (x < 1000)
        {
            n = "" + x;
        }
        else if (x < 1000000)
        {
            n = (Math.Floor(x / 10.0)/100.0).ToString("##0.00").Substring(0,(x >= 100000)?3:4) + "K";
        }
        else /*if (x < 1000000000)*/
        {
            n = (Math.Floor(x / 10000.0)/100.0).ToString("##0.00").Substring(0, (x >= 100000000)?3:4) + "M";
        }
       /* else
        {
            n = "999M";
        }*/
        return n;
    }

    private bool IsCollectingCollider(Collider2D objcol)
    {
        return objcol.gameObject.layer == 20
            || objcol.gameObject.layer == 19
            || (CGICycleMover.AtLeastOneExists() && objcol.gameObject.GetComponent<CGICycleMover>());
    }

    void OnTriggerEnter2D(Collider2D objcol)
    {
        if (!got && !scoreCounter && IsCollectingCollider(objcol)) //player or punch
        {
            // new, helpful behavior: heal the player very slightly
            Encontrolmentation enc = LevelInfoContainer.GetActiveControl();
            if (enc != null && enc.GetComponent<KHealth>() != null)
            {
                enc.GetComponent<KHealth>().ChangeHealth(0.125f, "");
            }


            int oldTetComboScore = 0;
            if (tetrahedron)
            {
                GetComponent<AudioSource>().PlayOneShot(tetrahedronOnlySounds[Utilities.tetrahedronCombo],0.3f);
                Utilities.tetrahedronTime = DoubleTime.ScaledTimeSinceLoad;
                score = tetrahedronOnlyNumbers[Utilities.tetrahedronCombo];
                oldTetComboScore = score;
                //GetScoreDisplay(score);
                Utilities.tetrahedronCombo = Mathf.Min(Utilities.tetrahedronCombo+1,7);
                Door1.collected++;
            }
            if (!tetrahedron)
            {
                Utilities.tetrahedronTime = DoubleTime.ScaledTimeSinceLoad;
                GetComponent<AudioSource>().PlayOneShot(collectSound, 1f);
            }
            got = true;
            GameObject num = new GameObject();
            num.transform.position = transform.position;
            TextMesh spr = num.AddComponent<TextMesh>();
            spr.color = Color.white;
            spr.GetComponent<Renderer>().sortingLayerName = "UI";
            spr.GetComponent<Renderer>().material = textfont.material;
            spr.font = textfont;
            spr.characterSize = 5f;
            spr.alignment = TextAlignment.Center; spr.anchor = TextAnchor.LowerCenter;
            scoreAdd = Mathf.Min(Mathf.FloorToInt(score * Utilities.loadedSaveData.multiplier * Utilities.loadedSaveData.multiplierMultiplier),999999999);
            scoreAdd = (scoreAdd < 0) ? 999999999 : scoreAdd;
            spr.text = GetScoreDisplay(scoreAdd);
            num.name = "score boi";
            Rigidbody2D r = num.AddComponent<Rigidbody2D>();
            r.gravityScale = 0f;
            r.drag = 5f;
            r.velocity = Vector2.up * 50f;
            Destroy(num, Mathf.Log10(scoreAdd/Mathf.Min(Utilities.loadedSaveData.multiplier * Utilities.loadedSaveData.multiplierMultiplier,999999))/3f+0.5f);
            Utilities.ChangeScore(scoreAdd);
            Utilities.loadedSaveData.score = Mathf.Clamp(Utilities.loadedSaveData.score, 0, 999999999);
            // if we save all the money they collected, the file size will quickly increase.
            // although this allows players to potentially collect infinite money, that's fine if they choose to do so.
            // it will just be incredibly tedious.
            /*if (GetComponent<primRevealLocalID>())
            {
                if (tetrahedron) { Utilities.ChangePersistentData(gameObject, oldTetComboScore); }
                Utilities.ChangePersistentData(gameObject, 1);
            }*/
            LevelInfoContainer.nonMultipliedScoreInLevel += score;
            Destroy(gameObject.GetComponent<Collider2D>());
            Destroy(gameObject.transform.GetChild(0).gameObject);
            Destroy(gameObject, 2f);
        }
    }

    private void OnEnable()
    {
        if (scoreCounter)
        {
            StartCoroutine(CounterUpdate());
        }
    }

    public IEnumerator CounterUpdate()
    {
        int currCounterId = ++counterUpdateId;
        while (currCounterId == counterUpdateId)
        {
            if (isActiveAndEnabled)
            {
                if (Time.timeScale > 0f)
                {
                    timeBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 180f * ((1.3183593f * Time.timeScale) - (float)(DoubleTime.ScaledTimeSinceLoad - Utilities.tetrahedronTime)) / (1.3183593f * Time.timeScale));
                    timeBar.GetComponent<Image>().color = new Color32(255, (byte)(120 + (Utilities.tetrahedronCombo * 17)), (byte)(120 + (Utilities.tetrahedronCombo * 6)), 255);
                    float m = (Utilities.loadedSaveData.multiplier * Utilities.loadedSaveData.multiplierMultiplier);
                    if (DoubleTime.ScaledTimeSinceLoad - Utilities.tetrahedronTime <= (1.3183593f * Time.timeScale))
                    {
                        m *= (Utilities.tetrahedronCombo + 1);
                    }
                    else
                    {
                        Utilities.tetrahedronCombo = 0;
                    }
                    multText.text = "x" + ((m >= 100) ? (Mathf.Floor(m).ToString()) : (m.ToString("##0.00").Substring(0, 4)));
                }
                if (Utilities.loadedSaveData.score - scoreAdd < 2)
                {
                    scoreAdd = Utilities.loadedSaveData.score;
                }
                else
                {
                    int cool = Mathf.FloorToInt(Fastmath.FastLog2(Utilities.loadedSaveData.score - scoreAdd) * Fastmath.log2tolog10);
                    scoreAdd = scoreAdd + (int)(Lol(cool) / 4.5f) + 1;
                }
                for (int i = 0; i < scoreCountOnlyNumbers.Length; i++)
                {
                    if (scoreAdd == 0)
                    {
                        scoreCountOnlyNumbers[i].sprite = digitImages[0];
                        scoreCountOnlyNumbers[i].color = (i == 0) ? (new Color(0.9f, 0.9f, 0.9f)) : (new Color(0.25f, 0.25f, 0.25f));
                    }
                    else
                    {
                        //string st = scoreAdd.ToString();
                        long placeVal = Lol(i);
                        if (scoreAdd / placeVal == 0)
                        {
                            scoreCountOnlyNumbers[i].sprite = digitImages[0];
                            scoreCountOnlyNumbers[i].color = new Color(0.25f, 0.25f, 0.25f);
                        }
                        else
                        {
                            int digit = (int)(scoreAdd / placeVal) % 10;
                            scoreCountOnlyNumbers[i].sprite = digitImages[digit];
                            if (digit == 0)
                            {
                                scoreCountOnlyNumbers[i].color = new Color(0.9f, 0.9f, 0.9f);
                            }
                            else
                            {
                                scoreCountOnlyNumbers[i].color = Color.Lerp(Utilities.colorCycle[digit], new Color(0.9f, 0.9f, 0.9f), 0.9f);
                            }
                            scoreCountOnlyNumbers[i].color = Color.Lerp(scoreCountOnlyNumbers[i].color, new Color(0f, 0.4f, 1f), Mathf.Clamp01((Mathf.Log10(Utilities.loadedSaveData.score / scoreAdd)) * 2f));
                        }
                    }
                }
                //GetComponent<Text>().text = scoreAdd.ToString("0000000000");
            }
            yield return new WaitForEndOfFrame();
        }
    }

    long Lol(int cool)
    {
        long zz = 1;
        for (int i = 0; i < cool; i++)
        {
            zz *= 10;
        }
        return zz;
    }

    void IVisHelperMain.Vis2()
    {
        if (col) { col.enabled = true; }
    }

    void IVisHelperMain.Invis2()
    {
        
        if (col) { col.enabled = false; }
    }

    public bool GetAlwaysVisible()
    {
        return false;
    }
}
