using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CensorGore : MonoBehaviour
{
    public enum Mode
    {
        AlternateParticle, DeleteRenderer, ChangeSpriteColor, ReplaceRendererMaterial, ReplaceRendererSprite
    }

    public Mode mode;
    public GameObject subParticle;
    public GameObject alternateParticle; // or alternate renderer
    public Material newMaterial;
    public Sprite newSprite;
    public Color newSpriteColor;

    void Start()
    {
        if (!Settings.ShowGore())
        {
            switch (mode)
            {
                case Mode.AlternateParticle:
                    if (alternateParticle) { alternateParticle.SetActive(true); }
                    if (subParticle) { Destroy(subParticle.GetComponent<ParticleSystem>()); }
                    Destroy(GetComponent<ParticleSystem>());
                    break;
                case Mode.DeleteRenderer:
                    if (alternateParticle) { alternateParticle.SetActive(true); }
                    Destroy(GetComponent<Renderer>());
                    break;
                case Mode.ChangeSpriteColor:
                    GetComponent<SpriteRenderer>().color = newSpriteColor;
                    break;
                case Mode.ReplaceRendererMaterial:
                    GetComponent<Renderer>().material = newMaterial;
                    break;
                case Mode.ReplaceRendererSprite:
                    GetComponent<SpriteRenderer>().sprite = newSprite;
                    break;
            }
        }
    }
}
