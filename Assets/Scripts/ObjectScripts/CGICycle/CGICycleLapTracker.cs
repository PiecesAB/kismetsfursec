using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGICycleLapTracker : MonoBehaviour
{

    private Dictionary<CGICycleMover, int> parityTracker = new Dictionary<CGICycleMover, int>();
    private Dictionary<CGICycleMover, int> lapsTracker = new Dictionary<CGICycleMover, int>();

    private BoxCollider2D myCol;
    private SpriteRenderer mySpr;

    [SerializeField]
    private TextMesh text;
    private Renderer textParentRend;

    [SerializeField]
    private AudioClip sound1;
    [SerializeField]
    private AudioClip sound0;
    [SerializeField]
    private AudioClip soundm1;
    private AudioSource myAud;

    private static AudioSource monadAud = null;

    private Color CE;
    private Color CO;

    public int lapsRemaining = 3;

    public GameObject[] triggered;

    private int animId = 0;

    private bool win = false;

    private int GetParity(CGICycleMover mover)
    {
        float d = Vector2.Dot(transform.right, mover.transform.position - transform.position);
        return d >= 0 ? 1 : -1;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        CGICycleMover mover = col.GetComponent<CGICycleMover>();
        if (!mover) { Physics2D.IgnoreCollision(col, myCol); return; }
        if (!parityTracker.ContainsKey(mover))
        {
            parityTracker.Add(mover, GetParity(mover));
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        CGICycleMover mover = col.GetComponent<CGICycleMover>();
        if (!mover) { Physics2D.IgnoreCollision(col, myCol); return; }
        if (parityTracker.ContainsKey(mover))
        {
            parityTracker.Remove(mover);
        }
    }

    void Start()
    {
        mySpr = GetComponent<SpriteRenderer>();
        myCol = gameObject.AddComponent<BoxCollider2D>();
        myCol.isTrigger = true;
        myCol.size = new Vector2(mySpr.size.x * 3, mySpr.size.y);
        Transform tpt = text.transform.parent;
        textParentRend = tpt.GetComponent<Renderer>();
        tpt.localPosition = new Vector3(0, -(mySpr.size.y * 0.5f) - 8f, -32f);
        text.transform.parent.gameObject.SetActive(true);
        text.text = lapsRemaining.ToString();
        text.transform.rotation = Quaternion.identity;
        CE = mySpr.material.GetColor("_CE");
        CO = mySpr.material.GetColor("_CO");
        myAud = GetComponent<AudioSource>();
    }

    private void PlayAudioClip(AudioClip c)
    {
        if (monadAud != null) { monadAud.Stop(); }
        monadAud = myAud;
        myAud.Stop();
        myAud.clip = c;
        myAud.Play();
    }

    private IEnumerator Animation(int amt)
    {
        if (amt == 0) { yield break; }
        int currAnimId = ++animId;
        double startTime = DoubleTime.UnscaledTimeSinceLoad;
        while (currAnimId == animId)
        {
            double t = DoubleTime.UnscaledTimeSinceLoad - startTime;
            if (t < 0.5f)
            {
                if (amt > 0)
                {
                    mySpr.material.SetColor("_CE", (t % 0.15 < 0.075) ? Color.red : Color.clear);
                    mySpr.material.SetColor("_CO", (t % 0.15 >= 0.075) ? Color.red : Color.clear);
                }
                else
                {
                    mySpr.material.SetColor("_CE", (t % 0.15 < 0.075) ? Color.white : Color.clear);
                    mySpr.material.SetColor("_CO", (t % 0.15 >= 0.075) ? Color.white : Color.clear);
                }
            }
            else
            {
                if (lapsRemaining <= 0)
                {
                    mySpr.material.SetColor("_CE", Color.white);
                    mySpr.material.SetColor("_CO", Color.white);
                }
                else
                {
                    mySpr.material.SetColor("_CE", CE);
                    mySpr.material.SetColor("_CO", CO);
                }
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    void TriggerAnimation(int amt)
    {
        StartCoroutine(Animation(amt));
        if (lapsRemaining <= 0) { PlayAudioClip(sound0); }
        else if (amt > 0) { PlayAudioClip(soundm1); }
        else if (amt < 0) { PlayAudioClip(sound1); }

        if (lapsRemaining <= 0)
        {
            if (!win)
            {
                foreach (GameObject g in triggered)
                {
                    IAmbushController iac = g.GetComponent<IAmbushController>();
                    iac.OnAmbushComplete();
                }
            }
            win = true;
        }
        else
        {
            if (win)
            {
                foreach (GameObject g in triggered)
                {
                    IAmbushController iac = g.GetComponent<IAmbushController>();
                    iac.OnAmbushBegin();
                }
            }
            win = false;
        }
    }

    private void ChangeLaps(int amt, CGICycleMover mover)
    {
        if (!lapsTracker.ContainsKey(mover)) { lapsTracker[mover] = 0; }
        lapsTracker[mover] += amt;
        mover.AddLapTracker(this);
        lapsRemaining += amt;
        TriggerAnimation(amt);
    }

    public void RevertLaps(CGICycleMover mover)
    {
        if (!lapsTracker.ContainsKey(mover)) { return; }
        lapsRemaining -= lapsTracker[mover];
        TriggerAnimation(-lapsTracker[mover]);
        lapsTracker.Remove(mover);
    }

    void Update()
    {
        Dictionary<CGICycleMover, int> parityTrackerClone = new Dictionary<CGICycleMover, int>(parityTracker);
        foreach (KeyValuePair<CGICycleMover, int> kv in parityTrackerClone)
        {
            CGICycleMover mover = kv.Key;
            int oldParity = kv.Value;
            int newParity = GetParity(mover);
            if (oldParity != newParity)
            {
                if (newParity == 1)
                {
                    // a lap has been completed.
                    ChangeLaps(-1, mover);
                }
                else
                {
                    // a negative lap has been completed!!!
                    ChangeLaps(1, mover);
                }
            }
            parityTracker[mover] = newParity;
        }

        if (textParentRend.isVisible)
        {
            text.text = Mathf.Max(lapsRemaining, 0).ToString();
        }
    }
}
