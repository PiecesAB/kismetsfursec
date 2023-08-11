using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelomerasePlatform : TurnMoverPlatform, IOnCustomCollect
{
    [SerializeField]
    private float width = 128;
    [SerializeField]
    private float decreaseSizePerSecond = 4;

    private float widthPlus = 0f;

    [SerializeField]
    private ParticleSystem particleL;
    [SerializeField]
    private ParticleSystem particleR;

    private float initWidth;
    private SpriteRenderer spr;

    private bool started = false;

    void Start()
    {
        initWidth = width;
        spr = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == 20 && col.GetContact(0).normal.y < 0.8f)
        {
            particleL.gameObject.SetActive(true);
            particleR.gameObject.SetActive(true);
            started = true;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Time.timeScale == 0 || !started) { return; }
        width -= Time.timeScale * decreaseSizePerSecond / 60f;
        float widthPlusFrac = (1f - Mathf.Pow(0.8f, Time.timeScale)) * widthPlus;
        widthPlus -= widthPlusFrac;
        width += widthPlusFrac;
        spr.color = Color.Lerp(Color.white, new Color(0f, 1f, 0.8f), Mathf.Clamp01(widthPlusFrac));
        if (width < 0) { width = 0f; }
        if (width > initWidth) { width = initWidth; }

        float rat = Mathf.Clamp01(width / initWidth);
        transform.localScale = new Vector3(rat, 1f, 1f);
        spr.material.SetVector("_TexSize", new Vector4(rat, 1f, 0.5f * (1f - rat), 0f));
        particleL.transform.localPosition = Vector3.left * rat * 0.5f * initWidth;
        particleR.transform.localPosition = Vector3.right * rat * 0.5f * initWidth;

        if (rat > 0.25f)
        {
            turnstile.transform.parent.localScale = new Vector3(1f, 1f, 1f / rat);
        }
        else
        {
            turnstile.transform.parent.localScale = new Vector3(1f, 1f, 4f);
        }
    }

    public void OnCustomCollect(float increment, GameObject collected)
    {
        widthPlus += increment;
    }
}
