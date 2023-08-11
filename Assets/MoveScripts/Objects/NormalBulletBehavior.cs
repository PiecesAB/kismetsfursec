using UnityEngine;
using System.Collections;

public class NormalBulletBehavior : MonoBehaviour {


    public float speed;
    public float damageMultiplier;
    public bool deleteOffScreen = true;
    public float spawnTime;

    private int minFramesExist = 5;
	
	void Start () {
        
    }
	
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 22)
        {
            GameObject z = other.transform.parent.gameObject;
            z.GetComponent<KHealth>().ChangeHealth(-z.GetComponent<BasicMove>().Damage * damageMultiplier,"normal bullet");

            z.GetComponent<BasicMove>().AddBlood(z.transform.position, Quaternion.LookRotation(100 * transform.up, Vector3.up));
            z.GetComponent<Rigidbody2D>().AddForce(1500 * transform.up);
            z.GetComponent<AudioSource>().PlayOneShot(z.GetComponent<BasicMove>().spikeTouchSound);
            Destroy(gameObject);
        }

           /*
            if (other.gameObject.layer == 8)
            {
                Destroy(gameObject);
            }*/

    }


	void Update () {
        if (Time.timeScale > 0)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
            /*if (GetComponent<Rigidbody2D>())
            {
                GetComponent<Rigidbody2D>().MovePosition(transform.position);
            }*/

            if (deleteOffScreen && !GetComponent<Renderer>().isVisible && minFramesExist == 0)
            {
                Destroy(gameObject);
            }

            if (minFramesExist > 0)
            {
                minFramesExist--;
            }
        }
    }
}
