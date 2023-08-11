using UnityEngine;
using System.Collections;

public class PlrUIBones : MonoBehaviour {

    public enum Func
    {
        Boneless,SpeedBonus
    }

    public GameObject player;
    public Func function;
    public SpriteRenderer scorePic;
    public TextMesh scoreTxt;
    public SpriteRenderer bg;

    public Sprite[] sprites;
    public int[] integers;
    public string[] strings;
    public Color[] colors;

    private int set;
    public int life = 200;

    void Start () {
        LevelInfoContainer li = FindObjectOfType<LevelInfoContainer>();
        set = 7;
        if (li != null)
        {
            set = 0;
            foreach (float v in li.timeRankings)
            {
                //print(DoubleTime.ScaledTimeSinceLoad);
                if (DoubleTime.ScaledTimeSinceLoad < v)
                {
                    break;
                }
                    set++;
            }

            if (set == 7)
            {
                DeleteMe();
            }
            else
            {
                scorePic.sprite = sprites[set];
                Utilities.ChangeScore(integers[set]);
                scoreTxt.text = strings[set] + " SPD BONUS";
            }
        }

	}

    public void DeleteMe()
    {
        PlrUI.DestroyStatusBox(gameObject, player.transform);
    }

	void Update () {
        life--;
        if (life <= 0)
        {
            DeleteMe();
        }

        Animator an = player.GetComponent<Animator>();
        if (set == 0)
        {
            Color rb = Color.HSVToRGB(((float)DoubleTime.UnscaledTimeRunning * 2f) % 1f, 0.6f, 1f);
            bg.color = rb;
            scorePic.color = scoreTxt.color = Color.black;
            an.CrossFade("ultraWin", 0f);
        }
        else if (set == 2 || set == 1)
        {
            bg.color = colors[set];
            scorePic.color = scoreTxt.color = Color.black;
            if (!an.GetCurrentAnimatorStateInfo(0).IsName("ultraWin"))
            {
                an.CrossFade("superWin", 0f);
            }
        }
        else if (set <= 6)
        {
            scorePic.color = colors[set];
        }

	}
}
