using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingBookshelf : MonoBehaviour, ILargeFanGravity
{
    public float speed = 64f;
    private Rigidbody2D r2;
    private Transform t;
    private bool falling = false;
    [SerializeField]
    private Transform rotateHandle;
    [SerializeField]
    private AudioClip movingSound;
    [SerializeField]
    private AudioClip stopSound;
    [SerializeField]
    private Transform shelfPicture;

    private AudioSource aso;
    private Renderer rotateHandleRenderer;
    private Color rhrColor;

    private const float gravScale = 50;

    private double lastPlayedStopSound = -5.0;

    private Dictionary<GameObject, int> allCollisions = new Dictionary<GameObject, int>();

    private float lastSpeed = 0f;

    private void OnTriggerEnter2D(Collider2D col)
    {
        GameObject g = col.gameObject;
        if (g.layer != 20) { return; }
        if (allCollisions.Count == 0 && DoubleTime.UnscaledTimeSinceLoad - lastPlayedStopSound >= 0.3f)
        {
            lastPlayedStopSound = DoubleTime.UnscaledTimeSinceLoad;
            aso.Stop();
            aso.loop = false;
            aso.clip = stopSound;
            aso.Play();
        }
        if (!allCollisions.ContainsKey(g)) { allCollisions[g] = 0; }
        ++allCollisions[g];
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        GameObject g = col.gameObject;
        if (g.layer != 20) { return; }
        --allCollisions[g];
        if (allCollisions[g] == 0) { allCollisions.Remove(g); }
    }

    void Start()
    {
        r2 = GetComponent<Rigidbody2D>();
        if (Mathf.Abs(180f - Mathf.Repeat(transform.eulerAngles.z, 360f)) < 45f)
        {
            // restructure: there is a weird bug with Raycasts not hitting upside-down rigidbodies
            GameObject g = Instantiate(gameObject, transform.parent);
            foreach (Transform t in g.transform)
            {
                if (t == g.transform) { continue; }
                Destroy(t.gameObject);
            }
            g.transform.rotation = Quaternion.identity;
            
            foreach (Collider2D c in g.GetComponents<Collider2D>())
            {
                Destroy(c);
            }
            Destroy(g.GetComponent<SlidingBookshelf>());
            Destroy(g.GetComponent<AudioSource>());
            Destroy(r2);
            r2 = g.GetComponent<Rigidbody2D>();
            transform.SetParent(g.transform);
        }
        r2.gravityScale = 0f; // custom directional gravity is needed
        t = transform;
        falling = false;
        aso = GetComponent<AudioSource>();
        rotateHandleRenderer = rotateHandle.GetComponent<Renderer>();
        rhrColor = rotateHandleRenderer.material.color;
        if (shelfPicture)
        {
            shelfPicture.position = transform.position - new Vector3(0, 2, 0);
        }
    }

    private float HorizontalSpeed()
    {
        return Vector2.Dot(r2.velocity, transform.right);
    }

    private float VerticalSpeed()
    {
        return Vector2.Dot(r2.velocity, transform.up);
    }

    private Vector2 HorizontalMovement()
    {
        return HorizontalSpeed() * transform.right;
    }

    private Vector2 VerticalMovement()
    {
        return VerticalSpeed() * transform.up;
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }

        Encontrolmentation e = LevelInfoContainer.GetActiveControl();

        if (e)
        {
            if ((e.transform.position - transform.position).magnitude > 480f)
            {
                r2.bodyType = RigidbodyType2D.Static;
                return;
            }
            else
            {
                r2.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        bool stopped = false;
        if (allCollisions.Count != 0 && !falling)
        {
            r2.velocity = VerticalMovement();
            r2.constraints = RigidbodyConstraints2D.FreezeAll;
            stopped = true;
        }
        else if (!falling)
        {
            if (!e)
            {
                r2.velocity = VerticalMovement();
                r2.constraints = RigidbodyConstraints2D.FreezeAll;
                stopped = true;
            }
            else
            {
                Vector2 epos = t.InverseTransformPoint(e.transform.position);
                float s = Mathf.Sign(epos.x);
                r2.velocity = (Mathf.MoveTowards(HorizontalSpeed(), s * speed, 8f) * (Vector2)transform.right) + VerticalMovement();
                r2.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        // fall speed
        falling = (Mathf.Abs(VerticalSpeed()) >= 1);
        if (falling)
        {
            float s = Mathf.Sign(HorizontalSpeed());
            if (Mathf.Abs(s) < 8f) { s = Mathf.Sign(lastSpeed); }
            if (s == 0f) { s = 1; }
            r2.velocity = (s * speed * (Vector2)transform.right) + VerticalMovement();
        }
        if (VerticalSpeed() < -speed)
        {
            r2.velocity = HorizontalMovement() - speed * (Vector2)transform.up;
            r2.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        if (VerticalSpeed() > speed)
        {
            r2.velocity = HorizontalMovement() + speed * (Vector2)transform.up;
            r2.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (Mathf.Abs(HorizontalSpeed()) / speed >= 0.3f)
        {
            if (!aso.isPlaying || aso.clip != movingSound)
            {
                aso.Stop();
                aso.loop = true;
                aso.clip = movingSound;
                aso.Play();
            }
        }
        else
        {
            if (aso.isPlaying && aso.clip == movingSound)
            {
                aso.Stop();
            }
        }

        // rotate handle
        float rz = rotateHandle.localEulerAngles.z;
        rotateHandle.localEulerAngles += Vector3.back * HorizontalSpeed() * Time.deltaTime * 8f;
        rotateHandleRenderer.material.color = stopped ? Color.red : rhrColor;
    }

    private float fanGravityY = 0;
    private int useFanGravity = 0;

    private void LateUpdate()
    {
        if (r2.bodyType == RigidbodyType2D.Dynamic)
        {
            float m = -8;
            if (useFanGravity > 0)
            {
                m += fanGravityY;
                --useFanGravity;
            }
            r2.velocity += (Vector2)transform.up * m * gravScale * Time.deltaTime;
        }
        lastSpeed = HorizontalSpeed();
    }

    public void OnStayInFan(LargeFan fan, Vector2 additionalGravDir)
    {
        useFanGravity = 3;
        fanGravityY = additionalGravDir.y;
    }
}
