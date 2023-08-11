using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StampObstacle : GenericBlowMeUp
{
    public enum State
    {
        Drop,Placed,Rise,Float
    }

    public float dropTime;
    public float placedTime;
    public float riseTime;
    public float floatTime;

    private double lastChangeTime;

    public State state = State.Drop;
    public GameObject placedCollider;
    public Transform meshTransform;
    public Renderer visibilityRenderer;
    public GameObject[] stampPatterns;
    private int stampPatternIndex = 0;

    public Vector3[] translationPerStamp;
    private int translationIndex = 0;
    private Vector3 currPos;
    private Vector3 nextPos;

    private AudioSource aud;
    public AudioClip press;
    public AudioClip lift;

    private int rand1;
    private int rand2;

    private void Start()
    {
        currPos = transform.position;
        aud = GetComponent<AudioSource>();
        nextPos = (translationPerStamp.Length == 0) ? (currPos) : (currPos + translationPerStamp[0]); 
        lastChangeTime = DoubleTime.ScaledTimeSinceLoad;
        placedCollider.SetActive(state == State.Placed);
        StateUpdate();
    }

    private void Stamp()
    {
        if (stampPatterns.Length == 0 || !visibilityRenderer.isVisible) { return; }
        GameObject stampPattern = stampPatterns[stampPatternIndex];
        if (stampPattern == null) { return; }
        GameObject newPattern = Instantiate(stampPattern, new Vector3(transform.position.x, transform.position.y, stampPattern.transform.position.z), transform.rotation);
        newPattern.SetActive(true);
        stampPatternIndex = (stampPatternIndex + 1) % stampPatterns.Length;
    }

    private void AdvancePosition()
    {
        if (translationPerStamp.Length == 0) { return; }
        currPos = nextPos;
        translationIndex = (translationIndex + 1) % translationPerStamp.Length;
        nextPos += translationPerStamp[translationIndex];
    }

    private void MakeSound(AudioClip clip)
    {
        aud.Stop();
        if (!visibilityRenderer.isVisible) { return; }
        aud.clip = clip;
        aud.Play();
    }

    private void AdvanceState()
    {
        switch (state)
        {
            case State.Drop:
                lastChangeTime += dropTime;
                meshTransform.localEulerAngles = Vector3.zero;
                meshTransform.localPosition = Vector3.zero;
                meshTransform.localScale = Vector3.one;
                placedCollider.SetActive(true);
                MakeSound(press);
                state = State.Placed;
                break;
            case State.Placed:
                lastChangeTime += placedTime;
                rand1 = Fakerand.Int(0, 2) * 2 - 1;
                rand2 = Fakerand.Int(0, 2) * 2 - 1;
                placedCollider.SetActive(false);
                state = State.Rise;
                MakeSound(lift);
                Stamp();
                break;
            case State.Rise:
                lastChangeTime += riseTime;
                meshTransform.localEulerAngles = new Vector3(rand1 * 30f, rand2 * 30f, 0f);
                meshTransform.localPosition = new Vector3(0, 0, -150) - meshTransform.forward * 30f;
                meshTransform.localScale = 0.5f*Vector3.one;
                state = State.Float;
                break;
            case State.Float:
                lastChangeTime += floatTime;
                transform.position = nextPos;
                AdvancePosition();
                state = State.Drop;
                break;
        }
    }

    private float GetProgress()
    {
        float elapse = (float)(DoubleTime.ScaledTimeSinceLoad - lastChangeTime);
        switch (state)
        {
            case State.Drop: return elapse / dropTime;
            case State.Placed: return elapse / placedTime;
            case State.Rise: return elapse / riseTime;
            case State.Float: return elapse / floatTime;
        }
        return 0;
    }

    private void StateUpdate()
    {
        if (Time.timeScale == 0) { return; }
        float progress = GetProgress();
        float antiProgress = 1f - progress;
        if (progress >= 1f)
        {
            AdvanceState();
            return;
        }
        if (!visibilityRenderer.isVisible) { return; }
        switch (state)
        {
            case State.Drop:
                meshTransform.localEulerAngles = new Vector3(antiProgress * rand1 * 30f, antiProgress * rand2 * 30f, 0f);
                meshTransform.localPosition = new Vector3(0, 0, antiProgress * -150) - meshTransform.forward * 30f * antiProgress;
                meshTransform.localScale = (0.5f + 0.5f * progress) * Vector3.one;
                break;
            case State.Placed:
                break;
            case State.Rise:
                meshTransform.localEulerAngles = new Vector3(progress * rand1 * 30f, progress * rand2 * 30f, 0f);
                meshTransform.localPosition = new Vector3(0, 0, progress * -150) - meshTransform.forward * 30f * progress;
                meshTransform.localScale = (0.5f + 0.5f * antiProgress) * Vector3.one;
                break;
            case State.Float:
                transform.position = Vector3.Lerp(currPos, nextPos, progress);
                break;
        }
    }

    private void Update()
    {
        StateUpdate();
    }
}
