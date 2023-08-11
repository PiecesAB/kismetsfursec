using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimFadeSprite : MonoBehaviour
{
    public bool deleteOnInvisible = false;
    public float fadeTime = 1f;
    public Gradient colorGradient;

    private Color startColor;
    private double startTime;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startTime = DoubleTime.ScaledTimeSinceLoad;
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        float rat = (float)((DoubleTime.ScaledTimeSinceLoad - startTime) / fadeTime);

        if (rat >= 1f)
        {
            sr.color = colorGradient.Evaluate(1);
            if (deleteOnInvisible) { Destroy(gameObject); }
        }

        sr.color = colorGradient.Evaluate(rat);
    }
}
