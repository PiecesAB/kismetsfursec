using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperGunPersonSFX : MonoBehaviour
{
    public AudioSource stompSound;
    public AudioSource whirrSound;
    public AudioSource trapCreakSound;
    public AudioSource trapCloseSound;
    public AudioSource showerSound;

    private float whirrPitch;

    public void StompSound()
    {
        stompSound.Stop(); stompSound.Play();
        FollowThePlayer.main.vibSpeed += 3f;
    }

    public void WhirrSound()
    {
        whirrPitch = 0.1f;
        whirrSound.Stop(); whirrSound.Play();
    }

    public void TrapCreakSound()
    {
        trapCreakSound.Stop(); trapCreakSound.Play();
    }

    public void TrapCloseSound()
    {
        trapCreakSound.Stop();
        trapCloseSound.Stop(); trapCloseSound.Play();
    }

    public void ShowerSound()
    {
        showerSound.Stop(); showerSound.Play();
    }

    public void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (whirrSound)
        {
            whirrPitch = Mathf.MoveTowards(whirrPitch, 1.8f, 0.1f);
            whirrSound.pitch = whirrPitch;
        }
    }
}
