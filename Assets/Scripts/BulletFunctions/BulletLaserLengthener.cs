using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletMakerTracker))]
[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletLaserLengthener : MonoBehaviour
{
    public float lengtheningTime = 0.5f;

    private BulletMakerTracker tracker;
    private BulletHellMakerFunctions maker;
    private float fullLength;

    private void Start()
    {
        tracker = GetComponent<BulletMakerTracker>();
        maker = GetComponent<BulletHellMakerFunctions>();
        fullLength = maker.bulletData.scale.x;
        maker.bulletData.scale = new Vector2(0, maker.bulletData.scale.y);
    }

    private void Update()
    {
        float lengtheningTime2 = lengtheningTime;
        if (lengtheningTime == 0f)
        {
            lengtheningTime2 = 0.5f * fullLength / (maker.bulletData.speed * maker.bulletData.simulationSpeedMult);
        }
        foreach (BulletObject b in tracker.myBullets)
        {
            double existTime = b.GetExistTime();
            if (existTime < lengtheningTime2) {
                b.UpdateTransform(b.GetPosition(), new Vector2(fullLength * (float)(existTime / lengtheningTime2), maker.bulletData.scale.y));
            }
            else
            {
                b.UpdateTransform(b.GetPosition(), new Vector2(fullLength, maker.bulletData.scale.y));
            }
        }
    }
}
