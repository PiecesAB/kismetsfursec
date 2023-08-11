using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamBlockHelper : MonoBehaviour
{
    public bool globalRotationLocked = false;

    private float origZ;
    private SpriteRenderer spr;

    private void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        if (globalRotationLocked)
        {
            origZ = transform.eulerAngles.z;
        }
    }

    void Update()
    {
        //if (Time.timeScale == 0) { return; } commented out because we want it to appear turning in the animation.

        if (globalRotationLocked)
        {
            float plrRot = LevelInfoContainer.GetActiveControl().transform.eulerAngles.z;
            transform.eulerAngles = new Vector3(0, 0, origZ + plrRot);
        }
    }
}
