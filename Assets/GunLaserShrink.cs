using UnityEngine;
using System.Collections;

public class GunLaserShrink : MonoBehaviour {

    public float size;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<LineRenderer>().SetWidth(size, size);
        size = Mathf.Lerp(0,size,0.93f);
        if(size < 0.2f)
        {
            Destroy(gameObject);
        }
	}
}
