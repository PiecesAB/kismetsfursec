using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGICycleEntry : MonoBehaviour
{
    public enum Speed
    {
        Slow, Moderate, Fast
    }

    private static Color[] speedToColor = new Color[3]
    {
        new Color(0f, 0.7f, 1f),
        new Color(0.6f, 0.9f, 0f),
        new Color(1f, 0.6f, 0f)
    };

    private static float[] rotSpeeds = new float[3]
    {
        2.5f, 4f, 6f
    };

    private static float[] mvtSpeeds = new float[3]
    {
        100f, 160f, 240f
    };

    public Speed speed;
    [SerializeField]
    private Transform bubbleTransform;
    [SerializeField]
    private Transform cycleTransform;
    [SerializeField]
    private Renderer cycleRenderer;
    [SerializeField]
    private Renderer bubbleRenderer;
    public GameObject moverSample;

    [HideInInspector]
    public Vector2 initDir;
    [HideInInspector]
    public GameObject myPlayer;

    private AudioSource aud;

    private bool activated = false;
    private double lastResetTime = -5.0;

    private bool sprinting = false;

    private static Vector2[] possibleDirs = new Vector2[4]
    {
        Vector2.right, Vector2.left, Vector2.up, Vector2.down
    };

    private void Start()
    {
        Color c = speedToColor[(int)speed];
        Color ca0 = new Color(c.r, c.g, c.b, 0f);
        bubbleRenderer.material.SetColor("_Color", ca0);
        cycleRenderer.materials[2].SetColor("_EmissionColor", c);
        cycleRenderer.GetComponent<Prim3DRotate>().speed = rotSpeeds[(int)speed];
        aud = GetComponent<AudioSource>();
    }

    public Color GetCycleColor()
    {
        return speedToColor[(int)speed];
    }

    public float GetSpeed()
    {
        return mvtSpeeds[(int)speed] * (sprinting ? 2 : 1);
    }

    public void ReturnToInactivated()
    {
        bubbleTransform.localScale = 0.625f * Vector3.one;
        bubbleRenderer.material.SetFloat("_Stripe", 1f);
        cycleTransform.localRotation = Quaternion.identity;
        cycleTransform.localScale = 0.75f * Vector3.one;
        activated = false;
        lastResetTime = DoubleTime.UnscaledTimeSinceLoad;
    }

    private void DeployBike()
    {
        GameObject mover = Instantiate(moverSample, transform.position, Quaternion.identity);
        mover.SetActive(true);
        CGICycleMover moverReal = mover.GetComponent<CGICycleMover>();
        moverReal.GetDataFromEntry(this);
    }

    private IEnumerator ActivateAnimation()
    {
        double startTime = DoubleTime.UnscaledTimeSinceLoad;
        Quaternion oldCycleRotation = cycleTransform.rotation;
        Quaternion targetCycleRotation = Quaternion.LookRotation(Vector3.back, Vector3.Cross(initDir, Vector3.back));
        while (true)
        {
            float prog = (float)((DoubleTime.UnscaledTimeSinceLoad - startTime) / 0.5);
            if (prog > 1f) { prog = 1f; }
            bubbleTransform.localScale = (0.625f + 3f * prog) * Vector3.one;
            bubbleRenderer.material.SetFloat("_Stripe", 1f - prog);
            cycleTransform.rotation = Quaternion.Lerp(oldCycleRotation, targetCycleRotation, prog);
            if (prog == 1f) { break; }
            yield return new WaitForEndOfFrame();
        }
        cycleTransform.localScale = Vector3.zero;
        cycleTransform.rotation = oldCycleRotation;
        DeployBike();
        yield return null;
    }

    private void Activate(GameObject plr)
    {
        activated = true;
        myPlayer = plr;
        BasicMove bm = myPlayer.GetComponent<BasicMove>();
        sprinting = bm.IsSprinting();
        MoreSoundsHolder.main.transform.Find("airSoundJumpAlwaysPlaying").GetComponent<AudioSource>().volume = 0f;
        LauncherEnemy.RemovePlayerControl(plr);
        Vector2 oldVel = transform.position - plr.transform.position;
        if (oldVel.magnitude < 1f) { initDir = Vector2.right; }
        else
        {
            float maxDot = Vector2.Dot(oldVel, possibleDirs[0]);
            Vector2 maxDir = possibleDirs[0];
            for (int i = 1; i < possibleDirs.Length; ++i)
            {
                float currDot = Vector2.Dot(oldVel, possibleDirs[i]);
                if (currDot > maxDot)
                {
                    maxDot = currDot;
                    maxDir = possibleDirs[i];
                }
            }
            initDir = maxDir;
        }

        plr.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        plr.transform.position = transform.position;
        plr.transform.localScale = Vector3.zero;
        StartCoroutine(ActivateAnimation());

        aud.Play();
    }

    private void Detect(Collider2D col)
    {
        if (col.gameObject.layer != 20 || activated || DoubleTime.UnscaledTimeSinceLoad - lastResetTime < 0.15) { return; }
        if (!col.gameObject.GetComponent<BasicMove>().enabled) { return; }
        Activate(col.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Detect(col);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        Detect(col);
    }
}
