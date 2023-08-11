using UnityEngine;
using System.Collections;

public class NormalGuillotine1 : MonoBehaviour {

    public float speed;
    public float endDistance;

    private Vector3 start;

	// Use this for initialization
	void Start () {
        start = transform.position;
    }
	

    public void Slice(float d)
    {
        
        GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, -speed));
        endDistance = d;

    }


	// Update is called once per frame
	void Update () {
	if ((start-transform.position).magnitude > endDistance)
        {
            Destroy(gameObject, 0);
        }
	}
}
