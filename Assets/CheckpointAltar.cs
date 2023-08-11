using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class CheckpointAltar : MonoBehaviour, IChoiceUIResponse
{
    private Encontrolmentation player;
    public BoxCollider2D operableBound;
    public bool con;
    public static CheckpointAltar currentlyActive = null;
    public string checkpointName;

    public Sprite noneSprite;
    public Sprite offSprite;
    public Material offMaterial;
    public Sprite onSprite;
    public Material onMaterial;

    public GameObject onUseText;
    public GameObject poorText;
    public GameObject poorText2;
    public GameObject poorTextHighClass;
    public GameObject introText;
    public GameObject replayText;
    public bool usedPoorText;
    [Header("set below to \"activate\" to... activate")]
    public string recieveMessage;
    public int cost;

    public bool giveIntro;

    private LevelInfoContainer lic;

    public SpriteRenderer shineEffect;
    public float shineEffectTime;
    public SpriteRenderer rayEffect;
    private float shineInnerCircle = 1f;
    private float shineOuterCircle = 1f;
    private float sicVel;
    private float socVel;

    public AudioClip activateSound;
    public AudioClip spawnSound;
    private AudioSource audioSource;

    // others spawn in a stack above the first player (see LevelInfoContainer)
    public string[] extraPlayers;

    public void MakeAppearance(bool turnOn)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        rayEffect.enabled = turnOn;
        if (turnOn)
        {
            sr.sprite = onSprite;
            sr.material = onMaterial;
            shineInnerCircle = shineOuterCircle = 0f;
            sicVel = socVel = 0f;
        }
        else
        {
            sr.sprite = offSprite;
            sr.material = offMaterial;
        }
    }

    void Awake()
    {
        if (!Application.isPlaying) { return; }
        audioSource = GetComponent<AudioSource>();
        shineEffect.enabled = true;
        shineInnerCircle = shineOuterCircle = 1f;
        sicVel = socVel = 0f;
        if (Utilities.GetLevelInInfo() == checkpointName)
        {
            currentlyActive = this;
            audioSource.clip = spawnSound;
            audioSource.Play();
        }
        if (this == currentlyActive)
        {
            MakeAppearance(true);
        }
        else
        {
            MakeAppearance(false);
        }
        if (Utilities.replayLevel)
        {
            GetComponent<SpriteRenderer>().sprite = noneSprite;
            shineEffect.enabled = false;
            Destroy(this);
            return;
        }
        usedPoorText = false;
        if (Utilities.GetLevelInInfo() == checkpointName)
        {
            lic = FindObjectOfType<LevelInfoContainer>(); // LevelInfoContainer.main may not work at this early loading stage.
            lic.otherStartPlaceValues.Add(checkpointName);
            lic.otherStartPlaces.Add(transform.position);
            lic.otherStartPlaces.Add(transform.position);
            lic.otherStartPlaceExtraPlayers.Add(extraPlayers);
        }
    }

    #if UNITY_EDITOR
    private void Start()
    {
        if (!Application.isPlaying)
        {
            string oldCN = checkpointName;
            checkpointName = SceneManager.GetActiveScene().name + ":" + gameObject.GetInstanceID();

            if (oldCN != checkpointName)
            {
                EditorUtility.SetDirty(this);
            }
            return;
        }
    }
    #endif

    public GameObject ChoiceResponse(string text)
    {
        recieveMessage = text;
        return null;
    }

    private bool WeAreIntersecting(Bounds otherBound)
    {
        Bounds tempThis = new Bounds((Vector2)transform.position + operableBound.offset, operableBound.size);
        Vector2 mid = otherBound.center - tempThis.center;
        float midX = Mathf.Abs(mid.x) - (otherBound.extents.x + tempThis.extents.x);
        float midY = Mathf.Abs(mid.y) - (otherBound.extents.y + tempThis.extents.y);
        return (midX <= 0f) && (midY <= 0f);
    }

    void Update()
    {
        if (!Application.isPlaying) { return; }

        if (shineInnerCircle < 0.5f || shineOuterCircle < 0.5f)
        {
            //We assume that the shine has the shader Sprites/Effects/SuperShine
            shineInnerCircle = Mathf.SmoothDamp(shineInnerCircle, 0.5f, ref sicVel, shineEffectTime, 666f, 0.0166666666f);
            shineOuterCircle = Mathf.SmoothDamp(shineOuterCircle, 0.5f, ref socVel, shineEffectTime * 0.8f, 666f, 0.0166666666f);
            Color hcol = Color.HSVToRGB((float)(((-DoubleTime.UnscaledTimeRunning) * -8f)%1.0), 0.3f, 1f);
            shineEffect.color = new Color(hcol.r, hcol.g, hcol.b, 0.75f);
            shineEffect.material.SetFloat("_ICS", shineInnerCircle);
            shineEffect.material.SetFloat("_OCS", shineOuterCircle);
            if (shineInnerCircle > 0.49f)
            {
                shineInnerCircle = 0.5f;
                sicVel = 0f;
            }
            if (shineOuterCircle > 0.49f)
            {
                shineOuterCircle = 1f;
                socVel = 0f;
            }
        }
        else
        {
            shineEffect.material.SetFloat("_ICS", 0.5f);
            shineEffect.material.SetFloat("_OCS", 0.5f);
        }

        if (this != currentlyActive)
        {
            bool clearControl = false;
            if (recieveMessage == "activate")
            {
                recieveMessage = "";
                Utilities.ChangeScore(-cost);
                Utilities.SoftSave();
                Utilities.SetLevelInInfo(checkpointName);
                audioSource.clip = activateSound;
                BGMController.main?.DuckOutAtSpeedForFrames(100f, 2);
                audioSource.Play();
                MakeAppearance(true);
                currentlyActive = this;
                clearControl = true;
            }
            else
            {
                MakeAppearance(false);
            }

            player = LevelInfoContainer.GetActiveControl();

            if (player && WeAreIntersecting(player.GetComponent<Renderer>().bounds))
            {
                if (giveIntro)
                {
                    GameObject ne = Instantiate(introText, Vector3.zero, Quaternion.identity);
                    ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
                    ne.SetActive(true);

                    giveIntro = false;
                }

                player.eventBbutton = Encontrolmentation.ActionButton.XButton;
                player.eventBName = "Imprint";
                player.givenObjIdentifier = gameObject.GetInstanceID();
                con = true;
            }
            else if (player && (player.eventBName == "Imprint" || currentlyActive))
            {
                if (con)
                {
                    //player.eventBbutton = Encontrolmentation.ActionButton.XButton;
                    //player.eventBName = "";
                    //player.givenObjIdentifier = 0;
                    con = false;
                }
            }

            if (player)
            {
                bool isAimingWeapon = (player.GetComponent<ClickToChangeTime>() && player.GetComponent<ClickToChangeTime>().isAiming);

                if (con && player.allowUserInput && player.ButtonDown(64UL, 64UL) && (player.currentState & 15UL) == 0UL
                    && player.givenObjIdentifier == gameObject.GetInstanceID() && !isAimingWeapon && Time.timeScale > 0)
                {
                    MainTextsStuff.insertableIntValue1 = cost;
                    GameObject ne = null;
                    if (Utilities.replayLevel && Utilities.replayMode != 0)
                    {
                        ne = Instantiate(replayText, Vector3.zero, Quaternion.identity);
                    }
                    else if (Utilities.loadedSaveData.score >= cost)
                    {
                        ne = Instantiate(onUseText, Vector3.zero, Quaternion.identity);
                        usedPoorText = false;
                    }
                    else if (cost >= 40000)
                    {
                        ne = Instantiate(poorTextHighClass, Vector3.zero, Quaternion.identity);
                    }
                    else
                    {
                        if (!usedPoorText)
                        {
                            ne = Instantiate(poorText, Vector3.zero, Quaternion.identity);
                            usedPoorText = true;
                        }
                        else
                        {
                            ne = Instantiate(poorText2, Vector3.zero, Quaternion.identity);
                        }

                    }
                    ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
                    ne.SetActive(true);
                }

                if (clearControl)
                {
                    player.eventBbutton = Encontrolmentation.ActionButton.Nothing;
                    player.eventBName = "";
                    player.givenObjIdentifier = 0;
                    con = false;
                }
            }
        }
    }
}
