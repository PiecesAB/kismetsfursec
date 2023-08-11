using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayHeightRuler : MonoBehaviour
{
    [Header("This belongs in the camera object")]
    public float zeroHeight;
    // one block = 0.8128m
    // mark separation is 0.5m, with big marks at 2.5m, and labelled marks at 5m

    private GameObject markSample;
    private Transform t;

    private void RenderMarks(List<float> marks)
    {
        for (int i = 0; i < marks.Count; ++i)
        {
            while (t.childCount <= i) { GameObject newMark = Instantiate(markSample, t); }
            Transform markT = t.GetChild(i);
            GameObject mark = markT.gameObject;
            mark.SetActive(true);
            float offset = 3f * (float)System.Math.Sin(DoubleTime.ScaledTimeSinceLoad * 4 + (double)marks[i] * 0.5f);
            markT.position = new Vector3(t.position.x + offset, (marks[i] / 0.0508f) + zeroHeight, markT.position.z);
            if (marks[i] % 5f == 0f)
            {
                markT.localScale = new Vector3(3, 1, 1);
                markT.GetChild(1).GetComponent<TextMesh>().text = (marks[i] > 0f ? "+" : "") + marks[i].ToString() + "m";
            }
            else if (marks[i] % 2.5f == 0f)
            {
                markT.localScale = new Vector3(2, 1, 1);
                markT.GetChild(1).GetComponent<TextMesh>().text = "";
            }
            else
            {
                markT.localScale = new Vector3(1, 1, 1);
                markT.GetChild(1).GetComponent<TextMesh>().text = "";
            }
        }

        for (int i = marks.Count; i < t.childCount; ++i)
        {
            t.GetChild(i).gameObject.SetActive(false);
        }
    }

    private List<float> GetVisibleMarks()
    {
        // find the lowest mark (ok to round, just displays one off screen sometimes)
        float screenBottom = (transform.position.y - 112f - zeroHeight) * 0.0508f;
        screenBottom = Mathf.Floor(screenBottom * 2f) / 2f;
        // screen is about 11m tall
        List<float> ret = new List<float>();
        ret.Add(screenBottom);
        for (float x = 0f; x < 12f; x += 0.5f)
        {
            ret.Add(screenBottom + x);
        }
        return ret;
    }

    private void Start()
    {
        markSample = transform.GetChild(0).gameObject;
        t = transform;
    }

    private void Update()
    {
       if (Time.timeScale == 0) { return; }
        RenderMarks(GetVisibleMarks());
    }
}
