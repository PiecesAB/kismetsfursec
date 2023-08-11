using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSnakeBullets : MonoBehaviour
{
    public BulletData bulletData;
    public Vector2 range;
    public Vector2 speed;
    public float spacing;
    public int count;
    public float offScreenDistance = 320;

    private Transform cameraTransform;
    private Transform myTransform;

    private List<BulletObject> myBullets = new List<BulletObject>();


    private float internalRand1;
    private float internalRand2;

    private Vector2 Evaluate1(float t)
    {
        return new Vector2(
            range.x*(Mathf.Clamp01(Mathf.PerlinNoise(internalRand1 + t * speed.x, internalRand2 + t * speed.x)) - 0.5f),
            range.y*(Mathf.Clamp01(Mathf.PerlinNoise(- internalRand2 - t * speed.y, - internalRand1 - t * speed.y)) - 0.5f)
            );
    }

    private void OnDestroy()
    {
        foreach (BulletObject b in myBullets)
        {
            BulletRegister.MarkToDestroy(b);
        }
    }

    public void UpdateBulletPositions()
    {
        float t = (float)(DoubleTime.ScaledTimeSinceLoad % 100000.0);
        for (int i = 0; i < count; ++i)
        {
            Vector2 localPos = Evaluate1(t + i * spacing);
            if (myBullets[i] == null) { continue; }
            myBullets[i].UpdateTransform(myTransform.TransformPoint(localPos));
        }
        
    }

    void Start()
    {
        internalRand1 = Fakerand.Single() * 8000f;
        internalRand2 = Fakerand.Single() * 8000f;
        cameraTransform = Camera.main.transform;
        myTransform = transform;

        for (int i = 0; i < count; ++i)
        {
            BulletObject b = new BulletObject();
            //b.image = bulletData.sprite.texture;
            //b.scale = bulletData.scale;
            //b.material = bulletData.material;
            b.killRadiusRatio = bulletData.killRadiusRatio;
            b.damage = bulletData.damage;
            b.deletTime = Mathf.Infinity;
            //b.position = transform.position;
            b.UpdateTransform(transform.position, bulletData.scale);
            b.doesntMoveOnItsOwn = true;
            b.color = bulletData.color;
            b.destroyOnLeaveScreen = false;
            b.collisionDisabled = bulletData.collisionDisabled;
            b.rotateWithMovementAngle = bulletData.rotateSprite;

            BulletRegister.Register(ref b, bulletData.material, bulletData.sprite.texture);
            myBullets.Add(b);
        }

        UpdateBulletPositions();
    }

    
    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (((Vector2)cameraTransform.position - (Vector2)myTransform.position).magnitude >= offScreenDistance) { return; }
        
        UpdateBulletPositions();
    }
}
