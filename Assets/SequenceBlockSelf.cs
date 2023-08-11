using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceBlockSelf : MonoBehaviour
{

    public enum Mode
    {
        Neutral, Next, Done, AllDone, Fail
    }

    public Mode mode = Mode.Neutral;
    public sequenceBlockGroup hub;
    public int value;
    public Sprite defaultIcon;
    public bool collidedAlready = false;
    public bool resetting = false;
    public Sprite[] allSprites = new Sprite[5];
    public Sprite[] checkOrCrossSprites = new Sprite[4];
    public Vector3 origin;
    private Mode oldMode;
    public bool movingPlat = false;

    private double collidedTime;

    private void Start()
    {
        resetting = false;
        origin = transform.localPosition;
        oldMode = mode = Mode.Neutral;
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!collidedAlready && !hub.collidedThisFrame && c.rigidbody && 
            !c.rigidbody.isKinematic && GetComponent<Renderer>().isVisible)
        {
            hub.ChildCollided(this);
            collidedTime = DoubleTime.ScaledTimeSinceLoad;
        }
    }

    void Update()
    {

        if (hub.loseFlag > 0)
        {
            collidedAlready = true;
            resetting = true;
        }
        else if (hub.loseFlag == 0 && resetting)
        {
            collidedAlready = false;
            resetting = false;
        }

        if (hub.loseFlag > 0)
        {
            mode = Mode.Fail;
        }
        else if (hub.current == 6 || hub.multiplicities[hub.current] == 0)
        {
            mode = Mode.AllDone;
        }
        else if (hub.current == value)
        {
            if (collidedAlready)
            {
                mode = Mode.Done;
            }
            else
            {
                mode = Mode.Next;
            }
        }
        else if (hub.current > value)
        {
            mode = Mode.Done;
        }
        else
        {
            mode = Mode.Neutral;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        SpriteRenderer isr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        switch (mode)
        {
            case Mode.Neutral:
                sr.sprite = allSprites[0];
                sr.color = Color.white;
                isr.sprite = defaultIcon;
                isr.color = Color.white;
                if (!movingPlat)
                {
                    transform.localPosition = origin;
                }
                col.offset = Vector2.zero;
                break;
            case Mode.Next:
                sr.sprite = allSprites[1];
                sr.color = Color.white;
                isr.sprite = defaultIcon;
                isr.color = Color.white * (0.9f + 0.1f * (float)System.Math.Cos(16.0 * DoubleTime.UnscaledTimeRunning));
                if (!movingPlat)
                {
                    transform.localPosition = origin;
                }
                col.offset = Vector2.zero;
                break;
            case Mode.Done:
                sr.sprite = allSprites[2];
                sr.color = Color.white;
                isr.sprite = checkOrCrossSprites[0];
                isr.color = Color.white;
                if (!movingPlat)
                {
                    transform.localPosition = origin;
                }
                col.offset = Vector2.zero;
                break;
            case Mode.AllDone:
                sr.sprite = allSprites[3];
                Color rc = Fakerand.Color();
                sr.color = new Color(rc.r,rc.g,rc.b,sr.color.a-0.01851852f);
                //print(sr.color.a);
                isr.sprite = checkOrCrossSprites[0];
                isr.color = new Color(0f, 1f, 1f, sr.color.a - 0.01851852f);
                if (!movingPlat)
                {
                    transform.localPosition = origin;
                }
                col.offset = Vector2.zero;
                if (sr.color.a <= 0.01f)
                {
                    Destroy(gameObject);
                }
                //isr.transform.DetachChildren();
                foreach (Transform bye in transform)
                {
                    if (bye != transform && bye != isr.transform)
                    {
                        Destroy(bye.gameObject);
                    }
                }
                break;
            case Mode.Fail:
                sr.sprite = allSprites[4];
                sr.color = Color.white;
                isr.sprite = checkOrCrossSprites[2];
                isr.color = Color.white;
                if (!movingPlat)
                {
                    Vector2 r1 = Fakerand.UnitCircle() * 3f;
                    transform.localPosition = origin + (Vector3)r1;
                    col.offset = -r1;
                }
                break;
            default:
                break;
        }
    }

}
