using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThetaPlatform : MonoBehaviour
{

    public static BGMController audioControl;
    public TextMesh myText;
    public string[] textChanges;
    public AudioSource[] audioChanges;
    public float audioBPM;
    public float rotationChangePerBeat;

    private float currAngle;
    private float nextAngle;

    private int i;

    private AudioSource audioSource;
    private Rigidbody2D r2;
    private SpriteRenderer sr;
    private float beatTime;
    private float lastTimeInBeat;

    private double lt2;

    private static AudioSource lastAudio = null;

    void CalculateNextAngle()
    {
        nextAngle = currAngle + rotationChangePerBeat;
        float s = Mathf.Sign(rotationChangePerBeat); //correct nextAngle for interpolation
        if (s * nextAngle < s * currAngle)
        {
            nextAngle += s * 360f;
        }
    }

    void Start()
    {
        beatTime = (60f / audioBPM);
        audioControl = audioControl ?? BGMController.main;
        audioSource = audioControl.GetComponent<AudioSource>();
        r2 = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        lastTimeInBeat = 0f;
        lt2 = DoubleTime.UnscaledTimeRunning;
        currAngle = transform.localEulerAngles.z;
        CalculateNextAngle();
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        audioControl = audioControl ?? BGMController.main;
        if (!audioControl) { return; }

        float timeInBeat = (audioSource.time % beatTime) / beatTime; // [0,1)

        if (timeInBeat < lastTimeInBeat && DoubleTime.UnscaledTimeRunning-lt2 > 6f*Time.unscaledDeltaTime)
        {
            currAngle = nextAngle;
            CalculateNextAngle();

            i++;
            if (i >= textChanges.Length)
            {
                i = 0;
            }

            myText.text = textChanges[i];
            if (i < audioChanges.Length && audioChanges[i] && sr.isVisible)
            {
                if (lastAudio) { lastAudio.Stop(); }
                audioChanges[i].Play();
                lastAudio = audioChanges[i];
            }

            lt2 = DoubleTime.UnscaledTimeRunning;
        }

        Vector3 oldLEA = transform.localEulerAngles;
        myText.transform.rotation = Quaternion.identity;
        float s1 = Mathf.Lerp(currAngle, nextAngle, timeInBeat);
        transform.localEulerAngles = new Vector3(oldLEA.x, oldLEA.y, s1);
        s1 = Mathf.Repeat(s1, 360f);
        if (s1 > 90 && s1 <= 270)
        {
            sr.flipY = true;
        }
        else
        {
            sr.flipY = false;
        }
        r2.angularVelocity = (nextAngle - currAngle) * audioBPM * 0.01666666f;
        //r2.MoveRotation(transform.eulerAngles.z);
        //r2.MovePosition(transform.position);

        lastTimeInBeat = timeInBeat;

        if (Time.timeScale == 0 && audioControl.pauseMusicDuringPauseMenu)
        {
            lt2 += Time.unscaledDeltaTime;
        }

    }
}
