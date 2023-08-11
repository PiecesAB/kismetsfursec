using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletMakerTracker))]
public class BulletDeleteOnPlrDist : MonoBehaviour
{
    public float minDist;
    public float maxDist;
    private BulletMakerTracker tracker;

    private void Start()
    {
        tracker = GetComponent<BulletMakerTracker>();
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        Transform plrt = LevelInfoContainer.GetActiveControl()?.transform;
        if (!plrt) { return; }
        Vector2 plrpos = plrt.position;
        for (int i = 0; i < tracker.myBullets.Count; ++i)
        {
            BulletObject b = tracker.myBullets[i];
            float d = (plrpos - (Vector2)b.GetPosition()).magnitude;
            if (d < minDist || d > maxDist)
            {
                BulletRegister.MarkToDestroy(b);
            }
        }
    }
}
