using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyCloneMaker : MonoBehaviour
{
    public float nextCloneDist = 256f;
    public GameObject sampleCloneObject;
    public SpriteRenderer mainGraphic;
    public SpriteRenderer[] subGraphics;
    public SpriteRenderer flash;

    private const float graphicRadius = 64f;
    private float graphicY;
    private float graphicMinLevel;
    private float graphicMaxLevel;

    public static int clonesMadeThisLevel;

    private Transform currPlr;
    private SpriteRenderer currSpr;
    private List<Vector3> pastPositions = new List<Vector3>();
    private List<Sprite> pastSprites = new List<Sprite>();
    private List<bool> pastFlipX = new List<bool>();

    private List<int> cloneIndices = new List<int>();
    private List<Transform> cloneTransforms = new List<Transform>();
    private List<SpriteRenderer> cloneSprs = new List<SpriteRenderer>();

    private Vector3 lastPosCopy;

    private int maxClones;
    private const float stepDist = 4f;
    private int stepsTillClone;
    private int currStepsTillClone;

    private float stepProgress;
    private Vector3 lastFramePos;

    private Color flashColor;

    private void OnLevelWasLoaded(int level)
    {
        clonesMadeThisLevel = 0;
    }

    void Start()
    {
        stepProgress = 0;
        graphicY = mainGraphic.transform.position.y;
        graphicMinLevel = graphicY - graphicRadius;
        graphicMaxLevel = graphicY + graphicRadius;

        maxClones = Mathf.FloorToInt(65536 * stepDist / nextCloneDist);
        stepsTillClone = currStepsTillClone = Mathf.FloorToInt(nextCloneDist / stepDist);

        currPlr = null;
        currSpr = null;

        sampleCloneObject.SetActive(false);

        flash.gameObject.SetActive(true);
        flash.transform.SetParent(Camera.main.transform, false);
        flash.transform.localPosition = new Vector3(0, 0, -500);
        flashColor = flash.color;
    }

    private void RemoveFirst()
    {
        pastPositions.RemoveAt(0);
        pastSprites.RemoveAt(0);
        for (int i = 0; i < cloneIndices.Count; ++i) { --cloneIndices[i]; }
    }

    private void MakeClone()
    {
        if (cloneIndices.Count >= maxClones) { return; }

        GameObject newClone = Instantiate(sampleCloneObject, pastPositions[0], Quaternion.identity, transform);
        newClone.SetActive(true);
        SpriteRenderer newSpr = newClone.GetComponent<SpriteRenderer>();
        newSpr.sprite = pastSprites[0];

        cloneIndices.Add(0);
        cloneTransforms.Add(newClone.transform);
        cloneSprs.Add(newSpr);

        ++clonesMadeThisLevel;
    }

    private void MoveClones()
    {
        for (int i = 0; i < cloneIndices.Count; ++i)
        {
            if (cloneTransforms[i] == null)
            {
                cloneIndices.RemoveAt(i);
                cloneTransforms.RemoveAt(i);
                cloneSprs.RemoveAt(i);
                --i; continue;
            }

            int k = cloneIndices[i];
            if (k >= pastPositions.Count - 1 || k < 0) { continue; }
            cloneTransforms[i].position = Vector3.Lerp(pastPositions[k], pastPositions[k + 1], stepProgress);
            cloneSprs[i].sprite = pastSprites[k];
            cloneSprs[i].flipX = pastFlipX[k];
        }
    }

    // careful: something may go wrong with teleporters involved.
    void Update()
    {
        if (currPlr == null)
        {
            currPlr = LevelInfoContainer.GetActiveControl().transform;
            currSpr = currPlr.GetComponent<SpriteRenderer>();
            pastPositions.Add(currPlr.position);
            lastPosCopy = currPlr.position;
            pastSprites.Add(currSpr.sprite);
            pastFlipX.Add(currSpr.flipX);

            lastFramePos = currPlr.position;
        }

        if (Time.timeScale == 0) { return; }

        stepProgress += (currPlr.position - lastFramePos).magnitude / stepDist;
        flash.color = new Color(flashColor.r, flashColor.g, flashColor.b, Mathf.Clamp01(flash.color.a - 0.1f));

        while (stepProgress >= 1f)
        {
            lastPosCopy = Vector3.MoveTowards(lastPosCopy, currPlr.position, stepDist);
            pastPositions.Add(lastPosCopy);
            pastSprites.Add(currSpr.sprite);
            pastFlipX.Add(currSpr.flipX);
            stepProgress -= 1f;

            for (int i = 0; i < cloneIndices.Count; ++i)
            {
                ++cloneIndices[i];
            }

            --currStepsTillClone;
            if (currStepsTillClone == 0)
            {
                currStepsTillClone = stepsTillClone;
                flash.color = flashColor;
                MakeClone();
            }
        }

        while (pastPositions.Count > 65536)
        {
            RemoveFirst();
        }

        MoveClones();

        lastFramePos = currPlr.position;

        float currY = Mathf.Lerp(graphicMinLevel, graphicMaxLevel, (nextCloneDist - 4*(currStepsTillClone - 1) + stepProgress)/nextCloneDist );
        mainGraphic.material.SetFloat("_Prog", currY);
        for (int i = 0; i < subGraphics.Length; ++i)
        {
            subGraphics[i].material.SetFloat("_Prog", currY);
        }
    }
}
