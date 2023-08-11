using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeWarnerGUI : MonoBehaviour {

    public Sprite[] digitSymbols;
    public Image ones;
    public Image tenths;
    public Image hundreths;

    private Vector3 startLocPos;

    public static TimeWarnerGUI main;

    private void Start()
    {
        startLocPos = GetComponent<RectTransform>().localPosition;
        main = this;
    }

    private void Update () {
	
        if ((Time.timeScale > 0.99f && Time.timeScale < 1.01f) || (Time.timeScale < 0.01f))
        {
            GetComponent<RectTransform>().localPosition = new Vector3(-9999, -9999, -9999);
        }
        else
        {
            //GetComponent<RectTransform>().localPosition = new Vector3(-320, 400, 0);
            GetComponent<RectTransform>().localPosition = startLocPos;

            if (Time.timeScale > 1.4f)
            {
                foreach (Transform ch in transform)
                {
                    ch.GetComponent<Image>().color = Color.Lerp(new Color(0.9f, 0.9f, 0.9f), new Color(0.9f, 0.3f, 0.3f), (Time.timeScale - 1.4f) / 2.6f);
                }
            }
            else if (Time.timeScale < 0.7f)
            {
                foreach (Transform ch in transform)
                {
                    ch.GetComponent<Image>().color = Color.Lerp(new Color(0.9f, 0.9f, 0.9f), new Color(0f, 0f, 0.9f), (0.7f-Time.timeScale));
                }
            }
            else
            {
                foreach (Transform ch in transform)
                {
                    ch.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);
                }
            }

            string numf = Mathf.Min(Time.timeScale,9.99f).ToString("0.00");

            ones.sprite = digitSymbols[int.Parse(numf.Substring(0, 1))];
            tenths.sprite = digitSymbols[int.Parse(numf.Substring(2, 1))];
            hundreths.sprite = digitSymbols[int.Parse(numf.Substring(3, 1))];
        }

	}
}
