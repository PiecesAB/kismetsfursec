using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimAttackSequencer : MonoBehaviour
{
    public GameObject[] attackHolders;
    public float startDelay;
    public Vector2 attackTime;
    public Vector2 waitTime;
    public float superWaitTime;
    public bool randomOrder;
    public int numberOfRepeats = -1;

    public Renderer mustBeOnScreenToContinue;

    public int curr = 0;

    void Start()
    {
        StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        yield return new WaitForSeconds(startDelay);
        while (true)
        {
            if (randomOrder)
            {
                for (int i = 0; i < attackHolders.Length - 1; ++i)
                {
                    int p = Fakerand.Int(i, attackHolders.Length);
                    GameObject temp = attackHolders[i];
                    attackHolders[i] = attackHolders[p];
                    attackHolders[p] = temp;
                }
            }

            for (curr = 0; curr < attackHolders.Length; ++curr)
            {
                if (mustBeOnScreenToContinue != null)
                {
                    if (!mustBeOnScreenToContinue.isVisible)
                    {
                        yield return new WaitUntil(() => mustBeOnScreenToContinue.isVisible);
                    }
                }

                GameObject fakeAttack = Instantiate(attackHolders[curr],
                    attackHolders[curr].transform.position,
                    attackHolders[curr].transform.rotation,
                    attackHolders[curr].transform.parent);

                fakeAttack.SetActive(true);

                if (attackTime.x == attackTime.y) { yield return new WaitForSeconds(attackTime.x); }
                else { yield return new WaitForSeconds(Fakerand.Single(attackTime.x, attackTime.y)); }

                Destroy(fakeAttack);

                if (waitTime.x == waitTime.y) {
                    if (waitTime.x == 0) { continue; }
                    else { yield return new WaitForSeconds(waitTime.x); }
                }
                else { yield return new WaitForSeconds(Fakerand.Single(waitTime.x, waitTime.y)); }

            }

            if (numberOfRepeats > 0){ --numberOfRepeats; }
            if (numberOfRepeats == 0) { yield break; }

            if (superWaitTime > 0) { yield return new WaitForSeconds(superWaitTime); }
        }
    }
}
