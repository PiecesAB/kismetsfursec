using UnityEngine;
using System.Collections;

public class BulletHellObject2 : MonoBehaviour {

    public Vector3 originPosition;
    public double startingDirection;
    public double  startingVelocity;
    public bool isAccelerationMultiplicative;
    public double acceleration;
    public double startingTorque;
    public bool isTorqueSineWave;
    public double  changeInTorque;
    public double sineMotionSpeed;
    [Range(0f,Mathf.PI*2)]
    public double sineMotionOffset;
    public Transform playerToHit;
    public double atWhatDistanceFromCenterIsAHit;
    public float damage;

    public double timeThisWasMade_ScriptsOnly;
    public double deletTime;

    private bool seen;

	// Use this for initialization
	void Start () {
        originPosition = transform.position;
        timeThisWasMade_ScriptsOnly = DoubleTime.ScaledTimeSinceLoad;
        playerToHit = GameObject.FindGameObjectWithTag("Player").transform;
        seen = false;
	}
	
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.transform == playerToHit)
        {
            if (playerToHit.gameObject.GetComponent<KHealth>())
            {
                playerToHit.gameObject.GetComponent<KHealth>().ChangeHealth(-damage,"energy bullet");
                Destroy(gameObject);
            }
        }
    }

	// Update is called once per frame
	void Update () {

        double timeSince = DoubleTime.ScaledTimeSinceLoad - timeThisWasMade_ScriptsOnly;

        if (timeSince>deletTime)
        {
            Destroy(gameObject);
        }

        if (timeSince >0.5f && !seen)
        {
            Destroy(gameObject);
        }

        if (!seen & GetComponent<SpriteRenderer>().isVisible)
        {
            seen = true;
        }

        if (seen & !GetComponent<SpriteRenderer>().isVisible)
        {
            Destroy(gameObject);
        }

        /*if (playerToHit != null)
        {
            if ((playerToHit.gameObject.GetComponent<Renderer>().bounds.center - transform.position).magnitude < atWhatDistanceFromCenterIsAHit)
            {
                if (playerToHit.gameObject.GetComponent<KHealth>())
                {
                    playerToHit.gameObject.GetComponent<KHealth>().ChangeHealth(-damage);
                    Destroy(gameObject);
                }
            }
        }*/


        double currVelocity = 0;
        
        if (isAccelerationMultiplicative)
        {
            currVelocity = startingVelocity * (System.Math.Pow(acceleration, timeSince)) - acceleration / 2;
        }
        if (!isAccelerationMultiplicative)
        {
            currVelocity = (float)(startingVelocity + (acceleration * timeSince / 2));
        }


        double currTorque = 0;

        double currDirection = 0;
        if (isTorqueSineWave)
        {
            currTorque = startingTorque;
            currDirection = startingDirection + (currTorque * timeSince) + (changeInTorque * System.Math.Sin((sineMotionSpeed*timeSince)+sineMotionOffset));
            
        }
        if (!isTorqueSineWave)
        {
            
            currTorque = startingTorque + (changeInTorque * timeSince);
            currDirection = startingDirection + (currTorque * timeSince);
            
        }

        double xChange = System.Math.Cos(currDirection*Mathf.Deg2Rad)*currVelocity;
        double yChange = System.Math.Sin(currDirection*Mathf.Deg2Rad)*currVelocity;

        transform.position = originPosition + new Vector3((float)xChange,(float)yChange,0);
    }
}
