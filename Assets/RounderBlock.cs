using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RounderBlock : MonoBehaviour {

    public float qario; //it stands for quality

    public float radius = 16f;

    void Start () {

        GetComponent<PolygonCollider2D>().offset = new Vector2(-radius / 2f, radius / 2f);

        List<Vector2> list = new List<Vector2>() {

            new Vector2(0,-radius),
            new Vector2(radius,-radius),
            new Vector2(radius,0),

        };

        for (float i=0;  i< Mathf.PI / 2f; i += Mathf.PI / (2f * qario))
        {
            list.Add(new Vector2(radius*Mathf.Cos(i),-radius*Mathf.Sin(i)));
        }

        GetComponent<PolygonCollider2D>().SetPath(0, list.ToArray());
	}

}
