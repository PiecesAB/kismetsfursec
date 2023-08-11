using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachedAlienSpirit : GenericBlowMeUp
{
    public enum State
    {
        HeadSleep, HeadFollow, HeadComplete, Body
    }

    public State state = State.HeadSleep;
    [Range(0, 99)]
    public int id;
    public Sprite[] heads;
    public SpriteRenderer headSprite;
    public SpriteRenderer bodySprite;
    public ParticleSystem snoreParticles;
    public TextMesh idText;
    public Transform movementComplex;
    public Transform selfInMovementComplex;
    public GameObject shooter;
    public GameObject[] extraShooters;
    private Animator animator;

    public AudioSource wakeSound;
    public AudioSource yaySound;

    private static Dictionary<GameObject, List<DetachedAlienSpirit>> attachments = new Dictionary<GameObject, List<DetachedAlienSpirit>>();

    private void PlayAnimation()
    {
        switch (state)
        {
            case State.Body: animator.CrossFade("Body", 0f); break;
            case State.HeadComplete: animator.CrossFade("Complete", 0f); break;
            case State.HeadFollow: animator.CrossFade("Follow", 0f); break;
            case State.HeadSleep: animator.CrossFade("Sleep", 0f); break;
        }
    }

    private IEnumerator Bye()
    {
        yield return new WaitForSeconds(1f);
        BlowMeUp();
    }

    public void Complete()
    {
        state = State.HeadComplete;
        Destroy(movementComplex.gameObject);
        PlayAnimation();
        transform.SetParent(null, true);
        if (shooter) { Destroy(shooter); shooter = null; }
        foreach (GameObject s in extraShooters)
        {
            if (s)
            {
                Destroy(s);
            }
        }
        yaySound.Stop();
        yaySound.Play();
        StartCoroutine(Bye());
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        switch (state)
        {
            case State.Body:
                if (attachments.ContainsKey(col.gameObject))
                {
                    bool destroyThis = false;
                    for (int i = 0; i < attachments[col.gameObject].Count; ++i)
                    {
                        DetachedAlienSpirit d = attachments[col.gameObject][i];
                        if (d.id == id)
                        {
                            destroyThis = true;
                            d.Complete();
                            attachments[col.gameObject].RemoveAt(i);
                            --i;
                        }
                    }
                    if (destroyThis)
                    {
                        Destroy(gameObject);
                    }
                }
                break;
            case State.HeadSleep:
                if (col.gameObject.layer != 20 && col.gameObject.layer != 19) { break; }
                Transform victim = col.transform;
                while (victim.parent) { victim = victim.parent; }
                movementComplex.gameObject.SetActive(true);
                movementComplex.SetParent(victim, false);
                movementComplex.localPosition = Vector3.zero;
                movementComplex.localRotation = Quaternion.identity;
                movementComplex.localScale = Vector3.one;
                transform.SetParent(selfInMovementComplex.parent, false);
                transform.localPosition = selfInMovementComplex.localPosition;
                transform.localRotation = selfInMovementComplex.localRotation;
                transform.localScale = selfInMovementComplex.localScale;
                Destroy(selfInMovementComplex.gameObject);
                Destroy(GetComponent<Collider2D>());
                state = State.HeadFollow;
                PlayAnimation();
                if (!attachments.ContainsKey(col.gameObject))
                {
                    attachments.Add(col.gameObject, new List<DetachedAlienSpirit>());
                }
                attachments[col.gameObject].Add(this);
                if (shooter) { shooter.SetActive(true); }
                foreach (GameObject s in extraShooters)
                {
                    s.SetActive(true);
                }
                wakeSound.Stop();
                wakeSound.Play();
                break;
            default: break;
        }
    }

    private void Start()
    {
        idText.text = id.ToString();
        Color c = Color.Lerp(Utilities.colorCycle[id % 32], new Color(1f, 1f, 1f, 0.4f), 0.5f);
        Color cb = Color.Lerp(c, Color.white, 0.5f);
        idText.color = headSprite.color = bodySprite.color = c;
        headSprite.sprite = heads[id % heads.Length];
        ParticleSystem.MainModule mm = snoreParticles.main;
        mm.startColor = cb;
        animator = GetComponent<Animator>();
        PlayAnimation();
    }

    private void OnEnable()
    {
        if (state != State.HeadFollow) { return; }
        // restart the shooters. a bug happens where it stops shooting if the plr teleports.
        if (shooter)
        {
            GameObject os = shooter;
            shooter = Instantiate(shooter, shooter.transform.position, shooter.transform.rotation, shooter.transform.parent);
            Destroy(os);
        }
        foreach (GameObject s in extraShooters)
        {
            GameObject os = s;
            shooter = Instantiate(s, s.transform.position, s.transform.rotation, s.transform.parent);
            Destroy(os);
        }

    }
}
