using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeFan : MonoBehaviour
{
    public float windDist = 160f;
    public ParticleSystem particles;
    // if true, try to draw the player closer to the fan
    public bool intake = false;

    private bool isError = false;

    void Start()
    {
        if (Mathf.Abs(transform.eulerAngles.z - 0f) >= 0.1f && Mathf.Abs(transform.eulerAngles.z - 180f) >= 0.1f)
        {
            isError = true;
            throw new System.Exception("Fans can only blow up or down");
        }
        if (windDist < 128f)
        {
            isError = true;
            throw new System.Exception("Blow distance is too short");
        }
        var pm = particles.main;
        float pspeed = pm.startSpeed.Evaluate(0);
        pm.startLifetime = new ParticleSystem.MinMaxCurve((windDist - 64f) / pspeed);
        if (intake)
        {
            particles.transform.localEulerAngles += Vector3.forward * 180f;
            particles.transform.localPosition = new Vector3(0f, -16f + windDist, -4f); 
        }
        BoxCollider2D trig = gameObject.AddComponent<BoxCollider2D>();
        trig.isTrigger = true;
        trig.size = new Vector2(64f, windDist);
        trig.offset = new Vector2(0f, 16f + 0.5f * windDist);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        Vector2 addGravity = (Vector2)transform.up * 16f * (intake ? -1f : 1f);
        Vector2 myGravity = Physics2D.gravity + addGravity;


        if (col.gameObject.layer == 20)
        {
            BasicMove bm = col.gameObject.GetComponent<BasicMove>();
            bm.fakeGravityOverrideFrames = 3;
            bm.fakeGravity = myGravity;
        }

        if (col.GetComponent<ILargeFanGravity>() != null)
        {
            col.GetComponent<ILargeFanGravity>().OnStayInFan(this, addGravity);
        }
    }
}
