using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAmbushCountEnabler : MonoBehaviour
{

    private Transform act;
    // if shot is empty it will try to act on the simple bullet shooter of this object
    public GameObject shot;
    private BulletHellMakerFunctions makerWhenNoShot;

    public int min;
    public int max;

    void Start()
    {
        act = GetComponentInParent<AmbushController>().transform;
        if (!shot) { makerWhenNoShot = GetComponent<BulletHellMakerFunctions>(); }
    }

    void Update()
    {
        bool e = act.childCount >= min && act.childCount <= max;
        if (shot) { shot.SetActive(e); }
        else { makerWhenNoShot.enabled = e; }
    }
}
