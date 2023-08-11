using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class electricCompass : MonoBehaviour
{
    public Transform needle;
    public Transform arrowEffect;
    public float animationProgress = 1f;
    public float animationProgPerFrame = 0.03333333f;
    public AnimationCurve effectSize;
    public Gradient effectColor;
    public AudioClip startSound;
    public AudioClip endSound;
    public AudioSource soundSource;

    private SpriteRenderer spr;
    private SpriteRenderer arrSpr;

    public static bool rotatingNow = false;

    private int touches = 0;

    void Start()
    {
        animationProgress = 1f;
        spr = GetComponent<SpriteRenderer>();
        arrSpr = arrowEffect.GetComponent<SpriteRenderer>();
    }

    private IEnumerator Cutscene(Transform t)
    {
        if (rotatingNow) { yield break; }
        rotatingNow = true;
        animationProgress = 0f;

        // float origTS = Utilities.StopTime();
        Rigidbody2D r2 = t.GetComponent<Rigidbody2D>();
        RigidbodyConstraints2D r2c = r2.constraints;
        r2.constraints = RigidbodyConstraints2D.FreezeAll;
        t.GetComponent<KHealth>().enabled = false;
        Utilities.canPauseGame = Utilities.canUseInventory = false;

        soundSource.Stop();
        soundSource.clip = startSound;
        soundSource.Play();

        float s = t.eulerAngles.z;
        float e = transform.eulerAngles.z;
        Vector2 dif = t.position - transform.position;
        if (dif.magnitude <= 30f) // 16block + 14player
        {
            t.position = transform.position + (Vector3)dif.normalized * 30f;
        }
        for (int i = 0; i < 30; ++i)
        {
            t.eulerAngles = Vector3.forward * Mathf.LerpAngle(s, e, Mathf.Pow(i / 30f, 0.75f));
            yield return new WaitForEndOfFrame();
        }
        t.eulerAngles = Vector3.forward * e;

        r2.constraints = r2c;
        t.GetComponent<KHealth>().enabled = true;
        Utilities.canPauseGame = Utilities.canUseInventory = true;

        soundSource.Stop();
        soundSource.clip = endSound;
        soundSource.Play();

        rotatingNow = false;

        //Utilities.ResumeTime(origTS);
        yield return null;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (Time.timeScale == 0) { return; }
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (!e) { return; }
        BasicMove bm = e.GetComponent<BasicMove>();
        if (col.gameObject.layer == 20 && bm.CanCollide)
        {
            if (touches == 0 && animationProgress == 1f && Mathf.Abs(e.transform.eulerAngles.z - transform.eulerAngles.z) > 0.01f)
            {
                if (col.gameObject == e.gameObject)
                {
                    // cutscene
                    StartCoroutine(Cutscene(e.transform));
                }
                else
                {
                    e.transform.eulerAngles = Vector3.forward * transform.localEulerAngles.z;
                    animationProgress = 0f;
                }
            }
            ++touches;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (!e) { return; }
        if (col.gameObject.layer == 20)
        {
            --touches;
            if (touches < 0) { touches = 0; }
        }
    }

    void Update()
    {
        if (!spr.isVisible) { return; }
        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (!e) { return; }
        needle.localEulerAngles = new Vector3(0,0,-transform.eulerAngles.z + e.transform.eulerAngles.z);
        arrowEffect.localScale = Vector3.one * effectSize.Evaluate(animationProgress);
        arrSpr.color = effectColor.Evaluate(animationProgress);
        animationProgress = Mathf.Clamp01(animationProgress+animationProgPerFrame);
    }
}
