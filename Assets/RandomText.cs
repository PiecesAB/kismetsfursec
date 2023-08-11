using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RandomText : MonoBehaviour {

    public string[] taunts;
	// Use this for initialization
	void Start () {
        GetComponent<Text>().text = taunts[Fakerand.Int(0, taunts.Length)];
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
