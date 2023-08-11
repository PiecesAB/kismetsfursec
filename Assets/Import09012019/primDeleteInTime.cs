using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class primDeleteInTime : MonoBehaviour {

    public enum EffectMode
    {
        ShaderProgress, FlashIn
    }

    public double t;
    public bool onlyWhenActive = false;
    public bool realTime;
    private double x;
    [Header("vvv Special vvv")]
    public SpriteRenderer spawnEffectHelper;
    public EffectMode effectMode;
    public bool explodeIfPossible = false;

    private float f1;

    [HideInInspector]
    public double activeC;

	void Start () {
        x = (realTime)?DoubleTime.UnscaledTimeRunning:DoubleTime.ScaledTimeSinceLoad;
        activeC = 0.0;
        if (spawnEffectHelper)
        {
            spawnEffectHelper.material.SetFloat("_Prog", 0f);
        }

        if (spawnEffectHelper)
        {
            switch (effectMode)
            {
                case EffectMode.FlashIn:
                    f1 = transform.localScale.x;
                    break;
                default:
                    break;
            }
        }
    }

	void Update () {
        double c = 0.0;
        if (onlyWhenActive)
        {
            if (gameObject.activeInHierarchy)
            {
                activeC += 0.0166666666666666 * ((realTime) ? 1.0 : Time.timeScale);
                c = x + activeC;
            }
        }
        else { c = (realTime) ? DoubleTime.UnscaledTimeRunning : DoubleTime.ScaledTimeSinceLoad; }
        
        double d = c - x;
        float prog = (float)(d / t);
        if (d >= t)
        {
            if (explodeIfPossible && GetComponent<GenericBlowMeUp>())
            {
                GetComponent<GenericBlowMeUp>().BlowMeUp();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (spawnEffectHelper)
        {
            switch (effectMode)
            {
                case EffectMode.ShaderProgress:
                    spawnEffectHelper.material.SetFloat("_Prog", prog);
                    break;
                case EffectMode.FlashIn:
                    Color lc = spawnEffectHelper.color;
                    if (prog < 0.2f)
                    {
                        float prog2 = prog * 5f;
                        float fi1 = Mathf.Lerp(f1 * 2.5f, f1, prog2);
                        transform.localScale = new Vector3(fi1, fi1, 1f);
                        spawnEffectHelper.color = new Color(lc.r, lc.g, lc.b, prog2*0.85f);
                    }
                    else
                    {
                        float prog3 = (prog - 0.2f) * 1.25f;
                        transform.localScale = new Vector3(f1, f1, 1f);
                        spawnEffectHelper.color = new Color(lc.r, lc.g, lc.b, (1f-prog3) * 0.85f);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
