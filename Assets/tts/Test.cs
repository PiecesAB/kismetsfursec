using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Test : MonoBehaviour {

    public string msg;
    public List<int> stresses;

	// Use this for initialization
	void Start () {
        //orthography.main(msg,true,out stresses);

	}

	// Update is called once per frame
	void Update () {
	    foreach (Transform t in transform)
        {
            t.position = new Vector3(Mathf.Round(t.position.x), Mathf.Round(t.position.y), t.position.z);
        }
	}
}
