using UnityEngine;
using System.Collections;

public class FloatyMouseScript : MonoBehaviour {

    private float rand1;
    private float rand2;
    private float rand3;
    private float rand4;
    private float rand5;
    private float rand6;
    private Vector3 center;

    // Use this for initialization
    void Start() {
        center = transform.position;
        rand1 = Random.value;
    rand2 = Random.value;
    rand3 = Random.value;
        rand4 = Random.value;
        rand5 = Random.Range(0, 200);
        rand6 = Random.Range(0, 200);
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(center.x+rand5*Mathf.Sin((4*(float)DoubleTime.ScaledTimeRunning*rand1)+(rand2*3)), center.y + rand6*Mathf.Cos((4 * (float)DoubleTime.ScaledTimeRunning * rand3) + (rand4 * 3)),0);
	}
}
