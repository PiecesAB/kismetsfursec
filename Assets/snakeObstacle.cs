using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snakeObstacle : GenericBlowMeUp
{
    public SkinnedMeshRenderer snekShape;

    public float shapeTimer;
    public float speed = 0.83333333f;
    public BoxCollider2D bodyCollider;
    public Vector2 dir = Vector2.right;
    public bool flipped;
    public Rigidbody2D myRigidbody2D;
    public float damageAmount;
    [Range(0f,16f)]
    public float meshFizzMultiplier;
    public Collider2D myGround;

    private float fallVel;

    public int turning = 0;
    public int stall = 0;

    private static Rect flatSize = new Rect(-36f, 3f, 64f, 6f);
    private static Rect coiledSize = new Rect(-16f, 16f, 24f, 32f);

    private const int turnFrames = 27;
    private const float dmgStart = 1f;
    private const float dmgInc = 0.005f;

    private Vector3 origRot;

    public float MoveWave(float x)
    {
        return (WaveFunctions.Hex(x) + 1f) * 0.5f;
    }

    void Start()
    {
        shapeTimer = 0f;
        turning = 0;
        myRigidbody2D = GetComponent<Rigidbody2D>();
        flipped = false;
        stall = 0;
        damageAmount = dmgStart;
        dir = transform.right;
        origRot = transform.localEulerAngles;
        //snekShape.sharedMesh.RecalculateBounds();
    }

    void Update()
    {
        if (Time.timeScale > 0 && snekShape.isVisible)
        {
            if (turning == 0)
            {
                if (stall == 0 && fallVel == 0f)
                {
                    float l = MoveWave(shapeTimer);
                    float ldx = WaveFunctions.HexDerivative(shapeTimer);
                    snekShape.SetBlendShapeWeight(0, l * 100f);
                    shapeTimer += speed * 0.016666666666666666f * Time.timeScale;
                    Vector2 oldBCS = bodyCollider.size;
                    bodyCollider.size = Vector2.Lerp(flatSize.size, coiledSize.size, l);
                    bodyCollider.offset = Vector2.Lerp(flatSize.position, coiledSize.position, l);
                    if (ldx < 0f)
                    {
                        transform.localPosition += (Vector3)(dir * (bodyCollider.size.x - oldBCS.x));
                    }
                }
                Collider2D col = Physics2D.OverlapPoint(transform.TransformPoint(new Vector3(13f, 15f, 0f)), 1049344);
                if (col)
                {
                    if (col.gameObject.layer == 20)
                    {
                        stall = 1;
                        KHealth kh = col.GetComponent<KHealth>();
                        if (kh)
                        {
                            kh.ChangeHealth(-damageAmount, "snek");
                            damageAmount += dmgInc;
                        }
                    }
                    else
                    {
                        stall = 0;
                        turning = turnFrames;
                    }
                }
                else
                {
                    stall = 0;
                }
            }
            else
            {
                turning--;
                float h = (flipped ? 0f : 180f) - (180f * ((float)turning / turnFrames));
                transform.localEulerAngles = origRot;
                transform.RotateAround(transform.position, transform.up, h);
                if (turning == 0)
                {
                    dir = -dir;
                    flipped = !flipped;
                    transform.localEulerAngles = origRot;
                    transform.RotateAround(transform.position, transform.up, flipped?180f:0f);
                }
            }
        }

        myGround = Physics2D.OverlapBox(transform.TransformPoint(bodyCollider.offset + Vector2.down*bodyCollider.size.y*0.5f),
                             new Vector2(bodyCollider.size.x, 1f), transform.eulerAngles.z, 256 + 2048 + 1048576);
        if (myGround)
        {
            if (myGround.gameObject.layer == 20)
            {
                transform.position -= fallVel * transform.up;
            }
            fallVel = 0f;
            snekShape.material.SetFloat("_Fizz", 0f);
            //make on ground... someday
        }
        else
        {
            fallVel = Mathf.Max(fallVel-0.1f, -2.5f);
            snekShape.material.SetFloat("_Fizz", -fallVel * 0.007f);
            transform.position += fallVel * transform.up;
        }

        if (myRigidbody2D)
        {
            myRigidbody2D.MovePosition(transform.position);
        }
    }
}
