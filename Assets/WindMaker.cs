using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class WindMaker : MonoBehaviour
{
    public BulletData windBullet0;
    public BulletData windBullet1;
    public float bulletIntensity = 0.25f;
    public float gravityBulletInfluence = 0.3f;
    public float intensity = 3f;
    public float changeSpeed = 0.2f;
    private Camera mainCam;
    private Transform mainCamTrans;
    [SerializeField]
    private ParticleSystem windParticlesRight;
    [SerializeField]
    private ParticleSystem windParticlesLeft;

    private List<BulletObject> myBullets = new List<BulletObject>();
    private List<Vector2> bulletVelocities = new List<Vector2>();

    public const float fadeIn = 5f;
    private float rand1;

    private AudioSource sound;

    void Start()
    {
        sound = GetComponent<AudioSource>();
        mainCam = Camera.main;
        mainCamTrans = mainCam.transform;
        transform.SetParent(mainCamTrans, false);
        mainCam.GetComponent<ColorCorrectionCurves>().DiscoStart();
        rand1 = Fakerand.Single() * 1000f;
    }

    private void MakeBullet(float r)
    {
        Vector3 spawn = transform.position;
        spawn += Vector3.right * ((r < 0) ? 168 : -168);
        spawn += Vector3.up * Fakerand.Single(-54f, 162f);

        BulletObject b = new BulletObject();
        BulletData windBullet = (Fakerand.Int(0, 2) == 1) ? windBullet1 : windBullet0;

        Texture nothing;
        windBullet.TransferBasicInfo(b, out nothing);

        b.UpdateTransform(spawn, 0, windBullet.scale);
        b.originPosition = spawn;

        b.startingDirection = (r > 0)?0:180;

        myBullets.Add(b);
        bulletVelocities.Add(Vector2.zero);
        BulletRegister.Register(ref b, windBullet.material, windBullet.sprite.texture);
    }

    private void ClearAllBullets()
    {
        foreach (BulletObject b in myBullets)
        {
            BulletRegister.MarkToDestroy(b);
        }
    }

    private void OnDestroy()
    {
        ClearAllBullets();
    }

    void Update()
    {

        if (Time.timeScale == 0) { return; }
        if (Door1.levelComplete) { ClearAllBullets(); return; }

        float r = (2f * Mathf.PerlinNoise(rand1, (float)((DoubleTime.ScaledTimeSinceLoad * changeSpeed) % 1000000.0)) - 1f);
        float f = (DoubleTime.ScaledTimeSinceLoad >= fadeIn) ? 1f : ((float)DoubleTime.ScaledTimeSinceLoad / fadeIn);
        float currGrav = f*r*intensity;
        Physics2D.gravity = new Vector2(currGrav, Physics2D.gravity.y);
        sound.volume = Mathf.Abs(r);
        sound.pitch = 0.7f + Mathf.Abs(r) * 0.8f;
        sound.panStereo = -0.7f * Mathf.Sign(r);

        if (r > 0f)
        {
            if (windParticlesRight.isEmitting) { windParticlesRight.Stop(false, ParticleSystemStopBehavior.StopEmitting); }
            if (!windParticlesLeft.isEmitting) { windParticlesLeft.Play(false); }
            ParticleSystem.MainModule ml = windParticlesLeft.main;
            ml.startSpeed = 640 * r;
            ParticleSystem.EmissionModule el = windParticlesLeft.emission;
            el.rateOverTime = 32f + 64f * r;
        }
        else
        {
            if (windParticlesLeft.isEmitting) { windParticlesLeft.Stop(false, ParticleSystemStopBehavior.StopEmitting); }
            if (!windParticlesRight.isEmitting) { windParticlesRight.Play(false); }
            ParticleSystem.MainModule mr = windParticlesRight.main;
            mr.startSpeed = -640 * r;
            ParticleSystem.EmissionModule er = windParticlesRight.emission;
            er.rateOverTime = 32f - 64f * r;
        }

        if (Fakerand.Single() < Mathf.Abs(r)*bulletIntensity) 
        {
            MakeBullet(r);
        }

        for (int i = 0; i < myBullets.Count; ++i)
        {
            BulletObject b = myBullets[i];
            Vector2 v = bulletVelocities[i];
            if (!BulletRegister.IsRegistered(b)) // if it has been deleted from the register
            {
                myBullets.RemoveAt(i); bulletVelocities.RemoveAt(i);
                --i; continue;
            }

            b.originPosition += (Vector3)v;
            bulletVelocities[i] += Physics2D.gravity*gravityBulletInfluence*0.016666666f;
        }
    }
}
