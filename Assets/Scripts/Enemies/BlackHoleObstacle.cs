using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleObstacle : GenericBlowMeUp
{
    public enum State
    {
        Idle, ToActive, Active, ToIdle
    }

    [HideInInspector]
    public State state = State.Idle;

    public SkinnedMeshRenderer largeArms;
    private Prim3DRotate largeArmsRot;
    public MeshRenderer smallArms;
    private Prim3DRotate smallArmsRot;
    public MeshRenderer aura;
    public SkinnedMeshRenderer face;
    public BulletHellMakerFunctions shooter;
    public float pullStrength = 1f;
    public float shootStrength = 0.05f;

    private const float largeArmsSpeedIdle = -1f;
    private const float largeArmsSpeedActive = -5f;
    private const float smallArmsSpeedIdle = 1f;
    private const float smallArmsSpeedActive = 5f;

    private double stateStartTime = 0f;
    private float intensity = 0f;

    public float durationIdle = 3f;
    public float durationToActive = 3f;
    public float durationActive = 3f;
    public float durationToIdle = 3f;

    private Encontrolmentation lastEncmt;
    private BasicMove bm;

    private void Start()
    {
        state = State.Idle;
        aura.material.SetFloat("_FakeAlpha", 0f);
        largeArmsRot = largeArms.GetComponent<Prim3DRotate>();
        smallArmsRot = smallArms.GetComponent<Prim3DRotate>();
        stateStartTime = DoubleTime.ScaledTimeSinceLoad;
    }

    private bool StageOver(float dur)
    {
        return (DoubleTime.ScaledTimeSinceLoad - stateStartTime >= dur);
    }

    private void NextStageCheck(float dur)
    {
        if (StageOver(dur))
        {
            stateStartTime = DoubleTime.ScaledTimeSinceLoad;
            switch (state)
            {
                case State.Idle: state = State.ToActive; break;
                case State.ToActive: state = State.Active; break;
                case State.Active: state = State.ToIdle; break;
                case State.ToIdle: state = State.Idle; break;
            }
        }
    }

    private float StageProgress(float dur)
    {
        return Mathf.Clamp01((float)(DoubleTime.ScaledTimeSinceLoad - stateStartTime) / dur);
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        float prog;
        switch (state)
        {
            case State.Idle:
                intensity = 0f;

                NextStageCheck(durationIdle);
                break;
            case State.ToActive:
                prog = StageProgress(durationToActive);
                intensity = prog;

                NextStageCheck(durationToActive);
                break;
            case State.Active:
                intensity = 1f;

                NextStageCheck(durationActive);
                break;
            case State.ToIdle:
                prog = StageProgress(durationToIdle);
                intensity = 1f - prog;

                NextStageCheck(durationToIdle);
                break;
        }

        // visual updates
        if (!largeArms.isVisible) { return; }

        Color tint = aura.material.GetColor("_Tint");
        aura.material.SetColor("_Tint", new Color(tint.r, tint.g, tint.b, intensity));
        largeArmsRot.speed = Mathf.Lerp(largeArmsSpeedIdle, largeArmsSpeedActive, intensity);
        smallArmsRot.speed = Mathf.Lerp(smallArmsSpeedIdle, smallArmsSpeedActive, intensity);
        face.SetBlendShapeWeight(0, Mathf.Clamp01(intensity * 2f) * 100f);
        if (Fakerand.Single() < intensity * shootStrength)
        {
            shooter.Fire();
        }

        Encontrolmentation e = LevelInfoContainer.GetActiveControl();
        if (e == null) { return; }
        if (lastEncmt != e)
        {
            bm = e.GetComponent<BasicMove>();
            lastEncmt = e;
        }
        if (bm == null) { return; }

        Vector2 succVec = transform.position - e.transform.position;

        if (intensity >= 0.1f)
        {
            if (succVec.sqrMagnitude < 12f)
            {
                bm.transform.position = transform.position;
                KHealth kh = bm.GetComponent<KHealth>();
                if (kh)
                {
                    kh.ChangeHealth(-0.1f, "black hole");
                }
            }
            else
            {
                succVec = succVec.normalized * 20000f * intensity * pullStrength / succVec.sqrMagnitude;
                succVec = succVec.normalized * Mathf.Clamp(succVec.magnitude, 5f, 100f);

                if (bm.grounded == 0) // not grounded
                {
                    bm.fakePhysicsVel += succVec;
                }
                else
                {
                    bm.fakePhysicsVel += new Vector2(succVec.x, 0);
                }
            }
        }

        SuperRayMagnet.AttractAllRays(transform.position, 200f, -150000f);
        
    }
}
