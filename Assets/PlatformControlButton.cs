using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformControlButton : MonoBehaviour
{
    private PlatformControlButtonMain main;
    private SpriteRenderer spr;

    [SerializeField]
    private SpriteRenderer iconBG;
    [SerializeField]
    private SpriteRenderer iconFG;
    [SerializeField]
    private float onDelay = 0;
    [SerializeField]
    private bool deactivateAfterPress;

    [HideInInspector]
    public bool on = false;

    [SerializeField]
    private Sprite offButton;
    [SerializeField]
    private Sprite onButton;
    [SerializeField]
    private Sprite pause;
    [SerializeField]
    private Sprite arrow;
    public Vector2 velocity;

    private void Start()
    {
        main = transform.parent.GetComponent<PlatformControlButtonMain>();
        spr = GetComponent<SpriteRenderer>();
        if (spr)
        {
            float ang = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            ang = Mathf.Repeat(ang, 360f);
            spr.flipY = (ang >= 135f && ang <= 315f);
            if (velocity.magnitude < 0.1f) { iconFG.sprite = pause; iconFG.transform.eulerAngles = Vector3.zero; }
            else { iconFG.sprite = arrow; iconFG.transform.eulerAngles = ang * Vector3.forward; }
        }
    }

    private IEnumerator WhileOnDisplay()
    {
        if (!spr) { yield break; }

        iconFG.color = Color.black;
        while (on)
        {
            iconBG.color = Color.Lerp(new Color(0f, 0.5f, 1f), Color.cyan, (1f + (float)System.Math.Sin(DoubleTime.ScaledTimeSinceLoad * 6.28)) * 0.5f);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
        iconFG.color = Color.white;
        iconBG.color = Color.black;
    }

    public IEnumerator OnWithDelay()
    {
        if (onDelay == 0)
        {
            On();
            yield break;
        }
        yield return new WaitForSeconds(onDelay);
        On();
    }

    public void On()
    {
        if (on) { return; }
        if (!main.CanIMoveThisDirection(velocity)) { return; }
        on = true;
        main.ActivateSwitch(this);
        if (spr) spr.sprite = onButton;
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(WhileOnDisplay());
        if (deactivateAfterPress)
        {
            Off();
            GetComponent<Collider2D>().enabled = false;
        }
    }

    public void Off()
    {
        if (!on) { return; }
        on = false;
        if (spr) spr.sprite = offButton;
        GetComponent<Collider2D>().enabled = true;
    }

    public void TryOn(Collider2D col)
    {
        if (col.isTrigger) { return; }
        if (col.GetComponent<beeDrone>()) { return; }
        // we want it to be pressed by ground, but only in the right direction.
        Rigidbody2D r2 = col.GetComponent<Rigidbody2D>();
        if (col.gameObject.layer != 20)
        {
            if (!r2 || r2.velocity.sqrMagnitude < 1f)
            {
                if (Vector2.Dot(transform.right, main.dm.v) <= 0.3f) { return; }
            }
        }
        On();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        TryOn(col);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        TryOn(col);
    }
}
