using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class ClambakeMicrowave : MonoBehaviour, ITripwire
{
    private bool on = false;
    private PlatformControlButton[] pcbs;
    private float crumbHeat = 0.5f;
    private ColorCorrectionCurves cc = null;

    public Transform coilHolder;
    private List<Transform> coils = new List<Transform>();

    private void Start()
    {
        pcbs = FindObjectsOfType<PlatformControlButton>();
        foreach (Transform c in coilHolder)
        {
            if (c == coilHolder) { continue; }
            coils.Add(c);
        }
    }

    public void OnTrip()
    {
        foreach (PlatformControlButton pcb in pcbs) { StartCoroutine(pcb.OnWithDelay()); }
        on = true;
    }

    private void Update()
    {
        if (Time.timeScale == 0 || !on) { return; }
        if (LevelInfoContainer.timerOn)
        {
            Encontrolmentation e = LevelInfoContainer.GetActiveControl();
            KHealth kh = e?.GetComponent<KHealth>();
            if (LevelInfoContainer.timer == 0)
            {
                if (kh)
                {
                    kh.overheat = crumbHeat;
                    crumbHeat = Mathf.Clamp01(crumbHeat + 0.003f);
                }
            }
            else if (LevelInfoContainer.timer <= 15)
            {
                if (kh)
                {
                    kh.overheat = 0.49f * (15 - LevelInfoContainer.timer) / 15f;
                }
            }

            if (!cc) { cc = FollowThePlayer.main?.GetComponent<ColorCorrectionCurves>(); }
            if (cc && LevelInfoContainer.timer <= 40)
            {
                float prog = (40f - LevelInfoContainer.timer) / 40f;
                cc.redChannel = AnimationCurve.Linear(0, 0, 1 - 0.6f * prog, 1);
                cc.greenChannel = AnimationCurve.Linear(0, 0, 1 - 0.4f * prog, 1f - 0.3f * prog);
                cc.blueChannel = AnimationCurve.Linear(0, 0, 1, 1f - 1f * prog * prog);
                cc.UpdateParameters();
            }

            foreach (Transform c in coils)
            {
                float prog = (60f - LevelInfoContainer.timer) / 60f;
                float sprog = Mathf.Min(1f, 0.5f * (60f - LevelInfoContainer.timer));
                c.GetComponent<Prim3DRotate>().speed = Mathf.Min(1f + prog * prog * 4.5f, 0.5f * (60f - LevelInfoContainer.timer));
                c.GetComponent<SpriteRenderer>().material.SetFloat("_Heat", 0.1f * sprog + 0.15f * prog * prog);
            }
        }
    }
}
