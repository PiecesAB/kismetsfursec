using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher1 : GenericProjectileShooter
{
    public float delay;
    private double ts;
    private double tm;
    public Renderer visibleCheck;
    public MeshRenderer center;

    private AudioSource aso;

    public void Start()
    {
        tm = 0.0;
        ts = delay;
        aso = GetComponent<AudioSource>();
    }

    public override void ExtraUpdate()
    {
        if (Mathf.Abs(visibleCheck.transform.position.x - Camera.main.transform.position.x) < 160f
            && Mathf.Abs(visibleCheck.transform.position.y - Camera.main.transform.position.y) < 108f)
        {
            tm += (1.0 / 60.0) * Time.timeScale;
        }

        while (tm >= ts)
        {
            aso.Stop();
            aso.Play();
            Fire();
            ts += delay;
        }

        if (center)
        {
            center.materials[1].SetFloat("_BWCutoff", ((float)((tm - ts + delay) / delay))*2f - 1f);
        }
    }
}
