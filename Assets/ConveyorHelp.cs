using UnityEngine;
using System.Collections;

public class ConveyorHelp : MonoBehaviour {

    public float speed;
    public float verticalDownSpeed;
    public bool vertical;
    public Collider2D trigger;
    public Animator wheel;
    public Renderer surfaceRenderer;

    private float lastTimeScale = 0f;
    private float oldSpeed = 0f;

    private const float wheelSpeedNormal = 100f;

	void Start () {
        oldSpeed = speed;
        if (vertical)
        {

        }
        else
        {
            GetComponent<PrimMovingPlatform>().velocity = new Vector2(speed, 0);
        }
        if (wheel)
        {
            wheel.speed = Mathf.Abs(speed / wheelSpeedNormal);
        }
	}

    void OnTriggerStay2D(Collider2D hi)
    {
        //print("dj");
        if (lastTimeScale == Time.timeScale)
        {
            if (hi.GetComponent<ConveyorHelp>() || hi.GetComponent<AmorphousGroundTileNormal>())
            {
                Physics2D.IgnoreCollision(hi, trigger, true);
            }
            if (vertical)
            {

                if (hi.GetComponent<BasicMove>() != null && hi.GetComponent<Encontrolmentation>() != null)
                {
                    if (!hi.GetComponent<BasicMove>().CanCollide) { return; }
                    float xdif = hi.transform.position.x - transform.position.x;
                    float xdif2 = xdif;
                    float flip = 1f;
                    if (speed < 0)
                    {
                        xdif2 *= -1;
                        flip = -1f;
                    }
                    if ((hi.GetComponent<Encontrolmentation>().currentState & 3UL) == ((xdif > 0) ? 1UL : 2UL))
                    {
                        BasicMove b = hi.transform.GetComponent<BasicMove>();
                        float s = (((xdif2 < 0) ? (flip * speed) : verticalDownSpeed) * Time.deltaTime / Time.timeScale);
                        if (b.grounded != 0 && s < 0) { return; } // don't move down when on ground
                        b.fakePhysicsVel = new Vector2(b.fakePhysicsVel.x, b.fakePhysicsVel.y + s);
                        b.udm = 2;
                    }
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D hi)
    {
        if (vertical)
        {

        }
        else
        {
            float z = Mathf.Sign(speed);
            BasicMove bm = hi.transform.GetComponent<BasicMove>();
            if (bm)
            {
                if ((hi.contacts[0].normal - Vector2.up).sqrMagnitude <= 0.708f)
                {
                    bm.fakePhysicsVel = new Vector2(-speed * 2f / Time.timeScale, bm.fakePhysicsVel.y);
                }
                if ((hi.contacts[0].normal - z * Vector2.left).sqrMagnitude <= 0.708f)
                {
                    bm.fakePhysicsVel = new Vector2(speed * 2f / Time.timeScale, bm.fakePhysicsVel.y);
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D hi)
    {
        if (hi.transform.GetComponent<FakeFrictionOnObject>() && hi.rigidbody)
        {
            hi.rigidbody.velocity = new Vector2(speed * Vector2.Dot(hi.GetContact(0).normal, Vector2.down) / Time.timeScale, hi.rigidbody.velocity.y);
        }
    }

    private float lastM = 0;

    void Update()
    {
        float m = speed / wheelSpeedNormal;
        if (wheel)
        {
            wheel.speed = Mathf.Abs(speed);
        }
        if (lastM != m && surfaceRenderer)
        {
            surfaceRenderer.material.SetFloat("_Speed", m);
        }
        lastTimeScale = Time.timeScale;

        if (speed != oldSpeed)
        {
            GetComponent<PrimMovingPlatform>().velocity = new Vector2(speed, 0);
            oldSpeed = speed;
        }
        lastM = m;
    }

}
