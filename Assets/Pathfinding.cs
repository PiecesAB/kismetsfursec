using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    public static Vector2 fakeStart = Vector2.zero;
    public static Vector2 fakeEnd = Vector2.zero;

    private static float L1(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static float DistFromEnd(Vector2 p)
    {
        return L1(p, fakeEnd);
    }

    public static float DistFromEnd(Node p)
    {
        return L1(p.position, fakeEnd);
    }

    public static float DistFromStart(Vector2 p)
    {
        return L1(p, fakeStart);
    }

    public static float DistFromStart(Node p)
    {
        return L1(p.position, fakeStart);
    }

    public class Node : IComparable<Node>, IEquatable<Node>
    {
        public Vector2 position;
        public Node parent;
        public float cost;
        public Node(Vector2 pos) { position = pos; parent = null; cost = 0; }
        public Node(Vector2 pos, Node par) { position = pos; parent = par; cost = 0; }
        public Node(Vector2 pos, Node par, float cos) { position = pos; parent = par; cost = cos; }
        public Node(Node old) { position = old.position; parent = old.parent; cost = old.cost; }

        public int CompareTo(Node other)
        {
            return Mathf.RoundToInt((DistFromStart(this) + 1.2f*DistFromEnd(this)
                 - DistFromStart(other) - 1.2f*DistFromEnd(other))*1000f) +
                 Mathf.RoundToInt((Vector2.Dot(fakeStart - position, fakeEnd - position) 
                 - Vector2.Dot(fakeStart - other.position, fakeEnd - other.position))*10f);
        }

        public bool Equals(Node other)
        {
            return (position - other.position).magnitude <= 1e-4;
        }

        public override bool Equals(object other)
        {
            if (other == null) { return false; }
            if (other.GetType() == typeof(Vector2))
            {
                return (position - (Vector2)other).sqrMagnitude <= 1e-4;
            }
            if (other.GetType() == typeof(Node))
            {
                return Equals(this, (Node)other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)Math.Abs(((position.x + 200000000.0)*23154812.0 + (position.y + 2000000000.0)*12839789.0)%2147483648.0);
        }
    }

    private class NodeEqualityComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Node obj)
        {
            return obj.GetHashCode();
        }
    }

    private static Vector2[] possibleNeighbors = {
        new Vector2(16,0), new Vector2(0,16), new Vector2(-16,0), new Vector2(0,-16)
    };

    private static List<Vector2> GetNeighbors(Vector2 current)
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < possibleNeighbors.Length; ++i)
        {
            Vector2 newPos = current + possibleNeighbors[i];
            if (!AmorphousGroundTileNormal.allBlocks.Fetch(newPos))
            {
                result.Add(newPos);
            }
        }
        return result;
    }

    private static List<Vector2> GetNeighbors(Node current)
    {
        return GetNeighbors(current.position);
    }

    public static IEnumerator MakePath(Vector2 start, Vector2 end, Action<List<Vector2>> callback)
    {
        // time for the blocks to load
        if (DoubleTime.UnscaledTimeSinceLoad < 0.01f)
        {
            yield return new WaitForEndOfFrame();
        }

        float xOffset = 0f;
        float yOffset = 8f;
        start = new Vector2(Mathf.Round((start.x + xOffset) / 16f) * 16f - xOffset, Mathf.Round((start.y + yOffset) / 16f) * 16f - yOffset);
        end = new Vector2(Mathf.Round((end.x + xOffset) / 16f) * 16f - xOffset, Mathf.Round((end.y + yOffset) / 16f) * 16f - yOffset);

        fakeStart = start;
        fakeEnd = end;

        SortedSet<Node> open = new SortedSet<Node>() { new Node(start) };
        SortedSet<Node> closed = new SortedSet<Node>();
        // for some reason, SortedSet doesn't have a logarithmic time search and return method. time to mutilate the memory.
        Dictionary<Vector2, Node> hashOpen = new Dictionary<Vector2, Node>() { { start, new Node(start) } };
        Dictionary<Vector2, Node> hashClosed = new Dictionary<Vector2, Node>() { };

        System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();

        while (open.Count > 0 && (open.Min.position - end).sqrMagnitude > 1e-4)
        {
            Node current = open.Min;
            open.Remove(open.Min);
            hashOpen.Remove(current.position);
            closed.Add(current);
            hashClosed.Add(current.position, current);
            List<Vector2> neighbors = GetNeighbors(current);
            for (int i = 0; i < neighbors.Count; ++i)
            {
                if (s.ElapsedMilliseconds >= 1)
                {
                    yield return new WaitForEndOfFrame();
                    s = System.Diagnostics.Stopwatch.StartNew();
                }
                Node neighbor = new Node(neighbors[i]);
                neighbor.cost = current.cost + 16f;

                Node inOpen = hashOpen.ContainsKey(neighbors[i])?hashOpen[neighbors[i]]:null;
                Node inClosed = hashClosed.ContainsKey(neighbors[i]) ? hashClosed[neighbors[i]] : null;

                if (inOpen != null && neighbor.cost < inOpen.cost)
                {
                    open.Remove(neighbor);
                    hashOpen.Remove(neighbors[i]);
                    inOpen = null;
                }

                if (inClosed != null && neighbor.cost < inClosed.cost)
                {
                    closed.Remove(neighbor);
                    hashClosed.Remove(neighbors[i]);
                    inClosed = null;
                }

                if (inOpen == null && inClosed == null)
                {
                    neighbor.parent = current;
                    open.Add(neighbor);
                    hashOpen.Add(neighbors[i], neighbor);
                }
            }
        }

        if (open.Count == 0)
        {
            callback(new List<Vector2>());
            yield break;
        }


        List<Vector2> result = new List<Vector2>();

        Node tracer = open.Min;

        while (tracer.parent != null)
        {
            result.Add(tracer.position);
            tracer = tracer.parent;
        }

        result.Reverse();
        callback(result);
    }
}
