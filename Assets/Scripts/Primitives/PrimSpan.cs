using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimSpan : MonoBehaviour
{
    public Transform endA;
    public Transform endB;

    public float startWidth = 16f;

    private Vector3 aPos = Vector3.negativeInfinity;
    private Vector3 bPos = Vector3.negativeInfinity;

    private Vector3 aScal = Vector3.negativeInfinity;
    private Vector3 bScal = Vector3.negativeInfinity;

    private void Reposition()
    {
        aPos = endA.position;
        bPos = endB.position;
        aScal = endA.localScale;
        bScal = endB.localScale;
        Vector2 dif = bPos - aPos;
        transform.localScale = new Vector3(dif.magnitude / startWidth, 0.5f*(endA.localScale.y + endB.localScale.y), 1f);
        transform.position = 0.5f*(aPos + bPos);
        transform.eulerAngles = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg * Vector3.forward;
    }

    private void Start()
    {
        Reposition();
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (aPos != endA.position || bPos != endB.position || aScal != endA.localScale || bScal != endB.localScale)
        {
            Reposition();
        }
    }
}
