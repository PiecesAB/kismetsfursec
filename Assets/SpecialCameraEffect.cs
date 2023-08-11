using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SpecialCameraEffect : MonoBehaviour, IAmbushController
{
    public enum Effect
    {
        DarkMonochrome
    }

    public enum TriggerBehavior
    {
        Flash
    }

    public MeshRenderer renderCube;
    public Effect effect;
    public TriggerBehavior triggerBehavior;

    public GameObject hiddenLayer;
    private List<Renderer> hiddenLayerCache;

    private Camera myCam;

    void CullSearch(Transform t)
    {
        if (t == null) { return; }
        bool prevActive = t.gameObject.activeSelf;
        t.gameObject.SetActive(true);
        if (t.GetComponent<Renderer>()) { hiddenLayerCache.Add(t.GetComponent<Renderer>()); }
        foreach (Transform tc in t) { if (tc != t) { CullSearch(tc); } }
        t.gameObject.SetActive(prevActive);
    }

    private void OnPreCull()
    {
        if (hiddenLayer == null) { return; }
        if (hiddenLayerCache == null)
        {
            hiddenLayerCache = new List<Renderer>();
            CullSearch(hiddenLayer.transform);
        }

        for (int i = 0; i < hiddenLayerCache.Count; ++i)
        {
            if (hiddenLayerCache[i])
            {
                hiddenLayerCache[i].enabled = false;
            }
        }
    }

    private void OnPostRender()
    {
        if (hiddenLayer == null || hiddenLayerCache == null) { return; }

        for (int i = 0; i < hiddenLayerCache.Count; ++i)
        {
            if (hiddenLayerCache[i])
            {
                hiddenLayerCache[i].enabled = true;
            }
        }
    }

    public void OnAmbushBegin()
    {
        switch (triggerBehavior)
        {
            case TriggerBehavior.Flash:
                StartCoroutine(Flash(1f)); break;
            default:
                break;
        }
    }

    public void OnAmbushComplete()
    {
        // ...
    }

    public IEnumerator Flash(float flashTime)
    {
       bool camEnabledAtFirst = myCam.enabled;
       float myTimer = 0f;
       while (myTimer < flashTime)
       {
            float myNoise = Mathf.PerlinNoise(myTimer * 14f, (float)((DoubleTime.ScaledTimeRunning)%100000.0)) - 0.5f;
            myCam.enabled = (myNoise > 0f);
            yield return new WaitForEndOfFrame();
            myTimer += 0.016666666f * Time.timeScale;
       }
       myCam.enabled = !camEnabledAtFirst;
       yield return null;
    }

    private void OnDestroy()
    {
        Halt();
    }

    private void OnDisable()
    {
        Halt();
    }

    private void Halt()
    {
        switch (effect)
        {
            case Effect.DarkMonochrome:
                renderCube.material.SetColor("_SolidColor", new Color32(0, 0, 0, 0));
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        myCam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!myCam.enabled) { Halt(); return; }

        switch(effect)
        {
            case Effect.DarkMonochrome:
                renderCube.material.SetColor("_SolidColor", new Color32(48, 56, 56, 255));
                break;
            default:
                break;
        }
    }
}
