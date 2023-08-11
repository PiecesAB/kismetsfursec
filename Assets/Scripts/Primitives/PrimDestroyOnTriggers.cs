using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimDestroyOnTriggers : MonoBehaviour, ITripwire, IAmbushController
{
    private void Bye()
    {
        if (GetComponent<GenericBlowMeUp>())
        {
            GetComponent<GenericBlowMeUp>().BlowMeUp();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnTrip()
    {
        Bye();
    }

    public void OnAmbushBegin()
    {
    }

    public void OnAmbushComplete()
    {
        Bye();
    }
}
