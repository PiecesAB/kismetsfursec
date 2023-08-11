using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is kind of a clone of WaypointPerson, but more complex for boss movement
public class BossMover : MonoBehaviour
{
    public enum InterpolationStyle
    {
        Normal, PopWhenInvisible
    }

    public Transform waypointHolder;
    [HideInInspector]
    public BossWaypoint[] waypoints;
    public Renderer visCheck;
    [HideInInspector]
    public int currentWaypointIndex;

    [HideInInspector]
    public float currentDelay = 0f;

    private bool travelling = false;

    private Vector3 travelOriginPosition;
    private Vector3 travelTargetPosition;
    private double startInterpTime;
    [HideInInspector]
    public BossWaypoint targetWaypoint;
    private BossController controller;
    private Transform t;

    private int startingWaypointIndex = 0;

    public void ChangeStartIndex(int i)
    {
        startingWaypointIndex = currentWaypointIndex = i;
    }


    private void Start()
    {
        t = transform;
        
        controller = GetComponent<BossController>();
        if (!controller)
        {
            throw new System.Exception("No BossController");
        }

        waypoints = new BossWaypoint[waypointHolder.childCount];
        for (int i = 0; i < waypointHolder.childCount; ++i)
        {
            waypoints[i] = waypointHolder.GetChild(i).GetComponent<BossWaypoint>();
            waypoints[i].mover = this;
            waypoints[i].controller = controller;
            waypoints[i].GetComponent<SpriteRenderer>().sprite = null;
        }
        if (waypoints.Length == 0) { throw new System.Exception("No way!"); }
        currentWaypointIndex = startingWaypointIndex;
        // go to the 0th waypoint
        t.position = waypoints[0].GetPosition();
        
        // it is possible for the health bar count to be higher: the boss doesn't need to move to the next waypoint.
        if (controller.barCount < waypoints.Length)
        {
            throw new System.Exception("Health bar count too low for the amount of waypoints!");
        }
    }

    public enum MoveToNextCode
    {
        Success = 0, CurrentlyTravelling, EndOfTheLine 
    }

    public BossWaypoint GetCurrentWaypoint()
    {
        if (travelling) { return null; }
        return waypoints[currentWaypointIndex];
    }

    public bool IsTravelling()
    {
        return travelling;
    }

    public MoveToNextCode MoveToNext(float delay)
    {
        if (travelling) { return MoveToNextCode.CurrentlyTravelling; }
        if (currentWaypointIndex >= waypoints.Length - 1) { return MoveToNextCode.EndOfTheLine; }
        travelling = true;
        travelOriginPosition = transform.position;
        targetWaypoint = waypoints[currentWaypointIndex + 1];
        travelTargetPosition = targetWaypoint.GetPosition();
        currentDelay = delay;
        startInterpTime = DoubleTime.ScaledTimeSinceLoad;
        return MoveToNextCode.Success;
    }

    private void IncrementWaypoint()
    {
        ++currentWaypointIndex;
        travelling = false;
        controller.JustStoppedTravelling();
    }

    public void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (travelling)
        {
            if (currentDelay > 0f)
            {
                currentDelay -= 0.0166666666f * Time.timeScale;
                currentDelay = Mathf.Max(0f, currentDelay);
                if (currentDelay <= 0f)
                {
                    startInterpTime = DoubleTime.ScaledTimeSinceLoad;
                }
            }
            else
            {
                travelTargetPosition = targetWaypoint.GetPosition(); // just in case the position moves
                switch (targetWaypoint.interpStyle)
                {
                    case BossWaypoint.InterpolationStyle.Normal:
                        float prog = (float)((DoubleTime.ScaledTimeSinceLoad - startInterpTime) / targetWaypoint.interpDuration);
                        float pv = EasingOfAccess.Evaluate(targetWaypoint.easingStyle, prog);
                        t.position = Vector3.Lerp(travelOriginPosition, travelTargetPosition, pv);
                        if (prog >= 1f) {
                            t.position = travelTargetPosition;
                            IncrementWaypoint();
                        }
                        break;
                    case BossWaypoint.InterpolationStyle.PopWhenInvisible:
                        if (!visCheck.isVisible)
                        {
                            t.position = travelTargetPosition;
                            IncrementWaypoint();
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        else // normal movement around a waypoint
        {
            BossWaypoint currentWaypoint = waypoints[currentWaypointIndex];
            if (currentWaypoint.mvtStyle != BossWaypoint.MovementStyle.DoNothing)
            {
                t.position = currentWaypoint.GetPosition();
            }
        }
    }
}
