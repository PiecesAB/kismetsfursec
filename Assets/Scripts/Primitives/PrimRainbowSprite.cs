using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimRainbowSprite : MonoBehaviour
{
    [SerializeField]
    private bool cycleInstead;
    [SerializeField]
    private float cycleAdvancePerFrame = 0.008333333f;
    [SerializeField]
    private float saturation = 1f;
    [SerializeField]
    private float value = 1f;
    private SpriteRenderer sr;
    private float ct;
    public bool useHSVShader = false;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ct = Fakerand.Single();
        if (!cycleInstead)
        {
            sr.material.SetFloat("_TOH", 0.7f);
            Destroy(this);
        }
        else
        {
            if (!useHSVShader)
            {
                float a = sr.color.a;
                sr.color = Color.HSVToRGB(ct, saturation, value);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
            }
        }
    }

    private void Update()
    {
        if (cycleInstead)
        {
            ct = (ct + cycleAdvancePerFrame*Time.timeScale) % 1f;
            if (useHSVShader)
            {
                sr.material.SetFloat("_HueShift", 360f*ct);
            }
            else
            {
                float a = sr.color.a;
                sr.color = Color.HSVToRGB(ct, saturation, value);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
            }
        }
    }
}
