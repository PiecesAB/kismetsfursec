using UnityEngine;
using System.Collections;

public class ShootingStarBehavior : MonoBehaviour {

    public float speed;
    public float damageMultiplier;

    private double timeWhenMade;
    private bool hit;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<KHealth>() && other.gameObject.GetComponent<BasicMove>())
        {
            other.gameObject.GetComponent<KHealth>().ChangeHealth(-other.gameObject.GetComponent<BasicMove>().Damage * damageMultiplier,"shooting star");

            other.gameObject.GetComponent<BasicMove>().AddBlood(other.gameObject.transform.position, Quaternion.LookRotation(100 * transform.up, Vector3.up));
            other.gameObject.GetComponent<Rigidbody2D>().AddForce(10000 * transform.up);
            other.gameObject.GetComponent<AudioSource>().PlayOneShot(other.gameObject.GetComponent<BasicMove>().spikeTouchSound);
          gameObject.transform.parent = other.gameObject.transform;
            Destroy(gameObject.GetComponent<Rigidbody2D>());
            foreach (BoxCollider2D i in GetComponents<BoxCollider2D>())
            {
                Destroy(i);
            }
        }

        if (other.gameObject.layer == 8)
        {
            Destroy(gameObject.GetComponent<Rigidbody2D>());
            foreach (BoxCollider2D i in GetComponents<BoxCollider2D>())
            {
                Destroy(i);
                
            }
        }

    }


    // Use this for initialization
    void Start () {
        hit = false;
        timeWhenMade = DoubleTime.ScaledTimeSinceLoad;
        GetComponent<Rigidbody2D>().AddForce(transform.up * speed * Time.deltaTime * 100);
        GetComponent<Rigidbody2D>().AddForce(transform.right * Fakerand.Single(-speed/3,speed/3) * Time.deltaTime * 100);
        GetComponent<Rigidbody2D>().AddTorque(Fakerand.Int(-1,2)*500);
    }

    // Update is called once per frame
    void Update()
    {
        double timeDiff = DoubleTime.ScaledTimeSinceLoad - timeWhenMade;

        if (hit)
        {
            Color c1 = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(c1.r, c1.g, c1.b, 1);
        }

        if (timeDiff > 11 && !hit)
        {
            Color c1 = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(c1.r, c1.g, c1.b, Mathf.Round(Mathf.PingPong((float)timeDiff,0.15f)/0.15f));
        }
        if (timeDiff > 13.5 && !hit)
        {
            Destroy(gameObject);
        }
    }
}
