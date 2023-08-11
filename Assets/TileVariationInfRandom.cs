using UnityEngine;
using System.Collections;

public class TileVariationInfRandom : MonoBehaviour {

    public Sprite[] sprites;
	// Use this for initialization
	void Start () {
        GetComponent<SpriteRenderer>().sprite = sprites[Mathf.FloorToInt(Fakerand.Single(0,sprites.Length-0.01f))];
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
