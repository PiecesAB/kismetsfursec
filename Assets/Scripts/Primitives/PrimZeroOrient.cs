using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimZeroOrient : MonoBehaviour
{
    public bool continuous = false;
    public bool alsoMakeScaleOne = false;

    private void SetScaleOne()
    {
        Transform oldParent = transform.parent;
        transform.SetParent(null);
        transform.localScale = Vector3.one;
        transform.SetParent(oldParent);
    }

    private void Start()
    {
        transform.rotation = Quaternion.identity;
        if (alsoMakeScaleOne) { SetScaleOne(); }
        if (!continuous) { Destroy(this); }
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity;
        if (alsoMakeScaleOne) { SetScaleOne(); }
    }
}
