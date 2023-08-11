using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbationDrill : GenericBlowMeUp
{
    public bool on = false;
    public bool timerControlled = false;
    public bool instantKill = false;
    public float progress = 0f;
    public float secondsToKill = 5f;
    public SkinnedMeshRenderer screw;

    private const float killPoint = 75f;
    private Prim3DRotate rot;
    private Prim3DRotate srot;
    private KHealth kh;
    private BasicMove bm;
    private Encontrolmentation control;
    private AudioSource aud;
    private int damageCooldown = 0;

    void Start()
    {
        rot = GetComponent<Prim3DRotate>();
        srot = screw.GetComponent<Prim3DRotate>();
        kh = transform.parent.GetComponent<KHealth>();
        bm = transform.parent.GetComponent<BasicMove>();
        control = transform.parent.GetComponent<Encontrolmentation>();
        aud = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (Door1.levelComplete || KHealth.someoneDied)
        {
            BlowMeUp();
            return;
        }
        if (timerControlled)
        {
            on = LevelInfoContainer.timerOn;
            instantKill = LevelInfoContainer.timerOn && LevelInfoContainer.timer == 0f;
        }
        if (instantKill) { on = true; }
        bool activePlayer = control == LevelInfoContainer.GetActiveControl();
        rot.enabled = srot.enabled = on && activePlayer;
        if (on && activePlayer && !aud.isPlaying) { aud.Play(); }
        if (!(on && activePlayer) && aud.isPlaying) { aud.Stop(); }
        if (on && activePlayer) { progress += 1f / (60f * secondsToKill); }
        else { progress -= 1f / (60f * secondsToKill); }
        if (instantKill) { progress = 1f; }
        progress = Mathf.Clamp01(progress);

        float sp = screw.GetBlendShapeWeight(0);
        float targ = (progress == 1f) ? 100f : (progress * killPoint);
        float nsp = Mathf.MoveTowards(sp, targ, 5f);
        screw.SetBlendShapeWeight(0, nsp);
        if (nsp >= killPoint && damageCooldown == 0)
        {
            kh.ChangeHealth(-6f, "probation");
            bm?.AddBlood(transform.position + transform.up * 8, Quaternion.LookRotation(Vector3.back, transform.up));
            damageCooldown = 6;
        }
        if (damageCooldown > 0) { --damageCooldown; }

        aud.pitch = (nsp >= 99f) ? 0.65f : 1f;
        aud.volume = Mathf.Pow(nsp * 0.01f, 1.5f) * 0.6f + 0.2f;
        if (nsp > 50f && on && activePlayer && FollowThePlayer.main)
        {
            FollowThePlayer.main.vibSpeed += nsp * 0.002f;
        }
    }
}
