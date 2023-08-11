using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearCircularDouble : GearCircular
{
    private float innerRadius;
    private float outerRadius;
    [HideInInspector]
    public CircleCollider2D innerCollider;
    [HideInInspector]
    public CircleCollider2D outerCollider;
    [HideInInspector]
    public HashSet<Gear> innerNeighbors = new HashSet<Gear>();
    [HideInInspector]
    public HashSet<Gear> outerNeighbors = new HashSet<Gear>();

    private void SetNeighbors()
    {
        if (innerNeighbors.Count > 0 && outerNeighbors.Count > 0) { return; }

        // more negative = closer to center
        foreach (Gear nb in neighbors)
        {
            float innerTest = innerCollider.Distance(nb.gearCollider).distance;
            if (innerTest < 0) { innerNeighbors.Add(nb); }
            else { outerNeighbors.Add(nb); }
        }
    }

    protected override void ChildStart()
    {
        CircleCollider2D[] circles = GetComponents<CircleCollider2D>();
        if (circles.Length != 2) { throw new System.Exception("There should be exactly two CircleCollider2Ds on this double gear"); }

        innerCollider = circles[0]; outerCollider = circles[1];
        if (innerCollider.radius > outerCollider.radius) { innerCollider = circles[1]; outerCollider = circles[0]; }
        innerRadius = innerCollider.radius; outerRadius = outerCollider.radius;

        innerNeighbors = new HashSet<Gear>();
        outerNeighbors = new HashSet<Gear>();

        lastRotZ = transform.eulerAngles.z;
    }

    protected override float CalculateTranslation()
    {
        SetNeighbors();

        float chosenRad = outerRadius;
        if (innerNeighbors.Contains(currTarget)) { chosenRad = innerRadius; }

        NormalizeLastRot();
        return chosenRad * (transform.eulerAngles.z - lastRotZ) * Mathf.Deg2Rad;
    }

    protected override void RotateInterlock(float dt)
    {
        SetNeighbors();

        float chosenRad = outerRadius;
        if (innerNeighbors.Contains(currSource)) { chosenRad = innerRadius; }

        float dt2 = -dt * Mathf.Rad2Deg / chosenRad;
        transform.eulerAngles += Vector3.forward * dt2;
    }


}
