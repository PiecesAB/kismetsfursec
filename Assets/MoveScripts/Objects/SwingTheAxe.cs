using UnityEngine;
using System.Collections;

public class SwingTheAxe : MonoBehaviour {

    public float startAngle;
    public float speed;
    public float sweepDegrees;

    public float axeVelocity;
    
	// Use this for initialization 
	void Start () {
        transform.rotation = Quaternion.AngleAxis(sweepDegrees * Mathf.Sin(Mathf.Deg2Rad * startAngle), transform.forward);
        axeVelocity =  (float)(3 * sweepDegrees * System.Math.Cos((speed * (Mathf.PI * DoubleTime.ScaledTimeRunning))));
    }
	
	// Update is called once per frame
	void Update () {

        transform.rotation = Quaternion.AngleAxis(((float)(sweepDegrees *System.Math.Sin((speed*(Mathf.PI*DoubleTime.ScaledTimeRunning))))+(Mathf.Deg2Rad*startAngle)), transform.forward);
        axeVelocity = (float)(3 * sweepDegrees * System.Math.Cos((speed * (Mathf.PI * DoubleTime.ScaledTimeRunning))));

    }
}
