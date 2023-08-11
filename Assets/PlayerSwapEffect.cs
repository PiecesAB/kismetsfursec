using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwapEffect : MonoBehaviour
{
    public GameObject spiritSample;
    public Transform target;
    public AudioClip[] sounds;

    private float[] times;
    private float[] archFactors;
    private Vector2 origPos;
    private Transform[] spirits;

    private double startTime;

    void Start()
    {
        startTime = DoubleTime.UnscaledTimeSinceLoad;

        times = new float[3];
        archFactors = new float[3];
        origPos = transform.position;
        spirits = new Transform[3];

        for (int i = 0; i < 3; ++i)
        {
            times[i] = Fakerand.Single(0.2f, 0.4f);
            archFactors[i] = Fakerand.Single(-0.5f, 0.5f);
            GameObject sg = Instantiate(spiritSample, transform.position, Quaternion.identity, transform);
            sg.SetActive(true);
            spirits[i] = sg.transform;
        }

        transform.position = target.position;

        AudioSource soundPlayer = GetComponent<AudioSource>();
        soundPlayer.clip = sounds[Fakerand.Int(0, sounds.Length)];
        soundPlayer.pitch = Fakerand.Single(1.5f, 2.5f);
        soundPlayer.Play();
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        transform.position = target.position;
        double t = DoubleTime.UnscaledTimeSinceLoad - startTime;
        if (t >= 0.5f)
        {
            Destroy(gameObject, 0.2f);
        }
        for (int i = 0; i < 3; ++i)
        {
            if (t >= times[i])
            {
                if (spirits[i])
                {
                    Destroy(spirits[i].gameObject, 0.2f);
                    spirits[i].SetParent(target, true);
                    spirits[i] = null;
                }
                continue;
            }
            float rat = (float)(t / times[i]);
            rat = EasingOfAccess.CubicIn(rat);
            Vector2 targPos = target.position;
            Vector2 lp = Vector2.Lerp(origPos, targPos, rat);
            Vector2 mid = 0.5f * (origPos + targPos);
            Vector2 norm = Vector2.Perpendicular(targPos - origPos).normalized;
            Vector2 sp = (Vector2)Vector3.Slerp(origPos - mid + norm, targPos - mid + norm, rat) + mid - norm;
            spirits[i].position = Vector3.LerpUnclamped(lp, sp, archFactors[i]);
        }
    }
}
