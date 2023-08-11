using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleMaker : MonoBehaviour
{
    public enum GrowthBehavior
    {
        Periodic, PlayerNear
    }

    [Header("The following is not a prefab. It's in the model")]
    public GameObject sampleIcicle;
    public GrowthBehavior growthBehavior;
    [Header("In PlayerNear mode, period is the recharge time")]
    public double period;
    public float maxSpeed;
    public float accel;
    public double offset;
    public float disappearSpeed = 1f;
    public bool alwaysRunning = false;
    private PrimDeleteOnLeaveScreen primDeleteOnLeaveScreen;
    private HomingBulletBehavior homingBulletBehavior;


    private SpriteRenderer mySpr;

    private double timer;
    private Vector3 sampleOrigin;

    private double oldRat = 0.0;

    private const float peekDistance = 96f;
    private const float triggerDistance = 16f;

    void Start()
    {
        primDeleteOnLeaveScreen = sampleIcicle.GetComponent<PrimDeleteOnLeaveScreen>();
        homingBulletBehavior = sampleIcicle.GetComponent<HomingBulletBehavior>();
        homingBulletBehavior.maxAccel = maxSpeed;
        homingBulletBehavior.accel = accel;
        homingBulletBehavior.icicleDisappearSpeed = disappearSpeed;
        homingBulletBehavior.enabled = false;
        primDeleteOnLeaveScreen.enabled = false;
        sampleIcicle.SetActive(true);
        sampleOrigin = sampleIcicle.transform.position;

        timer = 0.0;
        mySpr = GetComponent<SpriteRenderer>();
    }

    void MakeNewIcicle()
    {
        GameObject newIcicle = Instantiate(sampleIcicle, sampleOrigin + 16f * transform.right, sampleIcicle.transform.rotation, null);
        newIcicle.GetComponent<HomingBulletBehavior>().enabled = true;
        newIcicle.GetComponent<PrimDeleteOnLeaveScreen>().enabled = true;
    }


    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (!mySpr.isVisible && !alwaysRunning) { timer = 0.0; return; }
        if (period <= 0) { throw new System.Exception("I'm not repeating that every 0 seconds."); }

        if (offset > 1e-6)
        {
            offset -= 0.01666666666666666 * Time.timeScale;
            return;
        }

        switch (growthBehavior)
        {
            case GrowthBehavior.PlayerNear:
                Encontrolmentation e = LevelInfoContainer.GetActiveControl();
                if (e == null) { break; }
                float dif = Mathf.Abs(Vector2.Dot(e.transform.position - transform.position, transform.up));
                if (dif < peekDistance)
                {
                    float prog = Mathf.Sqrt(Mathf.Clamp01(Mathf.InverseLerp(peekDistance, triggerDistance, dif)));
                    sampleIcicle.transform.position = sampleOrigin + 16f * prog * (float)((period - timer)/period) * transform.right;
                    if (dif <= triggerDistance && timer == 0.0)
                    {
                        MakeNewIcicle();
                        timer = period;
                    }
                }
                else
                {
                    sampleIcicle.transform.position = sampleOrigin;
                }

                timer -= 0.016666666666666666 * Time.timeScale;
                if (timer <= 0.0) { timer = 0.0; }
                break;
            default:
            case GrowthBehavior.Periodic:

                float rat = (float)(timer / period);

                if (rat >= 0.5f)
                {
                    float halfRat = 2f * (rat - 0.5f);
                    sampleIcicle.transform.position = sampleOrigin + 16f*halfRat*transform.right;
                }
                else
                {
                    sampleIcicle.transform.position = sampleOrigin;
                }

                if (rat < oldRat) // then make a new one
                {
                    MakeNewIcicle();
                }

                timer += 0.016666666666666666 * Time.timeScale;
                timer %= period;
                oldRat = rat;
                break;
        }

    }
}
