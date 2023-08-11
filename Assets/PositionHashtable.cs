using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//chained hash solution
public class PositionHashtable<T>
{
    private int length;
    private List<T>[] data;
    private Func<T, Vector3> eval;
    private float toleranceXY;
    private float toleranceZ;
    private int count;

    private int Vector3ToHash(Vector3 v2)
    {
        return (int)Mathf.Repeat((v2.x * 81049.232f) + (v2.y * 1277.2431f), length); // same z: hashed to same value.
    }

    public void Remove(T val)
    {
        data[Vector3ToHash(eval(val))].Remove(val);
        --count;
    }

    public void Add(T val)
    {
        data[Vector3ToHash(eval(val))].Add(val);
        ++count;
    }

    public void Reposition(T val)
    {
        Remove(val);
        Add(val);
    }

    public int Count()
    {
        return count;
    }

    // tolerZ, when it exists, is custom Z tolerance
    public T Fetch(Vector3 pos, float tolerZ = -1)
    {
        if (tolerZ == -1) { tolerZ = toleranceZ; }
        int searchIndex = Vector3ToHash(pos);
        for (int i = 0; i < data[searchIndex].Count; ++i)
        {
            if (data[searchIndex][i] == default) { return default; }
            Vector3 spos = eval(data[searchIndex][i]) - pos;
            if (spos.x < -1e9 && spos.y < -1e9) { return default; }
            if (((Vector2)spos).sqrMagnitude <= toleranceXY*toleranceXY
                && Mathf.Abs(spos.z) <= tolerZ) { return data[searchIndex][i]; }
        }
        return default; // null? hopefully
    }

    public PositionHashtable(int len, Func<T,Vector3> hashEvaluator)
    {
        count = 0;
        length = len;
        data = new List<T>[len];
        for (int i = 0; i < data.Length; ++i) { data[i] = new List<T>(); }
        eval = hashEvaluator;
        toleranceXY = 0.5f;
        toleranceZ = 800000000000f;
    }
}
