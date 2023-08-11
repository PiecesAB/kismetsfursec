using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulley2 : MonoBehaviour
{

    public float movability;
    public float extraBump;
    public pulley2Helper leftPlat;
    public pulley2Helper rightPlat;
    public Rigidbody2D leftRB;
    public Rigidbody2D rightRB;
    public TextMesh leftTM;
    public TextMesh rightTM;
    public Prim3DRotate leftWheel;
    public Prim3DRotate rightWheel;
    public LineRenderer line;
    public float mvt;
    public Vector2 mvtRange;

    

    private Vector2 leftOrigin;
    private Vector2 rightOrigin;
    private float lDel;
    private float rDel;
    private float lastMvt;


    // left platform lower: negative.
    // right platform lower: positive.

    public void Impulse(pulley2Helper plat, float amt)
    {
        amt = Mathf.Round(amt);
        if (plat == leftPlat) // make more negative
        {
            lDel += amt;
            mvt -= amt * movability;
            mvt = Mathf.Clamp(mvt, mvtRange.x, mvtRange.y);
        }
        if (plat == rightPlat)
        {
            rDel += amt;
            mvt += amt * movability;
            mvt = Mathf.Clamp(mvt, mvtRange.x, mvtRange.y);
        }
    }

    public void AddPlrBump(pulley2Helper plat)
    {
        if (plat == leftPlat) // make more negative
        {
            extraBump = 2f;
        }
        if (plat == rightPlat)
        {
            extraBump = -2f;
        }
    }

    void Start()
    {
        extraBump = mvt = lDel = rDel = 0;
        leftOrigin = leftRB.transform.position;
        rightOrigin = rightRB.transform.position;
    }

    void Update()
    {
        if (extraBump != 0f)
        {
            mvt += extraBump;
            mvt = Mathf.Clamp(mvt, mvtRange.x, mvtRange.y);
            extraBump = Mathf.MoveTowards(extraBump,0f,0.1f);
        }

        leftRB.position = leftOrigin + (Vector2.up * mvt);
        leftRB.velocity = (mvt - lastMvt) * Vector2.up * 60f;
        rightRB.position = rightOrigin + (Vector2.down * mvt);
        rightRB.velocity = (mvt - lastMvt) * Vector2.down * 60f;
        if ((Vector2)leftRB.transform.position == leftOrigin + (Vector2.up * mvt)) // check if not moving
        {
            leftWheel.speed = rightWheel.speed = 0;
        }
        else
        {
            leftWheel.speed = rightWheel.speed = (lDel - rDel - extraBump) * 5f;
        }
        leftTM.text = lDel.ToString();
        rightTM.text = rDel.ToString();
        lDel = rDel = 0f;
        line.SetPosition(0, leftPlat.transform.localPosition);
        line.SetPosition(3, rightPlat.transform.localPosition);

        lastMvt = mvt;
    }
}
