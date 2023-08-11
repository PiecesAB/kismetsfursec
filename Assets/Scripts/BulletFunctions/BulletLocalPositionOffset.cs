using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLocalPositionOffset : MonoBehaviour, IBulletMakerOnShot
{
    public Vector3 offset;
    private BulletHellMakerFunctions maker;

    public void BeforeShot()
    {
        if (!maker) { maker = GetComponent<BulletHellMakerFunctions>(); }
        if (!maker) { return; }
        maker.bulletShooterData.posOffset = transform.TransformVector(offset);
    }

    public void OnShot()
    {
    }
}
