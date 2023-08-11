using UnityEngine;
using System.Collections;

public class AnotherTimeThingfghrtuyt6r : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.timeScale > 0.001f)
        {
            GetComponent<Renderer>().material.SetVector("_IntensityAndScrolling", new Vector4(((1 / Time.timeScale) - 1) * 0.0035f, ((1 / Time.timeScale) - 1) * 0.0035f, 1f, 1f));
        }
        if (Time.timeScale < 0.001f)
        {
            GetComponent<Renderer>().material.SetVector("_IntensityAndScrolling", new Vector4(0, 0, 1f, 1f));
        }
    }
}
