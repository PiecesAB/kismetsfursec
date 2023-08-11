using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FliesGenerator : MonoBehaviour
{
    public GameObject flySample;
    public int flyCount;
    public float accumulationRadius = 32f;
    public float nearPlayerRadius = 32f;
    public float flySpeed = 3f; // how much they normally move in one frame (when away from target they move twice as fast)
    public float chanceToChangeTarget = 0.003f;

    private List<Vector2> targets;
    private List<Transform> flies;
    private List<int> flyTargets;
    private List<SpriteRenderer> flySprites;

    void Start()
    {
        targets = new List<Vector2>();
        Transform t = transform;
        for (int i = 0; i < t.childCount; ++i)
        {
            Transform ct = t.GetChild(i);
            targets.Add(ct.position);
            Destroy(ct.gameObject);
        }
        flies = new List<Transform>();
        flyTargets = new List<int>();
        flySprites = new List<SpriteRenderer>();
        for (int i = 0; i < flyCount; ++i)
        {
            int r = Fakerand.Int(0, targets.Count);
            GameObject nf = Instantiate(flySample, targets[r], Quaternion.identity, t);
            flies.Add(nf.transform);
            flyTargets.Add(r);
            flySprites.Add(nf.GetComponent<SpriteRenderer>());
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        Transform plrt = LevelInfoContainer.GetActiveControl()?.transform;
        for (int i = 0; i < flyCount; ++i)
        {
            // brownian motion like
            Vector2 fp = flies[i].position;
            Vector2 toTarget = targets[flyTargets[i]] - fp;
            bool nearTarget = toTarget.magnitude < accumulationRadius;
            float bdist = Fakerand.NormalDist(flySpeed * (nearTarget ? 1f : 2f), flySpeed * 0.5f);
            Vector2 bv = bdist * Fakerand.UnitCircle(true);
            if (!nearTarget)
            {
                bv = Vector2.Lerp(bv, bdist * toTarget.normalized, 0.7f);
            }
            bool nearPlayer = plrt ? (((Vector2)plrt.position - fp).magnitude < nearPlayerRadius) : false;
            if ((nearPlayer && nearTarget) || Fakerand.Single() < chanceToChangeTarget)
            {
                flyTargets[i] = Fakerand.Int(0, targets.Count);
            }
            flySprites[i].flipX = (bv.x < 0f);
            flies[i].position += (Vector3)bv;
        }
    }
}
