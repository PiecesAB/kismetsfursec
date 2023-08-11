using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gear : MonoBehaviour
{
    protected HashSet<Gear> neighbors = new HashSet<Gear>();

    private bool rotatedThisFrame = false;
    private bool cancelledThisFrame = false;

    // for use in RotateInterlock to get the source gear
    [HideInInspector]
    public Gear currSource;

    // for use in CalcuateTranslation
    [HideInInspector]
    public Gear currTarget;

    [HideInInspector]
    public Collider2D gearCollider;

    protected abstract void ChildStart();

    private void Start()
    {
        neighbors = new HashSet<Gear>();
        rotatedThisFrame = false;
        cancelledThisFrame = false;
        currSource = null;
        gearCollider = GetComponent<Collider2D>();
        ChildStart();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject != this && col.gameObject.GetComponent<Gear>())
        {
            neighbors.Add(col.gameObject.GetComponent<Gear>());
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<Gear>())
        {
            neighbors.Remove(col.gameObject.GetComponent<Gear>());
        }
    }

    protected abstract void RotateInterlock(float dt);
    protected abstract void RotateCancel();

    public void RotateInterlockBegin(float dt)
    {
        if (rotatedThisFrame) { return; }
        rotatedThisFrame = true;
        RotateInterlock(dt);
        foreach (Gear nb in neighbors)
        {
            nb.currSource = this;
            currTarget = nb;
            nb.RotateInterlockBegin(CalculateTranslation());
        }
    }

    public void RotateCancelBegin()
    {
        if (cancelledThisFrame) { return; }
        cancelledThisFrame = true;
        RotateCancel();
        foreach (Gear nb in neighbors)
        {
            nb.currSource = this;
            currTarget = nb;
            nb.RotateCancelBegin();
        }
    }

    protected abstract bool RotationTrigger();

    protected abstract void ChildAfterUpdate();

    protected abstract float CalculateTranslation();

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        bool clean = false;
        while (!clean)
        {
            clean = true;
            foreach (Gear g in neighbors) { if (g == null) { neighbors.Remove(g); clean = false; break; } }
        }

        if (!rotatedThisFrame && RotationTrigger())
        {
            rotatedThisFrame = true;
            foreach (Gear nb in neighbors)
            {
                nb.currSource = this;
                currTarget = nb;
                nb.RotateInterlockBegin(CalculateTranslation());
            }
        }

        rotatedThisFrame = false;
        cancelledThisFrame = false;
        ChildAfterUpdate();
    }
}
