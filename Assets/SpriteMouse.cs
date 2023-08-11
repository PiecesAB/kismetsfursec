using UnityEngine;
using System.Collections;

public class SpriteMouse : MonoBehaviour {

    private Camera cam;

	// Use this for initialization
	void Start () {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        /*Vector3 hi = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = new Vector3(hi.x, hi.y, 0);  */      
	}
}
