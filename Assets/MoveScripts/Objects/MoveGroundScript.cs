using UnityEngine;
using System.Collections;

public class MoveGroundScript : MonoBehaviour {


    public bool ActivatedOnTouch;
    public Vector2 directionOfMvt;
    public float speed;
    public int distance;
    public float waitTime;

    private Vector2 initialPos;
    // Use this for initialization
    void Start () {
        initialPos = new Vector2(transform.position.x, transform.position.y);
        GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(speed*directionOfMvt.x,speed*directionOfMvt.y));
    }
	
    
	// Update is called once per frame
	void Update () {
	
	}
}
