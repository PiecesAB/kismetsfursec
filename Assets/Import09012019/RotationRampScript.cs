using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationRampScript : MonoBehaviour {

    //public Transform levelContainer;
    //public float rotGoal;
    //public Transform plrGoal;
    public bool frameCooldown;
    public float vel;
    //private bool queen = false;

    private void OnCollisionStay2D(Collision2D c)
    {
        if (frameCooldown && c.gameObject.CompareTag("Player"))
        {
            /*if (!queen)
            {
                FindObjectOfType<FollowThePlayer>().followCameraBounds = false;
                queen = true;
            }*/
            Vector2 dn = Vector2.down;
            Vector2 n = c.GetContact(0).normal;
            //print(n);
            if (Vector2.Dot(dn,n) >= 0.86f)
            {
                float ang1 = Mathf.Repeat(Mathf.Atan2(n.y, n.x) - 4.7123890f, 6.2831853f);
                //float ang2 = Mathf.Repeat(Mathf.Atan2(dn.y, dn.x) - 4.7123890f, 6.2831853f);
                //float angD = ang2 - ang1;
                //levelContainer.GetComponent<TestScriptRotateScene>().goalRotation = angD*Mathf.Rad2Deg + levelContainer.eulerAngles.z;
                //plrGoal = c.transform;
                c.transform.eulerAngles = new Vector3(0,0,ang1*Mathf.Rad2Deg);
                c.rigidbody.velocity += Vector2.down * 8;
            }

            frameCooldown = false;
        }
    }

    void Start()
    {
        //plrGoal = null;
        //queen = false;
    }

    void Update()
    {
        frameCooldown = true;
    }

}
