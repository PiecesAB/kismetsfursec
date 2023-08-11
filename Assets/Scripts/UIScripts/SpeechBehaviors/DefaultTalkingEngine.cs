using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.InputSystem;

public class DefaultTalkingEngine : MonoBehaviour {

    [System.Serializable]
    public struct LimbData
    {
        public string limbName;
        public float relativeRotation;
        public string spriteLookupName;
        public string behaviorLookupName;
    }

    [System.Serializable]
    public struct LimbData2
    {
        public LimbData[] limbDatas;
    }

    public Image imgObj;
    public Sprite[] dialogImages;
    [Header("Actually, we'll use my new dictionary to animate!")]
    public List<LimbData2> limbAnimations = new List<LimbData2>();
    [Header("<rnbw> means rainbow. <trmb> means trembling.")]
    [Header("<flsh> means flashing. <wave> means waving.")]
    [Header("<obfs> means obfuscation. <wait x> means wait for x seconds.")]
    public string[] speeches;
    public float[] waitBetweenTime;
    public float startDelay;
    public float openDelay;
    public float openSpeed;
    public float[] letterDelay;
    public GameObject textBox;
    public GameObject statsBarToHide;
    public Color boxColor;
    public double startedLastMessageTime;
    public Color flashTextColor;

    public bool done = false;

    private double oldTime = 0;
    private bool skipRestOfThisLine;
    public int originalFontSizeDoNotChange;
    public int i;

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

    // Use this for initialization


    public void Trigger ()
    {
        StartCoroutine(Succ());
    }

    public void PlayerAllowed(bool v)
    {
        GameObject plr = GameObject.FindGameObjectWithTag("Player");
        if (plr)
        {
            if (plr.GetComponent<BasicMove>())
            {
                plr.GetComponent<BasicMove>().enabled = v;
            }
            if (plr.GetComponent<KHealth>())
            {
                plr.GetComponent<KHealth>().enabled = v;
            }
            if (plr.GetComponent<ClickToChangeTime>())
            {
                plr.GetComponent<ClickToChangeTime>().enabled = v;
            }

            foreach (Collider2D c in plr.GetComponents<Collider2D>())
            {
                c.enabled = v;
            }

            if (plr.GetComponent<Rigidbody2D>())
            {
                plr.GetComponent<Rigidbody2D>().isKinematic = !v;
            }


        }
    }



    IEnumerator Succ()
    {
        float origDampTime = 0.15f;
        originalFontSizeDoNotChange = GetComponent<Text>().fontSize;
        Vector3 origCamPos = new Vector3();

        PlayerAllowed(false);

        origCamPos = Camera.main.gameObject.transform.position;
        Camera.main.gameObject.transform.position = GameObject.FindGameObjectWithTag("SpeechDummyObject").transform.position;

        GameObject.FindGameObjectWithTag("SkyboxCamera").GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
        if (Camera.main.GetComponent<FollowThePlayer>())
        {
            Camera.main.GetComponent<FollowThePlayer>().lockedScriptsOnly = true;
            origDampTime = Camera.main.GetComponent<FollowThePlayer>().dampTime;
            Camera.main.GetComponent<FollowThePlayer>().dampTime = 0;

            Camera.main.GetComponent<FollowThePlayer>().target = GameObject.FindGameObjectWithTag("SpeechDummyObject").transform;
        }
       

        Vector3 oldStatsPos = statsBarToHide.GetComponent<RectTransform>().localPosition;
        Vector3 oldTextPos = textBox.GetComponent<RectTransform>().localPosition;
        textBox.GetComponent<Image>().color = boxColor;
        yield return WaitForRealSeconds(openDelay);
        GetComponent<Text>().text = "";

       /*while (System.Math.Abs(textBox.GetComponent<RectTransform>().localPosition.y + 136) > 3 || System.Math.Abs(statsBarToHide.GetComponent<RectTransform>().localPosition.x - 640) > 3)
        {
            print(textBox.GetComponent<RectTransform>().localPosition.y);
            textBox.GetComponent<RectTransform>().position = Vector3.LerpUnclamped(textBox.GetComponent<RectTransform>().localPosition, textBox.GetComponent<RectTransform>().InverseTransformPoint(0,-136,0), 0.25f);
            statsBarToHide.GetComponent<RectTransform>().position = Vector3.LerpUnclamped(statsBarToHide.GetComponent<RectTransform>().localPosition, statsBarToHide.GetComponent<RectTransform>().InverseTransformPoint(640, -220, 0), 0.25f);
            yield return WaitForRealSeconds(0.04f);
        }*/
        textBox.GetComponent<RectTransform>().localPosition = new Vector3(0,-136, 0);
        statsBarToHide.GetComponent<RectTransform>().localPosition = new Vector3(640, -220, 0);



        imgObj.sprite = dialogImages[0];
        yield return WaitForRealSeconds(startDelay);


        for (i = 0; i < speeches.Length; i++)
        {
            startedLastMessageTime = DoubleTime.UnscaledTimeRunning;
            skipRestOfThisLine = false;
            speeches[i] = speeches[i] + "\0";
            GetComponent<Text>().text = "";
                GetComponent<Text>().fontSize = originalFontSizeDoNotChange;
            imgObj.sprite = dialogImages[i];
            GetComponent<MeshChangerLol>().bitMasks.Clear();

            for (int j = 0; j < speeches[i].Length; j++ )
            {

                float extraWait = 0;
                bool noNewLetter = false;

                string letter = speeches[i].Substring(j, 1);
                if (letter == "<")
                {
                    if (speeches[i].Substring(j, 2) == "</")
                    {
                        string parseTag = speeches[i].Substring(j, 7);

                        if (parseTag == "</rnbw>")
                        {
                            GetComponent<MeshChangerLol>().rainbowifyText = false;
                        }
                        if (parseTag == "</trmb>")
                        {
                            GetComponent<MeshChangerLol>().trembleText = false;
                        }
                        if (parseTag == "</wave>")
                        {
                            GetComponent<MeshChangerLol>().waveText = false;
                        }
                        if (parseTag == "</flsh>")
                        {
                            GetComponent<MeshChangerLol>().flashText = false;
                        }
                        if (parseTag == "</obfs>")
                        {
                            GetComponent<MeshChangerLol>().obfusText = false;
                        }
                        j = j + 7;
                    }
                    else
                    {
                        string parseTag = speeches[i].Substring(j, 6);

                        
                        if (parseTag == "<rnbw>")
                        {
                            GetComponent<MeshChangerLol>().rainbowifyText = true;
                        }
                        if (parseTag == "<trmb>")
                        {
                            GetComponent<MeshChangerLol>().trembleText = true;
                        }
                        if (parseTag == "<wave>")
                        {
                            GetComponent<MeshChangerLol>().waveText = true;  
                        }
                        if (parseTag == "<flsh>")
                        {
                            GetComponent<MeshChangerLol>().flashText = true;
                        }
                        if (parseTag == "<obfs>")
                        {
                            GetComponent<MeshChangerLol>().obfusText = true;
                        }
                        if (parseTag == "<wmbo>")
                        {
                            GetComponent<Text>().fontSize = (int)(GetComponent<Text>().fontSize * 2.5f);
                        }
                        
                        if (parseTag == "<smol>")
                        {
                            GetComponent<Text>().fontSize = GetComponent<Text>().fontSize / 2;
                        }
                        if (parseTag == "<wait ")
                        {
                            int whatsThis = j+6;
                            while (whatsThis > 0 && whatsThis < 100)
                            {
                                string waitTag = speeches[i].Substring(whatsThis, 1);
                                if (waitTag == ">")
                                {
                                    whatsThis = (-whatsThis)+1;
                                }
                                else
                                {
                                    whatsThis += 1;
                                }
                            }
                            string newWaitParsingThingy = speeches[i].Substring(j+6, (-whatsThis)-(j+6)+1);
                            float parsedNumberNow = float.Parse(newWaitParsingThingy);
                            extraWait = parsedNumberNow;
                            j += (-whatsThis) - (j + 6) + 2;
                        }
                        if (parseTag == "<bksp ")
                        {
                            int whatsThis = j + 6;
                            while (whatsThis > 0 && whatsThis < 1000000)
                            {
                                string waitTag = speeches[i].Substring(whatsThis, 1);
                                if (waitTag == ">")
                                {
                                    whatsThis = (-whatsThis) + 1;
                                }
                                else
                                {
                                    whatsThis += 1;
                                }
                            }

                            GetComponent<Text>().text = GetComponent<Text>().text.Substring(0, GetComponent<Text>().text.Length - 1);
                            string newWaitParsingThingy = speeches[i].Substring(j + 6, (-whatsThis) - (j + 6) + 1);
                            float parsedNumberNow = float.Parse(newWaitParsingThingy);
                            extraWait = parsedNumberNow;
                            noNewLetter = true;
                            j += (-whatsThis) - (j + 6) + 1;
                        }
                        j = j + 6;
                    }

                   
                }

                if (!noNewLetter)
                {
                    letter = speeches[i].Substring(j, 1);
                    GetComponent<Text>().text = GetComponent<Text>().text + letter;
                }
                



                if (!skipRestOfThisLine)
                {
                    double x = DoubleTime.UnscaledTimeRunning;
                        while (DoubleTime.UnscaledTimeRunning < x + letterDelay[i] + extraWait && !skipRestOfThisLine)
                        {
                            if (Keyboard.current[Key.Space].wasPressedThisFrame && j>2)
                            {
                                skipRestOfThisLine = true;
                            }
                        if (!skipRestOfThisLine)
                        {
                            yield return 1;
                        }
                        
                    }
                    
                }
          
            }

            yield return new WaitForEndOfFrame();
            oldTime = DoubleTime.ScaledTimeSinceLoad;
            while ((DoubleTime.ScaledTimeSinceLoad < (oldTime + waitBetweenTime[i])) && !(Keyboard.current[Key.Space].wasPressedThisFrame))
            {
                GetComponent<Text>().text = GetComponent<Text>().text + "\0";
                yield return 1;
            }
            yield return new WaitForEndOfFrame();
        }
        statsBarToHide.GetComponent<RectTransform>().localPosition = oldStatsPos;
        textBox.GetComponent<RectTransform>().localPosition = oldTextPos;
        
        imgObj.sprite = null;
        //for (float a = 9; a >= 0; a--)
        // {
        //    transform.localScale = new Vector3(1, a / 10, 1);
        //    textBox.transform.localScale = new Vector3(1, a / 10, 1);
        //    yield return WaitForRealSeconds(0.04f / openSpeed);
        // }
        done = true;
        GetComponent<Text>().fontSize = originalFontSizeDoNotChange;
        GetComponent<Text>().text = "";

       GameObject.FindGameObjectWithTag("SkyboxCamera").GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        Camera.main.gameObject.transform.position = origCamPos;
        if (Camera.main.GetComponent<FollowThePlayer>())
        {
            
            Camera.main.GetComponent<FollowThePlayer>().dampTime = origDampTime;
            Camera.main.GetComponent<FollowThePlayer>().target = GameObject.FindGameObjectWithTag("Player").transform;
            Camera.main.GetComponent<FollowThePlayer>().lockedScriptsOnly = false;
        }
        

        PlayerAllowed(true);



    }
	// Update is called once per frame
	void Update () {
	
	}
}
