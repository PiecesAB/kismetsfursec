using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class KHealth : MonoBehaviour
{

    public static float health;
    public static float maxHealth;
    public GameObject thePlayerReference;
    public bool dyingLava = false;
    public int toxicity;
    private bool db = true;
    public AudioClip[] deathSounds;
    public GameObject explodeParticle;
    public GameObject bloodParticle;
    public GameObject bloodSplatter;
    public Sprite[] bloodsplatterPics;
    public Texture2D blownApartPic;
    public AudioClip explodeSound;
    public AudioClip fallDeathSound;
    public AudioSource hurtSound;
    public int addToxicity;
    public float nontoxic;
    public float previousnontoxmax;
    public float overheat;
    public GameObject overheatBoxPrefab;
    public GameObject overheatBoxInstance;
    public GameObject fireEffect;
    public GameObject smokeEffect;
    public float electrocute;
    private float electroVel;
    public GameObject electroBoxPrefab;
    public GameObject electroBoxInstance;
    public AudioSource toxicShockSound;
    public AudioSource electrocutedSound;
    public AudioSource minorVictorySound;
    public AudioSource fastButtonPress;
    public AudioSource fireIgniteSound;
    public AudioSource fireOngoingSound;
    public int stunnedCantMove;
    public float limitedMovement = 0f;
    public GameObject limitedMovementBoxPrefab;
    private GameObject limitedMovementBoxInstance;
    public GameObject reverseBoxPrefab;
    private GameObject reverseBoxInstance;
    public GameObject limitedMovementEndDialog;
    public GameObject limitedMovementEndDialogPoor;
    [HideInInspector]
    public int limitedMovementCost;
    [HideInInspector]
    public SawbladeShave lastSpikeTouched = null; // only used to find out the last spike that the player touched when they're getting damaged.

    public float tiredOrHigh;
    public GameObject TOHBoxPrefab;
    public GameObject TOHBoxInstance;
    [Header("Only starts if you die after winning the level")]
    public AudioClip spuriousDeathSound;
    public AudioSource doorEnterRef;
    public static int hitsThisLevel;

    public static bool hitpointVisible = false;
    private GameObject hitpoint = null;

    private Rigidbody2D rg2;
    private SpecialGunTemplate sgt;

    public static bool someoneDied = false;

    public static List<KHealth> all = new List<KHealth>();

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

    private Rigidbody2D[] deathShards = new Rigidbody2D[24 * 8];
    private int deathShardsI = 0;
    const float blowUpTime = 0.2f;
    const int nullifyFrames = 7;

    [HideInInspector]
    public int justFiredBulletInvincibility = 0;

    public IEnumerator DeathShardsMove()
    {
        yield return WaitForRealSeconds(blowUpTime);
        for (int i = 0; i < deathShards.Length; i++)
        {
            if (deathShards[i] == null) { continue; }
            deathShards[i].gravityScale = 50;
            deathShards[i].velocity = 300 * Fakerand.UnitCircle();
        }
    }

    public IEnumerator Chunkify2(Texture2D crumbPic, Transform originObj)
    {
        // in case it gets deleted
        Vector3 origin = originObj.position;
        Vector3 forward = originObj.forward;

        for (int i = 0; i < 32; i += 8)
        {
            for (int j = 0; j < 42; j += 7)
            {
                GameObject newPiece = new GameObject();
                SceneManager.MoveGameObjectToScene(newPiece, SceneManager.GetActiveScene());
                SpriteRenderer newSpr = newPiece.AddComponent<SpriteRenderer>();
                newSpr.sprite = Sprite.Create(crumbPic, new Rect(i, j, 8, 6), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect); ;
                newSpr.sortingLayerName = "UI";
                Rigidbody2D newRig = newPiece.AddComponent<Rigidbody2D>();
                newRig.gravityScale = 0;
                newRig.velocity = 6 * Fakerand.UnitCircle();
                newPiece.transform.position = origin + new Vector3(i - 16, j - 24, 0);
                Destroy(deathShards[deathShardsI]?.gameObject);
                deathShards[deathShardsI] = newRig;
                deathShardsI = (deathShardsI + 1) % deathShards.Length;
            }
        }
        StartCoroutine(DeathShardsMove());
        yield return WaitForRealSeconds(0.1f);
        for (int i = 0; i < 9; i++)
        {
            float d = Mathf.Deg2Rad;

            RaycastHit2D ray = Physics2D.Raycast(origin + forward * 5, new Vector2(Mathf.Cos((360 * d * i) / 9), Mathf.Sin((360 * d * i) / 9)).normalized, 85, ~(1 << 20));
            Vector2 hit = ray.point;
            if (hit.magnitude > 0)
            {
                GameObject splat = Instantiate(bloodSplatter, new Vector3(hit.x, hit.y, 0), Quaternion.AngleAxis(0, Vector3.forward)) as GameObject;
                SpriteRenderer splatSR = splat.GetComponent<SpriteRenderer>();
                splatSR.sprite = bloodsplatterPics[Fakerand.Int(0, bloodsplatterPics.Length)];
                //float whatIs = Fakerand.Int(8, 26);
                splat.transform.localScale = Vector3.one;
                DontDestroyOnLoad(splat);
                Destroy(splat, 999);
            }
        }
    }

    public static void Chunkify(Texture2D crumbPic, Transform originObj)
    {
        if (all.Count == 0) { return; }
        all[0].StartCoroutine(all[0].Chunkify2(crumbPic, originObj));
    }

    public IEnumerator Respawn(string deadReason)
    {
        if (health < 1e8)
        {
            for (int i = 0; i < nullifyFrames + 1; ++i)
            {
                yield return new WaitForEndOfFrame();
                if (health < maxHealth)
                {
                    yield break;
                }
            }
        }

        if (db && enabled)
        {
            db = false;
            bool chunk = false;
            float extraWait = 0f;
            //camera junk
            if (deadReason != "spring ko")
            {
                Camera.main.GetComponent<FollowThePlayer>().target = transform;
            }
            someoneDied = true;
            AudioSource asc = GetComponent<AudioSource>();
            asc.Stop();
            electrocutedSound.Stop(); //just in case
            AudioSource aa = gameObject.AddComponent<AudioSource>();
            bool invisibleManLeft = true;


            // this needs to go before the death reason is changed.
            string origDeadReason = deadReason;
            if (!Utilities.toSaveData.allDeathReasons.ContainsKey(deadReason)) { Utilities.toSaveData.allDeathReasons.Add(deadReason, 1); }
            else { ++Utilities.toSaveData.allDeathReasons[deadReason]; }

            //special reasons

            if (deadReason == "below boundary" || deadReason == "out of picture")
            {
                aa.clip = fallDeathSound;
                extraWait = 1f;
            }
            else if (deadReason == "no more movement")
            {
                GetComponent<SpriteRenderer>().material.SetFloat("_BW", 1f);
                invisibleManLeft = false;
            }
            else
            {
                if (DoubleTime.UnscaledTimeSinceLoad < 3f && Fakerand.Int(0, 2) == 0)
                {
                    deadReason = "quick death";
                }

                GetComponent<AudioSource>().PlayOneShot(explodeSound, 1);
                GameObject boom = Instantiate(explodeParticle, transform.position, Quaternion.identity) as GameObject;
                GameObject blood = Instantiate(bloodParticle, transform.position, Quaternion.identity) as GameObject;
                DontDestroyOnLoad(blood);
                Destroy(blood, 18);
                if (Door1.levelComplete)
                {
                    doorEnterRef.Stop();
                    aa.clip = spuriousDeathSound;
                    deadReason = "after win";
                }
                else
                {
                    aa.clip = deathSounds[Fakerand.Int(0, deathSounds.Length)];
                }
                chunk = true;
            }
            aa.volume = 1;
            aa.spatialBlend = 0;
            aa.priority = 16;
            aa.Play();
            
            foreach (BoxCollider2D c in GetComponents<BoxCollider2D>())
            {
                c.enabled = false;
            }
            foreach (CircleCollider2D c in GetComponents<CircleCollider2D>())
            {
                c.enabled = false;
            }
            foreach (EdgeCollider2D c in GetComponents<EdgeCollider2D>())
            {
                c.enabled = false;
            }

            gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            //Destroy(gameObject.GetComponent<Rigidbody2D>());
            Destroy(gameObject.GetComponent<BasicMove>());
            Destroy(gameObject.GetComponent<Animator>());
            Destroy(gameObject.GetComponent<ClickToChangeTime>());

            if (invisibleManLeft)
            {
                transform.localScale = new Vector3(0, 0, 0);
            }

            //if (!Door1.levelComplete)
            Utilities.StopGameTimerHere();
            Door1.levelComplete = false;

            {
                //Utilities.loadedSaveData.deathsTotal++;
                if (!Utilities.replayLevel)
                {
                    ++Utilities.toSaveData.deathsTotalStory;
                }

                //if (!Utilities.replayLevel)
                {
                    //Utilities.loadedSaveData.deathsThisLevel++;
                    ++Utilities.toSaveData.deathsTotal;
                    ++Utilities.toSaveData.deathsThisLevel;

                    if ((Utilities.toSaveData.deathsThisLevel < 100 && Utilities.toSaveData.deathsThisLevel % 10 == 0)
                        || Utilities.toSaveData.deathsThisLevel % 25 == 0)
                    {
                        deadReason = "deaths this level note";
                    }
                    else if (Utilities.toSaveData.deathsThisLevel == 100 || Utilities.toSaveData.deathsThisLevel % 200 == 0)
                    {
                        deadReason = "deaths this level 100 note";
                    }
                }
                Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.DeathsTotal, origDeadReason);
                /*Utilities.loadedSaveData.previousDeathReason =*/
                Utilities.toSaveData.previousDeathReason = origDeadReason;
                Utilities.toSaveData.SharedPlayersHP = 0f;
                Utilities.loadedSaveData = new Utilities.SaveData(Utilities.toSaveData);
                Utilities.SaveGame(Utilities.activeGameNumber); // new! autosave
                Utilities.tetrahedronTime = -5f;
            }
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.01666666f;

            if (chunk)
            {
                Chunkify(blownApartPic, transform);
            }

            yield return WaitForRealSeconds(1.5f + extraWait);
            HudDieScreen hds = FindObjectOfType<HudDieScreen>();
            if (hds != null)
            {
                StartCoroutine(hds.TileScreenDeath(deadReason));
            }
            Key1Script.keyCount = 0;
        }
        yield return 1;
    }

    private void Start()
    {
        all.Add(this);
        maxHealth = WaveFunctions.HealthFromScore(Utilities.loadedSaveData.score);
        health = Utilities.replayLevel ? 0 : Utilities.loadedSaveData.SharedPlayersHP;
        if (health < 0)
        {
            health = 0;
        }
        toxicity = 0;
        addToxicity = 0;
        overheat = electrocute = tiredOrHigh = 0f;
        someoneDied = false;
        rg2 = GetComponent<Rigidbody2D>();
        limitedMovementCost = 20000;
        hitsThisLevel = 0;

    }

    private void OnDestroy()
    {
        all.Remove(this);
        for (int i = 0; i < deathShards.Length; ++i)
        {
            Destroy(deathShards[i]);
        }
    }

    public void HurtAnimStart()
    {
        BasicMove bm = GetComponent<BasicMove>();
        if (bm)
        {
            bm.ResetIdleCounter();
            bm.SetAnim("KhalHurt");
        }
    }

    // About to be crumbed?
    // Maybe not...
    // finalBlowChange is negative when damage is done.
    public bool NullifyRespawn(float finalBlowChange)
    {
        // Qi squared
        int i = -1;
        if (maxHealth - health >= -30 && InventoryItemsNew.InventoryHasItem(105, out i))
        {
            InventoryItemsNew.UseItem(i, gameObject, false);
            health = 0;
            return true;
        }

        return false;
    }

    private struct NullifyWindow
    {
        public int framesLeft;
        public float damageAmount;
        public NullifyWindow(int _framesLeft, float _damageAmount)
        {
            framesLeft = _framesLeft;
            damageAmount = _damageAmount;
        }
    }

    private List<NullifyWindow> nullifyWindows = new List<NullifyWindow>();

    private bool ActivateNullifyGun()
    {
        if (!sgt) { sgt = GetComponent<SpecialGunTemplate>(); }
        return sgt && sgt.FireIfPossible();
    }

    public void NullifyDamage()
    {
        if (nullifyWindows.Count > 0)
        {
            for (int i = 0; i < nullifyWindows.Count; ++i)
            {
                if (nullifyWindows[i].damageAmount < -1e8) { continue; }
                health += nullifyWindows[i].damageAmount;
            }
            nullifyWindows.Clear();
        }
    }

    private bool RavelShield(float dmgAmount)
    {
        if (dmgAmount > 0f) { return false; }
        if (!sgt) { sgt = GetComponent<SpecialGunTemplate>(); }
        if ((sgt is ClickToShieldAndInfJump) && (sgt as ClickToShieldAndInfJump).ShieldCircleActive() && lastSpikeTouched)
        {
            lastSpikeTouched.BlowMeUp();
            return true;
        }
        else if ((sgt is ClickToShieldAndInfJump) && (sgt as ClickToShieldAndInfJump).layers > 0)
        {
            --(sgt as ClickToShieldAndInfJump).layers;
            (sgt as ClickToShieldAndInfJump).UpdateLayerGraphic();
            if (lastSpikeTouched) { lastSpikeTouched.BlowMeUp(); }
            return true;
        }
        return false;
    }

    public void ChangeHealth(float amount, string damageReason, bool dontCareAboutTime = false)
    {
        Encontrolmentation e = GetComponent<Encontrolmentation>();
        if ((Time.timeScale != 0f || dontCareAboutTime) && e && e.allowUserInput && gameObject.activeInHierarchy) //ignore nonactive players
        {
            bool ravelShield = RavelShield(amount);
            if (amount < 0f && (!ravelShield || amount < -1e9f))
            {
                ++hitsThisLevel;
                Utilities.tetrahedronTime = -1000f;
                nullifyWindows.Add(new NullifyWindow(nullifyFrames, amount));
            }

            if (health < maxHealth && (!ravelShield || amount < -1e9f))
            {
                health -= amount;
            }

            if (amount > 0f)
            {
                health = Mathf.Max(0f, health);
            }

            if (health >= maxHealth && amount < 0f)
            {
                if (!NullifyRespawn(amount))
                {
                    //Destroy(thePlayerReference);
                    StartCoroutine(Respawn(damageReason));
                }
            }
        }

        if (amount <= -1f && !(damageReason == "below boundary" || damageReason == "out of picture"))
        {
            if (hurtSound.isPlaying)
            {
                if (hurtSound.time >= 0.25f)
                {
                    hurtSound.Stop();
                    hurtSound.pitch = 0.95f + (0.5f * health / maxHealth);
                    hurtSound.pitch = Mathf.Clamp(hurtSound.pitch, 1f, 2f);
                    hurtSound.Play();
                    HurtAnimStart();
                }
            }
            else
            {
                hurtSound.pitch = 0.95f + (0.5f * health / maxHealth);
                hurtSound.pitch = Mathf.Clamp(hurtSound.pitch, 1f, 2f);
                hurtSound.Play();
                HurtAnimStart();
            }
        }

        lastSpikeTouched = null;
    }

    public void SetHealth(float amount, string damageReason)
    {
        float prevHealth = health;
        /*if (amount < health)
        {
            Utilities.tetrahedronTime = -1000f;
        }*/
        if (Time.timeScale != 0f) //just making sure
        {
            health = amount;
            if (health >= maxHealth)
            {
                if (!NullifyRespawn(prevHealth - amount))
                {
                    //Destroy(thePlayerReference);
                    StartCoroutine(Respawn(damageReason));
                }
            }
        }
    }

    private Sprite hitboxOrigSprite = null;
    private SpriteRenderer hitboxSprend = null;

    private void UpdateHitbox()
    {
        if (hitpointVisible && hitpoint == null)
        {
            hitpoint = Instantiate(Resources.Load<GameObject>("hitpoint"));
            hitpoint.transform.SetParent(transform, false);
            hitboxSprend = hitpoint.GetComponent<SpriteRenderer>();
            hitboxOrigSprite = hitboxSprend.sprite;
            if (!sgt) { sgt = GetComponent<SpecialGunTemplate>(); }
        }
        if (!hitpointVisible && hitpoint != null)
        {
            Destroy(hitpoint);
            hitpoint = null;
        }
        if (hitpoint)
        {
            if (sgt is ClickToShieldAndInfJump && (sgt as ClickToShieldAndInfJump).layers > 0)
            {
                int l = (sgt as ClickToShieldAndInfJump).layers - 1;
                hitboxSprend.sprite = hitpoint.GetComponent<HitpointSpecialHolder>().large[l];
            }
            else
            {
                hitboxSprend.sprite = hitboxOrigSprite;
            }
        }
    }

    private BasicMove bm = null;

    public void Update()
    {
        if (bm == null) { bm = GetComponent<BasicMove>(); }

        UpdateHitbox();

        if (justFiredBulletInvincibility > 0) { --justFiredBulletInvincibility; }

        if (health < 0)
        {
            health = 0;
        }

        if (limitedMovement > 0f)
        {
            if (limitedMovementBoxInstance == null)
            {
                limitedMovementBoxInstance = PlrUI.MakeStatusBox(limitedMovementBoxPrefab, transform);
            }

            limitedMovementBoxInstance.transform.GetChild(0).GetComponent<TextMesh>().text = ((limitedMovement * 0.0508f).ToString("F1", CultureInfo.InvariantCulture)) + "m";
        }

        if (nullifyWindows.Count > 0)
        {
            if (!ActivateNullifyGun())
            {
                for (int i = 0; i < nullifyWindows.Count; ++i)
                {
                    if (nullifyWindows[i].framesLeft == 1)
                    {
                        nullifyWindows.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        nullifyWindows[i] = new NullifyWindow(nullifyWindows[i].framesLeft - 1, nullifyWindows[i].damageAmount);
                    }
                }
            }
        }

        if (bm)
        {
            if (rg2 && limitedMovement > 0f && Time.timeScale > 0 && !Door1.levelComplete)
            {
                limitedMovement -= (rg2.velocity.magnitude * 0.01666666f) + 0.05f;
                if (limitedMovement < 0f)
                {
                    limitedMovement = 0f;
                    limitedMovementBoxInstance.transform.GetChild(0).GetComponent<TextMesh>().text = "0.0m";
                    if (Utilities.loadedSaveData.score >= limitedMovementCost)
                    {
                        MainTextsStuff.insertableIntValue1 = limitedMovementCost;
                        Instantiate(limitedMovementEndDialog, GameObject.FindGameObjectWithTag("DialogueArea").transform).SetActive(true);
                    }
                    else
                    {
                        Instantiate(limitedMovementEndDialogPoor, GameObject.FindGameObjectWithTag("DialogueArea").transform).SetActive(true);
                    }
                }
            }


            if (stunnedCantMove == 0 && bm.movementModifier == BasicMove.MovementMod.Stunned)
            {
                bm.movementModifier = BasicMove.MovementMod.None;
            }

            if (Time.timeScale > 0)
            {
                maxHealth = WaveFunctions.HealthFromScore(Utilities.loadedSaveData.score);
                if (addToxicity >= 1)
                {
                    toxicity++;
                }
                else
                {
                    toxicity = 0;
                }
                nontoxic -= Time.timeScale;
                if (nontoxic > 0)
                {
                    toxicity = 0;
                }
                else
                {
                    nontoxic = 0;
                }
                if (toxicity >= 1)
                {
                    if (!toxicShockSound.isPlaying)
                    {
                        toxicShockSound.Play();
                    }
                    ChangeHealth(Mathf.Floor(-toxicity * 0.08f) * 0.4f, "toxic");
                }
                else
                {
                    toxicShockSound.Stop();
                }
                if (nontoxic > 0)
                {
                    toxicShockSound.Stop();
                }

                if (bm != null && (bm.swimming && !bm.lavaCheck))
                {
                    overheat = 0f;
                }

                if (Time.timeScale > 0)
                {
                    #region overheat1

                    if (overheat == 0f && overheatBoxInstance != null)
                    {
                        fireEffect.SetActive(false);
                        smokeEffect.SetActive(false);
                        fireOngoingSound.Stop();
                        minorVictorySound.Play();
                        PlrUI.DestroyStatusBox(overheatBoxInstance, transform);
                        overheatBoxInstance = null;
                    }
                    else if (overheat > 0.5f)
                    {
                        smokeEffect.SetActive(true);
                        fireEffect.SetActive(true);

                        if (overheat > 1f)
                        {
                            overheat = 1f;
                        }
                    }
                    else if (overheat > 0f)
                    {
                        smokeEffect.SetActive(true);
                        fireEffect.SetActive(false);
                    }

                    #endregion

                    #region electro1
                    if (electrocute == 0f && electroBoxInstance != null)
                    {
                        electrocutedSound.Stop();
                        minorVictorySound.Play();
                        PlrUI.DestroyStatusBox(electroBoxInstance, transform);
                        electroBoxInstance = null;
                    }
                    else if (electrocute > 0f)
                    {
                        //mash a
                        electrocute += electroVel;
                        electroVel += 0.000016666666f;
                        Encontrolmentation e = GetComponent<Encontrolmentation>();
                        if (e != null && health < maxHealth && (e.flags & 16UL) == 16UL && (e.currentState & 16UL) == 16UL)
                        {
                            fastButtonPress.Play();
                            electrocute -= 0.06f;
                        }

                        if (electrocute <= 0f)
                        {
                            electrocute = 0f;
                        }

                        if (electrocute >= 1f)
                        {
                            ChangeHealth(-5f*bm.Damage, "shock");
                            electrocute = Fakerand.Single(0.03f,0.5f);
                            electroVel = Mathf.Max(electroVel-0.005f, 0.001f);
                        }
                    }
                    #endregion

                    #region tiredOrHigh
                    if (tiredOrHigh == 0f && TOHBoxInstance != null)
                    {
                        PlrUI.DestroyStatusBox(TOHBoxInstance, transform);
                        TOHBoxInstance = null;
                    }

                    tiredOrHigh = Mathf.Clamp(tiredOrHigh, -1f, 1f);
                    if (System.Math.Abs(tiredOrHigh) < 0.005f)
                    {
                        tiredOrHigh = 0f;
                    }

                    if (bm != null)
                    {
                        if (tiredOrHigh == 0f)
                        {
                            if (bm.movementModifier == BasicMove.MovementMod.Tired ||
                            bm.movementModifier == BasicMove.MovementMod.High)
                            {
                                bm.movementModifier = BasicMove.MovementMod.None;
                            }
                        }
                        else if (tiredOrHigh < 0f)
                        {
                            bm.movementModifier = BasicMove.MovementMod.Tired;
                            if (tiredOrHigh == -1f)
                            {
                                ChangeHealth(-1f, "tired");
                            }
                        }
                        else // > 0f
                        {
                            bm.movementModifier = BasicMove.MovementMod.High;
                            if (tiredOrHigh == 1f)
                            {
                                ChangeHealth(-1f, "high");
                            }
                        }
                    }
                    tiredOrHigh = Mathf.MoveTowards(tiredOrHigh, 0f, 0.001111111f);

                    #endregion

                    #region reverse

                    if ((transform.localScale.x < 0) != (transform.localScale.y < 0) 
                        && reverseBoxInstance == null)
                    {
                        reverseBoxInstance = PlrUI.MakeStatusBox(reverseBoxPrefab, transform);
                    }
                    if ((transform.localScale.x < 0) == (transform.localScale.y < 0)
                        && reverseBoxInstance != null)
                    {
                        PlrUI.DestroyStatusBox(reverseBoxInstance, transform); reverseBoxInstance = null;
                    }

                    #endregion

                    //creates UI and handles damage for status effects

                    if (overheat > 0f)
                    {
                        if (overheat >= 0.5f)
                        {
                            ChangeHealth(-(0.4f * overheat * overheat), "fire");
                        }
                        if (overheatBoxInstance == null)
                        {
                            overheatBoxInstance = PlrUI.MakeStatusBox(overheatBoxPrefab, transform);
                            fireOngoingSound.Play();
                            fireIgniteSound.Play();
                        }
                        else
                        {
                            foreach (Transform ii in overheatBoxInstance.transform)
                            {
                                if (ii.gameObject.name == "OverheatBar")
                                {
                                    Renderer rs = ii.GetComponent<Renderer>();
                                    float ov1 = Mathf.Clamp01(overheat);
                                    rs.material.SetFloat("_Val", ov1);
                                    rs.material.SetFloat("_LerpVal", ov1);
                                }
                            }
                        }
                    }

                    if (electrocute > 0f)
                    {

                        if (electroBoxInstance == null)
                        {
                            electroBoxInstance = PlrUI.MakeStatusBox(electroBoxPrefab, transform);
                            electroVel = 0.002f;
                            electrocutedSound.Play();
                        }
                        else
                        {
                            foreach (Transform ii in electroBoxInstance.transform)
                            {
                                if (ii.gameObject.name == "ElectroBar")
                                {
                                    Renderer rs = ii.GetComponent<Renderer>();
                                    float ov1 = Mathf.Clamp01(electrocute);
                                    rs.material.SetFloat("_Val", ov1);
                                    rs.material.SetFloat("_LerpVal", ov1);
                                }
                            }
                        }
                    }
                }

                if (tiredOrHigh != 0f)
                {

                    if (TOHBoxInstance == null)
                    {
                        TOHBoxInstance = PlrUI.MakeStatusBox(TOHBoxPrefab, transform);
                        //sound
                    }
                    else
                    {
                        foreach (Transform ii in TOHBoxInstance.transform)
                        {
                            if (ii.gameObject.name == "Bar")
                            {
                                Renderer rs = ii.GetComponent<Renderer>();
                                float ov1 = Mathf.Clamp01(System.Math.Abs(tiredOrHigh));
                                rs.material.SetColor("_BarColor", (tiredOrHigh > 0f) ? (new Color(0.9f, 0.9f, 0.9f)) : (Color.magenta));
                                rs.material.SetFloat("_Val", ov1);
                                rs.material.SetFloat("_LerpVal", ov1);
                            }

                            if (ii.gameObject.name == "Display")
                            {
                                ii.GetComponent<TextMesh>().text = (tiredOrHigh > 0f) ? "HIGH" : "TIRED";
                            }
                        }
                    }
                }

                float t1 = Mathf.Sign(tiredOrHigh);
                GetComponent<SpriteRenderer>().material.SetFloat("_TOH", t1 * Fastmath.FastSqrt(System.Math.Abs(tiredOrHigh)));
                GetComponent<SpriteRenderer>().material.SetFloat("_Stun", (stunnedCantMove > 0) ? 0.15f : 0f);

                addToxicity = Mathf.Max(addToxicity - 1, 0);

                if (stunnedCantMove > 0 && bm.movementModifier != BasicMove.MovementMod.Stunned)
                {
                    bm.movementModifier = BasicMove.MovementMod.Stunned;
                }
                stunnedCantMove = Mathf.Max(stunnedCantMove - 1, 0);
            }
        }
    }
}
