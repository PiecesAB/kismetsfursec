using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimFizzAllMaterials : MonoBehaviour
{
    public float fizz = 0f;
    private Material[] m = null;

    private void Update()
    {
        if (m == null) { m = GetComponent<Renderer>().materials; }
        for (int i = 0; i < m.Length; ++i) { m[i].SetFloat("_Fizz", fizz); }
    }
}
