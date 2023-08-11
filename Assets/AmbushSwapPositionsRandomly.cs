using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushSwapPositionsRandomly : MonoBehaviour, IAmbushChildController
{
    public Transform[] swappables;
    [HideInInspector]
    public Vector3 swappedPosition = Vector3.positiveInfinity;

    public void OnAmbushBegin()
    {
        if (swappables.Length < 2) { return; }
        int r = Fakerand.Int(0, swappables.Length);
        Transform other = swappables[r];
        AmbushSwapPositionsRandomly otherSameSwapper = other.GetComponent<AmbushSwapPositionsRandomly>();
        Vector3 otherPosition = other.position;
        if (otherSameSwapper && otherSameSwapper.swappedPosition.x < 1e7f)
        {
            otherPosition = otherSameSwapper.swappedPosition;
        }
        Vector3 thisPosition = transform.position;
        if (swappedPosition.x < 1e7f)
        {
            thisPosition = swappedPosition;
        }
        transform.position = swappedPosition = otherPosition;
        otherSameSwapper.swappedPosition = other.position = thisPosition;
    }
}
