using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangramLaserCollider : MonoBehaviour
{
    public LineRenderer myLR;
    public Transform myColTransform;
    public BoxCollider2D myCollider;

    public int myFramesToActive;

    public int myIndex;

    private KHealth victim;

    private static Color orange = new Color(1.0f, 0.6f, 0f);

    void Start()
    {
        myLR = GetComponent<LineRenderer>();
        myColTransform = transform/*.GetChild(0)*/;
        myCollider = myColTransform.GetComponent<BoxCollider2D>();
        victim = null;
        myFramesToActive = TangramLaser.framesToActive;
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.GetComponent<KHealth>())
        {
            victim = c.GetComponent<KHealth>();
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (victim)
        {
            BasicMove b = victim.GetComponent<BasicMove>();
            if (b)
            {
                victim.ChangeHealth(-TangramLaser.plrDmgMultiplier * b.Damage, "tangram laser");
            }

            TangramLaser.pairs.RemoveAt(myIndex);
            for (int i = myIndex + 1; i < TangramLaser.pairLCs.Count; i++)
            {
                TangramLaser.pairLCs[i].myIndex--;
            }
            TangramLaser.pairLCs.RemoveAt(myIndex);
            TangramLaser.pairFramesToActive.RemoveAt(myIndex);
            //it'll come back
        }
    }

    void Update()
    {
        if (myFramesToActive == 0)
        {
            Vector2 d = myLR.GetPosition(1) - myLR.GetPosition(0);
            Vector2 a = (myLR.GetPosition(1) + myLR.GetPosition(0)) * 0.5f;
            myCollider.size = new Vector2(d.magnitude, myLR.startWidth);
            myColTransform.position = a;
            myColTransform.eulerAngles = Vector3.forward * Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            myLR.startColor = myLR.endColor = orange;
        }
        else
        {
            float t = (float)(TangramLaser.framesToActive - myFramesToActive) / TangramLaser.framesToActive;
            Color l = new Color(1.0f * t, 0.5f*t, 0f, 0.6f*t);
            myLR.startColor = myLR.endColor = l;
        }
    }
}
