using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimRotationRandomizer : MonoBehaviour
{
    public enum RotateType
    {
        Random, AimAtPlayer
    }

    public RotateType rotateType;
    public Vector2 randomRange = new Vector2(0,360);
    public float offset;
    public float inaccuracy;

    private void Awake()
    {
        switch (rotateType)
        {
            case RotateType.Random:
                transform.localEulerAngles = new Vector3(0, 0, Fakerand.Single(randomRange.x, randomRange.y) + offset);
                break;
            case RotateType.AimAtPlayer:
                if (LevelInfoContainer.GetActiveControl() == null) { transform.localEulerAngles = Vector3.zero; }
                Vector3 dif = LevelInfoContainer.GetActiveControl().transform.position - transform.position;
                transform.localEulerAngles = new Vector3(0, 0, (Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg) + Fakerand.Single(-inaccuracy, inaccuracy) + offset);
                break;
        }
    }
}
