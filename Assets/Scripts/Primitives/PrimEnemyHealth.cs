using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimEnemyHealth : MonoBehaviour
{

    public GameObject mainObject;
    public Renderer rendererForDetectingTetraLasers;
    [Header("for randomized names, type cat:<name category>")]
    public string mainObjectName;
    public GameObject hurtEffect;
    public float damage = 0f;
    public float damageToDestroy = 1f;
    public float defense = 0f;
    public double lastHurtTime = 0.0;
    public Vector2 hBarPosition = new Vector2(0,32);
    public float punchDamageMultiplier = 1f;
    public GameObject healthUIPrefab;
    public double hideHealthUIAfterThisTime = 4.0;
    public bool unrotatedUI = false;

    public GameObject iframesTimerPrefab;
    private GameObject iframesTimerCurr;
    private TextMesh iframesText;

    [HideInInspector]
    public bool isBossHealth = false;
    [HideInInspector]
    public bool displayUI = true;
    private string myName = "";

    private GameObject healthUIcurrent;
    private const float minHealthUIBGX = 32f;
    private const float unitsPerDamage = 6f;

    private double clrhuicTime = 0.0;


    private float damageLagBehind = 0f;

    private const float punchDamage = 1f;
    private const float damagePerSRWidth = 2f;

    private List<int> tetraLasersHit = new List<int>();
    private List<int> tetraLasersCooldown = new List<int>();

    //private const double timeBetweenHitRegisters = 0.1f;
    private List<GameObject> hitsThisFrame = new List<GameObject>();

    private CircleCollider2D cc2;
    private BoxCollider2D bc2;

    public struct OnHurtInfo
    {
        public Vector3 pos;
        public float amt;
        public float total;

        public OnHurtInfo(Vector3 _pos, float _amt, float _total)
        {
            pos = _pos;
            amt = _amt;
            total = _total;
        }

    }

    public void HealthUIUpdate()
    {
        if (!healthUIcurrent)
        {
            return;
        }

        if (DoubleTime.UnscaledTimeSinceLoad >= clrhuicTime)
        {
            Destroy(healthUIcurrent);
            return;
        }

        Transform bg = healthUIcurrent.transform.GetChild(0);
        Transform bar = healthUIcurrent.transform.GetChild(1);
        Transform disp = healthUIcurrent.transform.GetChild(2);

        float supposedLength = unitsPerDamage * damageToDestroy;

        bg.localScale = new Vector3(Mathf.Max(1f, (supposedLength + 4f) / 32f), bg.localScale.y, bg.localScale.z);
        bar.localScale = new Vector3(supposedLength/32f, bg.localScale.y, bg.localScale.z);
        SpriteRenderer barSR = bar.GetComponent<SpriteRenderer>();
        float rat = damage / damageToDestroy;
        barSR.material.SetFloat("_Val", rat);
        damageLagBehind = Mathf.MoveTowards(damageLagBehind, rat, 0.05f / damageToDestroy);
        barSR.material.SetFloat("_LerpVal", damageLagBehind);

        disp.GetComponent<TextMesh>().text = myName;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!hitsThisFrame.Contains(col.gameObject))
        {
            hitsThisFrame.Add(col.gameObject);
            if (col.gameObject.layer == 19)
            {
                BasicMove bm = col.transform.parent.GetComponent<BasicMove>();
                float ravelMult = 1f;
                if (!bm) //Ravel's ability
                {
                    bm = col.transform.parent.parent.GetComponent<BasicMove>();
                    bm.GetComponent<ClickToShieldAndInfJump>().EnemyDestroyed();
                    ravelMult = 3f;
                }
                float ppm = bm.punchPowerMultiplier;
                DoDamage(Mathf.Max(ppm * punchDamageMultiplier * punchDamage * ravelMult - defense, 0f), col.transform.position);
            }
            else if (col.gameObject.tag == "SuperRay")
            {
                SuperRay sr = col.GetComponent<SuperRay>();
                float remain = damageToDestroy - damage + defense;
                float maxRayDamage = damagePerSRWidth * sr.currentThickness;
                if (remain < maxRayDamage)
                {
                    sr.currentThickness -= remain / damagePerSRWidth;
                    DoDamage(remain - defense + 0.01f, (Vector3)sr.points[sr.points.Count-1] + col.transform.position);
                }
                else
                {
                    DoDamage(Mathf.Max(0f, maxRayDamage - defense), (Vector3)sr.points[sr.points.Count - 1] + col.transform.position);
                    sr.currentThickness = 0f;
                }
            }
        }
    }

    public void DoDamageWithDefense(float amt, Vector3 colPos)
    {
        DoDamage(amt - defense, colPos);
    }

    public void DoDamage(float amt, Vector3 colPos)
    {
        damage += amt;

        if (displayUI)
        {
            //make UI
            if (healthUIcurrent == null)
            {
                healthUIcurrent = Instantiate(healthUIPrefab, transform.position, transform.rotation, transform);
                healthUIcurrent.transform.localPosition += (Vector3)hBarPosition;
                healthUIcurrent.GetComponent<PlrUI>().desireY = hBarPosition.y;
                if (unrotatedUI)
                {
                    healthUIcurrent.AddComponent<PrimZeroOrient>();
                }
            }
            clrhuicTime = DoubleTime.UnscaledTimeSinceLoad + hideHealthUIAfterThisTime;
            HealthUIUpdate();
        }

        if (damageToDestroy-damage < 0.01f)
        {
            Destroy(healthUIcurrent);
            if (!isBossHealth)
            {
                try
                {
                    mainObject.SendMessage("BlowMeUp", 0f, SendMessageOptions.RequireReceiver);
                }
                catch
                {
                    Destroy(mainObject);
                }
            }
            else
            {
                Instantiate(hurtEffect, mainObject.transform.position, Quaternion.identity);
                mainObject.SendMessage("OnHurt", new OnHurtInfo(colPos, amt, damage), SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            Instantiate(hurtEffect, mainObject.transform.position, Quaternion.identity);
            mainObject.SendMessage("OnHurt", new OnHurtInfo(colPos, amt, damage), SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDestroy()
    {
        LevelInfoContainer.allEnemiesInLevel.Remove(this);
    }

    private void Start()
    {
        LevelInfoContainer.allEnemiesInLevel.Add(this);
        myName = (mainObjectName.Length > 4 && mainObjectName.Substring(0, 4) == "cat:") ?
                EnemyNamer.all[mainObjectName.Substring(4)][Fakerand.Int(0, EnemyNamer.all[mainObjectName.Substring(4)].Length)]
                : mainObjectName;
        lastHurtTime = 0.0;
        bc2 = GetComponent<BoxCollider2D>();
        cc2 = GetComponent<CircleCollider2D>();
    }

    public void IFramesDisplayUpdate(float remainingSeconds)
    {
        if (remainingSeconds <= 0f)
        {
            if (iframesTimerCurr)
            {
                Destroy(iframesTimerCurr);
                iframesTimerCurr = null;
                iframesText = null;
            }
        }
        else
        {
            if (!iframesTimerCurr)
            {
                iframesTimerCurr = Instantiate(iframesTimerPrefab, transform.position, transform.rotation, transform);
                iframesTimerCurr.transform.localPosition += (Vector3)hBarPosition;
                iframesTimerCurr.GetComponent<PlrUI>().desireY = hBarPosition.y;
                if (unrotatedUI)
                {
                    iframesTimerCurr.AddComponent<PrimZeroOrient>();
                }
                iframesText = iframesTimerCurr.GetComponentInChildren<TextMesh>();
            }
            iframesText.text = Mathf.Floor(remainingSeconds).ToString();
        }
    }

    private float GetRadius()
    {
        if (cc2)
        {
            return cc2.radius;
        }
        if (bc2)
        {
            return Mathf.Max(bc2.size.x, bc2.size.y) * 1.414f;
        }
        return 0;
    }

    void Update()
    {
        hitsThisFrame.Clear();
        HealthUIUpdate();

        if (rendererForDetectingTetraLasers != null
            && rendererForDetectingTetraLasers.isVisible)
        {
            float rad = GetRadius();

            foreach (LaserBullet v in LaserBullet.tetraLasers)
            {
                if (!tetraLasersHit.Contains(v.GetInstanceID()) && v.TetraLaserBlockCollision(transform.position, rad))
                {
                    tetraLasersHit.Add(v.GetInstanceID());
                    tetraLasersCooldown.Add(12);
                    DoDamage(0.5f, transform.position);
                }
            }
        }

        for (int i = 0; i < tetraLasersHit.Count; ++i)
        {
            --tetraLasersCooldown[i];
            if (tetraLasersCooldown[i] < 0)
            {
                tetraLasersHit.RemoveAt(i);
                tetraLasersCooldown.RemoveAt(i);
                --i;
            }
        }
        
    }
}
