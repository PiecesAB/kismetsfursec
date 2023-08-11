using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChokeCollarMove : MonoBehaviour
{
    private Rigidbody2D parentMvt;

    void Start()
    {
        parentMvt = transform.parent.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        transform.right = (Vector3)parentMvt.velocity.normalized + new Vector3(6f, 0f, 0f);
    }
}
