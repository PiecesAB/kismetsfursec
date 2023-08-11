using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFacePlayer : MonoBehaviour
{
    private SpriteRenderer sr;

    void Update()
    {
        if (!sr) { sr = GetComponent<SpriteRenderer>(); }
        if (!sr) { return; }
        if (Time.timeScale == 0) { return; }
        Transform pt = LevelInfoContainer.GetActiveControl()?.transform;
        if (!pt) { return; }
        sr.flipX = (pt.position.x - transform.position.x < 0);
    }
}
