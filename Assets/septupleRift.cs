using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class septupleRift : MonoBehaviour
{
    public SkinnedMeshRenderer mesh1;
    public SkinnedMeshRenderer mesh2;

    private double madeTime;
    private CircleCollider2D cc2;

    private const double mainSequenceTime = 3.6;
    private const double endAnimationTime = 0.4;

    private const double totalTime = 4.4;

    private const float scaleMin = 1f;
    private const float scaleMax = 1f;

    private static List<KHealth> plrsTrack;

    void Start()
    {
        float scaleThis = Fakerand.Single(scaleMin, scaleMax);
        transform.localScale = new Vector3(scaleThis, scaleThis, scaleThis);
        madeTime = DoubleTime.ScaledTimeSinceLoad;
        cc2 = GetComponent<CircleCollider2D>();
        mesh1.SetBlendShapeWeight(0, 100f);
        mesh1.SetBlendShapeWeight(1, 100f);
        mesh1.SetBlendShapeWeight(2, 100f);
        mesh2.SetBlendShapeWeight(0, 100f);
        mesh2.SetBlendShapeWeight(1, 100f);
        mesh2.SetBlendShapeWeight(2, 100f);
        plrsTrack = new List<KHealth>();
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        KHealth kh = col.GetComponent<KHealth>();
        BasicMove bm = col.GetComponent<BasicMove>();
        if (kh && bm && !plrsTrack.Contains(kh))
        {
            if (kh.electrocute > 0.9f)
            {
                kh.electrocute -= 0.85f;
                kh.ChangeHealth(-bm.Damage, "septuple rift");
            }
            if (kh.electrocute < 0.05f)
            {
                kh.electrocute = 0.05f;
            }
            kh.electrocute += 0.02f;
            plrsTrack.Add(kh);
        }
    }

    void F1(double x)
    {
        float buzz = 0.03f * (float)System.Math.Cos(DoubleTime.ScaledTimeSinceLoad * 3.141592653589793238);
        mesh1.material.SetFloat("_Fizz", buzz);
        mesh2.material.SetFloat("_Fizz", buzz);
        float tween = Mathf.Sin((float)x * 1.57079633f);
        float tweenM = 1f - tween;
        float[] ab = new float[3] { Mathf.Clamp01(tweenM * 3f) * 100f, Mathf.Clamp01(tweenM * 3f - 1f) * 100f, Mathf.Clamp01(tweenM * 3f - 2f) * 100f };
        cc2.radius = 32f * tween;
        mesh1.SetBlendShapeWeight(0, ab[0]);
        mesh1.SetBlendShapeWeight(1, ab[1]);
        mesh1.SetBlendShapeWeight(2, ab[2]);
        mesh2.SetBlendShapeWeight(0, ab[0]);
        mesh2.SetBlendShapeWeight(1, ab[1]);
        mesh2.SetBlendShapeWeight(2, ab[2]);
    }

    void Update()
    {
        double elapsed = DoubleTime.ScaledTimeSinceLoad - madeTime;
        if (elapsed < endAnimationTime)
        {
            F1(elapsed / endAnimationTime);
        }
        else if (elapsed <= mainSequenceTime + endAnimationTime)
        {
            F1(1f);
        }
        else if (elapsed < totalTime)
        {
            F1( (totalTime-elapsed) / endAnimationTime);
        }
        else
        {
            Destroy(gameObject);
        }
        plrsTrack.Clear();
    }
}
