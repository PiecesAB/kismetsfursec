using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//[ExecuteInEditMode]
public class SwitchBlockBetter : MonoBehaviour {

    public bool on;
    public bool inverted;
    [Range(0, 31)]
    public int ID;
    public bool color = true;

    private float rangeLol;

    private uint realID;
    private bool internalinv; //for test

    private Renderer myRend;

    private static HashSet<SwitchBlockBetter> all = new HashSet<SwitchBlockBetter>();

    public static void ChangedAll(uint mask)
    {
        foreach (SwitchBlockBetter s in all)
        {
            s.Changed(mask);
        }
    }

    private bool WouldTurnSolidNow(uint mask)
    {
        return (!inverted && ((realID & mask) != 0)) || (inverted && ((realID & mask) == 0));
    }

    private void DoChanged(uint mask)
    {
        if (WouldTurnSolidNow(mask))
        {
            if (!GetComponent<SwitchButtonBhvrs>())
            {
                if (GetComponentInChildren<BoxCollider2D>(true))
                    GetComponentInChildren<BoxCollider2D>(true).enabled = true;
            }

            {
                if (GetComponentInChildren<LineRenderer>(true))
                    GetComponentInChildren<LineRenderer>(true).enabled = true;

                if (GetComponent<Collider2D>())
                    GetComponent<Collider2D>().enabled = true;

                if (GetComponent<GunBehaviors>())
                {
                    GetComponent<GunBehaviors>().enabled = true;
                    if (GetComponent<GunBehaviors>().gunType == GunBehaviors.Type.MachineGun)
                        GetComponent<GunBehaviors>().StartCoroutine(GetComponent<GunBehaviors>().FireMachine());
                }
            }
            on = true;
        }
        else
        {
            if (!GetComponent<SwitchButtonBhvrs>())
            {
                if (GetComponentInChildren<BoxCollider2D>(true))
                    GetComponentInChildren<BoxCollider2D>(true).enabled = false;
            }

            {
                if (GetComponentInChildren<LineRenderer>(true))
                    GetComponentInChildren<LineRenderer>(true).enabled = false;

                if (GetComponent<Collider2D>())
                    GetComponent<Collider2D>().enabled = false;

                if (GetComponent<GunBehaviors>())
                {
                    GetComponent<GunBehaviors>().enabled = false;
                    GetComponent<GunBehaviors>().StopAllCoroutines();
                }
            }
            on = false;
        }
    } 
    
	public void Changed(uint mask)
    {
        //if (Application.isPlaying)
        //{

        //}

        if (insideCount == 0 || !WouldTurnSolidNow(mask))
        {
            DoChanged(mask);
        } else
        {
            StartCoroutine(ScheduleChanged(mask));
        }
    }

    private int scheduleChangedId = 0;

    private IEnumerator ScheduleChanged(uint mask)
    {
        int currId = ++scheduleChangedId;
        while (currId == scheduleChangedId)
        {
            if (insideCount == 0)
            {
                DoChanged(mask);
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    void Update()
    {
        //if (Application.isPlaying)
        {
            if (myRend && myRend.isVisible)
            {
                if (internalinv != inverted)
                {
                    internalinv = inverted;
                    uint masc = Utilities.loadedSaveData.switchMask;
                    Changed(masc);
                }

                if (on)
                {
                    rangeLol = Mathf.Lerp(rangeLol, 1f, 0.1f);
                }
                else
                {
                    rangeLol = Mathf.Lerp(rangeLol, 0f, 0.1f);
                }

                Color c = Color.white;
                if (color)
                {
                    c = Utilities.colorCycle[ID];
                }
                myRend.material.SetColor("_Color", new Color(1, 1, 1, Mathf.Lerp(0.3f, 1.0f, rangeLol)));
                myRend.material.SetColor("_RepColor", c);
                float z = (1f - rangeLol);
                //GetComponent<Renderer>().material.SetVector("_Speed", new Vector4(z * 2f, z * 2f, 0, 0));
                myRend.material.SetVector("_Intensity", new Vector4(z * 0.03f, z * 0.03f, 0, 0));
            }
        }
        /*else //be careful this leaks materials
        {
            Material mat = new Material(myRend.sharedMaterial);
            Color c = Color.white;
            if (color)
            {
                c = Utilities.colorCycle[ID];
            }
            myRend.material = mat;
            mat.SetColor("_Color", Color.white);
            mat.SetColor("_RepColor", c);
        }*/
    }

	void Start () {
        if (Application.isPlaying)
        {
            all.Add(this);
            internalinv = inverted;
            realID = 1u << ID;
            rangeLol = 0f;
            uint masc = Utilities.loadedSaveData.switchMask;
            if ((!inverted && ((realID & masc) != 0)) || (inverted && ((realID & masc) == 0)))
            {
                rangeLol = 1f;
            }
            Changed(masc);
        }
        myRend = GetComponent<Renderer>();
    }

    private int insideCount = 0;

    private bool CollisionIsValid(Collider2D col)
    {
        Rigidbody2D r2 = col.gameObject.GetComponent<Rigidbody2D>();
        return r2 && !r2.isKinematic;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (CollisionIsValid(col))
        {
            ++insideCount;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (CollisionIsValid(col))
        {
            --insideCount;
        }
    }
}
