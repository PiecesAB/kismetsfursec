using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;

/*public abstract class ModifyMeshes : MonoBehaviour, IMeshModifier
{
    public abstract void ModifyMesh(Mesh meshText);
    public abstract void ModifyMesh(VertexHelper vh);
}*/

[ExecuteInEditMode]
public class MainTextsStuff : ModifyMeshes, ControlsMainCS.IMainActions
{

    [Serializable]
    public struct MessageData
    {
        [Multiline]
        public string text;
        public bool noFormatting;
        public float letterWait;
        public Color highlightColor;
        public Color defaultColor;
        [HideInInspector]
        //wait time doesn't function anymore.
        public float messageFinishedWait;
        public float effectSpeed;
        public bool skippable;
        [Header("noneuclidean is true if it's not interactive dialog")]
        public bool noneuclidean;
        public string animationTrigger;

        public MessageData(string text_)
        {
            text = text_;
            noFormatting = false;
            letterWait = 0.03f;
            messageFinishedWait = 1f; //rudimentary because message length is constant by length now.
            highlightColor = Color.red;
            defaultColor = Color.white;
            effectSpeed = 1f;
            skippable = true;
            noneuclidean = false;
            animationTrigger = "";
        }

        public MessageData(string text_, float messageFinishedWait_)
        {
            text = text_;
            noFormatting = false;
            letterWait = 0.03f;
            messageFinishedWait = messageFinishedWait_;
            highlightColor = Color.red;
            defaultColor = Color.white;
            effectSpeed = 1f;
            skippable = true;
            noneuclidean = false;
            animationTrigger = "";
        }

        public MessageData(string text_, float messageFinishedWait_, Color _defaultColor, float _letterWait = 0.03f)
        {
            text = text_;
            noFormatting = false;
            letterWait = _letterWait;
            messageFinishedWait = messageFinishedWait_;
            highlightColor = Color.red;
            defaultColor = _defaultColor;
            effectSpeed = 1f;
            skippable = true;
            noneuclidean = false;
            animationTrigger = "";
        }

        public MessageData(string text_, bool noFormatting_, float messageFinishedWait_)
        {
            text = text_;
            noFormatting = noFormatting_;
            letterWait = 0.03f;
            messageFinishedWait = messageFinishedWait_;
            highlightColor = Color.red;
            defaultColor = Color.white;
            effectSpeed = 1f;
            skippable = true;
            noneuclidean = false;
            animationTrigger = "";
        }

        public MessageData(string text_, float letter_duration, float messageFinishedWait_)
        {
            text = text_;
            noFormatting = false;
            letterWait = letter_duration;
            messageFinishedWait = messageFinishedWait_;
            highlightColor = Color.red;
            defaultColor = Color.white;
            effectSpeed = 1f;
            skippable = true;
            noneuclidean = false;
            animationTrigger = "";
        }

        public MessageData(string text_,bool noFormatting_,float letter_duration, float messageFinishedWait_)
        {
            text = text_;
            noFormatting = noFormatting_;
            letterWait = letter_duration;
            messageFinishedWait = messageFinishedWait_;
            highlightColor = Color.red;
            defaultColor = Color.white;
            effectSpeed = 1f;
            skippable = true;
            noneuclidean = false;
            animationTrigger = "";
        }

        public MessageData(string text_, bool noFormatting_, float letter_duration, float messageFinishedWait_, float fxSpeed)
        {
            text = text_;
            noFormatting = noFormatting_;
            letterWait = letter_duration;
            messageFinishedWait = messageFinishedWait_;
            highlightColor = Color.red;
            defaultColor = Color.white;
            effectSpeed = fxSpeed;
            skippable = true;
            noneuclidean = false;
            animationTrigger = "";
        }

        public MessageData(string text_, bool noFormatting_, float letter_duration, float messageFinishedWait_, float fxSpeed, string animTrigger)
        {
            text = text_;
            noFormatting = noFormatting_;
            letterWait = letter_duration;
            messageFinishedWait = messageFinishedWait_;
            highlightColor = Color.red;
            defaultColor = Color.white;
            effectSpeed = fxSpeed;
            skippable = true;
            noneuclidean = false;
            animationTrigger = animTrigger;
        }
    }

    [Serializable]
    public struct ObjectsWorkaround
    {
        public GameObject[] objects;
    }


    private bool readyToCapture = true;
    private bool readyToDisplay = false;
    public List<MessageData> messages = new List<MessageData>() {new MessageData("When I say \"go\", get ready to mod",3)};
    VertexHelper lastTextMesh = new VertexHelper();
    public List<UIVertex> lastVertices = new List<UIVertex>();
    public Dictionary<int, uint> formatData = new Dictionary<int, uint>();
    private bool stopUpdating;
    private double timeWhenMsgStart = 0;
    private int currentIndex = 0;
    public int currentMessage = 0;
    public bool finishFlag = false;
    public bool startsByItself = false;
    public Animator mainAnimator;
    public bool instantAnimationChange = false;
    public bool choice;
    public bool moveToBanner;
    public Encontrolmentation.ActionButton[] choiceButtons;
    public string[] choiceDisplays;
    public ObjectsWorkaround[] choiceOutcomeObjects;
    public GameObject[] choiceNoRespObjs;
    public string[] choiceOutputs;
    public string choiceNoRespOutput;
    public float choiceTime;
    public static int insertableIntValue1;
    public static string insertableStringValue1;

    private bool debounce = true;
    private bool skip = false;
    private bool specialTextDoneCheck;
    private bool accelerating = false;
    private static GameObject textDoneAnimThing;

    public static float speedMultiplier = 1f;

    public ActualSpeaking narrator { private get; set; }

    [SerializeField]
    private bool noneuclideanRefresh = false;

    public static Dictionary<string, int> tags = new Dictionary<string, int>()
    {
        {"shake",0},
        {"wave",1},
        {"rainbow",2},
        {"wait",3},
        {"longwait",4},
        {"!",5},
        {"flash",6},
        {"obfus",7},
    };

    public Coroutine WaitForRealSeconds(float time)
    {
        return StartCoroutine(WaitForRealSecondsImpl(time));
    }

    private IEnumerator WaitForRealSecondsImpl(float time)
    {
        double startTime = DoubleTime.UnscaledTimeRunning;
        while (DoubleTime.UnscaledTimeRunning - startTime < time)
            yield return 1;
    }

    private float MsgWaitTime()//int currIdx)
    {
        return modText.Length * 0.075f + 1f;
    }

    private string modText = "";

    public static Dictionary<int,uint> PrepareTheFormatting(string text, out string modifiedText)
    {
        Dictionary<int, uint> newFormatData = new Dictionary<int, uint>();
        modifiedText = text;
        uint currentState = 0;

        int cl = Utilities.CalculateLevel();
        text = modifiedText = text.Replace("<flag text>", Utilities.secretFlagTexts[Fakerand.Int(0, Utilities.secretFlagTexts.Length)]);
        text = modifiedText = text.Replace("<special rank text>", "Rank up! Your ordinal is <rank>"); //Utilities.specialRankTexts[cl]);
        text = modifiedText = text.Replace("<special rank text 2>", "<rainbow>Score makes it easier to live, and you get a larger inventory. Be glorious and transfinite!"); //Utilities.specialRankTexts2[cl]);
        //text = modifiedText = text.Replace("<used item>", InventoryItemsNew.dialogMessage);
        text = modifiedText = text.Replace("<rank>", Utilities.longLevelNames(cl));
        text = modifiedText = text.Replace("<cheat>", Utilities.lastCheatCode);
        text = modifiedText = text.Replace("<int1>", insertableIntValue1.ToString("N0"));
        text = modifiedText = text.Replace("<str1>", insertableStringValue1);
        text = modifiedText = text.Replace("<deathstotal>", Utilities.loadedSaveData.deathsTotal.ToString());

        int spaceCount = 0;
        for (int pos = 0; pos < text.Length-3; pos++)
        {
            if (text.Substring(pos, 1) == " ")
            {
                ++spaceCount;
                continue;
            }

            bool modded = false;
            foreach (var item in tags)
            {
                string key = item.Key;
                string searcher = "<" + key + ">";
                uint val = (uint)(1 << item.Value);
                if (text.Length > pos + key.Length + 2)
                {
                    if (text.Substring(pos, key.Length + 2) == searcher)
                    {
                        currentState ^= val;
                        if (newFormatData.ContainsKey(pos - spaceCount))
                        {
                            newFormatData[pos - spaceCount] = currentState;
                        }
                        else
                        {
                            newFormatData.Add(pos - spaceCount, currentState);
                        }
                        modifiedText = (text.Length > pos + key.Length + 3) ? (text.Substring(0, pos) + text.Substring(pos + 2+key.Length)) : text.Substring(0, pos);
                        modded = true;
                        text = modifiedText;
                    }
                }
            }
            if (modded)
            {
                pos--;
            }
            if ((currentState & 8) == 8)
            {
                currentState ^= 8;
            }
        }

        return newFormatData;
    }

    public IEnumerator ResetSoon(float time)
    {
        if (messages[currentIndex].noneuclidean)
        {
            debounce = false;

            if (noneuclideanRefresh)
            {
                Text myText = GetComponent<Text>();
                yield return new WaitUntil(() => (myText.text != modText));

                Begin();
            }

            debounce = true;
        }
        else
        {
            float t = 0;
            debounce = false;

            yield return new WaitForEndOfFrame();


            GameObject spinner = null;
            if (messages[currentIndex].skippable) { spinner = Instantiate(textDoneAnimThing, transform.parent); }
            if (narrator && !narrator.CurrentVoiceIsIntelligible()) { narrator.Say(""); }

            while (t < time)
            {
                if (skip)
                {
                    skip = false;
                    break;
                }
                yield return new WaitForEndOfFrame();
                t += 0.01666666f*speedMultiplier;
            }

            jumpButton = false;

            if (messages.Count >= currentIndex + 2)
            {
                currentIndex++;
                Begin();
            }
            else
            {

                if (choice && !finishFlag)
                {
                    if (choiceButtons.Length >= 1)
                    {
                        try
                        {
                            GameObject loadChoiceObj = Resources.Load<GameObject>("bareChoiceBox");
                            GameObject newChoiceObj = Instantiate(loadChoiceObj);
                            newChoiceObj.transform.SetParent(transform.parent);
                            newChoiceObj.transform.localPosition = new Vector3(0, -Mathf.Sign(transform.parent.localPosition.y) * 104, 0);
                            newChoiceObj.GetComponent<Image>().sprite = transform.parent.GetComponent<Image>().sprite;
                            newChoiceObj.SetActive(true);
                            ChoiceUIBox cb = newChoiceObj.GetComponent<ChoiceUIBox>();
                            cb.controls = choiceButtons;
                            cb.choiceMsgs = choiceDisplays;
                            cb.returnObjs = choiceOutcomeObjects;
                            cb.noResponseObjs = choiceNoRespObjs;
                            cb.outputs = choiceOutputs;
                            cb.noResponseOutput = choiceNoRespOutput;
                            cb.time = choiceTime;
                            cb.timeShow = (choiceTime > 0);
                            cb.GetComponent<Encontrolmentation>().allowUserInput = true;
                            cb.origTimeScale = GetComponent<TextBoxGiverHandler>().origTimeScale;
                            cb.Init();
                        }
                        catch
                        {
                            //lol
                        }
                    }
                    else
                    {
                        for (int iz = 0;iz < choiceNoRespObjs.Length;iz++)
                        {
                            if (choiceNoRespObjs[iz].GetComponent<MessageBumpBox>())
                            {
                                choiceNoRespObjs[iz].GetComponent<MessageBumpBox>().output = choiceNoRespOutput;
                            }
                        }
                    }
                }
                finishFlag = true;

            }
            yield return new WaitForEndOfFrame();
            while (!readyToDisplay)
            {
                yield return new WaitForEndOfFrame();
            }
            if (spinner != null) { Destroy(spinner); }
            accelerating = false;
            debounce = true;
        }
    }

    private uint formatStuf = 0;

    private bool HasTextModTag(string tag)
    {
        return (formatStuf & (1 << tags[tag])) == (1 << tags[tag]);
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (readyToCapture)
        {
            readyToCapture = false;
            lastTextMesh = vh;
            lastTextMesh.GetUIVertexStream(lastVertices);
            timeWhenMsgStart = (DoubleTime.UnscaledTimeRunning*speedMultiplier) -2f;
            //stuff...
            readyToDisplay = true;
            vh.Clear();
            //vh.AddUIVertexTriangleStream(vertexList);
        }
        if (readyToDisplay)
        {
            float extraWait = 0;
            List<UIVertex> shownVertices = new List<UIVertex>(lastVertices);
            formatStuf = 0;
            specialTextDoneCheck = true; //gets set to false when a letter isn't there
            for (int i = 0; i < shownVertices.Count; i+=6)
            {
                if (formatData.ContainsKey(i/6))
                {
                    formatStuf = formatData[i / 6];
                }

                float rand1 = Fakerand.Single();
                float rand2 = Fakerand.Single();
                float rand3 = Mathf.PerlinNoise((float)DoubleTime.UnscaledTimeRunning/5, -(float)DoubleTime.UnscaledTimeRunning/5)+ (Fakerand.Single() * 0.01666666f - 0.00833333f);

                Color topHighlightColor = Color.Lerp(messages[currentIndex].highlightColor, Color.white, 0.25f);
                Color bottomHighlightColor = Color.Lerp(messages[currentIndex].highlightColor, Color.black, 0.25f);

                if (HasTextModTag("wait"))
                {
                    extraWait += 0.35f;
                }
                if (HasTextModTag("longwait"))
                {
                    extraWait += 1.25f;
                }
                for (int j = 0; j <= 5; j++)
                {
                    UIVertex v = shownVertices[i + j];
                    float w = messages[currentIndex].letterWait;
                    
                    double timeSince = (!messages[currentIndex].noneuclidean)?((DoubleTime.UnscaledTimeRunning*speedMultiplier) - timeWhenMsgStart - extraWait):(Mathf.Infinity);
                    float stuf = ((float)(i / 6))*w + 2;

                    
                    float ans = Mathf.Clamp01((float)timeSince - stuf);
                    if (ans / w <= 0)
                    {
                        //v.color = new Color32(0, 0, 0, 0);
                        v.position = Vector3.zero;
                        specialTextDoneCheck = false;
                    }
                    else if ((byte)Mathf.Lerp(0, 255, ans / w) != 255)
                    {
                        v.color = new Color32(255, 0, 255, (byte)Mathf.Lerp(0, 255, ans / w));
                        v.position = v.position + (Vector3.up * ((255 - v.color.a) / 32));
                        specialTextDoneCheck = false;
                    }

                    if (HasTextModTag("obfus")) // otherwise someone could read obfuscated stuff. did you?
                    {
                        v.uv0 = new Vector2(Mathf.PingPong((v.uv0.x+rand3),0.5f), Mathf.PingPong((v.uv0.y+rand3),0.5f));
                    }

                    if (v.color.a == 255)
                    {
                        v.color = messages[currentIndex].defaultColor;

                        if (HasTextModTag("shake"))
                        {
                            v.position = v.position + new Vector3((int)((rand1 * 2.4f) - 1.2f), (int)((rand2 * 2.4f) - 1.2f));
                        }

                        if (HasTextModTag("wave"))
                        {
                            v.position = v.position + new Vector3(0, 2.2f * (float)System.Math.Cos(1.2f * (shownVertices[i].position.x / 18) + (2 * DoubleTime.UnscaledTimeRunning * (60f / Application.targetFrameRate) * Mathf.PI * messages[currentIndex].effectSpeed)));
                        }

                        if (HasTextModTag("rainbow"))
                        {
                            Color sample = Color.HSVToRGB(((shownVertices[i].position.x / 200) + ((float)DoubleTime.UnscaledTimeRunning * messages[currentIndex].effectSpeed)) % 1, 0.7f, 1);
                            sample = Color.Lerp(v.color, sample, 0.5f);
                            v.color = new Color32((byte)(sample.r * 255), (byte)(sample.g * 255), (byte)(sample.b * 255), v.color.a);
                        }

                        if (HasTextModTag("!"))
                        {
                            if (j == 0 || j == 1 || j == 5) { v.color = topHighlightColor; }
                            else { v.color = bottomHighlightColor; }
                        }
                        if (HasTextModTag("flash"))
                        {
                            Color sample = messages[currentIndex].highlightColor;
                            v.color = Color32.Lerp(messages[currentIndex].defaultColor, sample, Mathf.Round((float)System.Math.Cos(DoubleTime.UnscaledTimeRunning * Mathf.PI * 2.7f * messages[currentIndex].effectSpeed) / 2f + 0.5f));
                        }
                        
                    }

                    shownVertices[i + j] = v;

                    //if the last vertex is opaque, trigger a coroutine that will reset the text to the next one.
                    
                }

                if (HasTextModTag("wait"))
                {
                    extraWait -= 0.35f;
                }
                if (HasTextModTag("longwait"))
                {
                    extraWait -= 1.25f;
                }

            }

            if (debounce && specialTextDoneCheck && shownVertices.Count > 0)
            {
                StartCoroutine(ResetSoon(MsgWaitTime()));
            }
            else if (noneuclideanRefresh)
            {
                StartCoroutine(ResetSoon(MsgWaitTime()));
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(shownVertices);
        }
        
    }

    public void OnGUI()
    {
        GetComponent<Text>().text = GetComponent<Text>().text + "\0"; //haxx
        GetComponent<Text>().text = GetComponent<Text>().text.Trim('\0');
    }


    public void Begin()
    {
        textDoneAnimThing = Resources.Load<GameObject>("TextDonePrefab");
        if (moveToBanner)
        {
            BannerText bt = FindObjectOfType<BannerText>();
            foreach (var t in messages)
            {
                if (t.text == "<special rank text 2>" && Utilities.CalculateLevel() != 1) { continue; } // special while rank text is not unique
                bt.texts.Add(t.text);
            }
            Destroy(transform.parent.gameObject);
        }
        else
        {
            string text = (!messages[currentIndex].noneuclidean) ? messages[currentIndex].text : GetComponent<Text>().text;
            //string modifiedText = "";
            formatData = PrepareTheFormatting(text, out modText);
            //print("modified text is " + modText);
            stopUpdating = false;
            readyToCapture = true;
            readyToDisplay = false;
            timeWhenMsgStart = (!messages[currentIndex].noneuclidean) ? (DoubleTime.UnscaledTimeRunning*speedMultiplier) : -1313f;
            GetComponent<Text>().text = modText;
            //print(GetComponent<Text>().text);
            if (mainAnimator && messages[currentIndex].animationTrigger != "")
            {
                if (instantAnimationChange) { mainAnimator.CrossFade(messages[currentIndex].animationTrigger, 0.15f); }
                else { mainAnimator.SetTrigger(messages[currentIndex].animationTrigger); }
            }
            if (narrator) { narrator.skipping = false; narrator.Say(text); }
        }
    }

    private ControlsMainCS controlsMainCS;

    public void Start()
    {
        finishFlag = false;
        currentMessage = 0;
        //eat an apple
        if (startsByItself)
        {
            Begin();
        }
        controlsMainCS = new ControlsMainCS();
        controlsMainCS.Main.Enable();
        controlsMainCS.Main.SetCallbacks(this);
    }

    private IEnumerator AccelerateTime()
    {
        while (!specialTextDoneCheck)
        {
            timeWhenMsgStart -= 0.1f;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private bool jumpButton = false;
    private bool lastJumpButton = false;

    public void Update()
    {
        if ((jumpButton && !lastJumpButton) && !messages[currentIndex].noneuclidean && messages[currentIndex].skippable)
        {
            if (/*System.Math.Abs(timeWhenMsgStart + 1313f) > 0.1f*/ !specialTextDoneCheck)
            {
                if (!accelerating)
                {
                    StartCoroutine(AccelerateTime());
                    accelerating = true;
                    if (narrator) { narrator.skipping = true; }
                }
                //timeWhenMsgStart = -1313f;
            }
            else
            {
                skip = true;
            }
        }
        lastJumpButton = jumpButton;
    }

    public override void ModifyMesh(Mesh meshText)
    {
        //deprecated
    }

    public void OnDPad(InputAction.CallbackContext context) { return; }
    public void OnCross(InputAction.CallbackContext context) {
        jumpButton = ((float)context.ReadValueAsObject() >= 0.5f);
    }
    public void OnSquare(InputAction.CallbackContext context) { return; }
    public void OnCircle(InputAction.CallbackContext context) { return; }
    public void OnTriangle(InputAction.CallbackContext context) { return; }
    public void OnShoulders1(InputAction.CallbackContext context) { return; }
    public void OnShoulders2(InputAction.CallbackContext context) { return; }
    public void OnStart(InputAction.CallbackContext context) { return; }
    public void OnSelect(InputAction.CallbackContext context) { return; }
}
