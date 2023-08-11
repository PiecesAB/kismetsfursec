using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class CameraCorrectionInterpolate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<ColorCorrectionCurves>().saturation = Mathf.Lerp(GetComponent<ColorCorrectionCurves>().saturation,1f,0.085f);
	}
}
