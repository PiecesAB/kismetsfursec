using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherCannon : GenericBlowMeUp
{
    public enum State
    {
        Wait, Full
    }

    public State state = State.Wait;
    public float fuseLength = 3f;
    public bool fuseStartsOnScreen = false;
    public float strength = 400f;
    public bool fixedAngle = false;
    public float regenSpeed = 0f;
    [SerializeField]
    private SpriteRenderer fuseSprite;
    [SerializeField]
    private ParticleSystem fuseSpark;
    [SerializeField]
    private Transform rotHead;
    [SerializeField]
    private GameObject shootDust;
    public AudioClip enterSound;
    public AudioClip exitSound;
    public AudioSource fuseSound;

    private double lastStateChangeTime;
    private Collider2D myTrigger;
    private float origFuseLength;
    private AudioSource myAS;

    private Vector3 tempFillScale;

    private int cooldown = 0;

    private void Start()
    {
        lastStateChangeTime = -5f;
        state = State.Wait;
        myTrigger = GetComponent<Collider2D>();
        fuseSprite.size = new Vector2(4, 16f * fuseLength);
        fuseSprite.transform.localPosition = Vector3.up * 8f * fuseLength;
        if (regenSpeed <= 0f)
        {
            fuseSprite.color = Color.Lerp(Color.red, Color.white, 0.5f);
        }
        fuseSpark.gameObject.SetActive(false);
        origFuseLength = fuseLength;
        myAS = GetComponent<AudioSource>();
    }

    private IEnumerator CooldownCount()
    {
        while (cooldown > 0)
        {
            yield return new WaitForEndOfFrame();
            --cooldown;
        }
        yield return null;
    }

    private IEnumerator ShootAnim()
    {
        float t = (float)(DoubleTime.ScaledTimeSinceLoad - lastStateChangeTime);
        while (state == State.Wait && t < 0.5f)
        {
            rotHead.localScale = Vector3.one + Vector3.right * Mathf.Lerp(0.7f, 0f, EasingOfAccess.CubicIn(t * 2f));
            yield return new WaitForEndOfFrame();
            t = (float)(DoubleTime.ScaledTimeSinceLoad - lastStateChangeTime);
        }
        rotHead.localScale = Vector3.one;
        yield return null;
    }

    private IEnumerator Regen()
    {
        while (state == State.Wait && fuseLength < origFuseLength)
        {
            fuseLength += regenSpeed * 0.016666666f * Time.timeScale;
            if (fuseLength > origFuseLength) { fuseLength = origFuseLength; }
            fuseSprite.size = new Vector2(4, 16f * fuseLength);
            fuseSprite.transform.localPosition = Vector3.up * 8f * fuseLength;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private void Shoot(GameObject plr)
    {
        if (state == State.Wait) { return; }
        state = State.Wait;
        plr.transform.localScale = tempFillScale;
        plr.transform.position = transform.position + rotHead.right * 12f;
        plr.GetComponent<AudioSource>().UnPause();
        StartCoroutine(LauncherEnemy.TrajectoryRestoreControl(rotHead.right, plr, strength));
        cooldown = 10;
        StartCoroutine(CooldownCount());
        lastStateChangeTime = DoubleTime.ScaledTimeSinceLoad;
        fuseSpark.gameObject.SetActive(false);
        Instantiate(shootDust, transform.position + rotHead.right * 24f, Quaternion.identity);
        StartCoroutine(ShootAnim());

        myAS.Stop();
        myAS.clip = exitSound;
        myAS.Play();
        fuseSound.Stop();

        if (fuseLength == 0f && regenSpeed <= 0f) { BlowMeUp(0.25f, true); }
        if (regenSpeed > 0f) { StartCoroutine(Regen()); }
    }

    private IEnumerator Fuse(Encontrolmentation ctrl)
    {
        while (state == State.Full)
        {
            float t = (float)(DoubleTime.ScaledTimeSinceLoad - lastStateChangeTime);
            rotHead.localScale = Vector3.one * Mathf.Lerp(1.5f, 1f, EasingOfAccess.CubicIn(t * 2f));

            ctrl.eventAbutton = Encontrolmentation.ActionButton.BButton;
            ctrl.eventAName = "Fire";

            if (Time.timeScale != 0)
            {
                if (!fixedAngle)
                {
                    float z = rotHead.localEulerAngles.z;
                    if ((ctrl.currentState & 768UL) == 256UL)
                    {
                        z = (rotHead.localEulerAngles.z + 723f) % 360f;
                    }
                    else if ((ctrl.currentState & 768UL) == 512UL)
                    {
                        z = (rotHead.localEulerAngles.z + 537f) % 360f - 180f;
                    }
                    z = Mathf.Clamp(z, 0f, 180f);
                    rotHead.localEulerAngles = Vector3.forward * z;
                }
                
                if (ctrl.ButtonDown(32UL, 32UL)) {
                    Shoot(ctrl.gameObject);
                }
                else
                {
                    ctrl.transform.position = transform.position + rotHead.right * 96f;
                }
            }

            yield return new WaitForEndOfFrame();
            fuseLength -= 0.016666666f * Time.timeScale;
            if (fuseLength < 0.01f) { fuseLength = 0f; }
            fuseSprite.size = new Vector2(4, 16f * fuseLength);
            fuseSprite.transform.localPosition = Vector3.up * 8f * fuseLength;
            fuseSpark.transform.localPosition = Vector3.up * 16f * fuseLength;
            if (fuseLength == 0f) { Shoot(ctrl.gameObject); }
        }
        yield return null;
    }

    private void Fill(GameObject g)
    {
        if (state == State.Full) { return; }
        state = State.Full;
        tempFillScale = g.transform.localScale;
        g.transform.localScale = Vector3.zero;
        g.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        g.GetComponent<BasicMove>().midairRush.volume = 0f;
        g.GetComponent<AudioSource>().Pause();
        LauncherEnemy.RemovePlayerControl(g);
        lastStateChangeTime = DoubleTime.ScaledTimeSinceLoad;
        fuseSpark.gameObject.SetActive(true);
        fuseSpark.transform.localPosition = Vector3.up * 16f * fuseLength;
        LauncherCannonUpdateHelper h = gameObject.AddComponent<LauncherCannonUpdateHelper>();
        h.e = g.GetComponent<Encontrolmentation>();
        h.main = this;

        myAS.Stop();
        myAS.clip = enterSound;
        myAS.Play();
        fuseSound.Play();

        PlatformControlButtonMain pm = gameObject.GetComponentInParent<PlatformControlButtonMain>();
        if (pm && pm.boxSpriteSample) //falling platform : trigger it
        {
            PlatformControlButton pb = pm.GetComponentInChildren<PlatformControlButton>();
            pb.On();
        }

        StartCoroutine(Fuse(h.e));
    }

    private void CheckCol(Collider2D col)
    {
        if (col.gameObject.layer != 20) { if (myTrigger) { Physics2D.IgnoreCollision(col, myTrigger); } return; }
        if (!col.gameObject.GetComponent<BasicMove>().CanCollide) { return; }
        if (cooldown > 0) { return; }
        if (fuseLength == 0f) { return; }
        Fill(col.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        CheckCol(col);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        CheckCol(col);
    }
}
