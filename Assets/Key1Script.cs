using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Key1Script : MonoBehaviour {

    

    public static int keyCount;
    public ParticleSystem particle;
    public List<Door1> unlockableDoors;
    public AudioClip c;

    public Coroutine WaitForRealSeconds(float time) //this piece of code is copied in a bunch of scripts. don't try this in your game!
    {
        return StartCoroutine(WaitForRealSecondsImpl(time));
    }

    private IEnumerator WaitForRealSecondsImpl(float time)
    {
        double startTime = DoubleTime.UnscaledTimeRunning;
        while (DoubleTime.UnscaledTimeRunning - startTime < time)
            yield return 1;
    }

    // Use this for initialization
    void Start () {
        keyCount++;
        foreach (Door1 i in FindObjectsOfType<Door1>())
        {
            unlockableDoors.Add(i);
        }

	}
	

    IEnumerator Win2(Collider2D col)
    {
        /* FollowThePlayer f = FindObjectOfType<FollowThePlayer>();
         Vector2 oldVel = Vector2.zero;
         GetComponent<SpriteRenderer>().sprite = null;
         if (col.gameObject.GetComponent<BasicMove>())
         {
             col.gameObject.GetComponent<BasicMove>().enabled = false;
         }
         if (col.gameObject.GetComponent<Rigidbody2D>())
         {
             oldVel = col.gameObject.GetComponent<Rigidbody2D>().velocity;
             col.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
         }


         Transform orig = f.target;
         float oldS = Time.timeScale;
         Time.timeScale = 0;
         foreach (Door1 a in unlockableDoors)
         {

             f.target = a.transform;
             yield return WaitForRealSeconds(0.84f);

             // ADD AN ANIMATION PL0X


             a.gameObject.GetComponent<BoxCollider2D>().enabled = true;
             yield return WaitForRealSeconds(0.84f);

         }

         f.target = orig;

         yield return WaitForRealSeconds(0.8f);

         Time.timeScale = oldS;


         if (col.gameObject.GetComponent<BasicMove>())
         {
             col.gameObject.GetComponent<BasicMove>().enabled = true;
         }
         if (col.gameObject.GetComponent<Rigidbody2D>())
         {
             col.gameObject.GetComponent<Rigidbody2D>().velocity = oldVel;
             col.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
         }


     */

        yield return null;
        Destroy(gameObject);
    }






    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<BasicMove>())
        {
            Win1(col);
        }
    }

    public void Win1(Collider2D col)
    {
        keyCount--;

        //Instantiate something later
        if (col.gameObject.GetComponent<ClickToChangeTime>())
        {
            col.gameObject.GetComponent<ClickToChangeTime>().gunHealth = Mathf.Clamp(col.gameObject.GetComponent<ClickToChangeTime>().gunHealth + (col.gameObject.GetComponent<ClickToChangeTime>().gunHealthDecreaseAmount * 1.3f),0,100);
        }


        if (keyCount == 0)
        {
            StartCoroutine(Win2(col));
        }
        if (keyCount >= 1)
        {
            enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = null;

            foreach (AudioSource a in FindObjectsOfType<AudioSource>())
            {
                if (a.clip != null)
                {
                    if (a.clip.name == "DotActCollect")
                    {
                        a.Stop();
                    }
                }
            }
            GetComponent<AudioSource>().PlayOneShot(c);

            Destroy(gameObject,1f);
        }
    }






	// Update is called once per frame
	void Update () {

	}
}
