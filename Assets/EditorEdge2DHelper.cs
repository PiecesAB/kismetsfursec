using UnityEngine;
using System.Collections;

public class EditorEdge2DHelper : MonoBehaviour {
    public Vector2[] points;

	void Start () {
        GetComponent<PolygonCollider2D>().SetPath(0, points);
    }

	void Update () {

	}
}
