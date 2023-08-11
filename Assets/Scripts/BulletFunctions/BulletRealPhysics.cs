using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRealPhysics : MonoBehaviour
{
    private BulletMakerTracker tracker;
    public Vector2 velocity;
    public Vector2 acceleration;
    public bool relativeToBulletDirection = false;

    private void Start()
    {
        tracker = GetComponent<BulletMakerTracker>();
    }

    private Vector2 RotateVector2(BulletObject b, Vector2 v)
    {
        if (b == null) { return Vector2.zero; }
        if (!relativeToBulletDirection) { return v; }
        float t = b.startingDirection * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(t) * v.x - Mathf.Sin(t) * v.y,
                           Mathf.Sin(t) * v.x + Mathf.Cos(t) * v.y);
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        for (int i = 0; i < tracker.myBullets.Count; ++i)
        {
            BulletObject b = tracker.myBullets[i]; if (b == null) { continue; }
            if (b.extraData == null) { b.extraData = new Dictionary<BulletObject.ExtraDataTag, object>(); }
            if (!b.extraData.ContainsKey(BulletObject.ExtraDataTag.PhysVel)) { b.extraData[BulletObject.ExtraDataTag.PhysVel] = RotateVector2(b, velocity); }
            if (!b.extraData.ContainsKey(BulletObject.ExtraDataTag.PhysAccel)) { b.extraData[BulletObject.ExtraDataTag.PhysAccel] = RotateVector2(b, acceleration); }

            b.originPosition += (Vector3)(Vector2)b.extraData[BulletObject.ExtraDataTag.PhysVel]*Time.timeScale*0.01666666f;
            b.extraData[BulletObject.ExtraDataTag.PhysVel] = (Vector2)b.extraData[BulletObject.ExtraDataTag.PhysVel] + (Vector2)b.extraData[BulletObject.ExtraDataTag.PhysAccel]*Time.timeScale*0.0166666666f; 
        }
    }
}
