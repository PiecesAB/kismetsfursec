using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GunBarChange : MonoBehaviour {

    public Image bar;
    public Text text;
    private SpecialGunTemplate healthObj;
    private SpecialGunTemplate[] hos = null;
    public static float changeGunHealthEmergency = 0; // used in emergencies when the SpecialGunTemplate isn't available (like in the CGICycles)

    private int change = 0;

    void SetControllers()
    {
        int newCount = LevelInfoContainer.allCtsInLevel.Length;
        hos = new SpecialGunTemplate[newCount];
        for (int i = 0; i < newCount; ++i)
        {
            if (LevelInfoContainer.allCtsInLevel[i])
            {
                hos[i] = LevelInfoContainer.allCtsInLevel[i].GetComponent<SpecialGunTemplate>();
            }
        }
    }

    void Start()
    {
        float zx = FindObjectOfType<LevelInfoContainer>().kStartEnergy / 100f;
        bar.material.SetFloat("_Val", zx); //change ths
        bar.material.SetFloat("_LerpVal", zx); //change ths
        SetControllers();
        //bar.material.SetColor("_BarColor", Color.HSVToRGB(1f / 1.7f, 0.7f, 0.8f)); //change ths
    }
	
    public IEnumerator Show(string t, Color c)
    {
        change = (change+1)%9000;
        text.color = c;
        text.text = t;
        int oc2 = change;
        yield return new WaitForSeconds(2.5f*Time.timeScale);
        if (change == oc2)
        {
            text.color = Color.white;
            text.text = "EN";
        }
    }

    void UpdateHealthObj()
    {
        for (int i = 0; i < LevelInfoContainer.allCtsInLevel.Length; ++i)
        {
            Encontrolmentation e = LevelInfoContainer.allCtsInLevel[i];
            if (e && e.allowUserInput)
            {
                healthObj = hos[i];
                return;
            }
        }
    }

	void Update ()
    {
        UpdateHealthObj();
        if (!healthObj || !healthObj.enabled)
        {
            bar.enabled = false;
            text.enabled = false;
            return;
        }
        else
        {
            bar.enabled = true;
            text.enabled = true;
        }
        if (healthObj)
        {
            if (changeGunHealthEmergency > 0)
            {
                healthObj.gunHealth += changeGunHealthEmergency;
                changeGunHealthEmergency = 0;
            }
            float num = healthObj.gunHealth / 100f;

            bar.material.SetFloat("_Val", num);

            string hptx = Mathf.Floor(num * 100f).ToString();

            float l = bar.material.GetFloat("_LerpVal");
            if (num > l)
            {
                if (text.text != hptx)
                {
                    StartCoroutine(Show(hptx, Color.green));
                }
                bar.material.SetFloat("_LerpVal", Mathf.Min(num, l + (1f / 108f)));
            }
            if (num < l && num > 0f)
            {
                if (text.text != hptx)
                {
                    StartCoroutine(Show(hptx, new Color(1f, 0.6f, 0.6f)));
                }
                bar.material.SetFloat("_LerpVal", Mathf.Max(num, l - (1f / 108f)));
            }
            if (num <= 0f)
            {
                bar.material.SetFloat("_LerpVal", 0f);
            }
        }
    }
}
