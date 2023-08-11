using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeaterGlobal : MonoBehaviour, IExaminableAction
{
    public bool heated = false;
    public PrimExaminableItem examinePart;
    public GameObject iceGroup;
    public GameObject waterGroup;
    public SpriteRenderer coalSprite;
    public Sprite coalSpriteOff;
    public Sprite coalSpriteOn;
    public MeshRenderer renderMeshForHeatEffect;
    public SpriteRenderer smogEffect;

    private Color smogColor;

    private bool duringEffect;

    private MeshRenderer thisMesh;

    public void Start()
    {
        duringEffect = false;

        thisMesh = GetComponent<MeshRenderer>();

        coalSprite.sprite = coalSpriteOff;
        smogEffect.gameObject.SetActive(false);
        smogColor = smogEffect.color;
        iceGroup.SetActive(true);
        waterGroup.SetActive(false);
        thisMesh.materials[3].SetColor("_Color", Color.clear);
        renderMeshForHeatEffect.material.SetFloat("_Heatstroke", 0);

        if (!Utilities.replayLevel)
        {
            int dat = -1;
            if (Utilities.GetPersistentData(gameObject, -1, out dat) && dat != 0 && !Utilities.replayLevel)
            {
                DoHeat();
            }
        }
    }

    public IEnumerator DoEffect()
    {
        duringEffect = true;

        smogEffect.transform.SetParent(Camera.main.transform);
        smogEffect.transform.localPosition = new Vector3(0, 0, -400);
        smogEffect.gameObject.SetActive(true);
        smogEffect.color = new Color(smogEffect.color.r, smogEffect.color.g, smogEffect.color.b, 0);
        smogEffect.material.SetVector("_WaveDistort", new Vector4(0.05f, 0f, 2f, 0f));
        coalSprite.sprite = coalSpriteOn;
        thisMesh.materials[3].SetColor("_Color", Color.white);
        yield return new WaitForEndOfFrame();

        while (smogEffect.color.a < 1f)
        {
            smogEffect.color = new Color(smogEffect.color.r, smogEffect.color.g, smogEffect.color.b, smogEffect.color.a + 0.1f);
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => (Time.timeScale > 0));
        }

        DoHeat();

        while (smogEffect.color.a > smogColor.a)
        {
            smogEffect.color = new Color(smogEffect.color.r, smogEffect.color.g, smogEffect.color.b, smogEffect.color.a - 0.1f);
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => (Time.timeScale > 0));
        }

        smogEffect.color = smogColor;

        duringEffect = false;
        yield return null;
    }

    public void DoHeat()
    {
        if (heated) { return; }
        heated = true;

        coalSprite.sprite = coalSpriteOn;
        smogEffect.transform.SetParent(Camera.main.transform);
        smogEffect.transform.localPosition = new Vector3(0, 0, -400);
        smogEffect.gameObject.SetActive(true);
        smogEffect.material.SetVector("_WaveDistort", new Vector4(0.03f, 0f, 2f, 0f));
        iceGroup.SetActive(false);
        waterGroup.SetActive(true);
        thisMesh.materials[3].SetColor("_Color", Color.white);
        renderMeshForHeatEffect.material.SetFloat("_Heatstroke", 1);

        if (!Utilities.replayLevel)
        {
            Utilities.ChangePersistentData(gameObject, 1);
        }

        Destroy(examinePart);
    }

    public void OnExamine(Encontrolmentation plr)
    {
        StartCoroutine(DoEffect());
    }

    public void Update()
    {
        if (thisMesh.enabled && thisMesh.isVisible)
        {
            thisMesh.materials[3].SetVector("_TexSize", new Vector4(1,1,0,-(float)(DoubleTime.ScaledTimeSinceLoad%1.0)));
        }

        if (!duringEffect && smogEffect.gameObject.activeSelf)
        {
            smogEffect.material.SetVector("_TexSize", new Vector4(1, 1, -(float)((DoubleTime.ScaledTimeSinceLoad * 0.2) % 1.0)));

            //smog builds up over time, beat the level before you are blind (30s)
            smogColor = new Color(smogColor.r, smogColor.g, smogColor.b, Mathf.Min(0.95f, smogColor.a + 0.5f * 0.0333333333f * 0.016666666f * Time.timeScale));
            smogEffect.color = new Color(smogColor.r, smogColor.g, smogColor.b, smogColor.a* smogColor.a);

            if (smogColor.a >= 0.9f && !KHealth.someoneDied && !Door1.levelComplete)
            {
                LevelInfoContainer.GetActiveControl().GetComponent<KHealth>().ChangeHealth(-0.1f, "smog");
            }
        }
    }
}
