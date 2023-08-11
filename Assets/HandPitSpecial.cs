using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPitSpecial : MonoBehaviour
{
    public LineRenderer sampleLine;

    public float baseY;
    public float leftX;
    public float rightX;
    public float spacing;
    public float meanHeight;
    public float sdHeight;
    public float minHeight;
    public float maxHeight;
    public float leftSprite;
    public float rightSprite;
    [Range(0f, 0.5f)]
    public float animSpeed = 0.05f;

    public Sprite[] spriteProgression;

    private List<Vector3> origins = new List<Vector3>();

    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<SpriteRenderer> handSprites = new List<SpriteRenderer>();

    private List<float> heights = new List<float>();

    private int WeighedRound(float x)
    {
        if (Fakerand.Single() < x % 1) { return Mathf.FloorToInt(x); }
        return Mathf.CeilToInt(x);
    }

    void MakeLine(float x, float y)
    {
        GameObject newLineObj = Instantiate(sampleLine.gameObject, sampleLine.transform.parent);
        LineRenderer newLine = newLineObj.GetComponent<LineRenderer>();
        float newHt = Fakerand.NormalDist(meanHeight, sdHeight, minHeight, maxHeight);

        lines.Add(newLine);
        newLine.positionCount = 3;
        Vector3 endLine = new Vector3(0, newHt, 0);
        newLine.SetPositions(new Vector3[3]{
            Vector3.zero,
            Vector3.Lerp(Vector3.zero, endLine, 0.8f),
            endLine
        });

        
        heights.Add(newHt);
        newLineObj.transform.position = new Vector3(x, y, sampleLine.transform.position.z);
        Transform newHandTr = newLineObj.transform.GetChild(0);
        newHandTr.localPosition = new Vector3(0, newHt, 0);
        SpriteRenderer newHandSr = newHandTr.GetComponent<SpriteRenderer>();
        handSprites.Add(newHandSr);
        newHandSr.sprite = spriteProgression[Mathf.Clamp(WeighedRound(Fakerand.Single(leftSprite, rightSprite)), 0, spriteProgression.Length - 1)];
    }

    void Start()
    {
        leftSprite = Mathf.Max(leftSprite, 0);
        rightSprite = Mathf.Min(rightSprite, spriteProgression.Length - 1);
        for (float i = leftX; i <= rightX; i += spacing)
        {
            MakeLine(i, baseY);
        }
    }

    private int GetHand(Sprite x)
    {
        for (int i = 0; i < spriteProgression.Length; ++i) { if (spriteProgression[i] == x) { return i; } }
        return -1;
    }

    private Sprite GetSprite(int x)
    {
        return spriteProgression[Mathf.Clamp(x, Mathf.FloorToInt(leftSprite), Mathf.CeilToInt(rightSprite))];
    }

    void Update()
    {
        Transform activePlrPos = LevelInfoContainer.GetActiveControl().transform;

        for (int i = 0; i < handSprites.Count; ++i)
        {
            if (!handSprites[i].isVisible) { continue; }
            int h = GetHand(handSprites[i].sprite);
            float transition = Fakerand.Single();
            if (transition < animSpeed) { handSprites[i].sprite = GetSprite(h-1); }
            if (transition > 1f - animSpeed) { handSprites[i].sprite = GetSprite(h + 1); }
            Transform handTr = handSprites[i].transform;
            Vector3 dif = activePlrPos.position - handTr.position;
            float noise1 = 0.2f * Mathf.PerlinNoise((float)(DoubleTime.ScaledTimeSinceLoad % 50.0), handTr.position.x) - 0.12f;
            float noise2 = 5f * Mathf.PerlinNoise((float)(DoubleTime.ScaledTimeSinceLoad*0.3 % 50.0), handTr.position.x) - 0.6f;
            handTr.eulerAngles =  new Vector3(0, 0, Mathf.LerpAngle(handTr.eulerAngles.z, Mathf.Rad2Deg * Mathf.Atan2(dif.y, dif.x) - 90f, Mathf.Clamp01(noise1)));
            handTr.position = Vector3.MoveTowards(handTr.position, activePlrPos.position, Mathf.Max(0f, noise2));
            if (handTr.localPosition.magnitude > heights[i])
            {
                handTr.localPosition = handTr.localPosition.normalized * heights[i];
            }
            lines[i].SetPosition(2, handTr.localPosition);
            lines[i].SetPosition(1, lines[i].GetPosition(2) - 8f*(new Vector3(-Mathf.Sin(handTr.eulerAngles.z*Mathf.Deg2Rad), Mathf.Cos(handTr.eulerAngles.z * Mathf.Deg2Rad))));
            
        }
    }
}
