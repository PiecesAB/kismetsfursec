using UnityEngine;
using System.Collections;

public class circleSpinner : MonoBehaviour {

    public Color tint;
    public Color circleColorTopRight;
    public Color circleColorBottomLeft;
    public float thiccness;
    public float thiccPulseSpeed;
    public float thiccPulseAmount;
    public float speed;
    public int repeatedness;
    public int resolution;
    public float holeOffset;
    public float holeSize;
    public float damage;
    public int invinicbleFrames;
    public float inverted = -1;

    private int eCount = 0;
    private bool open;

    // Use this for initialization
    void Start () {
        Material m = GetComponent<MeshRenderer>().material;
        m.SetColor("_Color", tint);
        m.SetColor("_C1", circleColorTopRight);
        m.SetColor("_C2", circleColorBottomLeft);
        m.SetFloat("_Thicc", thiccness);
        m.SetFloat("_TS", thiccPulseSpeed);
        m.SetFloat("_TA", thiccPulseAmount);
        m.SetFloat("_Speed", speed);
        m.SetInt("_Repeatedness", repeatedness);
        m.SetInt("_Res", resolution);
        m.SetFloat("_HoleO", holeOffset);
        m.SetFloat("_HoleS", holeSize);

        eCount = 0;
        open = true;

    }
	
    void OnTriggerStay2D(Collider2D c)
    {
        
        if (c.gameObject.GetComponent<KHealth>() && eCount == 0 && open)
        {
            open = false;
            KHealth k = c.gameObject.GetComponent<KHealth>();
            double t = DoubleTime.ScaledTimeSinceLoad * 0.05f;
            float angle = Mathf.Atan2(inverted*(transform.position.y - c.transform.position.y), c.transform.position.x - transform.position.x)-(Mathf.PI/2f);
            angle = (angle * repeatedness) % (2f * Mathf.PI);
            if (angle < 0f)
            {
                angle += 2f * Mathf.PI;
            }
            float size = transform.localScale.x * 5f;
            float killRangeLower = (1f-(thiccness + thiccPulseAmount * (float)System.Math.Sin(thiccPulseSpeed * t * 2f * Mathf.PI)))*size;
            float dist = Fastmath.FastV2Dist(c.transform.position, transform.position);
            if (killRangeLower<dist && dist<size && size-killRangeLower>0)
            {
                /*half start = ((_Time.z * _Speed) + _HoleO) % tau;
                start = lerp(start + tau, start, step(0.0, start));
                half end = (start + _HoleS) % tau;
                end = lerp(end + tau, end, step(0.0, end));*/

                double start = ((t * speed) + holeOffset) % (2f * Mathf.PI);
                if (start < 0f)
                {
                    start += 2f * Mathf.PI;
                }
                double end = (start + holeSize) % (2f * Mathf.PI);
                if (end < 0f)
                {
                    end += 2f * Mathf.PI;
                } 
                if (start > end)
                {
                    if (angle < start && angle > end)
                    {
                        k.ChangeHealth(-damage,"laser circle");
                        eCount = invinicbleFrames;
                    }
                    //clip(ang - end);
                    //clip(start - ang);
                }
                else
                {
                    if (start > angle || angle > end)
                    {
                        k.ChangeHealth(-damage,"laser circle");
                        eCount = invinicbleFrames;
                    }
                   // clip(abs(ang - ((start + end) / 2.0)) - ((end - start) / 2.0)); //hmm
                }
            }


        }
    }

	// Update is called once per frame
	void Update () {
	    if (eCount >= 1)
        {
            eCount--;
            float r = ((float)eCount) / ((float)invinicbleFrames);
            tint = new Color(tint.r, tint.g, tint.b, 1f-r);
            Material m = GetComponent<MeshRenderer>().material;
            m.SetColor("_Color", tint);
        }
        open = true;
	}
}
