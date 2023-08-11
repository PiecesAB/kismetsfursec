using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Encontrolmentation))]
public class EncontrolmentationAutoMover : MonoBehaviour
{
    private Encontrolmentation e;
    private BasicMove bm;

    public List<Encontrolmentation.PairTimeWithControl> fakeEventsTable = new List<Encontrolmentation.PairTimeWithControl>();
    public List<Encontrolmentation.PairPositionWithVelocity> checkpoints = new List<Encontrolmentation.PairPositionWithVelocity>();
    public GameObject popEffect;

    public Encontrolmentation copyRegularMovements = null;
    public enum CopyType
    {
        Reverse, BlockOnRight, JustFacePlayer
    }
    public CopyType copyType = CopyType.Reverse;

    private bool amPlaying;
    private double myStartTime;
    private int currEvent;
    public bool startPlayingAutomatically = false;
    private int currCheckpoint;
    private const float positionToleranceForPop = 16f;
    private static float p2;
    private SpriteRenderer sr;
    public bool loop1;
    public bool startOnScroll;
    public bool canSprint = false;
    public bool playOnScrollOnlyOnce = false;
    private bool playedOnScroll = false;
    public Vector2 whichScrollRegion;

    private void Start()
    {
        e = GetComponent<Encontrolmentation>();
        bm = GetComponent<BasicMove>();
        amPlaying = false;
        if (startPlayingAutomatically)
        {
            StartPlaying();
        }
        p2 = positionToleranceForPop * positionToleranceForPop;
        sr = GetComponent<SpriteRenderer>();
    }

    public void StartPlaying()
    {
        amPlaying = true;
        myStartTime = DoubleTime.ScaledTimeSinceLoad;
        currEvent = 1;
        currCheckpoint = 0;
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (startOnScroll)
        {
            bool s = whichScrollRegion == (FollowThePlayer.main?.perScreenPosition ?? Vector2.negativeInfinity);
            if (s && !amPlaying)
            {
                bool gone = false;
                if (playOnScrollOnlyOnce)
                {
                    if (!playedOnScroll) { playedOnScroll = true; }
                    else { s = false; transform.position = new Vector3(100000, 100000); gone = true; }
                }
                GetComponent<BasicMove>().enabled = true;
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                if (!gone) { StartPlaying(); }
            }
            if (!s && amPlaying)
            {
                GetComponent<BasicMove>().enabled = false;
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                amPlaying = false;
            }
        }

        if (copyRegularMovements && sr && sr.isVisible)
        {
            bool endUpdate = true;
            switch (copyType)
            {
                case CopyType.Reverse:
                    e.fakeInput = (((copyRegularMovements.currentState & 1UL) == 1UL) ? 2UL : 0UL)
                                 + (((copyRegularMovements.currentState & 2UL) == 2UL) ? 1UL : 0UL)
                                    + (copyRegularMovements.currentState & 16UL);
                    break;
                case CopyType.BlockOnRight:
                    bool playerIsLeft = copyRegularMovements.transform.position.x <= transform.position.x + 2;
                    if (playerIsLeft) { e.fakeInput = (copyRegularMovements.currentState & 16UL); }
                    sr.flipX = playerIsLeft;
                    break;
                case CopyType.JustFacePlayer:
                    sr.flipX = copyRegularMovements.transform.position.x <= transform.position.x;
                    endUpdate = false;
                    break;
            }
            if (endUpdate) { return; }
        }

        if (amPlaying)
        {
            while (currEvent < fakeEventsTable.Count && DoubleTime.ScaledTimeSinceLoad - myStartTime + 0.0001 >= fakeEventsTable[currEvent].Item1)
            {
                ++currEvent;
            }

            while (currCheckpoint < checkpoints.Count && Encontrolmentation.recCheckInterval*currCheckpoint <= DoubleTime.ScaledTimeSinceLoad - myStartTime)
            {
                if (((Vector2)transform.position - checkpoints[currCheckpoint].position).sqrMagnitude > p2)
                {
                    if (popEffect)
                    {
                        GameObject pop = Instantiate(popEffect, transform.position, Quaternion.identity, transform.parent);
                    }
                    transform.position = new Vector3(checkpoints[currCheckpoint].position.x, checkpoints[currCheckpoint].position.y, transform.position.z);
                    bm.fakePhysicsVel = checkpoints[currCheckpoint].velocity;
                    GetComponent<Rigidbody2D>().velocity = bm.fakePhysicsVel;
                }
                ++currCheckpoint;
            }

            if (currEvent < fakeEventsTable.Count)
            {
                e.fakeInput = fakeEventsTable[currEvent - 1].Item2;
            }
            else
            {
                e.fakeInput = fakeEventsTable[fakeEventsTable.Count - 1].Item2;
                if (loop1)
                {
                    myStartTime = DoubleTime.ScaledTimeSinceLoad;
                    currEvent = 1;
                    currCheckpoint = 0;
                }
            }

        }
    }
}
