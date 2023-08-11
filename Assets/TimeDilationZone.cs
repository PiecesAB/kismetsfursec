using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDilationZone : ColliderZoneTemplate<TimeDilationZone> //playing a mean trick on the compiler for static variables in subclasses
{
    public float timeIncreaseAmount;
    public bool playersAffectTime = true;
    static List<float> amounts = new List<float>();

    public override void ResetStuff()
    {
        amounts.Clear();
    }

    public override void ColliderAdd()
    {
        amounts.Add(timeIncreaseAmount);
    }

    public override void ColliderRemove(int index)
    {
        amounts.RemoveAt(index);
    }

    public override void ObjectIn(int index, GameObject obj, GameObject other)
    {
        if (!(!other.GetComponent<TimeDilationZone>().playersAffectTime && obj.layer == 20))
        {
            TimeCalc.SetExtraPerFrameTime(amounts[index]);
        }
    }
}
