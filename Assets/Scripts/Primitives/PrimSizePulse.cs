using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimSizePulse : MonoBehaviour
{
    public enum PulseType
    {
        Normal, Sine, Intermittent
    }

    [System.Serializable]
    public class IntermittentData
    {
        public float onTime = 1f;
        public float offTime = 1f;
        public Vector3 onSize = Vector3.one;
        public Vector3 offSize = Vector3.one;
        public float turnOnTime = 1f;
        public float turnOffTime = 1f;
        public bool startOn = false;
        public EasingOfAccess.EasingType easingInType = EasingOfAccess.EasingType.Linear;
        public EasingOfAccess.EasingType easingOutType = EasingOfAccess.EasingType.Linear;
        public AudioClip turnOnSound;
        public AudioClip turnOffSound;
    }

    private double t = 0.0;
    private float pulso = 1f;
    public double pulseTime = 0.5f;
    public float pulseSize = 1.5f;
    public float meanRandSize = 1f;
    public float sdRandSize = 0f;
    public float startDelay = 0f;
    public IntermittentData intermittentData;
    public PulseType pulseType = PulseType.Normal;
    public Renderer onScreenChecker;

    private AudioSource sound = null;


    private Vector3 disappearedPosition = Vector3.positiveInfinity;

    private IEnumerator Intermittent()
    {
        bool on = intermittentData.startOn;

        transform.localScale = on ? intermittentData.onSize : intermittentData.offSize;

        if (startDelay > 0f) { yield return new WaitForSeconds(startDelay); }

        while (this)
        {
            double timeA;

            if (on)
            {
                if (intermittentData.onTime > 0f) {
                    yield return new WaitForSeconds(intermittentData.onTime);
                }

                timeA = DoubleTime.ScaledTimeSinceLoad;

                if (sound)
                {
                    sound.clip = intermittentData.turnOffSound;
                    sound.Stop(); sound.Play();
                }

                while (DoubleTime.ScaledTimeSinceLoad - timeA < intermittentData.turnOffTime)
                {
                    float rat = (float)((DoubleTime.ScaledTimeSinceLoad - timeA) / intermittentData.turnOffTime);
                    rat = EasingOfAccess.Evaluate(intermittentData.easingOutType, rat);
                    transform.localScale = Vector3.Lerp(intermittentData.onSize, intermittentData.offSize, rat);
                    yield return new WaitForEndOfFrame();
                }

            }
            else
            {
                if (intermittentData.offTime > 0f) {
                    yield return new WaitForSeconds(intermittentData.offTime);
                }

                timeA = DoubleTime.ScaledTimeSinceLoad;

                if (sound)
                {
                    sound.clip = intermittentData.turnOnSound;
                    sound.Stop(); sound.Play();
                }

                while (DoubleTime.ScaledTimeSinceLoad - timeA < intermittentData.turnOnTime)
                {
                    float rat = (float)((DoubleTime.ScaledTimeSinceLoad - timeA) / intermittentData.turnOnTime);
                    rat = EasingOfAccess.Evaluate(intermittentData.easingInType, rat);
                    transform.localScale = Vector3.Lerp(intermittentData.offSize, intermittentData.onSize, rat);
                    yield return new WaitForEndOfFrame();
                }
            }

            on = !on;
            transform.localScale = on ? intermittentData.onSize : intermittentData.offSize;

            yield return null;
        }

        yield break;
    }

    void Start()
    {
        switch (pulseType)
        {
            case PulseType.Sine:
                break;
            case PulseType.Normal:
                t = DoubleTime.ScaledTimeSinceLoad + pulseTime;
                break;
            case PulseType.Intermittent:
                sound = GetComponent<AudioSource>();
                StartCoroutine(Intermittent());
                break;
        }
        
    }

    void Update()
    {
        if (onScreenChecker && !onScreenChecker.isVisible) { return; }

        switch (pulseType)
        {
            case PulseType.Sine:
                float c = meanRandSize + sdRandSize * (float)System.Math.Sin(pulseTime * (DoubleTime.ScaledTimeSinceLoad + pulseSize));
                transform.localScale = Vector3.one*c;
                break;
            case PulseType.Normal:
                while (t <= DoubleTime.ScaledTimeSinceLoad)
                {
                    t += pulseTime;
                    transform.localScale = pulseSize * Vector3.one;
                    pulso = 0f;
                }
                float newSize = Fakerand.NormalDist(meanRandSize, sdRandSize, 0f);
                transform.localScale = Mathf.Lerp(transform.localScale.x, newSize, pulso) * Vector3.one;
                if (pulso < 1f)
                {
                    pulso += 0.05f;
                    if (pulso > 1f)
                    {
                        pulso = 1f;
                    }
                }
                break;
        }

        
    }
}
