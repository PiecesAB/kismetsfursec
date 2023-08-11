using UnityEngine;
using System.Collections;

public class PushableObjectFriction : MonoBehaviour {

    public ParticleSystem leftDust;
    public ParticleSystem rightDust;
    [Range(0f,1f)]
    public float friction;
    public float staticVelX;

    // Use this for initialization
    void Start () {
        leftDust.enableEmission = false;
        rightDust.enableEmission = false;
    }
	
	// Update is called once per frame
	void Update () {

        if (GetComponent<Rigidbody2D>().velocity.x > staticVelX)
        {
            leftDust.startSpeed = System.Math.Abs(GetComponent<Rigidbody2D>().velocity.x)/2f;
            leftDust.enableEmission = true;
        }
        else
        {
            leftDust.enableEmission = false;
        }
        if (GetComponent<Rigidbody2D>().velocity.x < -staticVelX)
        {
            rightDust.startSpeed = System.Math.Abs(GetComponent<Rigidbody2D>().velocity.x)/2f;
            rightDust.enableEmission = true;
        }
        else
        {
            rightDust.enableEmission = false;
        }

        if (System.Math.Abs(GetComponent<Rigidbody2D>().velocity.x) < staticVelX)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0f, GetComponent<Rigidbody2D>().velocity.y);
        }
        else
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x * (1f - friction), GetComponent<Rigidbody2D>().velocity.y);
    }
                }
}
