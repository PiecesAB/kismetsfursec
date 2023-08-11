using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnMoverPlatform : MonoBehaviour
{
    [SerializeField]
    protected PrimBezierMove wire;
    [SerializeField]
    protected float initPlatformSpeed = 16f;
    [SerializeField]
    protected TurnstileSwitch turnstile;

    protected virtual void Update()
    {
        if (Time.timeScale == 0) { return; }
        wire.ChangeObjectSpeed(0, -turnstile.dx * initPlatformSpeed);
    }
}
