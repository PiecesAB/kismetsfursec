using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeMaker : MonoBehaviour
{
    [SerializeField]
    private float length;
    private float lastLength;

    [SerializeField]
    private float segmentLength = 16f;

    [SerializeField]
    private float tension = 100f;
    [SerializeField]
    private float damp = 100f;

    private List<Rigidbody2D> nodeList;
    private LineRenderer line;

    public GameObject ropeColliderSample;

    public Rigidbody2D attachmentStart;
    public Rigidbody2D attachmentEnd;

    public bool breakWhenCompletelyStretched;

    private float lastStretchFactor;

    void LengthChanged()
    {

    }

    void Start()
    {
        lastLength = length;

        

        int nodeCount = Mathf.CeilToInt(length / segmentLength);
        nodeList = new List<Rigidbody2D>();
        Vector3 startPos = (attachmentStart) ? ((Vector3)attachmentStart.position) : (ropeColliderSample.transform.position);
        Vector3 endPos = (attachmentEnd) ? ((Vector3)attachmentEnd.position) : (ropeColliderSample.transform.position);
        Collider2D[] allStartCols = new Collider2D[0];
        if (attachmentStart) { allStartCols = attachmentStart.GetComponents<Collider2D>(); }
        Collider2D[] allEndCols = new Collider2D[0];
        if (attachmentEnd) { allEndCols = attachmentEnd.GetComponents<Collider2D>(); }
        for (int i = 0; i < nodeCount; ++i)
        {
            GameObject newCol = Instantiate(ropeColliderSample);
            newCol.transform.SetParent(transform);
            
            newCol.transform.position = Vector3.Lerp(startPos, endPos, (float)i / (nodeCount - 1));
            Rigidbody2D newR2 = newCol.GetComponent<Rigidbody2D>();
            nodeList.Add(newR2);
            if (i == 0)
            {
                if (attachmentStart) { nodeList[0] = attachmentStart; Destroy(newCol); }
                else { newR2.constraints = RigidbodyConstraints2D.FreezePosition; }
            }
            if (i == nodeCount - 1)
            {
                if (attachmentEnd) { nodeList[nodeCount - 1] = attachmentEnd; Destroy(newCol); }
            }

            

            if (attachmentStart) {
                Collider2D myCol = newCol.GetComponent<Collider2D>();
                for (int j = 0; j < allStartCols.Length; ++j)
                {
                    Physics2D.IgnoreCollision(myCol, allStartCols[j]);
                }
            }
            if (attachmentEnd) {
                Collider2D myCol = newCol.GetComponent<Collider2D>();
                for (int j = 0; j < allEndCols.Length; ++j)
                {
                    Physics2D.IgnoreCollision(myCol, allEndCols[j]);
                }
            }
        }

        LengthChanged();
        line = GetComponent<LineRenderer>();
        line.material.SetVector("_TexSize", new Vector4(0.125f * length, 1f));
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }

        line.positionCount = nodeList.Count;

        float stretchFactor = 0f;

        for (int i = nodeList.Count - 1; i > 0; --i)
        {
            Vector2 displacement = nodeList[i - 1].position - nodeList[i].position;
            Vector2 sdisp = (displacement - (segmentLength * displacement.normalized));
            if (displacement.magnitude > segmentLength) { stretchFactor += sdisp.magnitude; }
            nodeList[i-1].AddForce(-tension * sdisp * Time.timeScale, ForceMode2D.Force);
            nodeList[i].AddForce(tension * sdisp * Time.timeScale, ForceMode2D.Force);
            nodeList[i].AddForce(- damp * nodeList[i].velocity * Time.timeScale, ForceMode2D.Force);
            line.SetPosition(i, nodeList[i].position);
        }

        stretchFactor /= segmentLength;
        stretchFactor /= nodeList.Count;
        stretchFactor *= tension / 250f;
        stretchFactor = Mathf.Lerp(stretchFactor, lastStretchFactor, 0.7f);
        lastStretchFactor = stretchFactor;

        //print(stretchFactor);

        if (breakWhenCompletelyStretched)
        {
            if (stretchFactor > 0.5f)
            {
                line.startColor = line.endColor = Color.LerpUnclamped(Color.white, Color.red, (stretchFactor - 0.5f) * 2f);
            }
            else
            {
                line.startColor = line.endColor = Color.white;
            }

            if (stretchFactor >= 1f)
            {
                Destroy(gameObject);
            }
        }

        line.SetPosition(0, nodeList[0].position);

        if (lastLength != length) { LengthChanged(); }

        lastLength = length;
    }
}
