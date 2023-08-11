using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnitbulletMazeChooser : MonoBehaviour
{
    [System.Serializable]
    public struct ShrinkGroup
    {
        public Transform[] whatToShrink;
    }

    public ShrinkGroup[] shrinkGroups;
    public float shrinkTime = 1.5f;
    public float growBackWait = 3f;
    public float growBackTime = 1.5f;
    public int choice = -1;

    public Transform extraMover;

    private bool db = false;

    private IEnumerator ScaleSize(Transform trs, Vector3 targSize, float time)
    {
        double timeA = DoubleTime.ScaledTimeSinceLoad;
        Vector3 oldSize = trs.localScale;

        while (DoubleTime.ScaledTimeSinceLoad - timeA < time)
        {
            float rat = (float)(DoubleTime.ScaledTimeSinceLoad - timeA) / time;
            trs.localScale = Vector3.Lerp(oldSize, targSize, rat);

            yield return new WaitForEndOfFrame();
        }

        trs.localScale = targSize;

        yield return null;
    }

    private IEnumerator ActivateLoop()
    {
        if (db) { yield break; }
        db = true;

        if (choice == -1) { choice = Fakerand.Int(0, shrinkGroups.Length); }

        for (int i = 0; i < shrinkGroups.Length; ++i)
        {
            if (i != choice)
            {
                foreach (Transform t in shrinkGroups[i].whatToShrink)
                {
                    StartCoroutine(ScaleSize(t, Vector3.zero, shrinkTime));
                }
            }
        }

        yield return new WaitForSeconds(shrinkTime + growBackWait);

        for (int i = 0; i < shrinkGroups.Length; ++i)
        {
            if (i != choice)
            {
                foreach (Transform t in shrinkGroups[i].whatToShrink)
                {
                    StartCoroutine(ScaleSize(t, Vector3.one, growBackTime));
                }
            }
        }

        yield return new WaitForSeconds(growBackTime);

        db = false;
    }

    public void Activate()
    {
        if (!Application.isPlaying) { return; }
        StartCoroutine(ActivateLoop());
    }

    public void PlaceMover()
    {
        if (choice == -1 || extraMover == null) { return; }

        extraMover.position = shrinkGroups[choice].whatToShrink[0].position;
    }
}
