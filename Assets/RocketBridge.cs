using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RocketBridge : MonoBehaviour, IAmbushController
{
    public enum OrderingTechnique
    {
        ToTheRight, ToTheLeft, Random
    }

    public OrderingTechnique orderingTechnique;
    public double delayBeforeFirstFire;
    public double timeUntilNextFire;

    [HideInInspector]
    public List<RocketBlock> order;

    void Start()
    {
        foreach (Transform c in transform)
        {
            if (c.GetComponent<RocketBlock>())
            {
                order.Add(c.GetComponent<RocketBlock>());
            }
        }

        switch (orderingTechnique)
        {
            case OrderingTechnique.ToTheRight:
                order.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
                break;
            case OrderingTechnique.ToTheLeft:
                order.Sort((a, b) => (-a.transform.localPosition.x).CompareTo(-b.transform.localPosition.x));
                break;
            case OrderingTechnique.Random:
                order = order.OrderBy(a => Fakerand.Single()).ToList();
                break;
            default:
                break;
        }
    }

    public IEnumerator Main1()
    {
        double t1 = DoubleTime.ScaledTimeSinceLoad + delayBeforeFirstFire;
        yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));

        while (order.Count > 0)
        {
            order[0].on = true;
            order.RemoveAt(0);

            t1 = DoubleTime.ScaledTimeSinceLoad + timeUntilNextFire;
            yield return new WaitUntil(() => (DoubleTime.ScaledTimeSinceLoad >= t1));
        }

        yield return new WaitForEndOfFrame();
    }
    
    public void HaltFire()
    {
        StopAllCoroutines();
        Destroy(this);
    }

    public void Activate()
    {
        StartCoroutine(Main1());
    }

    public void OnAmbushBegin()
    {
        Activate();
    }

    public void OnAmbushComplete()
    {
        HaltFire();
    }

    private void Update()
    {
        if (transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
