using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraScope : MonoBehaviour
{


    public enum State
    {
        Move, Aim, Fire
    }

    [HideInInspector]
    public State state;
    public SpriteRenderer onScreenChecker;
    [Header("boundBox null = use waypoints")]
    public BoxCollider2D boundBox;
    public Transform waypoints;
    public double moveTime;
    public double aimTime;
    public double fireTime;
    public AudioClip warnSound;
    public AudioClip fireSound;

    private Vector2 nextPos;
    private Vector2 currPos;
    private double timer;
    private int currWaypoint = 0;

    private Animator anim;
    private Renderer[] rends;
    private AudioSource audSrc;

    private void SetNextPos()
    {
        if (boundBox)
        {
            nextPos = new Vector3(
                Fakerand.Single(boundBox.bounds.min.x, boundBox.bounds.max.x),
                Fakerand.Single(boundBox.bounds.min.y, boundBox.bounds.max.y),
                transform.position.z
            );
        }
        else if (waypoints)
        {
            ++currWaypoint;
            if (currWaypoint == waypoints.childCount) { currWaypoint = 0; }
            Vector3 p = waypoints.GetChild(currWaypoint).position;
            nextPos = new Vector3(p.x, p.y, transform.position.z);
        }
    }

    void Start()
    {
        timer = 0.0;
        anim = GetComponent<Animator>();
        rends = GetComponentsInChildren<Renderer>();
        audSrc = GetComponent<AudioSource>();
        currPos = transform.position;
        if (!boundBox && waypoints)
        {
            currPos = transform.position = waypoints.GetChild(0).position;
        }
        SetNextPos();
    }
    

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Move:
                transform.position = Vector3.Lerp(currPos, nextPos, (float)(timer / moveTime));
                for (int i = 0; i < rends.Length; ++i)
                {
                    rends[i].material.color = Color.red;
                    rends[i].material.SetColor("_EmissionColor", Color.red);
                }
                if (timer >= moveTime)
                {
                    currPos = nextPos;
                    transform.position = nextPos; 
                    timer -= moveTime;
                    state = State.Aim;
                    audSrc.Stop();
                    audSrc.clip = warnSound;
                    audSrc.Play();
                }
                break;
            case State.Aim:
                for (int i = 0; i < rends.Length; ++i)
                {
                    rends[i].material.color = (timer % 0.13333 >= 0.06666) ? Color.red : Color.black;
                    rends[i].material.SetColor("_EmissionColor", (timer % 0.13333 >= 0.06666) ? Color.red : Color.black);
                }
                if (timer >= aimTime)
                {
                    timer -= aimTime;
                    state = State.Fire;
                    anim.SetTrigger("On");
                    if (onScreenChecker.isVisible && FollowThePlayer.main.vibSpeed < 8f)
                    {
                        FollowThePlayer.main.vibSpeed = 8f;
                    }
                    audSrc.Stop();
                    audSrc.clip = fireSound;
                    audSrc.Play();
                    foreach (BulletHellMakerFunctions bh in GetComponentsInChildren<BulletHellMakerFunctions>())
                    {
                        bh.Fire();
                    }
                }
                break;
            case State.Fire:
                for (int i = 0; i < rends.Length; ++i)
                {
                    rends[i].material.color = Color.red;
                    rends[i].material.SetColor("_EmissionColor", Color.red);
                }
                if (timer >= fireTime)
                {
                    timer -= fireTime;
                    state = State.Move;
                    anim.SetTrigger("Off");
                    SetNextPos();
                }
                break;
            default:
                break;
        }
    }
}
