using UnityEngine;
using System.Collections;

public class PrimBhvrSpin : MonoBehaviour {

    public float initDegrees;

    public float initDegreesPerSecond;

    public float acceleration;

    public bool isAccelMultiplicative;
   

    private float orig;
    private double timeMade;
	// Use this for initialization
	void Start () {
        orig = initDegreesPerSecond;
        timeMade = DoubleTime.ScaledTimeSinceLoad;
	}
	
	// Update is called once per frame
	void Update () {
        float t = (float)(DoubleTime.ScaledTimeSinceLoad - timeMade);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, (initDegreesPerSecond * t) + initDegrees);
        if (isAccelMultiplicative)
        {
            initDegreesPerSecond = orig * Mathf.Pow(acceleration, t);
        }
        else
        {
            initDegreesPerSecond = orig + (acceleration* t);
        }
	}
}
