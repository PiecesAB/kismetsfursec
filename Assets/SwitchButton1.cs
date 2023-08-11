using UnityEngine;
using System.Collections;

public class SwitchButton1 : MonoBehaviour {

    public bool on;
    public Sprite onSprite;
    public Sprite offSprite;
    [Range(0, 31)]
    public int switchID;
	// Use this for initialization
	void Start () {
        on = false;
        GetComponent<SpriteRenderer>().sprite = offSprite;
    }
	
    void OnCollisionEnter2D(Collision2D col)
    {
        if (on == false)
        {
            GetComponent<SpriteRenderer>().sprite = onSprite;
            GetComponent<Collider2D>().offset = new Vector2(0, -2);
            col.gameObject.transform.position = col.gameObject.transform.position - new Vector3(0, 4, 0);
            Utilities.ChangeSwitchRequest(1u << switchID);
            on = true;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (on == true)
        {
            GetComponent<SpriteRenderer>().sprite = offSprite;
            GetComponent<Collider2D>().offset = new Vector2(0, 2);
            col.gameObject.transform.position = col.gameObject.transform.position + new Vector3(0, 4, 0);
            Utilities.ChangeSwitchRequest(1u << switchID);
            on = false;
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
