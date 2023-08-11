using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E8DecorTrinkets : MonoBehaviour
{
    public Sprite[] trinketSprites;
    public Transform trinketParent;

    void Start()
    {
        foreach (SpriteRenderer s in trinketParent.GetComponentsInChildren<SpriteRenderer>())
        {
            s.sprite = trinketSprites[Fakerand.Int(0, trinketSprites.Length)];
        }
    }
}
