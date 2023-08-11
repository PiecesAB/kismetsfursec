using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MashToSkipCutscene : MonoBehaviour
{
    public bool skippable;
    public Encontrolmentation e;
    public CutsceneTextHandler cutsceneTextHandler;
    public RectTransform mainBox;
    public RectTransform bar;
    public AudioClip nothingClip;
    private float barFullWidth = 192;
    [HideInInspector]
    public float mashFactor = 0f;
    private float fall = 0f;

    void Start()
    {
        mainBox.anchoredPosition = new Vector2(0, 24);
        if (!skippable)
        {
            Destroy(this);
            return;
        }
    }

    void Update()
    {
        if (!skippable)
        {
            Destroy(this);
            return;
        }

        ulong es = e.flags;
        int buttonsMashed = 0;
        while (es != 0UL)
        {
            buttonsMashed += (int)(es & 1UL);
            es >>= 1;
        }
        if (buttonsMashed > 0) { fall = 0f; }
        mashFactor += 0.04f * Mathf.Pow(buttonsMashed, 0.5f);
        mashFactor -= fall;
        fall += 0.0005f;
        mashFactor = Mathf.Clamp(mashFactor, 0, 1.1f);

        if (mashFactor >= 1.05f && LoadingScreenScript.all.Count == 0) // skipping while a transitional screen exists may cause bad bugs!
        {
            if (BGMController.main) { BGMController.main.InstantMusicChange(nothingClip, true); }
            cutsceneTextHandler.ExitCutscene();
        }
        bar.sizeDelta = new Vector2(Mathf.Clamp(mashFactor * barFullWidth, 0f, barFullWidth), bar.sizeDelta.y);
        if (mashFactor >= 0.01f)
        {
            mainBox.anchoredPosition = Vector2.Lerp(mainBox.anchoredPosition, Vector2.zero, 0.3f);
        }
        else
        {
            mainBox.anchoredPosition = Vector2.Lerp(mainBox.anchoredPosition, new Vector2(0, 24), 0.3f);
        }
    }
}
