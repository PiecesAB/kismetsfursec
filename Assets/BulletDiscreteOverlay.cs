using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletDiscreteOverlay : MonoBehaviour
{
    private struct BulletObjectDiscrete
    {
        public BulletObject bullet;
        public BulletObject main;
        public BulletObjectDiscrete(BulletObject b, BulletObject m)
        {
            bullet = b;
            main = m;
        }
    }

    public float gridSize = 8f;

    private BulletHellMakerFunctions maker;

    private List<BulletObjectDiscrete> myBullets = new List<BulletObjectDiscrete>();

    private void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        maker.discreteOverlay = this;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < myBullets.Count; ++i)
        {
            BulletObjectDiscrete d = myBullets[i];
            bool mr = BulletRegister.IsRegistered(d.main);
            bool br = BulletRegister.IsRegistered(d.bullet);
            if (mr) { BulletRegister.MarkToDestroy(d.main, false); }
            if (br) { BulletRegister.MarkToDestroy(d.bullet, false); };
        }
    }

    public void MakeNew(BulletObject m)
    {
        BulletObject b = maker.MakeBulletForOtherObject(false);
        b.startingVelocity = 0.0001f;
        b.startingDirection = m.startingDirection;
        b.doesntMoveOnItsOwn = true;
        b.deletTime = m.deletTime + 0.5f;
        m.color = Color.clear;
        m.UpdateRenderGroup();
        m.killRadiusRatio = -1f;
        m.grazeDisabled = true;

        myBullets.Add(new BulletObjectDiscrete(b, m));
    }

    private float Round(float x)
    {
        return Mathf.RoundToInt(x / gridSize) * gridSize;
    }

    private void Update()
    {
        for (int i = 0; i < myBullets.Count; ++i)
        {
            BulletObjectDiscrete d = myBullets[i];
            bool mr = BulletRegister.IsRegistered(d.main);
            bool br = BulletRegister.IsRegistered(d.bullet);
            if (!mr || !br)
            {
                myBullets.RemoveAt(i--);
                if (mr) { BulletRegister.MarkToDestroy(d.main, false); }
                if (br) { BulletRegister.MarkToDestroy(d.bullet, false); };
                continue;
            }
            Vector3 p = d.main.GetPosition();
            d.bullet.UpdateTransform(new Vector3(Round(p.x), Round(p.y), p.z), d.main.GetRotationDegrees());
        }
    }
}
