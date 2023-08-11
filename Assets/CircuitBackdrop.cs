using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitBackdrop : MonoBehaviour
{
    public struct GridCoord : IEquatable<GridCoord>
    {
        public int x;
        public int y;
        public GridCoord(int _x, int _y)
        {
            x = _x; y = _y;
        }
        public bool Equals(GridCoord other)
        {
            return other.x == x && other.y == y;
        }
        public Vector2 LocalPosition()
        {
            float cellWidth = ((float)worldWidth) / (gridWidth - 1);
            float cellHeight = ((float)worldHeight) / (gridHeight - 1);
            Vector2 offset = new Vector2(-worldWidth / 2, -worldHeight / 2);
            return offset + new Vector2(x * cellWidth, y * cellHeight);
        }
    }

    private const int gridWidth = 16;
    private const int gridHeight = 13;
    private const int desiredPathLength = 16;
    private const int worldWidth = 320 + 8;
    private const int worldHeight = 216 + 8;
    private const float scrollYA = 64;
    private const float scrollYF = 3.1415926f / 4f;

    private List<List<GridCoord>> paths = new List<List<GridCoord>>();

    public GameObject sampleLine;
    public GameObject sampleChip;

    void Start()
    {
        transform.SetParent(Camera.main.transform, true);

        int num = gridHeight * gridWidth;
        int i = 0;
        List<GridCoord> getList = new List<GridCoord>(num);
        HashSet<GridCoord> inList = new HashSet<GridCoord>();
        // Populate
        i = 0;
        for (int y = 0; y < gridHeight; ++y)
        {
            for (int x = 0; x < gridWidth; ++x)
            {
                getList.Add(new GridCoord(x, y));
                ++i;
            }
        }

        // Randomize
        for (i = 0; i < num - 1; ++i)
        {
            int r = Fakerand.Int(i, num);
            GridCoord temp = getList[r];
            getList[r] = getList[num - 1];
            getList[num - 1] = temp;
        }

        // Design paths
        List<GridCoord> currPath = new List<GridCoord>();
        while (getList.Count > num / 4)
        {
            GridCoord p = getList[getList.Count - 1]; // Not a minimum. The order is random.
            if (inList.Contains(p))
            {
                getList.RemoveAt(getList.Count - 1);
                continue;
            }
            getList.RemoveAt(getList.Count - 1);

            bool cornerAdded = false;
            if (currPath.Count >= 1)
            {
                GridCoord prev = currPath[currPath.Count - 1];
                if (currPath.Count >= 2 
                    && ((currPath[currPath.Count - 2].x == prev.x && prev.x == p.x) 
                    || (currPath[currPath.Count - 2].y == prev.y && prev.y == p.y)))
                {
                    // colinear
                }
                else if (prev.x == p.x || prev.y == p.y) {
                    cornerAdded = true; // A white lie. We don't need a corner.
                } 
                else
                {
                    GridCoord[] corners = { new GridCoord(p.x, prev.y), new GridCoord(prev.x, p.y) };
                    if (Fakerand.Int(0, 2) == 0) { GridCoord temp = corners[0]; corners[0] = corners[1]; corners[1] = temp; }
                    if (!inList.Contains(corners[0]))
                    {
                        currPath.Add(corners[0]);
                        inList.Add(corners[0]);
                        cornerAdded = true;
                    }
                    else if (!inList.Contains(corners[1]))
                    {
                        currPath.Add(corners[1]);
                        inList.Add(corners[1]);
                        cornerAdded = true;
                    }
                    // Get here = cannot add corner :(
                }
            }
            else
            {
                currPath.Add(p);
                inList.Add(p);
                continue;
            }

            if (!cornerAdded)
            {
                if (currPath.Count > 1) { paths.Add(currPath); }
                currPath = new List<GridCoord>();
            }

            currPath.Add(p);
            inList.Add(p);

            if (currPath.Count >= desiredPathLength)
            {
                paths.Add(currPath);
                currPath = new List<GridCoord>();
            }
        }

        // Draw
        for (i = 0; i < paths.Count; ++i)
        {
            List<GridCoord> path = paths[i];
            GameObject newlineObj = Instantiate(sampleLine, sampleLine.transform.position, Quaternion.identity, sampleLine.transform.parent);
            LineRenderer newline = newlineObj.GetComponent<LineRenderer>();
            Material mat = newline.material;
            Color c = mat.GetColor("_ColT");
            float ch, cs, cv;
            Color.RGBToHSV(c, out ch, out cs, out cv);
            float nh = Mathf.Repeat(ch + Fakerand.Single(-0.1f, 0.35f), 1f);
            mat.SetColor("_ColT", Color.HSVToRGB(nh, cs, cv));
            newline.material = mat;
            Vector3[] positions = new Vector3[path.Count];
            for (int j = 0; j < path.Count; ++j) { positions[j] = path[j].LocalPosition(); }
            newline.positionCount = path.Count;
            newline.SetPositions(positions);
            if (Fakerand.Int(0, 5) == 0)
            {
                //chip
                GameObject newchipObj = Instantiate(sampleChip, sampleChip.transform.position, Quaternion.identity, sampleChip.transform.parent);
                newchipObj.transform.localPosition = path[0].LocalPosition();
                newchipObj.transform.localScale = Vector3.one * Fakerand.Single(0.3f, 0.75f);
                newchipObj.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(Fakerand.Single(0.13f, 0.55f), 0.75f, 0.5f);
            }
        }
    }

    void Update()
    {
    }
}
