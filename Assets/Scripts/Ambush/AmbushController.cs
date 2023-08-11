using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushController : MonoBehaviour
{
    public enum AmbushType
    {
        NormalEnemies, Bonus
    }

    public enum SpawnMode
    {
        Normal, InCamera
    }

    [Flags]
    public enum WinCondition
    {
        NoChildrenRemain = 1, ExternallyTriggered = 2
    }

    public float startDelay = 0f;
    public SpawnMode spawnMode;
    public WinCondition myWinCondition = WinCondition.NoChildrenRemain;
    public GameObject[] triggersOnActivate;
    public GameObject[] deleteOnActivate;
    public float deleteOnActivateDelay = 0f;
    public GameObject deleteEffectPrefab;
    public GameObject[] triggersOnWin;
    public GameObject[] deleteOnWin;
    public string statusMessage;
    public GameObject enemySpawnEffect;
    public AudioClip activateSound;
    public AudioClip winSound;
    public AudioClip ambushMusic;
    public double timeOut = double.PositiveInfinity;
    public bool clearBulletsOnWin = true;
    public bool startAutomatically = false;
    public bool trivialWinOnCrumb = true;

    public static int activeAmbushesCount;

    private Vector3[] positions;

    private BGMController bgm;
    private bool externallyTriggered = false;

    public AudioClip oldBgmClip;
    private bool oldBgmLooped;
    private bool thereIsNextStage;
    [HideInInspector]
    private bool induceWin = false;

    public float spawnEffectTime = 1f;

    public static string WIN_AMBUSH_MESSAGE = "win";

    public void Awake()
    {
        statusMessage = "inactive";
        activeAmbushesCount = 0;
        thereIsNextStage = false;
        induceWin = false;
        foreach (Transform c in transform)
        {
            c.gameObject.SetActive(false);
        }
    }

    public void Start()
    {
        if (startAutomatically)
        {
            Activate();
        }
    }

    public void PlaySound(AudioClip ac)
    {
        AudioSource aso = GetComponent<AudioSource>();
        if (!aso) { return; }
        aso.Stop();
        aso.clip = ac;
        aso.Play();
    }

    public void DoTriggerToObjA(GameObject g)
    {
        if (!g) { return; }

        Component[] comps = g.GetComponents(typeof(IAmbushController));
        for (int c = 0; c < comps.Length; ++c)
        {
            ((IAmbushController)comps[c]).OnAmbushBegin();
        } 

        if (comps.Length == 0 && g.GetComponent<AmbushController>() == null)
        {
            g.SetActive(!g.activeSelf);
        }
    }

    public void InduceWin()
    {
        induceWin = true;
    }

    public void DoTriggerToObjW(GameObject g)
    {
        if (!g) { return; }

        Component[] comps = g.GetComponents(typeof(IAmbushController));
        for (int c = 0; c < comps.Length; ++c)
        {
            ((IAmbushController)comps[c]).OnAmbushComplete();
        }

        if (comps.Length == 0 && g.GetComponent<AmbushController>() == null)
        {
            g.SetActive(!g.activeSelf);
        }

        if (g.GetComponent<AmbushController>() && !KHealth.someoneDied && !Door1.levelComplete) // possible next stage of ambush
        {
            thereIsNextStage = true;
            g.GetComponent<AmbushController>().Activate(oldBgmClip, oldBgmLooped);
        }
    }

    public static IEnumerator Spawn(GameObject objToClone, SpawnMode spawnMode, GameObject spawnEffect = null)
    {
        Transform newParent = null;
        if (spawnMode == SpawnMode.InCamera) { newParent = Camera.main.transform; }

        GameObject newObj = Instantiate(objToClone, objToClone.transform.position, Quaternion.identity, newParent);
        newObj.SetActive(false);
        double effectTime = 0.0;
        if (spawnEffect)
        {
            Instantiate(spawnEffect, objToClone.transform.position, Quaternion.identity, newParent);
            effectTime = spawnEffect.GetComponent<primDeleteInTime>().t;
        }
        if (effectTime > 0.0)
        {
            double t1 = DoubleTime.ScaledTimeSinceLoad + effectTime;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }
        newObj.SetActive(true);
        yield return null;
    }

    public static IEnumerator Spawn(GameObject objToClone, Vector3 pos, GameObject spawnEffect = null)
    {
        GameObject newObj = Instantiate(objToClone, pos, Quaternion.identity);
        newObj.SetActive(false);
        double effectTime = 0.0;
        if (spawnEffect)
        {
            Instantiate(spawnEffect, pos, Quaternion.identity);
            effectTime = spawnEffect.GetComponent<primDeleteInTime>().t;
        }
        if (effectTime > 0.0)
        {
            double t1 = DoubleTime.ScaledTimeSinceLoad + effectTime;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }
        newObj.SetActive(true);
        yield return null;
    }

    // place a new enemy into a collider trigger region of this object.
    public IEnumerator SpawnIntoAmbush(GameObject objToClone)
    {
        if (objToClone == null) { yield break; }
        if (activeAmbushesCount == 0) { yield break; } //... this will pass if ANY ambush is ongoing. watch out
        if (GetComponent<Collider2D>() == null) { yield break; }

        Collider2D[] temp = GetComponents<Collider2D>();
        Collider2D colPlace = temp[Fakerand.Int(0, temp.Length)];
        Vector2 randomPoint = Vector2.negativeInfinity;
        if (colPlace is PolygonCollider2D) { yield break; } // :(
        if (colPlace is CircleCollider2D)
        {
            randomPoint = colPlace.offset + ((CircleCollider2D)colPlace).radius * Fakerand.UnitCircle();
        }
        if (colPlace is BoxCollider2D)
        {
            Vector2 size = ((BoxCollider2D)colPlace).size;
            randomPoint = colPlace.offset + new Vector2(Mathf.Lerp(-size.x/2f,size.x/2f,Fakerand.Single()), Mathf.Lerp(-size.y / 2f, size.y / 2f, Fakerand.Single()));
        }

        GameObject newObj = Instantiate(objToClone, randomPoint, Quaternion.identity, transform);
        newObj.SetActive(false);

        if (enemySpawnEffect)
        {
            Instantiate(enemySpawnEffect, randomPoint, Quaternion.identity);
        }

        double t1 = DoubleTime.ScaledTimeSinceLoad + (double)spawnEffectTime;
        yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));

        newObj.SetActive(true);

    }

    private int delayedCount = 0;
    private double startTime;

    public IEnumerator DelayedAppearance(float time, Transform trs, Vector3 pos)
    {
        ++delayedCount;
        yield return new WaitForSeconds(time);
        if (enemySpawnEffect)
        {
            Instantiate(enemySpawnEffect, pos, Quaternion.identity);
        }
        double t1 = DoubleTime.ScaledTimeSinceLoad + spawnEffectTime;
        yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        trs.position = pos;
        trs.gameObject.SetActive(true);
        foreach (IAmbushChildController cc in trs.GetComponentsInChildren<IAmbushChildController>())
        {
            cc.OnAmbushBegin();
        }
        yield return new WaitForEndOfFrame();
        --delayedCount;
    }

    private bool TimeOut()
    {
        return DoubleTime.ScaledTimeSinceLoad - startTime >= timeOut;
    }

    private bool WinConditionMet()
    {
        bool win = false;
        if (myWinCondition == 0)
        {
            myWinCondition = WinCondition.NoChildrenRemain;
        }
        if ((myWinCondition & WinCondition.NoChildrenRemain) != 0)
        {
            win |= transform.childCount == 0 && delayedCount == 0;
        }
        if ((myWinCondition & WinCondition.ExternallyTriggered) != 0)
        {
            win |= externallyTriggered;
        }
        return win;
    }

    public void ExternalTrigger()
    {
        if (statusMessage != "active") { return; }
        externallyTriggered = true;
    }

    private static GameObject ambushCompletePlrUI = null;

    public IEnumerator Main1()
    {
        print("ambush started!");
        if (spawnMode == SpawnMode.InCamera)
        {
            transform.SetParent(Camera.main.transform, true);
        }
        if (startDelay > 0f) { yield return new WaitForSeconds(startDelay); }
        positions = new Vector3[transform.childCount];
        int k = 0;
        foreach (Transform c in transform)
        {
            if (c == transform) { continue; }

            //print(k);
            positions[k++] = c.position - transform.position;
            if (c.GetComponent<AmbushDelay>())
            {
                AmbushDelay ad = c.GetComponent<AmbushDelay>();
                float rand = (ad.randomVariation == 0) ? 0 : Fakerand.Single(-ad.randomVariation, ad.randomVariation);
                StartCoroutine(DelayedAppearance(ad.delay + rand, c, c.position));
                continue;
            }
            else if (c.GetComponent<AmbushBezierSpawn>())
            {
                c.gameObject.SetActive(true);
                StartCoroutine(c.GetComponent<AmbushBezierSpawn>().Activate(enemySpawnEffect));
                continue;
            }
            if (enemySpawnEffect)
            {
                Transform newParent = null;
                if (spawnMode == SpawnMode.InCamera) { newParent = Camera.main.transform; }
                Instantiate(enemySpawnEffect, c.position, Quaternion.identity, newParent);
            }
        }
        double t1 = DoubleTime.ScaledTimeSinceLoad + spawnEffectTime;
        yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        k = 0;
        foreach (Transform c in transform)
        {
            if (c == transform) { continue; }
            if (c.GetComponent<AmbushDelay>()) { k++; continue; }
            c.position = positions[k++] + transform.position;
            c.gameObject.SetActive(true);
            foreach (IAmbushChildController cc in c.GetComponentsInChildren<IAmbushChildController>())
            {
                cc.OnAmbushBegin();
            }
        }
        print("enemies spawned; waiting for elimination");
        startTime = DoubleTime.ScaledTimeSinceLoad;
        yield return new WaitUntil(() => WinConditionMet() 
                                      || (KHealth.someoneDied && trivialWinOnCrumb)
                                      || Door1.levelComplete 
                                      || induceWin
                                      || TimeOut());
        for (int i = 0; i < transform.childCount; ++i)
        {
            GameObject bye = transform.GetChild(i).gameObject;
            if (transform.gameObject.activeSelf && bye.GetComponent<GenericBlowMeUp>())
            {
                bye.GetComponent<GenericBlowMeUp>().BlowMeUp(Fakerand.Single(), true);
            }
            else
            {
                Destroy(bye);
            }
        }
        for (int i = 0; i < triggersOnWin.Length; ++i)
        {
            DoTriggerToObjW(triggersOnWin[i]);
        }
        if (clearBulletsOnWin)
        {
            BulletRegister.Clear(new BulletRegister.ClearFromAmbush());
        }
        print("ambush win!");
        statusMessage = WIN_AMBUSH_MESSAGE;
        activeAmbushesCount--;
        if (!thereIsNextStage)
        {
            if (!Door1.levelComplete && !KHealth.someoneDied)
            {
                PlaySound(winSound);
            }

            if (!bgm) { bgm = BGMController.main; }
            if (bgm)
            {
                bgm.InstantMusicChange(oldBgmClip, oldBgmLooped);
            }
        }
        GameObject plr = LevelInfoContainer.GetActiveControl()?.gameObject;
        if (plr && !KHealth.someoneDied && !Door1.levelComplete)
        {
            if (!ambushCompletePlrUI) { ambushCompletePlrUI = Resources.Load<GameObject>("AmbushComplete"); }
            PlrUI.MakeStatusBox(ambushCompletePlrUI, plr.transform);
        }
        StartCoroutine(DeleteOnWin());
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator DeleteOnActivate()
    {
        yield return new WaitForSeconds(deleteOnActivateDelay);
        for (int i = 0; i < deleteOnActivate.Length; i++)
        {
            if (deleteOnActivate[i] == null) { continue; }
            Instantiate(deleteEffectPrefab, deleteOnActivate[i].transform.position, Quaternion.identity, deleteOnActivate[i].transform.parent);
            Destroy(deleteOnActivate[i]);
        }
    }

    public IEnumerator DeleteOnWin()
    {
        yield return null;
        for (int i = 0; i < deleteOnWin.Length; i++)
        {
            Instantiate(deleteEffectPrefab, deleteOnWin[i].transform.position, Quaternion.identity, deleteOnWin[i].transform.parent);
            Destroy(deleteOnWin[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 20 && statusMessage == "inactive")
        {
            Activate();
        }
    }

    // to allow time for the BGMController to initialize
    private IEnumerator ChangeBgmAfterOneFrame()
    {
        yield return new WaitForEndOfFrame();
        if (!bgm) { bgm = BGMController.main; }
        if (bgm)
        {
            AudioSource aus = bgm.GetComponent<AudioSource>();
            if (!oldBgmClip)
            {
                oldBgmClip = bgm.nextMusic ? bgm.nextMusic : aus.clip;
            }
            oldBgmLooped = bgm.nextMusic ? bgm.nextMusicLoops : aus.loop;
            if (ambushMusic && aus.clip != ambushMusic)
            {
                bgm.InstantMusicChange(ambushMusic, true);
            }
        }
    }

    public void Activate()
    {
        activeAmbushesCount++;
        statusMessage = "active";
        for (int i = 0; i < triggersOnActivate.Length; i++)
        {
            DoTriggerToObjA(triggersOnActivate[i]);
        }
        StartCoroutine(DeleteOnActivate());
        if (activateSound)
        {
            PlaySound(activateSound);
        }
        StartCoroutine(ChangeBgmAfterOneFrame());
        StartCoroutine(Main1());
    }

    public void Activate(AudioClip originalOldBGM, bool looped)
    {
        activeAmbushesCount++;
        statusMessage = "active";
        for (int i = 0; i < triggersOnActivate.Length; i++)
        {
            DoTriggerToObjA(triggersOnActivate[i]);
        }
        if (activateSound)
        {
            PlaySound(activateSound);
        }
        //AudioSource aus = bgm.GetComponent<AudioSource>();
        oldBgmClip = originalOldBGM;
        oldBgmLooped = looped;
        StartCoroutine(Main1());
    }
    
}
