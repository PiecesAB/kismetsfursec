using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierMovePlatformOnTouch : MonoBehaviour
{
    public enum State
    {
        WaitingToStart, Moving
    }

    [HideInInspector]
    public State state = State.WaitingToStart;
    public float targetSpeed = 60f;
    public float acceleration = 2f;
    public float fadeOutTime = 2f;
    public float waitTime = 4f;
    [SerializeField]
    private PrimBezierMove wire;
    public SpriteRenderer[] sprites;

    private float oldSpeed = 0f;
    private float currSpeed = 0f;

    private float origSpeed;

    private int colliderCount = 0;

    private int graceBeforeStart = 4;

    private float timeSinceLastCollision = 0f;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.rigidbody || col.rigidbody.isKinematic) { return; }
        timeSinceLastCollision = 0f;
        ++colliderCount;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (!col.rigidbody || col.rigidbody.isKinematic) { return; }
        --colliderCount;
        if (colliderCount == 0) { graceBeforeStart = 4; }
    }

    void Start()
    {
        origSpeed = wire.speeds[0];
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }

        switch (state)
        {
            case State.WaitingToStart:
                if (colliderCount > 0) { --graceBeforeStart; }
                if (graceBeforeStart <= 0) { state = State.Moving; }
                break;
            case State.Moving:
                if (currSpeed < targetSpeed) { currSpeed += acceleration * Time.timeScale; }
                if (currSpeed > targetSpeed) { currSpeed = targetSpeed; }
                if (colliderCount == 0)
                {
                    timeSinceLastCollision += Time.deltaTime;
                }
                for (int i = 0; i < sprites.Length; ++i)
                {
                    Color sc = sprites[i].color;
                    if (timeSinceLastCollision < waitTime)
                    {
                        sprites[i].color = new Color(sc.r, sc.g, sc.b, 1f);
                    }
                    else
                    {
                        float rat = Mathf.Clamp01((timeSinceLastCollision - waitTime) / fadeOutTime);
                        sprites[i].color = new Color(sc.r, sc.g, sc.b, 1f - rat);
                    }
                }
                if (timeSinceLastCollision >= waitTime + fadeOutTime)
                {
                    timeSinceLastCollision = 0f;
                    int a = 0;
                    Transform o = wire.objs[0];
                    bool rae = wire.reverseAtEnd[0];
                    wire.RemoveObject(ref a, false);
                    wire.InsertObjectAtBeginning(o, origSpeed, false, rae);
                    state = State.WaitingToStart;
                    oldSpeed = currSpeed = 0f;
                    for (int i = 0; i < sprites.Length; ++i)
                    {
                        Color sc = sprites[i].color;
                        sprites[i].color = new Color(sc.r, sc.g, sc.b, 1f);
                    }
                }
                break;
            default:
                break;
        }

        if (currSpeed != oldSpeed)
        {
            oldSpeed = currSpeed;
            wire.ChangeObjectSpeed(0, (currSpeed == 0f) ? 1e-5f : currSpeed);
        }
    }
}
