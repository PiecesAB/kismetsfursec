using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearCircular : Gear
{
    private float radius;

    protected float lastRotZ;

    protected override void ChildStart()
    {
        radius = GetComponent<CircleCollider2D>().radius;
        lastRotZ = transform.eulerAngles.z;
    }

    protected void NormalizeLastRot()
    {
        if (lastRotZ - transform.eulerAngles.z > 180f)
        {
            lastRotZ -= 360f;
        }

        if (lastRotZ - transform.eulerAngles.z < -180f)
        {
            lastRotZ += 360f;
        }
    }

    protected override bool RotationTrigger()
    {
        NormalizeLastRot();
        return Mathf.Abs(lastRotZ - transform.eulerAngles.z) > 0.1f;
    }

    protected override float CalculateTranslation()
    {
        NormalizeLastRot();
        return radius*(transform.eulerAngles.z - lastRotZ)*Mathf.Deg2Rad;
    }

    protected override void RotateInterlock(float dt)
    {
        float dt2 = -dt * Mathf.Rad2Deg / radius;
        transform.eulerAngles += Vector3.forward * dt2;
    }

    protected override void RotateCancel()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, lastRotZ);
    }

    protected override void ChildAfterUpdate()
    {
        lastRotZ = transform.eulerAngles.z;
    }
}
