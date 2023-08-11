using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimEnableWhenTargetInTrigger : MonoBehaviour
{
    public Transform target;
    [HideInInspector]
    public Bounds bounds;
    public GameObject[] whatToEnable;
    public bool[] makeClonesInstead;
    private GameObject[] clones;

    private bool lastOn;
    private bool on;

    void Start()
    {
        bounds = GetComponent<Collider2D>().bounds;
        lastOn = on = false;
        clones = new GameObject[whatToEnable.Length];
        Toggle();
    }

    void ClearClones()
    {
        for (int i = 0; i < clones.Length; ++i)
        {
            if (clones[i] != null) { Destroy(clones[i]); }
        }
    }

    void Toggle()
    {
        for (int i = 0; i < whatToEnable.Length; ++i)
        {
            if (i < makeClonesInstead.Length && makeClonesInstead[i])
            {
                if (on)
                {
                    GameObject clone = Instantiate(whatToEnable[i], whatToEnable[i].transform.position, whatToEnable[i].transform.rotation, whatToEnable[i].transform.parent);
                    clone.SetActive(true);
                    clones[i] = clone;
                }
                else
                {
                    ClearClones();
                }
            }
            else
            {
                whatToEnable[i].SetActive(on);
            }
        }
        
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (target == null) { return; }
        on = bounds.Contains(target.position);
        if (on != lastOn) { Toggle(); }
        lastOn = on;
    }
}
