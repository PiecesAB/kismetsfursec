using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(PrimBezierRender))]
public class PrimBezierMove : MonoBehaviour
{
    public enum MoveMode
    {
        SameTimeToNodes, SameSpeed
    }

    public enum InitializationMode
    {
        Default, EvenlySpaceClones
    }

    public MoveMode moveMode;
    [Header("EvenlySpaceClones only requires setting one object")]
    public InitializationMode initMode = InitializationMode.Default;
    public float initX;
    public Vector2[] removeCloneFractions = new Vector2[0];

    public Transform[] objs;
    public float[] speeds;
    private float[] saveZeroSpeeds;
    public int[] lastNodes;
    public bool[] reverseAtEnd;
    public bool deleteAtEnd = false;
    public bool[] reversedNow;
    public bool haveRigidbody = false;
    public bool moveOtherRigidbodies = false;
    public bool useSwitchConnection = false;
    public bool onlyUpdateWhenVisible = false;
    [Range(0,31)]
    public int switchConnection = 0;


    [Header("0 to start at the normal point, 1 is at the next point")]
    public float[] extraStartingOffsets;

    private double[] t0;
    private double[] t1;
    private PrimBezierRender pbr;
    private Rigidbody2D r2;
    private Rigidbody2D[] otherRigidbodies;
    private Vector2[] otherRBsLastPos;

    private Vector2 lastPos;

    private static T[] RemoveAt<T>(T[] source, int index)
    {
        if (source.Length == 0) { return new T[0]; }
        T[] dest = new T[source.Length - 1];

        if (index > 0)
            System.Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            System.Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public void RemoveObject(ref int i, bool destroyObject = true)
    {
        if (objs[i] && destroyObject) { Destroy(objs[i].gameObject); }
        objs = RemoveAt(objs, i);
        speeds = RemoveAt(speeds, i);
        saveZeroSpeeds = RemoveAt(saveZeroSpeeds, i);
        lastNodes = RemoveAt(lastNodes, i);
        reverseAtEnd = RemoveAt(reverseAtEnd, i);
        reversedNow = RemoveAt(reversedNow, i);
        t0 = RemoveAt(t0, i);
        t1 = RemoveAt(t1, i);
        if (moveOtherRigidbodies)
        {
            otherRigidbodies = RemoveAt(otherRigidbodies, i);
            otherRBsLastPos = RemoveAt(otherRBsLastPos, i);
        }
        --i;
    }

    // Negative = Move to lower indices on wire
    // Positive = Move to higher indices on wire
    public void ChangeObjectSpeed(int index, float newSpeed)
    {
        if (moveMode != MoveMode.SameSpeed) { print("this mode isn't supported"); return; }
        if (newSpeed == speeds[index]) { return; }
        if (newSpeed == 0 && speeds[index] != 0)
        {
            saveZeroSpeeds[index] = speeds[index]; // save non-singular speed
            speeds[index] = 0;
            return;
        }
        if (newSpeed != 0 && speeds[index] == 0)
        {
            speeds[index] = saveZeroSpeeds[index]; // recover old non-singular speed
        }
        bool negativeCorrection = (reversedNow[index] && newSpeed > 0) || (!reversedNow[index] && newSpeed < 0);
        if (negativeCorrection)
        {
            lastNodes[index] = reversedNow[index] ? lastNodes[index] - 1 : lastNodes[index] + 1;
            if (lastNodes[index] < 0) { lastNodes[index] += pbr.bezier.points.Count; }
            if (lastNodes[index] >= pbr.bezier.points.Count) { lastNodes[index] -= pbr.bezier.points.Count; }
            reversedNow[index] = !reversedNow[index];
            double afterDistR = t1[index] - DoubleTime.ScaledTimeSinceLoad;
            double beforeDistR = DoubleTime.ScaledTimeSinceLoad - t0[index];
            t0[index] = DoubleTime.ScaledTimeSinceLoad - afterDistR;
            t1[index] = DoubleTime.ScaledTimeSinceLoad + beforeDistR;
        }
        newSpeed = Mathf.Abs(newSpeed);
        double distRat = speeds[index] / newSpeed; // never a division by zero
        double afterDist = t1[index] - DoubleTime.ScaledTimeSinceLoad;
        double beforeDist = DoubleTime.ScaledTimeSinceLoad - t0[index];
        t0[index] = DoubleTime.ScaledTimeSinceLoad - (beforeDist * distRat);
        t1[index] = DoubleTime.ScaledTimeSinceLoad + (afterDist * distRat);
        speeds[index] = newSpeed;
    }

    public void InsertObjectAtBeginning(Transform obj, float speed, bool reversed = false, bool rvsAtEnd = false)
    {
        int newSize = objs.Length + 1;
        int i = objs.Length;
        System.Array.Resize(ref objs, newSize); objs[i] = obj;
        System.Array.Resize(ref speeds, newSize); speeds[i] = speed;
        System.Array.Resize(ref saveZeroSpeeds, newSize); saveZeroSpeeds[i] = 0;
        System.Array.Resize(ref lastNodes, newSize); lastNodes[i] = (reversed)?pbr.bezier.points.Count:-1;
        System.Array.Resize(ref reverseAtEnd, newSize); reverseAtEnd[i] = rvsAtEnd;
        System.Array.Resize(ref reversedNow, newSize); reversedNow[i] = reversed;
        System.Array.Resize(ref t0, newSize);
        System.Array.Resize(ref t1, newSize);
        t0[i] = t1[i] = DoubleTime.ScaledTimeSinceLoad;
        if (moveOtherRigidbodies)
        {
            System.Array.Resize(ref otherRigidbodies, newSize); otherRigidbodies[i] = obj.GetComponent<Rigidbody2D>();
            if (otherRigidbodies[i])
            {
                System.Array.Resize(ref otherRBsLastPos, newSize); otherRBsLastPos[i] = obj.position;
            }
        }
    }

    void MainLoop(bool updateAnyway = false)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] == null) { continue; }
            if (speeds[i] == 0) { continue; }

            bool specialHandlingOnCorner = false;

            if (DoubleTime.ScaledTimeSinceLoad >= t1[i] || updateAnyway)
            {
                if (reversedNow[i])
                {
                    lastNodes[i]--;
                    if (lastNodes[i] == -1 && pbr.bezier.loop)
                    {
                        lastNodes[i] = pbr.bezier.points.Count - 1;
                    }
                }
                else
                {
                    lastNodes[i]++;
                    if (objs[i].GetComponent<RatchetPlatform>()) // help prevent spaghetti! make this statement use an interface when needed
                    { 
                        objs[i].GetComponent<RatchetPlatform>().TurnACorner();
                        specialHandlingOnCorner = true;
                    } 
                    if (lastNodes[i] == pbr.bezier.points.Count && pbr.bezier.loop)
                    {
                        lastNodes[i] = 0;
                    }
                }
                lastNodes[i] = Mathf.Clamp(lastNodes[i], 0, pbr.bezier.points.Count - 1);
                if (reverseAtEnd[i])
                {
                    if (lastNodes[i] == 0)
                    {
                        reversedNow[i] = false;
                    }

                    if (lastNodes[i] == pbr.bezier.points.Count - 1)
                    {
                        reversedNow[i] = true;
                    }
                }
                if (deleteAtEnd && lastNodes[i] == (reversedNow[i] ? 0 : pbr.bezier.points.Count - 1))
                {
                    RemoveObject(ref i);
                    continue;
                }
                else if (!pbr.bezier.loop)
                {
                    if (!reversedNow[i] && lastNodes[i] == pbr.bezier.points.Count - 1)
                    {
                        lastNodes[i] = 0;
                    }

                    if (reversedNow[i] && lastNodes[i] == 0)
                    {
                        lastNodes[i] = pbr.bezier.points.Count - 1;
                    }
                }

                bool addStartOffset = (t1[i] == t0[i]); // this should only occur at the initialization
                t0[i] = t1[i];
                switch (moveMode)
                {
                    case MoveMode.SameTimeToNodes:
                        t1[i] += speeds[i];
                        if (addStartOffset && extraStartingOffsets.Length > i && extraStartingOffsets[i] > 0)
                        {
                            float shift = speeds[i] * extraStartingOffsets[i];
                            t0[i] -= shift;
                            t1[i] -= shift;
                            extraStartingOffsets[i] = 0;
                        }
                        break;
                    case MoveMode.SameSpeed:
                        int u0 = reversedNow[i] ? (lastNodes[i] - 1) : lastNodes[i];
                        t1[i] += pbr.bezier.SegmentApproxLength(u0, 16) / speeds[i];
                        if (addStartOffset && extraStartingOffsets.Length > i && extraStartingOffsets[i] > 0)
                        {
                            float thisSectionTime = (float)(t1[i] - t0[i]); // this equals pbr.bezier.SegmentApproxLength(u0, 16) / speeds[i]
                            float shift = thisSectionTime * extraStartingOffsets[i];
                            t0[i] -= shift;
                            t1[i] -= shift;
                            extraStartingOffsets[i] = 0;
                        }
                        break;
                    default:
                        break;
                }
            }

            primDecorationMoving pdm = null;
            if (otherRigidbodies != null && otherRigidbodies[i])
            {
                pdm = otherRigidbodies[i].GetComponent<primDecorationMoving>();
            }

            Vector3 oldPos = objs[i].position;
            Vector3 newPos;
            if (reversedNow[i])
            {
                float u1 = (float)(1.0 - ((DoubleTime.ScaledTimeSinceLoad - t0[i]) / (t1[i] - t0[i])));
                newPos = pbr.bezier.EvaluateOnSegment((lastNodes[i] == 0) ? (pbr.bezier.points.Count - 1) : (lastNodes[i] - 1), u1);
                objs[i].position = newPos;
            }
            else
            {
                float u1 = (float)((DoubleTime.ScaledTimeSinceLoad - t0[i]) / (t1[i] - t0[i]));
                newPos = pbr.bezier.EvaluateOnSegment(lastNodes[i], u1);
            }
            if (!pdm || (objs[i].position - newPos).magnitude > 0.1f)
            {
                if (pdm)
                {
                    if (!specialHandlingOnCorner)
                    {
                        pdm.SetPosition(newPos, newPos - oldPos);
                        if ((newPos - pdm.transform.position).magnitude > 10f) // when blocked, just pop to the right position
                        {
                            objs[i].position = newPos;
                        }
                    }
                }
                else { objs[i].position = newPos; }
            }

            if (moveOtherRigidbodies && otherRigidbodies[i] && (!onlyUpdateWhenVisible || pbr.line.isVisible))
            {
                Rigidbody2D otherR2 = otherRigidbodies[i];
                //otherR2.MovePosition(transform.position);
                if (otherRBsLastPos[i].x < -1e8) // first frame: -inf
                {
                    otherRBsLastPos[i] = newPos;
                }
                Vector2 v = ((Vector2)newPos - otherRBsLastPos[i]) / Time.deltaTime;
                if (!specialHandlingOnCorner)
                {
                    if (v.magnitude < 1000f)
                    {
                        if (pdm)
                        {
                            pdm.v = v;
                        }
                        else
                        {
                            otherR2.velocity = v;
                        }
                    }
                    else
                    {
                        otherR2.MovePosition(transform.position);
                    }
                }
                otherRBsLastPos[i] = newPos;
            }
        }
    }

    void Start()
    {
        pbr = GetComponent<PrimBezierRender>();

        switch (initMode)
        {
            case InitializationMode.EvenlySpaceClones: // init X is the object spacing

                if (initX <= 0) { throw new System.Exception("You suck!!!!!!!"); }

                float baseSpeed = speeds[0];
                Transform starterSample = objs[0];
                
                int i = 0;
                float traversed = 0f;

                float[] bezierLengths = new float[(pbr.bezier.loop?0:-1) + pbr.bezier.points.Count];
                float sumLengths = 0f;
                for (int k = 0; k < pbr.bezier.points.Count; ++k)
                {
                    bezierLengths[k] = pbr.bezier.SegmentApproxLength(k, 16);
                    sumLengths += bezierLengths[k];
                }

                int totalCount = Mathf.CeilToInt((sumLengths / initX) + 1f);
                objs = new Transform[totalCount];
                speeds = new float[totalCount];
                saveZeroSpeeds = new float[totalCount];
                lastNodes = new int[totalCount];
                bool firstReverseAtEnd = (reverseAtEnd.Length == 0) ? false : reverseAtEnd[0];
                reverseAtEnd = new bool[totalCount];
                reversedNow = new bool[totalCount];
                extraStartingOffsets = new float[totalCount];

                for (int k = 0; k < totalCount && i < bezierLengths.Length; ++k)
                {
                    float kprog = k / (float)totalCount;
                    for (int rcfi = 0; rcfi < removeCloneFractions.Length; ++rcfi)
                    {
                        Vector2 rcf = removeCloneFractions[rcfi];
                        if (kprog >= rcf.x && kprog <= rcf.y) { goto afterMakeObjectK; }
                    }

                    //if (k == 0) { objs[k] = starterSample; }
                    /*else {*/
                    objs[k] = Instantiate(starterSample.gameObject, starterSample.position, starterSample.rotation, starterSample.parent).transform; 
                    /*}*/
                    speeds[k] = baseSpeed;
                    lastNodes[k] = (i - 1)%pbr.bezier.points.Count;
                    reverseAtEnd[k] = (reverseAtEnd.Length == 0) ? false : firstReverseAtEnd;
                    reversedNow[k] = false;
                    extraStartingOffsets[k] = traversed / bezierLengths[i];

                    afterMakeObjectK:

                    traversed += initX;
                    while (i < bezierLengths.Length && traversed >= bezierLengths[i])
                    {
                        traversed -= bezierLengths[i];
                        ++i;
                    }
                }

                Destroy(starterSample.gameObject);
                
                break;
            default:
            case InitializationMode.Default:
                saveZeroSpeeds = new float[speeds.Length];
                break;
        }

        t0 = Enumerable.Repeat(DoubleTime.ScaledTimeSinceLoad, objs.Length).ToArray();
        t1 = Enumerable.Repeat(DoubleTime.ScaledTimeSinceLoad, objs.Length).ToArray();
        

        if (moveOtherRigidbodies)
        {
            otherRigidbodies = new Rigidbody2D[objs.Length];
            otherRBsLastPos = new Vector2[objs.Length];
            for (int i = 0; i < objs.Length; ++i)
            {
                if (objs[i] == null) { otherRigidbodies[i] = null; continue; }
                otherRigidbodies[i] = objs[i].GetComponent<Rigidbody2D>();
                otherRBsLastPos[i] = Vector2.negativeInfinity; //objs[i].position;
            }
        }

        MainLoop(true);

        r2 = null;
        if (haveRigidbody)
        {
            r2 = gameObject.AddComponent<Rigidbody2D>();
            r2.isKinematic = true;
            lastPos = transform.position;
        }
    }

    public int test = 0;

    void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (initMode == InitializationMode.Default)
        {
            for (int i = 0; i < objs.Length; ++i)
            {
                if (objs[i] == null) { RemoveObject(ref i); }
            }
        }

        if (useSwitchConnection)
        {
            LineRenderer line = pbr.line;
            line.startColor = line.endColor = Utilities.colorCycle[switchConnection];
            if ((Utilities.loadedSaveData.switchMask & (1u << switchConnection)) != 0u)
            {
                MainLoop();
            }
            else
            {
                for (int i = 0; i < objs.Length; ++i)
                {
                    t0[i] += 0.016666666f * Time.timeScale;
                    t1[i] += 0.016666666f * Time.timeScale;
                }
            }
        }
        else
        {
            MainLoop();
        }

        if (haveRigidbody)
        {
            r2.velocity = ((Vector2)transform.position - lastPos) * 60f;
            lastPos = transform.position;
        }
    }
}
