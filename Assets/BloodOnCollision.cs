using UnityEngine;
using System.Collections;

public class BloodOnCollision : MonoBehaviour {

    //make sure there's less than 256 blood splatters by using magic and a stupid amount of memory

    public static int count = 0;
    public static GameObject[] objs = new GameObject[256];

    private void Start()
    {
        count = count % 256;
        if (objs[count] != null)
        {
            Destroy(objs[count]);
        }
        objs[count] = gameObject;
        count++;
    }

}
