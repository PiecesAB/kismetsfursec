using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Audio;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class BasicMove : MonoBehaviour
{
    //why i didn't use state machines? because this was my first script in unity at some point

    public bool youCanJump = true;
    public bool youCanDoubleJump;
    public bool youCanWallJump;
    public bool youCanInfinityJump;
    public float moveSpeed;

    public float jumpHeight;
    public float friction;
    public float maxFallSpeed;
    [HideInInspector]
    public float maxFallSpeedTranqMult = 1f;
    public float gravityMultiplier = 1f;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask whatIsGround;
    public Vector2 pointOfContact;
    //public Rigidbody2D rigRef;
    public GameObject prefabDust;
    public float Damage;
    [Header("-SOUNDS-")]
    public AudioClip spikeTouchSound;
    public AudioClip walkSound;
    public AudioClip sprintWalkSound;
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioSource landSound;
    public AudioSource wallSlideSound;
    public AudioSource outsideIceSound;
    private AudioSource wallPushSound;
    public AudioSource punchWhoosh;
    public AudioSource kickWhoosh;
    public AudioSource midairRush;
    public AudioSource ceilHit;
    public AudioSource momentumBounce;
    public AudioSource momentumAmbient;
    public AudioSource waterSplashSound;
    public AudioSource sprintStartSound;
    [Header("----------")]
    public GameObject bloodSpurt;
    public GameObject laserBurn;
    public GameObject sprintParticle;
    public GameObject[] wallDusts;
    public GameObject bloodStay;
    public Sprite fallSprite;
    public Sprite[] electrocutedSprites;

    [HideInInspector]
    public bool allowRotation;

    public int grounded;
    public bool doubleJump;
    public float currentHorizontalVelocity;
    public MovementMod movementModifier = 0;
    public bool momentumMode;
    public bool momentum = true; //false = left, true = right
    public Transform momentumParticle;

    float timeLastGrounded = 0;
    public bool swimming;
    private bool wasSwimming;
    public bool running;
    public bool jumping;
    private bool falling;
    public bool wallSliding;
    public bool noCustomSpeed;
    public bool CanCollide = true;
    private int CCLossDelay;
    public float gravMultiplier;
    public float fricMultiplier;
    public float speedMultiplier = 1f;
    public bool boosted;
    public int glued;
    public float iced;
    public int iced2;
    private float oldFric;
    private Rigidbody2D rig;
    private KHealth kh;
    private AudioSource audSrc;
    private Animator animator;

    private Vector2 touchingWallAndDirection;
    private int wallJumpFrameCountdown;
    private Transform originalParent;

    private byte storedHMvtByte = 0;
    private List<byte> storedMvtLogs = new List<byte>();
    private List<double> timesWhenMoved = new List<double>(); // these two lists record the player move
    public int sprintCounter = 0;
    private double lastTimeChangedHMvt;
    private int timeJumpCool = 0;
    private bool changeGroundedAnim = true;
    private int jumpAnimGrace;
    public Vector2 fakePhysicsVel;
    public PrimMovingPlatform extraPlat;

    [HideInInspector]
    public byte swimCount;
    [HideInInspector]
    public byte rampSmoothCount;

    [HideInInspector]
    public Collider2D[] allMyColliders;

    public Material CanCollideFalseMaterial;
    public Material ElectrocutionMaterial;
    private Material defaultMat;
    private int zzzzzzzzzz;
    public int udm;
    public int whichPlayerAmI;
    //private SliderJoint2D sliderH;
    private SliderJoint2D sliderV;
    [HideInInspector]
    public float prevTimeScale;
    private int electroSpriteChange;
    private int electroSpriteCurrent;
    //private bool stickt;

    public BoxCollider2D punchRBox;
    public BoxCollider2D kickRBox;

    public Vector2 extraPerFrameVel;

    public AudioMixer plrSFX;

    [HideInInspector]
    public Rigidbody2D fakePlatform;

    public bool disabledAllControlMvt;
    [HideInInspector]
    public bool notFlipwired = true;

    [HideInInspector]
    public bool ghost = false;

    private Vector2 minTransDir = Vector2.zero;
    private Vector2 prevMinTransDir = Vector2.zero;

    private int spikeTouchCooldown;
    private bool cornerFudgedThisFrame;
    private bool dontMinTranslateThisFrame = false;

    public enum MovementMod
    {
        None, Tired, High, Stunned
    };
    private static float[] ModJumpMults = new float[4] { 1f, 0.3f, 3.3f, 1f };
    private static float[] ModSpeedMults = new float[4] { 1f, 0.56f, 1.8f, 0.00001f };
    private static float[] ModGravMults = new float[4] { 1f, 0.18f, 6f, 1f };
    private const int groundedGraceFrames = 6;
    private const int idleCheckFrames = 600;
    private int idleCheck = 600;
    [HideInInspector]
    public bool lavaCheck;

    [Header("-----------------------------------------")]

    public float punchPowerMultiplier = 1f;

    private List<Collider2D[]> collisionIgnoredList = new List<Collider2D[]>();

    private BoxCollider2D myBoxCollider;
    private SpriteRenderer mySprRend;
    private BoxCollider2D betweenPlayersCushionB = null;
    private BoxCollider2D betweenPlayersCushionT = null;
    private BasicMove betweenPlayersCushionTOther = null;

    private bool turnOffCollisionForever = false;

    private Encontrolmentation encmt;
    private SpecialGunTemplate sgt;

    public ElectronTracker electronTracker;

    // does everything have to be fake???
    [HideInInspector]
    public Vector2 fakeGravity;
    [HideInInspector]
    public int fakeGravityOverrideFrames = 0;

    public void ResetIdleCounter()
    {
        idleCheck = idleCheckFrames;
    }

    //this state change detection might get rid of the animation variables, later on.
    void OnChangeOfHorizMovement(byte info)  // in this byte, 2 is left key, 1 is right key.
    {
        AudioSource asrc = audSrc;
        Utilities.ChangePlrMvtRequest(info);
        double lastMvtChange = DoubleTime.UnscaledTimeRunning-lastTimeChangedHMvt;

        storedMvtLogs.Add(info);
        timesWhenMoved.Add(DoubleTime.UnscaledTimeRunning);

        if (((info & 3) == 1 || (info & 3) == 2))
        {
            bool wsSoundPlaying = asrc.clip == walkSound || audSrc.clip == sprintWalkSound;
            if (grounded > 0
                && (!wsSoundPlaying || (wsSoundPlaying && !asrc.isPlaying))
            )
            {
                asrc.Stop();
                asrc.clip = (speedMultiplier >= 2f) ? sprintWalkSound : walkSound;
                asrc.loop = true;
                asrc.Play();
            }
        }
        if ((info & 4) == 4)
        {
            asrc.Stop();
            asrc.loop = false;
            asrc.clip = null;

            if (doubleJump)
            {
                asrc.clip = jumpSound;
                asrc.Play();
                //asrc.PlayOneShot(jumpSound, 1f);
            }
            else
            {
                asrc.clip = doubleJumpSound;
                asrc.Play();
                //asrc.PlayOneShot(doubleJumpSound, 1f);
            }
        }

        if ((info == 0 && (grounded > 0 || System.Math.Abs(fakePhysicsVel.y - maxFallSpeed) < 2)))
        {
            asrc.Stop();
        }

        for (int lol = 1; lol < 3; lol++)
        {
            if (0 > storedMvtLogs.Count - 1 - lol)
            {
                break;
            }
            byte tmpM = storedMvtLogs[storedMvtLogs.Count - 1 - lol];
            double tmpT = timesWhenMoved[timesWhenMoved.Count - 1 - lol];
            //this will help sprint

            if ((tmpM == 2 && (info & 3) == 2) || (tmpM == 1 && (info & 3) == 1))
            {
                bool otherKeys = (GetComponent<Encontrolmentation>().flags & (~3UL)) != 0UL;
                if (otherKeys)
                {
                    sprintCounter = 0;
                }
                if (!otherKeys && DoubleTime.UnscaledTimeRunning - tmpT < 0.3f && !momentumMode) //
                {
                    sprintCounter++;
                    break;
                }
            // values 0-20 are reserved for large sprint
            }
        }

        lastTimeChangedHMvt = DoubleTime.UnscaledTimeRunning;
    }

    void ZeroX()
    {
        currentHorizontalVelocity = fakePhysicsVel.x;
        currentHorizontalVelocity = Mathf.Lerp(currentHorizontalVelocity, 0, friction*fricMultiplier);
        fakePhysicsVel = new Vector2((Time.timeScale == 0f)? currentHorizontalVelocity : (currentHorizontalVelocity / Time.timeScale), fakePhysicsVel.y);

    }

    private void TurnOffWallSlideDust()
    {
        foreach (GameObject i in wallDusts)
        {
            ParticleSystem.EmissionModule emod = i.GetComponent<ParticleSystem>().emission;
            emod.enabled = false;
        }
    }

    private Rigidbody2D wallSliderMoverR2;
    private primDecorationMoving wallSliderMoverPDM;

    private Collider2D[] wallSlideColliders = new Collider2D[3];

    private int wallDoubleJumpDelay = 0;

    public void WallSlider(Vector2 dir)
    {
        if (youCanWallJump && timeJumpCool == 0)
        {
            bool okee = false;
            
            if (grounded > 0)
            {
                touchingWallAndDirection = new Vector2(0, 0);
                wallSliding = false;
                TurnOffWallSlideDust();
                wallSliderMoverR2 = null;
                wallSliderMoverPDM = null;
                maxFallSpeed = jumpHeight * ModJumpMults[(int)movementModifier];
            }
            if (grounded == 0)
            {
                if (encmt.ButtonHeld(8UL, 8UL, 0f, out _))
                {
                    goto AfterAllWallJumpRaycasts;
                }

                Vector2 wallCheckVec = Mathf.Sign(transform.lossyScale.x) * transform.lossyScale.y * transform.up;
                float wallCheckDist = Mathf.Abs(13.5f * transform.lossyScale.x);
                Vector2 rotDir = RotateVector2(dir, transform.eulerAngles.z);
                if (transform.lossyScale.x < 0) { rotDir = new Vector2(-rotDir.x, rotDir.y); }

                float offset = 0;

            wsRay1:
                RaycastHit2D ray1 = Physics2D.Raycast(((Vector2)transform.position) + (9 + offset) * wallCheckVec, rotDir, wallCheckDist, 3 << 8);
                if (ray1.collider && Physics2D.GetIgnoreCollision(ray1.collider, allMyColliders[0]))
                {
                    raycastSwap.Add(ray1.collider.gameObject);
                    raycastSwapLayer.Add(ray1.collider.gameObject.layer);
                    ray1.collider.gameObject.layer = 0;
                    goto wsRay1;
                }
                if (!ray1.collider && offset == 0)
                {
                    offset = -8;
                    goto wsRay1;
                }

                wsRay2:
                RaycastHit2D ray2 = Physics2D.Raycast(((Vector2)transform.position) + (-9 + offset) * wallCheckVec, rotDir, wallCheckDist, 3 << 8);
                if (ray2.collider && Physics2D.GetIgnoreCollision(ray2.collider, allMyColliders[0]))
                {
                    raycastSwap.Add(ray2.collider.gameObject);
                    raycastSwapLayer.Add(ray2.collider.gameObject.layer);
                    ray2.collider.gameObject.layer = 0;
                    goto wsRay2;
                }
                if (!ray2.collider && offset == 0)
                {
                    offset = 8;
                    goto wsRay1;
                }

                RaycastHit2D ray3 = new RaycastHit2D();
                bool useRay3 = (offset == 0);
                // [ ]
                //    
                // [ ]
                if (useRay3) // do a special check to avoid being able to wall jump on the above configuration of blocks
                {
                    wsRay3:
                    ray3 = Physics2D.Raycast(transform.position, rotDir, wallCheckDist, 3 << 8);
                    if (ray3.collider && Physics2D.GetIgnoreCollision(ray3.collider, allMyColliders[0]))
                    {
                        raycastSwap.Add(ray3.collider.gameObject);
                        raycastSwapLayer.Add(ray3.collider.gameObject.layer);
                        ray3.collider.gameObject.layer = 0;
                        goto wsRay3;
                    }
                }

                wallSlideColliders[0] = ray1.collider;
                wallSlideColliders[1] = ray2.collider;
                wallSlideColliders[2] = ray3.collider;

                if (ray1 && ray2 && (!useRay3 || ray3))
                {
                    if (ray1.transform.gameObject.tag != "Beam" && ray2.transform.gameObject.tag != "Beam" 
                        && ray1.transform.GetComponent<FakeFrictionOnObject>() == null && ray2.transform.GetComponent<FakeFrictionOnObject>() == null)
                    {
                        if (ray1.collider.isTrigger == false && ray2.collider.isTrigger == false 
                            && (CanCollide || (ColliderIsImpervious(ray1.collider) && ColliderIsImpervious(ray2.collider))))
                        {
                            okee = true;
                            wallJumpFrameCountdown = 6;

                            // moving platform behavior
                            Rigidbody2D rig1 = ray1.transform.GetComponent<Rigidbody2D>();
                            Rigidbody2D rig2 = ray2.transform.GetComponent<Rigidbody2D>();
                            Vector2 v = Vector2.zero;
                            if (rig1)
                            {
                                primDecorationMoving pdm1 = rig1.GetComponent<primDecorationMoving>();
                                v = pdm1 ? pdm1.GetVelocitation() : rig1.velocity;
                                if (pdm1) { wallSliderMoverPDM = pdm1; }
                                wallSliderMoverR2 = rig1;
                            }
                            else if (rig2)
                            {
                                primDecorationMoving pdm2 = rig2.GetComponent<primDecorationMoving>();
                                v = pdm2 ? pdm2.GetVelocitation() : rig2.velocity;
                                if (pdm2) { wallSliderMoverPDM = pdm2; }
                                wallSliderMoverR2 = rig2;
                            }
                            else
                            {
                                wallSliderMoverPDM = null;
                                wallSliderMoverR2 = null;
                            }
                            if (WallSlidePlatformIsMovingTowards())
                            {
                                v *= 0f;
                            }
                            extraPerFrameVel += v;
                        }
                    }
                }

                AfterAllWallJumpRaycasts:

                if (okee)
                {
                    touchingWallAndDirection = dir;
                    if (grounded == 0 && !doubleJump)
                    {
                        if (wallDoubleJumpDelay == 0) { doubleJump = true; }
                    }
                    wallSliding = true;
                    bool flip = mySprRend.flipX;
                    foreach (GameObject i in wallDusts)
                    {
                        ParticleSystem.EmissionModule emod = i.GetComponent<ParticleSystem>().emission;
                        emod.enabled = true;
                        Vector3 p = i.transform.localPosition;
                        i.transform.localPosition = new Vector3((flip ? -1f : 1f)*Mathf.Abs(p.x), p.y, -2f);
                    }
                    maxFallSpeed = jumpHeight * ModJumpMults[(int)movementModifier] * friction * 1.25f;
                }
                else
                {
                    /*if (grounded == 0 && wallSliding && fakePhysicsVel.y < 0)
                    {
                        doubleJump = false;
                    }*/
                    if (wallSliding)
                    {
                        touchingWallAndDirection = new Vector2(0, 0);
                        wallSliding = false;
                        TurnOffWallSlideDust();
                        wallSliderMoverR2 = null;
                        wallSliderMoverPDM = null;
                        maxFallSpeed = jumpHeight * ModJumpMults[(int)movementModifier];
                    }
                }

                ClearRaycastSwap();
            }
        }
    }

    private static Transform audHolder = null;
    AudioSource GetAud(string s)
    {
        if (Application.isPlaying)
        {
            if (audHolder == null)
            {
                audHolder = MoreSoundsHolder.main.transform;
            }
            return audHolder.Find(s).GetComponent<AudioSource>();
        }
        else
        {
            return GameObject.Find(s).GetComponent<AudioSource>();
        }
    }

    //better way coming soon
    void InitSpecialPlayerVars()
    {
        landSound = GetAud("landSound");
        outsideIceSound = GetAud("icesound");
        punchWhoosh = GetAud("punchWhoosh");
        kickWhoosh = GetAud("kickWhoosh");
        midairRush = GetAud("airSoundJumpAlwaysPlaying");
        ceilHit = GetAud("ceilHit");
        waterSplashSound = GetAud("publicDomainSplash");
        sprintStartSound = GetAud("sprintStartSound");
        wallPushSound = GetAud("wallPushSound");
        wallSlideSound = GetAud("wallSlideSound");

        KHealth kh = GetComponent<KHealth>();

        kh.toxicShockSound = GetAud("shockextreme");
        kh.electrocutedSound = GetAud("electrocutePlayer");
        kh.minorVictorySound = GetAud("minorSuccess");
        kh.fastButtonPress = GetAud("fastButtonPress");
        kh.fireIgniteSound = GetAud("fireIgnite");
        kh.fireOngoingSound = GetAud("fireContinuous");
        kh.doorEnterRef = GetAud("doorEnter");

        GetComponent<OffScreenArrow>().cam = Camera.main;
    }

    void Awake()
    {
        maxFallSpeed = jumpHeight;
        if (!Application.isPlaying)
        {
            InitSpecialPlayerVars();
            return;
        }
    }

    void Start()
    {
        //pathfind test
        /*var testtimer = System.Diagnostics.Stopwatch.StartNew();
        StartCoroutine(Pathfinding.MakePath(new Vector2(2560, -392+48), new Vector2(2144, -824 + 16), (result) =>
        {
            testtimer.Stop();
            Debug.Log("done in " + testtimer.ElapsedMilliseconds + " ms");
            for (int i = 0; i < result.Count - 1; ++i)
            {
                Debug.DrawLine(result[i], result[i + 1], Color.red, 100f);
            }
        }));*/

        if (Application.isPlaying)
        {
            InitSpecialPlayerVars();
        }

        kh = GetComponent<KHealth>();
        mySprRend = GetComponent<SpriteRenderer>();
        audSrc = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        if (!Application.isPlaying) { return; }
        rig = GetComponent<Rigidbody2D>();
        collisionIgnoredList.Clear();
        myBoxCollider = GetComponent<BoxCollider2D>();
        allMyColliders = GetComponents<Collider2D>();
        disabledAllControlMvt = false; //fix later for more players in a level
        extraPerFrameVel = Vector2.zero;
        zzzzzzzzzz = 3;
        swimCount = rampSmoothCount = 0;
        CCLossDelay = 0;
        momentum = true;
        //Utilities.LoadGame(0);
        //print(Utilities.loadedSaveData);
        lastTimeChangedHMvt = DoubleTime.UnscaledTimeRunning;
        jumpAnimGrace = 0;
        prevTimeScale = 0;
        //stickt = false;
        oldFric = friction;
        speedMultiplier = 1f;
        defaultMat = mySprRend.material;
        glued = 0;
        iced2 = 0;
        ParticleSystem.EmissionModule spremod = sprintParticle.GetComponent<ParticleSystem>().emission;
        spremod.enabled = false;
        foreach (GameObject i in wallDusts)
        {
            ParticleSystem.EmissionModule emod = i.GetComponent<ParticleSystem>().emission;
            emod.enabled = false;
        }
        //sprintParticle.GetComponent<ParticleSystem>().enableEmission = false;
        if (noCustomSpeed)
        {
            moveSpeed = 200;
            jumpHeight = 225;
            maxFallSpeed = 225;
        }

        originalParent = transform.parent;
        wallJumpFrameCountdown = 0;
       // GetComponent<Animator>().SetBool("Running", running);
       // GetComponent<Animator>().SetBool("Jumping", jumping);
        currentHorizontalVelocity = 0;
        swimming = false;
        iced = 0;
        wasSwimming = false;
        fakePhysicsVel = Vector2.zero;
        electroSpriteChange = electroSpriteCurrent = 0;
        grounded = 0;
        spikeTouchCooldown = 0;
        encmt = gameObject.GetComponent<Encontrolmentation>();
        sgt = gameObject.GetComponent<SpecialGunTemplate>();
    }

    // Update is called once per frame

    public void AddBlood(Vector3 at, Quaternion rot)
    {
        if (sgt && sgt is ClickToShieldAndInfJump && (sgt as ClickToShieldAndInfJump).layers > 0)
        {
            return;
        }
        GameObject bloodSpurtNew = Instantiate(bloodSpurt, at, rot) as GameObject;
        GameObject bloodStayNew = Instantiate(bloodStay, at, Quaternion.identity) as GameObject;
        bloodStayNew.transform.SetParent(transform, true);
        //bloodSpurtNew.GetComponent<ParticleSystem>().Play();
        //bloodStayNew.GetComponent<ParticleSystem>().Play();
        Destroy(bloodSpurtNew, 60f);
       // Destroy(bloodStayNew, 3000f);
    }

    public void AddBurn(Vector3 at, float rot)
    {
        GameObject laserNew = Instantiate(laserBurn, at, Quaternion.AngleAxis(rot,Vector3.forward)) as GameObject;
        laserNew.transform.SetParent(transform, true);
       // Destroy(laserNew, 3000f);
    }

    /*void OnCollisionExit2D(Collision2D coll)
    {
        if ((coll.gameObject.layer == 8 || coll.gameObject.layer == 9 ) && coll.GetContact(0).normal.y > 0.1f && coll.GetContact(0).normal.x < 0.9f && coll.gameObject.CompareTag("PlatformStand"))
        {
            transform.parent = originalParent;
        }
    }*/

    public bool IsPunchingOrKicking()
    {
        return punchRBox.enabled || kickRBox.enabled;
    }

    private void MinTranslateVector(Collision2D coll, bool entered)
    {
        // ignoring the wall slider rigidbody without conditions creates some problems with fast moving platforms when they're moving away.
        if (coll.rigidbody == fakePlatform) { return; }
        if (coll.rigidbody == wallSliderMoverR2 && WallSlidePlatformIsMovingTowards()) { return; }
        if (usedAntiThrashThisFrame == 3) { return; }
        if (IsPunchingOrKicking()) { return; }

        //move them slightly for minor inside-the-ground crap
        // coll.GetContact(0).separation is negative when they are overlapping

        // without this check, it's possible to push players inside the ground!!!!
        if (dontMinTranslateThisFrame || coll.gameObject.layer == 20) { dontMinTranslateThisFrame = true; return; }

        float sep = coll.GetContact(0).separation;
        if (sep < -5f) { sep = -5f; } //max move
        minTransDir += coll.GetContact(0).normal*(-sep);
        if ((Mathf.Abs(Vector2.Dot(coll.GetContact(0).normal, transform.right)) > 0.7f && sep < -0.005f) || sep < -0.05f)
        {
            transform.position += (Vector3)(coll.GetContact(0).normal * (-sep * (entered ? 0.75f : 0.5f)));
            //GetComponent<Rigidbody2D>().MovePosition(transform.position);
        }
    }

    private bool WallSlidePlatformIsMovingTowards() // lol!
    {
        Vector2 fakeRight = (transform.lossyScale.x < 0f) ? -transform.right : transform.right;
        Vector2 fr2 = fakeRight * (mySprRend.flipX ? -1f : 1f);
        // <= 0f is to prevent double-correcting wall sliding while platform is moving straight up or down
        // (0.1f due to imprecision)
        if (wallSliderMoverPDM)
        {
            return Vector2.Dot(fr2, wallSliderMoverPDM.GetVelocitation()) <= 0.1f;
        }
        else if (wallSliderMoverR2)
        {
            return Vector2.Dot(fr2, wallSliderMoverR2.velocity) <= 0.1f;
        }
        return false;
        /*return (wallSliderMoverPDM && Vector2.Dot(fr2, wallSliderMoverPDM.GetVelocitation()) <= 0.1f)
            || (wallSliderMoverR2 && Vector2.Dot(fr2, wallSliderMoverR2.velocity) <= 0.1f);*/
    }

    private int usedAntiThrashThisFrame = 0;

    // avoid "thrashing" when trying to move towards a moving platform.
    // "thrashing" = trying to slide on the side of a moving platform moving away, but fails due to being too high or low, and the player is stopped.
    private void AntiThrash(Collision2D coll) 
    {
        if (usedAntiThrashThisFrame != 0
            || !coll.rigidbody 
            || coll.rigidbody.velocity.magnitude < 1f
            || coll.gameObject.tag == "Player") { return; }

        usedAntiThrashThisFrame = 1;

        if (wallSliderMoverR2)
        {
            if (!WallSlidePlatformIsMovingTowards()) { return; } // fixes diagonally moving platforms
            if (wallSliderMoverPDM) { extraPerFrameVel += wallSliderMoverPDM.GetVelocitation(); }
            else { extraPerFrameVel += wallSliderMoverR2.velocity; }
            return;
        }

        //side of boxes is not sensible to stick to.
        if (!wallSliding && coll.rigidbody.GetComponent<FakeFrictionOnObject>()) { return; }

        usedAntiThrashThisFrame = 2;

        Vector2 v = coll.rigidbody.velocity;
        primDecorationMoving pdm = coll.rigidbody.GetComponent<primDecorationMoving>();
        if (pdm) { v = pdm.GetVelocitation(); }

        float d1 = Vector2.Dot(RotateVector2(coll.GetContact(0).point - (Vector2)transform.position, transform.eulerAngles.z).normalized, v);
        if (/*Mathf.Abs(Vector2.Dot(v, RotateVector2(fakePhysicsVel, transform.eulerAngles.z))) >= -0.05f
            &&*/ Mathf.Abs(d1) > 0.4f
            && Mathf.Abs(Vector2.Dot(coll.GetContact(0).normal, transform.right)) > 0.3f
            && (Mathf.Abs(fakePhysicsVel.x) > 5f || d1 < -0.4f)) // prevent moving up while hugging the side of vertically moving platform
        {
            usedAntiThrashThisFrame = 3;
            extraPerFrameVel += v;
        }
    }

    private IEnumerator YouCantIgnoreMeForever(Collider2D c1, Collider2D c2)
    {
        collisionIgnoredList.Add(new Collider2D[] { c1, c2 });

        if (collisionIgnoredList.Count == 1)
        {
            while (CCLossDelay != 0)
            {
                yield return new WaitForEndOfFrame();
            }

            for (int i = 0; i < collisionIgnoredList.Count; ++i)
            {
                Collider2D[] pair = collisionIgnoredList[i];
                if (pair[0] && pair[1])
                {
                    Physics2D.IgnoreCollision(pair[0], pair[1], false);
                }
            }
            collisionIgnoredList.Clear();
            yield break;
        }

        yield return null;
    }

    public bool ColliderIsImpervious(Collider2D c2)
    {
        if (!c2) { return false; }
        SpriteRenderer sr = c2.GetComponent<SpriteRenderer>();
        primExtraTags pet = c2.GetComponent<primExtraTags>();
        return (sr && sr.sprite.name.StartsWith(Utilities.imperviousBlockName)) || (pet && pet.tags.Contains("ImperviousBlock"));
    }

    public bool AntiCollisionStuff(Collision2D coll)
    {
        if (!CanCollide)
        {
            SpriteRenderer sr = coll.collider.GetComponent<SpriteRenderer>();
            primExtraTags pet = coll.collider.GetComponent<primExtraTags>();
            if ((sr && sr.sprite.name.StartsWith(Utilities.imperviousBlockName)) || (pet && pet.tags.Contains("ImperviousBlock")))
            {
                return false;
            }
            else if (!coll.collider.isTrigger)
            {
                Physics2D.IgnoreCollision(coll.otherCollider, coll.collider, true);
                StartCoroutine(YouCantIgnoreMeForever(coll.otherCollider, coll.collider));
                return true;
            }
        }
        return false;
    }

    public void IgnoreCollisionFromOutside(Collider2D other)
    {
        // need to be non-cancollide
        if (CanCollide || CCLossDelay == 0) { return; }
        //ground, enemy, or player
        if (((1 << other.gameObject.layer) & (256+512+2048+1048576)) == 0) { return; }
        if (other.gameObject.layer == 11 && other.gameObject.tag == "Beam") { return; }

        if (Physics2D.GetIgnoreCollision(other, myBoxCollider)) { return; }

        SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
        primExtraTags pet = other.GetComponent<primExtraTags>();

        if (!((sr && sr.sprite.name.StartsWith(Utilities.imperviousBlockName)) || (pet && pet.tags.Contains("ImperviousBlock"))))
        {
            for (int i = 0; i < allMyColliders.Length; ++i)
            {
                Physics2D.IgnoreCollision(allMyColliders[i], other, true);
                StartCoroutine(YouCantIgnoreMeForever(allMyColliders[i], other));
            }
        }

    }

    private List<Collider2D> sideColliders = new List<Collider2D>();
    private List<int> sideColliderRubTime = new List<int>();

    private void Bonk()
    {
        audSrc.Stop();
        if (mySprRend.isVisible)
        {
            ceilHit.Play();
        }
        /*if (coll.gameObject.GetComponent<MessageBumpBox>())
        {
            coll.gameObject.GetComponent<MessageBumpBox>().Bump(4f);
        }*/
        fakePhysicsVel = new Vector2(fakePhysicsVel.x, -maxFallSpeed * ModJumpMults[(int)movementModifier] / Time.timeScale);
    }

    private int jumpBuffer = 0;
    private bool jumpButtonDidNothingThisFrame = false;
    private bool JumpButtonDown(bool useBuffer = true)
    {
        if (Time.timeScale == 0) { return false; }
        if (jumpBuffer > 0 && useBuffer) { return true; }
        return ((encmt.currentState & 16UL) == 16UL) && ((encmt.flags & 16UL) == 16UL);
    }

    public void OnCollisionEnter2D(Collision2D coll)
    {
        if (AntiCollisionStuff(coll)) { return; }
        AntiThrash(coll);

        Vector2 fakeUp = (transform.lossyScale.y < 0f) ? -transform.up : transform.up;

        if (coll.rigidbody && !coll.rigidbody.isKinematic && Vector2.Dot(coll.GetContact(0).normal, fakeUp) > 0.7f)
        {
            AddFakePlatform(coll.GetContact(0).point, coll.rigidbody, coll.rigidbody.gameObject);
        }

        if (Mathf.Abs(Vector2.Dot(coll.GetContact(0).normal, transform.right)) > 0.9f && grounded > 0 && coll.gameObject.layer != 20) //climb step
        {
            CorrectJump(mySprRend.flipX, true);
        }

        if (iced2 > 0 && coll.gameObject.CompareTag("PlatformStand") && coll.rigidbody != null && !coll.rigidbody.isKinematic) //for momentum
        {
            float avgX = 0.5f* (coll.rigidbody.velocity.x + iced) / (coll.rigidbody.mass + rig.mass);
            iced = avgX;
            coll.rigidbody.velocity = new Vector2(avgX, coll.rigidbody.velocity.y);
        }

        if ((coll.gameObject.layer == 8 || coll.gameObject.layer == 9 || coll.gameObject.layer == 11) && coll.transform.parent != transform)
        {
            // Stop the player's bottom edge from "hooking" onto moving platforms.
            Vector3 clp = transform.InverseTransformPoint(coll.GetContact(0).point);
            Vector2 fakeRight = (transform.lossyScale.x < 0f) ? -transform.right : transform.right;
            if (Mathf.Abs(clp.y) > 16f && -Mathf.Sign(clp.y) * fakePhysicsVel.y > 10f && coll.rigidbody && coll.rigidbody.velocity.magnitude > 1f)
            {
                float trx = Mathf.Abs(Vector2.Dot(coll.rigidbody.velocity, fakeRight)) / 60f;
                transform.position += (Vector3)fakeRight * (clp.x < 0f ? trx : -trx);
            }
            
            /*if (coll.GetContact(0).normal.y > 0.8f && coll.GetContact(0).normal.x < 0.2f)
            {
                //Jump Script
                if (coll.gameObject.CompareTag("PlatformStand"))
                {
                    transform.parent = coll.transform;
                }
                if (grounded == false)
                {
                    //Destroy(Instantiate(prefabDust, transform.position - new Vector3(0f, 15.5f, 0f), Quaternion.identity), 0.4f);
                    
                }
            }*/

            float fudgeFactor = Vector2.Dot((Vector2)transform.position - coll.GetContact(0).point, fakeRight); // this is certified rotation-neutral
            if (coll.gameObject.layer != 11 && Vector2.Dot(coll.GetContact(0).normal, -fakeUp) > 0.9f 
                && (coll.collider.GetComponent<SpriteShapeRenderer>() || Vector2.Dot(coll.collider.bounds.center - transform.position, fakeUp) > 0f) )
            {
                // corner fudge: jump through one-block upper gaps
                RaycastHit2D upperHit = Physics2D.Raycast(transform.position + (Vector3)(13f * Mathf.Abs(transform.localScale.y) * fakeUp), fakeUp, 8f, 256 + 512 + 2048);
                if (upperHit.collider == null)
                {
                    if (Mathf.Abs(fudgeFactor) < 5.9999f * Mathf.Abs(transform.localScale.x))
                    {
                        float intendedVel = fakePhysicsVel.x + (((encmt.currentState & 3UL) == 1UL) ? -1000 : 0) + (((encmt.currentState & 3UL) == 2UL) ? 1000 : 0) / Time.timeScale;
                        if (Mathf.Abs(intendedVel) < 10 / Time.timeScale || Mathf.Sign(fudgeFactor) != -Mathf.Sign(intendedVel))
                        {
                            // uses velocity instead of position to avoid edge cases of going into the ground
                            extraPerFrameVel += 60f * fakeRight * Mathf.Sign(fudgeFactor) * ((6.49f * Mathf.Abs(transform.localScale.x)) - Mathf.Abs(fudgeFactor)) / Time.timeScale;
                            fakePhysicsVel = new Vector2(fakePhysicsVel.x, fakePhysicsVel.y - (fakeGravity.y/Time.timeScale));
                        }
                        else
                        {
                            Bonk();
                        }
                        
                    }
                    
                }
                else
                {
                    Bonk();
                }

            }
            //if (Vector2.Dot(coll.GetContact(0).normal, Vector2.down) > 0.99f && coll.gameObject.CompareTag("Glue"))
            //{
            /*if (!stickt)
            {
                fakePhysicsVel = new Vector2(fakePhysicsVel.x, 0);
                stickt = true;
            }*/
            //}
            float rightdot = Vector2.Dot(fakeRight, coll.GetContact(0).normal);
            if (Mathf.Abs(rightdot) > 0.99f)
            {
                sideColliders.Add(coll.collider);
                sideColliderRubTime.Add(0);
            }

            if (momentumMode)
            {
                PrimBreakable pbr = coll.gameObject.GetComponent<PrimBreakable>();
                if (pbr)
                {
                    Vector2 space = transform.position - coll.transform.position;
                    pbr.BreakIt(1000, Mathf.Rad2Deg * Mathf.Atan2(space.y, space.x));
                }
                else
                {
                    
                    if (rightdot > 0.99f && !momentum)
                    {
                        FollowThePlayer ftp = Camera.main.GetComponent<FollowThePlayer>();
                        //fakePhysicsVel = Vector2.up * (jumpHeight/Time.timeScale);
                        momentum = true;
                        momentumBounce.Stop();
                        momentumBounce.Play();
                        ftp.vibSpeed += 1.5f/(ftp.vibSpeed+1f);
                        
                    }
                    if (rightdot < -0.99f && momentum)
                    {
                        FollowThePlayer ftp = Camera.main.GetComponent<FollowThePlayer>();
                        //fakePhysicsVel = Vector2.up * (jumpHeight / Time.timeScale);
                        momentum = false;
                        momentumBounce.Stop();
                        momentumBounce.Play();
                        ftp.vibSpeed += 1.5f / (ftp.vibSpeed + 1f);
                    }
                }
            }
        }

        // very archaic script region! but some things depend on it... like icicles
        if (coll.gameObject.layer == 9 && spikeTouchCooldown == 0)
        {
            foreach (var spik in coll.gameObject.GetComponents<SpikeDirectionSetter>())
            {
                if (Vector2.Dot(coll.GetContact(0).normal, spik.directionToDie) > 0.75f)
                {
                    fakePhysicsVel = new Vector2(spik.directionToDie.x * 260 / Time.timeScale, spik.directionToDie.y * 200 / Time.timeScale);
                    kh.ChangeHealth(-Damage,spik.deathReason);
                    audSrc.clip = spikeTouchSound;
                    audSrc.PlayOneShot(spikeTouchSound);
                    Vector3 v3a = new Vector3(coll.GetContact(0).point.x, coll.GetContact(0).point.y);
                    AddBlood(v3a, Quaternion.LookRotation(v3a, Vector3.up));
                    spikeTouchCooldown = 5;
                    break;
                    //Object.Destroy(this.gameObject);
                }
            }
        }

        if (coll.gameObject.CompareTag("Axe")) // This obsolete code is terrible
        {
                fakePhysicsVel = 350f*coll.GetContact(0).normal / Time.timeScale;
                kh.ChangeHealth(-Damage,"axe");
                audSrc.PlayOneShot(spikeTouchSound);
            Vector3 v3a = new Vector3(coll.GetContact(0).point.x, coll.GetContact(0).point.y);
            AddBlood(v3a, Quaternion.LookRotation(v3a, Vector3.up));           
            //Object.Destroy(this.gameObject);
        }
        MinTranslateVector(coll, true);
    }

    public void OnCollisionStay2D(Collision2D coll)
    {
        AntiCollisionStuff(coll);
        AntiThrash(coll);

        if (Mathf.Abs(Vector2.Dot(coll.GetContact(0).normal, transform.right)) > 0.9f && grounded > 0 && coll.gameObject.layer != 20) //climb step
        {
            CorrectJump(mySprRend.flipX, true);
        }

        MinTranslateVector(coll, false);

        int i = sideColliders.IndexOf(coll.collider);
        if (i == -1) { return; }
        sideColliderRubTime[i] = Mathf.Min(6, sideColliderRubTime[i] + 1);
        Vector2 fakeRight = (transform.lossyScale.x < 0f) ? -transform.right : transform.right;
        bool collisionIsOpposing = Vector2.Dot(coll.GetContact(0).normal, Mathf.Sign(-fakePhysicsVel.x) * fakeRight) > 0.99f;
        bool movingInPlatformDirection = coll.rigidbody && Vector2.Dot(coll.rigidbody.velocity.normalized, Mathf.Sign(fakePhysicsVel.x) * fakeRight) >= 0.1f;

        if (fakePhysicsVel.x != 0 && sideColliderRubTime[i] == 6 && collisionIsOpposing && !movingInPlatformDirection)
        {
            fakePhysicsVel = new Vector2(0, fakePhysicsVel.y);
        }
    }

    public void OnCollisionExit2D(Collision2D coll)
    {
        int i = sideColliders.IndexOf(coll.collider);
        if (i == -1) { return; }
        sideColliders.RemoveAt(i);
        sideColliderRubTime.RemoveAt(i);
    }

    public void OnToggleSwimming(bool t)
    {
        swimming = t;
        if (!t)
        {
            gravMultiplier = 1f;
            doubleJump = true;
            SpecialGunTemplate sgt = GetComponent<SpecialGunTemplate>();
            bool holdingUp = (encmt.currentState & 4UL) == 4UL && !(sgt && sgt.mvtLocked);
            if (!disabledAllControlMvt && (holdingUp || (encmt.currentState & 16UL) == 16UL)) //holding up or A
            {
                fakePhysicsVel = new Vector2(fakePhysicsVel.x, jumpHeight * ModJumpMults[(int)movementModifier] / Time.timeScale);
                grounded = Mathf.Max(0,grounded-1);
            }
        }
        else
        {
            gravMultiplier = 0f;
        }

        if (waterSplashSound)
        {
            waterSplashSound.Stop();
            waterSplashSound.Play();
        }
    }

    public Transform CheckMoving(Rigidbody2D r2)
    {
        if (r2 && r2.gameObject == null) //if the object is about to be destroyed
        {
            return null;
        }

        Rigidbody2D curr = r2;
        int maxDepth = 50;
        while (curr && maxDepth > 0)
        {
            if (curr.transform.hasChanged) //&& (curr.velocity.sqrMagnitude > 0f || curr.angularVelocity != 0f))
            {
                return curr.transform;
            }
            if (curr.transform.parent)
            {
                curr = curr.transform.parent.GetComponent<Rigidbody2D>();
            }
            --maxDepth;
        }

        return null;
    }

    private Vector2 fakePlatformRotationArm = Vector2.zero;
    private primDecorationMoving fakePlatformPDM = null;

    public void SwitchBetweenPlayerCushion(bool on)
    {
        if (!betweenPlayersCushionT)
        {
            betweenPlayersCushionT = gameObject.AddComponent<BoxCollider2D>();
            betweenPlayersCushionT.size = new Vector2(11.9f, 6f);
            betweenPlayersCushionT.offset = new Vector2(0f, 8.1f);
        }
        betweenPlayersCushionT.enabled = on;
    }

    public bool PlayerHasSameRotation(Transform other)
    {
        float t = transform.eulerAngles.z + (transform.localScale.y < 0 && transform.localScale.x > 0 ? 180f : 0f);
        t = (t + 360f) % 360f;
        float o = other.eulerAngles.z + (other.localScale.y < 0 && other.localScale.x > 0 ? 180f : 0f);
        o = (o + 360f) % 360f;
        return Mathf.Approximately(t, o);
    }

    public void AddFakePlatform(Vector2 rayPoint, Rigidbody2D r2, GameObject curr)
    {
        Transform pt = CheckMoving(r2);
        if (pt && pt.GetComponent<Rigidbody2D>())
        {
            // special check for when players are standing on another player of different rotation.
            if (curr.gameObject.layer != 20 || PlayerHasSameRotation(curr.gameObject.transform))
            {
                fakePlatform = pt.GetComponent<Rigidbody2D>();
                fakePlatformRotationArm = rayPoint - (Vector2)pt.TransformPoint(fakePlatform.centerOfMass);
                fakePlatformPDM = fakePlatform.GetComponent<primDecorationMoving>();
            }
            //transform.SetParent(pt, true);
        }
    }

    Collider2D lastJumpRecoveredFromThis = null;

    public void GroundSelf(RaycastHit2D ray)
    {
        Collider2D col = ray.collider;
        lastJumpRecoveredFromThis = col;
        jumping = false;
        doubleJump = true;

        Rigidbody2D r2 = null;
        GameObject curr = col.gameObject;
        while (true)
        {
            r2 = col.GetComponentInParent<Rigidbody2D>();
            if (r2 != null) { break; }
            if (curr.transform.parent == null) { break; }
            curr = curr.transform.parent.gameObject;
        }
        
        /* if (col.GetComponent<PrimMovingPlatform>())
        {
            fakePhysicsVel += col.GetComponent<PrimMovingPlatform>().velocity*(1f-friction);
        }*/

        if (running && grounded == 0 && !((audSrc.clip == walkSound || audSrc.clip == sprintWalkSound) && audSrc.isPlaying)) //fix this bug: first jump not playing jump sound. cause is this block here
        {
            audSrc.clip = (speedMultiplier >= 2f) ? sprintWalkSound : walkSound;
            audSrc.loop = true;
            audSrc.Play();
        }
            /*if (col.gameObject.layer == 11 && transform.position.y < col.transform.position.y + 25.5f)
            {
            transform.position = new Vector3(transform.position.x,col.transform.position.y+26.5f, transform.position.z);
            fakePhysicsVel = new Vector2(fakePhysicsVel.x, 0);
            }*/
        if (grounded == 0 /*&& extraPlat == null*/ && !Physics2D.GetIgnoreCollision(GetComponent<BoxCollider2D>(),col))
        {
            if (kh.electrocute == 0f && mySprRend.isVisible)
            {
                landSound.volume = Mathf.Max(0.05f, -fakePhysicsVel.y / (maxFallSpeed * 2f * Time.timeScale));
                landSound.Play();
            }

            if (electronTracker && !col.GetComponent<AtomDoor>()) { electronTracker.LoseOne(); }
        }
        grounded = groundedGraceFrames;
        AddFakePlatform(ray.point, r2, curr);

        if (curr.gameObject.layer == 20)
        {
            if (!betweenPlayersCushionB)
            {
                betweenPlayersCushionB = gameObject.AddComponent<BoxCollider2D>();
                betweenPlayersCushionB.size = new Vector2(11.9f, 6f);
                betweenPlayersCushionB.offset = new Vector2(0f, -15.1f);
            }
            betweenPlayersCushionB.enabled = true;
            if (PlayerHasSameRotation(curr.gameObject.transform))
            {
                betweenPlayersCushionTOther = curr.gameObject.GetComponent<BasicMove>();
                betweenPlayersCushionTOther.SwitchBetweenPlayerCushion(true);
            }
        }
        else if (betweenPlayersCushionB)
        {
            betweenPlayersCushionB.enabled = false;
            if (betweenPlayersCushionTOther)
            {
                betweenPlayersCushionTOther.SwitchBetweenPlayerCushion(false);
                betweenPlayersCushionTOther = null;
            }
        }

        /*else
        {
            Unparent();
            if (pt)
            {
                extraPerFrameVel += pt.GetComponent<Rigidbody2D>().velocity;
            }
        }*/
    }

    static List<int> movingBlocksThisFrame = new List<int>();

    /*public void OnCollisionStay2D(Collision2D coll)
    {
        Rigidbody2D r2 = coll.gameObject.GetComponent<Rigidbody2D>();
        if (!coll.gameObject.GetComponent<PrimMovingPlatform>() && r2 && !movingBlocksThisFrame.Contains(r2.gameObject.GetInstanceID()))
        { // new movement junk
            Vector2 tv = r2.velocity;
            Vector2 av = Vector3.Cross(Vector3.forward * r2.angularVelocity, coll.GetContact(0).point - ((Vector2)coll.gameObject.transform.position));
            Vector2 f = (tv + av) * 2;
            extraPerFrameVel += f;
            //print(DoubleTime.ScaledTimeSinceLoad);
            movingBlocksThisFrame.Add(r2.gameObject.GetInstanceID());
        }
    }*/

    //also there are overrides, punch/kick takes precedence over idle
    static Dictionary<int, int> prec = new Dictionary<int, int>()
        {
            {Animator.StringToHash("PunchUpRight"),0},
            {Animator.StringToHash("KickDownRight"),0},
            {Animator.StringToHash("KhalHurt"), -1},
        };

    public void SetAnim(string name)
    {
        Animator a = animator;
        AnimatorStateInfo asi = a.GetCurrentAnimatorStateInfo(0);
        if (!asi.IsName(name))
        {
            int hashName = Animator.StringToHash(name);
            int c1 = prec.ContainsKey(asi.shortNameHash) ? prec[asi.shortNameHash] : int.MinValue;
            int c2 = prec.ContainsKey(hashName) ? prec[hashName] : int.MinValue;
            if (asi.normalizedTime < 1f && c1 > c2)
            {
                return;
            }
            else if (idleCheck > 0 || name == "Idle2") //idle check... should this be here?
            {
                a.CrossFade(name, 0f);
            }
        }
    }

    private List<GameObject> raycastSwap = new List<GameObject>();
    private List<int> raycastSwapLayer = new List<int>();

    private void ClearRaycastSwap()
    {
        if (raycastSwap.Count > 0)
        {
            for (int i = 0; i < raycastSwap.Count; ++i)
            {
                raycastSwap[i].layer = raycastSwapLayer[i];
            }
            raycastSwap.Clear();
            raycastSwapLayer.Clear();
        }
    }

    void CorrectJump(bool backwards, bool forStep = false)
    {
        if (!wallSliding && fakePhysicsVel.y <= 0 && (grounded == 0 || forStep))
        {
            RaycastHit2D zr = new RaycastHit2D();
            float conn = Mathf.Max(3f + 6f * (1f - ((maxFallSpeed + (fakePhysicsVel.y*Time.timeScale)) / maxFallSpeed)),0f)*transform.lossyScale.y;
            if (grounded != 0) { conn = 9f * transform.lossyScale.y; }
            float negative = backwards ? -1f : 1f;
            Vector3 fakeUp = (transform.lossyScale.y < 0) ? -transform.up : transform.up;
            Vector3 fakeRight = (transform.lossyScale.x < 0) ? -transform.right : transform.right;
        // name one thing this line does.

        corrJumpRay:

            Vector2 rayOrigin = transform.position + (Vector3)RotateVector2(
                    new Vector2(
                        negative * (6 + ((0.016666666f / Time.timeScale) * moveSpeed * speedMultiplier)) * transform.lossyScale.x,
                        -19 * transform.lossyScale.y + conn)
                    , transform.eulerAngles.z);
            zr = Physics2D.Raycast(rayOrigin, -fakeUp, conn, 2816);

            // try again if the collision is ignored.
            if (zr.collider && Physics2D.GetIgnoreCollision(zr.collider, allMyColliders[0]))
            {
                raycastSwap.Add(zr.collider.gameObject);
                raycastSwapLayer.Add(zr.collider.gameObject.layer);
                zr.collider.gameObject.layer = 0;

                goto corrJumpRay;
            }

            //jumpUpRay:
            RaycastHit2D upr = Physics2D.Raycast(rayOrigin, fakeUp, 33f * transform.lossyScale.y - conn, 2816);

            Vector2 upr2Origin = transform.position + (Vector3)RotateVector2(
                    new Vector2(0, -19 * transform.lossyScale.y + conn)
                , transform.eulerAngles.z);
            RaycastHit2D upr2 = Physics2D.Raycast(upr2Origin, fakeUp, 33f * transform.lossyScale.y - conn, 2816);

            /*if (upr.collider && Physics2D.GetIgnoreCollision(upr.collider, allMyColliders[0]))
            {
                raycastSwap.Add(upr.collider.gameObject);
                raycastSwapLayer.Add(upr.collider.gameObject.layer);
                upr.collider.gameObject.layer = 0;

                goto jumpUpRay;
            }*/



            if (iced2 == 0 
                && zr.collider != null 
                && upr.collider == null 
                && upr2.collider == null 
                && !zr.collider.isTrigger 
                && zr.distance > 0 
                && Vector2.Dot(zr.normal, fakeUp) > 0.99f
                && !Physics2D.GetIgnoreCollision(GetComponent<BoxCollider2D>(), zr.collider))
            {
                Rigidbody2D platPossible = zr.collider.attachedRigidbody;
                if (platPossible)
                {
                    if (platPossible.GetComponent<primDecorationMoving>())
                    {
                        transform.position += (Vector3)platPossible.GetComponent<primDecorationMoving>().GetVelocitation() * 0.01666666f;
                    }
                    else
                    {
                        transform.position += (Vector3)platPossible.velocity * 0.01666666f;
                    }
                }
                transform.position += (negative * fakeRight) + ((conn - zr.distance) * fakeUp);
                rig.MovePosition(transform.position);
            }

            ClearRaycastSwap();
        }
    }

    public void TurnOffCollision()
    {
        CanCollide = false;
        CCLossDelay = Mathf.Max(3, Mathf.RoundToInt(1.5f / Time.timeScale));
    }

    public Vector2 RotateVector2(Vector2 old, float degrees)
    {
        float AC = Mathf.Cos(degrees * Mathf.Deg2Rad);
        float AS = Mathf.Sin(degrees * Mathf.Deg2Rad);
        return new Vector2(old.x * AC - old.y * AS, old.x * AS + old.y * AC);
    }

    public Vector2 RotateVector2(Vector2 old)
    {
        return RotateVector2(old, transform.eulerAngles.z);
    }

    public Vector2 RotateVector2Back(Vector2 old)
    {
        float AC = Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad);
        float AS = Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad);
        return new Vector2(old.x * AC + old.y * AS, - old.x * AS + old.y * AC);
    }

    public void TurnOffCollisionForever() //cheat
    {
        turnOffCollisionForever = true;
    }

    private void MoveByFakePlatform()
    {
        if (fakePlatform != null /*&& fakePlatform.velocity.y < 0f*/ && !jumping && iced == 0)
        {
            //fakePhysicsVel += fakePlatform.velocity; //Vector2.up * fakePlatform.velocity.y;
            Vector2 fpv = fakePlatform.velocity;
            if (fakePlatformPDM)
            {
                fpv = fakePlatformPDM.GetVelocitation();
            }
            extraPerFrameVel += fpv; // It has to be this way to stick to platforms moving down
            if (fakePlatform.gameObject.layer == 20)
            {
                // try to center the top player to make things less finicky with "totem"s
                if ((encmt.currentState & 3UL) == 0UL || (encmt.currentState & 3UL) == 3UL)
                {
                    float dp = transform.InverseTransformPoint(fakePlatform.transform.position).x;
                    Vector2 fakeUp = (transform.lossyScale.y < 0) ? -transform.up : transform.up;
                    extraPerFrameVel += 24f * dp * (Vector2)transform.right - 16f * fakeUp;
                }
            }
            float angularDegreesChange = fakePlatform.angularVelocity * 0.016666666f * Time.timeScale;
            Vector2 angularFrameMvt = RotateVector2(fakePlatformRotationArm, angularDegreesChange) - fakePlatformRotationArm;
            if (angularFrameMvt.y < 0) { angularFrameMvt = new Vector2(angularFrameMvt.x, 2f * angularFrameMvt.y); }
            extraPerFrameVel += angularFrameMvt * 60f;
        }
    }

    private void VelocitateAndLeavePlatform(bool canBeLate = false)
    {
        if (!fakePlatform) { return; }
        Vector2 v = (fakePlatformPDM) ? fakePlatformPDM.GetVelocitation(canBeLate) : fakePlatform.velocity;
        fakePhysicsVel += RotateVector2(v, -transform.eulerAngles.z);
        extraPerFrameVel -= v;
        fakePlatform = null;
        fakePlatformPDM = null;
    }

    // deprecated
    public void Unparent()
    {
        fakePlatform = null;
        fakePlatformPDM = null;
    }

    public bool IsSprinting()
    {
        return speedMultiplier >= 2f;
    }

    private float velComp;

    void LateUpdate()
    {
        if (!Application.isPlaying) { return; }
        if (turnOffCollisionForever) { TurnOffCollision(); }

        boosted = false;
        Vector2 rampNormal = Vector2.down;

        fricMultiplier = 1f;//Time.timeScale;
        if (iced > 0)
        {
            fricMultiplier = 0f;
        }
        if (CanCollide)
        {
            MoveByFakePlatform();
        
            if (Physics2D.GetIgnoreLayerCollision(20, 8))
            {
                CCLossDelay = 0;
                GetComponent<PlatformEffector2D>().useColliderMask = true;
            }
            
        }
        else if (!CanCollide)
        {
            if (!Physics2D.GetIgnoreLayerCollision(20, 8))
            {
                GetComponent<PlatformEffector2D>().useColliderMask = false;
            }
        }

        bool selfTimerRunning = !(Time.timeScale == 0 || (prevTimeScale == 0 && encmt.allowUserInput));
        // Don't jump on unpause
        if (Time.timeScale != 0 && prevTimeScale == 0 && encmt.allowUserInput && Time.time > 0.02f)
        {
            prevTimeScale = Time.timeScale;
            return;
        }

        //materials Block /////////////////////////////////////////////////////////////////////////////
        if (CCLossDelay == 0 && mySprRend.material != defaultMat)
        {
            mySprRend.material = defaultMat;
        }
        else if (CCLossDelay > 0 && mySprRend.material != CanCollideFalseMaterial)
        {
            mySprRend.material = CanCollideFalseMaterial;
            mySprRend.material.SetColor("_GhostColor", ghost ? (new Color(0f, 1f, 1f, 0.55f)) : Color.white);
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////
        //if (DoubleTime.timeScaleCopy == 0 || Time.timeScale == 0 || (prevTimeScale == 0 && encmt.allowUserInput))
        //{
        //    if (CCLossDelay > 0) { CCLossDelay += 1; } //it decreases later, this keeps it the same 
        //}
        //else

        if (selfTimerRunning && notFlipwired)
        {

            if (zzzzzzzzzz >= 1)
            {
                zzzzzzzzzz -= 1;
            }

            SpecialGunTemplate sgt = GetComponent<SpecialGunTemplate>();

            if (youCanInfinityJump)
            {
                doubleJump = true;
            }
            if (grounded > 0 && (disabledAllControlMvt || ((encmt.currentState & 3UL) == 0UL)))
            {
                SetAnim("Idle1");
            }
            if (grounded == 0)
            {
                jumpAnimGrace = Mathf.Max(jumpAnimGrace - 1, 0);
                if (jumpAnimGrace == 0)
                {
                    float YVel = fakePhysicsVel.y / (Time.timeScale == 0 ? 1f : Time.timeScale);
                    animator.speed = (glued > 0)?0f:System.Math.Abs(YVel / maxFallSpeed);
                    if (YVel >= 60 / Time.timeScale)
                    {
                        changeGroundedAnim = true;
                        SetAnim("MovingUp");
                    }
                    else if (YVel <= -80 / Time.timeScale)
                    {
                        changeGroundedAnim = true;
                        SetAnim("MovingDown");
                    }
                    else
                    {
                        changeGroundedAnim = true;
                        SetAnim("Floating");
                    }
                }
            }
            // swimming /////////////
            if (swimming)
            {
                doubleJump = false;
                fricMultiplier *= 2f;
                if (!wasSwimming)
                {
                    OnToggleSwimming(true);
                    wasSwimming = true;
                }
                swimming = false;

            }
            swimCount -= (swimCount == 0) ? (byte)0 : (byte)1;

            if (!swimming)
            {
                if (wasSwimming && swimCount == 0)
                {
                    OnToggleSwimming(false);
                    wasSwimming = false;
                }
            }

            /////////////////////////

            #region new grounding stuff
            if (timeJumpCool == 0)
            {
                // note for raycasting
                // we are assuming the player extends to 18 pixels below.
                // the boxcast starts 1.5 pixels upwards of that though.
                // these values are heavily abused and hardcoded. too bad!!!!

                bool did = false;
                Vector3 fakeUp = (transform.lossyScale.y < 0) ? -transform.up : transform.up;
                float platVel = 0;
                if (fakePlatformPDM != null)
                {
                    platVel = Mathf.Abs(Vector2.Dot(fakePlatformPDM.GetVelocitation(true), fakeUp) * Time.deltaTime);
                }
                else if (fakePlatform != null) {
                    platVel = Mathf.Abs(Vector2.Dot(fakePlatform.velocity, fakeUp) * Time.deltaTime);
                }
                platVel = Mathf.Max(platVel, Mathf.Abs(velComp) * 0.016666666f * Time.timeScale);
                float rdist = 1.5f + Mathf.Abs(transform.lossyScale.y) * 2f + (((extraPlat == null) ? (platVel) /*maybe change 0*/ : Mathf.Max(0f, -extraPlat.dif.y)));
                RaycastHit2D[] r = Physics2D.BoxCastAll(
                    transform.position - fakeUp * 16.5f * Mathf.Abs(transform.lossyScale.y), 
                    new Vector2(Mathf.Abs(12f * transform.lossyScale.x), 0.1f), 
                    transform.eulerAngles.z, -fakeUp,
                    rdist, 
                    2816 + 1048576, transform.position.z - 256, transform.position.z + 256);
                //RaycastHit2D[] nr = Physics2D.BoxCastAll(transform.position - Vector3.up * 13f, new Vector2(12f, 0.1f), transform.eulerAngles.z, Vector3.down, 1f, 2816, transform.position.z - 256, transform.position.z + 256);//(transform.position + new Vector3(6, -13, 0), Vector2.down, 1f, 2816); //(layers 8 9 11)
                bool fakePlatformStandThisFrame = false;

                for (int ir = 0; ir < r.Length; ++ir)
                {
                    RaycastHit2D rr = r[ir];
                    if (rr.collider != null 
                        && rr.collider.gameObject != gameObject 
                        && (rr.collider.gameObject.layer != 20 || transform.InverseTransformPoint(rr.transform.position).y < -4.5f*transform.lossyScale.y) 
                        && (CCLossDelay == 0 || ColliderIsImpervious(rr.collider) || rr.collider.gameObject.layer == 11))
                    {
                        if (rr.rigidbody && rr.rigidbody.GetComponent<primDecorationMoving>())
                        {
                            // Why would this need to account for late velocitation???
                            // That's why it doesn't now.
                            velComp = Vector2.Dot(fakeUp, rr.rigidbody.GetComponent<primDecorationMoving>().GetVelocitation(false));
                        }
                        else if (rr.rigidbody)
                        {
                            velComp = Vector2.Dot(fakeUp, rr.rigidbody.velocity);
                        }

                        if (!rr.collider.isTrigger && Vector2.Dot(rr.normal, fakeUp) >= 0.25f && !Physics2D.GetIgnoreCollision(rr.collider, myBoxCollider) 
                            && (fakePhysicsVel.y + Vector2.Dot(extraPerFrameVel, fakeUp) <= velComp)
                            )
                        {
                            did = true;

                            float hoverCorrection = rr.distance - 1.5f; //Sometimes the player just barely hovers above the block. This was supposed to
                            // make it easier to jump. But it ruins the block behaviors.
                            if (hoverCorrection > 0 
                                && !rr.collider.GetComponent<SuperRay>() 
                                && (!rr.rigidbody || rr.rigidbody.gameObject.layer != 20)
                                && (grounded == 0 || (rr.rigidbody && !rr.rigidbody.isKinematic))
                                )
                            {
                                //print(rr.normal);
                                extraPerFrameVel += -60f * (hoverCorrection + 1.5f) * Time.timeScale * (Vector2)fakeUp;
                            }

                            GroundSelf(rr);
                            if (!rr.collider.gameObject.CompareTag("Ice") /*&& rr.collider.gameObject.layer != 11*/)
                            {
                                float y = fakePlatformPDM ? fakePlatformPDM.GetVelocitation().y : 
                                    (fakePlatform ? fakePlatform.velocity.y * Mathf.Abs(transform.lossyScale.y) : 0);
                                fakePhysicsVel = new Vector2(fakePhysicsVel.x, y);
                            }
                            /* if (fakePhysicsVel.y <= 0 && rr.collider.gameObject.layer == 11)
                             {
                                 fakePhysicsVel -= new Vector2(0, fakePhysicsVel.y);
                             }*/
                            PrimMovingPlatform pmp = rr.collider.GetComponent<PrimMovingPlatform>();
                            if (rr.collider.gameObject.CompareTag("PlatformStand") && pmp != null && pmp.enabled)
                            {
                                /*extraPlat = rr.collider.GetComponent<PrimMovingPlatform>();
                                if (transform.parent != rr.collider.transform)
                                {
                                    transform.parent = rr.collider.transform;
                                }*/
                                //what this do anyway GetComponent<Rigidbody2D>().AddForce( new Vector2(rr.transform.gameObject.GetComponent<PrimMovingPlatform>().dif.x,0)*0.001f,ForceMode2D.Impulse);
                                Rigidbody2D rb1 = rr.rigidbody;
                                extraPlat = rb1.GetComponent<PrimMovingPlatform>();
                                /*if (sliderH == null)
                                {
                                    sliderH = gameObject.AddComponent<SliderJoint2D>();
                                }*/
                                if (sliderV == null)
                                {
                                    sliderV = gameObject.AddComponent<SliderJoint2D>();
                                }
                                sliderV.connectedBody /*= sliderH.connectedBody*/ = rb1;
                                sliderV.autoConfigureAngle /*= sliderH.autoConfigureAngle*/ = false;
                                sliderV.angle = 0f; /*sliderH.angle = 0f;*/
                                sliderV.enableCollision /*= sliderH.enableCollision*/ = true;
                                sliderV.connectedAnchor /*= sliderH.connectedAnchor*/ = new Vector2(transform.position.x - rb1.transform.position.x, 18.5f +rr.collider.bounds.extents.y /*+ ((extraPlat == null) ? 0f : extraPlat.dif.y)*/);
                                
                            }
                            else
                            {
                                //transform.parent = originalParent;
                                DestroyImmediate(sliderV);
                                sliderV = null;
                                /*DestroyImmediate(sliderH);
                                sliderH = null;*/
                                extraPlat = null;
                            }
                            //print(rr.normal);
                            Vector2 rotNormal = RotateVector2Back(rr.normal);
                            if (Vector2.Dot(rotNormal, Vector2.up) < 0.99984f || rampSmoothCount == 0)
                            {
                                rampNormal = rotNormal;
                                rampSmoothCount = 2;
                            }
                            else
                            {
                                --rampSmoothCount;
                            }
                            jumpAnimGrace = 0; //formerly 2

                            if (rr.collider.GetComponent<GlueBlock>())
                            {
                                glued = 2;
                            }
                        }
                    }

                    if (fakePlatform && rr.collider.attachedRigidbody == fakePlatform)
                    {
                        fakePlatformStandThisFrame = true;
                    }
                }
            
                if (!did)
                {
                    VelocitateAndLeavePlatform(false);
                    DestroyImmediate(sliderV);
                    sliderV = null;
                    /*DestroyImmediate(sliderH);
                    sliderH = null;*/
                    extraPlat = null;
                    grounded = (glued>0)?0:Mathf.Max(0, grounded - 1);
                    velComp = 0f;
                }

                if (fakePlatform && !fakePlatformStandThisFrame)
                {
                    VelocitateAndLeavePlatform();
                }
            }
            else
            {
                timeJumpCool--;
            }
            #endregion

            /////////////////////////

            float mvtAngle = -Mathf.Atan2(rampNormal.x, rampNormal.y);

            var sprintParticleEmmision = sprintParticle.GetComponent<ParticleSystem>().emission;
            var sprintParticleShape = sprintParticle.GetComponent<ParticleSystem>().shape;
            if (sprintCounter >= 3 || speedMultiplier > 1.5f)
            {
                sprintParticleEmmision.enabled = true;
                sprintParticleShape.scale = new Vector3(1, 1, (mySprRend.flipX ? -1 : 1));
            }
            else
            {
                sprintParticleEmmision.enabled = false;
            }

            
            float mvSpeedTemp = moveSpeed * ModSpeedMults[(int)movementModifier] * (sgt && sgt.mvtLocked ? 0.00001f : 1f);
            float jumpHeightTemp = jumpHeight * ModJumpMults[(int)movementModifier];
            float gMulTemp = gravityMultiplier * ModGravMults[(int)movementModifier];
            float frictionTemp = (iced > 0)?0:friction;
            byte tempHMovementByte = 0;
            if (grounded == 0)
            {
                frictionTemp *= 0.6f;
                mvSpeedTemp *= 1.3f;
            }
            if ((encmt.currentState & 8UL) == 8UL)
            {
                mvSpeedTemp *= (grounded == 0) ? 1f : 0.4f;
                frictionTemp *= (grounded == 0) ? 2f : 6f;
            }

            #region getting crushed

            if (CanCollide)
            {
            bool oscillatingSmush = Vector2.Dot(minTransDir, prevMinTransDir) < -0.5f;
            Vector3 rd = transform.up + 0.4f * transform.right;
            RaycastHit2D ray1 = Physics2D.Raycast(transform.position + 3.5f * rd, rd, 8 * transform.lossyScale.y, 2816);
            RaycastHit2D ray2 = Physics2D.Raycast(transform.position - 3.5f * rd, -rd, 8 * transform.lossyScale.y, 2816);
            bool movingPlatformSmush = (ray1 && ray2) && (ray1.collider.gameObject != ray2.collider.gameObject && (ray1.rigidbody || ray2.rigidbody));
            
            if (ray1 && ray2 && (oscillatingSmush || movingPlatformSmush))
            {
                if (ray1.transform.gameObject.tag != "Beam" && ray2.transform.gameObject.tag != "Beam"
                    && ray1.collider.isTrigger != true && ray2.collider.isTrigger != true)
                {
                    bool finalRun = true;
                    if (ray1.collider.gameObject.GetComponent<AmorphousGroundTileNormal>())
                    {
                        if (ray1.collider.gameObject.GetComponent<AmorphousGroundTileNormal>().isHidingSomething)
                        {
                            finalRun = false;
                        }
                    }
                    if (ray2.collider.gameObject.GetComponent<AmorphousGroundTileNormal>())
                    {
                        if (ray2.collider.gameObject.GetComponent<AmorphousGroundTileNormal>().isHidingSomething)
                        {
                            finalRun = false;

                        }
                    }
                    if (finalRun 
                            && (Mathf.Abs(minTransDir.magnitude) < 0.1f || oscillatingSmush) 
                            && CanCollide)
                    {
                            kh.ChangeHealth(-Mathf.Infinity,"crushing");
                    }
                }
            }
            #endregion
            }
            //insert esoteric other stuff
            if (timesWhenMoved.Count > 0)
            {
                if (DoubleTime.UnscaledTimeRunning - timesWhenMoved[timesWhenMoved.Count - 1] > 0.25f)
                {
                    sprintCounter = 0;
                }
            }
            if ((encmt.currentState & 3UL) == 0UL || (sgt && sgt.isAiming))
            {
                speedMultiplier = 1;
                // Change sprint walk sound to regular walk sound
                if (audSrc.isPlaying && audSrc.clip == sprintWalkSound)
                {
                    audSrc.Stop();
                    audSrc.clip = walkSound;
                    audSrc.loop = true;
                    audSrc.Play();
                }
            }

            #region electrocution

            if (kh.electrocute > 0f)
            {
                grounded = 0;
                doubleJump = false;
            }

            #endregion

            currentHorizontalVelocity = (speedMultiplier == 0) ? 0 : (fakePhysicsVel.x / speedMultiplier);

            #region not moving logic
            if (disabledAllControlMvt || ((encmt.currentState & 3UL) == 0UL) /*|| ((encmt.currentState & 32UL) == 32UL)*/)
            {
                if (grounded > 0 && (audSrc.clip == walkSound || audSrc.clip == sprintWalkSound) && audSrc.isPlaying)
                {
                    audSrc.Stop();
                }
                wallJumpFrameCountdown = Mathf.Clamp(wallJumpFrameCountdown - 1, 0, 99);
                if (grounded > 0)
                {
                    if (changeGroundedAnim)
                    {
                        SetAnim("Idle1");
                    }
                    idleCheck = (idleCheck <= 0) ? 0 : (idleCheck - 1);
                    animator.speed = 1;
                }
                /*if (wallSliding && wallJumpFrameCountdown == 0)
                {
                    doubleJump = false;
                }*/

                if (wallJumpFrameCountdown == 0)
                {
                    wallSliding = false;
                    TurnOffWallSlideDust();
                    touchingWallAndDirection = new Vector2(0, 0);
                    maxFallSpeed = jumpHeightTemp;
                    wallSliderMoverR2 = null;
                    wallSliderMoverPDM = null;
                }

                
                if (momentumMode)
                {
                    currentHorizontalVelocity = (momentum?1f:-1f)*(1f*mvSpeedTemp / Time.timeScale);
                }
                else
                {
                    currentHorizontalVelocity = Mathf.Lerp(currentHorizontalVelocity, 0, frictionTemp * fricMultiplier * ((grounded > 0) ? 2.5f : 0.5f));
                }

                Vector2 slippage = Vector2.zero;
                if (grounded > 0 && Mathf.Abs(mvtAngle) > 0.13089969f)
                {
                    
                    ulong contud = disabledAllControlMvt?0UL:(encmt.currentState & 12UL);
                    float slipFric = 8f;
                    if (contud == 4UL && !(sgt && sgt.mvtLocked))
                    {
                        slipFric = -1f;
                    }
                    else if (contud == 8UL && !(sgt && sgt.mvtLocked))
                    {
                        slipFric = 100f;
                    }
                    slippage += slipFric*Mathf.Tan(mvtAngle)*(Vector2)Vector3.Cross(-rampNormal, Vector3.forward);
                }

                fakePhysicsVel = new Vector2(currentHorizontalVelocity /*/ Time.timeScale*/+slippage.x, fakePhysicsVel.y+slippage.y);
            }

            //fakePhysicsVel = new Vector2(0, fakePhysicsVel.y);
            #endregion

            #region left right mvt logic

            if (!disabledAllControlMvt && ((encmt.currentState & 3UL) == 2UL))
            {
                if (!(wallJumpFrameCountdown > 0 && touchingWallAndDirection.x == -1))
                {
                    if (grounded > 0)
                    {
                        running = true;
                        if (System.Math.Abs(currentHorizontalVelocity) > ((mvSpeedTemp / speedMultiplier) * 1.5f) / Time.timeScale || speedMultiplier > 1.5f)
                        {
                            SetAnim("Sprint1");
                            animator.speed = 1;
                        }
                        else
                        {
                            SetAnim("Running1");
                            animator.speed = System.Math.Abs(encmt.horizontalPressure); /*System.Math.Abs(currentHorizontalVelocity / (mvSpeedTemp / Time.timeScale))*/;
                        }
                        changeGroundedAnim = true;
                    }

                    CorrectJump(false);

                    WallSlider(new Vector2(1, 0));
                    tempHMovementByte += 1;
                    //transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                    mySprRend.flipX = false;
                    if (iced2 == 0 && Time.timeScale > 0)
                    {
                        if (momentumMode)
                        {
                            currentHorizontalVelocity = (momentum ? 1f : -1f) * (1f * mvSpeedTemp / Time.timeScale);
                        }
                        else
                        {
                            currentHorizontalVelocity = Mathf.Lerp(currentHorizontalVelocity, encmt.horizontalPressure * (mvSpeedTemp / Time.timeScale), frictionTemp * fricMultiplier);
                        }
                        fakePhysicsVel = new Vector2(currentHorizontalVelocity * speedMultiplier, fakePhysicsVel.y);
                    }
                }
                else
                {
                    wallJumpFrameCountdown = Mathf.Clamp(wallJumpFrameCountdown - 1, 0, 99);
                }
            }
            if (!disabledAllControlMvt && ((encmt.currentState & 3UL) == 1UL))
            {
                if (!(wallJumpFrameCountdown > 0 && touchingWallAndDirection.x == 1))
                {
                    if (grounded > 0)
                    {
                        running = true;
                        if (System.Math.Abs(currentHorizontalVelocity) > ((mvSpeedTemp / speedMultiplier) * 1.5f) / Time.timeScale || speedMultiplier > 1.5f)
                        {
                            SetAnim("Sprint1");
                            animator.speed = 1;
                        }
                        else
                        {
                            SetAnim("Running1");
                            animator.speed = System.Math.Abs(encmt.horizontalPressure); /*System.Math.Abs(currentHorizontalVelocity / (mvSpeedTemp / Time.timeScale))*/;
                        }
                        changeGroundedAnim = true;
                    }

                    CorrectJump(true);

                    WallSlider(new Vector2(-1, 0));
                    tempHMovementByte += 2;
                    mySprRend.flipX = true;
                    //transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);

                    if (iced2 == 0 && Time.timeScale > 0)
                    {
                        if (momentumMode)
                        {
                            currentHorizontalVelocity = (momentum ? 1f : -1f) * (1f * mvSpeedTemp / Time.timeScale);
                        }
                        else
                        {
                            currentHorizontalVelocity = Mathf.Lerp(currentHorizontalVelocity, encmt.horizontalPressure * (-mvSpeedTemp / Time.timeScale), frictionTemp * fricMultiplier);
                        }
                        fakePhysicsVel = new Vector2(currentHorizontalVelocity * speedMultiplier, fakePhysicsVel.y);
                    }
                }
                else
                {
                    wallJumpFrameCountdown = Mathf.Clamp(wallJumpFrameCountdown - 1, 0, 99);
                }
        }

            #endregion

            //start sprint
            if (sprintCounter >= 3 && DoubleTime.UnscaledTimeRunning - timesWhenMoved[timesWhenMoved.Count - 1] > 0.14f
                 && ((encmt.currentState & 3UL) == 1UL || (encmt.currentState & 3UL) == 2UL)
                 && (encmt.allowUserInput || (GetComponent<EncontrolmentationAutoMover>()?.canSprint ?? false))
                 && !(sgt && sgt.mvtLocked))
            {
                sprintCounter = 0;
                speedMultiplier = 2;
                sprintStartSound.Stop();
                sprintStartSound.Play();
                // Change regular walk sound to sprint walk sound
                if (audSrc.isPlaying && audSrc.clip == walkSound)
                {
                    audSrc.Stop();
                    audSrc.clip = sprintWalkSound;
                    audSrc.loop = true;
                    audSrc.Play();
                }
                // Drop gun
                sgt.ForceCancel();
            }

            // release from wall
            if (!disabledAllControlMvt && (encmt.currentState & 8UL) == 8UL && wallJumpFrameCountdown > 0)
            {
                wallJumpFrameCountdown = 0;
            }

            #region start jump logic

            jumpButtonDidNothingThisFrame = true;
            if ((movementModifier != MovementMod.Stunned) &&
                !disabledAllControlMvt && JumpButtonDown() && 
                doubleJump && glued == 0 && zzzzzzzzzz == 0 && youCanJump)
            {
                tempHMovementByte += 4;
                timeJumpCool = 1;
                jumping = true;
                DestroyImmediate(sliderV);
                sliderV = null;
                /*DestroyImmediate(sliderH);
                sliderH = null;*/
                jumpButtonDidNothingThisFrame = false;
                jumpBuffer = 0;
                    
                if (grounded == 0)
                {
                    doubleJump = false;
                }
                if (grounded > 0)
                {
                    grounded = 0;
                    if (!youCanDoubleJump) { doubleJump = false; }
                    if (audSrc.isPlaying && (audSrc.clip == walkSound || audSrc.clip == sprintWalkSound))
                    {
                        audSrc.Stop();
                    }
                }
                if (transform.parent != originalParent)
                {
                    transform.parent = originalParent;
                }
                bool wallJumpVelocitate = false;
                if (wallSliding)
                {
                    fakePhysicsVel = new Vector2((-touchingWallAndDirection.x * 12.5f * friction * mvSpeedTemp) / Time.timeScale, (Mathf.Max(0f, 0.5f * fakePhysicsVel.y * Time.timeScale) + jumpHeightTemp) / Time.timeScale);
                    if (wallSliderMoverPDM)
                    {
                        Vector2 pv = RotateVector2(wallSliderMoverPDM.GetVelocitation(true), -transform.eulerAngles.z);
                        if (pv.y < 0f) { pv = new Vector2(pv.x, 0f); }
                        fakePhysicsVel += pv;
                        wallSliderMoverPDM = null;
                        wallSliderMoverR2 = null;
                    }
                    else if (wallSliderMoverR2)
                    {
                        Vector2 pv = RotateVector2(wallSliderMoverR2.velocity, -transform.eulerAngles.z); // less accurate, but fine
                        if (pv.y < 0f) { pv = new Vector2(pv.x, 0f); }
                        fakePhysicsVel += pv;
                        wallSliderMoverR2 = null;
                    }

                    for (int i = 0; i < 3; ++i)
                    {
                        if (!wallSlideColliders[i]) { continue; }
                        IOnNontouchInteractions nt = wallSlideColliders[i].GetComponent<IOnNontouchInteractions>();
                        if (nt != null) { nt.WallJumpOff(gameObject); }
                    }

                    wallSliding = false;
                    wallJumpVelocitate = true;
                    TurnOffWallSlideDust();
                    touchingWallAndDirection = new Vector2(0, 0);
                    wallPushSound.Stop();
                    wallPushSound.Play();
                    maxFallSpeed = jumpHeightTemp;
                    timeJumpCool = 1;
                    wallDoubleJumpDelay = 5;
                }

                if (lastJumpRecoveredFromThis)
                {
                    IOnNontouchInteractions nt = lastJumpRecoveredFromThis.GetComponent<IOnNontouchInteractions>();
                    if (nt != null)
                    {
                        nt.NormalJumpOff(gameObject);
                        lastJumpRecoveredFromThis = null;
                    }
                }

                if (!wallSliding && !wallJumpVelocitate)
                {
                    if (doubleJump)
                    {
                        fakePhysicsVel = new Vector2(fakePhysicsVel.x, /*fakePhysicsVel.y + */(jumpHeightTemp / Time.timeScale)+ ((extraPlat == null) ? 0f : (extraPlat.dif.y / Time.deltaTime)));
                    }
                    if (!doubleJump)
                    {
                        //GetComponent<ConstantForce2D>().force = Vector2.zero;
                        fakePhysicsVel = new Vector2(fakePhysicsVel.x, jumpHeightTemp / Time.timeScale);
                    }
                }
                extraPlat = null;
                BinaryBullet.ToggleAll();
                // add the ability to velocitate off of moving platforms
                if (fakePlatform)
                {
                    VelocitateAndLeavePlatform(true);
                }
            }
            #endregion

            #region extra swimming mvts
            if (!disabledAllControlMvt && wasSwimming)
            {
                if ((encmt.currentState & 20UL) != 0UL)
                {
                    float newVertVelocity = Mathf.Lerp(fakePhysicsVel.y, moveSpeed, frictionTemp * fricMultiplier);
                    fakePhysicsVel = new Vector2(fakePhysicsVel.x, newVertVelocity);
                }
                if ((encmt.currentState & 8UL) == 8UL && !(sgt && sgt.mvtLocked))
                {
                    float newVertVelocity = Mathf.Lerp(fakePhysicsVel.y, /*Mathf.Min(-maxFallSpeed,*/(-moveSpeed), frictionTemp * fricMultiplier);
                    fakePhysicsVel = new Vector2(fakePhysicsVel.x, newVertVelocity);
                    /*
                    if (fakePhysicsVel.y > (-jumpHeightTemp / 2) / Time.timeScale)
                    {
                        fakePhysicsVel = new Vector2(fakePhysicsVel.x, (-jumpHeightTemp / 2) / Time.timeScale);
                    }*/
                }
                if ((encmt.currentState & 4UL) == 4UL && !(sgt && sgt.mvtLocked))
                {
                    if (grounded > 0)
                    {
                        extraPerFrameVel += (Vector2)(moveSpeed * 0.25f * transform.up);
                    }
                }
            }
            #endregion

            #region fake gravity

            if (fakeGravityOverrideFrames <= 0) { fakeGravity = Physics2D.gravity; }
            else { --fakeGravityOverrideFrames; }

            bool pressingDown = (encmt.currentState & 8UL) == 8UL && !(sgt && sgt.mvtLocked);

            if (grounded > 0 && !disabledAllControlMvt /*&& (encmt.currentState & 16UL) != 16UL*/)
            {
                fakePhysicsVel = new Vector2(fakePhysicsVel.x, Mathf.Min(fakePhysicsVel.x * Mathf.Tan(mvtAngle), 0));
            }
            if (udm > 0 && grounded > 0 && !wasSwimming)
            {
                fakePhysicsVel = new Vector2(fakePhysicsVel.x, 24);
            }
            if (!disabledAllControlMvt && (((encmt.currentState & 16UL) != 16UL) && ((encmt.currentState & 4UL) != 4UL || (sgt && sgt.mvtLocked)) && fakePhysicsVel.y > 0 && fakeGravity.y <= 0 && !wasSwimming && udm == 0)  /*|| GetComponent<ConstantForce2D>().force.magnitude > 1*/)
            {
                fakePhysicsVel += Vector2.up * gMulTemp * 2.1f * fakeGravity.y / Time.timeScale;
            }
            if (!disabledAllControlMvt && pressingDown && !wasSwimming && udm == 0 && grounded == 0)
            {
                fakePhysicsVel += Vector2.down * gMulTemp * ((fakeGravity.y >= 0f) ? 12f : 22.5f) / Time.timeScale;
                fakePhysicsVel += Vector2.right * gMulTemp * fakeGravity.x / Time.timeScale;
            }
            else if (!wasSwimming && udm == 0 && extraPlat == null && !(fakeGravity.y > 0f && (encmt.currentState & 12UL) == 8UL))
            {
                fakePhysicsVel += Vector2.up * gMulTemp * fakeGravity.y / Time.timeScale;
                fakePhysicsVel += Vector2.right * gMulTemp * fakeGravity.x / Time.timeScale;
            }

            rig.gravityScale = 0f;
            #endregion

            //this line is BAD.
            fakePhysicsVel = new Vector2((System.Math.Abs(fakePhysicsVel.x)<2f)?0f:fakePhysicsVel.x, 
                Mathf.Clamp(fakePhysicsVel.y, 
                (udm > 0)? (-4f*maxFallSpeed * maxFallSpeedTranqMult / Time.timeScale) : (-maxFallSpeed * (pressingDown ? 1.3f : 1f) * maxFallSpeedTranqMult / Time.timeScale), 
                (wallSliding || timeJumpCool > 0)?1600f:((fakeGravity.y < 0f)?4f:1f)*maxFallSpeed / Time.timeScale));

            bool ohNo = true;

            if (rig.velocity.SqrMagnitude() > 900f || fakePhysicsVel.SqrMagnitude() > 900f || (encmt.currentState & 1023UL) != 0UL)
            {
                idleCheck = idleCheckFrames;
            }

            if (System.Math.Abs(fakePhysicsVel.x) < 10 && grounded > 0)
            {
                running = false;
            }

            glued = Mathf.Max(glued - 1, 0);
            iced2 = Mathf.Max(iced2 - 1, 0);
            udm = Mathf.Max(udm - 1, 0);
            if (ohNo)
            {
                //GetComponent<Animator>().SetBool("Running", running);
                //GetComponent<Animator>().SetBool("Jumping", jumping);
            }

            if (encmt.allowUserInput)
            {
                if (grounded == 0 && !wallSliding)
                {
                    midairRush.volume = 0.5f * (rig.velocity.magnitude / (maxFallSpeed * 2f));
                }
                else
                {
                    midairRush.volume = 0.01f;
                }

                if (wallSliding && !wallSlideSound.isPlaying)
                {
                    wallSlideSound.Play();
                }
                if (!wallSliding && wallSlideSound.isPlaying)
                {
                    wallSlideSound.Stop();
                }
            }

            #region electrocution2

            if (kh.electrocute > 0f)
            {
                fakePhysicsVel = Vector2.zero;
                grounded = 0;
                doubleJump = false;
                SpriteRenderer sr = mySprRend;
                sr.material = ElectrocutionMaterial;
                if (electroSpriteChange <= 0)
                {
                    electroSpriteCurrent = Fakerand.Int(0, electrocutedSprites.Length);
                    electroSpriteChange = 3;
                }
                sr.sprite = electrocutedSprites[electroSpriteCurrent];
                electroSpriteChange--;
            }
            else
            {
                electroSpriteChange = 0;
                //nothing yet, stop the electro sound once it is added
            }

            #endregion

            //if (extraPlat)
            //print(extraPlat.dif);
            Vector2 rigVelTemp = (fakePhysicsVel) + (extraPlat ? (Vector2)extraPlat.transform.TransformDirection(new Vector3(extraPlat.dif.x * 60f, 0f)) : Vector2.zero);
            if (allowRotation)
            {
                rigVelTemp = RotateVector2(rigVelTemp, transform.eulerAngles.z);
            }

            if (!float.IsInfinity(rigVelTemp.sqrMagnitude) && !float.IsNaN(rigVelTemp.sqrMagnitude) && !float.IsNaN(rigVelTemp.x) && !float.IsNaN(rigVelTemp.y))
            {
                Vector2 set = (extraPerFrameVel) + new Vector2(rigVelTemp.x * transform.lossyScale.x, rigVelTemp.y * transform.lossyScale.y);
                if (!float.IsNaN(set.x) && !float.IsNaN(set.y) && !float.IsInfinity(set.x) && !float.IsInfinity(set.y))
                {
                    rig.velocity = set;
                }
            }
            else
            {
                fakePhysicsVel = Vector2.zero;
            }

            kh.overheat = Mathf.Clamp01(kh.overheat-((rig.velocity.magnitude) * 0.000016f)); // can we move this?

            if (iced != 0)
            {
                if (!outsideIceSound.isPlaying)
                {
                    outsideIceSound.Play();
                }
                fakePhysicsVel = new Vector2(iced, fakePhysicsVel.y);
            }
            if (iced != 0 && iced2 == 0)
            {
                outsideIceSound.Stop();
                iced = 0;
            }
            if (storedHMvtByte != tempHMovementByte)
            {
                OnChangeOfHorizMovement(tempHMovementByte);
            }
            storedHMvtByte = tempHMovementByte;
            //GetComponent<ConstantForce2D>().force = Vector2.zero;

            //punching behavior
            bool flippedMan = mySprRend.flipX;

            //ulong[] buttons = encmt.eventsTable.Values.ToArray();
            double buttonTime = encmt.eventsTable[encmt.eventsTable.Count - 1].Item1;
            if ((movementModifier != MovementMod.Stunned) && !disabledAllControlMvt && kh.electrocute == 0f 
                && (encmt.currentState & 64UL) == 64UL && (encmt.flags & 79UL) != 0UL && DoubleTime.ScaledTimeSinceLoad - buttonTime < 0.1f*Time.timeScale
                && !(sgt && sgt.isAiming))
            {
                if ((encmt.currentState & 76UL) == 68UL || (encmt.currentState & 76UL) == 64UL) //(encmt.currentState & 79UL) == (flippedMan ? 69UL : 70UL)) // ^ + > + X : top right punch
                {
                    SetAnim("PunchUpRight");
                }

                if ((encmt.currentState & 76UL) == 72UL)//(encmt.currentState & 79UL) == (flippedMan ? 73UL : 74UL)) // v + X : bottom right kick
                {
                    SetAnim("KickDownRight");
                }

                //be careful of ghosting!!!
            }

            AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
            float meleeOffset = 12f;
            punchRBox.enabled = (asi.IsName("PunchUpRight") && asi.normalizedTime >= 0.33333f);
            punchRBox.offset = new Vector2(meleeOffset * (flippedMan ? -1f : 1f), punchRBox.offset.y);
            kickRBox.enabled = (asi.IsName("KickDownRight") && asi.normalizedTime >= 0.33333f);
            kickRBox.offset = new Vector2(meleeOffset * (flippedMan ? -1f : 1f), kickRBox.offset.y);
            if (asi.IsName("PunchUpRight"))
            {
                if (!punchWhoosh.isPlaying && mySprRend.isVisible)
                {
                    float np = (movementModifier == MovementMod.None) ? 1f : ((movementModifier == MovementMod.Tired) ? 0.5f : 2f);

                    if (punchWhoosh.pitch != np)
                    {
                        punchWhoosh.pitch = np;
                    }
                    punchWhoosh.Play();
                }
                animator.speed = 1f;
            }
            if (asi.IsName("KickDownRight"))
            {
                if (!kickWhoosh.isPlaying && mySprRend.isVisible)
                {
                    float np = (movementModifier == MovementMod.None) ? 1f : ((movementModifier == MovementMod.Tired) ? 0.5f : 2f);

                    if (kickWhoosh.pitch != np)
                    {
                        kickWhoosh.pitch = np;
                    }
                    kickWhoosh.Play();
                }
                animator.speed = 1f;
            }
            if (asi.IsName("KhalHurt"))
            {
                animator.speed = 1f;
            }

            if (idleCheck == 0)
            {
                if (asi.IsName("Idle2") || Fakerand.Single() < 0.1f) //succeed: idle
                {
                    SetAnim("Idle2");
                }
                else
                {
                    idleCheck = idleCheckFrames;
                }
            }

        }


        #region sound filters
        //don't need to check every frame! fix this later
        if (movementModifier == MovementMod.High)
        {
            GetComponent<Animator>().speed *= 2f;
            plrSFX.SetFloat("FlangeWet", 1f);
            plrSFX.SetFloat("FlangeDry", 0f);
        }
        else if (movementModifier == MovementMod.Tired)
        {
            GetComponent<Animator>().speed *= 0.5f;
            plrSFX.SetFloat("FlangeWet", 0f);
            plrSFX.SetFloat("FlangeDry", 1f);
        }
        else if (movementModifier == MovementMod.Stunned)
        {
            GetComponent<Animator>().speed = 0f;
            plrSFX.SetFloat("FlangeWet", 0f);
            plrSFX.SetFloat("FlangeDry", 1f);
        }
        else
        {
            plrSFX.SetFloat("FlangeWet", 0f);
            plrSFX.SetFloat("FlangeDry", 1f);
        }
        #endregion
        /*if (extraPlat)
        {
            print(extraPlat.GetComponent<Rigidbody2D>().velocity);

        }*/

        if (grounded == 0 && audSrc.isPlaying && (audSrc.clip == walkSound || audSrc.clip == sprintWalkSound))
        {
            audSrc.Stop();
        }

        #region momentum mode
        if (momentumMode)
        {
            momentumParticle.gameObject.SetActive(true);
            if (momentum)
            {
                momentumParticle.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                momentumParticle.localScale = new Vector3(-1, 1, 1);
            }

            if (!momentumAmbient.isPlaying)
            {
                momentumAmbient.Play();
            }
        }
        else
        {
            momentumParticle.gameObject.SetActive(false);
            if (momentumAmbient.isPlaying)
            {
                momentumAmbient.Stop();
            }
        }
        #endregion


        if (CCLossDelay > 0 && selfTimerRunning) {

            Vector2 rSize = new Vector2(9f * Mathf.Abs(transform.localScale.x), 25f * Mathf.Abs(transform.localScale.y));
            int hits = Physics2D.BoxCastNonAlloc(transform.position, rSize, transform.eulerAngles.z, transform.up, new RaycastHit2D[1], 0.01f, 256 + 512 + 2048);
            if (hits == 0) { --CCLossDelay; }
        }
        if (CCLossDelay < 0) { CCLossDelay = 0; }
        if (CCLossDelay == 0) { CanCollide = true; }

        if (jumpButtonDidNothingThisFrame && JumpButtonDown(false)) { jumpBuffer = 6; }
        if (jumpBuffer > 0) { --jumpBuffer; }

        --spikeTouchCooldown;
        if (spikeTouchCooldown < 0) { spikeTouchCooldown = 0; }

        --wallDoubleJumpDelay;
        if (wallDoubleJumpDelay < 0) { wallDoubleJumpDelay = 0; }

        prevTimeScale = Time.timeScale;
        extraPerFrameVel = Vector2.zero;
        movingBlocksThisFrame.Clear();
        wallSlideColliders[0] = wallSlideColliders[1] = wallSlideColliders[2] = null;
        lastJumpRecoveredFromThis = null;
        if (!allowRotation) { transform.rotation = Quaternion.identity; }
        //transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        /*if (transform.localScale != Vector3.zero)
        {
            transform.localScale = Vector3.one;
        }*/
        prevMinTransDir = minTransDir;
        minTransDir = Vector2.zero;
        cornerFudgedThisFrame = dontMinTranslateThisFrame = false;
        usedAntiThrashThisFrame = 0;
    }

}