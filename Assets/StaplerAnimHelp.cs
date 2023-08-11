using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaplerAnimHelp : MonoBehaviour
{
    public BulletHellMakerFunctions bulletPatternInfo;
    public Transform topPart;
    public Transform wholeMesh;

    private AudioSource aso;
    private float lastP;

    void Start()
    {
        aso = GetComponent<AudioSource>();
        lastP = 0f;
    }

    void Update()
    {
        wholeMesh.localEulerAngles = new Vector3(180f + (float)(30.0 * System.Math.Sin(3.14159265358979 * DoubleTime.ScaledTimeSinceLoad)), 0, 0);

        float p = (float)(DoubleTime.ScaledTimeSinceLoad - bulletPatternInfo.lastShotTime) / bulletPatternInfo.waitTime;

        if (p < 0.9f)
        {
            topPart.localEulerAngles = new Vector3(0, -60f * p, 0);
        }
        else
        {
            topPart.localEulerAngles = new Vector3(0, -600f * (1f-p), 0);
        }

        if (lastP > p)
        {
            aso.Stop();
            aso.pitch = Fakerand.Single(0.9f, 1.1f);
            aso.Play();
        }

        lastP = p;
        
    }
}
