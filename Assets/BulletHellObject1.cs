using UnityEngine;
using System.Collections;

[System.Obsolete("BulletObject.cs", true)]
public class BulletHellObject1 : MonoBehaviour {

    public bool doesMove = true;

    public Vector3 originPosition;
    public float startingDirection;
    public float startingVelocity;
    public bool isAccelerationMultiplicative;
    public float acceleration;
    public float startingTorque;
    public bool isTorqueSineWave;
    public float changeInTorque;
    public float sineMotionSpeed;
    [Range(0f,Mathf.PI*2)]
    public float sineMotionOffset;
    //public Transform playerToHit;
    public float atWhatDistanceFromCenterIsAHit;
    public float damage;
    public float killRadius = 4.5f;
    public bool destroyOnLeaveScreen;

    public double timeThisWasMade_ScriptsOnly;
    public float deletTime;

    public float fadeInTime = 0.5f;



    //private float initAlpha;

    public bool grazed;


    public SpriteRenderer spr;
    public Transform t;

    void InternalStart()
    {
        originPosition = transform.position;
        timeThisWasMade_ScriptsOnly = DoubleTime.ScaledTimeSinceLoad;
        //playerToHit = GameObject.FindGameObjectWithTag("Player").transform;
        spr = GetComponent<SpriteRenderer>();
        t = transform;
        //initAlpha = spr.color.a;
        //BulletRegister.Register(this);
        BulletController.Load();
    }

    void InternalEnd()
    {
        //BulletRegister.MarkToDestroy(this);
    }

	void Start () {
        InternalStart();
	}

    private void OnEnable()
    {
        InternalStart();
    }

    private void OnDisable()
    {
        InternalEnd();
    }

    private void OnDestroy()
    {
        InternalEnd();
    }

    /*void OnTriggerEnter2D(Collider2D other)
    {
        double timeSince = DoubleTime.ScaledTimeSinceLoad - timeThisWasMade_ScriptsOnly;

        if (timeSince < fadeInTime) // fair warning
        {
            return;
        }

        if (other.gameObject.layer == 22)
        {
            other = other.transform.parent.GetComponent<Collider2D>();
        }

        Transform playerToHit = LevelInfoContainer.GetActiveControl().transform;
        if (other.gameObject.transform == playerToHit)
        {
            if (playerToHit.gameObject.GetComponent<KHealth>())
            {
                playerToHit.gameObject.GetComponent<KHealth>().ChangeHealth(-damage,"energy bullet");
                Destroy(gameObject);
            }
        }
    }*/
}
