using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class ElectricZone : ColliderZoneTemplate<ElectricZone>
{
    public enum Mode
    {
        Remove, Propagate
    }

    public Mode mode;

    public AudioClip changedSound;

    public static AudioSource nowPlaying = null;

    public void ExtraStart()
    {
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(bc.size.x - (1f/transform.localScale.x), bc.size.y - (1f / transform.localScale.y));
    }

    public override void ResetStuff()
    {
    }

    public override void ColliderAdd()
    {
    }

    public override void ColliderRemove(int index)
    {
    }

    private void PlayAudioClip(AudioClip c)
    {
        AudioSource aud = GetComponent<AudioSource>();
        if (nowPlaying) { nowPlaying.Stop(); }
        nowPlaying = aud;
        aud.Stop();
        aud.clip = c;
        aud.Play();
    }

    public override void ObjectIn(int index, GameObject obj, GameObject other)
    {
        SpecialGunTemplate cct = obj.GetComponent<SpecialGunTemplate>();
        KHealth kh = obj.GetComponent<KHealth>();
        //SuperRay sr = obj.GetComponent<SuperRay>();

        if (mode == Mode.Remove)
        {
            bool anythingChanged = false;
            if (cct) {
                if (cct.gunHealth > 0f) { anythingChanged = true; }
                cct.gunHealth = 0f;
            }
            if (cct is ClickToShieldAndInfJump && (cct as ClickToShieldAndInfJump).layers > 0)
            {
                anythingChanged = true;
                (cct as ClickToShieldAndInfJump).layers = 0;
                (cct as ClickToShieldAndInfJump).UpdateLayerGraphic();
            }
            if (LevelInfoContainer.main != null && Physics2D.gravity != LevelInfoContainer.main.levelStartGravity)
            {
                anythingChanged = true;
                Physics2D.gravity = LevelInfoContainer.main.levelStartGravity;
                Camera.main.GetComponent<ColorCorrectionCurves>().DiscoStart();
            }
            if (kh)
            {
                if (kh.nontoxic != 0) { anythingChanged = true; }
                kh.nontoxic = 0;
            }

            if (anythingChanged)
            {
                PlayAudioClip(changedSound);
            }
        }

        if (mode == Mode.Propagate)
        {
            if (cct) { cct.gunHealth = 100f; }
            //if (sr) { sr.cursorAccel = sr.cursorVelocity * 50f; }
        }
    }
}
