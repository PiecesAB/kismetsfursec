using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavalMine : GenericBlowMeUp
{
    [Header("v1 is explosion type velocity")]
    public float v1 = 1800f;
    [Header("d is the damage if applicable")]
    public float d;
    [Header("my main renderer (for mine)")]
    public Renderer myMainRenderer = null;
    public AudioSource soundObj;
    [Header("e is the explosion object, let's say it's bullets")]
    public GameObject e;
    public bool infinite;
    public tripwire instantTimedLoss;
    // negative = disabled.
    public float destroyTimeAfterVisible = -1;
    public AudioSource alertDestroySound;

    private double madeTime;
    private Rigidbody2D r2;

    private void Start()
    {
        name = "Mine";
        r2 = GetComponent<Rigidbody2D>();
        Prim3DRotate p3 = GetComponent<Prim3DRotate>();
        if (p3)
        {
            if (p3.speed == 1)
            {
                transform.rotation = Random.rotation;
                GetComponent<Prim3DRotate>().speed = (Fakerand.Int(0, 2) * 2) - 1;
            }
        }
        if (!p3) { p3 = GetComponentInChildren<Prim3DRotate>(); }
        p3.CallOnVisibleChange = VisChange;
        VisChange(p3.offScreenChecker.isVisible);
        
        madeTime = DoubleTime.UnscaledTimeSinceLoad;
    }

    private bool finalizedDestroy = false;
    private void FinalizeDestroy(float delay)
    {
        if (finalizedDestroy) { return; }
        finalizedDestroy = true;
        Destroy(GetComponent<Prim3DRotate>());
        Destroy(GetComponent<Collider2D>());
        if (myMainRenderer) { Destroy(myMainRenderer); }
        BlowMeUp(delay);
    }

    private bool visibleTriggered = false;
    private IEnumerator AlertDestroy()
    {
        if (destroyTimeAfterVisible > 0.6f)
        {
            yield return new WaitForSeconds(destroyTimeAfterVisible - 0.6f);
        }
        float t = Mathf.Min(0.6f, destroyTimeAfterVisible);
        while (t > 0)
        {
            if (myMainRenderer)
            {
                myMainRenderer.material.color = (myMainRenderer.material.color == Color.white) ? Color.black : Color.white;
            }
            if (alertDestroySound)
            {
                alertDestroySound.Stop(); alertDestroySound.Play();
            }
            yield return new WaitForSeconds(0.06f);
            t -= 0.06f;
        }
        if (finalizedDestroy) { yield break; }
        float delay = 0f;
        if (soundObj != null)
        {
            soundObj.Play();
            delay = soundObj.clip.length;
        }
        CreateBullet();
        FinalizeDestroy(delay);
    }

    public bool VisChange(bool vis)
    {
        print("!");
        if (vis)
        {
            r2.bodyType = RigidbodyType2D.Dynamic;
            r2.sleepMode = RigidbodySleepMode2D.StartAwake;
            if (destroyTimeAfterVisible >= 0f && !visibleTriggered)
            {
                visibleTriggered = true;
                StartCoroutine(AlertDestroy());
            }
        }
        else
        {
            r2.bodyType = RigidbodyType2D.Static;
            r2.sleepMode = RigidbodySleepMode2D.StartAsleep;
        }
        return true;
    }

    private void CreateBullet()
    {
        if (e) //bullet
        {
            GameObject g = Instantiate(e, transform.position + Vector3.back * 16f, Quaternion.identity, transform.root);
            if (!g.activeSelf)
            {
                g.SetActive(true);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (DoubleTime.UnscaledTimeSinceLoad - madeTime < 0.05f)
        {
            return;
        }
        if ((col.rigidbody && !col.rigidbody.isKinematic) || col.gameObject.CompareTag("SuperRay"))
        {
            BasicMove bm = col.gameObject.GetComponent<BasicMove>();
            if (bm != null)
            {
                Vector2 v = -col.contacts[0].normal;
                foreach (Vector2 c in new Vector2[4] { Vector2.up, Vector2.right, Vector2.left, Vector2.down })
                {
                    if (Vector2.Dot(v, c) > 0.7f)
                    {
                        v = c;
                    }
                }
                bm.fakePhysicsVel = v * v1;
                bm.doubleJump = true;
            }
            else if (col.rigidbody != null)
            {
                col.rigidbody.velocity = -col.contacts[0].normal * v1;
            }

            if (col.gameObject.GetComponent<KHealth>() && d > 0)
            {
                col.gameObject.GetComponent<KHealth>().ChangeHealth(-d * 0.125f * bm.Damage, "mine");
            }
            if (bm != null && d > 0)
            {
                bm.AddBlood(col.contacts[0].point, Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(col.contacts[0].normal.y, col.contacts[0].normal.x)));
            }

            if (instantTimedLoss)
            {
                instantTimedLoss.Trip();
                LevelInfoContainer.timer = 0f;
                BGMController.main.SetMusicTime(61f);
            }

            float delay = 0f;
            if (soundObj != null)
            {
                soundObj.Play();
                delay = soundObj.clip.length;
            }
            CreateBullet();

            if (infinite)
            {
                ExplodeEffectOnly();
                return;
            }

            FinalizeDestroy(delay);
        }
    }
}
