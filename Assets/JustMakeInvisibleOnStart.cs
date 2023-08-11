using UnityEngine;
using System.Collections;

public class JustMakeInvisibleOnStart : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
	}
	

}
