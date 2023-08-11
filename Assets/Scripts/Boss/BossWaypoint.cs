using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWaypoint : MonoBehaviour
{
    public enum InterpolationStyle
    {
        Normal, PopWhenInvisible
    }

    // If you want the boss to move by animation, use the BossController to play it and set the DoNothing mvt. style
    public enum MovementStyle
    {
        DoNothing, Static, OtherPosition, SubWaypoints
    }

    public MovementStyle mvtStyle = MovementStyle.Static;
    public float interpDuration = 1f;
    public EasingOfAccess.EasingType easingStyle = EasingOfAccess.EasingType.SineSmooth;
    public InterpolationStyle interpStyle;
    public Transform otherObject;
    public Vector3 offset;

    [HideInInspector]
    public BossMover mover;
    [HideInInspector]
    public BossController controller;

    public Vector3 GetPosition()
    {
        switch (mvtStyle)
        {
            case MovementStyle.OtherPosition:
                return otherObject.position + offset;
            default:
                return transform.position + offset;
        }
    }

    // Shortcuts for speedrunners: if this waypoint has collider(s), the boss will move to the next waypoint as soon as it's triggered.
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.layer == 20 && mover.GetCurrentWaypoint() == this)
        {
            controller.SkipToNextBarrage();
        }
    }
}
