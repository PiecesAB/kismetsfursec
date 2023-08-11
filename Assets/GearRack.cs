using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearRack : Gear
{
    private Vector3 origin;
    private float lastPosition = 0f;

    protected override void ChildStart()
    {
        origin = transform.position;
        lastPosition = 0f;
    }

    private float GetCurrPosition()
    {
        return Vector2.Dot(transform.up, transform.position - origin);
    }

    protected override float CalculateTranslation()
    {
        return GetCurrPosition() - lastPosition;
    }

    protected override void ChildAfterUpdate()
    {
        lastPosition = GetCurrPosition();
    }

    protected override bool RotationTrigger()
    {
        return (GetCurrPosition() - lastPosition) > 0.1f;
    }

    protected override void RotateInterlock(float dt)
    {
        transform.position += transform.up * dt;

        if (neighbors.Count > 0)
        {
            Gear singleNeighbor = null;
            foreach (Gear nb in neighbors) { singleNeighbor = nb; break; }

            bool isDoubleGear = (singleNeighbor is GearCircularDouble);

            if (isDoubleGear)
            {
                GearCircularDouble gcd = (GearCircularDouble)singleNeighbor;
                if (gcd.innerNeighbors.Contains(this) && gearCollider.Distance(gcd.innerCollider).distance > -3f)
                {
                    RotateCancelBegin();
                }
                else if (gearCollider.Distance(gcd.outerCollider).distance > -3f)
                {
                    RotateCancelBegin();
                }
            }
            else if (gearCollider.Distance(singleNeighbor.gearCollider).distance > -3f)
            {
                RotateCancelBegin();
            }
        }
    }

    protected override void RotateCancel()
    {
        transform.position = origin + (transform.up * lastPosition);
    }

    
}
