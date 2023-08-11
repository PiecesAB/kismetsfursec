using UnityEngine;
using System.Collections;

public class TurnstileSwitch : MonoBehaviour {

    //float xx = 0f;
    public float dx = 0f;
    float dx1 = 0f;
    public GameObject stile;
    double lastTick = 0f;
    private Vector3 lastPos;

	void Start () {
	
	}

    void A(Collider2D col)
    {
        if (col.GetComponent<Rigidbody2D>())
        {
            Vector2 myFakeVel = (transform.position - lastPos) * Time.deltaTime;
            float x = Vector2.Dot(col.GetComponent<Rigidbody2D>().velocity - myFakeVel,transform.right);
            dx1 -= x*Time.timeScale/33f;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        A(col);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        A(col);
    }

    void Update () {
        if (Time.timeScale > 0)
        {
            Vector3 s = stile.transform.localEulerAngles;
            dx = (System.Math.Abs(dx1) > System.Math.Abs(dx)) ? (dx1) : (dx1 == 0)?(Mathf.Lerp(dx, 0, 0.05f)) :(dx1);
            stile.transform.localEulerAngles = new Vector3(s.x, s.y, Mathf.Round(s.z + dx));
            dx1 = 0;
            if ((DoubleTime.UnscaledTimeRunning-lastTick)*System.Math.Abs(dx)>0.4f)
            {
                GetComponent<AudioSource>().pitch = (dx > 0f) ? (0.9f) : (1.1f);
                GetComponent<AudioSource>().Play();
                lastTick = DoubleTime.UnscaledTimeRunning;
            }
            lastPos = transform.position;
        }
    }
}
