using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAttractor : MonoBehaviour, IBulletMakerResetOnSuperRepeat
{
    public Vector2 offset;
    public Vector2 range;
    public float attractStrength = 0.04f;

    private Vector2 currPos;

    private BulletMakerTracker tracker;

    public void MakerReset()
    {
        float randX = Fakerand.Single() - 0.5f;
        float randY = Fakerand.Single() - 0.5f;
        currPos = new Vector2(range.x * randX, range.y * randY) + offset;
    }

    private void Start()
    {
        MakerReset();
        tracker = GetComponent<BulletMakerTracker>();
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        Vector3 t = transform.position;
        for (int i = 0; i < tracker.myBullets.Count; ++i)
        {
            BulletObject b = tracker.myBullets[i];
            Vector3 npos = t + new Vector3(currPos.x, currPos.y, b.originPosition.z);
            b.startingDirection = Mathf.Atan2(npos.y - b.originPosition.y, npos.x - b.originPosition.x)*Mathf.Rad2Deg;
            b.originPosition = Vector3.Lerp(b.originPosition, npos, attractStrength);
        }
    }
}
