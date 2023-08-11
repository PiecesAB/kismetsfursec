using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class C4collision : GenericBomb {

    public GameObject warningCircle;
    public GameObject explosionEffect;
    [HideInInspector]
    public Collider2D[] groundList;
    [HideInInspector]
    public GameObject[] cameraFind;
    public AudioClip BOOM;

    private GameObject newCircle;
    [HideInInspector]
    public bool db;
    private Color spColor;

    private AudioSource sound;

	protected override void SubStart () {
        db = false;
        sound = GetComponent<AudioSource>();
	}

    protected override void SubUpdate()
    {
    }

    protected override bool FinalClearanceBeforeExplode()
    {
        return true;
    }

    public override void OnStartCountdown()
    {
        newCircle = Instantiate(warningCircle, transform.position, Quaternion.identity) as GameObject;
    }

    public override IEnumerator Explode()
    {
        GetComponent<SpriteRenderer>().sprite = null;
        Destroy(newCircle);
        GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity) as GameObject;
        sound.Stop();
        sound.clip = BOOM;
        sound.volume = volo;
        sound.Play();
        cameraFind =  new GameObject[1] { Camera.main.gameObject };
        cameraFind[0].GetComponent<FollowThePlayer>().vibSpeed = 3;
        groundList = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), 64);
        foreach (Collider2D item in groundList)
        {
            if (item.gameObject.GetComponent<C4collision>() != null)
            {

                item.gameObject.GetComponent<C4collision>().ChainC4();
              
            }
            else
            {
                if (item.gameObject.CompareTag("Player"))
                {
                    if (item.gameObject.GetComponent<KHealth>() && item is BoxCollider2D)
                    {
                        item.gameObject.GetComponent<KHealth>().ChangeHealth(-100f,"c4 bomb");
                    }
                    
                }
                List<AmorphousGroundTileNormal> liz = new List<AmorphousGroundTileNormal>() { };
                if (item.GetComponent<BasicMove>())
                {
                    if (item is BoxCollider2D)
                    {
                        item.GetComponent<BasicMove>().fakePhysicsVel += 200 * (Vector2)(item.transform.position - transform.position).normalized;
                    }
                }
                else if (!item.gameObject.CompareTag("Indestructible Ground") && item.gameObject.GetComponent<Rigidbody2D>() != null
                    && item.gameObject.GetComponent<SpriteRenderer>()) // do not destroy SubMeld
                {
                    item.GetComponent<Rigidbody2D>().isKinematic = false;
                    item.GetComponent<Rigidbody2D>().AddForce(400 * Fakerand.UnitCircle());
                    spColor = item.gameObject.GetComponent<SpriteRenderer>().color;
                    item.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.7f * spColor.r, 0.7f * spColor.g, 0.7f * spColor.b);
                    Destroy(item.gameObject, 4);
                    Destroy(item, 0);
                }
                liz.Clear();
            }
        }
        Destroy(GetComponent<SpriteRenderer>());
        Destroy(GetComponent<Collider2D>());
        counter.text = "";
        Destroy(gameObject, 3f);
        yield return null;
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        if (!db)
        {
            db = true;
            Activate();
        }

    }

    public void ChainC4()
    {
        if (!db)
        {
            db = true;
            Activate();
        }
    }
}
