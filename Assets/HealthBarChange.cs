using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarChange : MonoBehaviour {

    public Image bar;
    public SpriteRenderer barr;
    public Text text;
    public TextMesh textt;
    [Header("healthObj does nothing if this is not rebuff mode!")]
    public KHealth healthObj;
    public bool rebuffVersion = false;
    private Color col;
    private int change = 0;

    void Start()
    {
        col = new Color(0.2f, 0.7f, 0.2f, 1f);
        if (!rebuffVersion)
        {
            bar.material.SetFloat("_Val", 0f); //change ths?
            bar.material.SetFloat("_LerpVal", 0f); //change ths?
            bar.material.SetColor("_BarColor", Color.HSVToRGB(0.5f, 0.7f, 0.8f)); //change ths?
            text.text = KHealth.health.ToString();
        }
        else
        {
            barr.material.SetFloat("_Val", 1f);
            barr.material.SetFloat("_LerpVal", 1f);
            barr.material.SetColor("_BarColor", col);
            barr.color = Color.clear;
            textt.text = "";
        }
    }

	void Update ()
    {
        if (!rebuffVersion)
        {
            float num2 = Utilities.GetPlayerDamageRatio();
            float num = 1f-num2;
            bar.material.SetFloat("_Val", num2);
            //bar.material.SetColor("_BarColor", Color.HSVToRGB((num / 1.7f)%1f, 0.7f, 0.8f));

            /*if (num >= 0.2f)
            {
                bar.color = Color.white;
            }*/
            ////
            if (num >= 3.00f)
            {
                bar.material.SetColor("_BarColor", Color.HSVToRGB(0.833f, 0.7f, 0.8f));
            }
            else if (num >= 1.001f)
            {
                bar.material.SetColor("_BarColor", Color.HSVToRGB(0.667f, 0.7f, 0.8f));
            }
            else if (num >= 1.00f)
            {
                bar.material.SetColor("_BarColor", Color.HSVToRGB(0.5f, 0.7f, 0.8f));
            }
            else if (num >= 0.55f)
            {
                bar.material.SetColor("_BarColor", Color.HSVToRGB(0.333f, 0.7f, 0.8f));
            }
            else if (num >= 0.25f)
            {
                bar.material.SetColor("_BarColor", Color.HSVToRGB(0.153f, 0.7f, 0.8f));
            }
            else if (num >= 0.1f)
            {
                bar.material.SetColor("_BarColor", Color.HSVToRGB(0f, 0.7f, 0.8f));
            }
            else
            {
                float t = 0.5f + 0.5f * (float)System.Math.Sin(DoubleTime.UnscaledTimeSinceLoad * Mathf.PI * 5.0);
                bar.material.SetColor("_BarColor", Color.Lerp(Color.HSVToRGB(0f, 0.7f, 0.8f),Color.white,t));

            }

            string hptx = Mathf.Ceil(num * 100f).ToString();
            Color c = Color.white;

            float l = bar.material.GetFloat("_LerpVal");
            if (num2 > l)
            {
                if (text.text != hptx)
                {
                    c = new Color(1f, 0.6f, 0.6f);
                }
                bar.material.SetFloat("_LerpVal", Mathf.Min(num2, l + (1f / 108f)));
            }
            if (num2 < l)
            {
                if (text.text != hptx)
                {
                    c = Color.green;
                }
                bar.material.SetFloat("_LerpVal", Mathf.Max(num2, l - (1f / 108f)));
            }

            text.color = c;
            string finalDisplayText = Mathf.Ceil(KHealth.health).ToString();
            if (finalDisplayText == "Infinity") { finalDisplayText = "∞"; }
            text.text = finalDisplayText;
        }
        else
        {
            if (healthObj == null) { return; }

            if (healthObj.nontoxic > 0)
            {
                float rat = healthObj.nontoxic / healthObj.previousnontoxmax;
                barr.material.SetFloat("_Val", rat);
                barr.material.SetFloat("_LerpVal", rat);
                barr.material.SetColor("_BarColor", col);
                barr.color = Color.white;
                textt.color = Color.white;
                textt.text = "RBF: " + Mathf.Floor(healthObj.nontoxic / 10f);
            }
            else
            {
                if (barr.color == Color.white)
                {
                    PlrUI.DestroyStatusBox(gameObject, healthObj.transform);
                }
                else
                {
                    barr.color = Color.clear;
                    textt.text = "";
                    barr.material.SetFloat("_Val", 1f);
                    barr.material.SetFloat("_LerpVal", 1f);
                    textt.color = Color.clear;
                    barr.material.SetColor("_BarColor", col);
                }
            }
        }
    }
}
