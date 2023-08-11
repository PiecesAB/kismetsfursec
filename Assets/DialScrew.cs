using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialScrew : DialBase, IPrimDismantle
{
    public AudioSource dismantleSound;

    public Transform[] otherRotors;
    public Transform[] attached;
    //public bool usePhysics; // will always use physics if the target has a rigidbody2d.

    public AmbushController rigged = null;
    public tripwire timerRigged;

    [HideInInspector]
    public HingeJoint2D physicsPivot = null;

    private static Dictionary<Transform, HashSet<DialScrew>> attachments = new Dictionary<Transform, HashSet<DialScrew>>();
    private List<Transform> myAttachedObjects = new List<Transform>();

    private bool detached = false;

    private void OnLevelWasLoaded(int level)
    {
        attachments.Clear();
    }

    public void OnDismantle()
    {
        ReleasePlayer();
        foreach (StaticBulletsOnVertices s in GetComponentsInChildren<StaticBulletsOnVertices>())
        {
            Destroy(s);
        }
        Destroy(this);
    }

    private void Dismantle(Transform t)
    {
        if (t == transform) { RemoveAttachments(); }

        rotatingSound.Stop();
        dismantleSound.Play();

        t.gameObject.AddComponent<PrimDismantle>();
    }

    private void HangObject(Rigidbody2D obj, DialScrew pivot) // 1 screw left, physics object
    {
        obj.isKinematic = false;

        Rigidbody2D r2 = (pivot.GetComponent<Rigidbody2D>() != null) ? pivot.GetComponent<Rigidbody2D>() : pivot.gameObject.AddComponent<Rigidbody2D>();
        r2.isKinematic = true;

        HingeJoint2D w2 = obj.gameObject.AddComponent<HingeJoint2D>();
        w2.connectedBody = r2;
        w2.anchor = pivot.transform.position - obj.transform.position;

        Physics2D.IgnoreCollision(obj.GetComponent<Collider2D>(), pivot.GetComponent<Collider2D>());

        pivot.physicsPivot = w2;
    }

    private void ReleaseObject() // 0 screws left, physics object
    {
        Destroy(physicsPivot);
    }

    private void RemoveAttachments()
    {
        for (int i = 0; i < myAttachedObjects.Count; ++i)
        {
            Transform t = myAttachedObjects[i];
            if (attachments.ContainsKey(t))
            {
                attachments[t].Remove(this);
            }

            Rigidbody2D usePhysics = t.GetComponent<Rigidbody2D>();

            if (attachments[t].Count == 0)
            {
                if (usePhysics != null)
                {
                    ReleaseObject();
                }
                else
                {
                    Dismantle(t);
                }
            }

            if (attachments[t].Count == 1)
            {
                if (usePhysics != null)
                {
                    HangObject(usePhysics, attachments[t].ToList()[0]);
                }
            }
        }
    }

    private void AddAttachment(Transform t)
    {
        if (!attachments.ContainsKey(t)) { attachments.Add(t, new HashSet<DialScrew>()); }
        attachments[t].Add(this);
        myAttachedObjects.Add(t);
    }

    protected override void ChildStart()
    {
        detached = false;
        for (int i = 0; i < attached.Length; ++i)
        {
            AddAttachment(attached[i]);

            if (attached[i].GetComponent<Rigidbody2D>())
            {
                attached[i].GetComponent<Rigidbody2D>().isKinematic = true;
            }
        }
    }

    protected override void ChildUpdate()
    {
        if (rigged && dx != 0 && AmbushController.activeAmbushesCount == 0)
        {
            rigged.Activate();
        }

        if (timerRigged && dx != 0)
        {
            timerRigged.Trip();
        }

        for (int i = 0; i < otherRotors.Length; ++i)
        {
            otherRotors[i].RotateAround(transform.position, Vector3.forward, -dx * rotorSpeedMult);
        }

        if (myValue == minValue)
        {
            Dismantle(transform);
        }
    }
}
