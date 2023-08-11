using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class ActiveGridPositionFiller : MonoBehaviour
{
    public Vector2 offset;
    public Vector2 size;

    public static List<List<Vector2>> positions = new List<List<Vector2>>();
    private static List<ActiveGridPositionFiller> objects = new List<ActiveGridPositionFiller>();
    private static int mainIndex = -1;
    private int myIndex = -1;

    private Vector2 lastPos;

    private void Start()
    {
        myIndex = ++mainIndex;
        objects.Add(this);
        positions.Add(new List<Vector2>());
    }

    private void OnDestroy()
    {
        --mainIndex;
        objects.RemoveAt(myIndex);
        positions.RemoveAt(myIndex);
        for (int i = myIndex; i < objects.Count; ++i)
        {
            --objects[i].myIndex;
        }
    }

    public static Vector2 center;

    public class MagnitudeComparer : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            return (x - center).sqrMagnitude.CompareTo((y - center).sqrMagnitude);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (((Vector2)transform.position - lastPos).sqrMagnitude < 4f) { return; }
        center = (Vector2)transform.position + offset;
        SortedSet<Vector2> visited = new SortedSet<Vector2>(new MagnitudeComparer()) { };
        SortedSet<Vector2> nextVisit = new SortedSet<Vector2>(new MagnitudeComparer()) {
            new Vector2(Mathf.Round(center.x / 16f) * 16f, Mathf.Round(center.y / 16f) * 16f)
        };
        positions[myIndex].Clear();
        while (nextVisit.Count > 0)
        {
            Vector2 current = nextVisit.Min;
            if (visited.Contains(current))
            {
                nextVisit.Remove(current);
                continue;
            }

            if (Mathf.Abs(current.x - center.x) <= size.x 
                && Mathf.Abs(current.y - center.y) <= size.y)
            {
                positions[myIndex].Add(current);
                nextVisit.Add(current + new Vector2(16, 0));
                nextVisit.Add(current + new Vector2(0, 16));
                nextVisit.Add(current + new Vector2(-16, 0));
                nextVisit.Add(current + new Vector2(0, -16));
                visited.Add(current);
            }

            nextVisit.Remove(current);
        }

        lastPos = transform.position;
    }
}
*/