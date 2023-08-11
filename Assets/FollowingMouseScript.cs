using UnityEngine;
using System.Collections;

public class FollowingMouseScript : MonoBehaviour {

    private float rand5;
    private float rand6;


    // Use this for initialization
    void Start () {
        rand5 = Random.Range(-80, 80);
        rand6 = Random.Range(-80, 80);
        while (System.Math.Abs(rand5) < 30 && System.Math.Abs(rand5) < 30)
        {
            rand5 = Random.Range(-80, 80);
            rand6 = Random.Range(-80, 80);
        }
    }
	
	// Update is called once per frame
	void Update () {
        /*if (!(gameObject.name == "FakeFollowingMouse"))
        {
            Vector3 hi = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            transform.position = new Vector3(hi.x, hi.y, 0) + new Vector3(rand5, rand6, 0);
        }*/
    }
}
