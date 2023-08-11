using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrimGenVersion2 : MonoBehaviour
{

    public int currentCount;
    public double nextSpawnTime;
    [Header("-----------------------------------------")]
    public GameObject[] spawnEffects;
    public GameObject[] objectKindsToSpawn;
    public double spawnEffectTime;
    public int minToSpawnAtOnce;
    public int maxToSpawnAtOnce;
    public double minTimeToNextSpawn;
    public double maxTimeToNextSpawn;
    public BoxCollider2D spawnRange;
    public int maxCount;
    public bool spawnOnlyIfPlayerInRange;
    public Transform parentOfSpawnedThings;
    public float objSize = 8f;
    public bool mustSpawnOnScreen = true;
    public bool countObjectsThatWereDeleted = false;
    [Header("-----------------------------------------")]
    public TextMesh printerTextOptional;
    public Transform printerArm1;
    public Transform printerArm2;


    private bool plrInRange;
    private List<GameObject> plrInRangeTracker;

    private List<GameObject> whatISpawnedIn;

    private bool inSpawnLoop;

    public IEnumerator SpawnLoop()
    {
        if ((plrInRange || !spawnOnlyIfPlayerInRange) && whatISpawnedIn.Count < maxCount)
        {
            //pick positions, round by objSize units
            Rect spawnRect = new Rect(spawnRange.offset.x - (spawnRange.size.x * 0.5f),
                                      spawnRange.offset.y - (spawnRange.size.y * 0.5f),
                                      spawnRange.size.x,
                                      spawnRange.size.y);
            int countToSpawnNow = Fakerand.Int(minToSpawnAtOnce, maxToSpawnAtOnce + 1);
            if (countToSpawnNow <= 0)
            {
                goto EndOfThisLoop;
            }
            if (countToSpawnNow + whatISpawnedIn.Count > maxCount)
            {
                countToSpawnNow = maxCount - whatISpawnedIn.Count;
            }
            List<Vector2> pickedPositions = new List<Vector2>(countToSpawnNow);
            for (int i = 0; i < countToSpawnNow; i++)
            {
                int tries = 0;
                Vector2 candidate = Vector2.zero;
                while (tries < 5) //try not to overlap
                {
                    candidate = new Vector2(Mathf.Round(Fakerand.Single(spawnRect.xMin, spawnRect.xMax) /objSize) * objSize,
                                            Mathf.Round(Fakerand.Single(spawnRect.yMin, spawnRect.yMax) /objSize) * objSize);
                    if (pickedPositions.Count == 0 || !pickedPositions.Any(v => (v == candidate)))
                    {
                        break;
                    }
                    tries++;
                }

                Vector2 screenTest = Camera.main.WorldToViewportPoint(transform.TransformPoint(candidate));
                if (mustSpawnOnScreen)
                {
                    if (screenTest.x == Mathf.Clamp01(screenTest.x) && screenTest.y == Mathf.Clamp01(screenTest.y))
                    {
                        pickedPositions.Add(candidate);
                    }
                }
                else
                {
                    pickedPositions.Add(candidate);
                }
            }

            //determine object type to spawn and make spawn particles
            int[] pickedTypes = new int[pickedPositions.Count];
            for (int i = 0; i < pickedPositions.Count; i++)
            {
                pickedTypes[i] = Fakerand.Int(0, objectKindsToSpawn.Length);
                GameObject g = Instantiate(spawnEffects[pickedTypes[i]], transform.TransformPoint(pickedPositions[i]), transform.rotation, parentOfSpawnedThings);
            }
            nextSpawnTime += spawnEffectTime;
            yield return new WaitUntil(() => DoubleTime.ScaledTimeSinceLoad > nextSpawnTime);
            //make objects
            for (int i = 0; i < pickedPositions.Count; i++)
            {
                GameObject g = Instantiate(objectKindsToSpawn[pickedTypes[i]], transform.TransformPoint(pickedPositions[i]), transform.rotation, parentOfSpawnedThings);
                if (!g.activeSelf)
                {
                    g.SetActive(true);
                }
                whatISpawnedIn.Add(g);
                currentCount++;
            }
        }
        EndOfThisLoop:
        nextSpawnTime += Fakerand.Double(minTimeToNextSpawn, maxTimeToNextSpawn);
        yield return new WaitForEndOfFrame();
        inSpawnLoop = false;
    }

    private void Start()
    {
        whatISpawnedIn = new List<GameObject>();
        plrInRangeTracker = new List<GameObject>();
        nextSpawnTime = DoubleTime.ScaledTimeSinceLoad + Fakerand.Double(minTimeToNextSpawn, maxTimeToNextSpawn);
        GetComponent<SpriteRenderer>().color = Color.clear;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        GameObject g = col.gameObject;
        if (g.CompareTag("Player"))
        {
            plrInRangeTracker.Add(g);
        }
        plrInRange = (plrInRangeTracker.Count > 0);
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        plrInRangeTracker.Remove(col.gameObject);
        plrInRange = (plrInRangeTracker.Count > 0);
    }

    private static readonly string[] printerLowTexts = new string[5] { "0", "+", "++", "+++", "++++" };

    void Update()
    {
        if (!countObjectsThatWereDeleted)
        {
            for (int i = 0; i < whatISpawnedIn.Count; i++)
            {
                if (whatISpawnedIn[i] == null)
                {
                    whatISpawnedIn.RemoveAt(i);
                    i--;
                    currentCount--;
                }
            }
        }

        if (DoubleTime.ScaledTimeSinceLoad > nextSpawnTime && !inSpawnLoop)
        {
            inSpawnLoop = true;
            StartCoroutine(SpawnLoop());
        }

        if (printerTextOptional)
        {
            int remain = maxCount - currentCount;
            if (remain < 5)
            {
                printerTextOptional.text = printerLowTexts[remain];
            }
            else
            {
                printerTextOptional.text = remain.ToString();
            }
        }
        if (printerArm1)
        {
            printerArm1.localPosition = new Vector3(0, 0, -2f + (float)DoubleTime.DoublePong(DoubleTime.ScaledTimeSinceLoad * 30.0, 13.0));
            if (printerArm2)
            {
                printerArm2.localPosition = new Vector3(-8f + (float)DoubleTime.DoublePong(DoubleTime.ScaledTimeSinceLoad * 30.0, 16.0), 0, 0);
            }
        }
    }
}
