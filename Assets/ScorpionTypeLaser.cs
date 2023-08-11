using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorpionTypeLaser : MonoBehaviour {




    [Header ("[off] [on]")]
    public Vector2[] timesTable;
    public bool startsOn;
    public Animator anim;
    public LineRenderer laser;
    public Vector2 lineEnds;
    public BoxCollider2D groundCollider;
    public BoxCollider2D laserCollider;
    public float animTime = 0.3833333333f;
    public int state;
    public int timeiter;
    public float locallaserZ;
    public float laserVariance;
    public double compareTime;
    public AudioSource slideInSound;
    public AudioSource slideOutSound;
    public AudioSource switchOnSound;
    public AudioSource switchOffSound;

    void Start () {
        timeiter = 0;
        if (startsOn)
        {
            state = 2;
            compareTime = timesTable[0].y;
        }
        else
        {
            state = 0;
            compareTime = timesTable[0].x;
        }
	}

	void Update () {

        laser.positionCount = 1;
        laser.enabled = false;
        laserCollider.enabled = groundCollider.enabled = false;
        if (state == 0)
        {
            anim.SetFloat("Time", 0f);
            slideInSound.Stop();
            slideOutSound.Stop();
            switchOnSound.Stop();
            switchOffSound.Stop();
            if (compareTime < DoubleTime.ScaledTimeSinceLoad)
            {
                compareTime += animTime;
                state = 1;
                slideInSound.pitch = slideInSound.clip.length / animTime;
                slideInSound.Play();
            }
        }
        if (state == 1)
        {
            anim.SetFloat("Time", Mathf.Clamp01(1f - ((float)(compareTime - DoubleTime.ScaledTimeSinceLoad) / animTime)));
            slideOutSound.Stop();
            switchOnSound.Stop();
            switchOffSound.Stop();
            if (compareTime < DoubleTime.ScaledTimeSinceLoad)
            {
                compareTime += timesTable[timeiter].y;
                state = 2;
                switchOnSound.Play();
            }
        }
        if (state==2)
        {
            slideInSound.Stop();
            slideOutSound.Stop();
            switchOffSound.Stop();
            anim.SetFloat("Time", 1f);
            laserCollider.enabled = groundCollider.enabled = true;
            laser.enabled = true;
            laser.positionCount = 6;
            Vector3[] laserposs = new Vector3[6];
            laserposs[0] = new Vector3(0f, lineEnds.x, locallaserZ);
            for (int i = 1; i < 5; i++)
            {
                laserposs[i] = new Vector3(Fakerand.Single(-laserVariance,laserVariance), Mathf.Lerp(lineEnds.x,lineEnds.y,i/5f), locallaserZ);
            }
            laserposs[5] = new Vector3(0f, lineEnds.y, locallaserZ);
            laser.SetPositions(laserposs);

            if (compareTime < DoubleTime.ScaledTimeSinceLoad)
            {
                compareTime += animTime;
                state = 3;
                switchOffSound.Play();
                slideOutSound.Play();
            }
        }
        if (state == 3)
        {
            slideInSound.Stop();
            switchOnSound.Stop();
            anim.SetFloat("Time", Mathf.Clamp01((float)(compareTime - DoubleTime.ScaledTimeSinceLoad) / animTime));
            if (compareTime < DoubleTime.ScaledTimeSinceLoad)
            {
                timeiter = (timeiter + 1) % timesTable.Length;
                compareTime += timesTable[timeiter].x;
                state = 0;
            }
        }

    }
}
