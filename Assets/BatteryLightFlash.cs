using UnityEngine;
using System.Collections;

public class BatteryLightFlash : MonoBehaviour {

    public float speed;
    public bool inverse;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float hecc = (float)System.Math.Cos(DoubleTime.ScaledTimeSinceLoad * speed);
        if (GetComponent<SpriteRenderer>() != null)
        {
            if ((hecc > 0f && !inverse) || (hecc < 0f && inverse))
            {
                Color c = GetComponent<SpriteRenderer>().color;
                GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, Mathf.Max(c.a - 0.4f, 0f));
            }
            else
            {
                Color c = GetComponent<SpriteRenderer>().color;
                GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 1f, 0.3f));
            }
        }

        if (GetComponent<TextMesh>() != null)
        {
            if ((hecc > 0f && !inverse) || (hecc < 0f && inverse))
            {
                Color c = GetComponent<TextMesh>().color;
                GetComponent<TextMesh>().color = new Color(c.r, c.g, c.b, Mathf.Max(c.a - 0.4f, 0f));
            }
            else
            {
                Color c = GetComponent<TextMesh>().color;
                GetComponent<TextMesh>().color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 1f, 0.3f));
            }
        }

    }
}
