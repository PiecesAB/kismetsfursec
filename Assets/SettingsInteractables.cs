using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class SettingsInteractables : MonoBehaviour {

    public enum Type
    {
        Slider,Switch,Choice,Button,SuperButton,nothing
    }

    

    public Image button;
    public RawImage Switch;
    public Sprite[] buttonImages;
    public GameObject[] objects;
    public string[] choices;
    public float min;
    public float increment;
    public float max;
    public int index = 0;
    public float myOwnValue = 0f;
    public float returnValue = -1f;

    public Encontrolmentation control;
    [HideInInspector]
    public InSaveMenuBase secondaryInterface; // for buttons only.
    public int disabled;
    public Type switchType;

    public AudioSource soundA;
    public AudioSource soundB;

    private bool buttonPressed;

    void Start()
    {
        if (switchType == Type.Choice)
        {
            objects[0].GetComponent<Text>().text = choices[index];
            objects[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            objects[1].GetComponent<Text>().text = "";
            objects[1].GetComponent<RectTransform>().localPosition = new Vector3(32, 0, 0);
            objects[2].GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            RawImage ri = objects[3].GetComponent<RawImage>();
            ri.uvRect = new Rect(0.0625f * index, ri.uvRect.y, ri.uvRect.width, ri.uvRect.height);

        }

        if (switchType == Type.Slider)
        {
            string lol = myOwnValue.ToString("000");
            objects[0].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(2, 1))];
            objects[1].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(1, 1))];
            objects[1].GetComponent<Image>().enabled = (lol.Substring(1, 1) != "0" || lol.Substring(0, 1) != "0");
            objects[2].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(0, 1))];
            objects[2].GetComponent<Image>().enabled = (lol.Substring(0, 1) != "0");
            float aa = (Mathf.InverseLerp(min, max, myOwnValue) * 61f) - 31f;
            Vector3 ab = objects[3].GetComponent<RectTransform>().localPosition;
            objects[3].GetComponent<RectTransform>().localPosition = new Vector3(Mathf.Round(aa), ab.y, ab.z);
        }
    }

    private IEnumerator MoveBoxAndWaitForControlToReturn() // when secondary menu opens
    {
        control.transform.position += new Vector3(0, 1000, 0);
        yield return new WaitForEndOfFrame();
        while (!control.allowUserInput) { yield return new WaitForEndOfFrame(); }
        control.transform.position -= new Vector3(0, 1000, 0);
    }

	void Update ()
    {
	    if (disabled == 0)
        {
            //////////////////////////////////////////////////////
            if (switchType == Type.Button)
            {
                if ((control.currentState & 16UL) != 0UL)
                {
                    returnValue = 1f;
                    button.sprite = buttonImages[1];
                    if (secondaryInterface) {
                        secondaryInterface.Open(control);
                        StartCoroutine(MoveBoxAndWaitForControlToReturn());
                    }
                    if (!buttonPressed) { soundA.Stop(); soundA.Play(); }
                    buttonPressed = true;
                }
                else
                {
                    returnValue = 0f;
                    button.sprite = buttonImages[0];
                    buttonPressed = false;
                }
            }
            ///////////////////////////////////////////////////////
            if (switchType == Type.SuperButton)
            {
                var kx = control.eventsTable[control.eventsTable.Count - 1].Item1;
                var ky = control.eventsTable[control.eventsTable.Count - 1].Item2;
                if ((control.currentState & 16UL) != 0UL && (ky == control.currentState) )
                {
                    button.sprite = buttonImages[Mathf.Min(Mathf.CeilToInt((float)(DoubleTime.UnscaledTimeSinceLoad - kx)),4)];
                    returnValue = 0f;
                    if (DoubleTime.UnscaledTimeSinceLoad - kx >= 3f)
                    {
                        returnValue = 1f;
                        if (!buttonPressed) { soundA.Stop(); soundA.Play(); }
                        buttonPressed = true;
                    }
                }
                else
                {
                    returnValue = 0f;
                    button.sprite = buttonImages[0];
                    buttonPressed = false;
                }
            }
            //////////////////////////////////////////////////////
            if (switchType == Type.Switch)
            {
                if ((control.currentState & 16UL) != 0UL && (control.flags & 240UL) != 0UL) //ABXY
                {
                    returnValue = 1f - returnValue;
                    if (returnValue == 0f)
                    {
                        soundA.Stop(); soundA.Play();
                    }
                    else
                    {
                        soundB.Stop(); soundB.Play();
                    }
                }
                else
                {
                    if ((control.currentState & 3UL) == 1UL && (control.flags & 3UL) == 1UL && returnValue == 1f) //Left
                    {
                        soundA.Stop(); soundA.Play();
                        returnValue = 0f;
                    }
                    if ((control.currentState & 3UL) == 2UL && (control.flags & 3UL) == 2UL && returnValue == 0f) //Right
                    {
                        soundB.Stop(); soundB.Play();
                        returnValue = 1f;
                    }
                }
                if (returnValue == 1f)
                {
                    myOwnValue = Mathf.Max(myOwnValue - 0.3f, 0f);
                }
                else
                {
                    myOwnValue = Mathf.Min(myOwnValue + 0.3f, 3f);
                }

                Switch.uvRect = new Rect(myOwnValue / 7f, Switch.uvRect.y, Switch.uvRect.width, Switch.uvRect.height);

            }
            //////////////////////////////////////////////////////
            if (switchType == Type.Choice)
                //objects 0 = Left text
                //objects 1 = Right text
                //objects 2 = Container of the texts
                //objects 3 = RawImage backdrop
            {

                float zz = objects[2].GetComponent<RectTransform>().localPosition.x;
                objects[2].GetComponent<RectTransform>().localPosition = new Vector3(Mathf.MoveTowards(objects[2].GetComponent<RectTransform>().localPosition.x,0,3.2f), 0, 0);
                    RawImage ri = objects[3].GetComponent<RawImage>();
                if (zz != 0f)
                {
                    ri.uvRect = new Rect(ri.uvRect.x + (Mathf.Sign(zz) * 0.00625f), ri.uvRect.y, ri.uvRect.width, ri.uvRect.height);
                }
                else
                {
                    myOwnValue = 1f;
                }

                if ((control.currentState & 3UL) == 1UL && (control.flags & 3UL) == 1UL && myOwnValue==1f) //Left
                {
                    myOwnValue = 0f;
                    int ib = index;
                    index--;
                    if (index < 0)
                    {
                        index = choices.Length-1;
                    }
                    soundA.Stop(); soundA.Play();
                    objects[0].GetComponent<Text>().text = choices[index];
                    objects[0].GetComponent<RectTransform>().localPosition = new Vector3(0f, 0, 0);
                    objects[1].GetComponent<Text>().text = choices[ib];
                    objects[1].GetComponent<RectTransform>().localPosition = new Vector3(32f, 0, 0);
                    objects[2].GetComponent<RectTransform>().localPosition = new Vector3(-32f, 0, 0);
                }
                if ((control.currentState & 3UL) == 2UL && (control.flags & 3UL) == 2UL && myOwnValue == 1f) //Left
                {
                    myOwnValue = 0f;
                    int ib = index;
                    index++;
                    if (index >= choices.Length)
                    {
                        index = 0;
                    }
                    soundB.Stop(); soundB.Play();
                    objects[0].GetComponent<Text>().text = choices[index];
                    objects[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    objects[1].GetComponent<Text>().text = choices[ib];
                    objects[1].GetComponent<RectTransform>().localPosition = new Vector3(-32f, 0, 0);
                    objects[2].GetComponent<RectTransform>().localPosition = new Vector3(32f, 0, 0);
                }
            }

            if (switchType == Type.Slider)
            //objects 0 = Ones
            //objects 1 = Tens
            //objects 2 = Hundreds
            //objects 3 = Mark
            {

                if (control.ButtonDown(1UL, 3UL)) //Left
                {
                    myOwnValue = Mathf.Max(myOwnValue - increment, min);
                    soundA.Stop(); soundA.Play();
                }
                if (control.ButtonDown(2UL, 3UL)) //Left
                {
                    myOwnValue = Mathf.Min(myOwnValue + increment, max);
                    soundB.Stop(); soundB.Play();
                }

                double kz = 0.0;
                if (control.ButtonHeld(1UL, 3UL, 0.25f, out kz)) //Left
                {
                    myOwnValue = Mathf.Max(myOwnValue - Mathf.Ceil(((float)kz)*0.8f/increment)*increment*increment, min);
                    soundA.Stop(); soundA.Play(); soundA.time = 0.05f;
                }
                if (control.ButtonHeld(2UL, 3UL, 0.25f, out kz)) //Right
                {
                    myOwnValue = Mathf.Min(myOwnValue + Mathf.Ceil(((float)kz) * 0.8f / increment) * increment * increment, max);
                    soundB.Stop(); soundB.Play();
                }

                string lol = myOwnValue.ToString("000");
                objects[0].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(2, 1))];
                objects[1].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(1, 1))];
                objects[1].GetComponent<Image>().enabled = (lol.Substring(1, 1) != "0" || lol.Substring(0, 1) != "0");
                objects[2].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(0, 1))];
                objects[2].GetComponent<Image>().enabled = (lol.Substring(0, 1) != "0");
                float aa = (Mathf.InverseLerp(min, max, myOwnValue)*61f)-31f;
                Vector3 ab = objects[3].GetComponent<RectTransform>().localPosition;
                objects[3].GetComponent<RectTransform>().localPosition = new Vector3(Mathf.Round(aa), ab.y, ab.z);
            }

        }
        else
        {
            //////////////////////////////////////////////////////
            if (switchType == Type.Button || switchType == Type.SuperButton)
            {
                returnValue = 0f;
                button.sprite = buttonImages[0];
            }

            //////////////////////////////////////////////////////
            if (switchType == Type.Switch)
            {
                /*if (returnValue == 1f)
                {
                    myOwnValue = Mathf.Min(myOwnValue + 0.4f, 3f);
                }
                else
                {
                    myOwnValue = Mathf.Max(myOwnValue - 0.4f, 0f);
                }*/
                myOwnValue = (1-returnValue) * 3f;
                Switch.uvRect = new Rect(myOwnValue / 7f, Switch.uvRect.y, Switch.uvRect.width, Switch.uvRect.height);
            }

            //////////////////////////////////////////////////////
            if (switchType == Type.Choice)
            {
                objects[0].GetComponent<Text>().text = choices[index];
                objects[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                objects[1].GetComponent<Text>().text = "";
                objects[1].GetComponent<RectTransform>().localPosition = new Vector3(32, 0, 0);
                objects[2].GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                RawImage ri = objects[3].GetComponent<RawImage>();
                ri.uvRect = new Rect(0.0625f * index, ri.uvRect.y, ri.uvRect.width, ri.uvRect.height);

            }

            if (switchType == Type.Slider)
            {
                string lol = myOwnValue.ToString("000");
                objects[0].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(2, 1))];
                objects[1].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(1, 1))];
                objects[1].GetComponent<Image>().enabled = (lol.Substring(1, 1) != "0" || lol.Substring(0, 1) != "0");
                objects[2].GetComponent<Image>().sprite = buttonImages[int.Parse(lol.Substring(0, 1))];
                objects[2].GetComponent<Image>().enabled = (lol.Substring(0, 1) != "0");
                float aa = (Mathf.InverseLerp(min, max, myOwnValue) * 61f) - 31f;
                Vector3 ab = objects[3].GetComponent<RectTransform>().localPosition;
                objects[3].GetComponent<RectTransform>().localPosition = new Vector3(Mathf.Round(aa), ab.y, ab.z);
            }
        }
	}
}
