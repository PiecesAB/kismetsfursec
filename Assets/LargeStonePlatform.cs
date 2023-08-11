using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeStonePlatform : MonoBehaviour
{
    //detects if it's moving based on attached primmovingplatform
    public bool amIHelper;
    public bool addChilds;
    public LargeStonePlatform mom;
    public PrimMovingPlatform movePlatScript;
    public string input;
    public string beginMovingCommand;
    public int framesDelayBeforeStart;
    [Header("z for the time")]
    public Vector3[] velocityChanges;
    public float velocityAccelerator = 1;
    public AudioClip collideSound;
    public AudioClip whirrSound;
    private float v8;
    private int currVIndex = -1;
    private double startTime;

    void Start()
    {
        currVIndex = -1;
        v8 = Mathf.Pow(velocityAccelerator, 0.01666666f);
        if (!amIHelper && addChilds)
        {
            foreach (Transform t in transform)
            {
                if (t.GetComponent<Collider2D>() != null)
                {
                    LargeStonePlatform lsn = t.gameObject.AddComponent<LargeStonePlatform>();
                    lsn.mom = this;
                    lsn.amIHelper = true;
                }
            }
        }
        if (beginMovingCommand == "")
        {
            BeginMoving();
        }
    }

    public void BeginMoving()
    {
        currVIndex = 0;
        movePlatScript.enabled = true;
        startTime = DoubleTime.ScaledTimeSinceLoad;
    }

    public void CollisionHappens()
    {
        movePlatScript.enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        //round position to the nearest 8
        Vector3 ty = transform.localPosition;
        transform.localPosition = new Vector3(Mathf.Round(ty.x * 0.125f) * 8f, Mathf.Round(ty.y * 0.125f) * 8f, ty.z);
        //vibrate screen
        Camera.main.GetComponent<FollowThePlayer>().vibSpeed += 1f;

        AudioSource aus = GetComponent<AudioSource>();
        if (aus != null)
        {
            aus.Stop();
            aus.clip = collideSound;
            aus.pitch = Fakerand.Single(0.5f, 0.7f);
            aus.loop = false;
            aus.volume = 0.25f;
            aus.Play();
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        int l = col.collider.gameObject.layer;
        if (amIHelper && mom != null && (l == 8 || l == 9) && col.collider.transform.parent != transform)
        {
            LargeStonePlatform ls = col.collider.GetComponent<LargeStonePlatform>();
            if (ls != null && ls.mom == mom)
            {
                return;
            }
            mom.CollisionHappens();
        }
    }

    void Update()
    {
        AudioSource aus = GetComponent<AudioSource>();
        float speed = GetComponent<Rigidbody2D>().velocity.magnitude;
        if (aus != null)
        {
            if (speed >= 1 && aus.clip != whirrSound)
            {
                aus.Stop();
                aus.clip = whirrSound;
                aus.loop = true;
                aus.volume = 1f;
                aus.Play();
            }

            if (aus.clip == whirrSound)
            {
                aus.pitch = Mathf.Clamp((0.1f * Mathf.PerlinNoise((float)DoubleTime.ScaledTimeSinceLoad, 2187f)) + (speed * 0.05f), 0.2f, 3f);
            }

            if (speed < 1 && aus.clip == whirrSound)
            {
                aus.Stop();
                aus.clip = null;
            }
        }

        if (beginMovingCommand != "")
        {
            if (input == beginMovingCommand)
            {
                if (framesDelayBeforeStart > 0)
                {
                    framesDelayBeforeStart--;
                }
                else
                {
                    input = "";
                    BeginMoving();
                }
            }
        }

        if (currVIndex > -1 && currVIndex < velocityChanges.Length)
        {
            if (DoubleTime.ScaledTimeSinceLoad-startTime > velocityChanges[currVIndex].z)
            {
                GetComponent<PrimMovingPlatform>().velocity = velocityChanges[currVIndex];
                currVIndex++;
            }
        }
        GetComponent<PrimMovingPlatform>().velocity *= v8;
    }
}
