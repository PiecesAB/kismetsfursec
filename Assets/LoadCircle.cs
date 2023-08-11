using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadCircle : MonoBehaviour
{
    public TextMesh textMesh;
    public Text text;
    public Renderer ren;
    public Image image;
    public Gradient fillGrad;
    public Vector2 fillSize;
    public float appearAfterTime = 0;
    private bool goneUntilTime = false;
    public bool test;
    public int testValue;

    void Change()
    {
        int n = test?testValue:primAddScene.loadProgressTracker;
        if (textMesh)
        {
            textMesh.text = n.ToString();
        }
        if (text)
        {
            text.text = n.ToString();
        }
        float t = n * 0.01f;
        if (ren)
        {
            ren.material.color = fillGrad.Evaluate(1f - t);
            ren.material.SetFloat("_H", Mathf.Lerp(fillSize.x, fillSize.y, t));
        }
        if (image)
        {
            image.material.color = fillGrad.Evaluate(1f - t);
            image.material.SetFloat("_H", Mathf.Lerp(fillSize.x, fillSize.y, t));
        }
        if (goneUntilTime && DoubleTime.UnscaledTimeSinceLoad > appearAfterTime)
        {
            goneUntilTime = false;
            transform.position += new Vector3(9999, 9999);
        }
    }

    void Start()
    {
        Change();
        if (ren)
        {
            ren.material.SetFloat("_Resolution", 32);
        }
        if (image)
        {
            image.material.SetFloat("_Resolution", 32);
        }
        if (appearAfterTime > 0)
        {
            goneUntilTime = true;
            transform.position -= new Vector3(9999, 9999);
        }
    }

    void Update()
    {
        Change();
    }
}
