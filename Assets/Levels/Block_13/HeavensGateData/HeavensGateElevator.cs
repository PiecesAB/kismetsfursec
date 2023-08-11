using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavensGateElevator : MonoBehaviour, ITextBoxDeactivate
{
    private primDecorationMoving[] pdms;
    private const int mySwitchId = 8;
    [SerializeField]
    private Encontrolmentation khalControl; // to detect first mvt on elevator spawn
    [SerializeField]
    private Encontrolmentation aktControl; // to make punch
    private float pdmSpeed = 0;
    private int phase = 0;
    public HeavensGateESpeedUp eSpeedUp;
    public AudioSource sound1;
    public AudioSource sound2;
    public GenericBlowMeUp blowUpLever;
    public SpriteRenderer shineFromTop;
    public AudioClip music;

    private bool startedOnElevator = false;

    void Start()
    {
        pdms = GetComponentsInChildren<primDecorationMoving>();
        startedOnElevator = Utilities.GetLevelInInfo() == "elevator";
    }

    private FollowThePlayer ftp;

    void Update()
    {
        float prog = Mathf.Clamp01(Mathf.InverseLerp(-732, 3200, Camera.main?.transform.position.y ?? -732));
        shineFromTop.transform.localPosition = new Vector3(0, 164 * (1f - Mathf.Pow(prog, 4)), -100);
        shineFromTop.material.SetFloat("_Speed", Mathf.Pow(prog, 2f) + 0.2f);

        if (startedOnElevator && phase == 0 && khalControl.AnyButtonDown(255UL))
        {
            OnTextBoxDeactivate();
        }

        bool on = (Utilities.loadedSaveData.switchMask & (1u << mySwitchId)) != 0u;
        for (int i = 0; i < pdms.Length; ++i)
        {
            pdms[i].v = on ? (Vector3.up * pdmSpeed * eSpeedUp.multiplier) : Vector3.zero;
        }
        if (pdms[0].v.magnitude < 1)
        {
            if (sound1.isPlaying) { sound1.Stop(); }
            if (sound2.isPlaying) { sound2.Stop(); }
        }
        else if (pdmSpeed < 180f)
        {
            sound1.pitch = eSpeedUp.multiplier * pdmSpeed / 60;
            if (!sound1.isPlaying) { sound1.Play(); }
            if (sound2.isPlaying) { sound2.Stop(); }
        }
        else
        {
            if (sound1.isPlaying) { sound1.Stop(); }
            if (!sound2.isPlaying) { sound2.Play(); }
            sound2.pitch = eSpeedUp.multiplier;
        }
        if (phase > 2 && pdmSpeed >= 180f && !KHealth.someoneDied)
        {
            if (!ftp) { ftp = Camera.main.GetComponent<FollowThePlayer>(); }
            if (ftp) { ftp.vibSpeed = 3f; }
        }
    }

    private IEnumerator Phaser(int p)
    {
        switch (p)
        {
            case 0:
                Utilities.SetLevelInInfo("elevator");
                if (BGMController.main)
                {
                    BGMController.main.InstantMusicChange(music, true);
                }
                yield return new WaitForSeconds(startedOnElevator ? 0.05f : 0.7f);
                pdmSpeed = 60f;
                aktControl.SimulateTap(64UL);
                break;
            case 1:
                BulletRegister.Clear(new BulletRegister.ClearFromAmbush());
                yield return new WaitForSeconds(0.1f);
                aktControl.SimulateTap(64UL);
                yield return new WaitForSeconds(0.4f);
                pdmSpeed = 90f;
                aktControl.SimulateTap(64UL);
                break;
            case 2:
                BulletRegister.Clear(new BulletRegister.ClearFromAmbush());
                yield return new WaitForSeconds(0.1f);
                aktControl.SimulateTap(64UL);
                pdmSpeed = 0f;
                for (int i = 0; i < 20; ++i)
                {
                    float w = Mathf.Clamp(0.4f - i * 0.05f, 0.05f, 0.5f);
                    yield return new WaitForSeconds(w);
                    aktControl.SimulateTap(64UL);
                    yield return new WaitForSeconds(w);
                    aktControl.SimulateTap(64UL);

                }
                yield return new WaitForSeconds(0.4f);
                pdmSpeed = 225f;
                aktControl.SimulateTap(64UL);
                yield return new WaitForSeconds(0.1f);
                blowUpLever.BlowMeUp();
                break;
            default:
                break;
        }
        yield return null;
    }

    public void OnTextBoxDeactivate()
    {
        StartCoroutine(Phaser(phase));
        ++phase;
    }
}
