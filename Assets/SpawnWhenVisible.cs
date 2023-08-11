using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWhenVisible : MonoBehaviour
{
    public GameObject[] whatToSpawn;
    public GameObject spawnEffect;
    public float delay = 0f;

    private IEnumerator Activate()
    {
        if (delay > 0f) { yield return new WaitForSeconds(delay); }
        for (int i = 0; i < whatToSpawn.Length; ++i)
        {
            StartCoroutine(AmbushController.Spawn(whatToSpawn[i], AmbushController.SpawnMode.Normal, spawnEffect));
        }
        yield return null;
    }

    private void OnBecameVisible()
    {
        StartCoroutine(Activate());
    }
}
