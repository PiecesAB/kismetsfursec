using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderTest : MonoBehaviour
{
    public Vector2 start;
    public Vector2 end;
    System.Diagnostics.Stopwatch s;
    string printable = "d";

    void Update()
    {
        if (printable != "")
        {
            print(printable);
            printable = "";
            print("pathfind start");
            s = System.Diagnostics.Stopwatch.StartNew();
            StartCoroutine(Pathfinding.MakePath(start, end, (result) =>
            {
                s.Stop();
                printable = (s.ElapsedMilliseconds + " elapsed ms");
                for (int i = 0; i < result.Count - 1; ++i)
                {
                    Debug.DrawLine(result[i], result[i + 1], Color.red, 10f);
                }
            }));
        }
    }
}
