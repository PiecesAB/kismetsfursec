using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class punchBag : MonoBehaviour
{
    [Range(0f, 179f)]
    public float punchPower = 120f;
    [Range(0f, 1f)]
    public float dampening = 0.003712f;
    public Prim3DRotate mainRotation;
    public Prim3DRotate auxRotation;
    public int delay;
    public AudioSource punchSound;
    public GameObject hurtDust;

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (delay == 0 && c.collider.gameObject.layer == 19) //punch
        {
            Instantiate(hurtDust, c.GetContact(0).point, Quaternion.identity);
            float currRot = transform.localEulerAngles.z;
            currRot = Mathf.Repeat(currRot + 180f, 360f) - 180f;
            //punch object is in the player, get player and find left or right position
            Vector2 plrPos = c.collider.transform.parent.position - transform.position;
            float dir = Vector2.Dot(plrPos, transform.right); //+:punched from right, -:punched from left
            float ndsign = -Mathf.Sign(dir);


            mainRotation.speed = ndsign * punchPower;
            //print(currRot / mainRotation.speed);
            mainRotation.offset = (-mainRotation.vv) + ndsign * Mathf.Asin(currRot / 240f);
            punchSound.Stop();
            punchSound.Play();
            delay = 10;
        }
    }

    void Start()
    {
        delay = 0;
    }

    void Update()
    {
        if (Time.timeScale != 0)
        {
            mainRotation.speed *= (1f - Mathf.Pow(dampening,Time.timeScale));
            auxRotation.speed = mainRotation.speed * 0.08f * Time.timeScale;
            if (System.Math.Abs(mainRotation.speed) < 2f)
            {
                mainRotation.speed = 0f;
            }
            if (delay > 0)
            {
                delay--;
            }
        }
    }
}
