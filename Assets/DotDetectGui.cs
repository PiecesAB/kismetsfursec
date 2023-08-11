using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DotDetectGui : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<Text>().text = "DOT: " + Key1Script.keyCount;
	}
}
