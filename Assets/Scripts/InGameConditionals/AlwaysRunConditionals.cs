using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for certain conditionals that fiddle with colliders
public class AlwaysRunConditionals : MonoBehaviour
{
    private InGameConditional[] run;

    void Update()
    {
        run = run ?? GetComponents<InGameConditional>();
        for (int i = 0; i < run.Length; ++i)
        {
            run[i].Evaluate();
        }
    }
}
