using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lanugo : MonoBehaviour
{
    private BoxCollider2D colliderMain;
    public int hairPerBlockWidth = 16;
    public GameObject[] hairSamples;
    public SpriteRenderer inactiveSprite;

    public AudioClip enterSound;
    public AudioClip exitSound;

    private int touchCount = 0;
    private List<GameObject> hairs = new List<GameObject>();
    private Dictionary<GameObject, int> myCollisions = new Dictionary<GameObject, int>();
    private static Dictionary<GameObject, int> allCollisions = new Dictionary<GameObject, int>();

    private const int waitBeforeAbleToMove = 4;

    private void GenerateHairUnits()
    {
        inactiveSprite.enabled = false;
        colliderMain = GetComponent<BoxCollider2D>();
        float widthTotal = colliderMain.size.x;
        for (float x = -widthTotal * 0.5f; x < widthTotal * 0.5f; x += 16f)
        {
            for (int i = 0; i < hairPerBlockWidth; ++i)
            {
                int p = -hairPerBlockWidth / 2 + i;
                GameObject sample = hairSamples[Fakerand.Int(0, hairSamples.Length)];
                GameObject ns = Instantiate(sample,
                    transform.position + transform.right * Fakerand.Single(x, x + 16f) + Vector3.forward * p,
                    transform.rotation, transform);
                ns.SetActive(true);
                hairs.Add(ns);
                SpriteRenderer sr = ns.GetComponent<SpriteRenderer>();
                if (p < 0)
                {
                    float p2 = 1f / (1 - p * 0.1f);
                    sr.color = new Color(1, p2, p2, p2);
                    sr.material.SetFloat("_Random1", Fakerand.Single() * 6.28f);
                }
            }
        }
    }

    private void DestroyHairUnits()
    {
        foreach (GameObject hair in hairs)
        {
            Destroy(hair);
        }
        hairs.Clear();
        inactiveSprite.enabled = true;
    }

    private bool reacting = false;

    private IEnumerator React()
    {
        if (reacting) { yield break; }
        reacting = true;
        while (myCollisions.Count != 0)
        {
            Vector2 v = Vector2.zero;
            int i = 1;
            foreach (var kv in myCollisions)
            {
                v = Vector2.Lerp(v, kv.Key.transform.position, 1f / i);
                ++i;
            }
            foreach (GameObject hair in hairs)
            {
                // hair is 8 pixels tall
                Transform t = hair.transform;
                t.localPosition = new Vector3(t.localPosition.x, 0f, t.localPosition.z);
                t.localScale = Vector3.one;
                t.localRotation = Quaternion.identity;
                float s = Mathf.Clamp(6f - 0.4f * Mathf.Abs(t.InverseTransformPoint(v).x), 0f, 1f);
                t.localScale = new Vector3(1f, s + 1f, 1f);
                t.localPosition = new Vector3(t.localPosition.x, (t.localScale.y - 1) * 4f, t.localPosition.z);
                t.LookAt(t.position + Vector3.forward, v - (Vector2)t.position);
                t.localRotation = Quaternion.Lerp(t.localRotation, Quaternion.identity, 1f - s * s * 0.5f);
            }

            yield return new WaitForEndOfFrame();
        }
        reacting = false;
        yield return null;
    }

    private void Unreact()
    {
        foreach (GameObject hair in hairs)
        {
            // hair is 8 pixels tall
            Transform t = hair.transform;
            t.localScale = Vector3.one;
            t.localPosition = new Vector3(t.localPosition.x, 0f, t.localPosition.z);
            t.localRotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 8 || col.isTrigger) { return; }

        if (!myCollisions.ContainsKey(col.gameObject))
        {
            myCollisions[col.gameObject] = 0;
        }
        if (!allCollisions.ContainsKey(col.gameObject))
        {
            allCollisions[col.gameObject] = 0;
            if (col.gameObject.layer == 20)
            {
                GameObject k = col.gameObject;
                BasicMove bm = k.GetComponent<BasicMove>();
                TranquilizerSpeedMod tsm = k.GetComponent<TranquilizerSpeedMod>();
                bool sideways = Mathf.Abs(Vector2.Dot(col.transform.right, transform.up)) > 0.7f;
                if (!tsm)
                {
                    tsm = k.gameObject.AddComponent<TranquilizerSpeedMod>();
                    float rms = -bm.moveSpeed * 0.999f;
                    float rmj = -bm.jumpHeight * 0.6f * 0.6f;
                    float rmf = sideways ? -0.7f : 0f;
                    tsm.speedChange += rms;
                    bm.moveSpeed += rms;
                    tsm.jumpHeightChange += rmj;
                    bm.jumpHeight += rmj;
                    tsm.maxFallSpeedMultChange += rmf;
                    tsm.refreshDoubleJumpOnRecover = !sideways;
                    bm.maxFallSpeedTranqMult += rmf;
                    tsm.removeJumpWhenNotGrounded = true;
                }

                AudioSource a = GetComponent<AudioSource>();
                a.Stop();
                a.clip = enterSound;
                a.Play();
            }
        }
        ++myCollisions[col.gameObject];
        ++allCollisions[col.gameObject];
        StartCoroutine(React());
    }

    private IEnumerator CountdownToMove(TranquilizerSpeedMod tsm)
    {
        for (int i = 0; i < waitBeforeAbleToMove; ++i)
        {
            if (allCollisions.ContainsKey(tsm.gameObject))
            {
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
        if (tsm) { tsm.RemoveTranquilizer(); }
        AudioSource a = GetComponent<AudioSource>();
        a.Stop();
        a.clip = exitSound;
        a.Play();
        yield return null;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == 8 || col.isTrigger) { return; }

        --myCollisions[col.gameObject];
        --allCollisions[col.gameObject];
        if (myCollisions[col.gameObject] == 0)
        {
            myCollisions.Remove(col.gameObject);
        }
        if (allCollisions[col.gameObject] == 0)
        {
            allCollisions.Remove(col.gameObject);
            if (col.gameObject.layer == 20)
            {
                TranquilizerSpeedMod tsm = col.gameObject.GetComponent<TranquilizerSpeedMod>();
                if (tsm) { StartCoroutine(CountdownToMove(tsm)); }
            }
        }
        Unreact();
    }

    private void OnBecameVisible()
    {
        GenerateHairUnits();
    }

    private void OnBecameInvisible()
    {
        DestroyHairUnits();
    }


    void Start()
    {
        DestroyHairUnits();
    }
}
