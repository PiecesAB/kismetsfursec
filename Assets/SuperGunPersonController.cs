using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperGunPersonController : MonoBehaviour, ITextBoxDeactivate
{
    public enum Phase
    {
        NotStarted, SGPFalling, SGPChasing, SGPVictory
    }

    public AudioClip musicChange;
    public GameObject deleteWhenSequenceStarts;
    public GameObject enableWhenSequenceStarts;
    public GameObject deleteWhenCrush;
    public GameObject enableWhenCrush;
    public GameObject enableWhenCrushImmediate;
    public GameObject superGunPerson;
    public GameObject effectOnTouch;
    public Prim3DRotate machineGunRotation;
    public GameObject machineGunFire;
    public GameObject machineGunFire2;
    public GameObject armFireL;
    public GameObject armFireR;
    public GameObject armFireL2;
    public GameObject armFireR2;
    public GameObject chestFire;
    public GameObject chestFire2A;
    public GameObject chestFire2B;
    public Transform glassToBreak;
    public Transform propsToObliterate;
    public GameObject victoryLastFire;
    public Animator victoryDoorClose;
    public GameObject victoryDialog;
    public SpriteRenderer fadeToBlack;
    public AudioClip nothingMusic;
    public Transform uArmL;
    public Transform lArmL;
    public Transform uArmR;
    public Transform lArmR;
    public SkinnedMeshRenderer chestCannon;
    public AudioSource chestWhirr;
    public Phase currPhase = Phase.NotStarted;
    public int testAttack = -1;

    private Transform sgpTransform;
    private Animator sgpAnimator;

    private int timerToRubble = 12;
    private int timerToStartMoving = 120;
    private int timerToNextAttack = 120;
    private int timerToEndAttack = 300;
    private GameObject smallExplo;

    // This is specific to the Travail - Animal Kingdom level!
    public void OnTextBoxDeactivate()
    {
        smallExplo = Resources.Load<GameObject>("SmallExplo");
        sgpTransform = superGunPerson.transform;
        sgpAnimator = superGunPerson.GetComponent<Animator>();
        BGMController.main.InstantMusicChange(musicChange, true, BGMController.main.aso.time);
        Destroy(deleteWhenSequenceStarts);
        enableWhenSequenceStarts.SetActive(true);
        sgpTransform.position = new Vector3(-3070, -274, 0);
        sgpTransform.rotation = Quaternion.identity;
        sgpAnimator.enabled = false;
        Utilities.SetLevelInInfo("podium");
        Utilities.SoftSave();
        currPhase = Phase.SGPFalling;
        FollowThePlayer.main.originalScrolling = FollowThePlayer.main.perScreenScrolling = false;
        // queue all attacks
        int[] a = { 4, 2, 0, 1, 5, 3 };
        //for (int i = 0; i < attackCount; ++i) { a[i] = i; }
        /*for (int i = 0; i < attackCount; ++i)
        {
            int j = Fakerand.Int(i, attackCount);
            int temp = a[i];
            a[i] = a[j];
            a[j] = temp;
        }*/
        for (int i = 0; i < attackCount; ++i) { attackQueue.Enqueue(a[i]); }
    }

    private int currAttack = -1;
    private Queue<int> attackQueue = new Queue<int>();
    private int attackCount = 6;
    private float attackTime = 0f;
    private GameObject attackObj1;
    private GameObject attackObj2;

    private float UArmRotationToAimAtPlayer()
    {
        Transform plrt = LevelInfoContainer.GetActiveControl()?.transform;
        if (!plrt) { return -90f; } // LOLHOW?
        Vector2 d = plrt.position - uArmL.position;
        if (d.x >= 0) { return 0f; }
        float a = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        if (a < 0) { a += 360; } // angle is now in (90, 270), 90 --> -180, 270 --> 0
        return a - 270;
    }

    public void SetAttack()
    {
        timerToEndAttack = 300;
        // decide currAttack by random order
        if (testAttack > -1) { currAttack = testAttack; }
        else if (attackQueue.Count != 0) { currAttack = attackQueue.Dequeue(); }
        else
        {
            sgpAnimator.CrossFade("StandThere", 0f);
            Instantiate(victoryLastFire, victoryLastFire.transform.parent).SetActive(true);
            timerToEndAttack = 333;
            currAttack = -1;
            currPhase = Phase.SGPVictory;
        }
        
        attackTime = 0f;
        switch (currAttack)
        {
            case 0:
                attackObj1 = Instantiate(machineGunFire, machineGunFire.transform.parent);
                attackObj1.SetActive(true);
                machineGunRotation.speed = 12;
                // walking animation remains
                break;
            case 3:
                attackObj1 = Instantiate(machineGunFire2, machineGunFire2.transform.parent);
                attackObj1.SetActive(true);
                machineGunRotation.speed = 12;
                // walking animation remains
                break;
            case 1:
                sgpAnimator.CrossFade("WalkLegs", 0f);
                lArmL.localEulerAngles = lArmR.localEulerAngles = new Vector3(90, 0, 0);
                uArmL.localEulerAngles = new Vector3(90, 0, 0);
                uArmR.localEulerAngles = new Vector3(-90, 0, 0);
                attackObj1 = Instantiate(armFireL, armFireL.transform.parent);
                attackObj1.SetActive(true);
                attackObj2 = Instantiate(armFireR, armFireR.transform.parent);
                attackObj2.SetActive(true);
                break;
            case 4:
                sgpAnimator.CrossFade("WalkLegs", 0f);
                lArmL.localEulerAngles = lArmR.localEulerAngles = new Vector3(90, 0, 0);
                uArmL.localEulerAngles = uArmR.localEulerAngles = Vector3.right * UArmRotationToAimAtPlayer();
                attackObj1 = Instantiate(armFireL2, armFireL2.transform.parent);
                attackObj1.SetActive(true);
                attackObj2 = Instantiate(armFireR2, armFireR2.transform.parent);
                attackObj2.SetActive(true);
                break;
            case 2:
            case 5:
                chestWhirr.Play();
                break;
            default:
                break;
        }
    }

    public void EndAttack()
    {
        timerToNextAttack = 120;
        switch (currAttack)
        {
            case 0:
            case 3:
                Destroy(attackObj1);
                machineGunRotation.speed = 0;
                break;
            case 1:
            case 4:
                Destroy(attackObj1);
                Destroy(attackObj2);
                break;
            case 2:
            case 5:
                chestCannon.SetBlendShapeWeight(0, 100);
                Destroy(attackObj1);
                if (currAttack == 5) { Destroy(attackObj2); }
                break;
            default:
                break;
        }
        sgpAnimator.CrossFade("Walk", 0f);
        currAttack = -1;
    }

    // Note: moving items in LateUpdate works because it's after the animator and before the render
    public void AttackUpdate()
    {
        attackTime += Time.timeScale / 60f;
        switch (currAttack)
        {
            case 1:
                float r = Mathf.PingPong(75f * Mathf.Pow(attackTime, 1.3f), 180f);
                uArmL.localEulerAngles = new Vector3(- r, 0, 0);
                uArmR.localEulerAngles = new Vector3(-180 + r, 0, 0);
                lArmL.localEulerAngles = lArmR.localEulerAngles = new Vector3(90, 0, 0);
                break;
            case 4:
                lArmL.localEulerAngles = lArmR.localEulerAngles = new Vector3(90, 0, 0);
                uArmL.localEulerAngles = uArmR.localEulerAngles = Vector3.right * UArmRotationToAimAtPlayer();
                break;
            case 2:
            case 5:
                float cannonHide = chestCannon.GetBlendShapeWeight(0);
                if (timerToEndAttack > 60)
                {
                    chestCannon.SetBlendShapeWeight(0, Mathf.MoveTowards(cannonHide, 0f, 5f));
                    if (chestCannon.GetBlendShapeWeight(0) == 0)
                    {
                        chestWhirr.Stop();
                    }
                }
                else
                {
                    chestCannon.SetBlendShapeWeight(0, Mathf.MoveTowards(cannonHide, 100f, 5f));
                }
                if (timerToEndAttack == 255)
                {
                    if (currAttack == 5)
                    {
                        attackObj1 = Instantiate(chestFire2A, chestFire2A.transform.parent);
                        attackObj1.SetActive(true);
                        attackObj2 = Instantiate(chestFire2B, chestFire2B.transform.parent);
                        attackObj2.SetActive(true);
                    }
                    else
                    {
                        attackObj1 = Instantiate(chestFire, chestFire.transform.parent);
                        attackObj1.SetActive(true);
                    }
                    chestWhirr.Stop();
                }
                break;
            default:
                break;
        }
    }

    private HashSet<Transform> broken = new HashSet<Transform>();

    void Update()
    {
        GameObject plr = LevelInfoContainer.GetActiveControl()?.gameObject;
        if (!plr) { return; }
        if (Time.timeScale == 0) { return; }

        if (KHealth.someoneDied && currPhase != Phase.NotStarted)
        {
            Utilities.SetActionData("DiedAfterAKPodium");
        }

        foreach (Transform t in glassToBreak)
        {
            PrimBreakable pb = t.GetComponent<PrimBreakable>();
            if (!pb) { continue; }
            if (!sgpTransform) { sgpTransform = superGunPerson.transform; }
            if (t.position.x >= sgpTransform.position.x - 40 && !broken.Contains(t))
            {
                pb.BreakIt(999, -90);
                broken.Add(t);
            }
        }

        if (smallExplo && sgpTransform)
        {
            foreach (Transform t in propsToObliterate)
            {
                if (t.position.x >= sgpTransform.position.x - 40 && !broken.Contains(t))
                {
                    Instantiate(smallExplo, t.position + Vector3.back * 100, Quaternion.identity, null);
                    Destroy(t.gameObject, 0.01f);
                    broken.Add(t);
                }
            }
        }

        switch (currPhase)
        {
            case Phase.SGPFalling:
                sgpTransform.position -= Vector3.up * 16f;
                Vector3 oldCamPos = FollowThePlayer.main.transform.position;
                Vector3 newCamPos = new Vector3(oldCamPos.x, -732, oldCamPos.z);
                FollowThePlayer.main.SetTransformPosition(Vector3.MoveTowards(oldCamPos, newCamPos, 4f));
                if (sgpTransform.position.y <= -706)
                {
                    sgpTransform.position = new Vector3(sgpTransform.position.x, -706, sgpTransform.position.z);
                    FollowThePlayer.main.vibSpeed += 6f;
                    FollowThePlayer.main.SetTransformPosition(newCamPos);
                    FollowThePlayer.main.ritualScrollingUnlocked = true;
                    FollowThePlayer.main.originalScrolling = FollowThePlayer.main.perScreenScrolling = false;
                    enableWhenCrushImmediate.SetActive(true);
                    currPhase = Phase.SGPChasing;
                }
                break;
            case Phase.SGPChasing:
                if (timerToRubble > 0)
                {
                    --timerToRubble;
                    if (timerToRubble == 0)
                    {
                        Destroy(deleteWhenCrush);
                        enableWhenCrush.SetActive(true);
                    }
                }
                if (plr.transform.position.x >= sgpTransform.position.x)
                {
                    // player should not touch it: crumb them instantly
                    plr.GetComponent<KHealth>().ChangeHealth(float.NegativeInfinity, "sgplaser");
                    Instantiate(effectOnTouch, plr.transform.position, Quaternion.identity);
                    // spawn laser so it actually makes sense
                    Destroy(gameObject);
                }
                if (timerToStartMoving > 0)
                {
                    --timerToStartMoving;
                    if (timerToStartMoving == 0)
                    {
                        sgpAnimator.enabled = true;
                        sgpAnimator.CrossFade("Walk", 0f);
                        sgpTransform.position = new Vector3(-3070, -706, 0);
                        sgpTransform.rotation = Quaternion.identity;
                    }
                }
                else
                {
                    sgpTransform.position -= Vector3.right * 0.5f; // scrolling speed
                    if (timerToNextAttack > 0)
                    {
                        --timerToNextAttack;
                        if (timerToNextAttack == 0)
                        {
                            SetAttack();
                        }
                    }
                    if (timerToEndAttack > 0)
                    {
                        --timerToEndAttack;
                        if (timerToEndAttack == 0)
                        {
                            EndAttack();
                        }
                    }
                }
                break;
            case Phase.SGPVictory:
                if (timerToEndAttack > 0)
                {
                    --timerToEndAttack;
                    if (timerToEndAttack == 120)
                    {
                        victoryDoorClose.CrossFade("Close", 0f);
                    }
                    if (timerToEndAttack == 0)
                    {
                        TextBoxGiverHandler.SpawnNewBox(ref victoryDialog, null);
                    }
                }
                else
                {
                    --timerToEndAttack;
                    plr.GetComponent<KHealth>().tiredOrHigh = -Mathf.InverseLerp(-40, -540, timerToEndAttack) * 0.9f;
                    BGMController.main.fadeOutSpeed = 0.15f;
                    BGMController.main.nextMusic = nothingMusic;
                    if (timerToEndAttack < -240)
                    {
                        fadeToBlack.color = new Color(0, 0, 0, Mathf.InverseLerp(-240, -540, timerToEndAttack));
                    }
                    if (timerToEndAttack == -540 && !KHealth.someoneDied)
                    {
                        GetComponent<primAddScene>().activate = true;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (currAttack != -1)
        {
            AttackUpdate();
        }
    }
}
