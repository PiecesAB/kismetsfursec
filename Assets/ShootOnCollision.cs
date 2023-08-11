using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootOnCollision : MonoBehaviour
{
    [System.Serializable]
    public struct CollisionType
    {
        public bool enter;
        public bool stay;
        public bool exit;
    }

    public CollisionType trigger;
    public CollisionType regular;
    public BulletHellMakerFunctions[] makers;

    public double fireDelay = 0f;
    private double lastFireTime = Mathf.NegativeInfinity;

    private bool DelayCheck()
    {
        if (DoubleTime.ScaledTimeSinceLoad >= lastFireTime + fireDelay)
        {
            lastFireTime = DoubleTime.ScaledTimeSinceLoad;
            return true;
        }
        else { return false; }
    }

    private void Fire(Collider2D col)
    {
        if (!DelayCheck()) { return; }

        foreach (var maker in makers)
        {
            maker.Fire();
        }
    }

    private void Fire(Collision2D col)
    {
        if (!DelayCheck()) { return; }

        foreach (var maker in makers)
        {
            maker.Fire();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (trigger.enter) { Fire(col); }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (trigger.stay) { Fire(col); }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (trigger.exit) { Fire(col); }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (regular.enter) { Fire(col); }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (regular.stay) { Fire(col); }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (regular.exit) { Fire(col); }
    }
}
