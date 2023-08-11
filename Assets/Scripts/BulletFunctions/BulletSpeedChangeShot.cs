using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BulletMakerTracker))]
[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletSpeedChangeShot : MonoBehaviour
{
    private BulletMakerTracker tracker;
    private BulletHellMakerFunctions maker;
    public enum SpeedMode
    {
        CurveOverTime, CurveOverDistance
    }
    public SpeedMode spinMode;
    public AnimationCurve curve;

    private Dictionary<BulletObject, float> realStartSpeed = new Dictionary<BulletObject, float>();

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
            float oldVelocity = b.startingVelocity;
            if (!realStartSpeed.ContainsKey(b))
            {
                realStartSpeed[b] = b.startingVelocity;
            }
            switch (spinMode)
            {
                case SpeedMode.CurveOverTime:
                    b.startingVelocity = realStartSpeed[b] * curve.Evaluate(existTime);
                    break;
                case SpeedMode.CurveOverDistance:
                    b.startingVelocity = realStartSpeed[b] * curve.Evaluate(radius);
                    break;
            }
            // Hack to make the bullet consistent with our curve.
            // Ignore acceleration. Just don't use it with this module.
            float t = b.startingDirection * Mathf.Deg2Rad;
            Vector2 dirVec = new Vector2(Mathf.Cos(t), Mathf.Sin(t));
            b.originPosition += (Vector3)(existTime * (oldVelocity - b.startingVelocity) * dirVec);
        }

        List<KeyValuePair<BulletObject, float>> kvps = realStartSpeed.ToList();
        foreach (var kvp in kvps)
        {
            if (!BulletRegister.IsRegistered(kvp.Key))
            {
                realStartSpeed.Remove(kvp.Key);
            }
        }
    }
}
