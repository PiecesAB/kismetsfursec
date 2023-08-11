using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetAmbush : GenericBlowMeUp
{
    // This enemy is truly simple: Once it appears on screen, start flying in a direction or along a path
    // Relative to the camera!
    // No path = Fly in direction (mvtBasic)

    public Vector2 mvtBasic = Vector2.right * -60;
    public Transform holder;
    public Renderer mainRenderer;
    public BulletHellMakerFunctions shooter;

    private Vector3 lastPos;

    private bool activated = false;
    private int activatedFrames = 0;

    void Start()
    {
        activated = false;
    }

    public void Activate()
    {
        activated = true;
        transform.SetParent(FollowThePlayer.main.transform, true);
        lastPos = transform.localPosition;
        shooter.gameObject.SetActive(true);
        GetComponent<AudioSource>().Play();
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (mainRenderer.isVisible && !activated) { Activate(); }
        if (activated)
        {
            transform.localPosition += Time.timeScale * 0.0166666666f * (Vector3)mvtBasic;

            Vector3 dif = transform.localPosition - lastPos;
            mainRenderer.transform.localRotation = Quaternion.LookRotation(Vector3.Cross(dif, Vector3.forward), -dif);
            holder.transform.localEulerAngles = Vector3.right * (float)((DoubleTime.ScaledTimeSinceLoad * mvtBasic.magnitude * 2f) % 360.0);
            ++activatedFrames;
            if (!mainRenderer.isVisible && activatedFrames >= 60 / Time.timeScale)
            {
                BlowMeUp();
            }
            lastPos = transform.localPosition;
        }
    }
}
