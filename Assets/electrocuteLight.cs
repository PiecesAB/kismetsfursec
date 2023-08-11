using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class electrocuteLight : MonoBehaviour
{

    public bool collided;
    public SkinnedMeshRenderer meshWithSKey;
    public Collider2D colliderToDestroy;
    public int shapeKey;
    public AudioSource breakSound;
    public GameObject glassShard;
    public bool fallOnThings;
    public Rigidbody2D myRB;
    //add particles

    void Start()
    {
        collided = false;
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!collided)
        {
            /*if (meshWithSKey != null && shapeKey >= 0)
            {
                meshWithSKey.SetBlendShapeWeight(shapeKey, 100f);
            }*/

            if (meshWithSKey != null)
            {
                meshWithSKey.enabled = false;
            }

            if (c.gameObject.GetComponent<KHealth>())
            {
                c.gameObject.GetComponent<KHealth>().electrocute += 0.5f;
            }

            if (breakSound != null)
            {
                breakSound.Play();
            }

            collided = true;
            Vector2 norm = c.GetContact(0).normal;
            float rot = Mathf.Rad2Deg * Mathf.Atan2(norm.y, norm.x);
            GameObject shard = Instantiate(glassShard, transform.position, Quaternion.AngleAxis(rot, Vector3.forward));
            shard.transform.SetParent(transform.parent, true);
            Destroy(colliderToDestroy);
            myRB.gravityScale = 40f;
        }
    }

    private void Update()
    {
        if (fallOnThings)
        {
            RaycastHit2D rh = Physics2D.Raycast(transform.position, -transform.up, 360f, 1049344);
            if (rh.collider != null && rh.collider.gameObject.layer == 20)
            {
                myRB.gravityScale = 80f;
            }
        }
    }
}
