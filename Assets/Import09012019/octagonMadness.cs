using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class octagonMadness : MonoBehaviour {

    public Vector2 rangeSize;
    public AnimationCurve xDistribution;
    public AnimationCurve yDistribution;
    public bool moveToRandomAtBeginning;
    public float[] mvtIntervals;
    public Vector2 bobIntensity;
    public Vector2 bobSpeed;
    public float mvtSpeed;
    private int mvi = 0;
    private float t = 0;
    [Header("Don't change the stuff below")]
    public Vector4 lspaceBounds;
    public Vector2 oldPos;
    public Vector2 targPos;
    public Vector2 bobRandOffset;
    public Vector2 sdvel;

    Vector2 NewPositionGet()
    {
        float nx = Mathf.Lerp(lspaceBounds.x,lspaceBounds.z,xDistribution.Evaluate(Fakerand.Single()));
        float ny = Mathf.Lerp(lspaceBounds.y, lspaceBounds.w, yDistribution.Evaluate(Fakerand.Single()));
        return new Vector2(nx, ny);
    }

    Vector2 NewBobGet()
    {
        float dx = bobIntensity.x * (float)System.Math.Cos((DoubleTime.ScaledTimeSinceLoad * bobSpeed.x) + bobRandOffset.x);
        float dy = bobIntensity.y * (float)System.Math.Sin((DoubleTime.ScaledTimeSinceLoad * bobSpeed.y) + bobRandOffset.y);
        return new Vector2(dx, dy);
    }

	void Start () {
        mvi = 0;
        t = 0f;
        Vector2 rh = rangeSize * 0.5f;
        Vector2 tp = transform.localPosition;
        lspaceBounds = new Vector4(tp.x - rh.x, tp.y - rh.y, tp.x + rh.x, tp.y + rh.y);
        if (moveToRandomAtBeginning)
        {
            oldPos = tp;
            transform.localPosition = targPos = NewPositionGet();
        }
        else
        {
            oldPos = targPos = tp;
        }
        t += mvtIntervals[0];
        bobRandOffset = new Vector2(Fakerand.Single(0f, 6.2831853f), Fakerand.Single(0f, 6.2831853f));
	}
	
	void Update () {

        if (DoubleTime.ScaledTimeSinceLoad-t > mvtIntervals[mvi])
        {
            mvi = (mvi + 1) % mvtIntervals.Length;
            t += mvtIntervals[mvi];
            oldPos = targPos;
            targPos = NewPositionGet();
        }

        Rigidbody2D rg2 = GetComponent<Rigidbody2D>();
        rg2.MovePosition(Vector2.SmoothDamp(transform.localPosition, targPos, ref sdvel, 0.3f, mvtSpeed * Time.timeScale) + NewBobGet());
        Debug.DrawLine(new Vector3(lspaceBounds.x, lspaceBounds.y), new Vector3(lspaceBounds.z, lspaceBounds.w), Color.red, 0.0166666f);
        Debug.DrawLine(new Vector3(lspaceBounds.x, lspaceBounds.w), new Vector3(lspaceBounds.z, lspaceBounds.y), Color.red, 0.0166666f);
    }
}
