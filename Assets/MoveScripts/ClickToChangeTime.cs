using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using System;

public class ClickToChangeTime : SpecialGunTemplate {

    public GameObject arrow;
    public Color arrowAdditiveColor;
    public Color arrowSubtractiveColor;
    private SpriteRenderer arrowSprite;
    public GameObject prefabKhalLaser;
    
    protected override void ChildStart () {
    }

    protected override void AimingBegin()
    {
        Vector2 selfRight = transform.right * Mathf.Sign(transform.localScale.x);
        Vector2 selfUp = transform.up * Mathf.Sign(transform.localScale.y);
        Vector2 v = Vector2.zero;
        if ((e.currentState & 1UL) == 1UL) { v -= selfRight; }
        if ((e.currentState & 2UL) == 2UL) { v += selfRight; }
        if ((e.currentState & 4UL) == 4UL) { v += selfUp; }
        if ((e.currentState & 8UL) == 8UL) { v -= selfUp; }
        nextangle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        arrow.transform.localEulerAngles = Vector3.zero;
        arrow.transform.RotateAround(transform.position, Vector3.forward, nextangle);
    }

    protected override void AimingUpdate()
    {
        if ((e.currentState & 768UL) == 256UL) { nextangle += 3f; }
        if ((e.currentState & 768UL) == 512UL) { nextangle -= 3f; }
        // don't allow mid-aim change with these buttons, because they can affect mvt. simultaneously
        //if (e.ButtonDown(4UL, 12UL)) { nextangle = 90f; }
        //if (e.ButtonDown(8UL, 12UL)) { nextangle = 270f; }

        nextangle = Mathf.Repeat(nextangle, 360f);

        arrow.transform.localEulerAngles = Vector3.zero;
        arrow.transform.RotateAround(transform.position, Vector3.forward, nextangle);
    }

    protected override float Fire()
    {
        Vector3 fakeRight = arrow.transform.right;
        if (transform.localScale.x < 0) { fakeRight = -fakeRight; }
        Vector3 selfRight = transform.right * Mathf.Sign(transform.localScale.x);
        Vector3 selfUp = transform.up * Mathf.Sign(transform.localScale.y);
        float dirX = Vector2.Dot(selfRight, fakeRight) * 9f;
        float dirY = Vector2.Dot(selfUp, fakeRight) * 16.5f;
        Vector3 offset = selfRight*Mathf.Clamp(dirX, -4, 999) + selfUp*dirY;
        GameObject neu = Instantiate(prefabKhalLaser, transform.position + (Vector3.back * 64) + offset, Quaternion.identity);
        SuperRay sr = neu.GetComponent<SuperRay>();
        sr.cursorVelocity = gunStrength * fakeRight;
        sr.fakeRight = ((transform.localScale.x < 0) != (transform.localScale.y < 0)) ?-transform.right:transform.right;
        // stop the player from moving in the cursor direction, lest they run into it (and other finicky scenarios ensue)
        Vector2 rv = bm.RotateVector2(bm.fakePhysicsVel);
        Vector2 cn = sr.cursorVelocity.normalized;
        float d = Vector2.Dot(rv, cn);
        //if (d > 0.1f) { bm.extraPerFrameVel -= d * cn; }
        return 1f;
    }

    protected override void GraphicsUpdateWhenAiming()
    {
        arrow.SetActive(true);
    }

    protected override void GraphicsUpdateWhenNotAiming()
    {
        arrow.SetActive(false);
    }

    protected override void ChildUpdate () {
        if (!disabled)
        {
            if (!arrowSprite) { arrowSprite = arrow.GetComponent<SpriteRenderer>(); }
            if (arrowSprite)
            {
                nextangle = Mathf.Repeat(nextangle, 360f);
                bool wouldBeAdditive = nextangle <= 90f || nextangle > 270f;
                bool flipped = transform.localScale.y < 0f;
                wouldBeAdditive ^= flipped;
                float t = (float)Math.Sin((DoubleTime.UnscaledTimeRunning * 12.0) % 6.2832);
                if (wouldBeAdditive)
                {
                    arrowSprite.color = Color.Lerp(arrowAdditiveColor, Color.white, (0.5f * t) + 0.5f);
                }
                else
                {
                    arrowSprite.color = Color.Lerp(arrowSubtractiveColor, Color.black, (0.5f * t) + 0.5f);
                }
            }
        }
    }
}
