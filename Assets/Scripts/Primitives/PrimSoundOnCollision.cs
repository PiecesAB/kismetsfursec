using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimSoundOnCollision : MonoBehaviour
{
    public AudioSource audioSource;

    public bool helpWithBouncePhysics = true;

    private void OnCollisionEnter2D(Collision2D col)
    {
        audioSource.Stop();
        audioSource.Play();
    }
}
