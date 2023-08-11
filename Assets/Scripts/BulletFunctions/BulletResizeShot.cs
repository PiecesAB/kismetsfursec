using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletMakerTracker))]
[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletResizeShot : MonoBehaviour
{
    private BulletMakerTracker tracker;
    private BulletHellMakerFunctions maker;

    public AnimationCurve x;
    public AnimationCurve y;

    void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        tracker = GetComponent<BulletMakerTracker>();
    }

    void Update()
    {
        foreach (BulletObject b in tracker.myBullets)
        {
            float existTime = (float)b.GetExistTime();
            b.UpdateTransform(b.GetPosition(), new Vector2(maker.bulletData.scale.x * x.Evaluate(existTime), maker.bulletData.scale.y * y.Evaluate(existTime)));
        }
    }
}
