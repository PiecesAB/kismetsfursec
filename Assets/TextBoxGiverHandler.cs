using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TextBoxGiverHandler : MonoBehaviour {

    public enum SpecialBehavior
    {
        None, LimitedMovementPoor, Dummy
    }

    public float delayOpenTime;
    public const float fadeOutTime = 0f;
    private MainTextsStuff a;
    public bool doesThisFreezeTime = true;
    private bool dying;
    private double deathStartTime;
    
    public float origTimeScale = 1f;
    public GameObject choiceObjOld;
    public GameObject dialogObjOld;
    public GameObject[] onDeactivateObjects;
    public GameObject mainSpeaker;
    public SpecialBehavior specialBehavior = SpecialBehavior.None;

    public GameObject alternateBoxIfFailedCondition;
    public GameObject nextBoxInSequence;

    private static GameObject narrator;

    public IEnumerator Lol()
    {
        if (delayOpenTime > 0) // for some reason, it can get stuck trying to wait 0 seconds when time scale is 0. this caused a softlock for checkpoints. 
        {
            yield return new WaitForSeconds(delayOpenTime);
        }

        a = gameObject.GetComponent<MainTextsStuff>();
        //print(a);
        dying = false;
        if (doesThisFreezeTime && Time.timeScale > 0 && !a.moveToBanner)
        {
            origTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }
        Transform sp = transform.parent.parent.parent.Find("Stats");
        if (sp != null && sp.gameObject != null)
        {
            sp.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-1000,-104,0) ;
        }
        if (choiceObjOld != null)
        {
            choiceObjOld.transform.SetParent(transform.parent, false);
        }
        if (dialogObjOld != null)
        {
            Destroy(dialogObjOld);
        }
        
        // create narrator
        if (!narrator) { narrator = Resources.Load<GameObject>("Narrator"); }
        GameObject myNarrator = Instantiate(narrator, transform);
        ActualSpeaking mnas = myNarrator.GetComponent<ActualSpeaking>();
        mnas.SetVoiceFromBoxImage(GetComponentInParent<Image>());
        if (a)
        {
            a.narrator = mnas;
            a.Begin();
        }
    }

    Vector3 CalcPos()
    {
        if (mainSpeaker)
        {
            if (mainSpeaker.transform.position.y - Camera.main.transform.position.y < -32f)
            {
                return new Vector3(0, 76, 0);
            }
        }

        return new Vector3(0, -76, 0);
    }

	void Start () {
        try
        {
            if (transform.parent.parent.gameObject.CompareTag("DialogueArea"))
            {
                bool passedConditions = true;
                foreach (InGameConditional c in transform.parent.GetComponents<InGameConditional>())
                {
                    if (!c.Evaluate()) { passedConditions = false; break; }
                }
                if (!passedConditions)
                {
                    if (!alternateBoxIfFailedCondition) { throw new Exception("No alternate dialog for failed conditional"); }
                    transform.parent.parent = null;
                    transform.parent.position = new Vector3(999999, 999999, 0);
                    SpawnNewBox(ref alternateBoxIfFailedCondition, mainSpeaker);
                    Destroy(transform.parent.gameObject);
                    return;
                }
                transform.parent.localPosition = CalcPos();
                transform.parent.gameObject.GetComponent<Image>().material.SetFloat("_Progress", 0);
                transform.parent.gameObject.GetComponent<Image>().color = Color.white;
                StartCoroutine(Lol());
            }
        }
        catch
        {
            //you are bad at code!!!
        }
        
	}

    private static GameObject dialogArea = null;

    public static GameObject SpawnNewBox(ref GameObject sample, GameObject mainSpeaker)
    {
        if (dialogArea == null) { dialogArea = GameObject.FindGameObjectWithTag("DialogueArea"); }
        if (Time.timeScale == 0 || dialogArea.transform.childCount > 0) { return null; }
        GameObject made = Instantiate(sample);
        // if main speaker is given, it will be set; otherwise keep the one already in the sample, if possible
        if (mainSpeaker) { made.GetComponentInChildren<TextBoxGiverHandler>().mainSpeaker = mainSpeaker; }
        made.SetActive(true);
        made.transform.SetParent(dialogArea.transform, false);
        return made;
    }
	
	// Update is called zeroce per frame???? Fix that
	void Update () {
	    if ((!dying && (a && a.finishFlag)) || specialBehavior == SpecialBehavior.Dummy)
        {
            dying = true;
            deathStartTime = DoubleTime.UnscaledTimeRunning;
            GetComponent<Text>().text = "";
            if (choiceObjOld != null)
            {
                Destroy(choiceObjOld);
            }
        }

        if (specialBehavior == SpecialBehavior.Dummy)
        {
            dying = true;
            deathStartTime = DoubleTime.UnscaledTimeRunning;
            GetComponent<Text>().text = "";
            if (choiceObjOld != null)
            {
                Destroy(choiceObjOld);
            }
        }

        if (dying)
        {
            double diff = DoubleTime.UnscaledTimeRunning - deathStartTime; //have fun with this
            double prog = (fadeOutTime == 0)?(2.0):(diff / fadeOutTime);
            if (prog >= 1)
            {
                if (specialBehavior == SpecialBehavior.LimitedMovementPoor)
                {
                    KHealth kh = LevelInfoContainer.GetActiveControl().GetComponent<KHealth>();
                    if (kh) { kh.ChangeHealth(Mathf.NegativeInfinity, "no more movement", true); }
                }

                if (!GetComponent<MainTextsStuff>().choice || GetComponent<MainTextsStuff>().choiceButtons.Length == 0)
                {
                    Transform sp = transform.parent.parent.parent.Find("Stats");
                    if (sp != null && sp.gameObject != null)
                    {
                        transform.parent.parent.parent.Find("Stats").gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, -104, 0);
                    }
                    transform.parent.gameObject.GetComponent<Image>().material.SetFloat("_Progress", 0);
                    if (Utilities.lastCheatCode != "")
                    {
                        Utilities.lastCheatCode = "";
                    }
                    if (doesThisFreezeTime && a != null && !a.moveToBanner)
                    {
                        Time.timeScale = origTimeScale;
                    }

                    foreach (GameObject go in onDeactivateObjects)
                    {
                        foreach (var tbd in go.GetComponents<ITextBoxDeactivate>()) { tbd.OnTextBoxDeactivate(); }
                    }

                    if (nextBoxInSequence)
                    {
                        transform.parent.parent = null;
                        transform.parent.position = new Vector3(999999, 999999, 0);
                        SpawnNewBox(ref nextBoxInSequence, null);
                    }
                    Destroy(transform.parent.gameObject);
                }
                else
                {

                }
            }
            else
            {
                transform.parent.gameObject.GetComponent<Image>().material.SetFloat("_Progress", (float)prog);
            }
        }

    }
}
