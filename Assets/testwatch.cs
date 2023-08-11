using UnityEngine;
using System.Collections;

public class testwatch : MonoBehaviour {

    public double t1;
    public int i1;

	// Use this for initialization
	void Start () {
        t1 = Fakerand.Double();
        i1 = 2;
	}
	
    void OnCollisionEnter2D(Collision2D hi)
    {
        print(DoubleTime.ScaledTimeSinceLoad);
    }

	// Update is called once per frame
	void Update () {
        //t1 = (((i1 - 1) * t1) + Fakerand.Double()) / i1;
        //i1++;
        //print(t1);
	}
}
