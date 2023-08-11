using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimDestroyTogether : MonoBehaviour
{
    public GameObject[] toDelete;

    private void OnDestroy()
    {
        for (int i = 0; i < toDelete.Length; ++i)
        {
            GameObject g = toDelete[i];
            if (g.GetComponent<GenericBlowMeUp>())
            {
                g.GetComponent<GenericBlowMeUp>().BlowMeUp();
            }
            else
            {
                Destroy(g);
            }
        }
    }
}
