using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FibonacciSpiralBullet : MonoBehaviour
{
    public int count = 400;
    public float rotationAngle = 10.1664073f;
    public float baseRadius = 24f;

    public float timeToMove = 1f;
    public float timeToMoveMultiplier = 0.99f;

    public int[] spiralSkips;
    public Color[] spiralColors;
    private LineRenderer[][] lines;

    private Vector2[] positions;

    public LineRenderer sampleLine;
    public BulletHellMakerFunctions bulletMaker;
    private BulletObject[] myBullets;

    [HideInInspector]
    public bool moving = false;

    private IEnumerator Move(int index)
    {
        if (moving) { yield break; }
        moving = true;
        if (GetComponent<AudioSource>())
        {
            AudioSource a = GetComponent<AudioSource>();
            a.Stop();
            a.pitch = 1f / timeToMove;
            a.Play();
        }

        for (int i = 0; i < spiralSkips[index]; ++i)
        {
            if (myBullets[i] == null)
            {
                myBullets[i] = bulletMaker.MakeBulletForOtherObject();
                myBullets[i].originPosition = transform.TransformPoint(positions[i]);
            }
        } 

        for (int h = 0; h < lines.Length; ++h)
        {
            for (int i = 0; i < lines[h].Length; ++i)
            {
                lines[h][i].enabled = (h == index);
            }
        }

        for (int i = count - 1; i >= 0; --i)
        {
            if (i + spiralSkips[index] >= count || !BulletRegister.IsRegistered(myBullets[i]))
            {
                BulletRegister.MarkToDestroy(myBullets[i]);
                myBullets[i] = null;
                continue;
            }
            myBullets[i + spiralSkips[index]] = myBullets[i];
            if (myBullets[i] != null)
            {
                myBullets[i].color = Color.Lerp(spiralColors[index], Color.white, 0.6f);
                myBullets[i].renderGroup = new BulletControllerHelper.RenderGroup(myBullets[i].materialInternalIdx, myBullets[i].textureInternalIdx, myBullets[i].color);
            }
            myBullets[i] = null;
        }

        double t = DoubleTime.ScaledTimeSinceLoad;
        while (DoubleTime.ScaledTimeSinceLoad - t < timeToMove)
        {
            float rt = (float)(DoubleTime.ScaledTimeSinceLoad - t) / timeToMove;
            for (int i = 0; i < count; ++i)
            {
                if (myBullets[i] == null) { continue; }
                Vector2 lastPos = positions[i - spiralSkips[index]];
                if (lastPos == positions[i])
                {
                    BulletRegister.MarkToDestroy(myBullets[i]);
                    myBullets[i] = null;
                    continue;
                }
                myBullets[i].originPosition = Vector3.Lerp(
                    transform.TransformPoint(lastPos),
                    transform.TransformPoint(positions[i]),
                    rt);
            }
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < count; ++i)
        {
            if (myBullets[i] == null) { continue; }
            myBullets[i].originPosition = transform.TransformPoint(positions[i]);
        }

        moving = false;

        timeToMove *= timeToMoveMultiplier;
        StartCoroutine(Move(1 - index));
    }

    void Start()
    {
        positions = new Vector2[count];
        myBullets = new BulletObject[count];
        for (int i = 0; i < count; ++i)
        {
            myBullets[i] = null;
            float r = baseRadius * Mathf.Sqrt(i + 1);
            float a = rotationAngle * i;
            positions[i] = new Vector2(r * Mathf.Cos(a), r * Mathf.Sin(a));
            //Debug.DrawLine(positions[i], positions[i] + new Vector2(0, 2), Color.red, 1f);
        }

        lines = new LineRenderer[spiralSkips.Length][];
        for (int h = 0; h < spiralSkips.Length; ++h)
        {
            int spiralSkip = spiralSkips[h];
            lines[h] = new LineRenderer[spiralSkip];
            for (int i = 0; i < spiralSkip; ++i)
            {
                GameObject newLineObj = Instantiate(sampleLine.gameObject, transform);
                LineRenderer newLine = newLineObj.GetComponent<LineRenderer>();
                Vector3[] newPoints = new Vector3[(count - i) / spiralSkip];
                int npi = 0;
                for (int j = i; npi < newPoints.Length; j += spiralSkip)
                {
                    newPoints[npi++] = positions[j];
                }
                newLine.startColor = newLine.endColor = spiralColors[h];
                newLine.positionCount = newPoints.Length;
                newLine.SetPositions(newPoints);
                lines[h][i] = newLine;
            }
        }

        StartCoroutine(Move(1));

        Destroy(sampleLine);
    }
}
