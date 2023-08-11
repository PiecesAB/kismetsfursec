using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimAnimationEvents : MonoBehaviour, ITripwire
{
    public enum EventType
    {
        OnVisible, OnTripwire
    }

    [System.Serializable]
    public struct EventToAnimName
    {
        public EventType eventType;
        public string animName;
    }

    public EventToAnimName[] fakeEventToAnimNameDict;
    private Dictionary<EventType, string> eventToAnimNameDict = new Dictionary<EventType, string>();

    public Animator animator;
    public float delayAllEvents = 0f;

    public void Start()
    {
        if (animator == null) { animator = GetComponent<Animator>(); }
        foreach (EventToAnimName e in fakeEventToAnimNameDict) { eventToAnimNameDict[e.eventType] = e.animName; }
    }

    public void OnTrip()
    {
        Event(EventType.OnTripwire);
        
    }

    private void OnBecameVisible()
    {
        Event(EventType.OnVisible);
    }

    private IEnumerator EventAfterDelay(EventType t)
    {
        yield return new WaitForSeconds(delayAllEvents);
        animator.CrossFade(eventToAnimNameDict[t], 0f);
    }

    public void Event(EventType t)
    {
        if (delayAllEvents > 0f)
        {
            StartCoroutine(EventAfterDelay(t));
            return;
        }
        animator.CrossFade(eventToAnimNameDict[t], 0f);
    }
}
