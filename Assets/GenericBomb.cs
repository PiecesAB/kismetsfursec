using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericBomb : MonoBehaviour
{

    public float waitBetween;
    public int startTime;
    public bool haltCountdown;
    public TextMesh counter;
    public AudioClip[] sounds;
    protected double tuv;
    protected float volo;

    public abstract IEnumerator Explode();

    public abstract void OnStartCountdown();

    protected abstract void SubStart();
    protected abstract void SubUpdate();
    protected abstract bool FinalClearanceBeforeExplode();

    private void Start()
    {
        //GetComponent<SpriteRenderer>().sprite = numbers[startTime];
        counter.text = (startTime + 1).ToString();
        tuv = -5;
        if (GetComponent<AudioSource>() != null)
        {
            volo = GetComponent<AudioSource>().volume;
        }

        SubStart();
    }

    public IEnumerator Countdown()
    {
        while (startTime >= 0)
        {
            if (!haltCountdown)
            {
                startTime = startTime - 1;
                counter.text = (startTime + 1).ToString();
                tuv = DoubleTime.ScaledTimeSinceLoad;
                //GetComponent<SpriteRenderer>().sprite = numbers[startTime+1];
            }
            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().clip = sounds[Mathf.Min(startTime + 1, 8)];
            GetComponent<AudioSource>().volume = Mathf.Max((0.3f * volo) + (0.7f * (7f - startTime) / 8f * volo), volo / 10f);
            GetComponent<AudioSource>().Play();
            if (startTime == -1)
            {
                // GetComponent<AudioSource>().PlayOneShot(beep2);
                yield return new WaitForSeconds(waitBetween);
            }
            else
            {
                //GetComponent<AudioSource>().PlayOneShot(beep1);
                yield return new WaitForSeconds(waitBetween);
            }
        }

        if (Door1.levelComplete)
        {
            yield break;
        }

        if (!FinalClearanceBeforeExplode())
        {
            yield return new WaitUntil(() => FinalClearanceBeforeExplode());
        }

        counter.text = "";
        StartCoroutine(Explode());
    }

    public void Activate()
    {
        OnStartCountdown();
        StartCoroutine(Countdown());
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        float h = (Mathf.Atan((startTime + 1) / 7f)) * 0.5305165f;
        double s = (startTime == -1)?1f:((-0.06f / (DoubleTime.ScaledTimeSinceLoad - tuv + 0.06f)) + 1f);
        float v = (startTime == -1) ? 1f : (1f - ((float)s / 1.8f));
        counter.color = Color.HSVToRGB(h, (float)s, v);
        SubUpdate();
    }
}
