using UnityEngine;
using System.Collections;

public class posMeshOnScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Camera.main.transform.position + new Vector3(0, 0, 1000);
	}
}
