using UnityEngine;
using System.Collections;

public class PrimSprAlt : MonoBehaviour {

    public enum Pattern
    {
        Custom,Sprint,
    }

    public Pattern pattern;
    private float t = 0;
    private int i = 0;
    public bool currentState;
    public Sprite[] sprites;
    public bool flipXNotSprChange;
    public float[] times;
    public AudioSource soundIfAvailable;

	// Use this for initialization
	void Start () {
	    if (pattern == Pattern.Sprint)
        {
            times = new float[] { 1.2f, Fakerand.Single(0.1f, 0.3f), Fakerand.Single(0.1f, 0.3f), Fakerand.Single(0.1f, 0.3f), Fakerand.Single(0.1f, 0.3f), Fakerand.Single(0.1f, 0.3f), Fakerand.Single(0.1f, 0.3f), Fakerand.Single(0.1f, 0.3f) };
        }
        i = 0;
        t = times[0];
	}
	
	// Update is called once per frame
	void Update () {

	    if (DoubleTime.ScaledTimeSinceLoad-t > times[i])
        {
            currentState = !currentState;
            i = (i + 1) % times.Length;
            t += times[i];
        }

        if (GetComponent<SpriteRenderer>() != null)
        {
            if (flipXNotSprChange)
            {
                GetComponent<SpriteRenderer>().flipX = currentState;
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = sprites[currentState ? 1 : 0];
            }
        }
	}
}
