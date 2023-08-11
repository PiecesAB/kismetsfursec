using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class papercut : GenericBlowMeUp
{

    public SkinnedMeshRenderer meshRend;

    public bool mainScript;

    private const float amplitude = 64f;
    private const float frequency = 4.71238898f;
    private const float downwardsSpeed = 12f;
    private const float lift1 = 4f;
    private const float plrDamageMult = 1f;
    private const float destroyTime = 90f;

    private double timeOfRelease;
    private bool released = false;
    private float currc1 = 0f;

    Vector3 origLPos;

    public void Release()
    {
        timeOfRelease = DoubleTime.ScaledTimeSinceLoad;
        released = true;
    }

    void Start()
    {
        origLPos = transform.localPosition;
        released = false;

        Release(); //test
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (!mainScript)
        {
            KHealth kh = c.GetComponent<KHealth>();
            BasicMove bm = c.GetComponent<BasicMove>();
            if (c.GetType() == typeof(BoxCollider2D) && kh && bm)
            {
                float totalDmg = bm.Damage * plrDamageMult * System.Math.Abs(currc1);
                if (totalDmg > 1f)
                {
                    kh.ChangeHealth(-totalDmg, "paper");
                }
            }
        }
    }

    void Update()
    {
        double pTime = DoubleTime.ScaledTimeSinceLoad - timeOfRelease;
        if (mainScript)
        {
            if (released)
            {
                double s1 = System.Math.Sin(frequency * pTime);
                double u1 = lift1 * System.Math.Abs(s1);
                transform.localPosition = origLPos + new Vector3((float)(amplitude * s1), (float)((-downwardsSpeed * pTime) + (u1 * u1)));

                meshRend.SetBlendShapeWeight(0, (float)((1f + s1) * 50f));
                meshRend.SetBlendShapeWeight(1, (float)((1f - s1) * 50f));
                meshRend.SetBlendShapeWeight(2, 0f);

                if (pTime >= 15f && !meshRend.isVisible)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                meshRend.SetBlendShapeWeight(0, 0f);
                meshRend.SetBlendShapeWeight(1, 0f);
                meshRend.SetBlendShapeWeight(2, 100f);
            }
        }
        else
        {
            currc1 = released ? (float)System.Math.Cos(frequency * pTime) : 0f;
        }
    }
}
