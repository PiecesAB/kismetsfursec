using UnityEngine;
using System.Collections;

public class PolygonDrawer : MonoBehaviour {


    [Header("high number of sides makes a circle")]
    [Range(3, 360)]
    public ushort sides;
    public float degreesOffset;
    public Transform center;
    public float radius;
    public GameObject lineRendPrefab;

    public AnimationCurve sizeRatioOverTime;
    public Gradient colorOverTime;
    [Header("not 0 makes it rainbow; the closer to 0 the slower")]
    public float rainbowSpeed;
    public float durationOfCurve;

    public bool drawsItself;
    public bool updatesItself;

    private bool drawn;
    private float origRadius;
    private double startTime;

    private float origDegOffset;
    private Color color;
    // Use this for initialization
    void Start()
    {
        drawn = false;
        color = colorOverTime.Evaluate(0);
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t != transform)
            {
                if (t.GetComponent<SpriteRenderer>())
                    Destroy(t.GetComponent<SpriteRenderer>());
            }
        }
        startTime = DoubleTime.ScaledTimeSinceLoad;

            origRadius = radius;

        origDegOffset = degreesOffset + transform.eulerAngles.z;
        if (drawsItself && !updatesItself)
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

        float newX = center.position.x + radius * Mathf.Cos(degreesOffset * Mathf.Deg2Rad);
        float newY = center.position.y + radius * Mathf.Sin(degreesOffset * Mathf.Deg2Rad);


        Vector2 lastPos = new Vector2(newX, newY);
        Vector2 newPos = new Vector2(newX, newY);
        for (ushort i = 0; i <= sides; i++)
        {
            float progression = (((float)i) / ((float)sides)) * (2 * Mathf.PI) + (degreesOffset*Mathf.Deg2Rad);

            newX = center.position.x + radius*Mathf.Cos(progression);
            newY = center.position.y + radius * Mathf.Sin(progression);

            newPos = new Vector2(newX, newY);

            GameObject newLRObj = (GameObject)Instantiate(lineRendPrefab, Vector3.zero, Quaternion.identity);
            newLRObj.name = "LR"+ GetInstanceID();
            LineRenderer newLR = newLRObj.GetComponent<LineRenderer>();
           
            if (newLR != null)
            {
                newLR.SetColors(color, color);
                newLR.SetPosition(0, lastPos);
                newLR.SetPosition(1, newPos);
            }
            newLRObj.transform.SetParent(transform, false);
            lastPos = newPos;
        }
        drawn = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        
        if (updatesItself)
        {
            if (GetComponent<PrimBhvrSpin>() != null)
            {
                degreesOffset = transform.eulerAngles.z+origDegOffset;
            }
            if ((DoubleTime.ScaledTimeSinceLoad - startTime) / durationOfCurve > 1)
            {
                Destroy(gameObject);
            }
            radius = origRadius * sizeRatioOverTime.Evaluate((float)(DoubleTime.ScaledTimeSinceLoad - startTime) / durationOfCurve);
            if (rainbowSpeed != 0)
            {
                color = Color.HSVToRGB((rainbowSpeed*(float)(DoubleTime.ScaledTimeSinceLoad -startTime))% 1, 0.4f, 0.5f);
            }
            else
            {
                color = colorOverTime.Evaluate((float)(DoubleTime.ScaledTimeSinceLoad - startTime) / durationOfCurve);
            }
            Draw();
        }
    }
}
