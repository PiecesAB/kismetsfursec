using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipwire : MonoBehaviour
{
    public float speed = 80f;
    public GameObject ripple;

    public AudioSource flipOn;
    public AudioSource flipOff;

    private enum Mode { Wire, Square }
    private SpriteRenderer myRenderer;
    private BoxCollider2D myCollider;
    private Mode mode;
    private bool verticalWire;

    private void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myCollider = gameObject.AddComponent<BoxCollider2D>();
        myCollider.isTrigger = true;
        mode = (myRenderer.size.x == myRenderer.size.y) ? Mode.Square : Mode.Wire;
        if (mode == Mode.Square)
        {
            myCollider.size = myRenderer.size - Vector2.one * 2;
            myCollider.isTrigger = false;
        } else {
            if (myRenderer.size.x > myRenderer.size.y)
            {
                verticalWire = false;
                myCollider.size = new Vector2(myRenderer.size.x, 8);
            }
            else
            {
                verticalWire = true;
                myCollider.size = new Vector2(8, myRenderer.size.y);
            }
        }
    }

    // wires only
    private Vector2 GetFlipAxis(Transform other)
    {
        Vector2 dif;
        dif = other.position - transform.position;
        if (verticalWire) { return new Vector2(-1, 1); }
        else { return new Vector2(1, -1); }
    }

    // wires only
    private Vector2 GetMovementAxis(Transform other)
    {
        Vector2 dif;
        dif = other.position - transform.position;
        if (verticalWire) { return new Vector2(Mathf.Sign(dif.x), 0); }
        else { return new Vector2(0, Mathf.Sign(dif.y)); }
    }

    private bool flipping = false;
    private IEnumerator Flip(GameObject plr, Vector2 flipAxis, Vector2 mvtAxis)
    {
        if (flipping) { yield break; }
        flipping = true;
        FlipwireHelper existingHelper = plr.GetComponent<FlipwireHelper>();
        if (existingHelper != null) { Destroy(existingHelper); }
        else { LauncherEnemy.RemovePlayerControl(plr, LauncherEnemy.RemoveControlMode.Flipwire); }
        FlipwireHelper newHelper = plr.AddComponent<FlipwireHelper>();
        newHelper.velocity = mvtAxis * speed;
        newHelper.source = this;
        newHelper.ripple = ripple;
        newHelper.flipOff = flipOff;
        flipOn.Stop(); flipOn.Play();

        myCollider.enabled = false;
        Vector2 oldScale = transform.localScale;
        Vector2 targetScale = oldScale * flipAxis;
        bool flipped = false;
        for (float t = 1; t > -1; t -= 0.2f * Time.timeScale)
        {
            transform.localScale = Vector2.Lerp(oldScale, targetScale, EasingOfAccess.SineSmooth(1f - (0.5f * (t + 1))));
            if (t <= 0 && !flipped)
            {
                flipped = true;
                myRenderer.flipX = !myRenderer.flipX;
                myRenderer.flipY = !myRenderer.flipY;
            }
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = targetScale;
        myCollider.enabled = true;        
        flipping = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { return; }
        if (!(col is BoxCollider2D)) { return; }
        StartCoroutine(Flip(col.gameObject, GetFlipAxis(col.transform), GetMovementAxis(col.transform)));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer != 20) { return; }
        Vector2 d = col.GetContact(0).normal;
        StartCoroutine(Flip(col.gameObject, -2 * new Vector2(Mathf.Abs(d.x), Mathf.Abs(d.y)) + Vector2.one, -d));
    }

    private bool visible = false;
    private Coroutine whileVisible;

    private IEnumerator WhileVisible()
    {
        WaitForEndOfFrame eof = new WaitForEndOfFrame();
        int f = 0;
        while (true)
        {
            if (Time.timeScale > 0)
            {
                if (f < 60)
                {
                    float prog = EasingOfAccess.SineSmooth(f / 60f);
                    myRenderer.material.SetVector("_TintOffset", new Vector2(prog, prog));
                }
                else
                {
                    myRenderer.material.SetVector("_TintOffset", Vector4.zero);
                }

                ++f;
                if (f >= 100) { f = 0; }
            }
            yield return eof;
        }
    }

    private void OnBecameVisible()
    {
        visible = true;
        if (whileVisible == null)
        {
            whileVisible = StartCoroutine(WhileVisible());
        }
    }

    private void OnBecameInvisible()
    {
        visible = false;
        if (whileVisible != null)
        {
            StopCoroutine(whileVisible);
            whileVisible = null;
            myRenderer.material.SetVector("_TintOffset", Vector4.zero);
        }
    }
}
