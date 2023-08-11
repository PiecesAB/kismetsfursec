using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMakerTracker : MonoBehaviour
{
    public List<BulletObject> myBullets = new List<BulletObject>();
    public bool destroyBulletsWithObject = true;
    public bool dontFireIfWentOffscreen = true;

    [HideInInspector]
    public List<BulletHellMakerFunctions> nextMakersOnBulletUnregister = new List<BulletHellMakerFunctions>();

    public bool useRotationDegrees = false;

    private StaticBulletsOnVertices staticBullets;

    private void Start()
    {
        staticBullets = GetComponent<StaticBulletsOnVertices>();
    }

    private void OnDestroy()
    {
        if (destroyBulletsWithObject)
        {
            ClearAllBullets();
        }
        TryToFire(true);
    }

    private void ClearAllBullets()
    {
        foreach (BulletObject b in myBullets)
        {
            BulletRegister.MarkToDestroy(b);
        }
    }

    private void TryToFire(bool beingDestroyed = false)
    {
        for (int i = 0; i < myBullets.Count; ++i)
        {
            BulletObject b = myBullets[i];
            if (b == null) {
                myBullets.RemoveAt(i);
                --i; continue;
            }
            if (dontFireIfWentOffscreen && b.wentOffscreen) {
                myBullets.RemoveAt(i);
                --i; continue;
            }
            if (beingDestroyed || (!BulletRegister.IsRegistered(b))) // if it has been deleted from the register
            {
                if (nextMakersOnBulletUnregister.Count > 0 && gameObject.activeInHierarchy)
                {
                    foreach (BulletHellMakerFunctions submaker in nextMakersOnBulletUnregister)
                    {
                        submaker.RepositionToOtherBullet(b, useRotationDegrees);
                        submaker.Fire();
                    }
                }

                myBullets.RemoveAt(i);
                --i; continue;
            }
        }
    }

    private void Update()
    {
        if (staticBullets) { myBullets = staticBullets.myBullets; }
        TryToFire();
    }
}
