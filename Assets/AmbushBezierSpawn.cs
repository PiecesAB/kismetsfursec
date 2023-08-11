using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbushBezierSpawn : MonoBehaviour
{
    [System.Serializable]
    public class SpawnDescriptor
    {
        public float time;
        public int repeats;
        public float repeatDelay;
        public float speed;
        public GameObject originalObject;
        public PrimBezierMove mover;
        public bool reversed;

        public SpawnDescriptor()
        {
            time = 0f;
            repeats = 1;
            repeatDelay = 1f;
            speed = 1f;
            originalObject = null;
            mover = null;
            reversed = false;
        }
    }

    public float startDelay;
    public SpawnDescriptor[] spawns;

    private const float spawnEffectTime = 1f;

    private int unborn = 0;

    private IEnumerator SpawnEffectAndMake(SpawnDescriptor spawn, GameObject spawnEffect = null)
    {
        if (spawnEffect != null)
        {
            if (spawnEffect)
            {
                Instantiate(spawnEffect, transform.position, Quaternion.identity);
            }
            double t1 = DoubleTime.ScaledTimeSinceLoad + spawnEffectTime;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }
        // The parent is the ambush, because this object should be in the ambush too
        GameObject clone = Instantiate(spawn.originalObject, transform.position, transform.rotation, transform.parent);
        clone.SetActive(true);
        spawn.mover.InsertObjectAtBeginning(clone.transform, spawn.speed, spawn.reversed);
        --unborn;
        yield return null;
    }

    private IEnumerator ExecuteSpawn(SpawnDescriptor spawn, GameObject spawnEffect = null)
    {
        if (spawn.originalObject == null || spawn.mover == null) { yield break; }
        if (spawn.time > 0f) { yield return new WaitForSeconds(spawn.time); }
        int unborn = spawn.repeats;
        for (int i = 0; i < spawn.repeats; ++i)
        {
            StartCoroutine(SpawnEffectAndMake(spawn, spawnEffect));
            double t1 = DoubleTime.ScaledTimeSinceLoad + spawn.repeatDelay;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }
        yield return null;
    }

    public IEnumerator Activate(GameObject spawnEffect = null)
    {
        if (startDelay > 0f) { yield return new WaitForSeconds(startDelay); }
        double startTime = DoubleTime.ScaledTimeSinceLoad;

        for (int i = 0; i < spawns.Length; ++i)
        {
            unborn += spawns[i].repeats;
            StartCoroutine(ExecuteSpawn(spawns[i], spawnEffect));
        }

        yield return new WaitUntil(() => unborn == 0);
        Destroy(gameObject);
    }
    
}
