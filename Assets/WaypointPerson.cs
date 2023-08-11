using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointPerson : MonoBehaviour
{
    public enum InterpolationStyle
    {
        Normal, PopWhenInvisible
    }

    public Transform waypointHolder;
    public double interpDuration;
    public EasingOfAccess.EasingType easingType;
    public InterpolationStyle style;
    public BoxCollider2D dialogCollider;
    public GameObject mainObject = null;
    public Renderer mainRenderer;

    private Vector2 dialogColliderOrigOffset;

    private Transform[] waypoints;
    private int nextWaypoint = -1;

    private double startInterpTime = -999.0;
    private Vector3 startInterpPos;

    private Transform t;

    private void Start()
    {
        if (mainObject == null) { mainObject = gameObject; }
        t = mainObject.transform;
        if (dialogCollider)
        {
            dialogColliderOrigOffset = dialogCollider.offset;
        }
        waypoints = new Transform[waypointHolder.childCount];
        for (int i = 0; i < waypointHolder.childCount; ++i)
        {
            waypoints[i] = waypointHolder.GetChild(i);
            waypoints[i].GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public void MoveToNextWaypoint()
    {
        if (nextWaypoint >= waypoints.Length - 1) { return; }
        switch (style)
        {
            case InterpolationStyle.Normal: StartCoroutine(MoveToNextWaypointNormal()); break;
            case InterpolationStyle.PopWhenInvisible: StartCoroutine(MoveWhenInvisible()); break;
        }
    }

    private IEnumerator MoveToNextWaypointNormal()
    {
        int thisNext = ++nextWaypoint;
        dialogCollider.offset = new Vector2(0, 999999);
        startInterpTime = DoubleTime.ScaledTimeSinceLoad;
        startInterpPos = (thisNext == 0) ? transform.position : waypoints[thisNext - 1].position;
        Vector3 goalPos = waypoints[thisNext].position;
        while (thisNext == nextWaypoint)
        {
            float prog = (float)((DoubleTime.ScaledTimeSinceLoad - startInterpTime) / interpDuration);
            float pv = EasingOfAccess.Evaluate(easingType, prog);
            t.position = Vector3.Lerp(startInterpPos, goalPos, pv);
            if (prog >= 1f) {
                t.position = goalPos;
                dialogCollider.offset = dialogColliderOrigOffset;
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    private IEnumerator MoveWhenInvisible()
    {
        int thisNext = ++nextWaypoint;
        dialogCollider.offset = new Vector2(0, 999999);
        Vector3 goalPos = waypoints[thisNext].position;
        yield return new WaitUntil(() => !mainRenderer.isVisible);
        if (thisNext != nextWaypoint) { print("no pop"); yield break; }
        t.position = goalPos;
        dialogCollider.offset = dialogColliderOrigOffset;
    }
}
