using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialGunTemplate : MonoBehaviour
{
    public float gunHealth;
    public float gunHealthDecreaseAmount;

    public bool isAiming;
    public bool disabled;
    public float angle;
    [HideInInspector]
    public bool mvtLocked = false; // used in grid guns: player must hold L/R or gun key to aim, and mvt is disabled.
    protected float nextangle;

    public AudioSource gunaudio;
    public AudioClip fail1;
    public AudioClip gunLoad;
    public AudioClip gunUnload;
    public AudioClip gunShoot;

    public float gunStrength = 96f;
    public SpriteRenderer gunTorsoSR;
    public SpriteRenderer gunHeadSR;
    public Transform gunFrontArmTF;
    public Transform gunBackArmTF;
    public SpriteRenderer gunFrontArmSR;
    public SpriteRenderer gunBackArmSR;
    public Sprite[] gunHeadAngles = new Sprite[3];

    public bool swapCooldownDisabled = false;

    protected static string[][] mainAnimToGunBody = new string[5][]
    {
        new string[1]{"Running1"},
        new string[1]{"MovingUp"},
        new string[1]{"Floating"},
        new string[1]{"MovingDown"},
        new string[1]{"Sprint1"},
    };

    protected static string[] mainAnimToGunBody2 = new string[5]
    {
        "gunBodyWalk",
        "gunBodyMovingUp",
        "gunBodyFloating",
        "gunBodyMovingDown",
        "gunBodySprint",
    };

    protected static string defaultAnim = "gunBodyIdle";

    protected SpriteRenderer mainSR;
    protected Encontrolmentation e;
    protected BasicMove bm;
    protected AnimatorStateInfo an;

    protected abstract void ChildStart();

    void Start()
    {
        angle = 0f;
        gunHealth = LevelInfoContainer.main.kStartEnergy;
        bm = GetComponent<BasicMove>();

        e = GetComponent<Encontrolmentation>();
        mainSR = GetComponent<SpriteRenderer>();
        ChildStart();
    }

    protected abstract void AimingBegin();
    protected abstract void AimingUpdate();

    protected abstract void GraphicsUpdateWhenAiming();
    protected abstract void GraphicsUpdateWhenNotAiming();

    /**
     * Return the multiplier by which to decrease gun energy
     */
    protected abstract float Fire();

    protected abstract void ChildUpdate();

    private bool fakeFireInput = false;

    private KHealth kh;

    private bool forceCancel = false;

    public void ForceCancel()
    {
        if (isAiming)
        {
            forceCancel = true;
        }
    }

    public bool FireIfPossible()
    {
        if (isAiming && !bm.disabledAllControlMvt && gunHealth >= gunHealthDecreaseAmount && !fakeFireInput)
        {
            fakeFireInput = true;
            return true;
        }
        return false;
    }

    private int fireBuffer = 0;
    private bool fireButtonDidNothingThisFrame = false;
    private bool FireButtonDown(bool useBuffer = true)
    {
        if (useBuffer && fireBuffer > 0) { return true; }
        return e.ButtonDown(32UL, 32UL);
    }

    private void OnDisable()
    {
        ForceCancel();
        InternalUpdate();
    }

    private void InternalUpdate()
    {
        gunHealth = Mathf.Clamp(gunHealth, 0, 100);
        if (!kh) { kh = GetComponent<KHealth>(); }

        if (GetComponent<Animator>())
        {
            an = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);

            gunTorsoSR.transform.localScale = gunFrontArmSR.transform.localScale = gunBackArmSR.transform.localScale = new Vector3(mainSR.flipX ? -1 : 1, 1, 1);

            if (Time.timeScale > 0 && e && mainSR)
            {
                if (!disabled && !swapCooldownDisabled)
                {
                    fireButtonDidNothingThisFrame = true;
                    if (forceCancel) { goto fc; }

                    if (isAiming)
                    {
                        e.eventAName = "Fire";
                        e.eventAbutton = Encontrolmentation.ActionButton.BButton;
                        e.eventBName = "Cancel";
                        e.eventBbutton = Encontrolmentation.ActionButton.XButton;
                    }

                    if ((!bm.disabledAllControlMvt && FireButtonDown()) || fakeFireInput)
                    {
                        if (gunHealth >= gunHealthDecreaseAmount)
                        {
                            fireButtonDidNothingThisFrame = false;
                            fireBuffer = 0;
                            if (!isAiming)
                            {
                                gunaudio.Stop();
                                gunaudio.clip = gunLoad;
                                gunaudio.Play();

                                isAiming = true;

                                AimingBegin();

                                angle = nextangle;

                                mainSR.color = Color.clear;
                                gunTorsoSR.gameObject.SetActive(true);

                                gunTorsoSR.color = gunHeadSR.color = gunFrontArmSR.color = gunBackArmSR.color = Color.white;
                                gunTorsoSR.material = gunHeadSR.material = gunFrontArmSR.material = gunBackArmSR.material = mainSR.material;
                            }
                            else
                            {
                                if (this is ClickToTeleportBomb)
                                {
                                    (this as ClickToTeleportBomb).isFakeFire = fakeFireInput;
                                }
                                fakeFireInput = false;
                                float decreaseMultiplier = Fire();
                                if (decreaseMultiplier > 0)
                                {
                                    gunaudio.Stop();
                                    gunaudio.clip = gunShoot;
                                    gunaudio.Play();

                                    gunHealth -= decreaseMultiplier * gunHealthDecreaseAmount;
                                    isAiming = false;

                                    mainSR.color = Color.white;
                                    gunTorsoSR.gameObject.SetActive(false);
                                    gunTorsoSR.color = gunHeadSR.color = gunFrontArmSR.color = gunBackArmSR.color = Color.clear;

                                    NoWeaponBorder.WeaponWasFired(decreaseMultiplier);
                                    kh.justFiredBulletInvincibility = 7;
                                }

                                if (kh) { kh.NullifyDamage(); }
                            }
                        }
                        else if (FireButtonDown(false))
                        {
                            gunaudio.Stop();
                            gunaudio.clip = fail1;
                            gunaudio.Play();
                        }
                    }

                fc:

                    if ((forceCancel || (((e.currentState & 64UL) == 64UL && (e.flags & 64UL) == 64UL) || gunHealth < gunHealthDecreaseAmount)) && isAiming)
                    {

                        gunaudio.Stop();
                        gunaudio.clip = gunUnload;
                        gunaudio.Play();

                        isAiming = false;
                        forceCancel = false;
                        mvtLocked = false;

                        mainSR.color = Color.white;
                        gunTorsoSR.gameObject.SetActive(false);
                        gunTorsoSR.color = gunHeadSR.color = gunFrontArmSR.color = gunBackArmSR.color = Color.clear;
                    }

                    if (fireBuffer > 0) { --fireBuffer; }
                    if (fireButtonDidNothingThisFrame && FireButtonDown(false)) { fireBuffer = 6; }

                }
                if (disabled || swapCooldownDisabled)
                {
                    isAiming = false;
                }


                if (isAiming)
                {
                    GraphicsUpdateWhenAiming();

                    nextangle = angle;

                    AimingUpdate();

                    gunFrontArmSR.transform.rotation = Quaternion.AngleAxis(nextangle, Vector3.forward);
                    if (bm && (bm.wallSliding || bm.udm > 0))
                    {
                        gunBackArmSR.transform.rotation = mainSR.flipX ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
                    }
                    else if (bm)
                    {
                        gunBackArmSR.transform.rotation = Quaternion.AngleAxis(Mathf.Floor(nextangle / 30f) * 30f, Vector3.forward);
                    }
                    bool flipside = (nextangle >= 90f && nextangle < 270f);
                    gunFrontArmSR.flipY = flipside;
                    float headAngle = flipside ? Mathf.Repeat(180f - nextangle, 360f) : Mathf.Repeat(nextangle, 360f);
                    if (headAngle > 25f && headAngle < 100f)
                    {
                        gunHeadSR.sprite = gunHeadAngles[0]; //up
                    }
                    else if (headAngle > 100f && headAngle < 335f)
                    {
                        gunHeadSR.sprite = gunHeadAngles[2]; //down
                    }
                    else
                    {
                        gunHeadSR.sprite = gunHeadAngles[1]; //mid
                    }
                    angle = nextangle;

                    string newAnimName = "gunBodyIdle";
                    int newAnimID = -1;

                    for (int i = 0; i < mainAnimToGunBody.Length; i++)
                    {
                        for (int j = 0; j < mainAnimToGunBody[i].Length; j++)
                        {
                            if (an.IsName(mainAnimToGunBody[i][j]))
                            {
                                newAnimName = mainAnimToGunBody2[i];
                                newAnimID = i;
                                goto endAnimLoop;
                            }
                        }
                    }
                endAnimLoop:
                    gunTorsoSR.GetComponent<Animator>().CrossFade(newAnimName, 0f);
                    if (newAnimID >= 1 && newAnimID <= 3) //falling anims
                    {
                        gunHeadSR.color = Color.clear;
                    }
                    else
                    {
                        gunHeadSR.color = Color.white;
                    }
                }
                else
                {
                    GraphicsUpdateWhenNotAiming();
                }
            }

            ChildUpdate();
        }
    }

    private void Update()
    {
        InternalUpdate();
    }
}
