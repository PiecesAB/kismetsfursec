using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperRayMagnet : MonoBehaviour {

    public float affectCutoffDistance;
    [Header("Negative = Attract")]
    public float affectPower;
    public Sprite pulseSprite;
    public Gradient pulseRepelColor;
    public Gradient pulseAttractColor;
    public string pulseSortLayer;
    public short pulseSortOrderInLayer;
    public int pulseFramesBetween;
    public int pulseFramesToExist;
    private float pfteRcpr;
    private float imageScaleRatio;
    private int pulseMakingTimer;
    private float rcpr;
    public bool pulsoReverso;
    public bool on;

    void Start()
    {
        rcpr = 1f / pulseFramesToExist;
        pfteRcpr = affectCutoffDistance / pulseFramesToExist;
        imageScaleRatio = 2f * affectCutoffDistance * pulseSprite.pixelsPerUnit / pulseSprite.texture.width;
        pulseMakingTimer = pulseFramesBetween;
    }

    public void Reverse()
    {
        affectPower = -affectPower;
    }

    public static void AttractAllRays(Vector3 attractPos, float cutoffDistance, float power)
    {
        foreach (SuperRay r in SuperRay.allRays)
        {
            Vector2 v = ((Vector2)r.transform.position + r.cursor - (Vector2)attractPos);
            float m = v.magnitude;
            if (m <= cutoffDistance)
            {
                r.cursorAccel = (power * v.normalized * Time.timeScale) / Mathf.Max(m * m, 8f);
            }
        }
    }

    void Update () {
        if (on)
        {
            AttractAllRays(transform.position, affectCutoffDistance, affectPower);
        }

        Gradient pulseColor = pulseRepelColor;
        if (affectPower < 0)
        {
            pulseColor = pulseAttractColor;
            pulsoReverso = true;
        }
        else
        {
            pulsoReverso = false;
        }

        pulseMakingTimer = Mathf.Max(0, pulseMakingTimer-1);
        if (on && pulseMakingTimer <= 0)
        {
            GameObject n = new GameObject();
            Transform t = n.transform;
            SpriteRenderer r = n.AddComponent<SpriteRenderer>();
            t.SetParent(transform);
            t.localPosition = Vector3.zero;
            if (pulsoReverso)
            {
                t.localScale = Vector3.one * imageScaleRatio;
                r.color = pulseColor.Evaluate(1f);
            }
            else
            {
                t.localScale = Vector3.zero;
                r.color = pulseColor.Evaluate(0f);
            }
            
            r.sortingLayerName = pulseSortLayer;
            r.sortingOrder = pulseSortOrderInLayer;
            r.sprite = pulseSprite;
            pulseMakingTimer = pulseFramesBetween;
        }

        foreach (Transform t in transform)
        {
            float prog = t.localScale.x / imageScaleRatio;
            bool upd = true;
            if (pulsoReverso)
            {
                prog -= rcpr;
                if (prog <= 0f)
                {
                    Destroy(t.gameObject);
                    upd = false;
                }
            }
            else
            {
                prog += rcpr;
                if (prog >= 1f)
                {
                    Destroy(t.gameObject);
                    upd = false;
                }
            }

            if (upd)
            {
                t.GetComponent<SpriteRenderer>().color = pulseColor.Evaluate(prog);
                t.localScale = Vector3.one * prog * imageScaleRatio;
            }
        }
	}
}
