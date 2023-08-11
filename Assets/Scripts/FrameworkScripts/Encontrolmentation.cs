using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem;

public class Encontrolmentation: MonoBehaviour
{
    [HideInInspector]
    public ulong currentState = 0UL; //This will give the current state of controls to everything in the game!
    [HideInInspector]
    public float horizontalPressure = 0f; //The current left-right pressure on the analog stick/ D-PAD from 0f to 1f.
    [HideInInspector]
    public float verticalPressure = 0f; //The current up-down pressure on the analog stick/ D-PAD from 0f to 1f.
    [HideInInspector]
    public float horizontalPressureRaw = 0f; //-1 to 1
    [HideInInspector]
    public float verticalPressureRaw = 0f; //-1 to 1
    //sensitivity in a built-in control, yay?
    public bool allowUserInput = true; //other characters don't move
    [HideInInspector]
    public ulong fakeInput = 0UL; //OMG HAXX
    private ulong tempFakeInput = 0UL; // cleared every frame: used internally for auto pressing
    [HideInInspector]
    public Vector2 fakePressure = Vector2.one; //H, V

    private ControlsMainCS controlsMainCS;

    [System.Serializable]
    public struct PairTimeWithControl
    {
        public double Item1;
        public ulong Item2;
        public PairTimeWithControl(double i1, ulong i2)
        {
            Item1 = i1;
            Item2 = i2;
        }
    }

    [System.Serializable]
    public struct PairPositionWithVelocity
    {
        public Vector2 position;
        public Vector2 velocity;
        public PairPositionWithVelocity(Vector2 pos, Vector2 vel)
        {
            position = pos;
            velocity = vel;
        }
    }

    public List<PairTimeWithControl> eventsTable = new List<PairTimeWithControl>(4096) { new PairTimeWithControl(0.0,0UL) }; //This contains all the event changes at their corresponding times.
    public List<PairTimeWithControl> eventsTableUnscaled = new List<PairTimeWithControl>(4096) { new PairTimeWithControl(0.0, 0UL) };
    public int maxEventsTableLength = 4096; //no more than this
    public List<PairPositionWithVelocity> recorderCheckStates = new List<PairPositionWithVelocity>(1024);
    public const double recCheckInterval = 0.5;
    private double baseRecCheckTime = 0.0;
    public string[] initQuickSettingsMessages;
    public enum ActionButton
    {
        Nothing,AButton,BButton,XButton,YButton,LButton,RButton,DPad //these will be important later on
    }

    [Flags]
    public enum NButtons //just in case
    {
        Left = 1,
        Right = 2,
        Up = 4,
        Down = 8,
        A = 16,
        B = 32,
        X = 64,
        Y = 128,
        L = 256,
        R = 512,
        Start = 1024,
        Select = 2048,
        L2 = 4096,
        R2 = 8192,
        //these ones are just for the end of movement of NPCs...
        TriggerDialog = 16384,
        TriggerDialogWhenNear = 32768,
        TriggerDialogAfterShortWait = 65536

    }


    public ulong flags = 0UL; // this shows button downs in the last frame
    private ulong upFlags = 0UL; // this shows button ups in the last frame
    public string eventAName = "Nothing";
    public ActionButton eventAbutton = ActionButton.BButton;
    public string eventBName = "Situate";
    public ActionButton eventBbutton = ActionButton.XButton;
    public Encontrolmentation possiblePreviousFocus;
    public int givenObjIdentifier;
    public bool masterControl;
    public bool initControl;
    public bool debugKeysMode;
    //public Texture2D randtest;

    [SerializeField]
    private AudioMixer[] au1; //lol!
    private bool initializedStuff;
    private int xx;

    private static bool didSettings = false;
    private static Encontrolmentation master = null;

    public bool recordPositionsAndVelocities;
    public bool recordForAutoMover = false;

    private BasicMove bm;

    //1 is the rightmost bit of the ulong
    //bit 1 is LEFT
    //bit 2 is RIGHT
    //bit 4 is UP
    //bit 8 is DOWN
    //bit 16 is X
    //bit 32 is Square
    //bit 64 is Circle
    //bit 128 is Triangle
    //bit 256 is L1
    //bit 512 is R1
    //bit 1024 is Start
    //bit 2048 is Select
    //bit 4096 is L2
    //bit 8192 is R2
    //...other controls :>

    private bool hasGun = false;

    public void Start() //these are for the start of the game. hold down a button for quick game settings
    {
        DoubleTime.Load();
        CrossSceneControl.Load();
        hasGun = GetComponent<SpecialGunTemplate>() && GetComponent<SpecialGunTemplate>().enabled;

        if (!didSettings)
        {
            didSettings = true;
            Settings.PrepareAll();
        }

        //if (!PlayerPrefs.HasKey("MasterVolume"))
        //{
        //    PlayerPrefs.SetInt("MasterVolume", 8); //default volume
        //}

        /*for (int i = 0; i < 65536; i++)
        {
            float t = Fakerand.Single();
            float u = Fakerand.Single();
            float v = Fakerand.Single();
            randtest.SetPixel(i % 256, i / 256, new Color(t,u,v,1));
        }
        randtest.Apply();*/
     
        PlayerPrefs.Save();
        initializedStuff = false;
        xx = 4;

        BulletController.Load();

        if (masterControl)
        {
            //prevent too many masters
            if (master == null) { master = this; }
            else { masterControl = false; }

            //DoubleTime.ScaledTimeSinceLoad = DoubleTime.UnscaledTimeSinceLoad = 0f;
            BrainwaveReader.Reset();
            //test
            //Utilities.SaveGame(1);
        }

        baseRecCheckTime = 0.0;
        bm = GetComponent<BasicMove>(); //if it exists

        if (!csch) { csch = CrossSceneControl.GetHelper(); }
        if (allowUserInput)
        {
            currentState |= csch.controlBuffer;
            horizontalPressure = csch.horizontalPressure;
            horizontalPressureRaw = csch.horizontalPressureRaw;
            verticalPressure = csch.verticalPressure;
            verticalPressureRaw = csch.verticalPressureRaw;
        }
    }

    /*public void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("InitQuickSet");
    }*/

    public bool HasEverMoved()
    {
        return eventsTable.Count > 1;
    }

    public void SimulateTap(ulong u)
    {
        tempFakeInput |= u;
    }

    public bool ButtonDown(ulong u, ulong check, bool bypassInput = false)
    {
        return ((allowUserInput || bypassInput) && (currentState & check) == u && (flags & check) == u);
    }

    public bool ButtonUp(ulong u, ulong check, bool bypassInput = false)
    {
        return ((allowUserInput || bypassInput) && (currentState & check) == 0UL && (upFlags & check) == u);
    }

    public bool AnyButtonDown(ulong check = 0xFFFFFFFFFFFFFFFF)
    {
        return (allowUserInput && (currentState & check) != 0UL && (flags & check) != 0UL);
    }

    public bool ButtonHeld(ulong u,ulong check,float conditiontime, out double pressedtime)
    {
        var ev = eventsTableUnscaled[eventsTableUnscaled.Count-1]; //most recent change
        pressedtime = DoubleTime.UnscaledTimeSinceLoad - ev.Item1;
        return ((currentState & check) == u && (DoubleTime.UnscaledTimeSinceLoad - ev.Item1) >= conditiontime && (ev.Item2 & check) == (currentState & check));
    }

    public Vector2 AnalogDirection(float deadZone = 0.2f)
    {
        float x = horizontalPressureRaw;
        float y = verticalPressureRaw;
        if (Mathf.Abs(x) < deadZone) { x = 0; }
        if (Mathf.Abs(y) < deadZone) { y = 0; }
        return new Vector2(x, y);
    }

    public void TalkNPC()
    {
        if (ButtonDown(16384UL, 16384UL, true))
        {
            Transform msgs = transform.Find("Messages");
            if (msgs && msgs.childCount > 0)
            {
                GameObject newBox = msgs.GetChild(0).gameObject;
                newBox.SetActive(true);
                newBox.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
                newBox.GetComponentInChildren<TextBoxGiverHandler>().mainSpeaker = gameObject;
            }
        }
    }

    private static CrossSceneControlHelper csch;

    public void Update() 
    {
        ulong newState = 0UL;
        flags = upFlags = 0UL;

        if (recordPositionsAndVelocities)
        {
            if (recorderCheckStates.Count == 0)
            {
                recorderCheckStates.Add(new PairPositionWithVelocity(transform.position, bm ? bm.fakePhysicsVel : Vector2.zero));
                baseRecCheckTime = DoubleTime.ScaledTimeSinceLoad;
            }
            else if (recCheckInterval*recorderCheckStates.Count <= DoubleTime.ScaledTimeSinceLoad - baseRecCheckTime)
            {
                recorderCheckStates.Add(new PairPositionWithVelocity(transform.position, bm ? bm.fakePhysicsVel : Vector2.zero));
            }
        }

        if (allowUserInput)
        {
            if (!csch) { csch = CrossSceneControl.GetHelper(); }
            newState |= csch.controlBuffer;
            horizontalPressure = csch.horizontalPressure;
            horizontalPressureRaw = csch.horizontalPressureRaw;
            verticalPressure = csch.verticalPressure;
            verticalPressureRaw = csch.verticalPressureRaw;
            Fakerand.input1 = (uint)(newState%4294967296);
            Fakerand.Int(0, 10);
        }
        else
        {
            newState = fakeInput; //it's fake!
            newState |= tempFakeInput;
            tempFakeInput = 0UL;
            horizontalPressure = Mathf.Clamp01(fakePressure.x);
            verticalPressure = Mathf.Clamp01(fakePressure.y);
        }

        if (newState != currentState)
        {
            eventsTable.Add(new PairTimeWithControl(DoubleTime.ScaledTimeSinceLoad, newState)); //scaled or unscaled??
            eventsTableUnscaled.Add(new PairTimeWithControl(DoubleTime.UnscaledTimeSinceLoad, newState));
            // what depends on the time being scaled: 
            // the ability to melee attack
            // what depends on the time being unscaled: 
            // button hold controls

            // there is now an unscaled table too, as a compromise.
            ulong changes = newState ^ currentState;
            upFlags = changes & (~newState);
            flags = changes & newState; // it's a ulong which has binary 1s where button downs just happened
            currentState = newState;

            while (eventsTable.Count > maxEventsTableLength) { eventsTable.RemoveRange(0, Math.Min(1024, eventsTable.Count / 2)); }
            while (eventsTableUnscaled.Count > maxEventsTableLength) { eventsTableUnscaled.RemoveRange(0, Math.Min(1024, eventsTableUnscaled.Count / 2)); }
        }

        TalkNPC();

        if (recordForAutoMover && GetComponent<EncontrolmentationAutoMover>())
        {
            recordForAutoMover = false;
            GetComponent<EncontrolmentationAutoMover>().fakeEventsTable = eventsTable;
            GetComponent<EncontrolmentationAutoMover>().checkpoints = recorderCheckStates;
        }

        //this is where all the settings magic happens. except to reduce the spaghetti, a lot has been moved to a more sustainable pattern.
        if (masterControl)
        {

            /*float newVol = PlayerPrefs.GetInt("MasterVolume") / 10f;
            foreach (var au in au1)
            {
                if (newVol <= 200f)
                {
                    au.SetFloat("Volume", newVol);
                }
                else
                {
                    au.SetFloat("Volume", newVol);
                    au.SetFloat("Distortion", (newVol-200f)/200f);
                }
        }*/

            //DoubleTime.AddToTime(0.0166666666666666666666);
            BrainwaveReader.Update(this);
            //print(DoubleTime.ScaledTimeRunning);
        }

        xx = Mathf.Max(xx-1,0);
        if ((initControl || debugKeysMode) && !initializedStuff && xx == 0)
        {
           initializedStuff = Init();
        }

        //must be set every frame!
        ClearContextActions();
    }

    public void ClearContextActions()
    {
        eventAbutton = ActionButton.BButton;
        eventAName = hasGun ? "Weapon" : "...";
        eventBbutton = ActionButton.XButton;
        eventBName = "Melee";
        givenObjIdentifier = 0;
    }

    public bool Init()
    {
         /*   if (In*put.GetKey("m"))
            { // "m"
                if (In*put.GetKey(","))
                { //"m" + ","
                print("dfd");
                PlayerPrefs.SetInt("MasterVolume", -800); //mute the game

                if (In*put.GetKey("."))
                { //"m" + "." + ","
                    PlayerPrefs.SetInt("MasterVolume", 0);//put game at normal volume
                }
            }
            else if (In*put.GetKey("."))
            { //"m" + "."
                PlayerPrefs.SetInt("MasterVolume", 200);//put game at full volume
            }
            if (In*put.GetKey("/"))
            { //"m" + "/"
                PlayerPrefs.SetInt("MasterVolume", 400);//earrape
            }
        }*/

        //PlayerPrefs.Save();
        return true;
    }

    public string ClearEventsTable()
    {
        try
        {
            eventsTable = new List<PairTimeWithControl>() { new PairTimeWithControl( DoubleTime.ScaledTimeSinceLoad, currentState) };
            return "Congration!";
        }
        catch
        {
            return "An error to clean control events!";
        }
    }

    
}

