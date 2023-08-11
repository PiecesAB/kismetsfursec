using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuGUIElements : MonoBehaviour
{
    public StartMenuGUIs main;
    private const float centerY = 40f;
    private const float middleRange = 60f;
    private const float rotationRange = 60f;
    private RectTransform rt;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        ChangeAppearance();
    }

    private void ChangeAppearance()
    {
        float h = rt.position.y - centerY;
        if (Mathf.Abs(h) < middleRange)
        {
            rt.localScale = Vector3.one;
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, -32f);
            rt.localRotation = Quaternion.identity;
        }
        else if (Mathf.Abs(h) < middleRange + rotationRange*0.8f)
        {
            rt.localScale = Vector3.one;
            float v0 = (Mathf.Abs(h) - middleRange) / rotationRange * 90f;
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, -32f + v0*1.5f);
            rt.localRotation = Quaternion.AngleAxis(Mathf.Sign(h)*v0, Vector3.right);
        }
        else
        {
            rt.localScale = Vector3.zero;
        }
        if (main.selection == int.Parse(transform.Find("FileNum").GetComponent<Text>().text) - 1)
        {
            rt.localPosition += Vector3.back * (48f + 16f*(float)System.Math.Sin(DoubleTime.UnscaledTimeRunning*2.0));
            //rt.LookAt(Camera.main.transform.position);
            //rt.localEulerAngles += new Vector3(1f,0f,1f)*180f;
        }
    }

    private void Update()
    {
        if (main.selection > -1)
        {
            ChangeAppearance();
        }
    }
}
