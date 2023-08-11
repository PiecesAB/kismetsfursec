using UnityEngine;
using System.Collections;

public class BossChp2ChainScript : MonoBehaviour {

    public Vector3 posStart;
    public Vector3 posEnd;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(Vector3.Distance(posStart, posEnd) / 28, 1);
        Vector3[] cool = { posStart, posEnd };
        GetComponent<LineRenderer>().SetPositions(cool);
	}
}
