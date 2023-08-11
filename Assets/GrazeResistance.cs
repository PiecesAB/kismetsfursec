using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrazeResistance : MonoBehaviour
{
    [Header("Make it a dyadic fraction or precision issues will happen")]
    [Range(0f, 100f)]
    public float resistPercent = 0;

    public static GrazeResistance main;

    private LineRenderer lr;
    private SpriteRenderer mySquare;

    private void Start()
    {
        main = this;
        GetComponentInChildren<TextMesh>().text = ((int)resistPercent).ToString() + "%";
        lr = GetComponentInChildren<LineRenderer>();
        mySquare = GetComponent<SpriteRenderer>();
    }

    public static float GetMultiplier()
    {
        if (main == null) { return 1f; }
        return 1f - (main.resistPercent * 0.01f);
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        Transform lineTarget = LevelInfoContainer.GetActiveControl()?.transform;
        if (lineTarget && mySquare.isVisible)
        {
            lr.SetPosition(1, transform.InverseTransformPoint(lineTarget.position));
        }
        else { lr.SetPosition(1, Vector3.zero); }
    }
}
