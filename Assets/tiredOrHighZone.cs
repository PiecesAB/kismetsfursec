using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tiredOrHighZone : MonoBehaviour
{
    public enum Mode
    {
        Tired, High
    }

    public Mode mode;

    public static HashSet<int> tiredPlrsThisFrame = new HashSet<int>();
    public static HashSet<int> highPlrsThisFrame = new HashSet<int>();

    private void OnTriggerStay2D(Collider2D c)
    {
        
        if (c.gameObject.CompareTag("Player"))
        {
            KHealth kh = c.gameObject.GetComponent<KHealth>();
            if (kh != null)
            {
                int cID = c.gameObject.GetInstanceID();
                if (mode == Mode.Tired && !tiredPlrsThisFrame.Contains(cID))
                {
                    tiredPlrsThisFrame.Add(cID);
                    if (kh.tiredOrHigh >= 0f) { GetComponent<AudioSource>().Play(); }
                    kh.tiredOrHigh = Mathf.Lerp(kh.tiredOrHigh, -1f, 0.005f);
                    kh.tiredOrHigh -= 0.0022222222f;
                }

                if (mode == Mode.High && !highPlrsThisFrame.Contains(cID))
                {
                    highPlrsThisFrame.Add(cID);
                    if (kh.tiredOrHigh <= 0f) { GetComponent<AudioSource>().Play(); }
                    kh.tiredOrHigh = Mathf.Lerp(kh.tiredOrHigh, 1f, 0.005f);
                    kh.tiredOrHigh += 0.0022222222f;
                }
            }
        }
        else
        {
            Physics2D.IgnoreCollision(c, GetComponent<Collider2D>(), true);
        }
    }

    private void Start()
    {
        tiredPlrsThisFrame = new HashSet<int>();
        highPlrsThisFrame = new HashSet<int>();
    }

    private void Update()
    {
        if (tiredPlrsThisFrame.Count > 0) { tiredPlrsThisFrame.Clear(); }
        if (highPlrsThisFrame.Count > 0) { highPlrsThisFrame.Clear(); }
    }
}
