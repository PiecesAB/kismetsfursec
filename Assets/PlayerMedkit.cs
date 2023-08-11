using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMedkit : MonoBehaviour
{

    public ParticleSystem particle;
    public float healAmount;
    public AudioClip c;

    // Use this for initialization
    void Start()
    {


    }


   






    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<BasicMove>() && col.gameObject.GetComponent<KHealth>())
        {
            if (KHealth.health != 150)
            {
                Win1(col);
            }
        }
    }

    public void Win1(Collider2D col)
    {


        //Instantiate something later
        if (col.gameObject.GetComponent<KHealth>())
        {
            if (KHealth.health - healAmount > 0)
            {
                col.gameObject.GetComponent<KHealth>().ChangeHealth(healAmount,"heal");
            }
            else
            {
                col.gameObject.GetComponent<KHealth>().SetHealth(0,"heal");
            }
        }


            enabled = false;
            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = null;

            foreach (AudioSource a in FindObjectsOfType<AudioSource>())
            {
                if (a.clip.name == "DotActCollect")
                {
                    a.Stop();
                }
            }
            GetComponent<AudioSource>().PlayOneShot(c);

            Destroy(gameObject, 1f);

    }






    // Update is called once per frame
    void Update()
    {

    }
}
