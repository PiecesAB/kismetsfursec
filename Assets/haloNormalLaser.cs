using UnityEngine;
using System.Collections;

public class haloNormalLaser : MonoBehaviour {

    public float WaitTime;
    public float shotDurationTime;
    public float damagePerFrame;
    public Color warnLineStartColor;
    public Color warnLineEndColor;
    public Color shotColor;
    public bool shaking = false;
    public bool followingPlayer = false;
    public float jitter;

    private double LvTimeAtStart;
    private bool shotYet;
	// Use this for initialization
	void Start () {
        LvTimeAtStart = DoubleTime.ScaledTimeSinceLoad;
	}
	
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<KHealth>() && other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<KHealth>().ChangeHealth(-damagePerFrame,"");
        }
    }
	// Update is called once per frame
	void Update () {
        
        double timeSince = DoubleTime.ScaledTimeSinceLoad - LvTimeAtStart;
        GetComponent<SpriteRenderer>().color = shotColor;
        if (timeSince >= WaitTime && timeSince < WaitTime + shotDurationTime)
        {
            GetComponent<Collider2D>().enabled = true;
            transform.localScale = new Vector3(1, transform.localScale.y, 1);
        }
        else
        {
            GetComponent<Collider2D>().enabled = false;
            if (timeSince < WaitTime)
            {
                if (shaking)
                {
                    transform.position = transform.position + (Vector3)(Vector2)(Random.insideUnitSphere*jitter);
                }
                if (followingPlayer)
                {
                    float x = GameObject.FindGameObjectWithTag("Player").transform.position.x;
                    x = Mathf.Lerp(transform.position.x, x, (1 - (float)(timeSince / WaitTime)) / 4);
                    transform.position = new Vector3(x, transform.position.y, transform.position.z);
                }
                GetComponent<SpriteRenderer>().color = Color.Lerp(warnLineStartColor, warnLineEndColor, (float)(timeSince / WaitTime));
                transform.localScale = new Vector3((float)(timeSince/WaitTime)/1.5f,transform.localScale.y,1);
            }
            else
            {
                Destroy(gameObject);
            }
        }
	}
}
