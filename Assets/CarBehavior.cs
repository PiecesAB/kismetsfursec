using UnityEngine;
using System.Collections;

public class CarBehavior : MonoBehaviour {

    public byte currentMvt; //0 left 1 stopped 2 right
    public float speed;

    void Start()
    {
        currentMvt = 1;
    }

    /*void OnCollisionEnter2D(Collision2D col)
    {
        if ((col.contacts[0].normal-Vector2.down).sqrMagnitude < 0.01f)
        {
            if (col.collider.GetComponent<BasicMove>() != null)
            {
                BasicMove rg = col.collider.GetComponent<BasicMove>();
                print(rg.jumpHeight * 1.4f);
                col.collider.GetComponent<BasicMove>().GroundSelf(GetComponent<Collider2D>());
                col.collider.GetComponent<BasicMove>().fakePhysicsVel = new Vector2(rg.fakePhysicsVel.x, rg.jumpHeight*1.4f);
            }
            Destroy(gameObject);
        }
    }*/

        void Update () {
        RaycastHit2D camleft = Physics2D.Raycast(transform.position+Vector3.left*25, Vector2.left, 8000, 1049344, -100000, 100000);
       
        RaycastHit2D camright = Physics2D.Raycast(transform.position + Vector3.right * 25, Vector2.right, 8000, 1049344, -100000, 100000);
        byte zx = 0;

        if (camleft && camleft.transform.gameObject.CompareTag("Player"))
        {
            zx++;
        }

        if (camright && camright.transform.gameObject.CompareTag("Player"))
        {
            zx += 2;
        }



        switch (zx)
        {
            case 0:
                //do not change
                break;
            case 1:
                currentMvt = 0; //move left but
                break;
            case 2:
                currentMvt = 2; //move right but
                
                break;
            case 3:
                currentMvt = 1; //stop
                break;
            default:
                break;
        }
        Rigidbody2D rg = GetComponent<Rigidbody2D>();

        switch (currentMvt)
        {
            case 0:
                rg.velocity = new Vector2(-speed, rg.velocity.y);
                if (!Physics2D.Raycast(transform.position + new Vector3(-11, -10), Vector2.down, 61, 1049344, -100000, 100000))
                {
                    currentMvt = 1; //stop
                    rg.velocity = new Vector2(0, rg.velocity.y);
                }
                break;
            case 1:
                rg.velocity = new Vector2(0, rg.velocity.y);
                break;
            case 2:
                rg.velocity = new Vector2(speed, rg.velocity.y);
                if (!Physics2D.Raycast(transform.position + new Vector3(11, -10), Vector2.down, 61, 1049344, -100000, 100000))
                {

                    currentMvt = 1; //stop
                    rg.velocity = new Vector2(0, rg.velocity.y);
                }
                break;
            default:
                break;
        }

        GetComponent<PrimMovingPlatform>().velocity = GetComponent<Rigidbody2D>().velocity;

    }
}
