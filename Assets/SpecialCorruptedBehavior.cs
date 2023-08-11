using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialCorruptedBehavior : MonoBehaviour
{
    void Start()
    {
        Destroy(FindObjectOfType<PauseMenuMain>());
        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop()
    {
        yield return new WaitForSeconds(5f);

    }
}
