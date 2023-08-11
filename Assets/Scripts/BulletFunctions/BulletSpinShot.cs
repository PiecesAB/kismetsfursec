using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BulletMakerTracker))]
[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletSpinShot : MonoBehaviour
{
    private BulletMakerTracker tracker;
    private BulletHellMakerFunctions maker;
    public enum SpinMode
    {
        DecreaseWithDistance, CurveOverTime, CurveOverDistance
    }
    public SpinMode spinMode;
    public float referenceRadius = 64f;
    public AnimationCurve curve;

    private Dictionary<BulletObject, float> realStartTorque = new Dictionary<BulletObject, float>();

    void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        tracker = GetComponent<BulletMakerTracker>();
    }

    void Update()
    {
        for (int i = 0; i < tracker.myBullets.Count; ++i)
        {
            BulletObject b = tracker.myBullets[i];
            float existTime = (float)b.GetExistTime();
            Vector2 rv = b.originPosition - b.GetPosition();
            float radius = rv.magnitude;
            float oldTorque = b.startingTorque;
            if (!realStartTorque.ContainsKey(b))
            {
                realStartTorque[b] = b.startingTorque;
            }
            switch (spinMode)
            {
                case SpinMode.DecreaseWithDistance:
                    b.startingTorque = realStartTorque[b] * Mathf.Sqrt(referenceRadius / radius);
                    break;
                case SpinMode.CurveOverTime:
                    b.startingTorque = realStartTorque[b] * curve.Evaluate(existTime);
                    break;
                case SpinMode.CurveOverDistance:
                    b.startingTorque = realStartTorque[b] * curve.Evaluate(radius);
                    break;
            }
            // Hack to make the bullet consistent with our curve.
            // Ignore angular acceleration. Just don't use it with this module.
            b.startingDirection += existTime * (oldTorque - b.startingTorque);
        }

        List<KeyValuePair<BulletObject, float>> kvps = realStartTorque.ToList();
        foreach (var kvp in kvps)
        {
            if (!BulletRegister.IsRegistered(kvp.Key))
            {
                realStartTorque.Remove(kvp.Key);
            }
        }
    }
}
