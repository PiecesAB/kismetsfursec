using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDamager : MonoBehaviour {

    public float damagePerFrame;
    public AudioSource lightLaserSound;
    public AudioSource heavyLaserSound;
    public string damageReason = "laser";
    public bool hitPlr;

    public void Start()
    {
        hitPlr = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        KHealth kh = col.GetComponent<KHealth>();
        if (kh != null)
        {
            hitPlr = true;
            kh.ChangeHealth(-damagePerFrame, damageReason);
        }
    }

    public void Update()
    {
        if (GetComponent<Collider2D>().enabled)
        {
            if (hitPlr)
            {
                lightLaserSound.Stop();
                if (!heavyLaserSound.isPlaying)
                {
                    heavyLaserSound.Play();
                }
                hitPlr = false;
            }
            else
            {
                heavyLaserSound.Stop();
                if (!lightLaserSound.isPlaying)
                {
                    lightLaserSound.Play();
                }
            }
        }
        else
        {
            lightLaserSound.Stop();
            heavyLaserSound.Stop();
        }
    }


}
