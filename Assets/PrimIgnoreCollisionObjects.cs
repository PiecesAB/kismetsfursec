using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimIgnoreCollisionObjects : MonoBehaviour
{
    public GameObject[] ignore;

    void Start()
    {
        Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < ignore.Length; ++i)
        {
            foreach (Collider2D c in ignore[i].GetComponentsInChildren<Collider2D>())
            {
                for (int j = 0; j < myCols.Length; ++j)
                {
                    Physics2D.IgnoreCollision(myCols[j], c);
                }
            }
        }
    }
}
