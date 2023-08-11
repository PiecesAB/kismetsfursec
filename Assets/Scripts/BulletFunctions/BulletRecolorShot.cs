using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletMakerTracker))]
public class BulletRecolorShot : MonoBehaviour
{
    private BulletMakerTracker tracker;

    public Gradient gradient;
    public AnimationCurve gradientPositionOverTime;
    public float delay = 0f;

    void Start()
    {
        tracker = GetComponent<BulletMakerTracker>();
    }

    void Update()
    {
        foreach (BulletObject b in tracker.myBullets)
        {
            float existTime = (float)b.GetExistTime();
            b.color = gradient.Evaluate(gradientPositionOverTime.Evaluate(existTime - delay));
            b.renderGroup = new BulletControllerHelper.RenderGroup(b.materialInternalIdx, b.textureInternalIdx, b.color);
        }
    }
}
