using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GravWarnerGUI : MonoBehaviour
{
    [Header("0-9 is digits | 10 - | 11 +")]
    public Sprite[] digitSymbols;
    public Image X;
    public Image Y;

    private LevelInfoContainer lic;
    private Vector3 startLocPos;

    public static GravWarnerGUI main;

    private void Start()
    {
        lic = FindObjectOfType<LevelInfoContainer>();
        startLocPos = GetComponent<RectTransform>().localPosition;
        main = this;
    }

    private void Update()
    {
        if (Physics2D.gravity == lic.levelStartGravity)
        {
            GetComponent<RectTransform>().localPosition = new Vector3(-9999, -9999, -9999);
        }
        else
        {
            GetComponent<RectTransform>().localPosition = startLocPos;
            int cx = Mathf.RoundToInt(Physics2D.gravity.x / 2f);
            int cxa = (cx > 0) ? cx : (-cx);
            cxa = Mathf.Min(10, cxa);
            cxa = (cx < -9 && cxa == 10) ? 11 : cxa;
            X.sprite = digitSymbols[cxa];
            X.color = (cx == 0) ? (new Color(0.9f, 0.9f, 0.9f)) : ((cx < 0) ? (new Color(0.45f, 0.45f, 0.9f)) : (new Color(0.9f, 0.55f, 0.55f)));

            int cy = Mathf.RoundToInt(Physics2D.gravity.y / 2f);
            int cya = (cy > 0) ? cy : (-cy);
            cya = Mathf.Min(10, cya);
            cya = (cy < -9 && cya == 10) ? 11 : cya;
            Y.sprite = digitSymbols[cya];
            Y.color = (cy == 0) ? (new Color(0.9f, 0.9f, 0.9f)) : ((cy < 0) ? (new Color(0.45f, 0.45f, 0.9f)) : (new Color(0.9f, 0.55f, 0.55f)));
        }
    }
}
