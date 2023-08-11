using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimRandomChooser : MonoBehaviour
{
    public GameObject[] objects;

    public GameObject Choose()
    {
        return objects[Fakerand.Int(0, objects.Length)];
    }
}
