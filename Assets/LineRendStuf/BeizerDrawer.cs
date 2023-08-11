using UnityEngine;
using System.Collections;

public class BeizerDrawer : MonoBehaviour {

    [System.Serializable]
    public struct BeizerPointData
    {
        public Transform point;
        public Transform tangentPoint;
    }



    [Header("curve accuracy is the number of line renderers")]
    [Range(3,120)]
    public byte curveAccuracy;

    public BeizerPointData[] points;
    public GameObject lineRendPrefab;
    public bool drawsItself;
    public bool updatesItself;

    private Vector2 p0;
    private Vector2 p1;
    private Vector2 p2;
    private Vector2 p3;

    private bool drawn;

    // Use this for initialization
    void Start () {
        drawn = false;
	if (drawsItself)
        {
            Draw();
        }
	}
	
    public void Draw()
    {
        if (drawn == true)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                if (t != transform)
                {
                    if (t.name == "LR" + GetInstanceID())
                    Destroy(t.gameObject);
                }
            }
        }
        for (int a = 0; a <= points.Length-2; a++)
        {


            Vector2 lastPos = points[a].point.position;
            Vector2 newPos = points[a].point.position;
            p0 = points[a].point.position;
            p1 = points[a].tangentPoint.position;
            p2 = points[a+1].point.TransformPoint((-1 * (points[1].tangentPoint.localPosition)));
            p3 = points[a+1].point.position;

            for (byte i = 0; i <= curveAccuracy; i++)
            {
                float t = ((float)i) / ((float)curveAccuracy);
                float u = 1 - t;
                float t2 = t * t; float u2 = u * u;
                float t3 = t2 * t; float u3 = u2 * u;

                newPos = (u3 * p0) + (3 * u2 * t * p1) + (3 * u * t2 * p2) + (t3 * p3);

                GameObject newLRObj = (GameObject)Instantiate(lineRendPrefab, Vector3.zero, Quaternion.identity);
                newLRObj.name = "LR" + GetInstanceID();
                LineRenderer newLR = newLRObj.GetComponent<LineRenderer>();
                if (newLR != null)
                {

                    newLR.SetPosition(0, lastPos);
                    newLR.SetPosition(1, newPos);
                }
                newLRObj.transform.SetParent(transform, false);
                lastPos = newPos;
            }
        }
        drawn = true;
    }



	// Update is called once per frame
	void Update () {
	if (updatesItself)
        {
            Draw();
        }
	}
}
