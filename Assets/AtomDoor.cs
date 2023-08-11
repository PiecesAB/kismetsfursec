using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomDoor : GenericBlowMeUp
{
    public int amount;
    public GameObject nucleus;
    public GameObject electron;

    private bool unlocked = false;

    void Start()
    {
        GetComponentInChildren<TextMesh>().text = amount.ToString();
    }

    private void Check(Collision2D col)
    {
        if (col.gameObject.layer == 20)
        {
            ElectronTracker et = col.gameObject.GetComponent<ElectronTracker>();
            if (et && !unlocked) {
                bool succ = et.DecrementByDoor(transform.position, amount);
                if (succ)
                {
                    unlocked = true;
                    nucleus.transform.SetParent(transform.parent);
                    nucleus.GetComponent<ShrinkOut>().enabled = true;
                    electron.SetActive(true);
                    BlowMeUp(0.25f, true);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Check(col);
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        Check(col);
    }

    void Update()
    {
        
    }
}
