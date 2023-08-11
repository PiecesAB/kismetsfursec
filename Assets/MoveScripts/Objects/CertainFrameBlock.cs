using UnityEngine;
using System.Collections;

public class CertainFrameBlock : MonoBehaviour {

    public float TimeScaleToAppear;
    public Sprite onAnim;
    public Sprite offAnim;
    public bool on = false;


	// Use this for initialization
	void Start () {
        GetComponent<Collider2D>().enabled = false;
        on = false;
    }
	
	// Update is called once per frame
	void Update () {
	if (Time.timeScale == TimeScaleToAppear)
        {
            //GetComponent<Animation>().Play();
            GetComponent<SpriteRenderer>().sprite = onAnim;
            GetComponent<Collider2D>().enabled = true;
            on = true;
        }
        else
        {
            // GetComponent<Animation>().Play();
            if (on)
            {
                GetComponent<SpriteRenderer>().sprite = offAnim;
            }
            GetComponent<Collider2D>().enabled = false;
            on = false;
        }
	}
}
