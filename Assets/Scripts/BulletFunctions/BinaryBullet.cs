using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BinaryBullet
{
    public static HashSet<BulletObject> bullets = new HashSet<BulletObject>();

    public static Texture bulletBit0 = Resources.Load<Texture>("BinaryBullet/bulletBit0");
    public static Texture bulletBit1 = Resources.Load<Texture>("BinaryBullet/bulletBit1");

    // call this after setting other things like collisionDisabled
    public static void Initialize(BulletObject b, int mode, out Texture img) // 0: start off. 1: start on
    {
        img = null;
        if (mode != 0 && mode != 1) { return; }
        img = (mode == 0) ? bulletBit0 : bulletBit1;
        b.collisionDisabled = (mode == 0);
        if (mode == 0) {
            b.color *= 0.5f;
            //b.originPosition = new Vector3(b.originPosition.x, b.originPosition.y, 32);
        }
        bullets.Add(b);
    }

    public static void ToggleAll()
    {
        if (Time.timeScale == 0) { return; }
        if (bullets.Count == 0) { return; }

        BulletObject[] bListCopy = new BulletObject[bullets.Count];
        bullets.CopyTo(bListCopy);
        for (int i = 0; i < bListCopy.Length; ++i)
        {
            BulletObject b = bListCopy[i];
            Texture img;
            if (BulletRegister.GetImage(ref b) == bulletBit0)
            {
                img = bulletBit1;
                b.color *= 2f;
                //b.originPosition = new Vector3(b.originPosition.x, b.originPosition.y, 0);
            }
            else
            {
                img = bulletBit0;
                b.color *= 0.5f;
                //b.originPosition = new Vector3(b.originPosition.x, b.originPosition.y, 32);
            }
            b.collisionDisabled = !b.collisionDisabled;
            BulletRegister.Reregister(ref b, BulletRegister.GetMaterial(ref b), img);
            bullets.Add(b);
        }

        foreach (BulletHellMakerFunctions m in BulletHellMakerFunctions.all)
        {
            if (m == null) { continue; }
            if (m.bulletData.bitToggleOnJump != -1)
            {
                m.bulletData.bitToggleOnJump = (m.bulletData.bitToggleOnJump == 0) ? 1 : 0;
            }
        }
    }
}
