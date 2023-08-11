using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeDetectGUIOnly : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<Text>().text = "SPEED: " + Mathf.Floor(100 * Time.timeScale);
    }
}
