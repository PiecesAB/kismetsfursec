using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChoiceUIBox : MonoBehaviour {

    public enum SpecialBehavior
    {
        None, LimitedMovement
    }

    public Encontrolmentation.ActionButton[] controls;
    public sbyte selected;
    public sbyte confirmedSelected;
    public RectTransform selectLight;
    public GameObject[] choiceTexts;
    public MainTextsStuff mainText;
    public string[] choiceMsgs;
    public MainTextsStuff.ObjectsWorkaround[] returnObjs;
    public GameObject[] noResponseObjs;
    public Sprite[] buttonSprites;
    public string[] outputs;
    public string noResponseOutput;
    public double time;
    public float origTimeScale;
    public bool test;
    public bool finished;
    public Text timer;
    public bool connectedToDialog = true;
    public bool forMenu;
    public SpecialBehavior specialBehavior = SpecialBehavior.None;

    private float leaveVelocity = 0f;

    public bool timeShow;

    public AudioSource openSound;
    public AudioSource highlightSound;
    public AudioSource selectSound;

	void Start () {
	if (test)
        {
            Init();
        }
	}

    public void Init()
    {

        selected = -1;
        confirmedSelected = -1;
        Vector3 sll = selectLight.anchoredPosition;
        selectLight.anchoredPosition = new Vector3(sll.x, 1000000f, sll.z); //this is really really far away
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, 12);
        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (i >= controls.Length)
            {
                choiceTexts[i].SetActive(false);
            }
            else
            {
                choiceTexts[i].transform.GetChild(0).GetComponent<Image>().sprite = buttonSprites[System.Convert.ToInt32(controls[i]) - 1];
                choiceTexts[i].GetComponent<Text>().text = choiceMsgs[i];
                choiceTexts[i].GetComponent<MainTextsStuff>().Begin();
            }
        }
        if (time <= 0)
        {
            time = Mathf.Infinity;
        }
        else
        {
            time += DoubleTime.UnscaledTimeRunning;
        }

        openSound.Play();
    }

    private void MakeTextbox(GameObject output2)
    {
        GameObject ne = Instantiate(output2, Vector3.zero, Quaternion.identity) as GameObject;
        ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
        ne.transform.Find("ActualText").GetComponent<TextBoxGiverHandler>().origTimeScale = origTimeScale;
        ne.transform.Find("ActualText").GetComponent<TextBoxGiverHandler>().choiceObjOld = gameObject;
        ne.transform.Find("ActualText").GetComponent<TextBoxGiverHandler>().dialogObjOld = transform.parent.gameObject;
        ne.SetActive(true);
    }

    public void DoResponse(string text)
    {
        if (timer) { Destroy(timer.gameObject); }

        List<GameObject> outputTextboxes = new List<GameObject>();

        List<GameObject> addedObjectsFromUIResponse = new List<GameObject>();
        foreach (GameObject output in returnObjs[confirmedSelected].objects)
        {
            if (output == null) { continue; }
            Component[] comps = output.GetComponents(typeof(IChoiceUIResponse));
            for (int c = 0; c < comps.Length; ++c)
            {
               addedObjectsFromUIResponse.Add(((IChoiceUIResponse)comps[c]).ChoiceResponse(text));
            }
        }

        List<GameObject> totalObjects = new List<GameObject>(returnObjs[confirmedSelected].objects);
        totalObjects.AddRange(addedObjectsFromUIResponse);

        foreach (GameObject output in totalObjects)
        {
            GameObject output2 = output;
            if (output2 == null) { continue; }

            if (output2.GetComponent<PrimRandomChooser>() != null)
            {
                output2 = output2.GetComponent<PrimRandomChooser>().Choose();
            }

            if (output2.CompareTag("TextBox"))
            {
                outputTextboxes.Add(output2);
            }
            
            if (output2.GetComponent<Animator>() != null)
            {
                output2.GetComponent<Animator>().SetTrigger(text);
            }
        }

        if (outputTextboxes.Count >= 1)
        {
            if (outputTextboxes.Count == 1)
            {
                MakeTextbox(outputTextboxes[0]);
            }
            else
            {
                // choose random text
                MakeTextbox(outputTextboxes[Fakerand.Int(0, outputTextboxes.Count)]);
            }
        }
    }

    public void Finish(bool noResponse)
    {
        selectSound.Play();

        if (connectedToDialog)
        {
            transform.parent.GetComponent<Image>().enabled = false;
            if (mainText)
            {
                mainText.lastVertices.Clear();
            }
        }

        if (specialBehavior == SpecialBehavior.LimitedMovement)
        {
            KHealth kh = FindObjectOfType<KHealth>();
            if (kh)
            {
                if (outputs[confirmedSelected] == "yes")
                {
                    Utilities.ChangeScore(-kh.limitedMovementCost);
                    kh.limitedMovement += 393.7f;
                    kh.limitedMovementCost *= 2;
                    kh.limitedMovementCost = Mathf.Clamp(kh.limitedMovementCost, 1, 1000000000);
                }
                else
                {
                    kh.ChangeHealth(Mathf.NegativeInfinity, "no more movement", true);
                }
            }
        }

        if (!noResponse)
        {
            DoResponse(outputs[confirmedSelected]);
        }
        else
        {
            DoResponse(noResponseOutput);
            for (int i = 0; i < choiceTexts.Length; i++)
            {
                choiceTexts[i].GetComponent<Text>().text = "(No response)";
            }
        }

    }
	
	void Update () {
        if (!timeShow && timer)
        {
            Destroy(timer.gameObject);
            timer = null;
        }

        if (timer)
        {
            if (time - DoubleTime.UnscaledTimeRunning < 31)
            {
                timer.text = "" + Mathf.Floor((float)(time - DoubleTime.UnscaledTimeRunning));
                timer.color = Color.white;
            }

            if (time - DoubleTime.UnscaledTimeRunning >= 31 || confirmedSelected >= 0)
            {
                timer.color = Color.clear;
            }
        }
        Encontrolmentation e = GetComponent<Encontrolmentation>();

        if (confirmedSelected == -1 && DoubleTime.UnscaledTimeRunning < time)
        {
            byte buttonsPressed = 0;
            sbyte wouldSelect = -1;
            for (int i = 0; i < controls.Length; i++)
            {
                ulong ab = Utilities.ActButtonToUL[controls[i]];
                if ((e.flags & ab) == ab && (e.currentState & ab) == ab) //ab is just pushed
                {
                    wouldSelect = (sbyte)i;
                    buttonsPressed++;
                }
            }

            sbyte oldSel = selected;
            
            if (buttonsPressed == 1 && wouldSelect >= 0)
            {
                if (selected != wouldSelect)
                {
                    selected = wouldSelect;
                    highlightSound.Stop(); highlightSound.Play();
                }
                else
                {
                    confirmedSelected = wouldSelect;
                    for (int i = 0; i < choiceTexts.Length; i++)
                    {
                        if (i != confirmedSelected)
                        {
                            choiceTexts[i].SetActive(false);
                        }
                        
                    }

                    Finish(false);
                }
            }

            float nn = Mathf.Lerp(GetComponent<RectTransform>().sizeDelta.y, (1 + controls.Length) * 16f, 0.5f);

            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, nn);

            if (oldSel == -1 && selected >= 0)
            {
                Vector3 sll = selectLight.anchoredPosition;
                selectLight.anchoredPosition = new Vector3(sll.x, choiceTexts[selected].GetComponent<RectTransform>().anchoredPosition.y, sll.z);
            }
            
            if (oldSel >= 0)
            {
                Vector3 sll = selectLight.anchoredPosition;
                selectLight.anchoredPosition = new Vector3(sll.x, Mathf.Lerp(sll.y, choiceTexts[selected].GetComponent<RectTransform>().anchoredPosition.y, 0.5f), sll.z);
            }

        }
        else
        {
            if (DoubleTime.UnscaledTimeRunning > time && confirmedSelected == -1) //out of time
            {
                confirmedSelected = 0;
                time = 0;
                Finish(true);
            }
            else
            {
                Vector3 vs = choiceTexts[confirmedSelected].GetComponent<RectTransform>().anchoredPosition;
                choiceTexts[confirmedSelected].GetComponent<RectTransform>().anchoredPosition = new Vector3(vs.x, Mathf.Lerp(vs.y, -16f, 0.5f), vs.z);
                Vector3 sll = selectLight.anchoredPosition;
                selectLight.anchoredPosition = new Vector3(sll.x, Mathf.Lerp(sll.y, -16f, 0.5f), sll.z);
                GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, Mathf.Lerp(GetComponent<RectTransform>().sizeDelta.y, 32f, 0.5f));

                GetComponent<RectTransform>().localPosition += Vector3.right * Mathf.Sign(GetComponent<RectTransform>().localPosition.y) * leaveVelocity;
                leaveVelocity += 4f;

                if (leaveVelocity >= 100f)
                {
                    Destroy(gameObject);
                }
            }
        }



    }

}
