using UnityEngine;
using System.Collections.Generic;

public class BossController : MonoBehaviour {

    public float health;
    [HideInInspector]
    public int currentBar;
    [HideInInspector]
    public float remainingTime;

    [System.Serializable]
    public class BarInformation
    {
        public string name = "Untitled" +
            "";
        public float health = 10f;
        public float defense = 0f;
        public float iframesTime = 0f;
        public float time = 10f;
        public float delay = 2f;
        public GameObject attackObject;
        public string animName;
        public bool goToNextWaypoint = true;
        public bool playHurtSound = true;
    }

    public BarInformation[] barInfos;
    public int barCount;
    [Header("Starting bar should only be nonzero in test situations")]
    public int startingBar = 0;
    public bool useLargeHealthDisplay = true;
    public bool changeLevelName = true;
    //public float[] healthPerBar;
    //public float[] timePerBar;
    //public float[] delaysBeforeTimerStarts;

    //public GameObject[] attackObjects;

    public bool defeated = false;
    public bool timerNotYetStarted = true;
    public bool halted = false;
    public bool beganThisBarsAttack = false;
    public bool awaitMoveBeforeAttack = false; // Waits for the player to move on first attack; waits for a special collider to be hit on all future attacks
    public bool stopMusicOnDefeat = false;
    private bool waitingToStart = false;

    public AudioSource audioSource;
    public AudioClip defeatSoundNonfinal;
    public AudioClip defeatSoundFinal;
    public AudioClip recoverSound;
    public GameObject defeatFinalExplosion;

    public static BossController main;

    public List<GameObject> ambushTriggerOnDefeat = new List<GameObject>();

    private double lastBarStart;

    private BossMover mover;
    public Animator animator;
    public GameObject iframesShield = null;
    // iframesSounds must contain:
    // AudioSource named "On" for when the shield begins
    // AudioSource named "Off" for when the shield ends
    // AudioSource named "Rebuf" for when a hit is attempted with shield.
    public Transform iframesSounds;
    private PrimEnemyHealth healthInterface;

    [HideInInspector]
    public float damageMultiplier = 1f;

    // 0: no damage. 1: all damage (moves onto next bar)
    public float HealthRatio()
    {
        if (currentBar < 0 || currentBar >= barCount) { return 0; }
        return health / (barInfos[currentBar].health);
    }

    public void BeginAttack(int i)
    {
        barInfos[i].attackObject.SetActive(true);
    } 

    public void DeleteAttack(int i)
    {
        if (barInfos[i].attackObject)
        {
            barInfos[i].attackObject.SetActive(false);
        }
        //Destroy(barInfos[i].attackObject);
    }

    public void BeginAttacks()
    {
        barInfos[0].attackObject.SetActive(true);
    }

    private void Start()
    {
        main = this;
        mover = GetComponent<BossMover>();
        if (!mover)
        {
            throw new System.Exception("No BossMover");
        }
        currentBar = startingBar;
        if (startingBar > 0) {
            int startingWaypointIndex = 0;
            for (int i = 1; i <= startingBar; ++i)
            {
                if (barInfos[i].goToNextWaypoint) { ++startingWaypointIndex; }
            }
            mover.ChangeStartIndex(startingWaypointIndex);
        }
        lastBarStart = DoubleTime.ScaledTimeSinceLoad;
        timerNotYetStarted = true;
        defeated = false;
        halted = false;
        healthInterface = GetComponentInChildren<PrimEnemyHealth>();
        if (!healthInterface)
        {
            throw new System.Exception("No PrimEnemyHealth");
        }
        healthInterface.displayUI = true; //!useLargeHealthDisplay;
        healthInterface.isBossHealth = true;
        damageMultiplier = 1f;
        if (iframesShield) { iframesShield.SetActive(false); }
        animator.CrossFade(barInfos[currentBar].animName, 0f);
    }

    private void PlayIFrameSound(string x)
    {
        if (!iframesSounds) { return; }
        AudioSource a = iframesSounds.Find(x).GetComponent<AudioSource>();
        a.Stop(); a.Play();
    }

    public void OnHurt(PrimEnemyHealth.OnHurtInfo ohi)
    {
        if ((iframesShield && iframesShield.activeSelf) || (timerNotYetStarted && barInfos[currentBar].iframesTime > 0) || !beganThisBarsAttack) {
            PlayIFrameSound("Rebuf");
            return;
        }
        // keep the same amount of time to timeout an attack
        BarInformation barInfo = barInfos[currentBar];
        float left = barInfo.health - health;
        float proportion = Mathf.Clamp01(ohi.amt / left);
        damageMultiplier *= (1f - proportion);
        health += ohi.amt;
    }

    public void JustStoppedTravelling()
    {
        if (currentBar >= barInfos.Length) { return; }
        if (animator && barInfos[currentBar].animName != "")
        {
            animator.CrossFade(barInfos[currentBar].animName, 0f);
        }
    }

    private void OnDefeat()
    {
        foreach (GameObject g in ambushTriggerOnDefeat)
        {
            foreach (IAmbushController ia in g.GetComponents<IAmbushController>())
            {
                ia.OnAmbushComplete();
            }
        }
        Destroy(healthInterface);
        if (stopMusicOnDefeat && BGMController.main)
        {
            BGMController.main.InstantMusicChange(null, false);
        }
        if (defeatFinalExplosion)
        {
            GameObject expl = Instantiate(defeatFinalExplosion, animator.transform.position, Quaternion.identity, null);
            UltimateExplosion uexpl = expl.GetComponent<UltimateExplosion>();
            uexpl.whatToDestroy = animator.gameObject;
        }
    }

    private void EndThisBarrage(bool noDelay = false)
    {
        if (currentBar >= barInfos.Length) { return; }
        health = 0;
        damageMultiplier = 1f;
        DeleteAttack(currentBar);
        if (iframesShield) { iframesShield.SetActive(false); }
        if (awaitMoveBeforeAttack) { waitingToStart = true; }
        ++currentBar;
        //print("now starting boss attack " + currentBar.ToString());
        lastBarStart = DoubleTime.ScaledTimeSinceLoad;
        timerNotYetStarted = true;
        beganThisBarsAttack = false;
        BulletRegister.Clear(new BulletRegister.ClearFromBossEndingTheBarrage());
        audioSource.Stop();
        audioSource.clip = (currentBar == barCount) ? defeatSoundFinal : defeatSoundNonfinal;
        if (audioSource.clip != null && (barInfos.Length <= currentBar || barInfos[currentBar].playHurtSound)) { audioSource.Play(); }
        if (animator)
        {
            animator.CrossFade((currentBar == barCount) ? "HurtFinal" : "HurtNonfinal", 0f); // Special anim. name
        }
        if (currentBar == barCount)
        {
            //print("Boss is defeated");
            defeated = true;
            OnDefeat();
            return;
        }
        if (barInfos[currentBar].goToNextWaypoint)
        {
            mover.MoveToNext(noDelay ? 0f : barInfos[currentBar].delay);
        }
    }

    public void SkipToNextBarrage()
    {
        EndThisBarrage(true);
    }

    public static void StopAwaitingNextAttack()
    {
        if (!main) { return; }
        main.waitingToStart = false;
        main.lastBarStart = DoubleTime.ScaledTimeSinceLoad;
    }

    private void Update()
    {
        BossHealthBarChange.TryUpdateBar(this);

        if (Time.timeScale == 0 || defeated || halted || KHealth.someoneDied) { return; }
        if (awaitMoveBeforeAttack && 
            (currentBar == 0 && (!LevelInfoContainer.GetActiveControl()?.HasEverMoved() ?? true) || (currentBar > 0 && waitingToStart))
            ) { return; }

        BarInformation barInfo = barInfos[currentBar];

        double elapse = DoubleTime.ScaledTimeSinceLoad - lastBarStart;
        if (barInfo.time > 0 && elapse >= barInfo.delay)
        {
            if (timerNotYetStarted && currentBar > 0)
            {
                if (barInfo.goToNextWaypoint)
                {
                    audioSource.Stop();
                    audioSource.clip = recoverSound;
                    audioSource.Play();
                }
                else
                {
                    JustStoppedTravelling();
                }
            }
            timerNotYetStarted = false;
            health += damageMultiplier * barInfo.health * Time.timeScale * 0.016666666666f / barInfo.time;
            remainingTime = Mathf.Clamp((barInfo.health - health)*(barInfo.time / barInfo.health) / damageMultiplier,0,99);
            if (!beganThisBarsAttack && barInfos[currentBar].attackObject != null && !mover.IsTravelling())
            {
                beganThisBarsAttack = true;
                BeginAttack(currentBar);
            }

            if (iframesShield && barInfos[currentBar].iframesTime > 0)
            {
                double ift = barInfos[currentBar].iframesTime - elapse;
                if (ift <= 0) // iframes over
                {
                    if (iframesShield.activeSelf)
                    {
                        PlayIFrameSound("Off");
                        iframesShield.SetActive(false);
                    }
                    healthInterface.IFramesDisplayUpdate(0);
                }
                else
                {
                    if (!iframesShield.activeSelf)
                    {
                        PlayIFrameSound("On");
                        iframesShield.SetActive(true);
                    }
                    healthInterface.IFramesDisplayUpdate((float)ift);
                }
            }
        }

        healthInterface.damage = health;
        healthInterface.defense = timerNotYetStarted ? Mathf.Infinity : barInfo.defense;
        healthInterface.damageToDestroy = barInfo.health;
        healthInterface.HealthUIUpdate();


        if (health >= barInfo.health)
        {
            EndThisBarrage();
        }
        
    }
}
