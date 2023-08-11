using UnityEngine;
using System.Collections;

public class SpringBhvr : MonoBehaviour {

    public float power;
    public bool oneTimeUse;
    public AnimationClip springAnimation;
    public bool absurdlyPowerful = false;
    public GameObject gameJokeSign;

    private bool on = true;

    [HideInInspector]
    public PlatformControlButton pcbSurrogate;

	// Use this for initialization
	void Start () {
        on = true;
	}

    public IEnumerator SpringKO(Renderer rend, KHealth kh)
    {
        yield return new WaitUntil(() => (!rend.isVisible));
        kh.ChangeHealth(Mathf.NegativeInfinity, "spring ko");
        yield return new WaitForSecondsRealtime(0.125f);
        GameObject gs = Instantiate(gameJokeSign, new Vector3(320f,240f,0f), Quaternion.identity, GameObject.FindWithTag("HUD").transform);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (on)
        {
            float angery = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(-Mathf.Sin(angery), Mathf.Cos(angery));
            //try
            {
                Rigidbody2D r = col.gameObject.GetComponent<Rigidbody2D>();
                if (r && !r.isKinematic)
                {
                    GetComponent<Animator>().Play("spring");
                    r.velocity = dir*power;
                    GetComponent<AudioSource>().pitch = 1f + Fakerand.Single(-0.06f,0.06f);
                    GetComponent<AudioSource>().Play();
                    if (pcbSurrogate) { pcbSurrogate.TryOn(col); }
                }
            }
            //catch
            //{
            //    print("the spring tried to bounce a non-physics object");
            //}
            //try
            {
                if (absurdlyPowerful)
                {
                    print("lol");
                    BasicMove b = col.GetComponent<BasicMove>();
                    Rigidbody2D r = col.GetComponent<Rigidbody2D>();
                    if (r && b)
                    {
                        r.velocity = (new Vector2(-Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad))) * 2000f;
                        b.enabled = false;
                        Camera.main.GetComponent<FollowThePlayer>().target = null;
                        foreach (var c in col.GetComponents<Collider2D>())
                        {
                            c.enabled = false;
                        }
                        StartCoroutine(SpringKO(col.GetComponent<SpriteRenderer>(), col.GetComponent<KHealth>()));
                    }
                }
                else
                {
                    BasicMove b = col.GetComponent<BasicMove>();
                    if (b)
                    {
                        b.jumping = true; b.grounded = 0; b.doubleJump = true; b.iced = 0;
                        b.fakePhysicsVel = dir * power / (Time.timeScale * Mathf.Abs(b.transform.localScale.y));
                        GetComponent<AudioSource>().pitch = 1f + Fakerand.Single(-0.06f, 0.06f);
                        GetComponent<AudioSource>().Play();
                        if (pcbSurrogate) { pcbSurrogate.TryOn(col); }
                    }
                }
            }
            //catch
            //{
            //    print("no player");
            //}
            if (oneTimeUse)
            {
                on = false;
                GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
