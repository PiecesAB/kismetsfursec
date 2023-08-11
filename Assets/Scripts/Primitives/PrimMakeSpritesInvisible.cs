using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimMakeSpritesInvisible : MonoBehaviour
{
    void Start()
    {
        foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>())
        {
            s.color = Color.clear;
        }
    }
}
