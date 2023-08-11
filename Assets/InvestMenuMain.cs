using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InvestMenuMain : MonoBehaviour {

    int choice;
    int z;
    public Transform[] buttonHolders;
    public CanvasRenderer investButton;
    public Text[] scoreTexts;
    public Text hint;
    public int nscore;
    public float nmult;
    public float nmult2;
    public AudioSource openSound;
    public AudioSource changeSound;
    public AudioSource selectSound;
    public AudioSource buySound;
    public Text info1;
    public Text info2;
    public Text infoReplay;
    Encontrolmentation e;

    void Start () {
        openSound.Stop(); openSound.Play();
        choice = 19;
        z = 0;
        e = GetComponent<Encontrolmentation>();
        if (Utilities.replayLevel)
        {
            info1.gameObject.SetActive(false);
            //info2.gameObject.SetActive(false);
            infoReplay.gameObject.SetActive(true);
        }
        ScoreUpdate();
	}

    void ScoreUpdate()
    {
        var ls = Utilities.loadedSaveData;
        //score
        scoreTexts[0].text = (ls.score+nscore).ToString("########0");
        scoreTexts[0].color = (nscore == 0) ? Color.white : ((nscore > 0)?Color.green:Color.red);
        //stock
        scoreTexts[1].text = ((ls.multiplier+nmult)* (ls.multiplierMultiplier+nmult2)).ToString("########0.00");
        scoreTexts[1].color = (Mathf.Approximately(nmult,0f)) ? Color.white : ((nmult > 0) ? Color.green : Color.red);
        //option
        scoreTexts[2].text = (ls.multiplierMultiplier+nmult2).ToString("########0.00");
        scoreTexts[2].color = (Mathf.Approximately(nmult2, 0f)) ? Color.cyan : ((nmult2 > 0) ? Color.green : Color.red/*???*/);
    }

    void Purchase()
    {

    }

    ulong MultToScore(float s) 
    {
        s--;
        return (ulong)(250000 * s * s);
    }

    float Mult2ToMult(float s) 
    {
        s--;
        return 2.82843f*Mathf.Pow(s,1.5f)+1f;
    }

    float ScoreToMult(int s)
    {
        return 0.002f * Mathf.Sqrt(s) + 1f;
    }

    float MultToMult2(float s)
    {
        s--;
        return 0.5f * Mathf.Pow(s,0.6666666666f) + 1f;
    }

    // sorry. only god understands this calculation now.
    // and if god doesn't exist, well, T.S.

   void AddScore(int x)
    {
        var ls = Utilities.loadedSaveData;
        int n1 = nscore + x;
        int ul = Mathf.FloorToInt(Mathf.Min((int)(MultToScore(ls.multiplier) - MultToScore(Mult2ToMult(nmult2 + ls.multiplierMultiplier) - Mult2ToMult(ls.multiplierMultiplier) + 1f))+ls.score, 999999999));
        if (ls.score + n1 < 0)
        {
            nscore = -ls.score;
        }
        else if (ls.score + n1 > ul)
        {
            nscore = ul - ls.score;
        }
        else
        {
            nscore = n1;
        }

        nmult = (Mult2ToMult(ls.multiplierMultiplier) - Mult2ToMult(ls.multiplierMultiplier + nmult2)) + ScoreToMult((int)MultToScore(ls.multiplier)-nscore)-ls.multiplier;
    }

    void AddMultA(float x)
    {
        var ls = Utilities.loadedSaveData;
        float n1 = nmult + x;
        float ul = Mathf.Min(ScoreToMult(ls.score+(int)MultToScore(ls.multiplier))-Mult2ToMult(ls.multiplierMultiplier+nmult2)+ Mult2ToMult(ls.multiplierMultiplier), 8500000f);
        float ll = Mathf.Max(1f , 1f);
        if (ls.multiplier + n1 < ll)
        {
            nmult = 1f-ls.multiplier;
        }
        else if (ls.multiplier + n1 > ul)
        {
            nmult = ul - ls.multiplier;
        }
        else
        {
            nmult = n1;
        }

        nscore = (int)(MultToScore(ls.multiplier)-MultToScore(Mult2ToMult(nmult2+ls.multiplierMultiplier)-Mult2ToMult(ls.multiplierMultiplier)+ls.multiplier+nmult));
    }

    void AddMult2(float x)
    {
        var ls = Utilities.loadedSaveData;
        float n1 = nmult2 + x;
        float ul = Mathf.Min(MultToMult2(ScoreToMult((int)MultToScore(ls.multiplier)-nscore)+Mult2ToMult(ls.multiplierMultiplier)-1f), 1400f);
        if (ls.multiplierMultiplier + n1 < ls.multiplierMultiplier)
        {
            nmult2 = 0f;
        }
        else if (ls.multiplierMultiplier + n1 > ul)
        {
            nmult2 = ul - ls.multiplierMultiplier;
        }
        else
        {
            nmult2 = n1;
        }
        nmult = (Mult2ToMult(ls.multiplierMultiplier) - Mult2ToMult(ls.multiplierMultiplier + nmult2)) + ScoreToMult((int)MultToScore(ls.multiplier) - nscore) - ls.multiplier;
    }

    Dictionary<int, Material> m = new Dictionary<int, Material>();

    private void SeeIfEscape()
    {
        if (e.AnyButtonDown(1056UL)) //b or start
        {
            Destroy(transform.parent.gameObject);
            e.possiblePreviousFocus.allowUserInput = true;
            SceneManager.UnloadSceneAsync(gameObject.scene.buildIndex);
        }
    }

    void Update () {
        if (Utilities.replayLevel) {
            SeeIfEscape();
            return;
        }
        
        z = Mathf.Max(z - 1, 0);

        if (choice < 20 && nmult2 < 0) //how could this even be possible?
        {
            choice = 20 + (choice%10);
        }

        if (e.ButtonDown(16UL, 16UL)) //A
        {
            selectSound.Stop(); selectSound.Play();
            switch (choice)
            {
                #region 0-9
                case 0:
                    AddScore(-10000000);
                    break;
                case 1:
                    AddScore(-500000);
                    break;
                case 2:
                    AddScore(-20000);
                    break;
                case 3:
                    AddScore(-1000);
                    break;
                case 4:
                    AddScore(-nscore);
                    break;
                case 5:
                    AddScore(1000);
                    break;
                case 6:
                    AddScore(20000);
                    break;
                case 7:
                    AddScore(500000);
                    break;
                case 8:
                    AddScore(10000000);
                    break;
                case 9:
                    AddScore(999999999);
                    break;
                #endregion
                #region 10-19
                case 10:
                    AddMultA(-10f);
                    break;
                case 11:
                    AddMultA(-1f);
                    break;
                case 12:
                    AddMultA(-0.1f);
                    break;
                case 13:
                    AddMultA(-0.01f);
                    break;
                case 14:
                    AddMultA(-nmult);
                    break;
                case 15:
                    AddMultA(0.01f);
                    break;
                case 16:
                    AddMultA(0.1f);
                    break;
                case 17:
                    AddMultA(1f);
                    break;
                case 18:
                    AddMultA(10f);
                    break;
                case 19:
                    AddMultA(999999999f);
                    break;
                #endregion
                #region 20-29
                case 20:
                    AddMult2(-10f);
                    break;
                case 21:
                    AddMult2(-1f);
                    break;
                case 22:
                    AddMult2(-0.1f);
                    break;
                case 23:
                    AddMult2(-0.01f);
                    break;
                case 24:
                    AddMult2(-nmult2);
                    break;
                case 25:
                    AddMult2(0.01f);
                    break;
                case 26:
                    AddMult2(0.1f);
                    break;
                case 27:
                    AddMult2(1f);
                    break;
                case 28:
                    AddMult2(10f);
                    break;
                case 29:
                    AddMult2(999999999f);
                    break;
                #endregion
                case 30:
                    buySound.Stop(); buySound.Play();
                    Utilities.loadedSaveData.score += nscore; // do not use the "ChangeScore" function. that would allow arbitrarily high total score!
                    Utilities.loadedSaveData.multiplier += nmult;
                    Utilities.loadedSaveData.multiplierMultiplier += nmult2;
                    nmult = nmult2 = 0f; nscore = 0;
                    break;
                default:
                break; //lmao
            }
        }
        SeeIfEscape();

        var ls = Utilities.loadedSaveData;
        if (ls.score + nscore < 0)
        {
            nscore = -ls.score;
        }
        if (ls.multiplier + nmult < 1f)
        {
            nmult = 1f - ls.multiplier;
        }
        if (ls.multiplierMultiplier + nmult2 < 1f)
        {
            nmult2 = 1f - ls.multiplierMultiplier;
        }

        #region changeSelected
        if (e.ButtonDown(1UL, 3UL)) //left
            {
            changeSound.Stop(); changeSound.Play();
            choice = (choice % 10 == 0) ? choice : (choice - 1);
            }
            if (e.ButtonDown(2UL, 3UL)) //right
            {
            changeSound.Stop(); changeSound.Play();
            choice = (choice % 10 == 9) ? choice : Mathf.Min(choice + 1,30);
            }
            if (e.ButtonDown(4UL, 12UL)) //up
            {
            changeSound.Stop(); changeSound.Play();
            choice = (choice == 30) ? 29 : ((choice >= 10) ? Mathf.Max((nmult2 < 0) ? (20 + (choice % 10)) : 0, choice - 10) : choice);
            }
            if (e.ButtonDown(8UL, 12UL)) //down
            {
            changeSound.Stop(); changeSound.Play();
            choice = Mathf.Min(30, choice + 10);
            }

        if (z == 0)
        {
            double pt = 0f;
            if (e.ButtonHeld(1UL, 3UL, 0.5f, out pt))
            {
                changeSound.Stop(); changeSound.Play();
                choice = (choice%10 == 0)?choice:( choice - 1);
                z = (int)(10 / pt);
            }
            if (e.ButtonHeld(2UL, 3UL, 0.5f, out pt))
            {
                changeSound.Stop(); changeSound.Play();
                choice = (choice % 10 == 9) ? choice : Mathf.Min(choice + 1,30);
                z = (int)(10 / pt);
            }
            if (e.ButtonHeld(4UL, 12UL, 0.5f, out pt))
            {
                changeSound.Stop(); changeSound.Play();
                choice = (choice == 30) ? 29 : ((choice >= 10) ? Mathf.Max((nmult2<0)?(20+(choice%10)):0, choice - 10) : choice);
                z = (int)(10 / pt);
            }
            if (e.ButtonHeld(8UL, 12UL, 0.5f, out pt))
            {
                changeSound.Stop(); changeSound.Play();
                choice = Mathf.Min(30, choice + 10);
                z = (int)(10 / pt);
            }
        }




        #endregion

        #region changeGraphics
        for (int i = 0; i < 3; i++)
        {
            foreach (Transform t in buttonHolders[i])
            {
                int cval = (i * 10) + int.Parse(t.name);
                if (!m.ContainsKey(cval)) { m[cval] = Instantiate(t.GetComponent<CanvasRenderer>().GetMaterial()); } //using the shader "Sprites/RodBandW"
                // won't
                if (cval == choice)
                {
                    m[cval].SetFloat("_B", 0.5f);
                    m[cval].SetFloat("_I", 0f);
                }
                else
                {
                    if (i <= 1 && nmult2 < 0) //rudimentary code
                    {
                        m[cval].SetFloat("_B", 0.07f);
                        m[cval].SetFloat("_I", 1f);
                    }
                    else
                    {
                        m[cval].SetFloat("_B", 0.07f);
                        m[cval].SetFloat("_I", 0.85f);
                    }
                }
                t.GetComponent<CanvasRenderer>().SetMaterial(m[cval], 0);
            }
        }
        if (!m.ContainsKey(30)) { m[30] = Instantiate(investButton.GetMaterial()); }
        if (choice == 30)
        {
            m[30].SetFloat("_B", 0.5f);
            m[30].SetFloat("_I", 0f);
        }
        else
        {
            m[30].SetFloat("_B", 0.07f); //CHANGE THIS
            m[30].SetFloat("_I", 0.85f);
        }
        investButton.SetMaterial(m[30], 0);
        ScoreUpdate();
        #endregion

    }
}
