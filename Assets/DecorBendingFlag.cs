using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorBendingFlag : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer flagShape;
    [SerializeField]
    private Transform armatureTransform;
    [SerializeField]
    private Transform curveBone;
    private double t;
    private float lastZ;
    private float r1;
    public Transform plrThatRotates;

    void Start()
    {
        t = 0.0;
        lastZ = 0;
        r1 = Fakerand.Single() * Mathf.PI * 2f;
    }
    void Update()
    {
        if (Time.timeScale == 0 || !flagShape.isVisible) { return; }

        float gx = Mathf.Clamp(Physics2D.gravity.x, -12f, 12f);
        if (plrThatRotates) { gx *= Mathf.Cos(plrThatRotates.eulerAngles.z * Mathf.Deg2Rad); }

        float z = Mathf.LerpAngle(lastZ, (gx > 0f) ? 180 : 0, Mathf.Clamp(Mathf.Abs(gx) * 0.4f, 0f, 0.3f));
        armatureTransform.localEulerAngles = new Vector3(-90, 0, z);
        lastZ = z;

        float flagWindFactor = 0.5f + Mathf.Abs(gx) * 0.6f;

        flagShape.SetBlendShapeWeight(0, 50f*(Mathf.Sin((float)(t % 1.0) * Mathf.PI * 2f + r1)+1f));
        t += Time.deltaTime*flagWindFactor;

        curveBone.localEulerAngles = new Vector3(90, 0, -90 + (6f * Mathf.Abs(gx)) + 5f * Mathf.Abs(gx) * Mathf.PerlinNoise(r1*800f, (float)((2.0f*DoubleTime.ScaledTimeSinceLoad) % 1000000.0)))  ;
    }
}
