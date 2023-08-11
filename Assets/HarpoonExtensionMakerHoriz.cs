using UnityEngine;
using System.Collections;

public class HarpoonExtensionMakerHoriz : MonoBehaviour {

    public Vector3 origin;
    public LineRenderer extension;

	// Use this for initialization
	void Awake () {
        origin = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        Vector3[] constructedExtension = { transform.position, origin };
        float dist = ((origin - transform.position).magnitude);
        extension.SetPositions(constructedExtension);
        GetComponent<BoxCollider2D>().size = new Vector2(6, 26 + (dist));
        GetComponent<BoxCollider2D>().offset = new Vector2(1, -10 + (dist / 2));

    }
}
