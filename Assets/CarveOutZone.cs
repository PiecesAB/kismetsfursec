using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarveOutZone : MonoBehaviour
{
    public Transform[] insetColliders;
    public SpriteRenderer mainSR; //to get the main block length and width

    private class TempBoxes1 : IComparable
    {
        public float beginX;
        public float endX;
        public float beginY;
        public float endY;
        public TempBoxes1(float _beginX, float _endX, float _beginY, float _endY)
        {
            beginX = _beginX; endX = _endX; beginY = _beginY; endY = _endY;
        }
        public int CompareTo(object other)
        {
            return beginX.CompareTo((other as TempBoxes1).beginX);
        }
    }

    private class RowInfo : IComparable
    {
        public float x;
        public List<Vector2> y; //components are low and high y bounds respectively.
        public void simplifyY()
        {
            y.Sort((a, b) => a.x.CompareTo(b.x));
            for (int i = 1; i < y.Count; ++i)
            {
                if (y[i].x <= y[i - 1].y) //ugh. it's just checking overlap. this could have been more clear
                {
                    y[i] = new Vector2(y[i - 1].x,y[i].y); //ugh
                    y.RemoveAt(i - 1);
                    --i;
                }
            }
        }
        public RowInfo(float _x)
        {
            x = _x;
            y = new List<Vector2>();
        }
        public int CompareTo(object other)
        {
            return x.CompareTo((other as RowInfo).x);
        }
    }

    private const float insetSizeMult = 16f;

    private TempBoxes1[] tb1;
    private List<RowInfo> ri;
    public bool changesEveryFrame = true;

    private int FindRowExact(float x)
    {
        for (int i = 0; i < ri.Count; ++i)
        {
            if (ri[i].x == x) { return i; }
        }
        return -1;
    }

    private int FindRowPrevious(float x)
    {
        int closestIndex = -1;
        float closest = Mathf.Infinity;
        for (int i = 0; i < ri.Count; ++i)
        {
            if (ri[i].x <= x && x - ri[i].x < closest)
            {
                closestIndex = i;
                closest = x - ri[i].x;
            }
        }
        return closestIndex;
    }
    
    private void MakeTempBoxes1()
    {
        tb1 = new TempBoxes1[insetColliders.Length];
        for (int i = 0; i < insetColliders.Length; ++i)
        {
            Transform itrn = insetColliders[i].transform;
            Vector3 ipos = itrn.position;
            float ispanx = 0.5f * insetSizeMult * itrn.localScale.x;
            float ispany = 0.5f * insetSizeMult * itrn.localScale.y;
            tb1[i] = new TempBoxes1(ipos.x - ispanx, ipos.x + ispanx, ipos.y - ispany, ipos.y + ispany);
        }
        //Array.Sort(tb1);
    }

    private void MakeRowInfo()
    {
        ri = new List<RowInfo>();
        float allMin = transform.position.x - (0.5f * mainSR.size.x);
        float allMax = transform.position.x + (0.5f * mainSR.size.x);
        float allLow = transform.position.y - (0.5f * mainSR.size.y);
        float allHigh = transform.position.y + (0.5f * mainSR.size.y);
        //make the basic rectangle
        ri.Add(new RowInfo(allMin));
        //ri[0].y.Add(new Vector2(allLow, allHigh));
        ri.Add(new RowInfo(allMax));
        //then make the cutouts
        for (int i = 0; i < tb1.Length; ++i)
        {
            float thisMin = tb1[i].beginX;
            //print(thisMin);
            float thisMax = tb1[i].endX;
            float thisLow = tb1[i].beginY;
            float thisHigh = tb1[i].endY;
            if (thisMin < allMin) { thisMin = allMin; }
            if (thisMax > allMax) { thisMax = allMax; }
            if (thisLow < allLow) { thisLow = allLow; }
            if (thisHigh > allHigh) { thisHigh = allHigh; }

            int findMin = FindRowExact(thisMin);
            if (findMin == -1)
            {
                int frp = FindRowPrevious(thisMin);
                ri.Add(new RowInfo(thisMin));
                if (frp != -1) { ri[ri.Count - 1].y = new List<Vector2>(ri[frp].y); }
                ri[ri.Count - 1].y.Add(new Vector2(thisLow, thisHigh));
            }
            else
            {
                ri[findMin].y.Add(new Vector2(thisLow, thisHigh));
            }

            //continulus
            for (int j = 0; j < ri.Count; ++j)
            {
                if (ri[j].x > thisMin && ri[j].x < thisMax)
                {
                    ri[j].y.Add(new Vector2(thisLow, thisHigh));
                }
            }

            int findMax = FindRowExact(thisMax);
            if (findMax == -1)
            {
                int frp = FindRowPrevious(thisMax);
                ri.Add(new RowInfo(thisMax));
                if (frp != -1) { ri[ri.Count - 1].y = new List<Vector2>(ri[frp].y); }
                ri[ri.Count - 1].y.Remove(new Vector2(thisLow, thisHigh));
                //ri[ri.Count - 1].y.Add(new Vector2(allLow, allHigh));
            }
        }

        //simplify junk
        ri.Sort();
        for (int i = 0; i < ri.Count; ++i)
        {
            ri[i].simplifyY();
        }
    }

    void MakeColliders()
    {
        float allMin = transform.position.x - (0.5f * mainSR.size.x);
        float allMax = transform.position.x + (0.5f * mainSR.size.x);
        float allLow = transform.position.y - (0.5f * mainSR.size.y);
        float allHigh = transform.position.y + (0.5f * mainSR.size.y);

        BoxCollider2D[] oldColliders = GetComponents<BoxCollider2D>();
        for (int i = 0; i < oldColliders.Length; ++i)
        {
            Destroy(oldColliders[i]);
        }

        for (int i = 0; i < ri.Count - 1; ++i)
        {
            RowInfo ix = ri[i];
            for (int j = 0; j < ix.y.Count + 1; ++j)
            {
                float ijminY = 0f;
                if (j == 0) { ijminY = allLow; }
                else { ijminY = ix.y[j - 1].y; }

                float ijmaxY = 0f;
                if (j == ix.y.Count) { ijmaxY = allHigh; }
                else { ijmaxY = ix.y[j].x; }

                if (ijmaxY - ijminY < 0.5f || ri[i + 1].x - ri[i].x < 0.5f)
                {
                    continue;
                }

                BoxCollider2D newCol = gameObject.AddComponent<BoxCollider2D>();
                newCol.size = new Vector2(ri[i + 1].x - ri[i].x, ijmaxY - ijminY);
                newCol.offset = transform.InverseTransformPoint(new Vector3((ri[i + 1].x + ri[i].x) * 0.5f, (ijmaxY + ijminY) * 0.5f));
                //newCol.usedByComposite = true;
            }
        }
    }

    void UpdateCollider()
    {
        MakeTempBoxes1();
        MakeRowInfo();
        MakeColliders();
    }

    void Start()
    {
        UpdateCollider();
    }

    void Update()
    {
        if (changesEveryFrame && mainSR.isVisible) { UpdateCollider(); }
    }
}
