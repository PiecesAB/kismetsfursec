using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThermalBlock : MonoBehaviour, IOnNontouchInteractions
{
    private Coroutine mainUpdate;
    private List<ThermalBlock> all = new List<ThermalBlock>();

    private class HealthColInfo
    {
        public double startTime;
        public int colliderCount;

        public HealthColInfo(double st, int c)
        {
            startTime = st;
            colliderCount = c;
        }
    }

    // stores start collision time
    private static Dictionary<KHealth, HealthColInfo> healthCols = new Dictionary<KHealth, HealthColInfo>();
    private static List<KHealth> damagedThisFrame = new List<KHealth>();

    private void Start()
    {
        all.Add(this);
    }

    private void Enter(KHealth kh)
    {
        if (!healthCols.ContainsKey(kh))
        {
            healthCols[kh] = new HealthColInfo(DoubleTime.ScaledTimeSinceLoad, 1);
            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().Play();
        }
        else { ++healthCols[kh].colliderCount; }
        if (damagedThisFrame.Contains(kh)) { return; }
        damagedThisFrame.Add(kh);
        StartCoroutine(RemoveDamagedNextFrame(kh));
        kh.overheat += 0.2f;
    }

    private void Stay(KHealth kh)
    {
        if (Time.timeScale == 0) { return; }
        if (damagedThisFrame.Contains(kh)) { return; }
        if (!healthCols.ContainsKey(kh)) { throw new System.Exception("how?"); }
        damagedThisFrame.Add(kh);
        StartCoroutine(RemoveDamagedNextFrame(kh));
        float e = (float)System.Math.Min(DoubleTime.ScaledTimeSinceLoad - healthCols[kh].startTime, 10000.0);
        kh.overheat += 0.0166666666f * Time.timeScale * e * e;
    }

    private void Exit(KHealth kh)
    {
        if (!healthCols.ContainsKey(kh)) { throw new System.Exception("how?"); }
        --healthCols[kh].colliderCount;
        if (healthCols[kh].colliderCount <= 0) { healthCols.Remove(kh); }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.rigidbody || col.rigidbody.isKinematic) { Physics2D.IgnoreCollision(col.collider, col.otherCollider); return; }
        KHealth kh = col.gameObject.GetComponent<KHealth>();
        if (!kh) { return; }
        Enter(kh);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        KHealth kh = col.gameObject.GetComponent<KHealth>();
        if (!kh) { return; }
        Stay(kh);
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        KHealth kh = col.gameObject.GetComponent<KHealth>();
        if (!kh) { return; }
        Exit(kh);
    }

    private IEnumerator RemoveDamagedNextFrame(KHealth kh)
    {
        yield return new WaitForEndOfFrame();
        damagedThisFrame.Remove(kh);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    private void PlayerNontouchInteraction(KHealth kh)
    {
        if (healthCols.ContainsKey(kh)) { return; }
        if (damagedThisFrame.Contains(kh)) { return; }
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().Play();
        damagedThisFrame.Add(kh);
        StartCoroutine(RemoveDamagedNextFrame(kh));
        kh.overheat += 0.2f;
    }

    public void NormalJumpOff(GameObject plr)
    {
        KHealth kh = plr.GetComponent<KHealth>();
        if (!kh) { return; }
        PlayerNontouchInteraction(kh);
    }

    public void WallJumpOff(GameObject plr)
    {
        KHealth kh = plr.GetComponent<KHealth>();
        if (!kh) { return; }
        PlayerNontouchInteraction(kh);
    }
}
