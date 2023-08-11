using UnityEngine;
using System.Collections;

public class FallBeamCollision : MonoBehaviour {

    public Sprite[] numbers;
    public float waitBetween;
    public int startTime;
    public bool haltCountdown;
    public AudioClip beep1;
    public AudioClip fallSound;

    private Object newCircle;
    public bool db;
    private Color spColor;


	// Use this for initialization
	void Start () {
        GetComponent<SpriteRenderer>().sprite = numbers[startTime];
        db = false;
	}
	
	// Update is called once per frame
    public IEnumerator ASPLODE()
    {
        
        while (startTime >= 0)
        {
            if (!haltCountdown)
            {
                startTime = startTime - 1;
                GetComponent<SpriteRenderer>().sprite = numbers[startTime+1];
            }
            if (startTime == -1)
            {
                yield return new WaitForSeconds(waitBetween / 2);
            }
            else
            {
                GetComponent<AudioSource>().PlayOneShot(beep1);
                yield return new WaitForSeconds(waitBetween);
            }
        }

        GetComponent<AudioSource>().PlayOneShot(fallSound);
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 4);
    }


    void OnCollisionStay2D(Collision2D col)
    {
        if (!db && col.contacts[0].normal == new Vector2(0, -1))
        {
            db = true;
            StartCoroutine(ASPLODE());
        }

    }

    public void Whatever()
    {
        if (!db)
        {
            db = true;
            StartCoroutine(ASPLODE());
        }
    }

	void Update () {
	
	}
}
