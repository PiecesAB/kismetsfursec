using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimRandomExister : MonoBehaviour
{
    private List<Transform> choices = new List<Transform>();

    void Start()
    {
        foreach (Transform t in transform)
        {
            if (t.parent != transform) { continue; }
            choices.Add(t);
        }

        Transform chosen = choices[Fakerand.Int(0, choices.Count)];
        chosen.SetParent(transform.parent, true);

        Destroy(gameObject);
    }
}
