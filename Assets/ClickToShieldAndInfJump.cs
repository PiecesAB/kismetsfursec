using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToShieldAndInfJump : SpecialGunTemplate
{
    public int layers = 0;
    private GameObject shieldObject = null;
    [SerializeField]
    private GameObject shieldPrefab;
    [SerializeField]
    private GameObject circlePrefab;
    [SerializeField]
    private AudioSource ravelSpecialAudio;
    [SerializeField]
    private AudioClip createSound;
    [SerializeField]
    private AudioClip shedSound0;
    [SerializeField]
    private AudioClip shedSound1;
    [SerializeField]
    private GameObject currShield = null;

    public bool ShieldCircleActive()
    {
        return currShield != null;
    }

    public void UpdateLayerGraphic(bool increase = false)
    {
        if (!shieldObject) { shieldObject = Instantiate(shieldPrefab, transform); }
        Transform t = shieldObject.transform;
        for (int i = 0; i < 5; ++i)
        {
            t.GetChild(i).gameObject.SetActive(i < layers);
        }
        t.GetChild(5).gameObject.SetActive(layers > 0); // 5 is punch hitbox
        Destroy(currShield);
        currShield = Instantiate(circlePrefab, transform);
        currShield.transform.position = transform.position;
        ShieldingCircle nsc = currShield.GetComponent<ShieldingCircle>();
        nsc.radius = increase ? 64f : 48f;
        nsc.shrinkSelfSpeed = increase ? 32f : 24f;
        nsc.clearBulletsTime = increase ? -1.0 : 0.05;

        ravelSpecialAudio.Stop();
        ravelSpecialAudio.pitch = 1f;
        if (increase)
        {
            ravelSpecialAudio.clip = createSound;
        }
        else if (layers > 0)
        {
            ravelSpecialAudio.clip = shedSound1;
            ravelSpecialAudio.pitch = Mathf.Pow(2f, layers / 12f);
        }
        else {
            ravelSpecialAudio.clip = shedSound0;
            GameObject cutTail0 = t.GetChild(6).gameObject;
            GameObject cutTail1 = Instantiate(cutTail0, cutTail0.transform.position - Vector3.forward * 32f, transform.rotation, null);
            cutTail1.SetActive(true);
            cutTail1.GetComponent<Rigidbody2D>().velocity = new Vector2(Fakerand.Single(-50f, 50f), 50f);
            Destroy(cutTail1, 3f);
        }
        ravelSpecialAudio.Play();
    }

    public void EnemyDestroyed()
    {
        if (!currShield)
        {
            --layers;
            UpdateLayerGraphic();
        }
    }

    protected override void AimingBegin()
    {
        bool inv = (transform.localScale.x < 0);
        if ((e.currentState & 3UL) == 1UL) { nextangle = inv ? 0f : 180f; }
        if ((e.currentState & 3UL) == 2UL) { nextangle = inv ? 180f : 0f; }
        if ((e.currentState & 12UL) == 4UL) { nextangle = 90f; }
        if ((e.currentState & 12UL) == 8UL) { nextangle = 270f; }
    }

    protected override void AimingUpdate()
    {
    }

    protected override void ChildStart()
    {
    }

    protected override void ChildUpdate()
    {
        bm.youCanInfinityJump = (layers > 0);
        mainSR.material.SetFloat("_Tail", layers);
    }

    protected override float Fire()
    {
        if (layers >= 3) { return 0f; } // nerfed: 3 layers instead of 5, 5 is too cheese!
        ++layers;
        UpdateLayerGraphic(true);
        return 1f;
    }

    protected override void GraphicsUpdateWhenAiming()
    {
    }

    protected override void GraphicsUpdateWhenNotAiming()
    {
    }
}
