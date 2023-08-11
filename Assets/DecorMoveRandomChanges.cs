using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(primDecorationMoving))]
public class DecorMoveRandomChanges : MonoBehaviour
{
    private primDecorationMoving pdm;

    public Vector3 speedChangeMultiplier;
    public Vector2 minMaxChangeTime;
    public int changeCount = 0;

    void Start()
    {
        pdm = GetComponent<primDecorationMoving>();
        StartCoroutine(MinMaxChange());
    }

    private IEnumerator MinMaxChange()
    {
        while (gameObject && changeCount != 0)
        {
            yield return new WaitForSeconds(Fakerand.Single(minMaxChangeTime.x, minMaxChangeTime.y));
            pdm.v = new Vector3(pdm.v.x * speedChangeMultiplier.x, pdm.v.y * speedChangeMultiplier.y);
            if (changeCount > 0) { --changeCount; }
        }
        yield return null;
    }
}
