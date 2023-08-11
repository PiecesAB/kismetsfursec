using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletHellMakerFunctions))]
public class BulletSpecialAimer : MonoBehaviour
{
    public Transform obj;
    public bool aimUsingSpeed;
    public float speedMultiplier = 1f; // at multiplier 1, the bullet will reach the object in 1 second
    public float maxSpeed = 1e6f;

    private BulletHellMakerFunctions maker;
    private Transform t;

    private void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        t = GetComponent<Transform>();
    }

    void Update()
    {
        if (Time.timeScale == 0 || !obj || !t) { return; }
        Vector2 d = ((Vector2)obj.position) - ((Vector2)t.position);
        if (aimUsingSpeed) {
            maker.bulletData.speed = Mathf.Min(maxSpeed, speedMultiplier * d.magnitude);
        }
    }
}
