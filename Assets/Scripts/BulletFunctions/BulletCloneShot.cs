using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletCloneShot : MonoBehaviour, IBulletMakerOnShot
{
    public int instantCloneShots = 0;
    public int numberInCircleChangePerClone = 0;
    public float selfRotationPerClone = 0;
    public float offsetRotationPerClone = 0;
    public float offsetRotAfterPosPerClone = 0;
    public float speedChangePerClone = 0;
    public float torqueChangePerClone = 0;
    public float radialStartDistChangePerClone = 0;
    public float spreadAngleChangePerClone = 0;
    public AnimationCurve radialStartDistChangeCurve = AnimationCurve.Constant(0, 1, 0);
    public Vector3 meshOffsetChangePerClone = Vector3.zero;
    public float waitBetweenShots = 0f;

    private bool debounce = false;

    private BulletHellMakerFunctions maker;
    private float radialStartDistCurveVal;

    private void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
    }

    public void BeforeShot() { }

    public void IncrementStats (int i)
    {
        transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                transform.eulerAngles.z + selfRotationPerClone
                );
        maker.bulletShooterData.numberInCircle += numberInCircleChangePerClone;
        maker.bulletData.speed += speedChangePerClone;
        maker.bulletShooterData.offsetRotation += offsetRotationPerClone;
        maker.bulletShooterData.offsetRotationAfterPosition += offsetRotAfterPosPerClone;
        maker.bulletData.torque += torqueChangePerClone;
        maker.meshShotOffset += meshOffsetChangePerClone;
        maker.bulletShooterData.radialStartDistance += radialStartDistChangePerClone;
        radialStartDistCurveVal += radialStartDistChangeCurve.Evaluate(i);
        maker.bulletShooterData.radialStartDistance += radialStartDistChangeCurve.Evaluate(i);
        maker.bulletShooterData.spreadAngle += spreadAngleChangePerClone;
    }

    public void ResetStats()
    {
        transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                transform.eulerAngles.z - selfRotationPerClone * instantCloneShots
                );
        maker.bulletShooterData.numberInCircle -= numberInCircleChangePerClone * instantCloneShots;
        maker.bulletData.speed -= speedChangePerClone * instantCloneShots;
        maker.bulletShooterData.offsetRotation -= offsetRotationPerClone * instantCloneShots;
        maker.bulletShooterData.offsetRotationAfterPosition -= offsetRotAfterPosPerClone * instantCloneShots;
        maker.bulletData.torque -= torqueChangePerClone * instantCloneShots;
        maker.meshShotOffset -= meshOffsetChangePerClone * instantCloneShots;
        maker.bulletShooterData.radialStartDistance -= (radialStartDistChangePerClone * instantCloneShots) + radialStartDistCurveVal;
        maker.bulletShooterData.spreadAngle -= spreadAngleChangePerClone * instantCloneShots;
    }

    private IEnumerator OnShotTimer()
    {
        radialStartDistCurveVal = 0f;
        double t = DoubleTime.ScaledTimeSinceLoad;
        for (int i = 0; i < instantCloneShots; ++i)
        {
            t += waitBetweenShots;
            if (t - DoubleTime.ScaledTimeSinceLoad > 0.15)
            {
                yield return new WaitForSeconds((float)(t - DoubleTime.ScaledTimeSinceLoad - 0.15));
            }
            while (DoubleTime.ScaledTimeSinceLoad < t)
            {
                yield return new WaitForEndOfFrame();
            }
            IncrementStats(i);
            maker.FireAtTime(t);
        }
        ResetStats();
        debounce = false;
        yield return null;
    }

    public void OnShot()
    {
        if (!maker) { maker = GetComponent<BulletHellMakerFunctions>(); }
        // debounce prevents infinite recursion.
        if (debounce) { return; }
        debounce = true;

        if (waitBetweenShots <= 0f) // all clones shoot at same time
        {
            radialStartDistCurveVal = 0f;
            for (int i = 0; i < instantCloneShots; ++i)
            {
                IncrementStats(i);
                maker.Fire();
            }
            ResetStats();
            debounce = false;
        } else
        {
            StartCoroutine(OnShotTimer());
        }
    }
}
