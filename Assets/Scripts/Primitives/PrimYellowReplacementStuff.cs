using UnityEngine;
using System.Collections;

public class PrimYellowReplacementStuff : MonoBehaviour {

    public Color color;
    public bool updateEveryFrame;

	void Start () {
        GetComponent<Renderer>().material.SetColor("_RepColor", color);
	}
	
	void Update () {
	if (updateEveryFrame || (Application.isEditor && !Application.isPlaying))
        {
            GetComponent<Renderer>().material.SetColor("_RepColor", color);
        }
	}
}
