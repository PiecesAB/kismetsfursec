using UnityEngine;
using System.Collections;

public class PrimCollectibleAux : MonoBehaviour {

    public enum Type
    {
        PlrEnergy, ExtraTime, YBFlag, Custom, DoubleJump, Electron, TimeScaleModify
    }

    public Type type;
    public float increment;
    public AudioClip collectSound;
    public AudioClip overflowSound;
    private bool collected = false;
    public float regen = 0f;
    private GameObject clone = null;
    public GameObject getBox;
    public GameObject getTooEasyBox;
    public GameObject customCollectRecipient;
    public GameObject collectEffect;
    public bool mustBeActivePlayerToCollect = false;

	private void Start () {
        collected = false;
        if (type == Type.YBFlag)
        {
            int dat = -1;
            if (Utilities.GetPersistentData(gameObject, -1, out dat) && dat == 1)
            {
                collected = true;
                Destroy(GetComponent<Collider2D>());
                GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.75f);
            }
            else
            {
                Utilities.ChangePersistentData(gameObject, -1);
            }
        }
	}

    private IEnumerator DeathRites()
    {
        AudioSource aso = GetComponent<AudioSource>();
        if (regen == 0f)
        {
            if (aso) { Destroy(gameObject, aso.clip.length); }
            else { Destroy(gameObject); }
        }
        else if (aso && aso.clip.length > regen)
        {
            yield return new WaitForSeconds(regen);
            clone.SetActive(true);
            clone.name = gameObject.name;
            yield return new WaitForSeconds(aso.clip.length - regen);
            Destroy(gameObject);
        }
        else // regen > aso.clip.length
        {
            yield return new WaitForSeconds(regen);
            clone.SetActive(true);
            Destroy(gameObject);
        }
        yield return null;
    }

    private void DoubleJumpPlatform(Collider2D c, BasicMove bm)
    {
        if (type == Type.DoubleJump && bm != null && !bm.doubleJump)
        {
            Destroy(GetComponent<Collider2D>());
            AudioSource aso = GetComponent<AudioSource>();
            if (aso != null)
            {
                aso.pitch = 1f;
                //aso.clip = bm.doubleJump ? overflowSound : collectSound;
                aso.clip = collectSound;
                aso.Play();
            }
            bm.doubleJump = true;
            // handle sparkles
            Transform spark = transform.GetChild(1);
            spark.SetParent(null, true);
            Destroy(spark.gameObject, regen);

            transform.localScale = Vector3.zero;
            collected = true;
            StartCoroutine(DeathRites());
        }
    }

    private void OnTriggerStay2D(Collider2D c)
    {
        if (type == Type.DoubleJump)
        {
            BasicMove bm = c.GetComponent<BasicMove>();
            DoubleJumpPlatform(c, bm);
            return;
        }
    }

    private bool IsValidColliderComponent(Collider2D c)
    {
        return c.GetComponent<BasicMove>() 
            || (CGICycleMover.AtLeastOneExists() && c.GetComponent<CGICycleMover>());
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (IsValidColliderComponent(c)
            && (!mustBeActivePlayerToCollect || c.GetComponent<Encontrolmentation>().allowUserInput) 
            && !collected)
        {
            if (regen > 0f)
            {
                clone = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
                clone.SetActive(false);
            }

            SpecialGunTemplate ct = c.GetComponent<SpecialGunTemplate>();
            if (type == Type.PlrEnergy)
            {
                Destroy(GetComponent<Collider2D>());
                AudioSource aso = GetComponent<AudioSource>();
                if (aso != null)
                {
                    if (((ct != null) ? (ct.gunHealth + increment) : 0) > 100 || (ct != null && !ct.enabled))
                    {
                        aso.clip = overflowSound;
                        aso.pitch = 1f;
                    }
                    else
                    {
                        aso.clip = collectSound;
                        aso.pitch = Mathf.Pow(2f, ((ct != null) ? ct.gunHealth : 50f) / 240f); //equal temperament ???
                    }
                    aso.Play();
                }

                if (ct != null) {
                    if (ct.enabled)
                    {
                        ct.gunHealth = Mathf.Clamp(ct.gunHealth + increment, 0, 100);
                    }
                }
                else { GunBarChange.changeGunHealthEmergency += increment; }
            }

            BasicMove bm = c.GetComponent<BasicMove>();
            if (type == Type.DoubleJump)
            {
                DoubleJumpPlatform(c, bm);
                return;
            }

            // likely will be unused; stopwatches already change the speed of time
            if (type == Type.ExtraTime)
            {
                LevelInfoContainer.timer += increment;

                //transform.localScale = Vector3.zero;
                //collected = true;
            }

            if (type == Type.TimeScaleModify)
            {
                TimeCalc.ChangeNormalTime(increment);
                AudioSource aso = GetComponent<AudioSource>();
                aso.clip = collectSound;
                aso.Play();
                // make FASTER! or SLOWER! sign exist
                Transform sign = transform.GetChild(1);
                sign.SetParent(null);
                sign.gameObject.SetActive(true);
            }

            if (type == Type.YBFlag)
            {
                Utilities.ChangePersistentDataSpecial(Utilities.PersistentValueSpecialMode.FlagsTotal, 1);
                Utilities.ChangePersistentData(gameObject, 1, true);
                if (Utilities.replayLevel && Utilities.replayMode == 0)
                {
                    GameObject go = GameObject.FindGameObjectWithTag("DialogueArea");
                    if (go.transform.childCount == 0 && Time.timeScale > 0)
                    {
                        GameObject made = Instantiate(getTooEasyBox);
                        made.SetActive(true);
                        made.transform.SetParent(go.transform, false);
                    }
                }
                else
                {
                    GameObject go = GameObject.FindGameObjectWithTag("DialogueArea");
                    if (go.transform.childCount == 0 && Time.timeScale > 0)
                    {
                        GameObject made = Instantiate(getBox);
                        made.SetActive(true);
                        made.transform.SetParent(go.transform, false);
                    }
                }

                AudioSource aso = GetComponent<AudioSource>();
                if (aso != null)
                {
                    aso.clip = collectSound;
                    aso.Play();
                }

                BGMController.main?.DuckOutAtSpeedForFrames(20f, 120);
                if (collectEffect) { Instantiate(collectEffect, transform.position, transform.rotation, null); }
            }

            if (type == Type.Electron)
            {
                ElectronTracker et = c.GetComponent<ElectronTracker>();
                if (!et) {
                    et = c.gameObject.AddComponent<ElectronTracker>();
                    et.uiPrefab = GetComponent<ElectronTracker>().uiPrefab;
                    et.lightningPrefab = GetComponent<ElectronTracker>().lightningPrefab;
                }
                ++et.amount;
                et.respawns.Enqueue(clone);
                AudioSource aso = GetComponent<AudioSource>();
                aso.clip = collectSound;
                aso.Play();
            }

            if (type == Type.Custom && customCollectRecipient != null)
            {
                customCollectRecipient.GetComponent<IOnCustomCollect>().OnCustomCollect(increment, gameObject);
                AudioSource aso = GetComponent<AudioSource>();
                if (aso) { aso.Play(); }
            }

            transform.localScale = Vector3.zero;
            collected = true;
            StartCoroutine(DeathRites());
        }
    }
	
	/*void Update () {
	
	}*/
}
