using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantDeathBlock : MonoBehaviour
{
    [Header("the render cube for screen effects...")]
    public MeshRenderer rc;
    public AudioSource soundObj;
    [Header("[insert needed materials here]")]
    public Material[] mats;

    private static bool dyingInstantly = false;

    private Shader mms;
    private double trs;

    private bool toush = false;

    private SpriteRenderer sr;
    private Collider2D myCol;

    private static MeshRenderer pictCubeCache = null;

    private IEnumerator DieInstantly(KHealth deathee)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.2f);
        soundObj.Stop();
        myCol.enabled = true;
        rc.material.shader = mms;
        Time.timeScale = (float)trs;
        Utilities.canPauseGame = true;
        Utilities.canUseInventory = true;
        deathee.ChangeHealth(Mathf.NegativeInfinity, "instant death block");
    }

    private MeshRenderer GetPictCube()
    {
        if (pictCubeCache == null)
        {
            pictCubeCache = GameObject.FindGameObjectWithTag("PictCube").GetComponent<MeshRenderer>();
        }
        return pictCubeCache;
    }

    private void Awake()
    {
        if (dyingInstantly) { dyingInstantly = false; }
        sr = GetComponent<SpriteRenderer>();
        myCol = GetComponent<Collider2D>();
        if (rc == null) { rc = GetPictCube(); }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.GetComponent<KHealth>() && col.gameObject == LevelInfoContainer.GetActiveControl().gameObject)
        {
            if (dyingInstantly) { return; }
            dyingInstantly = true;
            col.collider.GetComponent<Animator>().enabled = false;
            soundObj.Stop();
            soundObj.Play();
            toush = true;
            myCol.enabled = false;
            mms = rc.material.shader;
            rc.material.shader = mats[0].shader;
            Utilities.canPauseGame = false;
            Utilities.canUseInventory = false;
            trs = Time.timeScale;
            myCol.enabled = false;
            StartCoroutine(DieInstantly(col.collider.GetComponent<KHealth>()));
        }
    }

    private void Update()
    {
        sr.color = toush ? Color.white : new Color(Fakerand.Single(0.8f, 1.0f), Fakerand.Single(0.8f, 1.0f), Fakerand.Single(0.8f, 1.0f), 1f);
    }
}
