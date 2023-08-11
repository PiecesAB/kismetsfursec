using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavensGateESpeedUp : MonoBehaviour
{
    public float multiplier = 1f;

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        Transform pt = LevelInfoContainer.GetActiveControl()?.transform;
        if (!pt) { return; }
        multiplier = 1f + Mathf.Clamp(pt.transform.position.y - transform.position.y - 40, 0, 128) / 64;
    }
}
