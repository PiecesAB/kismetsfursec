using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBulletSingle : MonoBehaviour
{
    [Header("The sprite renderer will determine color, material, and image.")]
    public BulletData bulletData;

    private BulletObject b;

    private void OnDestroy()
    {
        BulletRegister.MarkToDestroy(b);
    }

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        b = new BulletObject();
        //b.image = sr.sprite.texture;
        //b.scale = bulletData.scale;
        //b.material = sr.material;
        b.killRadiusRatio = bulletData.killRadiusRatio;
        b.damage = bulletData.damage;
        b.deletTime = Mathf.Infinity;
        //b.position = transform.position;
        b.UpdateTransform(transform.position, bulletData.scale);
        b.doesntMoveOnItsOwn = true;
        b.color = sr.color;
        b.destroyOnLeaveScreen = false;
        b.collisionDisabled = bulletData.collisionDisabled;
        b.rotateWithMovementAngle = bulletData.rotateSprite;

        BulletRegister.Register(ref b, sr.material, sr.sprite.texture);

        Destroy(sr);
    }
}
