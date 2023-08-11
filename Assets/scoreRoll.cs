using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class scoreRoll : MonoBehaviour
{
    public enum CollectibleArrangement
    {
        Tetrahedron, Cube
    }

    public float size = 64f;
    private const float shrinkSpeed = 0.2666666f;
    public GameObject top;
    public LineRenderer middle;
    public BoxCollider2D middleCol;
    public Renderer bottom;
    public GameObject popEffect;
    public GameObject money;
    public AudioSource myAudio;
    public AudioClip popSound;
    public AudioClip punchSound;
    public CollectibleArrangement collectibleArrangement = CollectibleArrangement.Tetrahedron;
    //ADD SOUND

    private bool destroyed = false;

    void Start()
    {
        top.transform.position = transform.position + (transform.up * (16f + size));
        //top.MovePosition(top.transform.position);
        middle.SetPosition(0, new Vector3(0f,(size + 8f)/middle.transform.localScale.y));
        float s2 = size + 31.6f;
        middleCol.size = new Vector2(15.6f, s2);
        middleCol.offset = new Vector2(0f, size * 0.5f + 8f);
        destroyed = false;
        Collider2D[] tc = top.GetComponents<Collider2D>();
        Collider2D[] bc = bottom.GetComponents<Collider2D>();
        foreach (var c in tc)
        {
            Physics2D.IgnoreCollision(middleCol, c);
        }
        foreach (var c in bc)
        {
            Physics2D.IgnoreCollision(middleCol, c);
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        //(c.gameObject.CompareTag("Player") && Vector2.Dot(c.GetContact(0).normal,Vector2.up) > 0.99f)
        if (c.collider.gameObject.layer == 19 && !destroyed)
        {
            destroyed = true;
                Instantiate(popEffect, transform.position, Quaternion.identity, transform.parent);
                for (float p = 0f; p < size + 24.1f; p += 8f)
                {
                    Vector3 p1 = transform.position + (transform.up * p);
                    Vector3 lr = (transform.right * 4f);
                    switch (collectibleArrangement)
                    {
                        case CollectibleArrangement.Tetrahedron:
                            Instantiate(money, p1 - lr, Quaternion.Euler(30f,0f,0f), transform.parent);
                            Instantiate(money, p1 + lr, Quaternion.Euler(30f, 0f, 0f), transform.parent);
                            break;
                        case CollectibleArrangement.Cube:
                            Instantiate(money, p1, Quaternion.Euler(30f, 0f, -30f), transform.parent);
                            break;
                        default:
                            break;
                    }
                }
            myAudio.Stop();
            myAudio.clip = punchSound;
            myAudio.Play();
            DestroyRoutine();
            Destroy(gameObject, 1f);
        }

        /*if (c.gameObject.CompareTag("Player") && Vector2.Dot(c.GetContact(0).normal, Vector2.up) > 0.99f)
        {
            bottom.GetComponent<Rigidbody2D>().velocity += Vector2.up * 100f;
        }*/
    }

    void DestroyRoutine()
    {
        Destroy(top);
        Destroy(middle.gameObject);
        Destroy(middleCol);
        Destroy(bottom);
        Destroy(GetComponent<PrimMovingPlatform>());
        Destroy(GetComponent<FakeFrictionOnObject>());
        Destroy(GetComponent<Rigidbody2D>());
        LevelInfoContainer.allBoxPhysicsObjects.Remove(gameObject);
        Destroy(this);
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            if (bottom.isVisible || top.GetComponent<Renderer>().isVisible)
            {
                size -= shrinkSpeed * Time.timeScale;
            }
        }

        if (size <= 0f)
        {
            if (transform.parent)
            {
                Instantiate(popEffect, transform.position, Quaternion.identity, transform.parent);
                //Instantiate(money, transform.position, Quaternion.identity, transform.parent);
            }
            else
            {
                Instantiate(popEffect, transform.position, Quaternion.identity);
                //Instantiate(money, transform.position, Quaternion.identity);
            }
            myAudio.Stop();
            myAudio.clip = popSound;
            myAudio.Play();
            DestroyRoutine();
            Destroy(gameObject, 1f);
        }
        else
        {
            top.transform.position = transform.position + (transform.up * (16f + size));
            //top.MovePosition(top.transform.position);
            middle.SetPosition(0, new Vector3(0f, (size + 8f) / middle.transform.localScale.y));
            float s2 = size + 31.6f;
            middleCol.size = new Vector2(15.6f, s2);
            middleCol.offset = new Vector2(0f, size * 0.5f + 8f);

            //print(GetComponent<Rigidbody2D>().velocity);
        }

    }
}
