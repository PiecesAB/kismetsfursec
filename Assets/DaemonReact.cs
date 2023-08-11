using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaemonReact : MonoBehaviour
{
    public Transform rotatingBody;
    public Transform robe;

    private Vector2 lastPosition;
    private Vector2 lastRobePos;

    void Start()
    {
        lastPosition = rotatingBody.position;
        if (robe) { lastRobePos = transform.position; }
    }

    void Update()
    {
        Vector2 dif = (Vector2)rotatingBody.position - lastPosition;
        dif = new Vector2(Mathf.Clamp(dif.x * 10f, -45, 45), Mathf.Clamp(dif.y * 10f, -45, 45));
        rotatingBody.localEulerAngles = new Vector3(-90 - dif.y, 180 - dif.x, 0f);
        lastPosition = rotatingBody.position;
        if (robe) {
            robe.position = lastRobePos =  transform.TransformPoint(0.95f * transform.InverseTransformPoint(lastRobePos));
        }
    }
}
