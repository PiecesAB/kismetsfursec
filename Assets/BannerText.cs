using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BannerText : MonoBehaviour {

    public List<string> texts = new List<string>() { };
    public RectTransform sampleTextObject;
    public RectTransform currentTextObject;
    public bool showing;

    public ActualSpeaking speaker;

    private Vector2 origPos;
    private Vector2 idealPos;
    //private int colorCount;

    void Start()
    {
        origPos = idealPos = GetComponent<RectTransform>().anchoredPosition;
        //colorCount = 20;
    }

    void Update () {
        if (Time.timeScale != 0)
        {
            Encontrolmentation e = GetComponent<Encontrolmentation>();
               float speed = -1f;
            if ((e.currentState & 12288UL) == 4096UL) //L2
            {
                speed = -3f;
            }
            if ((e.currentState & 12288UL) == 8192UL) //R2
            {
                speed = 3f;
            }
            double whatever1 = 0f;
            if (e.ButtonHeld(12288UL,12288UL,0.9f,out whatever1) && texts.Count > 0) //L2+R2 held for 1 second
            {
                texts = new List<string>() { texts[0] };
                speed = Mathf.NegativeInfinity;
            }
            /*colorCount--;
            if (colorCount == 0)
            {
                colorCount = 20;
                GetComponent<Image>().color = Color.Lerp(Fakerand.Color(240), Color.black, 0.3f);
            }*/
            if (texts.Count == 0)
            {
                showing = false;
                Vector2 s = idealPos;
                idealPos = Vector2.Lerp(s, origPos,0.1f);   
            }
            else
            {
                showing = true;

                if (currentTextObject == null)
                {

                    GameObject nt = Instantiate(sampleTextObject.gameObject, transform);
                    nt.GetComponent<Text>().text = texts[0];
                    currentTextObject = nt.GetComponent<RectTransform>();
                    if (speaker != null)
                    {
                        string whatToSay = "";
                        MainTextsStuff.PrepareTheFormatting(texts[0], out whatToSay);

                        //StartCoroutine(speaker.Say(whatToSay)); // annoying
                    }
                }
                else
                {
                    currentTextObject.anchoredPosition = new Vector2(Mathf.Min(speed+ currentTextObject.anchoredPosition.x,GetComponent<RectTransform>().sizeDelta.x), currentTextObject.anchoredPosition.y);

                    if (currentTextObject.anchoredPosition.x <= -currentTextObject.GetComponent<Text>().preferredWidth)
                    {
                        texts.RemoveAt(0);
                        Destroy(currentTextObject.gameObject);
                        currentTextObject = null;
                    }
                }


                Vector2 s = idealPos;
                idealPos = Vector2.Lerp(s, origPos-new Vector2(0f,16f), 0.1f);
            }
            GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Round(idealPos.x), Mathf.Round(idealPos.y));
        }
	}
}
