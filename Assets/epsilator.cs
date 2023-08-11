using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class epsilator : MonoBehaviour
{

    public enum Direction
    {
        epsilon, zeta
    }

    public GameObject blockPrefab;

    public Direction direction;
    [Range(1,64)]
    public int stairBlockNum;
    [Range(1,64)]
    public int curveBlockNum;
    public float pivotSize;

    public float speed;

    public GameObject[] allBlocks;
    public float[] allBlockPositions; //define a curve from 0 to 1 which turns into the escalator

    public float blockSize = 32f;
    
    private const float sqrthalf = 0.70710678f;
    private const float qpi = 0.78539816f;
    private const float tqpi = 2.3561945f;

    private float totalCurveLen = 0f;

    void Start()
    {
        if (Application.isPlaying)
        {

        }
    }
    
    Vector2 curveMap(float x) //pill shape generator. this is hard to comprehend. i'm sorry
    {
        x = Mathf.Repeat(x, 1f);
        if (x < 0.5f)
        {
            Vector2 e1 = new Vector2(sqrthalf, sqrthalf);
            Vector2 e2 = new Vector2(-sqrthalf, sqrthalf);
            Vector2 pivotAngles = new Vector2(tqpi, -qpi);
            if (direction == Direction.zeta)
            {
                e1 = new Vector2(sqrthalf, -sqrthalf);
                e2 = new Vector2(sqrthalf, sqrthalf);
                pivotAngles = new Vector2(qpi, -tqpi);
            }

            Vector2 lineCenter = e2 * pivotSize;
            Vector2 lineExtent = e1 * stairBlockNum * blockSize * sqrthalf;

            Vector2 zeroPoint = lineCenter-lineExtent;
            Vector2 lineEndPoint = lineCenter+lineExtent;

            float rat = (float)stairBlockNum / (curveBlockNum+stairBlockNum);
            float lineEndRat = 0.5f*rat;

            if (x < lineEndRat)
            {
                return Vector2.Lerp(zeroPoint, lineEndPoint, x / lineEndRat);
            }

            float ang = Mathf.Lerp(pivotAngles.x, pivotAngles.y, Mathf.InverseLerp(lineEndRat, 0.5f, x));

            return lineExtent + new Vector2(pivotSize * Mathf.Cos(ang), pivotSize * Mathf.Sin(ang));
        }
        return -curveMap(x - 0.5f);
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < allBlocks.Length; i++)
            {
                if (allBlocks[i])
                {
                    allBlockPositions[i] += speed * Time.timeScale;
                    allBlockPositions[i] = Mathf.Repeat(allBlockPositions[i], 1f);
                    allBlocks[i].transform.localPosition = curveMap(allBlockPositions[i]);
                    Rigidbody2D r2 = allBlocks[i].GetComponent<Rigidbody2D>();
                    if (r2)
                    {
                        r2.MovePosition(allBlocks[i].transform.position);
                    }
                }
            }
        }
        else
        {
            int blockNum = (stairBlockNum + curveBlockNum) * 2;

            for (int i = 0; i < allBlocks.Length; i++)
            {
                DestroyImmediate(allBlocks[i]);
            }
            allBlocks = new GameObject[blockNum];
            allBlockPositions = new float[blockNum];
            for (int i = 0; i < allBlocks.Length; i++)
            {
                allBlocks[i] = Instantiate(blockPrefab);
                allBlocks[i].transform.SetParent(transform);
                allBlocks[i].transform.localRotation = Quaternion.identity;
                allBlockPositions[i] = (float)i / blockNum;
                allBlocks[i].transform.localPosition = curveMap(allBlockPositions[i]);
            }
        }
    }
}
