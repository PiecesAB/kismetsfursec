using UnityEngine;
using System.Collections;

public class ResolutionScaler2 : MonoBehaviour {

    public float myScale = 1.3333333f;
    private Vector3 oldSize;
	
	void Awake () {
            oldSize = GetComponent<Transform>().localScale;
	}
	
	
	void Update () {
        float rat = myScale/(((int)Screen.currentResolution.width) / ((int)Screen.currentResolution.height));
        //print(rat);
            if (rat > 1f)
            GetComponent<Transform>().localScale = oldSize * rat;
            foreach (Transform t in transform)
        {
            t.localScale = oldSize / rat;
        }
    }
}
