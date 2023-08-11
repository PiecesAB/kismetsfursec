using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipPanel : MonoBehaviour
{
    [Range(0, 31)]
    public int switchId;

    public int flipsLeft = 1;
    public bool switchInverted = false;
    private int origFlipsLeft = 1;

    public TextMesh tm;

    public Sprite s0;
    public Sprite s1;

    public bool deleteOnZero = false;
    public BulletHellMakerFunctions shooter;

    private int inside = 0;
    private BoxCollider2D bc;
    private SpriteRenderer sr;

    private Vector2 origColSize;
    private Vector2 smallColSize;

    private int flipStateCounter = 0;

    public static HashSet<FlipPanel> all = new HashSet<FlipPanel>();

    public static void SwitchUpdate(uint nmask)
    {
        foreach (FlipPanel fp in all)
        {
            if ((nmask & (1u << fp.switchId)) == 0u) { continue; }
            fp.StartCoroutine(fp.FlipBoth());
            fp.inside = 0;
            fp.tm.color = Color.white;
            if (fp.flipsLeft == 0)
            {
                fp.flipsLeft = fp.origFlipsLeft;
                fp.bc.size = fp.smallColSize;
                fp.bc.isTrigger = true;
                fp.sr.sprite = fp.s1;
                fp.tm.text = fp.flipsLeft.ToString();
            }
            else
            {
                fp.flipsLeft = 0;
                fp.bc.size = fp.origColSize;
                fp.bc.isTrigger = false;
                fp.sr.sprite = fp.s0;
                fp.tm.text = "";
            }
        }
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    private IEnumerator FlipStart()
    {
        int s = ++flipStateCounter;
        int a = 10;
        while (s == flipStateCounter && a <= 90)
        {
            sr.transform.localEulerAngles = Vector3.up * a;
            a += 10;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private IEnumerator FlipEnd()
    {
        int s = ++flipStateCounter;
        int a = 100;
        while (s == flipStateCounter && a <= 180)
        {
            sr.transform.localEulerAngles = Vector3.up * a;
            a += 10;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private IEnumerator FlipBoth()
    {
        int s = ++flipStateCounter;
        int a = 20;
        while (s == flipStateCounter && a <= 180)
        {
            sr.transform.localEulerAngles = Vector3.up * a;
            a += 20;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private void Start()
    {
        all.Add(this);
        origFlipsLeft = flipsLeft;
        tm.color = Color.white;
        bc = GetComponent<BoxCollider2D>();
        sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        sr.material.SetColor("_RepColor", Utilities.colorCycle[switchId % Utilities.colorCycle.Length]);
        origColSize = bc.size;
        smallColSize = bc.size - new Vector2(4, 4);

        if (switchInverted)
        {
            flipsLeft = 0;
            sr.sprite = s0;
            bc.isTrigger = false;
            tm.text = "";
        }
        else
        {
            bc.size = smallColSize;
            sr.sprite = s1;
            bc.isTrigger = true;
            tm.text = flipsLeft.ToString();
        }
    }

    private IEnumerator FadeTextToWhite()
    {
        tm.color = Color.yellow;
        int flp = flipsLeft;
        int a = 10;
        while (a > 0 && flipsLeft == flp)
        {
            --a;
            tm.color = Color.Lerp(Color.yellow, Color.white, 1f - ((float)a / 10f));
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Rigidbody2D r2 = col.GetComponent<Rigidbody2D>();
        if (!r2 || r2.isKinematic) { return; }
        if (inside == 0)
        {
            bc.size = origColSize;
            --flipsLeft;
            tm.text = flipsLeft.ToString();
            if (flipsLeft == 0)
            {
                tm.color = Color.red;
            }
            else
            {
                StartCoroutine(FadeTextToWhite());
            }
            StartCoroutine(FlipStart());
        }
        ++inside;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        Rigidbody2D r2 = col.GetComponent<Rigidbody2D>();
        if (!r2 || r2.isKinematic) { return; }
        --inside;
        if (inside == 0)
        {
            if (shooter)
            {
                shooter.transform.position = transform.position;
                shooter.Fire();
            }
            if (flipsLeft == 0)
            {
                if (deleteOnZero)
                {
                    GetComponent<GenericBlowMeUp>().BlowMeUp();
                    return;
                }
                bc.isTrigger = false;
                tm.text = "";
                sr.sprite = s0;
            }
            else
            {
                bc.size = smallColSize;
            }
            StartCoroutine(FlipEnd());
        }
    }
}
