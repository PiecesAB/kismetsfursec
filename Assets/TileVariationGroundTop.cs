using UnityEngine;
using System.Collections;

public class TileVariationGroundTop : MonoBehaviour {

    public Sprite common;
    public Sprite uncommon;
    public Sprite rare;
    private float rand;

    // Use this for initialization
    void Start() {
        rand = Fakerand.Single();
    if (rand <= 0.7f) {
            GetComponent<SpriteRenderer>().sprite = common;
        }
        if (rand > 0.7f && rand <= 0.95f)
        {
            GetComponent<SpriteRenderer>().sprite = uncommon;
        }
        if (rand > 0.95f)
        {
            GetComponent<SpriteRenderer>().sprite = rare;
            GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color + new Color(0.14f,0.14f,0.14f,1f);

        }
    }
	
	
	// Update is called once per frame
	void Update () {
	
	}
}